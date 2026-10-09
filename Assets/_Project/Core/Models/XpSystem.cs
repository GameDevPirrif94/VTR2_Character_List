using System;

namespace VTR.Core.Models
{
    [Serializable]
    public class XpCosts
    {
        public int Attribute = 4;
        public int FavoredAttribute = 3;
        public int Skill = 2;
        public int FavoredSkill = 1;

        /// <summary>
        /// Стоимость повышения навыка с 0 до 1.
        /// Используется в режиме «Текущее значение × стоимость», где 0 × cost = 0.
        /// 0 = старое поведение (бесплатно).
        /// </summary>
        public int SkillFromZero = 2;

        public int Discipline = 5;
        public int ClanDiscipline = 4;
        public int Addon = 3;
        public int Willpower = 1;
    }

    [Serializable]
    public class XpSystem
    {
        public XpMode Mode = XpMode.CurrentValues;
        public XpCosts Costs = new XpCosts();
    }
}