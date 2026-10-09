using UnityEngine;
using VTR.Core.Models;

namespace VTR.Core.Rules
{
    public static class XpCalculator
    {
        /// <summary>
        /// Стоимость прокачки параметра. Работает ТОЛЬКО при трате опыта.
        /// Стартовые очки при создании персонажа тратятся 1:1 и этот метод не используют.
        /// </summary>
        public static int Cost(XpMode mode, int currentValue, int costPerUnit)
        {
            switch (mode)
            {
                case XpMode.Flat: return costPerUnit;
                case XpMode.CurrentValues: return currentValue * costPerUnit;
                case XpMode.NextValues: return (currentValue + 1) * costPerUnit;
            }
            return costPerUnit;
        }

        public static int AttributeCost(XpSystem sys, int current, bool favored)
        {
            int c = favored ? sys.Costs.FavoredAttribute : sys.Costs.Attribute;
            return Cost(sys.Mode, current, c);
        }

        /// <summary>
        /// Стоимость повышения навыка.
        /// В режиме CurrentValues при current == 0 использует SkillFromZero,
        /// потому что «0 × cost = 0» не имеет смысла.
        /// </summary>
        public static int SkillCost(XpSystem sys, int current, bool favored)
        {
            if (sys.Mode == XpMode.CurrentValues && current == 0)
                return sys.Costs.SkillFromZero;

            int c = favored ? sys.Costs.FavoredSkill : sys.Costs.Skill;
            return Cost(sys.Mode, current, c);
        }

        public static int DisciplineCost(XpSystem sys, int current, bool clanDiscipline)
        {
            int c = clanDiscipline ? sys.Costs.ClanDiscipline : sys.Costs.Discipline;
            return Cost(sys.Mode, current, c);
        }

        public static int AddonCost(XpSystem sys, int current)
            => Cost(sys.Mode, current, sys.Costs.Addon);

        public static int WillpowerCost(XpSystem sys, int current)
            => Cost(sys.Mode, current, sys.Costs.Willpower);
    }
}