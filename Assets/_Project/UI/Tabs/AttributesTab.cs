using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class AttributesTab : ITab
    {
        public string Name => "Атрибуты";

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

            scroll.Add(UIRoot.MakeHeader("Атрибуты"));
            scroll.Add(UIRoot.MakeMuted(
                "Группы атрибутов и сами атрибуты. По умолчанию: Ментальные, Физические, Социальные."));

            BuildGroupsSection();
            BuildAttributesSection();
        }

        // ============================================================
        // Секция 1: группы
        // ============================================================

        private void BuildGroupsSection()
        {
            var section = UIWidgets.Section("Группы атрибутов");

            if (_chr.AttributeGroups.Count == 0)
                section.Add(UIRoot.MakeMuted("(нет групп)"));

            foreach (var group in _chr.AttributeGroups.ToList())
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

                int used = _chr.Attributes.Count(a => a.GroupId == g.Id);
                var info = UIRoot.MakeMuted($"атрибутов: {used}", 11);
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
            var header = _scrollContent.Q<Label>($"group-header-{groupId}");
            if (header != null)
            {
                var g = _chr.AttributeGroups.Find(x => x.Id == groupId);
                if (g != null) header.text = g.Name;
            }
        }

        // ============================================================
        // Секция 2: атрибуты
        // ============================================================

        private void BuildAttributesSection()
        {
            var section = UIWidgets.Section("Атрибуты");

            if (_chr.AttributeGroups.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("Сначала добавьте хотя бы одну группу."));
                _scrollContent.Add(section);
                return;
            }

            foreach (var group in _chr.AttributeGroups)
            {
                var g = group;

                var groupBlock = new VisualElement();
                groupBlock.style.marginBottom = 16;

                var header = new Label(g.Name);
                header.name = $"group-header-{g.Id}";
                header.style.fontSize = 15;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = UIRoot.TextPrimary;
                header.style.marginBottom = 6;
                groupBlock.Add(header);

                var attrsInGroup = _chr.Attributes.Where(a => a.GroupId == g.Id).ToList();
                if (attrsInGroup.Count == 0)
                {
                    var empty = UIRoot.MakeMuted("(нет атрибутов)");
                    empty.style.marginLeft = 12;
                    groupBlock.Add(empty);
                }

                foreach (var attr in attrsInGroup)
                {
                    var a = attr;
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.Center;
                    row.style.marginBottom = 4;
                    row.style.marginLeft = 12;

                    var nameField = new TextField { value = a.Name };
                    nameField.style.flexGrow = 1;
                    UIRoot.StyleTextField(nameField, "");
                    nameField.RegisterValueChangedCallback(e => a.Name = e.newValue);
                    row.Add(nameField);

                    var groupChoices = _chr.AttributeGroups.Select(x => x.Name).ToList();
                    int currentIdx = _chr.AttributeGroups.FindIndex(x => x.Id == a.GroupId);
                    if (currentIdx < 0) currentIdx = 0;

                    var groupDrop = new DropdownField(groupChoices, currentIdx);
                    groupDrop.style.width = 180;
                    UIWidgets.StyleDropdown(groupDrop);

                    var attrLocal = a;
                    groupDrop.RegisterValueChangedCallback(e =>
                    {
                        int newIdx = groupChoices.IndexOf(e.newValue);
                        if (newIdx >= 0 && newIdx < _chr.AttributeGroups.Count)
                        {
                            attrLocal.GroupId = _chr.AttributeGroups[newIdx].Id;
                            Rebuild();
                        }
                    });
                    row.Add(groupDrop);

                    var delBtn = new Button(() => DeleteAttribute(a));
                    delBtn.text = "✕";
                    UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 28);
                    delBtn.style.width = 40;
                    delBtn.style.marginLeft = 8;
                    row.Add(delBtn);

                    groupBlock.Add(row);
                }

                var addAttrBtn = new Button(() => AddAttribute(g.Id));
                addAttrBtn.text = $"＋  Добавить атрибут в «{g.Name}»";
                UIRoot.StyleButton(addAttrBtn, UIRoot.AccentNeutral, Color.white, 30);
                addAttrBtn.style.marginLeft = 12;
                addAttrBtn.style.marginTop = 6;
                groupBlock.Add(addAttrBtn);

                section.Add(groupBlock);
            }

            _scrollContent.Add(section);
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddGroup()
        {
            var newGroup = new GroupDef
            {
                Id = JsonSaveSystem.NewId("agroup"),
                Name = $"Новая группа {_chr.AttributeGroups.Count + 1}"
            };
            _chr.AttributeGroups.Add(newGroup);
            Debug.Log($"[AttributesTab] Добавлена группа: {newGroup.Id}");
            Rebuild();
        }

        private void DeleteGroup(GroupDef g)
        {
            int used = _chr.Attributes.Count(a => a.GroupId == g.Id);
            if (used > 0)
            {
                Debug.LogWarning($"[AttributesTab] Нельзя удалить группу «{g.Name}»: в ней {used} атрибут(ов).");
                return;
            }
            _chr.AttributeGroups.Remove(g);
            Debug.LogWarning($"[AttributesTab] Удалена группа: {g.Name}");
            Rebuild();
        }

        private void AddAttribute(string groupId)
        {
            var newAttr = new AttributeDef
            {
                Id = JsonSaveSystem.NewId("attr"),
                GroupId = groupId,
                Name = "Новый атрибут"
            };
            _chr.Attributes.Add(newAttr);
            Debug.Log($"[AttributesTab] Добавлен атрибут: {newAttr.Id} в группе {groupId}");
            Rebuild();
        }

        private void DeleteAttribute(AttributeDef a)
        {
            _chr.Attributes.Remove(a);
            Debug.LogWarning($"[AttributesTab] Удалён атрибут: {a.Name} ({a.Id})");
            Rebuild();
        }
    }
}