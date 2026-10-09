using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    /// <summary>
    /// Таблица значений ауры.
    /// Ключ: рейтинг морали (0..10). Значение: три числа (weak, regular, strong).
    /// </summary>
    [Serializable]
    public class AuraTable
    {
        public List<AuraRow> Rows = new List<AuraRow>();

        public int GetAura(int moralRating, AuraType type)
        {
            foreach (var r in Rows)
            {
                if (r.MoralityRating == moralRating)
                {
                    switch (type)
                    {
                        case AuraType.Weak: return r.Weak;
                        case AuraType.Regular: return r.Regular;
                        case AuraType.Strong: return r.Strong;
                    }
                }
            }
            return 0;
        }
    }

    [Serializable]
    public class AuraRow
    {
        public int MoralityRating;
        public int Weak;
        public int Regular;
        public int Strong;
    }
}