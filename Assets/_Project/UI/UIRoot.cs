using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIRoot : MonoBehaviour
    {
        public static UIRoot Instance { get; private set; }

        private UIDocument _doc;
        private VisualElement _root;

        // --- Палитра ---
        public static readonly Color BgDark = new Color(0.12f, 0.12f, 0.14f);
        public static readonly Color BgPanel = new Color(0.16f, 0.16f, 0.20f);
        public static readonly Color BgInput = new Color(0.24f, 0.24f, 0.30f);
        public static readonly Color BgInputFocus = new Color(0.28f, 0.28f, 0.36f);
        public static readonly Color BorderSoft = new Color(0.38f, 0.38f, 0.46f);
        public static readonly Color TextPrimary = new Color(0.96f, 0.96f, 0.96f);
        public static readonly Color TextMuted = new Color(0.70f, 0.70f, 0.74f);
        public static readonly Color TextDisabled = new Color(0.50f, 0.50f, 0.55f);

        public static readonly Color AccentBlue = new Color(0.25f, 0.45f, 0.75f);
        public static readonly Color AccentGreen = new Color(0.25f, 0.55f, 0.35f);
        public static readonly Color AccentRed = new Color(0.65f, 0.22f, 0.22f);
        public static readonly Color AccentNeutral = new Color(0.30f, 0.30f, 0.36f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _doc = GetComponent<UIDocument>();

            // Создаём мост между сетевыми событиями и UI,
            // если его ещё нет на этом объекте.
            if (GetComponent<NetworkEventsBridge>() == null)
                gameObject.AddComponent<NetworkEventsBridge>();
        }

        private void OnEnable()
        {
            _root = _doc.rootVisualElement;
            ShowMainMenu();
        }

        // ============================================================
        // Навигация
        // ============================================================

        public void ShowMainMenu() => Show(new MainMenuScreen());
        public void ShowChronicleList() => Show(new ChronicleListScreen());
        public void ShowJoinGame() => Show(new JoinGameScreen());
        public void ShowCharacterList() => Show(new CharacterListScreen());
        public void ShowCharacterWizard() => Show(new CharacterWizardScreen());

        public void ShowCharacterSheet() => Show(new CharacterSheetScreen());
        public void ShowGMScreen() => Show(new GMScreen());

        public void ShowGMScreenCharacterSheet(Character character)
        {
            SessionContext.CurrentCharacter = character;
            SessionContext.IsGMViewingCharacter = true;
            Show(new CharacterSheetScreen());
        }
        public void ShowChronicleEditor(Chronicle chronicle)
        {
            SessionContext.CurrentChronicle = chronicle;
            Show(new ChronicleEditorScreen());
        }

        public void ShowHostGame(Chronicle chronicle)
        {
            SessionContext.CurrentChronicle = chronicle;
            Show(new HostGameScreen());
        }

        public void Show(IScreen screen)
        {
            if (_root == null) _root = _doc.rootVisualElement;
            _root.Clear();
            _root.style.backgroundColor = BgDark;
            _root.style.color = TextPrimary;
            _root.style.flexGrow = 1;
            screen.Build(_root);
        }

        // ============================================================
        // Стили-утилиты
        // ============================================================

        public static void StyleButton(Button b, Color bg, Color textColor, int height = 40)
        {
            b.style.height = height;
            b.style.backgroundColor = bg;
            b.style.color = textColor;
            b.style.fontSize = 16;
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.marginBottom = 6;
            b.style.marginTop = 6;
            b.style.marginLeft = 4;
            b.style.marginRight = 4;
            b.style.paddingLeft = 12;
            b.style.paddingRight = 12;
            b.style.borderTopLeftRadius = 4;
            b.style.borderTopRightRadius = 4;
            b.style.borderBottomLeftRadius = 4;
            b.style.borderBottomRightRadius = 4;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
        }

        public static void StyleTextField(TextField f, string label)
        {
            f.label = label ?? "";

            f.style.marginBottom = 10;
            f.style.marginTop = 6;
            f.style.marginLeft = 4;
            f.style.marginRight = 4;
            f.style.fontSize = 15;

            var labelEl = f.labelElement;
            if (labelEl != null)
            {
                if (string.IsNullOrEmpty(label))
                {
                    labelEl.style.display = DisplayStyle.None;
                }
                else
                {
                    labelEl.style.display = DisplayStyle.Flex;
                    labelEl.style.color = TextPrimary;
                    labelEl.style.fontSize = 14;
                    labelEl.style.unityFontStyleAndWeight = FontStyle.Bold;
                    labelEl.style.minWidth = 140;
                    labelEl.style.marginRight = 10;
                    labelEl.style.unityTextAlign = TextAnchor.MiddleLeft;
                }
            }

            var input = f.Q<VisualElement>(className: "unity-base-text-field__input")
                     ?? f.Q<VisualElement>(className: "unity-text-field__input");

            if (input != null)
            {
                input.style.backgroundColor = BgInput;
                input.style.minHeight = 30;
                input.style.paddingLeft = 8;
                input.style.paddingRight = 8;
                input.style.paddingTop = 4;
                input.style.paddingBottom = 4;

                input.style.borderTopLeftRadius = 4;
                input.style.borderTopRightRadius = 4;
                input.style.borderBottomLeftRadius = 4;
                input.style.borderBottomRightRadius = 4;

                input.style.borderLeftWidth = 1;
                input.style.borderRightWidth = 1;
                input.style.borderTopWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftColor = BorderSoft;
                input.style.borderRightColor = BorderSoft;
                input.style.borderTopColor = BorderSoft;
                input.style.borderBottomColor = BorderSoft;
            }

            f.Query<TextElement>().ForEach(t =>
            {
                if (labelEl != null && t == labelEl) return;
                t.style.color = TextPrimary;
                t.style.fontSize = 15;
            });

            f.RegisterCallback<FocusInEvent>(_ => { if (input != null) input.style.backgroundColor = BgInputFocus; });
            f.RegisterCallback<FocusOutEvent>(_ => { if (input != null) input.style.backgroundColor = BgInput; });
        }

        public static Label MakeLabel(string text, int fontSize = 14, Color? color = null)
        {
            var l = new Label(text);
            l.style.fontSize = fontSize;
            l.style.color = color ?? TextPrimary;
            l.style.marginBottom = 4;
            l.style.marginTop = 4;
            l.style.marginLeft = 4;
            l.style.marginRight = 4;
            l.style.whiteSpace = WhiteSpace.Normal;
            return l;
        }

        public static Label MakeHeader(string text, int fontSize = 24)
        {
            var l = new Label(text);
            l.style.fontSize = fontSize;
            l.style.color = TextPrimary;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            l.style.marginBottom = 12;
            l.style.marginTop = 12;
            l.style.marginLeft = 4;
            return l;
        }

        public static Label MakeMuted(string text, int fontSize = 12)
            => MakeLabel(text, fontSize, TextMuted);

        /// <summary>
        /// Корневой визуальный элемент UI Toolkit. К нему можно добавлять
        /// оверлеи (модалки, попапы), которые не будут затронуты при Show().
        /// </summary>
        public static VisualElement PanelRoot
        {
            get
            {
                if (Instance == null) return null;
                if (Instance._root == null && Instance._doc != null)
                    Instance._root = Instance._doc.rootVisualElement;
                return Instance._root;
            }
        }
    }
}