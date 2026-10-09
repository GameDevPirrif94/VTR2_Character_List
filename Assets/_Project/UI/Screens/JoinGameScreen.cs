using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Net;

namespace VTR.UI
{
    public class JoinGameScreen : IScreen
    {
        private VisualElement _root;
        private string _pin = "";
        private string _playerName = "";

        public void Build(VisualElement root)
        {
            _root = root;
            _playerName = SessionContext.PlayerName;

            if (NetworkSession.IsPlayer)
                BuildConnectedUI();
            else
                BuildForm();
        }

        // ============================================================
        // Форма
        // ============================================================

        private void BuildForm()
        {
            _root.Clear();

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            _root.Add(container);

            container.Add(UIRoot.MakeHeader("Вход как Игрок", 26));

            var section = UIWidgets.Section("Подключение к лобби");
            section.style.width = 460;

            var nameField = UIWidgets.MakeTextField("Ваше имя / никнейм", _playerName, v => _playerName = v);
            section.Add(nameField);

            var pinField = UIWidgets.MakeTextField("PIN-код (4 цифры)", _pin, v =>
            {
                _pin = v != null ? v.Trim() : "";
            });
            section.Add(pinField);
            section.Add(UIRoot.MakeMuted(
                "PIN выдаёт мастер, когда создаёт лобби. Введите 4 цифры.", 12));

            container.Add(section);

            Button connectBtn = new Button();
            connectBtn.text = "🔌  Подключиться";
            UIRoot.StyleButton(connectBtn, UIRoot.AccentGreen, Color.white, 48);
            connectBtn.style.width = 460;
            connectBtn.clicked += async () => await OnConnectClicked(connectBtn, section);
            container.Add(connectBtn);

            var backBtn = new Button(() => UIRoot.Instance.ShowMainMenu());
            backBtn.text = "←  Назад";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 36);
            backBtn.style.width = 460;
            container.Add(backBtn);
        }

        private async Task OnConnectClicked(Button btn, VisualElement errorHost)
        {
            var old = errorHost.Q<Label>("error-label");
            if (old != null) old.RemoveFromHierarchy();

            if (string.IsNullOrWhiteSpace(_playerName))
            {
                ShowError(errorHost, "Введите имя.");
                return;
            }
            if (!PinGenerator.IsValid(_pin))
            {
                ShowError(errorHost, "PIN должен состоять из 4 цифр.");
                return;
            }

            btn.SetEnabled(false);
            btn.text = "⏳  Подключение...";

            var result = await NetworkSession.JoinAsPlayerAsync(_pin, _playerName.Trim());

            if (!result.Ok)
            {
                btn.SetEnabled(true);
                btn.text = "🔌  Подключиться";
                ShowError(errorHost, result.Error);
                return;
            }

            BuildConnectedUI();
        }

        private static void ShowError(VisualElement parent, string msg)
        {
            var lbl = UIRoot.MakeLabel($"⚠ {msg}", 14, new Color(1f, 0.55f, 0.55f));
            lbl.name = "error-label";
            lbl.style.marginTop = 8;
            parent.Add(lbl);
        }

        // ============================================================
        // Подключено
        // ============================================================

        private void BuildConnectedUI()
        {
            _root.Clear();

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            _root.Add(container);

            container.Add(UIRoot.MakeHeader("✓ Подключено к лобби", 26));

            var chronicle = SessionContext.CurrentChronicle;
            var info = UIWidgets.Section("Хроника");
            info.style.width = 520;
            if (chronicle != null)
            {
                info.Add(UIRoot.MakeLabel($"Название: {chronicle.Name}", 15));
                info.Add(UIRoot.MakeLabel($"Время: {chronicle.SelectedTimeFrame}", 14));
                info.Add(UIRoot.MakeLabel($"Рангов: {chronicle.Ranks.Count}", 14));
                info.Add(UIRoot.MakeLabel($"Кланов: {chronicle.Clans.Count}", 14));
                info.Add(UIRoot.MakeLabel($"Дисциплин: {chronicle.Disciplines.Count}", 14));
            }
            else
            {
                info.Add(UIRoot.MakeLabel("Хроника не загружена.", 14));
            }
            container.Add(info);

            var status = UIWidgets.Section("Статус");
            status.style.width = 520;
            status.Add(UIRoot.MakeLabel("Ожидаем, когда мастер начнёт игру.", 14));
            status.Add(UIRoot.MakeMuted(
                "Создание персонажа будет доступно после старта сессии. " +
                "Хроника уже сохранена локально — она будет доступна в следующих сессиях.", 12));
            container.Add(status);

            var charsBtn = new Button(() =>
            {
                UIRoot.Instance.ShowCharacterList();
            });
            charsBtn.text = "👤  Мои персонажи";
            UIRoot.StyleButton(charsBtn, UIRoot.AccentGreen, Color.white, 44);
            charsBtn.style.width = 520;
            container.Add(charsBtn);

            var leaveBtn = new Button(() =>
            {
                NetworkSession.ShutdownPlayer();
                UIRoot.Instance.ShowMainMenu();
            });
            leaveBtn.text = "✕  Покинуть лобби";
            UIRoot.StyleButton(leaveBtn, UIRoot.AccentRed, Color.white, 40);
            leaveBtn.style.width = 520;
            container.Add(leaveBtn);
        }
    }
}