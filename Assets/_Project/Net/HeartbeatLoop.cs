using UnityEngine;

namespace VTR.Net
{
    /// <summary>
    /// Раз в 15 секунд шлёт heartbeat, чтобы лобби не истекало.
    /// Вешается на любой GameObject в сцене мастера.
    /// </summary>
    public class HeartbeatLoop : MonoBehaviour
    {
        private float _timer = 0f;
        private const float Interval = 15f;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= Interval)
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