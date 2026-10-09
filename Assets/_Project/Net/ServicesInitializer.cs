using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace VTR.Net
{
    /// <summary>
    /// Инициализация Unity Services и анонимный вход.
    /// Должна быть вызвана ОДИН раз при запуске игры.
    /// </summary>
    public static class ServicesInitializer
    {
        public static bool IsInitialized { get; private set; }
        public static string PlayerId => AuthenticationService.Instance?.PlayerId ?? "";

        public static async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
                return true;

            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    Debug.Log("[Services] Инициализация Unity Services...");
                    await UnityServices.InitializeAsync();
                    Debug.Log("[Services] Инициализировано.");
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("[Services] Анонимный вход...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[Services] Вход выполнен. PlayerId: {AuthenticationService.Instance.PlayerId}");
                }

                IsInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Services] Ошибка инициализации: {e}");
                return false;
            }
        }
    }
}