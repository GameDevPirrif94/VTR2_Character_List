using UnityEngine;

namespace VTR.Net
{
    /// <summary>
    /// Раз в 15 секунд шлёт heartbeat в лобби, чтобы оно не истекало.
    /// Создаётся автоматически при старте хоста.
    /// </summary>
    public class HeartbeatHelper : MonoBehaviour
    {
        private static HeartbeatHelper _instance;
        private float _timer;

        public static void EnsureInstance()
        {
            if (_instance != null) return;
            var go = new GameObject("HeartbeatHelper");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<HeartbeatHelper>();
            Debug.Log("[Heartbeat] Запущен.");
        }

        public static void DestroyInstance()
        {
            if (_instance == null) return;
            Destroy(_instance.gameObject);
            _instance = null;
            Debug.Log("[Heartbeat] Остановлен.");
        }

        private void Update()
        {
            // Если сервисы не инициализированы — вообще не трогаем лобби.
            if (!ServicesInitializer.IsInitialized) return;

            _timer += Time.deltaTime;
            if (_timer >= 15f)
            {
                _timer = 0f;

                if (LobbyService.CurrentLobby != null && LobbyService.IsHost)
                {
                    _ = LobbyService.HeartbeatAsync();
                }
            }
        }
    }
}