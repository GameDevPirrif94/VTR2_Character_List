using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class MoralitiesTab : ITab
    {
        public string Name => "Морали";

        private const int MaxAuraParams = 5;
        private const int SinLevels = 10;

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (string.IsNullOrEmpty(_selectedId) && chronicle.Moralites.Count > 0)
                _selectedId = chronicle.Moralites[0].Id;

            _root.Add(UIRoot.MakeHeader("Морали"));
            _root.Add(UIRoot.MakeMuted(
                "Шкалы морали (по умолчанию — Человечность). Для каждой задаются стартовое значение, " +
                "10 грехов по степени тяжести и параметры, на которые влияет аура."));

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

        // ============================================================
        // Левая панель
        // ============================================================

        private void BuildLeftPanel(VisualElement parent)
        {
            var left = new VisualElement();
            left.style.width = 280;
            left.style.minWidth = 240;
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

            var title = UIRoot.MakeLabel("Морали хроники", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            left.Add(UIWidgets.MakeLongButton("＋  Новая мораль", AddMorality, UIRoot.AccentGreen));
            left.Add(UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral));
            left.Add(UIWidgets.MakeLongButton("✕  Удалить выбранную", DeleteMorality, UIRoot.AccentRed));
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Moralites.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет моралей. Добавьте первую."));
                return;
            }

            foreach (var m in _chr.Moralites)
            {
                var mor = m;
                var btn = new Button(() =>
                {
                    _selectedId = mor.Id;
                    RefreshList();
                    RefreshForm();
                });
                btn.text = mor.Name;
                bool isSelected = mor.Id == _selectedId;
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

            var m = _chr.FindMorality(_selectedId);
            if (m == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите мораль слева или добавьте новую."));
                return;
            }

            // Нормализуем данные, если из JSON пришло что-то нестандартное.
            EnsureSinsStructure(m);
            if (m.AuraAffectedParams == null)
                m.AuraAffectedParams = new List<string>();

            _formPanel.Add(UIRoot.MakeHeader(m.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {m.Id}"));

            // --- Основное ---
            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название морали", m.Name, v =>
            {
                m.Name = v;
                RefreshList();
            }));
            s1.Add(UIWidgets.MakeIntField("Стартовое значение (1–10)",
                m.StartRating, v => m.StartRating = v, 1, 10));
            s1.Add(UIWidgets.MakeTextField("Название ауры", m.AuraName, v => m.AuraName = v));
            _formPanel.Add(s1);

            // --- Параметры ауры ---
            var s2 = UIWidgets.Section($"Параметры, на которые влияет аура (до {MaxAuraParams})");
            s2.Add(UIRoot.MakeMuted(
                "Значение ауры берётся из таблицы аур и добавляется как бонус/штраф " +
                "к указанным параметрам."));

            var (allIds, allNames) = CollectAuraParamOptions();
            s2.Add(UIWidgets.MakeMultiPickerBlock(
                "Параметры ауры",
                allIds, allNames,
                m.AuraAffectedParams,
                MaxAuraParams,
                () => RefreshForm()));
            _formPanel.Add(s2);

            // --- 10 грехов ---
            var s3 = UIWidgets.Section("10 грехов (по степени тяжести)");
            s3.Add(UIRoot.MakeMuted(
                "Уровень 10 — самый лёгкий грех (почти безобидный). Уровень 1 — худшее преступление."));

            // Порядок: сверху 10, снизу 1 — так удобнее визуально.
            for (int lvl = SinLevels; lvl >= 1; lvl--)
            {
                int lvlLocal = lvl;
                var sin = m.Sins.FirstOrDefault(x => x.Level == lvlLocal);
                if (sin == null)
                {
                    sin = new MoralitySin { Level = lvlLocal, Text = "" };
                    m.Sins.Add(sin);
                }

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var lvlLbl = new Label($"{lvlLocal}");
                lvlLbl.style.minWidth = 26;
                lvlLbl.style.fontSize = 14;
                lvlLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                lvlLbl.style.color = (lvlLocal >= 8)
                    ? new Color(0.6f, 0.9f, 0.6f)
                    : (lvlLocal <= 3
                        ? new Color(0.95f, 0.55f, 0.55f)
                        : UIRoot.TextPrimary);
                lvlLbl.style.unityTextAlign = TextAnchor.MiddleRight;
                lvlLbl.style.marginRight = 10;
                row.Add(lvlLbl);

                var textField = new TextField { value = sin.Text ?? "" };
                textField.style.flexGrow = 1;
                UIRoot.StyleTextField(textField, "");
                textField.RegisterValueChangedCallback(e => sin.Text = e.newValue);
                row.Add(textField);

                s3.Add(row);
            }
            _formPanel.Add(s3);

            // --- Проверка и действия ---
            var s4 = UIWidgets.Section("Проверка и действия");

            var status = GetValidationStatus(m);
            if (status.Count == 0)
            {
                var ok = new Label("✓ Всё заполнено корректно");
                ok.style.color = new Color(0.55f, 0.85f, 0.55f);
                ok.style.fontSize = 13;
                s4.Add(ok);
            }
            else
            {
                foreach (var p in status)
                {
                    var l = new Label("• " + p);
                    l.style.color = new Color(0.95f, 0.7f, 0.3f);
                    l.style.fontSize = 13;
                    s4.Add(l);
                }
            }

            var saveLibBtn = UIWidgets.MakeLongButton(
                "📚  Сохранить в библиотеку",
                () =>
                {
                    SessionContext.Library.AddOrUpdateMorality(m);
                    Debug.Log($"[MoralitiesTab] Мораль сохранена в библиотеку: {m.Name} ({m.Id})");
                },
                UIRoot.AccentGreen);
            s4.Add(saveLibBtn);
            _formPanel.Add(s4);
        }

        // ============================================================
        // Утилиты
        // ============================================================

        private void EnsureSinsStructure(Morality m)
        {
            if (m.Sins == null)
                m.Sins = new List<MoralitySin>();

            // Убедимся, что все 10 уровней присутствуют.
            for (int lvl = 1; lvl <= SinLevels; lvl++)
            {
                if (!m.Sins.Any(s => s.Level == lvl))
                    m.Sins.Add(new MoralitySin { Level = lvl, Text = "" });
            }

            // Удалим всё, что выходит за пределы 1..10.
            m.Sins.RemoveAll(s => s.Level < 1 || s.Level > SinLevels);
        }

        /// <summary>
        /// Собирает список id и имён для параметров ауры: все атрибуты, навыки, дисциплины
        /// + системные параметры (сила воли, запас крови).
        /// </summary>
        private (List<string> ids, List<string> names) CollectAuraParamOptions()
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

            // Системные параметры
            ids.Add("willpower.max");
            names.Add("Система: Макс. сила воли");
            ids.Add("blood.max");
            names.Add("Система: Макс. запас крови");

            return (ids, names);
        }

        // ============================================================
        // Валидация
        // ============================================================

        private List<string> GetValidationStatus(Morality m)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(m.Name))
                result.Add("Название морали пустое.");

            if (string.IsNullOrWhiteSpace(m.AuraName))
                result.Add("Название ауры не заполнено.");

            if (m.StartRating < 1 || m.StartRating > 10)
                result.Add($"Стартовое значение должно быть 1–10 (сейчас {m.StartRating}).");

            if (m.AuraAffectedParams == null || m.AuraAffectedParams.Count == 0)
                result.Add("Не выбраны параметры ауры.");

            int emptySins = 0;
            for (int lvl = 1; lvl <= SinLevels; lvl++)
            {
                var sin = m.Sins.FirstOrDefault(s => s.Level == lvl);
                if (sin == null || string.IsNullOrWhiteSpace(sin.Text))
                    emptySins++;
            }
            if (emptySins > 0)
                result.Add($"Не заполнено грехов: {emptySins} из {SinLevels}.");

            return result;
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddMorality()
        {
            var m = new Morality
            {
                Id = JsonSaveSystem.NewId("moral"),
                Name = $"Новая мораль {_chr.Moralites.Count + 1}",
                StartRating = 7,
                AuraName = "Аура",
                AuraAffectedParams = new List<string>(),
                Sins = new List<MoralitySin>()
            };
            for (int lvl = 1; lvl <= SinLevels; lvl++)
                m.Sins.Add(new MoralitySin { Level = lvl, Text = "" });

            _chr.Moralites.Add(m);
            _selectedId = m.Id;
            Debug.Log($"[MoralitiesTab] Добавлена мораль: {m.Id}");
            RefreshList();
            RefreshForm();
        }

        private void DeleteMorality()
        {
            var m = _chr.FindMorality(_selectedId);
            if (m == null) return;

            if (_chr.Moralites.Count <= 1)
            {
                Debug.LogWarning("[MoralitiesTab] Нельзя удалить последнюю мораль — персонажам нужна хотя бы одна.");
                return;
            }

            _chr.Moralites.Remove(m);
            Debug.LogWarning($"[MoralitiesTab] Удалена мораль: {m.Name} ({m.Id})");

            _selectedId = _chr.Moralites.Count > 0 ? _chr.Moralites[0].Id : null;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.Moralites.Count == 0)
            {
                Debug.LogWarning("[MoralitiesTab] Библиотека пуста (нет моралей).");
                return;
            }

            int added = 0;
            foreach (var mor in lib.Moralites)
            {
                if (_chr.Moralites.Any(x => x.Id == mor.Id)) continue;
                var json = JsonSaveSystem.Serialize(mor);
                var clone = JsonSaveSystem.Deserialize<Morality>(json);
                _chr.Moralites.Add(clone);
                added++;
            }

            Debug.Log($"[MoralitiesTab] Импортировано моралей: {added}.");
            RefreshList();
            RefreshForm();
        }
    }
}