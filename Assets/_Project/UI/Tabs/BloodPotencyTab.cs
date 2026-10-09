using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI.Tabs
{
    public class BloodPotencyTab : ITab
    {
        public string Name => "Густота крови";

        private Chronicle _chr;
        private VisualElement _root;

        private enum DamageKind { Bashing, Lethal, Aggravated }

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (_chr.BloodPotencyBloodMax == null)
                _chr.BloodPotencyBloodMax = new Dictionary<int, int>();
            if (_chr.BloodPotencyMaxRating == null)
                _chr.BloodPotencyMaxRating = new Dictionary<int, int>();
            if (_chr.BloodPotencyHealCosts == null)
                _chr.BloodPotencyHealCosts = new Dictionary<int, BloodPotencyHealCost>();

            for (int bp = 1; bp <= 10; bp++)
            {
                if (!_chr.BloodPotencyBloodMax.ContainsKey(bp))
                    _chr.BloodPotencyBloodMax[bp] = DefaultBloodMax(bp);
                if (!_chr.BloodPotencyMaxRating.ContainsKey(bp))
                    _chr.BloodPotencyMaxRating[bp] = DefaultMaxRating(bp);
                if (!_chr.BloodPotencyHealCosts.ContainsKey(bp) || _chr.BloodPotencyHealCosts[bp] == null)
                    _chr.BloodPotencyHealCosts[bp] = new BloodPotencyHealCost();
            }

            Rebuild();
        }

        private static int DefaultBloodMax(int bp)
        {
            switch (bp)
            {
                case 1: return 10;
                case 2: return 11;
                case 3: return 12;
                case 4: return 13;
                case 5: return 15;
                case 6: return 20;
                case 7: return 30;
                case 8: return 40;
                case 9: return 50;
                case 10: return 60;
            }
            return 10;
        }

        private static int DefaultMaxRating(int bp)
        {
            switch (bp)
            {
                case 1: return 5;
                case 2: return 5;
                case 3: return 5;
                case 4: return 6;
                case 5: return 6;
                case 6: return 7;
                case 7: return 7;
                case 8: return 8;
                case 9: return 9;
                case 10: return 10;
            }
            return 5;
        }

        private void Rebuild()
        {
            _root.Clear();

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            _root.Add(scroll);

            scroll.Add(UIRoot.MakeHeader("Настройки густоты крови"));
            scroll.Add(UIRoot.MakeMuted(
                "Густота крови бывает от 1 до 10. Для каждого уровня задаётся: максимальный " +
                "запас крови, максимальный рейтинг параметров (атрибуты, навыки, дисциплины) и " +
                "стоимость лечения (сколько крови тратится на 1 уровень урона)."));

            // ---- Секция 1: максимальный запас крови ----
            var s1 = UIWidgets.Section("Максимальный запас крови по густоте");
            s1.Add(UIRoot.MakeMuted("Сколько крови персонаж может хранить при данной густоте."));
            var grid1 = new VisualElement();
            grid1.style.flexDirection = FlexDirection.Row;
            grid1.style.flexWrap = Wrap.Wrap;
            s1.Add(grid1);
            for (int bp = 1; bp <= 10; bp++)
            {
                int bpLocal = bp;
                grid1.Add(MakeIntBlock(
                    $"Густота {bpLocal}",
                    _chr.BloodPotencyBloodMax[bpLocal],
                    v => _chr.BloodPotencyBloodMax[bpLocal] = v,
                    0, 100));
            }
            scroll.Add(s1);

            // ---- Секция 2: максимальный рейтинг ----
            var s2 = UIWidgets.Section("Максимальный рейтинг параметров по густоте");
            s2.Add(UIRoot.MakeMuted(
                "Максимум, до которого можно поднять атрибут, навык или дисциплину."));
            var grid2 = new VisualElement();
            grid2.style.flexDirection = FlexDirection.Row;
            grid2.style.flexWrap = Wrap.Wrap;
            s2.Add(grid2);
            for (int bp = 1; bp <= 10; bp++)
            {
                int bpLocal = bp;
                grid2.Add(MakeIntBlock(
                    $"Густота {bpLocal}",
                    _chr.BloodPotencyMaxRating[bpLocal],
                    v => _chr.BloodPotencyMaxRating[bpLocal] = v,
                    0, 20));
            }
            scroll.Add(s2);

            // ---- Секция 3: стоимость лечения (три подсекции) ----
            var s3 = UIWidgets.Section("Стоимость лечения по густоте");
            s3.Add(UIRoot.MakeMuted(
                "Сколько крови тратится на 1 уровень урона указанного типа. " +
                "Можно задавать дробные значения. Примеры: 1 = 1 кровь за 1 HP; " +
                "0.5 = 1 кровь за 2 HP; 5 = 5 крови за 1 HP."));

            BuildHealCostGrid(s3, "Ударный (/)", DamageKind.Bashing);
            BuildHealCostGrid(s3, "Летальный (X)", DamageKind.Lethal);
            BuildHealCostGrid(s3, "Агрегированный (✱)", DamageKind.Aggravated);
            scroll.Add(s3);

            // ---- Сводка ----
            var s4 = UIWidgets.Section("Сводка");
            for (int bp = 1; bp <= 10; bp++)
            {
                var cfg = _chr.BloodPotencyHealCosts[bp];
                s4.Add(UIRoot.MakeMuted(
                    $"Густота {bp}: кровь до {_chr.BloodPotencyBloodMax[bp]}, " +
                    $"рейтинг до {_chr.BloodPotencyMaxRating[bp]}, " +
                    $"стоимость / {cfg.Bashing}, X {cfg.Lethal}, ✱ {cfg.Aggravated}"));
            }
            scroll.Add(s4);
        }

        private void BuildHealCostGrid(VisualElement parent, string title, DamageKind kind)
        {
            var header = new Label(title);
            header.style.fontSize = 14;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = UIRoot.TextPrimary;
            header.style.marginTop = 10;
            header.style.marginBottom = 6;
            parent.Add(header);

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            parent.Add(grid);

            for (int bp = 1; bp <= 10; bp++)
            {
                int bpLocal = bp;
                var cfg = _chr.BloodPotencyHealCosts[bpLocal];

                float current;
                switch (kind)
                {
                    case DamageKind.Bashing: current = cfg.Bashing; break;
                    case DamageKind.Lethal: current = cfg.Lethal; break;
                    default: current = cfg.Aggravated; break;
                }

                grid.Add(MakeFloatBlock(
                    $"Густота {bpLocal}",
                    current,
                    v =>
                    {
                        if (v < 0f) v = 0f;
                        switch (kind)
                        {
                            case DamageKind.Bashing: cfg.Bashing = v; break;
                            case DamageKind.Lethal: cfg.Lethal = v; break;
                            default: cfg.Aggravated = v; break;
                        }
                    }));
            }
        }

        // ============================================================
        // Блоки полей
        // ============================================================

        private VisualElement MakeIntBlock(string title, int value,
            System.Action<int> onChange, int min, int max)
        {
            var block = new VisualElement();
            block.style.flexDirection = FlexDirection.Column;
            block.style.marginRight = 10;
            block.style.marginBottom = 10;
            block.style.width = 130;
            block.style.minWidth = 130;
            block.style.flexGrow = 0;
            block.style.flexShrink = 0;

            var lbl = new Label(title);
            lbl.style.fontSize = 12;
            lbl.style.color = UIRoot.TextMuted;
            lbl.style.marginBottom = 4;
            lbl.style.marginLeft = 0;
            lbl.style.marginRight = 0;
            lbl.style.marginTop = 0;
            block.Add(lbl);

            var field = new IntegerField();
            field.value = value;
            field.style.width = 120;
            field.style.height = 42;
            field.style.marginBottom = 0;
            field.style.marginTop = 0;
            field.style.marginLeft = 0;
            field.style.marginRight = 0;

            var labelEl = field.labelElement;
            if (labelEl != null)
                labelEl.style.display = DisplayStyle.None;

            StyleFieldInput(field);

            field.RegisterValueChangedCallback(e =>
            {
                int v = e.newValue;
                if (v < min) { v = min; field.SetValueWithoutNotify(v); }
                if (v > max) { v = max; field.SetValueWithoutNotify(v); }
                onChange?.Invoke(v);
            });

            block.Add(field);
            return block;
        }

        private VisualElement MakeFloatBlock(string title, float value,
            System.Action<float> onChange)
        {
            var block = new VisualElement();
            block.style.flexDirection = FlexDirection.Column;
            block.style.marginRight = 10;
            block.style.marginBottom = 10;
            block.style.width = 130;
            block.style.minWidth = 130;
            block.style.flexGrow = 0;
            block.style.flexShrink = 0;

            var lbl = new Label(title);
            lbl.style.fontSize = 12;
            lbl.style.color = UIRoot.TextMuted;
            lbl.style.marginBottom = 4;
            lbl.style.marginLeft = 0;
            lbl.style.marginRight = 0;
            lbl.style.marginTop = 0;
            block.Add(lbl);

            var field = new FloatField();
            field.value = value;
            field.style.width = 120;
            field.style.height = 42;
            field.style.marginBottom = 0;
            field.style.marginTop = 0;
            field.style.marginLeft = 0;
            field.style.marginRight = 0;

            var labelEl = field.labelElement;
            if (labelEl != null)
                labelEl.style.display = DisplayStyle.None;

            StyleFieldInput(field);

            field.RegisterValueChangedCallback(e =>
            {
                float v = e.newValue;
                if (v < 0f) { v = 0f; field.SetValueWithoutNotify(v); }
                onChange?.Invoke(v);
            });

            block.Add(field);
            return block;
        }

        private void StyleFieldInput(VisualElement field)
        {
            var input = field.Q<VisualElement>(className: "unity-base-text-field__input")
                     ?? field.Q<VisualElement>(className: "unity-text-field__input")
                     ?? field.Q<VisualElement>(className: "unity-base-field__input");
            if (input != null)
            {
                input.style.backgroundColor = UIRoot.BgInput;
                input.style.minHeight = 42;
                input.style.height = 42;
                input.style.paddingLeft = 10;
                input.style.paddingRight = 10;
                input.style.paddingTop = 0;
                input.style.paddingBottom = 0;
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
                input.style.flexGrow = 1;
                input.style.justifyContent = Justify.Center;
                input.style.alignItems = Align.Center;
            }

            field.Query<TextElement>().ForEach(t =>
            {
                t.style.color = UIRoot.TextPrimary;
                t.style.fontSize = 14;
                t.style.unityTextAlign = TextAnchor.MiddleLeft;
                t.style.unityFontStyleAndWeight = FontStyle.Bold;
            });
        }
    }
}