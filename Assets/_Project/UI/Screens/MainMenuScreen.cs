using UnityEngine;
using UnityEngine.UIElements;

namespace VTR.UI
{
    public class MainMenuScreen : IScreen
    {
        public void Build(VisualElement root)
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            root.Add(container);

            container.Add(UIRoot.MakeHeader("VTM: Хроники", 36));

            // Имя игрока/мастера
            var nameField = new TextField();
            UIRoot.StyleTextField(nameField, "Ваше имя / никнейм");
            nameField.value = SessionContext.PlayerName;
            nameField.style.width = 320;
            container.Add(nameField);

            // Кнопка "Войти как мастер"
            var gmBtn = new Button(() => OnGameMasterClicked(nameField.value));
            gmBtn.text = "Войти как Мастер";
            UIRoot.StyleButton(gmBtn, new Color(0.55f, 0.20f, 0.20f), Color.white, 48);
            gmBtn.style.width = 320;
            container.Add(gmBtn);

            // Кнопка "Войти как игрок"
            var playerBtn = new Button(() => OnPlayerClicked(nameField.value));
            playerBtn.text = "Войти как Игрок";
            UIRoot.StyleButton(playerBtn, UIRoot.AccentBlue, Color.white, 48);
            playerBtn.style.width = 320;
            container.Add(playerBtn);

            // Подпись
            var version = UIRoot.MakeMuted("Фаза 4 — создание персонажа (в разработке)", 12);
            version.style.marginTop = 20;
            container.Add(version);

            // Кнопка "Выход"
            var quitBtn = new Button(OnQuitClicked);
            quitBtn.text = "✕  Выход";
            UIRoot.StyleButton(quitBtn, UIRoot.AccentNeutral, Color.white, 36);
            quitBtn.style.width = 320;
            quitBtn.style.marginTop = 30;
            container.Add(quitBtn);
        }

        private void OnGameMasterClicked(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("Введите имя.");
                return;
            }
            SessionContext.PlayerName = name.Trim();
            SessionContext.IsGameMaster = true;
            Debug.Log($"[MainMenu] Мастер: {SessionContext.PlayerName}");
            UIRoot.Instance.ShowChronicleList();
        }

        private void OnPlayerClicked(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("Введите имя.");
                return;
            }
            SessionContext.PlayerName = name.Trim();
            SessionContext.IsGameMaster = false;
            Debug.Log($"[MainMenu] Игрок: {SessionContext.PlayerName}");
            UIRoot.Instance.ShowJoinGame();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Выход из игры.");

#if UNITY_EDITOR
            // В редакторе Application.Quit не работает — останавливаем Play вручную.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}