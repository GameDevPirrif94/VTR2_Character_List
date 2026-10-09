using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    /// <summary>
    /// Стоимость лечения для одной густоты крови.
    /// Значения — сколько единиц крови уходит на 1 уровень урона соответствующего типа.
    /// Может быть дробным: 0.5 = за 1 кровь лечится 2 уровня.
    /// </summary>
    [Serializable]
    public class BloodPotencyHealCost
    {
        public float Bashing = 1f;      // ударный
        public float Lethal = 1f;       // летальный
        public float Aggravated = 5f;   // агрегированный
    }

    [Serializable]
    public class Chronicle
    {
        public string Version = "1.0";

        public string Id;
        public string Name;
        public string Description;

        public List<string> TimeFrames = new List<string> { "DA", "XX", "MN" };
        public string SelectedTimeFrame = "MN";

        public int MaxPlayers = 5;

        public List<string> Natures = new List<string>();
        public List<string> Masks = new List<string>();

        public List<GroupDef> AttributeGroups = new List<GroupDef>();
        public List<AttributeDef> Attributes = new List<AttributeDef>();

        public List<GroupDef> SkillGroups = new List<GroupDef>();
        public List<SkillDef> Skills = new List<SkillDef>();

        public List<Rank> Ranks = new List<Rank>();

        public Dictionary<int, int> BloodPotencyBloodMax = new Dictionary<int, int>();
        public Dictionary<int, int> BloodPotencyMaxRating = new Dictionary<int, int>();
        public Dictionary<int, BloodPotencyHealCost> BloodPotencyHealCosts = new Dictionary<int, BloodPotencyHealCost>();

        public List<Discipline> Disciplines = new List<Discipline>();
        public List<Clan> Clans = new List<Clan>();
        public List<Bloodline> Bloodlines = new List<Bloodline>();
        public List<Merit> Merits = new List<Merit>();
        public List<Morality> Moralites = new List<Morality>();
        public List<Addon> Addons = new List<Addon>();

        public XpSystem XpSystem = new XpSystem();
        public List<RollPreset> RollPresets = new List<RollPreset>();

        public Rank FindRank(string id) => Ranks.Find(r => r.Id == id);
        public AttributeDef FindAttribute(string id) => Attributes.Find(a => a.Id == id);
        public SkillDef FindSkill(string id) => Skills.Find(s => s.Id == id);
        public Discipline FindDiscipline(string id) => Disciplines.Find(d => d.Id == id);
        public Clan FindClan(string id) => Clans.Find(c => c.Id == id);
        public Bloodline FindBloodline(string id) => Bloodlines.Find(b => b.Id == id);
        public Merit FindMerit(string id) => Merits.Find(m => m.Id == id);
        public Morality FindMorality(string id) => Moralites.Find(m => m.Id == id);
        public Addon FindAddon(string id) => Addons.Find(a => a.Id == id);

        public string GetSkillName(SkillDef skill)
        {
            if (skill == null) return "";
            if (skill.AltByTimeFrame != null &&
                skill.AltByTimeFrame.TryGetValue(SelectedTimeFrame, out var alt))
                return alt;
            return skill.Name;
        }

        public string GetParamName(string paramId)
        {
            if (string.IsNullOrEmpty(paramId)) return "";
            var parts = paramId.Split('.');
            if (parts.Length != 2) return paramId;
            switch (parts[0])
            {
                case "attr": { var a = FindAttribute(parts[1]); return a != null ? a.Name : parts[1]; }
                case "skill": { var s = FindSkill(parts[1]); return s != null ? GetSkillName(s) : parts[1]; }
                case "disc": { var d = FindDiscipline(parts[1]); return d != null ? d.Name : parts[1]; }
            }
            return paramId;
        }
    }
}