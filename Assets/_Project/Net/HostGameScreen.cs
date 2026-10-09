using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Net;

namespace VTR.UI
{
    public class HostGameScreen : IScreen
    {
        private Chronicle _chronicle;
        private VisualElement _root;
        private int _maxPlayers;

        public void Build(VisualElement root)
        {
            _chronicle = SessionContext.CurrentChronicle;
            _root = root;

            if (_chronicle == null)
            {
                root.Add(UIRoot.MakeLabel("Хроника не выбрана.", 16, Color.red));
                return;
            }

            _maxPlayers = Mathf.Clamp(_chronicle.MaxPlayers, 1, 32);

            // Проверяем, что лобби реально живо.
            bool isHostWithLobby = NetworkSession.IsHost
                                   && LobbyService.CurrentLobby != null
                                   && !string.IsNullOrEmpty(NetworkSession.Pin);

            if (isHostWithLobby)
            {
                BuildLobbyUI();
            }
            else
            {
                // Если сервис сессии считает нас хостом, а лобби нет — сбрасываем сессию.
                if (NetworkSession.IsHost && LobbyService.CurrentLobby == null)
                {
                    Debug.LogWarning("[HostGame] Лобби потеряно, сбрасываю сессию хоста.");
                    NetworkSession.Shutdown();
                }
                BuildSetupForm();
            }
        }

        // ============================================================
        // Форма настройки
        // ============================================================

        private void BuildSetupForm()
        {
            _root.Clear();

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            _root.Add(container);

            container.Add(UIRoot.MakeHeader("Создание лобби", 26));

            var info = UIWidgets.Section("Хроника");
            info.style.width = 500;
            info.Add(UIRoot.MakeLabel($"Название: {_chronicle.Name}", 15));
            info.Add(UIRoot.MakeLabel($"Время: {_chronicle.SelectedTimeFrame}", 14));
            info.Add(UIRoot.MakeLabel($"Рангов: {_chronicle.Ranks.Count}, дисциплин: {_chronicle.Disciplines.Count}", 14));
            container.Add(info);

            var settings = UIWidgets.Section("Параметры игры");
            settings.style.width = 500;

            var maxPlayersField = UIWidgets.MakeIntField(
                "Максимум игроков (включая вас)",
                _maxPlayers,
                v => _maxPlayers = v,
                1, 32);
            settings.Add(maxPlayersField);

            settings.Add(UIRoot.MakeMuted(
                "От 1 до 32. Клиентов может быть максимум на 1 меньше — вы сами занимаете слот хоста.",
                12));
            container.Add(settings);

            Button createBtn = new Button();
            createBtn.text = "🌐  Создать лобби";
            UIRoot.StyleButton(createBtn, UIRoot.AccentGreen, Color.white, 48);
            createBtn.style.width = 500;
            createBtn.clicked += async () => await OnCreateLobbyClicked(createBtn);
            container.Add(createBtn);

            var backBtn = new Button(() =>
            {
                SessionContext.CurrentChronicle = null;
                UIRoot.Instance.ShowChronicleList();
            });
            backBtn.text = "←  Отмена";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 36);
            backBtn.style.width = 500;
            container.Add(backBtn);
        }

        private async Task OnCreateLobbyClicked(Button btn)
        {
            btn.SetEnabled(false);
            btn.text = "⏳  Создание лобби...";

            var result = await NetworkSession.StartHostAsync(_chronicle, _maxPlayers);

            if (!result.Ok)
            {
                btn.SetEnabled(true);
                btn.text = "🌐  Создать лобби";
                Debug.LogError($"[HostGameScreen] Ошибка: {result.Error}");
                var errLbl = UIRoot.MakeLabel($"Ошибка: {result.Error}", 14, new Color(1f, 0.6f, 0.6f));
                _root.Add(errLbl);
                return;
            }

            BuildLobbyUI();
        }

        // ============================================================
        // Лобби
        // ============================================================

        private void BuildLobbyUI()
        {
            _root.Clear();

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.paddingTop = 20;
            container.style.paddingBottom = 20;
            container.style.paddingLeft = 30;
            container.style.paddingRight = 30;
            _root.Add(container);

            container.Add(UIRoot.MakeHeader("Лобби создано", 26));
            container.Add(UIRoot.MakeMuted($"Хроника: {_chronicle.Name}"));

            // --- PIN ---
            var pinSection = UIWidgets.Section("PIN-код для игроков");
            pinSection.style.alignItems = Align.Center;

            var pinLbl = new Label(NetworkSession.Pin);
            pinLbl.style.fontSize = 64;
            pinLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            pinLbl.style.color = new Color(0.95f, 0.8f, 0.35f);
            pinLbl.style.marginTop = 8;
            pinLbl.style.marginBottom = 8;
            pinLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            pinSection.Add(pinLbl);

            pinSection.Add(UIRoot.MakeMuted(
                "Передайте этот код игрокам. Они вводят его в своём клиенте на экране «Войти как Игрок»."));
            container.Add(pinSection);

            // --- Игроки ---
            var playersSection = UIWidgets.Section("Подключившиеся игроки");

            var playerCountLbl = new Label();
            playerCountLbl.style.fontSize = 20;
            playerCountLbl.style.color = UIRoot.TextPrimary;
            playerCountLbl.style.marginBottom = 6;
            playersSection.Add(playerCountLbl);

            var playerListLbl = UIRoot.MakeMuted("Список игроков появится после подключения.", 13);
            playersSection.Add(playerListLbl);

            container.Add(playersSection);

            // --- Поллинг ---
            float lastShown = -1;
            playerCountLbl.schedule.Execute(() =>
            {
                var mgr = NetworkGameManager.Instance;
                if (mgr == null) return;
                int count = mgr.PlayerCount.Value;
                if (count != lastShown)
                {
                    lastShown = count;
                    playerCountLbl.text = $"Подключено: {count} / {NetworkSession.MaxPlayers}";
                    if (count == 1)
                        playerListLbl.text = "Пока никого. Ждём игроков...";
                    else
                        playerListLbl.text = $"{count - 1} игрок(ов) в лобби.";
                }
            }).Every(500);

            // --- Кнопки ---
            var buttonsSection = UIWidgets.Section("Действия");

            var startBtn = new Button(() =>
            {
                UIRoot.Instance.ShowGMScreen();
            });
            startBtn.text = "▶  В экран мастера";
            UIRoot.StyleButton(startBtn, UIRoot.AccentGreen, Color.white, 48);
            startBtn.style.width = 400;
            buttonsSection.Add(startBtn);

            var copyBtn = new Button(() =>
            {
                GUIUtility.systemCopyBuffer = NetworkSession.Pin;
                Debug.Log($"[HostGameScreen] PIN скопирован в буфер: {NetworkSession.Pin}");
            });
            copyBtn.text = "📋  Скопировать PIN";
            UIRoot.StyleButton(copyBtn, UIRoot.AccentBlue, Color.white, 36);
            copyBtn.style.width = 400;
            buttonsSection.Add(copyBtn);

            var cancelBtn = new Button(() =>
            {
                NetworkSession.Shutdown();
                SessionContext.CurrentChronicle = null;
                UIRoot.Instance.ShowChronicleList();
            });
            cancelBtn.text = "✕  Закрыть лобби";
            UIRoot.StyleButton(cancelBtn, UIRoot.AccentRed, Color.white, 36);
            cancelBtn.style.width = 400;
            buttonsSection.Add(cancelBtn);

            container.Add(buttonsSection);
        }
    }
}