using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace VTR.Net
{
    public enum NetMessageType : int
    {
        ChroniclePayload = 1,
        CharacterPayload = 2,
        CharacterRemoved = 3,
    }

    public class NetworkGameManager : NetworkBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }

        public NetworkVariable<int> PlayerCount = new NetworkVariable<int>(0);

        public readonly Dictionary<ulong, string> PlayerCharacters = new Dictionary<ulong, string>();

        public event Action<ulong, string> OnCharacterReceived;
        public event Action<ulong> OnCharacterRemoved;
        public event Action<string> OnChronicleReceived;

        private string _serverChronicleJson = "";
        private string _clientChronicleJson = "";

        public string ChronicleJson => IsServer ? _serverChronicleJson : _clientChronicleJson;
        public bool HasChronicle => !string.IsNullOrEmpty(ChronicleJson);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                PlayerCount.Value = 1;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedServer;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedServer;
            }

            NetworkManager.Singleton.CustomMessagingManager.OnUnnamedMessage += OnUnnamedMessageReceived;
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton == null) return;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedServer;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedServer;
            }

            if (NetworkManager.Singleton.CustomMessagingManager != null)
                NetworkManager.Singleton.CustomMessagingManager.OnUnnamedMessage -= OnUnnamedMessageReceived;
        }

        // ============================================================
        // ХРОНИКА
        // ============================================================

        public void SetChronicleServer(string chronicleJson)
        {
            if (!IsServer) return;
            _serverChronicleJson = chronicleJson ?? "";
            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                if (kv.Key == NetworkManager.ServerClientId) continue;
                SendChronicleToClient(kv.Key, _serverChronicleJson);
            }
        }

        private void SendChronicleToClient(ulong clientId, string json)
        {
            byte[] data = Encoding.UTF8.GetBytes(json ?? "");
            using var writer = new FastBufferWriter(data.Length + 16, Allocator.Temp);
            writer.TryBeginWrite(data.Length + 8);
            writer.WriteValueSafe((int)NetMessageType.ChroniclePayload);
            writer.WriteValueSafe(data.Length);
            if (data.Length > 0) writer.WriteBytesSafe(data);

            NetworkManager.Singleton.CustomMessagingManager.SendUnnamedMessage(
                clientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestChronicleServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            if (!string.IsNullOrEmpty(_serverChronicleJson))
                SendChronicleToClient(clientId, _serverChronicleJson);
        }

        // ============================================================
        // ПЕРСОНАЖИ — ПУБЛИЧНЫЙ API
        // ============================================================

        /// <summary>
        /// Игрок отсылает СВОЕГО персонажа мастеру.
        /// </summary>
        public void SendMyCharacter(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[Net] Пустой JSON персонажа — не отправляю.");
                return;
            }

            ulong myId = NetworkManager.Singleton.LocalClientId;

            if (IsServer)
            {
                PlayerCharacters[myId] = json;
                OnCharacterReceived?.Invoke(myId, json);
                BroadcastCharacterToOthers(myId, json);
                Debug.Log($"[Net] Хост обновил своего персонажа ({json.Length} байт).");
            }
            else
            {
                SubmitCharacterServerRpc(json);
                Debug.Log($"[Net] → мастеру: персонаж {json.Length} байт.");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitCharacterServerRpc(string json, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (string.IsNullOrEmpty(json))
            {
                PlayerCharacters.Remove(senderId);
                OnCharacterRemoved?.Invoke(senderId);
                BroadcastCharacterRemoved(senderId);
                return;
            }

            PlayerCharacters[senderId] = json;
            OnCharacterReceived?.Invoke(senderId, json);
            BroadcastCharacterToOthers(senderId, json);

            Debug.Log($"[Net] Получен персонаж от {senderId} ({json.Length} байт).");
        }

        /// <summary>
        /// Мастер меняет чужого персонажа через чит-режим.
        /// Рассылает ВСЕМ, включая владельца.
        /// </summary>
        public void MasterSetCharacter(ulong ownerId, string json)
        {
            if (!IsServer) return;

            if (string.IsNullOrEmpty(json))
            {
                PlayerCharacters.Remove(ownerId);
                OnCharacterRemoved?.Invoke(ownerId);
                BroadcastCharacterRemoved(ownerId);
                return;
            }

            PlayerCharacters[ownerId] = json;
            OnCharacterReceived?.Invoke(ownerId, json);
            BroadcastCharacterToEveryone(ownerId, json);

            Debug.Log($"[Net] Мастер обновил персонажа игрока {ownerId}.");
        }

        // ============================================================
        // КЛИЕНТСКИЕ КОЛБЭКИ
        // ============================================================

        private void OnClientConnectedServer(ulong clientId)
        {
            PlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
            Debug.Log($"[Net] Клиент {clientId} подключился. Всего: {PlayerCount.Value}");

            if (!string.IsNullOrEmpty(_serverChronicleJson))
                SendChronicleToClient(clientId, _serverChronicleJson);

            foreach (var kv in PlayerCharacters)
            {
                if (kv.Key == clientId) continue;
                SendCharacterToClient(clientId, kv.Key, kv.Value);
            }
        }

        private void OnClientDisconnectedServer(ulong clientId)
        {
            PlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
            Debug.Log($"[Net] Клиент {clientId} отключился. Всего: {PlayerCount.Value}");

            if (PlayerCharacters.Remove(clientId))
            {
                OnCharacterRemoved?.Invoke(clientId);
                BroadcastCharacterRemoved(clientId);
            }
        }

        // ============================================================
        // РАССЫЛКА ПЕРСОНАЖЕЙ
        // ============================================================

        /// <summary>Всем, КРОМЕ автора (для Submit от игрока).</summary>
        private void BroadcastCharacterToOthers(ulong ownerId, string json)
        {
            if (!IsServer) return;

            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                ulong target = kv.Key;
                if (target == NetworkManager.ServerClientId) continue;
                if (target == ownerId) continue;
                SendCharacterToClient(target, ownerId, json);
            }
        }

        /// <summary>Всем, ВКЛЮЧАЯ владельца (для MasterSetCharacter).</summary>
        private void BroadcastCharacterToEveryone(ulong ownerId, string json)
        {
            if (!IsServer) return;

            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                ulong target = kv.Key;
                if (target == NetworkManager.ServerClientId) continue;
                SendCharacterToClient(target, ownerId, json);
            }
        }

        private void BroadcastCharacterRemoved(ulong ownerId)
        {
            if (!IsServer) return;

            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                ulong target = kv.Key;
                if (target == NetworkManager.ServerClientId) continue;
                SendCharacterRemovedToClient(target, ownerId);
            }
        }

        private void SendCharacterToClient(ulong targetClientId, ulong ownerId, string json)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json ?? "");

            using var writer = new FastBufferWriter(jsonBytes.Length + 32, Allocator.Temp);
            writer.TryBeginWrite(jsonBytes.Length + 16);
            writer.WriteValueSafe((int)NetMessageType.CharacterPayload);
            writer.WriteValueSafe(ownerId);
            writer.WriteValueSafe(jsonBytes.Length);
            if (jsonBytes.Length > 0)
                writer.WriteBytesSafe(jsonBytes);

            NetworkManager.Singleton.CustomMessagingManager.SendUnnamedMessage(
                targetClientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
        }

        private void SendCharacterRemovedToClient(ulong targetClientId, ulong ownerId)
        {
            using var writer = new FastBufferWriter(16, Allocator.Temp);
            writer.TryBeginWrite(12);
            writer.WriteValueSafe((int)NetMessageType.CharacterRemoved);
            writer.WriteValueSafe(ownerId);

            NetworkManager.Singleton.CustomMessagingManager.SendUnnamedMessage(
                targetClientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
        }

        // ============================================================
        // ПРИЁМ
        // ============================================================

        private void OnUnnamedMessageReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(sizeof(int))) return;
            reader.ReadValueSafe(out int msgType);

            switch ((NetMessageType)msgType)
            {
                case NetMessageType.ChroniclePayload:
                    HandleChroniclePayload(reader);
                    break;
                case NetMessageType.CharacterPayload:
                    HandleCharacterPayload(reader);
                    break;
                case NetMessageType.CharacterRemoved:
                    HandleCharacterRemoved(reader);
                    break;
                default:
                    Debug.LogWarning($"[Net] Неизвестный тип сообщения: {msgType}");
                    break;
            }
        }

        private void HandleChroniclePayload(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int len);
            if (len < 0 || len > 50_000_000)
            {
                Debug.LogError($"[Net] Некорректная длина хроники: {len}");
                return;
            }

            byte[] bytes = new byte[len];
            if (len > 0) reader.ReadBytesSafe(ref bytes, len);

            _clientChronicleJson = Encoding.UTF8.GetString(bytes);
            Debug.Log($"[Net] Получена хроника: {_clientChronicleJson.Length} символов.");
            OnChronicleReceived?.Invoke(_clientChronicleJson);
        }

        private void HandleCharacterPayload(FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong ownerId);
            reader.ReadValueSafe(out int len);
            if (len < 0 || len > 10_000_000)
            {
                Debug.LogError($"[Net] Некорректная длина персонажа: {len}");
                return;
            }

            byte[] bytes = new byte[len];
            if (len > 0) reader.ReadBytesSafe(ref bytes, len);

            string json = Encoding.UTF8.GetString(bytes);
            PlayerCharacters[ownerId] = json;

            ulong myId = NetworkManager.Singleton != null
                ? NetworkManager.Singleton.LocalClientId : 0;

            Debug.Log($"[Net] Получен персонаж игрока {ownerId} ({len} байт). LocalClientId={myId}.");
            OnCharacterReceived?.Invoke(ownerId, json);
        }

        private void HandleCharacterRemoved(FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong ownerId);

            if (PlayerCharacters.Remove(ownerId))
                Debug.Log($"[Net] Персонаж игрока {ownerId} удалён.");

            OnCharacterRemoved?.Invoke(ownerId);
        }
    }
}