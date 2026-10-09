using VTR.Core.Models;

namespace VTR.Core.Rules
{
    public static class AuraCalculator
    {
        /// <summary>
        /// Вычисляет значение ауры по рейтингу морали и типу ауры (слабая/обычная/сильная).
        /// </summary>
        public static int Calculate(AuraTable table, int moralityRating, AuraType type)
        {
            if (table == null) return 0;
            if (moralityRating < 0) moralityRating = 0;
            if (moralityRating > 10) moralityRating = 10;
            return table.GetAura(moralityRating, type);
        }

        /// <summary>
        /// Определяет тип ауры по наличию черт "Слабая аура" / "Сильная аура".
        /// </summary>
        public static AuraType DetermineAuraType(bool hasWeakAura, bool hasStrongAura)
        {
            if (hasWeakAura) return AuraType.Weak;
            if (hasStrongAura) return AuraType.Strong;
            return AuraType.Regular;
        }

        /// <summary>Сила воли = Самоконтроль (Composure) + Решительность (Resolve), максимум 10.</summary>
        public static int CalculateMaxWillpower(int composure, int resolve)
        {
            int total = composure + resolve;
            return total > 10 ? 10 : total;
        }

        /// <summary>Максимальный запас крови по густоте крови.</summary>
        public static int CalculateMaxBlood(Chronicle chronicle, int bloodPotency)
        {
            if (chronicle?.BloodPotencyBloodMax != null &&
                chronicle.BloodPotencyBloodMax.TryGetValue(bloodPotency, out var max))
                return max;
            return 10;
        }
    }
}