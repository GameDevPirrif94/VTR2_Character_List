using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    [Serializable]
    public class PointSplit
    {
        public int Primary;
        public int Secondary;
        public int Tertiary;

        public PointSplit() { }
        public PointSplit(int p, int s, int t) { Primary = p; Secondary = s; Tertiary = t; }
    }

    [Serializable]
    public class FirstCharBonus
    {
        public int Xp;
        public List<string> MeritIds = new List<string>();
        public int ExtraAttributePoints;
        public int ExtraSkillPoints;
        public int ExtraDisciplinePoints;
    }

    [Serializable]
    public class Rank
    {
        public string Id;
        public string Name;

        /// <summary>Максимум игроков, единовременно находящихся в этом ранге. 0 = безлимит.</summary>
        public int MaxPlayers = 0;

        // Атрибуты
        public PointSplit AttributePoints = new PointSplit(5, 4, 3);
        public int AttributeMax = 5;

        // Навыки
        public PointSplit SkillPoints = new PointSplit(11, 7, 4);
        public int SkillMax = 3;
        public int SpecializationPoints = 3;

        // Дисциплины / черты / дополнения
        public int DisciplinePoints = 3;
        public int MeritPoints = 10;
        public int AddonPoints = 5;

        // Стартовые значения
        public int StartBloodPotency = 1;
        public int StartHumanity = 7;

        public FirstCharBonus FirstCharBonus = new FirstCharBonus();
    }
}