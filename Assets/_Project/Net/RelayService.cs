using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace VTR.Net
{
    /// <summary>
    /// Обёртка над Unity Relay: создание и подключение к серверу-релею.
    /// Использует ServerEndpoints, чтобы выбрать правильный эндпоинт (dtls/udp/wss).
    /// </summary>
    public static class RelayService
    {
        private const string ConnectionType = "dtls"; // "udp" | "dtls" | "wss"

        // ============================================================
        // Хост
        // ============================================================

        public static async Task<(string joinCode, RelayServerData serverData)> CreateRelayAsync(int maxPlayers)
        {
            try
            {
                int maxConnections = Mathf.Max(1, maxPlayers - 1);
                Allocation allocation = await Unity.Services.Relay.RelayService.Instance
                    .CreateAllocationAsync(maxConnections);
                string joinCode = await Unity.Services.Relay.RelayService.Instance
                    .GetJoinCodeAsync(allocation.AllocationId);

                var serverData = BuildRelayServerData(allocation, isHost: true);

                Debug.Log($"[Relay] Аллокация создана. JoinCode: {joinCode}, " +
                          $"Endpoint: {serverData.Endpoint}, Secure: {serverData.IsSecure}");
                return (joinCode, serverData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Relay] Ошибка создания аллокации: {e}");
                throw;
            }
        }

        // ============================================================
        // Клиент
        // ============================================================

        public static async Task<RelayServerData> JoinRelayAsync(string joinCode)
        {
            try
            {
                JoinAllocation join = await Unity.Services.Relay.RelayService.Instance
                    .JoinAllocationAsync(joinCode);

                var serverData = BuildRelayServerData(join, isHost: false);

                Debug.Log($"[Relay] Подключение к аллокации. JoinCode: {joinCode}, " +
                          $"Endpoint: {serverData.Endpoint}, Secure: {serverData.IsSecure}");
                return serverData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Relay] Ошибка подключения: {e}");
                throw;
            }
        }

        // ============================================================
        // Утилита
        // ============================================================

        private static RelayServerData BuildRelayServerData(Allocation allocation, bool isHost)
        {
            // Выбираем правильный эндпоинт по типу соединения.
            var endpoint = allocation.ServerEndpoints
                .FirstOrDefault(ep => ep.ConnectionType == ConnectionType)
                ?? allocation.ServerEndpoints.First();

            bool isSecure = endpoint.ConnectionType == "dtls";

            var serverData = new RelayServerData(
                endpoint.Host,
                (ushort)endpoint.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.ConnectionData,   // хост использует свой же ConnectionData
                allocation.Key,
                isSecure);

            Debug.Log($"[Relay] Хост-эндпоинт: {endpoint.Host}:{endpoint.Port} ({endpoint.ConnectionType})");
            return serverData;
        }

        private static RelayServerData BuildRelayServerData(JoinAllocation allocation, bool isHost)
        {
            var endpoint = allocation.ServerEndpoints
                .FirstOrDefault(ep => ep.ConnectionType == ConnectionType)
                ?? allocation.ServerEndpoints.First();

            bool isSecure = endpoint.ConnectionType == "dtls";

            var serverData = new RelayServerData(
                endpoint.Host,
                (ushort)endpoint.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.HostConnectionData,   // клиент использует HostConnectionData
                allocation.Key,
                isSecure);

            Debug.Log($"[Relay] Клиент-эндпоинт: {endpoint.Host}:{endpoint.Port} ({endpoint.ConnectionType})");
            return serverData;
        }
    }
}