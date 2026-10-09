using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI
{
    public partial class CharacterWizardScreen
    {
        private Image _portraitPreview;

        // ============================================================
        // ШАГ 6: Биография и портрет
        // ============================================================

        private void BuildStep_Biography(VisualElement parent)
        {
            parent.Add(UIRoot.MakeHeader("Шаг 6: Биография и портрет"));

            // --- Основные данные ---
            var s1 = UIWidgets.Section("Основные данные");
            s1.Add(UIWidgets.MakeTextField("Дата рождения", Draft.BirthDate,
                v => Draft.BirthDate = v));
            s1.Add(UIWidgets.MakeTextField("Дата смерти (обращения)",
                Draft.DeathDate, v => Draft.DeathDate = v));
            s1.Add(UIWidgets.MakeIntField("Визуальный возраст", Draft.VisualAge,
                v => Draft.VisualAge = v, 0, 999));
            s1.Add(UIWidgets.MakeIntField("Реальный возраст", Draft.RealAge,
                v => Draft.RealAge = v, 0, 9999));
            s1.Add(UIWidgets.MakeTextField("Национальность", Draft.Nationality,
                v => Draft.Nationality = v));

            var genderLabel = UIRoot.MakeLabel("Пол", 14);
            genderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            s1.Add(genderLabel);

            var radio = new RadioButtonGroup("", new List<string> { "Мужской", "Женский" });
            radio.value = Draft.Gender == "female" ? 1 : 0;
            radio.RegisterValueChangedCallback(e =>
            {
                Draft.Gender = e.newValue == 1 ? "female" : "male";
            });
            StyleRadio(radio);
            s1.Add(radio);

            parent.Add(s1);

            // --- Внешность ---
            var s2 = UIWidgets.Section("Внешность");
            s2.Add(UIWidgets.MakeTextField("Телосложение", Draft.Build,
                v => Draft.Build = v));
            s2.Add(UIWidgets.MakeTextField("Рост (см)", Draft.Height,
                v => Draft.Height = v));
            s2.Add(UIWidgets.MakeTextField("Вес (кг)", Draft.Weight,
                v => Draft.Weight = v));
            s2.Add(UIWidgets.MakeTextField("Волосы", Draft.Hair,
                v => Draft.Hair = v));
            s2.Add(UIWidgets.MakeTextField("Глаза", Draft.Eyes,
                v => Draft.Eyes = v));
            s2.Add(UIWidgets.MakeMultilineField("Примечательные особенности",
                Draft.NotableFeatures, v => Draft.NotableFeatures = v, 80));
            parent.Add(s2);

            // --- Портрет ---
            BuildPortraitSection(parent);

            // --- Предыстория ---
            var s4 = UIWidgets.Section("Предыстория");
            s4.Add(UIWidgets.MakeMultilineField("Предыстория персонажа",
                Draft.Backstory, v => Draft.Backstory = v, 140));
            parent.Add(s4);

            // --- Убежище ---
            var s5 = UIWidgets.Section("Убежище");
            s5.Add(UIWidgets.MakeMultilineField("Описание жилья или убежища",
                Draft.Haven, v => Draft.Haven = v, 120));
            parent.Add(s5);

            // --- Финальная подсказка ---
            var s6 = UIWidgets.Section("Готовы завершить создание");
            s6.Add(UIRoot.MakeMuted(
                "Нажмите «Далее» ещё раз, чтобы завершить создание персонажа " +
                "и сохранить его. Все поля биографии необязательны."));
            parent.Add(s6);
        }

        // ============================================================
        // Портрет
        // ============================================================

        private void BuildPortraitSection(VisualElement parent)
        {
            var section = UIWidgets.Section("Портрет");

            section.Add(UIRoot.MakeMuted(
                "Файл изображения (PNG/JPG). Портрет будет сохранён вместе с персонажем."));

            _portraitPreview = new Image();
            _portraitPreview.style.width = 180;
            _portraitPreview.style.height = 180;
            _portraitPreview.style.backgroundColor = UIRoot.BgInput;
            _portraitPreview.style.marginBottom = 8;
            _portraitPreview.scaleMode = ScaleMode.ScaleToFit;
            section.Add(_portraitPreview);

            var pathField = new TextField("Путь к файлу");
            pathField.value = Draft.PortraitPath ?? "";
            UIRoot.StyleTextField(pathField, "Путь к файлу");
            pathField.RegisterValueChangedCallback(e =>
            {
                Draft.PortraitPath = e.newValue;
                RefreshPortraitPreview();
            });
            section.Add(pathField);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            section.Add(btnRow);

#if UNITY_EDITOR
            var browseBtn = new Button(() =>
            {
                string path = UnityEditor.EditorUtility.OpenFilePanel(
                    "Выберите портрет", "", "png,jpg,jpeg");
                if (!string.IsNullOrEmpty(path))
                {
                    Draft.PortraitPath = path;
                    pathField.SetValueWithoutNotify(path);
                    RefreshPortraitPreview();
                }
            });
            browseBtn.text = "📂  Выбрать файл";
            UIRoot.StyleButton(browseBtn, UIRoot.AccentBlue, Color.white, 34);
            btnRow.Add(browseBtn);
#endif

            var clearBtn = new Button(() =>
            {
                Draft.PortraitPath = "";
                pathField.SetValueWithoutNotify("");
                RefreshPortraitPreview();
            });
            clearBtn.text = "✕  Очистить";
            UIRoot.StyleButton(clearBtn, UIRoot.AccentNeutral, Color.white, 34);
            btnRow.Add(clearBtn);

            parent.Add(section);

            RefreshPortraitPreview();
        }

        private void RefreshPortraitPreview()
        {
            if (_portraitPreview == null) return;

            _portraitPreview.image = null;
            _portraitPreview.style.display = DisplayStyle.None;

            if (string.IsNullOrEmpty(Draft.PortraitPath)) return;
            if (!File.Exists(Draft.PortraitPath)) return;

            try
            {
                byte[] bytes = File.ReadAllBytes(Draft.PortraitPath);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                _portraitPreview.image = tex;
                _portraitPreview.style.display = DisplayStyle.Flex;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Wizard] Не удалось загрузить портрет: {e.Message}");
            }
        }

        private void StyleRadio(RadioButtonGroup radio)
        {
            radio.style.marginBottom = 8;
            radio.style.marginTop = 4;
            radio.Query<TextElement>().ForEach(t =>
            {
                t.style.color = UIRoot.TextPrimary;
                t.style.fontSize = 14;
            });
        }

        // ============================================================
        // Клонирование специализаций (используется в FinishCreation)
        // ============================================================

        private Dictionary<string, List<string>> CloneSpecs(
            Dictionary<string, List<string>> source)
        {
            var result = new Dictionary<string, List<string>>();
            if (source == null) return result;
            foreach (var kv in source)
                result[kv.Key] = new List<string>(kv.Value);
            return result;
        }
    }
}