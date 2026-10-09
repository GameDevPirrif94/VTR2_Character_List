using System.Collections.Generic;

namespace VTR.Core.Models
{
    public class CharacterDraft
    {
        public string Name = "";
        public string ClanId = "";
        public string BloodlineId = "";
        public string RankId = "";
        public string Nature = "";
        public string Mask = "";
        public string FavoredAttributeId = "";

        // Атрибуты
        public string PrimaryAttributeGroupId = "";
        public string SecondaryAttributeGroupId = "";
        public string TertiaryAttributeGroupId = "";
        public Dictionary<string, int> Attributes = new Dictionary<string, int>();

        // Навыки
        public string PrimarySkillGroupId = "";
        public string SecondarySkillGroupId = "";
        public string TertiarySkillGroupId = "";
        public Dictionary<string, int> Skills = new Dictionary<string, int>();
        public Dictionary<string, List<string>> Specializations = new Dictionary<string, List<string>>();

        // Черты / дополнения / мораль
        public List<string> MeritIds = new List<string>();

        /// <summary>id дополнения → количество вложенных очков (0..5).</summary>
        public Dictionary<string, int> AddonLevels = new Dictionary<string, int>();

        public string MoralityId = "";

        /// <summary>Выборы игрока для свободных слотов дисциплин, открытых чертами.</summary>
        public Dictionary<string, string> FreeDiscChoices = new Dictionary<string, string>();

        // Дисциплины
        public Dictionary<string, int> Disciplines = new Dictionary<string, int>();

        // Биография
        public string BirthDate = "";
        public string DeathDate = "";
        public int VisualAge;
        public int RealAge;
        public string Nationality = "";
        public string Gender = "male";
        public string Build = "";
        public string Height = "";
        public string Weight = "";
        public string Hair = "";
        public string Eyes = "";
        public string NotableFeatures = "";
        public string Backstory = "";
        public string Haven = "";
        public string PortraitPath = "";

        public int CurrentStep = 0;
    }
}