using System;

namespace VTR.Core.Models
{
    [Serializable]
    public class Addon
    {
        public string Id;
        public string Name;

        /// <summary>
        /// Если true — дополнение показывается игрокам при создании персонажа.
        /// Если false — остаётся только в хронике для справки.
        /// </summary>
        public bool AvailableFromStart = true;
    }
}