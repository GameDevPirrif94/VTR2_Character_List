using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class MeritsTab : ITab
    {
        public string Name => "Черты";

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (string.IsNullOrEmpty(_selectedId) && chronicle.Merits.Count > 0)
                _selectedId = chronicle.Merits[0].Id;

            _root.Add(UIRoot.MakeHeader("Черты"));
            _root.Add(UIRoot.MakeMuted(
                "Черты персонажа: достоинства и недостатки. Стоимость может быть отрицательной — " +
                "тогда черта даёт персонажу дополнительные очки черт."));

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

            var title = UIRoot.MakeLabel("Черты хроники", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            left.Add(UIWidgets.MakeLongButton("＋  Добавить черту", AddMerit, UIRoot.AccentGreen));
            left.Add(UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral));
            left.Add(UIWidgets.MakeLongButton("✕  Удалить выбранную", DeleteMerit, UIRoot.AccentRed));
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Merits.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет черт. Добавьте первую."));
                return;
            }

            foreach (var m in _chr.Merits)
            {
                var merit = m;
                var btn = new Button(() =>
                {
                    _selectedId = merit.Id;
                    RefreshList();
                    RefreshForm();
                });

                string costStr = merit.Cost > 0 ? $"(+{merit.Cost})" :
                                 (merit.Cost < 0 ? $"({merit.Cost})" : "(0)");
                string multi = merit.CanBeLearnedMultipleTimes ? " ×N" : "";
                btn.text = merit.Name + "  " + costStr + multi;

                bool isSelected = merit.Id == _selectedId;
                UIRoot.StyleButton(btn,
                    isSelected ? UIRoot.AccentBlue : UIRoot.AccentNeutral,
                    Color.white, 32);
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.marginTop = 2;
                btn.style.marginBottom = 2;
                _listPanel.Add(btn);
            }
        }

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

            var merit = _chr.FindMerit(_selectedId);
            if (merit == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите черту слева или добавьте новую."));
                return;
            }

            if (merit.Modifiers == null)
                merit.Modifiers = new List<MeritModifier>();

            _formPanel.Add(UIRoot.MakeHeader(merit.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {merit.Id}"));

            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название черты", merit.Name, v =>
            {
                merit.Name = v;
                RefreshList();
            }));
            s1.Add(UIWidgets.MakeMultilineField("Описание", merit.Description,
                v => merit.Description = v, 80));

            var costHelp = UIRoot.MakeMuted(
                "Стоимость в очках черт. Положительная — персонаж тратит очки. " +
                "Отрицательная — черта даёт дополнительные очки (недостаток).", 11);
            s1.Add(costHelp);
            s1.Add(UIWidgets.MakeIntField("Стоимость",
                merit.Cost, v => { merit.Cost = v; RefreshList(); }, -20, 20));

            var multiToggle = new Toggle("Можно учить несколько раз")
            { value = merit.CanBeLearnedMultipleTimes };
            UIWidgets.StyleToggle(multiToggle);
            multiToggle.RegisterValueChangedCallback(e =>
            {
                merit.CanBeLearnedMultipleTimes = e.newValue;
                RefreshList();
            });
            s1.Add(multiToggle);
            s1.Add(UIRoot.MakeMuted(
                "Если включено — игрок может взять эту черту несколько раз, " +
                "пока хватает очков. Если выключено — только один раз.", 11));

            _formPanel.Add(s1);

            // --- Модификаторы ---
            var s2 = UIWidgets.Section("Модификаторы");
            s2.Add(UIRoot.MakeMuted(
                "Бонусы и штрафы, которые черта даёт персонажу. Значение может быть отрицательным."));

            var (paramIds, paramNames) = CollectParamOptions();

            if (merit.Modifiers.Count == 0)
                s2.Add(UIRoot.MakeMuted("(модификаторов нет)"));

            for (int i = 0; i < merit.Modifiers.Count; i++)
            {
                int idx = i;
                var mod = merit.Modifiers[idx];

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 6;

                int selIdx = paramIds.IndexOf(mod.Target);
                if (selIdx < 0) selIdx = 0;

                var picker = UIWidgets.MakePickerBlock(
                    null,
                    paramNames.Count > 0 ? paramNames : new List<string> { "(нет параметров)" },
                    selIdx,
                    v =>
                    {
                        if (v >= 0 && v < paramIds.Count)
                            mod.Target = paramIds[v];
                    },
                    width: 260);
                picker.style.marginRight = 8;
                picker.style.marginBottom = 0;
                row.Add(picker);

                var valField = UIWidgets.MakeIntField("", mod.Value, v => mod.Value = v, -20, 20);
                valField.style.width = 80;
                valField.style.marginBottom = 0;
                valField.style.marginTop = 0;
                row.Add(valField);

                var rm = new Button(() =>
                {
                    merit.Modifiers.RemoveAt(idx);
                    RefreshForm();
                });
                rm.text = "✕";
                UIRoot.StyleButton(rm, UIRoot.AccentRed, Color.white, 28);
                rm.style.width = 40;
                rm.style.marginLeft = 8;
                rm.style.marginTop = 0;
                rm.style.marginBottom = 0;
                row.Add(rm);

                s2.Add(row);
            }

            var addModBtn = UIWidgets.MakeLongButton(
                "＋  Добавить модификатор",
                () =>
                {
                    merit.Modifiers.Add(new MeritModifier
                    {
                        Target = paramIds.FirstOrDefault() ?? "",
                        Value = 0
                    });
                    RefreshForm();
                },
                UIRoot.AccentGreen);
            s2.Add(addModBtn);
            _formPanel.Add(s2);

            // --- Проверка и действия ---
            var s3 = UIWidgets.Section("Проверка и действия");

            var status = GetValidationStatus(merit);
            if (status.Count == 0)
            {
                var ok = new Label("✓ Всё заполнено корректно");
                ok.style.color = new Color(0.55f, 0.85f, 0.55f);
                ok.style.fontSize = 13;
                s3.Add(ok);
            }
            else
            {
                foreach (var p in status)
                {
                    var l = new Label("• " + p);
                    l.style.color = new Color(0.95f, 0.7f, 0.3f);
                    l.style.fontSize = 13;
                    s3.Add(l);
                }
            }

            var saveLibBtn = UIWidgets.MakeLongButton(
                "📚  Сохранить в библиотеку",
                () =>
                {
                    SessionContext.Library.AddOrUpdateMerit(merit);
                    Debug.Log($"[MeritsTab] Черта сохранена в библиотеку: {merit.Name} ({merit.Id})");
                },
                UIRoot.AccentGreen);
            s3.Add(saveLibBtn);
            _formPanel.Add(s3);
        }

        private (List<string> ids, List<string> names) CollectParamOptions()
        {
            var ids = new List<string>();
            var names = new List<string>();

            ids.Add(MeritSpecialTargets.FreeClanDiscipline);
            names.Add("★ Спец: Клановая дисциплина (выбор игрока)");

            ids.Add(MeritSpecialTargets.FreeRegularDiscipline);
            names.Add("★ Спец: Неклановая дисциплина (выбор игрока)");

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

            ids.Add("willpower.max"); names.Add("Система: Макс. сила воли");
            ids.Add("willpower.current"); names.Add("Система: Текущая сила воли");
            ids.Add("blood.max"); names.Add("Система: Макс. запас крови");
            ids.Add("blood.current"); names.Add("Система: Текущий запас крови");
            ids.Add("morality"); names.Add("Система: Мораль");
            ids.Add("aura"); names.Add("Система: Аура");
            ids.Add("health.max"); names.Add("Система: Макс. здоровье");
            ids.Add("blood.potency"); names.Add("Система: Густота крови");

            return (ids, names);
        }

        private List<string> GetValidationStatus(Merit m)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(m.Name))
                result.Add("Название черты пустое.");

            if (string.IsNullOrWhiteSpace(m.Description))
                result.Add("Описание не заполнено.");

            if (m.Modifiers != null)
            {
                for (int i = 0; i < m.Modifiers.Count; i++)
                {
                    if (string.IsNullOrEmpty(m.Modifiers[i].Target))
                        result.Add($"Модификатор #{i + 1}: параметр не выбран.");
                    if (m.Modifiers[i].Value == 0)
                        result.Add($"Модификатор #{i + 1}: значение равно 0.");
                }
            }

            return result;
        }

        private void AddMerit()
        {
            var merit = new Merit
            {
                Id = JsonSaveSystem.NewId("merit"),
                Name = $"Новая черта {_chr.Merits.Count + 1}",
                Description = "",
                Cost = 1,
                CanBeLearnedMultipleTimes = false,
                Modifiers = new List<MeritModifier>()
            };
            _chr.Merits.Add(merit);
            _selectedId = merit.Id;
            Debug.Log($"[MeritsTab] Добавлена черта: {merit.Id}");
            RefreshList();
            RefreshForm();
        }

        private void DeleteMerit()
        {
            var merit = _chr.FindMerit(_selectedId);
            if (merit == null) return;

            var refs = new List<string>();

            foreach (var bl in _chr.Bloodlines)
            {
                if (bl.PrimaryBonus != null &&
                    bl.PrimaryBonus.Type == BloodlineBonusType.Merit &&
                    bl.PrimaryBonus.ReferenceId == merit.Id)
                    refs.Add($"осн. бонус бладлайна «{bl.Name}»");

                if (bl.ExtraBonuses != null)
                    foreach (var bonus in bl.ExtraBonuses)
                        if (bonus.Type == BloodlineBonusType.Merit &&
                            bonus.ReferenceId == merit.Id)
                            refs.Add($"доп. бонус бладлайна «{bl.Name}»");
            }

            foreach (var r in _chr.Ranks)
                if (r.FirstCharBonus?.MeritIds != null && r.FirstCharBonus.MeritIds.Contains(merit.Id))
                    refs.Add($"бонус ранга «{r.Name}»");

            if (refs.Count > 0)
            {
                Debug.LogWarning($"[MeritsTab] Нельзя удалить «{merit.Name}»: используется в {string.Join(", ", refs)}");
                return;
            }

            _chr.Merits.Remove(merit);
            Debug.LogWarning($"[MeritsTab] Удалена черта: {merit.Name} ({merit.Id})");

            _selectedId = _chr.Merits.Count > 0 ? _chr.Merits[0].Id : null;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.Merits.Count == 0)
            {
                Debug.LogWarning("[MeritsTab] Библиотека пуста (нет черт).");
                return;
            }

            int added = 0;
            foreach (var merit in lib.Merits)
            {
                if (_chr.Merits.Any(x => x.Id == merit.Id)) continue;
                var json = JsonSaveSystem.Serialize(merit);
                var clone = JsonSaveSystem.Deserialize<Merit>(json);
                _chr.Merits.Add(clone);
                added++;
            }

            Debug.Log($"[MeritsTab] Импортировано из библиотеки: {added} черт.");
            RefreshList();
            RefreshForm();
        }
    }
}