using System;
using Unity.Netcode;
using UnityEngine;
using VTR.Core.Models;
using VTR.Data;
using VTR.Net;

namespace VTR.UI
{
    /// <summary>
    /// Мост между сетевыми событиями NetworkGameManager и UI.
    /// Автоматически подписывается на события менеджера, когда он появляется,
    /// и перерисовывает открытые экраны при изменениях.
    /// </summary>
    public class NetworkEventsBridge : MonoBehaviour
    {
        private NetworkGameManager _subscribed;

        private void Update()
        {
            // Если менеджер ещё не существует или сменился — переподписываемся.
            if (NetworkGameManager.Instance != _subscribed)
            {
                Unsubscribe();
                Subscribe();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _subscribed = NetworkGameManager.Instance;
            if (_subscribed == null) return;

            _subscribed.OnCharacterReceived += HandleCharacterReceived;
            _subscribed.OnCharacterRemoved += HandleCharacterRemoved;
            _subscribed.OnChronicleReceived += HandleChronicleReceived;

            Debug.Log("[Bridge] Подписка на сетевые события выполнена.");
        }

        private void Unsubscribe()
        {
            if (_subscribed == null) return;

            _subscribed.OnCharacterReceived -= HandleCharacterReceived;
            _subscribed.OnCharacterRemoved -= HandleCharacterRemoved;
            _subscribed.OnChronicleReceived -= HandleChronicleReceived;
            _subscribed = null;
        }

        // ============================================================
        // Обработчики
        // ============================================================

        private void HandleCharacterReceived(ulong ownerId, string json)
        {
            ulong myId = 0;
            if (NetworkManager.Singleton != null)
                myId = NetworkManager.Singleton.LocalClientId;

            // 1. Это обновление МОЕГО персонажа (мастер изменил через чит-режим).
            if (ownerId == myId)
            {
                try
                {
                    var ch = JsonSaveSystem.Deserialize<Character>(json);
                    if (ch == null) return;

                    JsonSaveSystem.SaveCharacter(ch);
                    Debug.Log($"[Bridge] Мой персонаж обновлён мастером и сохранён: {ch.Name}");

                    // Если сейчас открыт чар-лист именно этого персонажа — перерисуем.
                    if (!SessionContext.IsGMViewingCharacter &&
                        SessionContext.CurrentCharacter != null &&
                        SessionContext.CurrentCharacter.Id == ch.Id)
                    {
                        SessionContext.CurrentCharacter = ch;
                        UIRoot.Instance.ShowCharacterSheet();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Bridge] Ошибка обработки своего персонажа: {e.Message}");
                }
                return;
            }

            // 2. Это обновление персонажа ДРУГОГО игрока (мастер смотрит чужой чар-лист).
            if (SessionContext.IsGMViewingCharacter && SessionContext.GMViewingOwnerId == ownerId)
            {
                try
                {
                    var ch = JsonSaveSystem.Deserialize<Character>(json);
                    if (ch == null) return;

                    SessionContext.CurrentCharacter = ch;
                    UIRoot.Instance.ShowCharacterSheet();
                    Debug.Log($"[Bridge] Чар-лист игрока {ownerId} обновлён.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Bridge] Ошибка обновления чужого персонажа: {e.Message}");
                }
            }
        }

        private void HandleCharacterRemoved(ulong ownerId)
        {
            Debug.Log($"[Bridge] Персонаж игрока {ownerId} удалён.");

            // Если у мастера открыт чар-лист этого игрока — возвращаемся на GMScreen.
            if (SessionContext.IsGMViewingCharacter && SessionContext.GMViewingOwnerId == ownerId)
            {
                SessionContext.IsGMViewingCharacter = false;
                SessionContext.GMViewingOwnerId = 0;
                SessionContext.CurrentCharacter = null;
                UIRoot.Instance.ShowGMScreen();
            }
        }

        private void HandleChronicleReceived(string json)
        {
            try
            {
                var chr = JsonSaveSystem.Deserialize<Chronicle>(json);
                if (chr == null) return;

                JsonSaveSystem.SaveChronicle(chr);
                SessionContext.CurrentChronicle = chr;
                Debug.Log($"[Bridge] Хроника получена и обновлена: {chr.Name}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Bridge] Ошибка обработки хроники: {e.Message}");
            }
        }
    }
}