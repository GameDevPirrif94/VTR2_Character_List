using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    [Serializable]
    public class RollPreset
    {
        public string Id;
        public string Name;

        /// <summary>Параметры, участвующие в броске (attr.*, skill.*, disc.*).</summary>
        public List<string> Params = new List<string>();

        /// <summary>
        /// Модификатор, сложность и целевое число успехов не задаются в предустановке —
        /// они определяются мастером/игроком в момент броска.
        /// Поля оставлены для совместимости со старыми сейвами.
        /// </summary>
        public int Modifier = 0;
        public int Difficulty = 0;
        public RollType Type = RollType.Regular;

        public bool Extended = false;
        public int TargetSuccesses = 0;
    }
}