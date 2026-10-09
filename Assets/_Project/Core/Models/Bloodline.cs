using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    [Serializable]
    public class BloodlineBonus
    {
        public BloodlineBonusType Type;
        public string ReferenceId;  // id дисциплины / амальгамы / черты
    }

    [Serializable]
    public class Bloodline
    {
        public string Id;
        public string Name;
        public string Description;
        public string ClanId;
        public string AdditionalCurse;

        /// <summary>Основной бонус бладлайна.</summary>
        public BloodlineBonus PrimaryBonus = new BloodlineBonus();

        /// <summary>До 4 дополнительных бонусов.</summary>
        public List<BloodlineBonus> ExtraBonuses = new List<BloodlineBonus>();

        /// <summary>
        /// Требование по густоте крови вычисляется автоматически:
        ///   базовая 1 + PrimaryBonus + ExtraBonuses
        ///   Клановая дисциплина: +2
        ///   Обычная дисциплина:  +1
        ///   Амальгама:           +2
        ///   Черта:               +1
        /// </summary>
        public int GetRequiredBloodPotency()
        {
            int req = 1;
            if (ExtraBonuses != null)
                foreach (var b in ExtraBonuses)
                    req += CostOf(b);
            return req;
        }

        private static int CostOf(BloodlineBonus b)
        {
            if (b == null) return 0;
            switch (b.Type)
            {
                case BloodlineBonusType.ClanDiscipline: return 2;
                case BloodlineBonusType.RegularDiscipline: return 1;
                case BloodlineBonusType.Amalgam: return 2;
                case BloodlineBonusType.Merit: return 1;
            }
            return 0;
        }
    }
}