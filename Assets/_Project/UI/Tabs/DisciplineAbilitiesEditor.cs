using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI.Tabs
{
    /// <summary>
    /// Редактор способностей дисциплины.
    /// Не использует DropdownField — вместо него кастомный пикер через UIRoot-оверлей.
    /// </summary>
    public static class DisciplineAbilitiesEditor
    {
        private const string NoneLabel = "— (нет) —";

        public static void Build(VisualElement parent, Chronicle chr, Discipline d, Action onChanged)
        {
            var section = UIWidgets.Section("Способности");

            if (d.IsAmalgam)
            {
                section.Add(UIRoot.MakeMuted("У амальгамы ровно одна способность."));
                BuildAmalgamAbility(section, chr, d, onChanged);
            }
            else
            {
                section.Add(UIRoot.MakeMuted(
                    "Хотя бы по одной способности на уровни 1–5. Уровни 6–10 — опциональны. " +
                    "Если на уровне несколько способностей — отметь AutoLearn у тех, что выдаются автоматически."));

                for (int lvl = 1; lvl <= 10; lvl++)
                    BuildLevelBlock(section, chr, d, lvl, onChanged);
            }

            var validateBtn = new Button(() => Validate(d));
            validateBtn.text = "Проверить корректность";
            UIRoot.StyleButton(validateBtn, UIRoot.AccentNeutral, Color.white, 34);
            validateBtn.style.marginTop = 12;
            section.Add(validateBtn);

            parent.Add(section);
        }

        // ============================================================
        // Амальгама
        // ============================================================

        private static void BuildAmalgamAbility(VisualElement parent, Chronicle chr, Discipline d, Action onChanged)
        {
            if (d.Abilities.Count == 0)
                d.Abilities.Add(new DisciplineAbility { Level = 1, Name = "Способность амальгамы" });
            while (d.Abilities.Count > 1)
                d.Abilities.RemoveAt(d.Abilities.Count - 1);

            var ability = d.Abilities[0];
            parent.Add(BuildAbilityCard(chr, d, ability, 1, false, onChanged));
        }

        // ============================================================
        // Блок уровня
        // ============================================================

        private static void BuildLevelBlock(VisualElement parent, Chronicle chr, Discipline d,
            int level, Action onChanged)
        {
            var abilitiesOnLevel = d.Abilities.Where(a => a.Level == level).ToList();

            var block = new VisualElement();
            block.style.marginBottom = 10;
            block.style.marginTop = 10;
            block.style.paddingTop = 6;
            block.style.paddingBottom = 6;
            block.style.paddingLeft = 8;
            block.style.paddingRight = 8;
            block.style.backgroundColor = new Color(0.13f, 0.13f, 0.16f);
            block.style.borderTopLeftRadius = 4;
            block.style.borderTopRightRadius = 4;
            block.style.borderBottomLeftRadius = 4;
            block.style.borderBottomRightRadius = 4;

            var levelHeader = new VisualElement();
            levelHeader.style.flexDirection = FlexDirection.Row;
            levelHeader.style.alignItems = Align.Center;
            block.Add(levelHeader);

            var lvlLabel = new Label($"Уровень {level}");
            lvlLabel.style.fontSize = 15;
            lvlLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            lvlLabel.style.color = UIRoot.TextPrimary;
            lvlLabel.style.marginRight = 10;
            levelHeader.Add(lvlLabel);

            int cnt = abilitiesOnLevel.Count;
            levelHeader.Add(UIRoot.MakeMuted($"способностей: {cnt}", 11));

            if (cnt == 0 && level <= 5)
            {
                var warn = new Label("⚠ требуется минимум 1");
                warn.style.color = new Color(0.95f, 0.7f, 0.3f);
                warn.style.fontSize = 11;
                warn.style.marginLeft = 10;
                levelHeader.Add(warn);
            }

            bool multiple = cnt > 1;
            foreach (var ability in abilitiesOnLevel.ToList())
                block.Add(BuildAbilityCard(chr, d, ability, level, multiple, onChanged));

            int capturedCnt = cnt;
            var addBtn = new Button(() =>
            {
                d.Abilities.Add(new DisciplineAbility
                {
                    Level = level,
                    Name = $"Способность {level} уровня",
                    Type = AbilityType.Active,
                    AutoLearn = capturedCnt == 0,
                    RollParams = new List<string>()
                });
                Debug.Log($"[Abilities] +способность на уровне {level} для «{d.Name}»");
                onChanged?.Invoke();
            });
            addBtn.text = $"＋  Добавить способность уровня {level}";
            UIRoot.StyleButton(addBtn, UIRoot.AccentGreen, Color.white, 28);
            addBtn.style.marginTop = 4;
            block.Add(addBtn);

            parent.Add(block);
        }

        // ============================================================
        // Карточка способности
        // ============================================================

        private static VisualElement BuildAbilityCard(Chronicle chr, Discipline d, DisciplineAbility ability,
            int level, bool multipleOnLevel, Action onChanged)
        {
            var card = new VisualElement();
            card.style.marginTop = 6;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.backgroundColor = new Color(0.17f, 0.17f, 0.21f);
            card.style.borderTopLeftRadius = 4;
            card.style.borderTopRightRadius = 4;
            card.style.borderBottomLeftRadius = 4;
            card.style.borderBottomRightRadius = 4;

            // --- Строка 1: имя + удалить ---
            var row1 = new VisualElement();
            row1.style.flexDirection = FlexDirection.Row;
            row1.style.alignItems = Align.Center;
            card.Add(row1);

            var nameField = new TextField { value = ability.Name ?? "" };
            nameField.style.flexGrow = 1;
            UIRoot.StyleTextField(nameField, "");
            nameField.RegisterValueChangedCallback(e => ability.Name = e.newValue);
            row1.Add(nameField);

            var delBtn = new Button(() =>
            {
                d.Abilities.Remove(ability);
                Debug.LogWarning($"[Abilities] Удалена способность «{ability.Name}»");
                onChanged?.Invoke();
            });
            delBtn.text = "✕";
            UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 28);
            delBtn.style.width = 40;
            delBtn.style.marginLeft = 8;
            row1.Add(delBtn);

            // --- Описание ---
            var descField = new TextField { value = ability.Description ?? "", multiline = true };
            UIRoot.StyleTextField(descField, "Описание");
            descField.style.height = 60;
            descField.style.marginTop = 4;
            descField.RegisterValueChangedCallback(e => ability.Description = e.newValue);
            card.Add(descField);

            // --- Тип + AutoLearn ---
            var row2 = new VisualElement();
            row2.style.flexDirection = FlexDirection.Row;
            row2.style.alignItems = Align.Center;
            row2.style.marginTop = 4;
            card.Add(row2);

            var typeChoices = new List<string> { "Активная", "Пассивная" };
            int typeIdx = ability.Type == AbilityType.Passive ? 1 : 0;
            row2.Add(UIWidgets.MakePickerBlock(
                "Тип",
                typeChoices,
                typeIdx,
                i => ability.Type = (i == 1) ? AbilityType.Passive : AbilityType.Active,
                width: 180));

            if (multipleOnLevel)
            {
                var autoLearnToggle = new Toggle("AutoLearn") { value = ability.AutoLearn };
                UIWidgets.StyleToggle(autoLearnToggle);
                autoLearnToggle.style.marginTop = 14; // чтобы выровнять с пикером (у которого label сверху)
                autoLearnToggle.RegisterValueChangedCallback(e => ability.AutoLearn = e.newValue);
                row2.Add(autoLearnToggle);
            }

            // --- Параметры броска ---
            var rollHeader = UIRoot.MakeMuted(
                "Параметры броска (пусто = броска нет):", 11);
            rollHeader.style.marginTop = 8;
            card.Add(rollHeader);

            var rollRow = new VisualElement();
            rollRow.style.flexDirection = FlexDirection.Row;
            rollRow.style.flexWrap = Wrap.Wrap;
            rollRow.style.marginTop = 4;
            card.Add(rollRow);

            rollRow.Add(BuildAttrParamPicker(chr, ability));
            rollRow.Add(BuildSkillParamPicker(chr, ability));
            rollRow.Add(BuildDiscParamPicker(chr, ability));

            return card;
        }

        // ============================================================
        // Пикеры параметров
        // ============================================================

        private static VisualElement BuildAttrParamPicker(Chronicle chr, DisciplineAbility ability)
        {
            var attrs = chr.Attributes;
            var names = new List<string> { NoneLabel };
            names.AddRange(attrs.Select(a => a.Name));

            string current = GetParam(ability.RollParams, "attr");
            int idx = 0;
            if (!string.IsNullOrEmpty(current))
            {
                int found = attrs.FindIndex(a => a.Id == current);
                if (found >= 0) idx = found + 1;
            }

            return UIWidgets.MakePickerBlock("Атрибут", names, idx, i =>
            {
                if (i <= 0) SetParam(ability.RollParams, "attr", null);
                else SetParam(ability.RollParams, "attr", attrs[i - 1].Id);
            });
        }

        private static VisualElement BuildSkillParamPicker(Chronicle chr, DisciplineAbility ability)
        {
            var skills = chr.Skills;
            var names = new List<string> { NoneLabel };
            names.AddRange(skills.Select(s => chr.GetSkillName(s)));

            string current = GetParam(ability.RollParams, "skill");
            int idx = 0;
            if (!string.IsNullOrEmpty(current))
            {
                int found = skills.FindIndex(s => s.Id == current);
                if (found >= 0) idx = found + 1;
            }

            return UIWidgets.MakePickerBlock("Навык", names, idx, i =>
            {
                if (i <= 0) SetParam(ability.RollParams, "skill", null);
                else SetParam(ability.RollParams, "skill", skills[i - 1].Id);
            });
        }

        private static VisualElement BuildDiscParamPicker(Chronicle chr, DisciplineAbility ability)
        {
            var dis = chr.Disciplines;
            var names = new List<string> { NoneLabel };
            names.AddRange(dis.Select(x => x.Name));

            string current = GetParam(ability.RollParams, "disc");
            int idx = 0;
            if (!string.IsNullOrEmpty(current))
            {
                int found = dis.FindIndex(x => x.Id == current);
                if (found >= 0) idx = found + 1;
            }

            return UIWidgets.MakePickerBlock("Дисциплина", names, idx, i =>
            {
                if (i <= 0) SetParam(ability.RollParams, "disc", null);
                else SetParam(ability.RollParams, "disc", dis[i - 1].Id);
            });
        }

        // ============================================================
        // Утилиты RollParams
        // ============================================================

        private static string GetParam(List<string> rollParams, string prefix)
        {
            if (rollParams == null) return null;
            return rollParams.FirstOrDefault(p => p.StartsWith(prefix + "."));
        }

        private static void SetParam(List<string> rollParams, string prefix, string newId)
        {
            if (rollParams == null) return;
            rollParams.RemoveAll(p => p.StartsWith(prefix + "."));
            if (!string.IsNullOrEmpty(newId))
                rollParams.Add(prefix + "." + newId);
        }

        // ============================================================
        // Валидация
        // ============================================================

        public static void Validate(Discipline d)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(d.Name))
                problems.Add("Название дисциплины пустое.");

            if (d.IsPath && string.IsNullOrEmpty(d.ParentDisciplineId))
                problems.Add("Путь не имеет родительской дисциплины.");

            if (d.IsAmalgam)
            {
                if (d.Abilities.Count != 1)
                    problems.Add($"Амальгама должна иметь ровно одну способность (сейчас {d.Abilities.Count}).");
                if (d.AmalgamRequirements.Count == 0)
                    problems.Add("Амальгама не имеет требований.");
            }
            else
            {
                for (int lvl = 1; lvl <= 5; lvl++)
                {
                    int cnt = d.Abilities.Count(a => a.Level == lvl);
                    if (cnt == 0) problems.Add($"Нет способностей на уровне {lvl}.");
                }

                foreach (var a in d.Abilities)
                    if (string.IsNullOrWhiteSpace(a.Name))
                        problems.Add($"Способность уровня {a.Level} без имени.");
            }

            if (problems.Count == 0)
                Debug.Log($"<color=#8BC34A><b>[Валидация] «{d.Name}» — ОК.</b></color>");
            else
                Debug.LogWarning($"[Валидация] «{d.Name}» — найдено проблем: {problems.Count}\n" +
                                 string.Join("\n", problems.Select(p => " • " + p)));
        }
    }
}