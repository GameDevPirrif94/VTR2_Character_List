using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;

namespace VTR.Net
{
    /// <summary>
    /// Управляет стартом Host/Client. Устанавливает транспорт и запускает NetworkManager.
    /// </summary>
    public static class NetworkBootstrap
    {
        public static async Task<bool> StartHostWithRelayAsync(RelayServerData serverData)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogError("[Net] NetworkManager не найден в сцене.");
                return false;
            }

            var transport = nm.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[Net] На NetworkManager нет UnityTransport.");
                return false;
            }

            transport.SetRelayServerData(serverData);

            bool ok = nm.StartHost();
            Debug.Log($"[Net] StartHost результат: {ok}");
            await Task.CompletedTask;
            return ok;
        }

        public static async Task<bool> StartClientWithRelayAsync(RelayServerData serverData)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogError("[Net] NetworkManager не найден в сцене.");
                return false;
            }

            var transport = nm.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[Net] На NetworkManager нет UnityTransport.");
                return false;
            }

            transport.SetRelayServerData(serverData);

            bool ok = nm.StartClient();
            Debug.Log($"[Net] StartClient результат: {ok}");
            await Task.CompletedTask;
            return ok;
        }

        public static void Shutdown()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();
        }
    }
}