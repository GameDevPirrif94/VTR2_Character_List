using UnityEngine;
using VTR.Core.Models;

namespace VTR.Core.Rules
{
    public static class HealthSystem
    {
        // ============================================================
        // Нанесение урона
        // ============================================================

        public static void ApplyDamage(Character c, DamageType type, int amount)
        {
            if (c == null || amount <= 0) return;
            c.EnsureHealthBoxes();

            for (int n = 0; n < amount; n++)
            {
                int idx = c.FindRightmostEmpty();
                if (idx >= 0)
                {
                    c.HealthBoxes[idx] = type;
                }
                else
                {
                    if (type == DamageType.Bashing)
                    {
                        if (!UpgradeLeftmost(c, DamageType.Bashing, DamageType.Lethal))
                        {
                            if (!UpgradeLeftmost(c, DamageType.Lethal, DamageType.Aggravated))
                                break;
                        }
                    }
                    else if (type == DamageType.Lethal)
                    {
                        if (!UpgradeLeftmost(c, DamageType.Lethal, DamageType.Aggravated))
                        {
                            if (!UpgradeLeftmost(c, DamageType.Bashing, DamageType.Lethal))
                                break;
                        }
                    }
                    else // Aggravated
                    {
                        if (!UpgradeLeftmost(c, DamageType.Lethal, DamageType.Aggravated))
                            break;
                    }
                }
            }

            c.RecalculateHealthCurrent();
        }

        private static bool UpgradeLeftmost(Character c, DamageType from, DamageType to)
        {
            for (int i = 0; i < c.HealthMax && i < c.HealthBoxes.Length; i++)
            {
                if (c.HealthBoxes[i] == from)
                {
                    c.HealthBoxes[i] = to;
                    return true;
                }
            }
            return false;
        }

        // ============================================================
        // Анализ
        // ============================================================

        public static int CountType(Character c, DamageType type)
        {
            if (c == null) return 0;
            c.EnsureHealthBoxes();
            int n = 0;
            for (int i = 0; i < c.HealthMax && i < c.HealthBoxes.Length; i++)
                if (c.HealthBoxes[i] == type) n++;
            return n;
        }

        public static bool HasDamageOfType(Character c, DamageType type)
            => CountType(c, type) > 0;

        public static DamageType GetTopHealPriority(Character c)
        {
            if (HasDamageOfType(c, DamageType.Bashing)) return DamageType.Bashing;
            if (HasDamageOfType(c, DamageType.Lethal)) return DamageType.Lethal;
            if (HasDamageOfType(c, DamageType.Aggravated)) return DamageType.Aggravated;
            return DamageType.None;
        }

        // ============================================================
        // Стоимость
        // ============================================================

        /// <summary>
        /// Сколько уровней лечится за клик.
        /// Если стоимость ≥ 1 — 1 уровень.
        /// Если стоимость &lt; 1 — floor(1 / cost), минимум 1.
        /// </summary>
        public static int LevelsPerClick(float costPerLevel)
        {
            if (costPerLevel <= 0f) return 1;
            if (costPerLevel >= 1f) return 1;
            int levels = Mathf.FloorToInt(1f / costPerLevel);
            return levels < 1 ? 1 : levels;
        }

        /// <summary>Сколько крови уйдёт на n уровней.</summary>
        public static int BloodCostForLevels(int n, float costPerLevel)
        {
            if (n <= 0) return 0;
            if (costPerLevel < 0f) costPerLevel = 0f;
            return Mathf.CeilToInt(n * costPerLevel);
        }

        /// <summary>Сколько силы воли уйдёт на n уровней (только agg).</summary>
        public static int WillpowerCostForLevels(DamageType type, int n)
        {
            if (n <= 0) return 0;
            return type == DamageType.Aggravated ? n : 0;
        }

        /// <summary>
        /// Стоимость одной клетки указанного типа. Возвращает costPerLevel.
        /// </summary>
        public static float GetCostForType(DamageType type, BloodPotencyHealCost cfg)
        {
            if (cfg == null)
            {
                switch (type)
                {
                    case DamageType.Bashing: return 1f;
                    case DamageType.Lethal: return 1f;
                    case DamageType.Aggravated: return 5f;
                }
                return 1f;
            }
            switch (type)
            {
                case DamageType.Bashing: return cfg.Bashing;
                case DamageType.Lethal: return cfg.Lethal;
                case DamageType.Aggravated: return cfg.Aggravated;
            }
            return 1f;
        }

        // ============================================================
        // Лечение
        // ============================================================

        public static int HealLevels(Character c, DamageType type, int maxLevels, float costPerLevel, bool free = false)
        {
            if (c == null || maxLevels <= 0) return 0;
            if (type == DamageType.None) return 0;
            if (costPerLevel < 0f) costPerLevel = 0f;

            c.EnsureHealthBoxes();

            int have = CountType(c, type);
            if (have <= 0) return 0;

            int canHeal = Mathf.Min(have, maxLevels);

            if (!free)
            {
                while (canHeal > 0)
                {
                    int bc = BloodCostForLevels(canHeal, costPerLevel);
                    int wc = WillpowerCostForLevels(type, canHeal);
                    if (bc <= c.BloodCurrent && wc <= c.WillpowerCurrent) break;
                    canHeal--;
                }
                if (canHeal <= 0) return 0;

                int bloodCost = BloodCostForLevels(canHeal, costPerLevel);
                int wpCost = WillpowerCostForLevels(type, canHeal);
                c.BloodCurrent -= bloodCost;
                c.WillpowerCurrent -= wpCost;
            }

            int removed = 0;
            for (int i = c.HealthMax - 1; i >= 0 && removed < canHeal; i--)
            {
                if (i >= c.HealthBoxes.Length) continue;
                if (c.HealthBoxes[i] == type)
                {
                    c.HealthBoxes[i] = DamageType.None;
                    removed++;
                }
            }

            c.RecalculateHealthCurrent();
            return removed;
        }
    }
}