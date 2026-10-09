using System;
using System.Collections.Generic;
using VTR.Core.Models;

namespace VTR.Core.Rules
{
    [Serializable]
    public class RollResult
    {
        public int Pool;
        public int Modifier;
        public int Difficulty;
        public bool IsFateRoll;
        public int[] Dice;
        public int RawSuccesses;
        public int NetSuccesses;
        public int Ones;
        public RollOutcome Outcome;

        public string DiceString => Dice == null ? "" : string.Join(", ", Dice);
    }

    [Serializable]
    public class ExtendedRollResult
    {
        public List<RollResult> Rolls = new List<RollResult>();
        public int TotalSuccesses;
        public int Target;
        public bool Completed;
    }

    public static class DiceRoller
    {
        private const int Faces = 10;

        public static RollResult Roll(int pool, int modifier = 0, int difficulty = 0, Random rng = null)
        {
            rng ??= new Random();
            int total = pool + modifier;

            var res = new RollResult
            {
                Pool = pool,
                Modifier = modifier,
                Difficulty = difficulty
            };

            // Бросок Судьбы: пул <= 0
            if (total <= 0)
            {
                res.IsFateRoll = true;
                int d = rng.Next(1, Faces + 1);
                res.Dice = new[] { d };
                res.RawSuccesses = d == 10 ? 1 : 0;
                res.NetSuccesses = res.RawSuccesses;
                res.Ones = d == 1 ? 1 : 0;

                if (d == 10) res.Outcome = RollOutcome.FateRoll;
                else if (d == 1) res.Outcome = RollOutcome.CritFail;
                else res.Outcome = RollOutcome.Fail;

                // Для Судьбы "Success" означает единственный успех на 10.
                if (d == 10) res.Outcome = RollOutcome.Success;
                else if (d == 1) res.Outcome = RollOutcome.CritFail;
                else res.Outcome = RollOutcome.Fail;
                return res;
            }

            res.Dice = new int[total];
            int raw = 0, ones = 0;
            for (int i = 0; i < total; i++)
            {
                int d = rng.Next(1, Faces + 1);
                res.Dice[i] = d;
                if (d == 10) raw += 2;
                else if (d >= 8) raw += 1;
                else if (d == 1) ones += 1;
            }

            res.RawSuccesses = raw;
            res.Ones = ones;

            int net = raw - difficulty;
            if (net < 0) net = 0;
            res.NetSuccesses = net;

            if (raw == 0 && ones > 0) res.Outcome = RollOutcome.CritFail;
            else if (raw == 0) res.Outcome = RollOutcome.Fail;
            else if (net == 0) res.Outcome = RollOutcome.Fail;
            else if (net >= 5) res.Outcome = RollOutcome.CritSuccess;
            else res.Outcome = RollOutcome.Success;

            return res;
        }

        /// <summary>Соревновательный бросок: два пула, вычитание успехов.</summary>
        public static (RollResult a, RollResult b, int margin) CompetitiveRoll(
            int poolA, int poolB, int modA = 0, int modB = 0, Random rng = null)
        {
            rng ??= new Random();
            var a = Roll(poolA, modA, 0, rng);
            var b = Roll(poolB, modB, 0, rng);
            return (a, b, a.RawSuccesses - b.RawSuccesses);
        }

        /// <summary>Сопротивление: атакующий получает штраф = пул защищающегося.</summary>
        public static RollResult ResistanceRoll(
            int attackerPool, int defenderPool, int modifier = 0, int difficulty = 0, Random rng = null)
        {
            return Roll(attackerPool, modifier - defenderPool, difficulty, rng);
        }

        /// <summary>Расширенный бросок: до провала или достижения цели.</summary>
        public static ExtendedRollResult ExtendedRoll(
            int pool, int modifier, int difficulty, int target, int maxRolls = 20, Random rng = null)
        {
            rng ??= new Random();
            var result = new ExtendedRollResult { Target = target };
            int accumulated = 0;
            for (int i = 0; i < maxRolls; i++)
            {
                var r = Roll(pool, modifier, difficulty, rng);
                result.Rolls.Add(r);
                if (r.Outcome == RollOutcome.Fail || r.Outcome == RollOutcome.CritFail)
                    break;
                accumulated += r.NetSuccesses;
                if (accumulated >= target)
                {
                    result.Completed = true;
                    break;
                }
            }
            result.TotalSuccesses = accumulated;
            return result;
        }

        /// <summary>Форматирование результата в игровой журнал.</summary>
        public static string FormatLog(string paramString, RollResult r)
        {
            string outcome;
            switch (r.Outcome)
            {
                case RollOutcome.CritSuccess: outcome = "Крит. успех"; break;
                case RollOutcome.Success: outcome = "Успех"; break;
                case RollOutcome.CritFail: outcome = "Крит. провал"; break;
                case RollOutcome.Fail: outcome = "Провал"; break;
                case RollOutcome.FateRoll: outcome = "Бросок Судьбы"; break;
                default: outcome = "?"; break;
            }
            int totalDice = r.Dice?.Length ?? 0;
            string diffPart = r.Difficulty > 0 ? $", сложность: {r.Difficulty}" : "";
            string fatePart = r.IsFateRoll ? " [СУДЬБА]" : "";
            return $"{paramString}: {totalDice} кубов{fatePart}, бросок: [{r.DiceString}], " +
                   $"{r.NetSuccesses} успехов{diffPart}, {outcome}.";
        }

        public static string FormatExtendedLog(string paramString, ExtendedRollResult er)
        {
            var stepSuccesses = new List<int>();
            foreach (var r in er.Rolls) stepSuccesses.Add(r.NetSuccesses);
            string steps = string.Join(", ", stepSuccesses);
            string status = er.Completed ? "цель достигнута" : "провал";
            return $"{paramString} [Расширенный]: шагов {er.Rolls.Count}, успехи по шагам: [{steps}], " +
                   $"всего {er.TotalSuccesses}/{er.Target} ({status}).";
        }
    }
}