using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    [Serializable]
    public class AmalgamRequirement
    {
        public string DisciplineId;
        public int MinLevel = 1;
    }

    [Serializable]
    public class DisciplineAbility
    {
        public int Level;              // 1..10
        public string Name;
        public string Description;
        public AbilityType Type = AbilityType.Active;

        /// <summary>
        /// Параметры, участвующие в броске.
        /// Формат id: "attr.int", "skill.occult", "disc.thaumaturgy".
        /// Пусто = у способности нет броска.
        /// </summary>
        public List<string> RollParams = new List<string>();

        /// <summary>
        /// true  — способность выдаётся автоматически при изучении уровня.
        /// false — способность нужно докупать за опыт, если на уровне несколько способностей.
        /// Если на уровне только одна способность — AutoLearn игнорируется.
        /// </summary>
        public bool AutoLearn = true;
    }

    [Serializable]
    public class Discipline
    {
        public string Id;
        public string Name;
        public string Description;

        public bool IsPath = false;
        public string ParentDisciplineId = "";  // если IsPath == true

        public bool IsAmalgam = false;
        public List<AmalgamRequirement> AmalgamRequirements = new List<AmalgamRequirement>();

        public List<DisciplineAbility> Abilities = new List<DisciplineAbility>();
    }
}