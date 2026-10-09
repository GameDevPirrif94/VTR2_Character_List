using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class DisciplinesTab : ITab
    {
        public string Name => "Дисциплины";

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (string.IsNullOrEmpty(_selectedId) && chronicle.Disciplines.Count > 0)
                _selectedId = chronicle.Disciplines[0].Id;

            _root.Add(UIRoot.MakeHeader("Дисциплины"));
            _root.Add(UIRoot.MakeMuted(
                "Дисциплины, пути и амальгамы. Созданные дисциплины сохраняются в библиотеку контента " +
                "и могут использоваться в других хрониках."));

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
            left.style.width = 320;
            left.style.minWidth = 280;
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

            var title = UIRoot.MakeLabel("Дисциплины хроники", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            var addBtn = UIWidgets.MakeLongButton("＋  Добавить дисциплину", AddDiscipline, UIRoot.AccentGreen);
            left.Add(addBtn);

            var fromLibBtn = UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral);
            left.Add(fromLibBtn);

            var delBtn = UIWidgets.MakeLongButton("✕  Удалить выбранную", DeleteDiscipline, UIRoot.AccentRed);
            left.Add(delBtn);
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Disciplines.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет дисциплин. Добавьте первую."));
                return;
            }

            AddGroupHeader("Основные дисциплины");
            foreach (var d in _chr.Disciplines.Where(x => !x.IsPath && !x.IsAmalgam))
                _listPanel.Add(MakeListEntry(d, 0));

            var paths = _chr.Disciplines.Where(x => x.IsPath).ToList();
            if (paths.Count > 0)
            {
                AddGroupHeader("Пути");
                foreach (var p in paths)
                {
                    var parentDisc = _chr.FindDiscipline(p.ParentDisciplineId);
                    string parentName = parentDisc != null ? parentDisc.Name : "?";
                    _listPanel.Add(MakeListEntry(p, 0, $"путь «{parentName}»"));
                }
            }

            var amalgams = _chr.Disciplines.Where(x => x.IsAmalgam).ToList();
            if (amalgams.Count > 0)
            {
                AddGroupHeader("Амальгамы");
                foreach (var a in amalgams)
                    _listPanel.Add(MakeListEntry(a, 0, "амальгама"));
            }
        }

        private void AddGroupHeader(string text)
        {
            var l = UIRoot.MakeMuted(text, 11);
            l.style.marginTop = 8;
            l.style.marginBottom = 2;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            _listPanel.Add(l);
        }

        private VisualElement MakeListEntry(Discipline d, int indent, string tag = null)
        {
            var container = new VisualElement();
            container.style.marginLeft = indent;

            var btn = new Button(() =>
            {
                _selectedId = d.Id;
                RefreshList();
                RefreshForm();
            });
            btn.text = d.Name;
            bool isSelected = d.Id == _selectedId;
            UIRoot.StyleButton(btn,
                isSelected ? UIRoot.AccentBlue : UIRoot.AccentNeutral,
                Color.white, 32);
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;
            btn.style.marginTop = 2;
            btn.style.marginBottom = 2;
            container.Add(btn);

            if (!string.IsNullOrEmpty(tag))
            {
                var tagLbl = UIRoot.MakeMuted(tag, 10);
                tagLbl.style.marginLeft = 6;
                tagLbl.style.marginTop = -4;
                tagLbl.style.marginBottom = 4;
                container.Add(tagLbl);
            }

            return container;
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

            var d = _chr.FindDiscipline(_selectedId);
            if (d == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите дисциплину слева или добавьте новую."));
                return;
            }

            _formPanel.Add(UIRoot.MakeHeader(d.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {d.Id}"));

            // --- Основное ---
            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название", d.Name, v =>
            {
                d.Name = v;
                RefreshList();
            }));
            s1.Add(UIWidgets.MakeMultilineField("Описание", d.Description, v => d.Description = v, 100));

            var typeLabel = UIRoot.MakeLabel("Тип дисциплины", 14);
            typeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            s1.Add(typeLabel);

            var typeRow = new VisualElement();
            typeRow.style.flexDirection = FlexDirection.Row;
            typeRow.style.marginTop = 4;
            s1.Add(typeRow);

            var typeChoices = new List<string> { "Обычная дисциплина", "Путь другой дисциплины", "Амальгама" };
            int currentTypeIdx = d.IsAmalgam ? 2 : (d.IsPath ? 1 : 0);

            var typeDrop = new DropdownField(typeChoices, currentTypeIdx);
            UIWidgets.StyleDropdown(typeDrop, 320);
            typeDrop.RegisterValueChangedCallback(e =>
            {
                int idx = typeChoices.IndexOf(e.newValue);
                if (idx == 0)
                {
                    d.IsPath = false;
                    d.IsAmalgam = false;
                    d.ParentDisciplineId = "";
                    d.AmalgamRequirements.Clear();
                }
                else if (idx == 1)
                {
                    d.IsPath = true;
                    d.IsAmalgam = false;
                    d.AmalgamRequirements.Clear();
                }
                else
                {
                    d.IsPath = false;
                    d.IsAmalgam = true;
                    d.ParentDisciplineId = "";
                    if (d.AmalgamRequirements.Count == 0)
                        d.AmalgamRequirements.Add(new AmalgamRequirement());
                }
                RefreshList();
                RefreshForm();
            });
            typeRow.Add(typeDrop);
            _formPanel.Add(s1);

            // --- Родительская дисциплина (для пути) ---
            if (d.IsPath)
            {
                var s2 = UIWidgets.Section("Родительская дисциплина (путь)");
                s2.Add(UIRoot.MakeMuted(
                    "Игрок сможет изучить этот путь только если у него есть родительская дисциплина хотя бы 1-го уровня."));

                var candidates = _chr.Disciplines
                    .Where(x => x.Id != d.Id && !x.IsPath && !x.IsAmalgam)
                    .ToList();

                if (candidates.Count == 0)
                {
                    s2.Add(UIRoot.MakeMuted("(нет доступных родительских дисциплин)"));
                }
                else
                {
                    var names = candidates.Select(x => x.Name).ToList();
                    int idx = candidates.FindIndex(x => x.Id == d.ParentDisciplineId);
                    if (idx < 0) idx = 0;

                    var parentDrop = new DropdownField("Родитель", names, idx);
                    UIWidgets.StyleDropdown(parentDrop, 320);
                    parentDrop.RegisterValueChangedCallback(e =>
                    {
                        int i = names.IndexOf(e.newValue);
                        if (i >= 0) d.ParentDisciplineId = candidates[i].Id;
                    });
                    s2.Add(parentDrop);
                }
                _formPanel.Add(s2);
            }

            // --- Требования амальгамы ---
            if (d.IsAmalgam)
            {
                var s3 = UIWidgets.Section("Требования амальгамы");
                s3.Add(UIRoot.MakeMuted(
                    "Каждая амальгама требует уже вкачанные дисциплины до определённого уровня. " +
                    "У амальгамы только одна способность (см. секцию «Способности»)."));

                var reqBlock = new VisualElement();
                s3.Add(reqBlock);

                System.Action rebuildReqs = null;
                rebuildReqs = () =>
                {
                    reqBlock.Clear();

                    if (d.AmalgamRequirements.Count == 0)
                        reqBlock.Add(UIRoot.MakeMuted("(нет требований)"));

                    for (int i = 0; i < d.AmalgamRequirements.Count; i++)
                    {
                        int idx = i;
                        var req = d.AmalgamRequirements[idx];

                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.alignItems = Align.Center;
                        row.style.marginBottom = 4;

                        var disCandidates = _chr.Disciplines.Where(x => !x.IsAmalgam).ToList();
                        var disNames = disCandidates.Select(x => x.Name).ToList();
                        int sel = disCandidates.FindIndex(x => x.Id == req.DisciplineId);
                        if (sel < 0) sel = 0;

                        var disDrop = new DropdownField(disNames, sel);
                        UIWidgets.StyleDropdown(disDrop, 220);
                        disDrop.style.marginRight = 8;
                        disDrop.RegisterValueChangedCallback(e =>
                        {
                            int k = disNames.IndexOf(e.newValue);
                            if (k >= 0) req.DisciplineId = disCandidates[k].Id;
                        });
                        row.Add(disDrop);

                        var lvlField = MakeNarrowIntField("уровень", req.MinLevel, v => req.MinLevel = v, 1, 10);
                        lvlField.style.width = 190;
                        lvlField.style.minWidth = 190;
                        row.Add(lvlField);

                        var rm = new Button(() =>
                        {
                            d.AmalgamRequirements.RemoveAt(idx);
                            rebuildReqs();
                        });
                        rm.text = "✕";
                        UIRoot.StyleButton(rm, UIRoot.AccentRed, Color.white, 28);
                        rm.style.width = 40;
                        rm.style.marginLeft = 8;
                        row.Add(rm);

                        reqBlock.Add(row);
                    }

                    var addReqBtn = new Button(() =>
                    {
                        d.AmalgamRequirements.Add(new AmalgamRequirement
                        {
                            DisciplineId = _chr.Disciplines.FirstOrDefault(x => !x.IsAmalgam)?.Id ?? "",
                            MinLevel = 1
                        });
                        rebuildReqs();
                    });
                    addReqBtn.text = "＋  Добавить требование";
                    UIRoot.StyleButton(addReqBtn, UIRoot.AccentGreen, Color.white, 30);
                    reqBlock.Add(addReqBtn);
                };

                rebuildReqs();
                _formPanel.Add(s3);
            }

            // --- Действия ---
            var s4 = UIWidgets.Section("Действия");
            var saveLibBtn = UIWidgets.MakeLongButton(
                "📚  Сохранить в библиотеку",
                () =>
                {
                    SessionContext.Library.AddOrUpdateDiscipline(d);
                    Debug.Log($"[DisciplinesTab] Дисциплина сохранена в библиотеку: {d.Name} ({d.Id})");
                },
                UIRoot.AccentGreen);
            s4.Add(saveLibBtn);
            _formPanel.Add(s4);

            // --- Способности ---
            DisciplineAbilitiesEditor.Build(_formPanel, _chr, d, () =>
            {
                RefreshForm();
            });
        }

        // ============================================================
        // Узкое поле IntegerField
        // ============================================================

        private static IntegerField MakeNarrowIntField(string label, int value,
            System.Action<int> onChange, int min, int max)
        {
            var f = new IntegerField(label) { value = value };
            f.style.marginBottom = 4;
            f.style.marginTop = 0;
            f.style.marginLeft = 6;
            f.style.marginRight = 0;
            f.style.fontSize = 13;
            f.style.flexShrink = 0;

            var labelEl = f.labelElement;
            if (labelEl != null)
            {
                labelEl.style.color = UIRoot.TextPrimary;
                labelEl.style.fontSize = 12;
                labelEl.style.minWidth = 60;
                labelEl.style.marginRight = 6;
                labelEl.style.unityTextAlign = TextAnchor.MiddleLeft;
                labelEl.style.flexShrink = 0;
                labelEl.style.whiteSpace = WhiteSpace.NoWrap;
            }

            var input = f.Q<VisualElement>(className: "unity-base-text-field__input")
                     ?? f.Q<VisualElement>(className: "unity-text-field__input");
            if (input != null)
            {
                input.style.backgroundColor = UIRoot.BgInput;
                input.style.minHeight = 30;
                input.style.height = 30;
                input.style.minWidth = 60;
                input.style.paddingLeft = 8;
                input.style.paddingRight = 8;
                input.style.flexGrow = 1;
                input.style.flexShrink = 0;
                input.style.borderTopLeftRadius = 4;
                input.style.borderTopRightRadius = 4;
                input.style.borderBottomLeftRadius = 4;
                input.style.borderBottomRightRadius = 4;
                input.style.borderLeftWidth = 1;
                input.style.borderRightWidth = 1;
                input.style.borderTopWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftColor = UIRoot.BorderSoft;
                input.style.borderRightColor = UIRoot.BorderSoft;
                input.style.borderTopColor = UIRoot.BorderSoft;
                input.style.borderBottomColor = UIRoot.BorderSoft;
            }

            f.Query<TextElement>().ForEach(t =>
            {
                if (labelEl != null && t == labelEl) return;
                t.style.color = UIRoot.TextPrimary;
                t.style.fontSize = 14;
            });

            f.RegisterValueChangedCallback(e =>
            {
                int v = Mathf.Clamp(e.newValue, min, max);
                if (v != e.newValue) f.SetValueWithoutNotify(v);
                onChange?.Invoke(v);
            });

            return f;
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddDiscipline()
        {
            var d = new Discipline
            {
                Id = JsonSaveSystem.NewId("disc"),
                Name = $"Новая дисциплина {_chr.Disciplines.Count + 1}",
                Description = ""
            };
            _chr.Disciplines.Add(d);
            _selectedId = d.Id;
            Debug.Log($"[DisciplinesTab] Добавлена дисциплина: {d.Id}");
            RefreshList();
            RefreshForm();
        }

        private void DeleteDiscipline()
        {
            var d = _chr.FindDiscipline(_selectedId);
            if (d == null) return;

            var refs = new List<string>();
            foreach (var c in _chr.Clans)
                if (c.ClanDisciplineIds != null && c.ClanDisciplineIds.Contains(d.Id))
                    refs.Add($"клан «{c.Name}»");
            foreach (var p in _chr.Disciplines)
                if (p.IsPath && p.ParentDisciplineId == d.Id)
                    refs.Add($"путь «{p.Name}»");
            foreach (var a in _chr.Disciplines)
                if (a.IsAmalgam && a.AmalgamRequirements != null &&
                    a.AmalgamRequirements.Any(r => r.DisciplineId == d.Id))
                    refs.Add($"амальгама «{a.Name}»");

            if (refs.Count > 0)
            {
                Debug.LogWarning($"[DisciplinesTab] Нельзя удалить «{d.Name}»: используется в {string.Join(", ", refs)}");
                return;
            }

            _chr.Disciplines.Remove(d);
            Debug.LogWarning($"[DisciplinesTab] Удалена дисциплина: {d.Name} ({d.Id})");

            _selectedId = _chr.Disciplines.Count > 0 ? _chr.Disciplines[0].Id : null;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.Disciplines.Count == 0)
            {
                Debug.LogWarning("[DisciplinesTab] Библиотека пуста.");
                return;
            }

            int added = 0;
            foreach (var d in lib.Disciplines)
            {
                if (_chr.Disciplines.Any(x => x.Id == d.Id)) continue;
                var json = JsonSaveSystem.Serialize(d);
                var clone = JsonSaveSystem.Deserialize<Discipline>(json);
                _chr.Disciplines.Add(clone);
                added++;
            }

            Debug.Log($"[DisciplinesTab] Импортировано из библиотеки: {added} дисциплин.");
            RefreshList();
            RefreshForm();
        }
    }
}