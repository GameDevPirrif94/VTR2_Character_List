using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI.Tabs
{
    public class XpTab : ITab
    {
        public string Name => "Опыт";

        private Chronicle _chr;
        private VisualElement _root;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (_chr.XpSystem == null)
                _chr.XpSystem = new XpSystem();
            if (_chr.XpSystem.Costs == null)
                _chr.XpSystem.Costs = new XpCosts();

            Rebuild();
        }

        private void Rebuild()
        {
            _root.Clear();

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            _root.Add(scroll);

            scroll.Add(UIRoot.MakeHeader("Система опыта"));
            scroll.Add(UIRoot.MakeMuted(
                "Система распределения опыта. ВАЖНО: эти формулы работают ТОЛЬКО при трате опыта. " +
                "Стартовые очки при создании персонажа тратятся 1:1 и не подчиняются этим правилам."));

            var s1 = UIWidgets.Section("Режим распределения");

            var modeChoices = new List<string>
            {
                "Флатовое значение (фиксированная стоимость)",
                "Текущее значение × стоимость",
                "Следующее значение × стоимость"
            };
            int modeIdx = (int)_chr.XpSystem.Mode;

            s1.Add(UIWidgets.MakePickerBlock(
                "Режим",
                modeChoices,
                modeIdx,
                i =>
                {
                    _chr.XpSystem.Mode = (XpMode)i;
                    Rebuild();
                },
                width: 420));

            s1.Add(UIRoot.MakeMuted(GetModeDescription(_chr.XpSystem.Mode), 12));
            scroll.Add(s1);

            var s2 = UIWidgets.Section("Стоимость за единицу");
            s2.Add(UIRoot.MakeMuted(
                "Базовая стоимость. В формулах она участвует в зависимости от выбранного режима."));

            var c = _chr.XpSystem.Costs;
            s2.Add(UIWidgets.MakeIntField("Атрибут",
                c.Attribute, v => c.Attribute = v, 0, 100));
            s2.Add(UIWidgets.MakeIntField("Любимый атрибут",
                c.FavoredAttribute, v => c.FavoredAttribute = v, 0, 100));
            s2.Add(UIWidgets.MakeIntField("Навык",
                c.Skill, v => c.Skill = v, 0, 100));
            s2.Add(UIWidgets.MakeIntField("Любимый навык",
                c.FavoredSkill, v => c.FavoredSkill = v, 0, 100));

            s2.Add(UIWidgets.MakeIntField("Навык с 0 (только для режима «текущее × цена»)",
                c.SkillFromZero, v => c.SkillFromZero = v, 0, 100));
            s2.Add(UIRoot.MakeMuted(
                "Используется, когда у навыка значение 0. В этом режиме 0 × cost = 0 — " +
                "чтобы навык не прокачивался бесплатно, применяется эта цена.", 11));

            s2.Add(UIWidgets.MakeIntField("Дисциплина",
                c.Discipline, v => c.Discipline = v, 0, 100));
            s2.Add(UIWidgets.MakeIntField("Клановая дисциплина",
                c.ClanDiscipline, v => c.ClanDiscipline = v, 0, 100));
            s2.Add(UIWidgets.MakeIntField("Дополнение",
                c.Addon, v => c.Addon = v, 0, 100));
            s2.Add(UIWidgets.MakeIntField("Сила воли",
                c.Willpower, v => c.Willpower = v, 0, 100));
            scroll.Add(s2);

            var s3 = UIWidgets.Section("Примеры");
            s3.Add(UIRoot.MakeMuted(
                "Как будет считаться повышение при текущем режиме и указанных ценах."));

            s3.Add(UIRoot.MakeMuted(
                $"Повысить обычный атрибут с 3 до 4: {CalculateExample(c.Attribute, 3)} опыта."));
            s3.Add(UIRoot.MakeMuted(
                $"Повысить любимый атрибут с 3 до 4: {CalculateExample(c.FavoredAttribute, 3)} опыта."));
            s3.Add(UIRoot.MakeMuted(
                $"Повысить навык с 0 до 1: {CalculateSkillExample(c, 0, false)} опыта."));
            s3.Add(UIRoot.MakeMuted(
                $"Повысить навык с 2 до 3: {CalculateSkillExample(c, 2, false)} опыта."));
            s3.Add(UIRoot.MakeMuted(
                $"Повысить дисциплину с 1 до 2: {CalculateExample(c.Discipline, 1)} опыта."));

            scroll.Add(s3);
        }

        private string GetModeDescription(XpMode mode)
        {
            switch (mode)
            {
                case XpMode.Flat:
                    return "Цена фиксирована: не зависит от текущего значения параметра.";
                case XpMode.CurrentValues:
                    return "Цена = текущее значение × стоимость. Пример: атрибут 3 → 4 стоит 3 × base.";
                case XpMode.NextValues:
                    return "Цена = значение после повышения × стоимость. Пример: атрибут 3 → 4 стоит 4 × base.";
            }
            return "";
        }

        private string CalculateExample(int baseCost, int currentValue)
        {
            switch (_chr.XpSystem.Mode)
            {
                case XpMode.Flat:
                    return baseCost.ToString();
                case XpMode.CurrentValues:
                    return (currentValue * baseCost).ToString();
                case XpMode.NextValues:
                    return ((currentValue + 1) * baseCost).ToString();
            }
            return "?";
        }

        private string CalculateSkillExample(XpCosts c, int currentValue, bool favored)
        {
            var sys = _chr.XpSystem;
            int cost = Core.Rules.XpCalculator.SkillCost(sys, currentValue, favored);
            return cost.ToString();
        }
    }
}