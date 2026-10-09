using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class RollsTab : ITab
    {
        public string Name => "Броски";

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private int _selectedIndex = -1;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            EnsureIds();

            if (_selectedIndex < 0 && chronicle.RollPresets.Count > 0)
                _selectedIndex = 0;

            _root.Add(UIRoot.MakeHeader("Предустановки бросков"));
            _root.Add(UIRoot.MakeMuted(
                "Готовые наборы параметров для самых частых бросков. Модификатор, сложность " +
                "и целевое число успехов задаются в момент броска. Предустановки можно сохранять " +
                "в библиотеку контента и использовать в других хрониках."));

            var split = new VisualElement();
            split.style.flexDirection = FlexDirection.Row;
            split.style.flexGrow = 1;
            split.style.flexBasis = 0;
            _root.Add(split);

            BuildLeftPanel(split);
            BuildRightPanel(split);

            RefreshList();
            RefreshForm();
        }

        /// <summary>
        /// Старые предустановки из дефолтного JSON не имеют Id — генерируем.
        /// </summary>
        private void EnsureIds()
        {
            foreach (var p in _chr.RollPresets)
            {
                if (string.IsNullOrEmpty(p.Id))
                    p.Id = JsonSaveSystem.NewId("roll");
            }
        }

        // ============================================================
        // Левая панель
        // ============================================================

        private void BuildLeftPanel(VisualElement parent)
        {
            var left = new VisualElement();
            left.style.width = 320;
            left.style.minWidth = 260;
            left.style.flexShrink = 0;
            left.style.marginRight = 14;
            left.style.paddingTop = 12;
            left.style.paddingBottom = 12;
            left.style.paddingLeft = 12;
            left.style.paddingRight = 12;
            left.style.backgroundColor = UIRoot.BgPanel;
            left.style.borderTopLeftRadius = 6;
            left.style.borderTopRightRadius = 6;
            left.style.borderBottomLeftRadius = 6;
            left.style.borderBottomRightRadius = 6;
            parent.Add(left);

            var title = UIRoot.MakeLabel("Предустановки", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            left.Add(UIWidgets.MakeLongButton("＋  Добавить предустановку", AddPreset, UIRoot.AccentGreen));
            left.Add(UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral));
            left.Add(UIWidgets.MakeLongButton("✕  Удалить выбранную", DeletePreset, UIRoot.AccentRed));
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.RollPresets.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет предустановок. Добавьте первую."));
                return;
            }

            for (int i = 0; i < _chr.RollPresets.Count; i++)
            {
                int idx = i;
                var p = _chr.RollPresets[idx];

                var btn = new Button(() =>
                {
                    _selectedIndex = idx;
                    RefreshList();
                    RefreshForm();
                });
                btn.text = string.IsNullOrEmpty(p.Name) ? "(без имени)" : p.Name;
                bool isSelected = idx == _selectedIndex;
                UIRoot.StyleButton(btn,
                    isSelected ? UIRoot.AccentBlue : UIRoot.AccentNeutral,
                    Color.white, 32);
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.marginTop = 2;
                btn.style.marginBottom = 2;
                _listPanel.Add(btn);
            }
        }

        // ============================================================
        // Правая панель
        // ============================================================

        private void BuildRightPanel(VisualElement parent)
        {
            _formPanel = new ScrollView();
            _formPanel.style.flexGrow = 1;
            _formPanel.style.flexBasis = 0;
            _formPanel.style.paddingLeft = 4;
            _formPanel.style.paddingRight = 4;
            parent.Add(_formPanel);
        }

        private void RefreshForm()
        {
            _formPanel.Clear();

            if (_selectedIndex < 0 || _selectedIndex >= _chr.RollPresets.Count)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите предустановку слева или добавьте новую."));
                return;
            }

            var p = _chr.RollPresets[_selectedIndex];
            if (p.Params == null) p.Params = new List<string>();

            _formPanel.Add(UIRoot.MakeHeader(string.IsNullOrEmpty(p.Name) ? "(без имени)" : p.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {p.Id}"));

            // --- Основное ---
            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название", p.Name, v =>
            {
                p.Name = v;
                RefreshList();
            }));
            _formPanel.Add(s1);

            // --- Параметры ---
            var s2 = UIWidgets.Section("Параметры броска");
            s2.Add(UIRoot.MakeMuted(
                "Параметры, которые суммируются в пул кубов. Обычно это атрибут + навык, " +
                "иногда + дисциплина."));

            var (paramIds, paramNames) = CollectParamOptions();
            s2.Add(UIWidgets.MakeMultiPickerBlock(
                "Параметры",
                paramIds, paramNames,
                p.Params,
                5,
                () => RefreshForm()));
            _formPanel.Add(s2);

            // --- Тип ---
            var s3 = UIWidgets.Section("Тип броска");

            var typeChoices = new List<string>
            {
                "Обычный",
                "Соревновательный",
                "Сопротивление"
            };
            int typeIdx = (int)p.Type;
            s3.Add(UIWidgets.MakePickerBlock(
                "Тип броска",
                typeChoices,
                typeIdx,
                i => p.Type = (RollType)i,
                width: 260));

            var extendedToggle = new Toggle("Расширенный бросок") { value = p.Extended };
            UIWidgets.StyleToggle(extendedToggle);
            extendedToggle.RegisterValueChangedCallback(e => p.Extended = e.newValue);
            s3.Add(extendedToggle);

            s3.Add(UIRoot.MakeMuted(
                "Модификатор к пулу, сложность и целевое число успехов задаются в момент броска.", 11));
            _formPanel.Add(s3);

            // --- Проверка и действия ---
            var s4 = UIWidgets.Section("Проверка и действия");

            var status = GetValidationStatus(p);
            if (status.Count == 0)
            {
                var ok = new Label("✓ Предустановка корректна");
                ok.style.color = new Color(0.55f, 0.85f, 0.55f);
                ok.style.fontSize = 13;
                s4.Add(ok);
            }
            else
            {
                foreach (var msg in status)
                {
                    var l = new Label("• " + msg);
                    l.style.color = new Color(0.95f, 0.7f, 0.3f);
                    l.style.fontSize = 13;
                    s4.Add(l);
                }
            }

            var saveLibBtn = UIWidgets.MakeLongButton(
                "📚  Сохранить в библиотеку",
                () =>
                {
                    // Страховка: если Id почему-то пустой — сгенерируем.
                    if (string.IsNullOrEmpty(p.Id))
                        p.Id = JsonSaveSystem.NewId("roll");

                    SessionContext.Library.AddOrUpdateRollPreset(p);
                    Debug.Log($"[RollsTab] Предустановка сохранена в библиотеку: {p.Name} ({p.Id})");
                    RefreshForm();
                },
                UIRoot.AccentGreen);
            s4.Add(saveLibBtn);

            _formPanel.Add(s4);
        }

        private (List<string> ids, List<string> names) CollectParamOptions()
        {
            var ids = new List<string>();
            var names = new List<string>();

            foreach (var a in _chr.Attributes)
            {
                ids.Add("attr." + a.Id);
                names.Add($"Атрибут: {a.Name}");
            }
            foreach (var s in _chr.Skills)
            {
                ids.Add("skill." + s.Id);
                names.Add($"Навык: {_chr.GetSkillName(s)}");
            }
            foreach (var d in _chr.Disciplines)
            {
                ids.Add("disc." + d.Id);
                names.Add($"Дисциплина: {d.Name}");
            }

            ids.Add("willpower.current"); names.Add("Система: Текущая сила воли");
            ids.Add("blood.current"); names.Add("Система: Текущий запас крови");

            return (ids, names);
        }

        private List<string> GetValidationStatus(RollPreset p)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(p.Name))
                result.Add("Название не заполнено.");

            if (p.Params == null || p.Params.Count == 0)
                result.Add("Не выбрано ни одного параметра.");

            return result;
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddPreset()
        {
            var p = new RollPreset
            {
                Id = JsonSaveSystem.NewId("roll"),
                Name = $"Новая предустановка {_chr.RollPresets.Count + 1}",
                Params = new List<string>(),
                Type = RollType.Regular,
                Extended = false
            };
            _chr.RollPresets.Add(p);
            _selectedIndex = _chr.RollPresets.Count - 1;
            Debug.Log($"[RollsTab] Добавлена предустановка.");
            RefreshList();
            RefreshForm();
        }

        private void DeletePreset()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _chr.RollPresets.Count) return;

            var removed = _chr.RollPresets[_selectedIndex];
            _chr.RollPresets.RemoveAt(_selectedIndex);
            Debug.LogWarning($"[RollsTab] Удалена предустановка: {removed.Name}");

            _selectedIndex = _chr.RollPresets.Count > 0 ? 0 : -1;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.RollPresets == null || lib.RollPresets.Count == 0)
            {
                Debug.LogWarning("[RollsTab] Библиотека пуста (нет предустановок).");
                return;
            }

            int added = 0;
            foreach (var preset in lib.RollPresets)
            {
                if (_chr.RollPresets.Any(x => x.Id == preset.Id)) continue;

                var json = JsonSaveSystem.Serialize(preset);
                var clone = JsonSaveSystem.Deserialize<RollPreset>(json);

                // Клонируем Id, чтобы импорт не перетёр библиотечный.
                clone.Id = JsonSaveSystem.NewId("roll");
                _chr.RollPresets.Add(clone);
                added++;
            }

            Debug.Log($"[RollsTab] Импортировано из библиотеки: {added} предустановок.");
            RefreshList();
            RefreshForm();
        }
    }
}