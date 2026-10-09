using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace VTR.Net
{
    public static class LobbyService
    {
        public const string PinKey = "PIN";
        public const string RelayJoinCodeKey = "RELAY_JOIN_CODE";
        public const string ChronicleIdKey = "CHRONICLE_ID";
        public const string MasterNameKey = "MASTER_NAME";

        public static Unity.Services.Lobbies.Models.Lobby CurrentLobby { get; private set; }

        /// <summary>
        /// Безопасная проверка «мы хост». Не падает, если сервисы не инициализированы
        /// или игрок уже вышел.
        /// </summary>
        public static bool IsHost
        {
            get
            {
                try
                {
                    if (CurrentLobby == null) return false;
                    if (!ServicesInitializer.IsInitialized) return false;

                    var auth = AuthenticationService.Instance;
                    if (auth == null || !auth.IsSignedIn) return false;

                    return CurrentLobby.HostId == auth.PlayerId;
                }
                catch
                {
                    return false;
                }
            }
        }

        // ============================================================
        // СОЗДАНИЕ
        // ============================================================

        public static async Task<Unity.Services.Lobbies.Models.Lobby> CreateLobbyAsync(
            string lobbyName,
            int maxPlayers,
            string pin,
            string relayJoinCode,
            string chronicleId,
            string masterName)
        {
            try
            {
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, masterName) }
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { PinKey, new DataObject(
                            DataObject.VisibilityOptions.Public,
                            pin,
                            DataObject.IndexOptions.S1) },
                        { RelayJoinCodeKey, new DataObject(
                            DataObject.VisibilityOptions.Public, relayJoinCode) },
                        { ChronicleIdKey, new DataObject(
                            DataObject.VisibilityOptions.Public, chronicleId) },
                        { MasterNameKey, new DataObject(
                            DataObject.VisibilityOptions.Public, masterName) }
                    }
                };

                var lobby = await Unity.Services.Lobbies.LobbyService.Instance.CreateLobbyAsync(
                    lobbyName, maxPlayers, options);

                CurrentLobby = lobby;
                Debug.Log($"[Lobby] Создано лобби: {lobby.Name}, Id={lobby.Id}, PIN={pin}");
                return lobby;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Ошибка создания: {e}");
                throw;
            }
        }

        // ============================================================
        // ПОИСК
        // ============================================================

        public static async Task<Unity.Services.Lobbies.Models.Lobby> FindLobbyByPinAsync(string pin)
        {
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                var found = await QueryByS1Async(pin);
                if (found != null)
                {
                    Debug.Log($"[Lobby] Найдено лобби (попытка {attempt}): {found.Name}");
                    return found;
                }
                await Task.Delay(500);
            }

            var fallback = await FallbackScanAsync(pin);
            if (fallback != null)
            {
                Debug.Log($"[Lobby] Fallback сработал: {fallback.Name}");
                return fallback;
            }

            Debug.LogWarning($"[Lobby] Лобби с PIN={pin} не найдено.");
            return null;
        }

        private static async Task<Unity.Services.Lobbies.Models.Lobby> QueryByS1Async(string pin)
        {
            try
            {
                var queryOptions = new QueryLobbiesOptions
                {
                    Count = 20,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.S1, pin, QueryFilter.OpOptions.EQ)
                    }
                };

                var query = await Unity.Services.Lobbies.LobbyService.Instance
                    .QueryLobbiesAsync(queryOptions);

                if (query.Results != null && query.Results.Count > 0)
                    return query.Results[0];
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Lobby] Ошибка QueryByS1: {e.Message}");
            }
            return null;
        }

        private static async Task<Unity.Services.Lobbies.Models.Lobby> FallbackScanAsync(string pin)
        {
            try
            {
                var queryOptions = new QueryLobbiesOptions { Count = 100 };
                var query = await Unity.Services.Lobbies.LobbyService.Instance
                    .QueryLobbiesAsync(queryOptions);

                if (query.Results == null) return null;

                foreach (var l in query.Results)
                {
                    if (l.Data != null &&
                        l.Data.TryGetValue(PinKey, out var pinData) &&
                        pinData.Value == pin)
                        return l;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Lobby] Ошибка FallbackScan: {e.Message}");
            }
            return null;
        }

        // ============================================================
        // ПРИСОЕДИНЕНИЕ
        // ============================================================

        public static async Task JoinLobbyAsync(string lobbyId, string playerName)
        {
            try
            {
                var options = new JoinLobbyByIdOptions
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                        }
                    }
                };
                CurrentLobby = await Unity.Services.Lobbies.LobbyService.Instance
                    .JoinLobbyByIdAsync(lobbyId, options);
                Debug.Log($"[Lobby] Подключились к лобби: {CurrentLobby.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] Ошибка подключения: {e}");
                throw;
            }
        }

        // ============================================================
        // HEARTBEAT / LEAVE
        // ============================================================

        public static async Task HeartbeatAsync()
        {
            if (CurrentLobby == null) return;
            if (!ServicesInitializer.IsInitialized) return;

            try
            {
                await Unity.Services.Lobbies.LobbyService.Instance
                    .SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Lobby] Heartbeat не отправлен: {e.Message}");
            }
        }

        public static async Task LeaveLobbyAsync()
        {
            if (CurrentLobby == null) return;
            if (!ServicesInitializer.IsInitialized) { CurrentLobby = null; return; }

            try
            {
                if (IsHost)
                    await Unity.Services.Lobbies.LobbyService.Instance
                        .DeleteLobbyAsync(CurrentLobby.Id);
                else
                    await Unity.Services.Lobbies.LobbyService.Instance
                        .RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log("[Lobby] Вышли из лобби.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Lobby] Ошибка выхода: {e}");
            }
            finally
            {
                CurrentLobby = null;
            }
        }
    }
}