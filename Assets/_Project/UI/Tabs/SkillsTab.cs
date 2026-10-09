using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class SkillsTab : ITab
    {
        public string Name => "Навыки";

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _scrollContent;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;
            Rebuild();
        }

        private void Rebuild()
        {
            _root.Clear();

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            _root.Add(scroll);
            _scrollContent = scroll;

            scroll.Add(UIRoot.MakeHeader("Навыки"));
            scroll.Add(UIRoot.MakeMuted(
                "Группы навыков, сами навыки и альтернативные названия по временным рамкам " +
                "(например, «Вождение» в DA = «Верховая езда», «Компьютеры» в DA = «Энигма»)."));

            BuildGroupsSection();
            BuildSkillsSection();
        }

        // ============================================================
        // Секция 1: группы
        // ============================================================

        private void BuildGroupsSection()
        {
            var section = UIWidgets.Section("Группы навыков");

            if (_chr.SkillGroups.Count == 0)
                section.Add(UIRoot.MakeMuted("(нет групп)"));

            foreach (var group in _chr.SkillGroups.ToList())
            {
                var g = group;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 6;

                var nameField = new TextField { value = g.Name };
                nameField.style.flexGrow = 1;
                UIRoot.StyleTextField(nameField, "");
                nameField.RegisterValueChangedCallback(e =>
                {
                    g.Name = e.newValue;
                    UpdateGroupHeaderLabel(g.Id);
                });
                row.Add(nameField);

                int used = _chr.Skills.Count(s => s.GroupId == g.Id);
                var info = UIRoot.MakeMuted($"навыков: {used}", 11);
                info.style.minWidth = 100;
                info.style.marginLeft = 8;
                info.style.marginRight = 8;
                row.Add(info);

                var delBtn = new Button(() => DeleteGroup(g));
                delBtn.text = "✕";
                UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 28);
                delBtn.style.width = 40;
                delBtn.SetEnabled(used == 0);
                row.Add(delBtn);

                section.Add(row);
            }

            var addBtn = new Button(AddGroup);
            addBtn.text = "＋  Добавить группу";
            UIRoot.StyleButton(addBtn, UIRoot.AccentGreen, Color.white, 34);
            section.Add(addBtn);

            _scrollContent.Add(section);
        }

        private void UpdateGroupHeaderLabel(string groupId)
        {
            var header = _scrollContent.Q<Label>($"skill-group-header-{groupId}");
            if (header != null)
            {
                var g = _chr.SkillGroups.Find(x => x.Id == groupId);
                if (g != null) header.text = g.Name;
            }
        }

        // ============================================================
        // Секция 2: навыки
        // ============================================================

        private void BuildSkillsSection()
        {
            var section = UIWidgets.Section("Навыки");

            if (_chr.SkillGroups.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("Сначала добавьте хотя бы одну группу."));
                _scrollContent.Add(section);
                return;
            }

            foreach (var group in _chr.SkillGroups)
            {
                var g = group;

                var groupBlock = new VisualElement();
                groupBlock.style.marginBottom = 20;

                var header = new Label(g.Name);
                header.name = $"skill-group-header-{g.Id}";
                header.style.fontSize = 15;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = UIRoot.TextPrimary;
                header.style.marginBottom = 6;
                groupBlock.Add(header);

                var skillsInGroup = _chr.Skills.Where(s => s.GroupId == g.Id).ToList();
                if (skillsInGroup.Count == 0)
                {
                    var empty = UIRoot.MakeMuted("(нет навыков)");
                    empty.style.marginLeft = 12;
                    groupBlock.Add(empty);
                }

                foreach (var skill in skillsInGroup)
                    groupBlock.Add(BuildSkillCard(skill));

                var addSkillBtn = new Button(() => AddSkill(g.Id));
                addSkillBtn.text = $"＋  Добавить навык в «{g.Name}»";
                UIRoot.StyleButton(addSkillBtn, UIRoot.AccentNeutral, Color.white, 30);
                addSkillBtn.style.marginLeft = 12;
                addSkillBtn.style.marginTop = 6;
                groupBlock.Add(addSkillBtn);

                section.Add(groupBlock);
            }

            _scrollContent.Add(section);
        }

        private VisualElement BuildSkillCard(SkillDef skill)
        {
            var s = skill;

            var card = new VisualElement();
            card.style.marginLeft = 12;
            card.style.marginBottom = 10;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.backgroundColor = new Color(0.14f, 0.14f, 0.18f);
            card.style.borderTopLeftRadius = 4;
            card.style.borderTopRightRadius = 4;
            card.style.borderBottomLeftRadius = 4;
            card.style.borderBottomRightRadius = 4;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;
            card.Add(row);

            var nameField = new TextField { value = s.Name };
            nameField.style.flexGrow = 1;
            UIRoot.StyleTextField(nameField, "");
            nameField.RegisterValueChangedCallback(e => s.Name = e.newValue);
            row.Add(nameField);

            var groupChoices = _chr.SkillGroups.Select(x => x.Name).ToList();
            int currentIdx = _chr.SkillGroups.FindIndex(x => x.Id == s.GroupId);
            if (currentIdx < 0) currentIdx = 0;

            var groupDrop = new DropdownField(groupChoices, currentIdx);
            groupDrop.style.width = 180;
            groupDrop.style.marginLeft = 8;
            UIWidgets.StyleDropdown(groupDrop);

            var skillLocal = s;
            groupDrop.RegisterValueChangedCallback(e =>
            {
                int newIdx = groupChoices.IndexOf(e.newValue);
                if (newIdx >= 0 && newIdx < _chr.SkillGroups.Count)
                {
                    skillLocal.GroupId = _chr.SkillGroups[newIdx].Id;
                    Rebuild();
                }
            });
            row.Add(groupDrop);

            var delBtn = new Button(() => DeleteSkill(s));
            delBtn.text = "✕";
            UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 28);
            delBtn.style.width = 40;
            delBtn.style.marginLeft = 8;
            row.Add(delBtn);

            var altHeader = UIRoot.MakeMuted("Альтернативные названия по временным рамкам (пусто = основное имя):", 11);
            altHeader.style.marginTop = 4;
            altHeader.style.marginLeft = 0;
            card.Add(altHeader);

            var altBlock = new VisualElement();
            altBlock.style.marginLeft = 8;
            card.Add(altBlock);

            if (s.AltByTimeFrame == null)
                s.AltByTimeFrame = new Dictionary<string, string>();

            foreach (var tf in _chr.TimeFrames)
            {
                string tfLocal = tf;
                string currentValue = "";
                if (s.AltByTimeFrame.TryGetValue(tfLocal, out var val))
                    currentValue = val;

                var altRow = new VisualElement();
                altRow.style.flexDirection = FlexDirection.Row;
                altRow.style.alignItems = Align.Center;
                altRow.style.marginBottom = 2;

                var tfLabel = UIRoot.MakeMuted(tfLocal, 12);
                tfLabel.style.minWidth = 50;
                tfLabel.style.marginLeft = 0;
                tfLabel.style.marginRight = 8;
                altRow.Add(tfLabel);

                var altField = new TextField { value = currentValue };
                altField.style.flexGrow = 1;
                UIRoot.StyleTextField(altField, "");
                altField.RegisterValueChangedCallback(e =>
                {
                    string v = e.newValue ?? "";
                    if (string.IsNullOrEmpty(v))
                        s.AltByTimeFrame.Remove(tfLocal);
                    else
                        s.AltByTimeFrame[tfLocal] = v;
                });
                altRow.Add(altField);

                altBlock.Add(altRow);
            }

            return card;
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddGroup()
        {
            var newGroup = new GroupDef
            {
                Id = JsonSaveSystem.NewId("sgroup"),
                Name = $"Новая группа {_chr.SkillGroups.Count + 1}"
            };
            _chr.SkillGroups.Add(newGroup);
            Debug.Log($"[SkillsTab] Добавлена группа навыков: {newGroup.Id}");
            Rebuild();
        }

        private void DeleteGroup(GroupDef g)
        {
            int used = _chr.Skills.Count(s => s.GroupId == g.Id);
            if (used > 0)
            {
                Debug.LogWarning($"[SkillsTab] Нельзя удалить группу «{g.Name}»: в ней {used} навык(ов).");
                return;
            }
            _chr.SkillGroups.Remove(g);
            Debug.LogWarning($"[SkillsTab] Удалена группа: {g.Name}");
            Rebuild();
        }

        private void AddSkill(string groupId)
        {
            var newSkill = new SkillDef
            {
                Id = JsonSaveSystem.NewId("skill"),
                GroupId = groupId,
                Name = "Новый навык",
                AltByTimeFrame = new Dictionary<string, string>()
            };
            _chr.Skills.Add(newSkill);
            Debug.Log($"[SkillsTab] Добавлен навык: {newSkill.Id} в группе {groupId}");
            Rebuild();
        }

        private void DeleteSkill(SkillDef s)
        {
            _chr.Skills.Remove(s);
            Debug.LogWarning($"[SkillsTab] Удалён навык: {s.Name} ({s.Id})");
            Rebuild();
        }
    }
}