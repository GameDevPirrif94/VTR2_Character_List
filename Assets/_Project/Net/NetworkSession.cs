using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VTR.Core.Models;
using VTR.Data;
using VTR.UI;

namespace VTR.Net
{
    /// <summary>
    /// Обёртка над всей цепочкой: Unity Services → Relay → Lobby → NGO Host/Client.
    /// </summary>
    public static class NetworkSession
    {
        public static bool IsHost { get; private set; }
        public static bool IsPlayer { get; private set; }
        public static string Pin { get; private set; }
        public static string RelayJoinCode { get; private set; }
        public static int MaxPlayers { get; private set; }

        public class Result
        {
            public bool Ok;
            public string Error;
        }

        // ============================================================
        // ХОСТ
        // ============================================================

        /// <summary>
        /// Запускает хост: инициализация → Relay → Lobby → NGO → публикация хроники.
        /// </summary>
        public static async Task<Result> StartHostAsync(Chronicle chronicle, int maxPlayers)
        {
            if (chronicle == null)
                return new Result { Ok = false, Error = "Хроника не выбрана." };

            try
            {
                // 1. Services + анонимный вход.
                bool inited = await ServicesInitializer.InitializeAsync();
                if (!inited)
                    return new Result { Ok = false, Error = "Не удалось войти в Unity Services." };

                // 2. Relay-аллокация.
                var (joinCode, serverData) = await RelayService.CreateRelayAsync(maxPlayers);
                RelayJoinCode = joinCode;

                // 3. PIN.
                Pin = PinGenerator.Generate();

                // 4. Лобби.
                string masterName = string.IsNullOrEmpty(SessionContext.PlayerName)
                    ? "Мастер"
                    : SessionContext.PlayerName;

                var lobby = await LobbyService.CreateLobbyAsync(
                    lobbyName: $"VTR: {chronicle.Name}",
                    maxPlayers: maxPlayers,
                    pin: Pin,
                    relayJoinCode: joinCode,
                    chronicleId: chronicle.Id,
                    masterName: masterName);

                // 5. NGO Host.
                bool hostStarted = await NetworkBootstrap.StartHostWithRelayAsync(serverData);
                if (!hostStarted)
                    return new Result { Ok = false, Error = "NetworkManager не смог запустить хост." };

                // 6. Публикация хроники.
                var mgr = NetworkGameManager.Instance;
                if (mgr == null)
                    return new Result { Ok = false, Error = "NetworkGameManager не найден в сцене." };

                string chronicleJson = JsonSaveSystem.Serialize(chronicle);
                mgr.SetChronicleServer(chronicleJson);

                // 7. Heartbeat.
                HeartbeatHelper.EnsureInstance();

                IsHost = true;
                MaxPlayers = maxPlayers;
                Debug.Log($"[Session] Хост запущен. PIN={Pin}, JoinCode={joinCode}");
                return new Result { Ok = true };
            }
            catch (Exception e)
            {
                Debug.LogError($"[Session] Ошибка старта хоста: {e}");
                return new Result { Ok = false, Error = e.Message };
            }
        }

        /// <summary>
        /// Корректно останавливает хоста и удаляет лобби.
        /// </summary>
        public static async void Shutdown()
        {
            try
            {
                if (LobbyService.CurrentLobby != null)
                    await LobbyService.LeaveLobbyAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Session] Ошибка выхода из лобби: {e.Message}");
            }

            NetworkBootstrap.Shutdown();
            HeartbeatHelper.DestroyInstance();

            IsHost = false;
            Pin = null;
            RelayJoinCode = null;
            MaxPlayers = 0;
            Debug.Log("[Session] Хост остановлен.");
        }

        // ============================================================
        // ИГРОК
        // ============================================================

        /// <summary>
        /// Подключение игрока: Services → найти лобби по PIN → Join Relay → StartClient
        /// → дождаться хроники → сохранить локально.
        /// </summary>
        public static async Task<Result> JoinAsPlayerAsync(string pin, string playerName)
        {
            try
            {
                // 1. Services.
                bool inited = await ServicesInitializer.InitializeAsync();
                if (!inited)
                    return new Result { Ok = false, Error = "Не удалось войти в Unity Services." };

                // 2. Найти лобби по PIN.
                var lobby = await LobbyService.FindLobbyByPinAsync(pin);
                if (lobby == null)
                    return new Result { Ok = false, Error = "Лобби с таким PIN не найдено." };

                // 3. Проверка на заполненность.
                if (lobby.Players != null && lobby.MaxPlayers > 0 &&
                    lobby.Players.Count >= lobby.MaxPlayers)
                    return new Result { Ok = false, Error = "Лобби заполнено." };

                // 4. Войти в лобби.
                await LobbyService.JoinLobbyAsync(lobby.Id, playerName);

                // 5. Достать join-код relay.
                if (lobby.Data == null ||
                    !lobby.Data.TryGetValue(LobbyService.RelayJoinCodeKey, out var relayData) ||
                    string.IsNullOrEmpty(relayData.Value))
                    return new Result { Ok = false, Error = "У лобби отсутствует relay-код." };

                string joinCode = relayData.Value;
                var serverData = await RelayService.JoinRelayAsync(joinCode);

                // 6. Запустить клиент NGO.
                bool started = await NetworkBootstrap.StartClientWithRelayAsync(serverData);
                if (!started)
                    return new Result { Ok = false, Error = "Не удалось запустить сетевой клиент." };

                // 7. Дождаться подключения к серверу.
                bool connected = await WaitForClientConnected(20000);
                if (!connected)
                    return new Result { Ok = false, Error = "Не удалось подключиться к серверу (таймаут)." };

                // 8. Дождаться получения хроники.
                string chronicleJson = await WaitForChronicle(20000);
                if (string.IsNullOrEmpty(chronicleJson))
                    return new Result { Ok = false, Error = "Хроника не получена от мастера." };

                // 9. Распарсить и сохранить локально.
                var chronicle = JsonSaveSystem.Deserialize<Chronicle>(chronicleJson);
                JsonSaveSystem.SaveChronicle(chronicle);

                SessionContext.CurrentChronicle = chronicle;
                SessionContext.IsGameMaster = false;
                SessionContext.PlayerName = playerName;
                IsPlayer = true;

                Debug.Log($"[Session] Игрок подключён. Хроника сохранена локально: {chronicle.Name} ({chronicle.Id})");
                return new Result { Ok = true };
            }
            catch (Exception e)
            {
                Debug.LogError($"[Session] Ошибка подключения игрока: {e}");
                return new Result { Ok = false, Error = e.Message };
            }
        }

        private static async Task<bool> WaitForClientConnected(int timeoutMs)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return false;

            int waited = 0;
            while (waited < timeoutMs)
            {
                if (nm.IsConnectedClient) return true;
                await Task.Delay(100);
                waited += 100;
            }
            return false;
        }

        private static async Task<string> WaitForChronicle(int timeoutMs)
        {
            int waited = 0;
            while (waited < timeoutMs)
            {
                if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.HasChronicle)
                    return NetworkGameManager.Instance.ChronicleJson;
                await Task.Delay(100);
                waited += 100;
            }
            return null;
        }

        /// <summary>
        /// Выход игрока: LeaveLobby + Shutdown.
        /// </summary>
        public static async void ShutdownPlayer()
        {
            try
            {
                if (LobbyService.CurrentLobby != null)
                    await LobbyService.LeaveLobbyAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Session] Ошибка выхода из лобби: {e.Message}");
            }

            NetworkBootstrap.Shutdown();
            IsPlayer = false;
            Debug.Log("[Session] Игрок отключён.");
        }
    }
}