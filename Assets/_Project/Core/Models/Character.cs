using System;
using System.Collections.Generic;
using UnityEngine;

namespace VTR.Core.Models
{
    [Serializable]
    public class Character
    {
        public string Id;
        public string PlayerId;
        public string ChronicleId;
        public string PlayerName;

        public string Name;
        public string ClanId;
        public string BloodlineId;
        public string RankId;
        public string Nature;
        public string Mask;
        public string MoralityId;
        public string FavoredAttributeId;

        [SerializeField] public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        [SerializeField] public Dictionary<string, int> Skills = new Dictionary<string, int>();

        [SerializeField]
        public Dictionary<string, List<string>> Specializations = new Dictionary<string, List<string>>();

        [SerializeField] public Dictionary<string, int> Disciplines = new Dictionary<string, int>();

        [SerializeField]
        public Dictionary<string, List<string>> LearnedAbilities = new Dictionary<string, List<string>>();

        public List<string> MeritIds = new List<string>();

        [SerializeField] public Dictionary<string, int> AddonLevels = new Dictionary<string, int>();

        // === Здоровье ===
        public int HealthMax = 7;
        public int HealthCurrent = 7;

        /// <summary>
        /// Клетки здоровья. Длина всегда = HealthMax.
        /// Индекс 0 — самая левая (тяжкая) клетка.
        /// </summary>
        public DamageType[] HealthBoxes = new DamageType[0];

        public int MoralityRating;
        public int Aura;
        public int WillpowerCurrent;
        public int WillpowerMax;
        public int BloodPotency;
        public int BloodCurrent;
        public int BloodMax;

        public int XpTotal;
        public int XpSpent;
        public int XpAvailable
        {
            get
            {
                int v = XpTotal - XpSpent;
                return v < 0 ? 0 : v;
            }
        }

        public string BirthDate;
        public string DeathDate;
        public int VisualAge;
        public int RealAge;
        public string Nationality;
        public string Gender;
        public string Build;
        public string Height;
        public string Weight;
        public string Hair;
        public string Eyes;
        public string NotableFeatures;
        public string Backstory;
        public string Haven;

        public string PortraitPath;

        // ============================================================
        // Клетки здоровья
        // ============================================================

        /// <summary>Приводит длину HealthBoxes к HealthMax.</summary>
        public void EnsureHealthBoxes()
        {
            int max = HealthMax < 1 ? 1 : HealthMax;
            if (HealthBoxes == null || HealthBoxes.Length != max)
            {
                var newBoxes = new DamageType[max];
                if (HealthBoxes != null)
                {
                    for (int i = 0; i < HealthBoxes.Length && i < max; i++)
                        newBoxes[i] = HealthBoxes[i];
                }
                HealthBoxes = newBoxes;
            }
        }

        /// <summary>Пересчитать HealthCurrent по клеткам. Гарантированно 0..HealthMax.</summary>
        public void RecalculateHealthCurrent()
        {
            EnsureHealthBoxes();
            int damaged = 0;
            for (int i = 0; i < HealthMax && i < HealthBoxes.Length; i++)
                if (HealthBoxes[i] != DamageType.None) damaged++;

            int hp = HealthMax - damaged;
            if (hp < 0) hp = 0;
            if (hp > HealthMax) hp = HealthMax;
            HealthCurrent = hp;
        }

        public int CountDamaged()
        {
            EnsureHealthBoxes();
            int n = 0;
            for (int i = 0; i < HealthMax && i < HealthBoxes.Length; i++)
                if (HealthBoxes[i] != DamageType.None) n++;
            return n;
        }

        public int FindRightmostEmpty()
        {
            EnsureHealthBoxes();
            for (int i = HealthMax - 1; i >= 0; i--)
                if (i < HealthBoxes.Length && HealthBoxes[i] == DamageType.None) return i;
            return -1;
        }

        public bool HasDamageOfType(DamageType type)
        {
            EnsureHealthBoxes();
            for (int i = 0; i < HealthMax && i < HealthBoxes.Length; i++)
                if (HealthBoxes[i] == type) return true;
            return false;
        }

        public bool IsAllAggravated()
        {
            EnsureHealthBoxes();
            if (HealthBoxes.Length == 0) return false;
            for (int i = 0; i < HealthMax && i < HealthBoxes.Length; i++)
                if (HealthBoxes[i] != DamageType.Aggravated) return false;
            return true;
        }
    }
}