using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    /// <summary>
    /// Специальные значения MeritModifier.Target, которые не привязаны к конкретной дисциплине.
    /// </summary>
    public static class MeritSpecialTargets
    {
        public const string FreeClanDiscipline = "disc.clan.free";
        public const string FreeRegularDiscipline = "disc.regular.free";
    }

    [Serializable]
    public class MeritModifier
    {
        public string Target;
        public int Value;
    }

    [Serializable]
    public class Merit
    {
        public string Id;
        public string Name;
        public string Description;

        /// <summary>Стоимость в очках черт. Может быть отрицательной (даёт очки).</summary>
        public int Cost = 1;

        /// <summary>
        /// Если true — игрок может взять эту черту несколько раз (если хватает очков).
        /// Если false — только один раз.
        /// </summary>
        public bool CanBeLearnedMultipleTimes = false;

        public List<MeritModifier> Modifiers = new List<MeritModifier>();
    }
}