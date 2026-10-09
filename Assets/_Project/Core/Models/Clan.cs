using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    [Serializable]
    public class Clan
    {
        public string Id;
        public string Name;
        public string Description;
        public string Curse;

        /// <summary>Ровно 3 id дисциплин, помеченных как клановые.</summary>
        public List<string> ClanDisciplineIds = new List<string>();

        /// <summary>2 id любимых атрибутов.</summary>
        public List<string> FavoredAttributeIds = new List<string>();

        /// <summary>4 id любимых навыков.</summary>
        public List<string> FavoredSkillIds = new List<string>();
    }
}