using System;
using UnityEngine;

namespace VTR.Net
{
    public static class PinGenerator
    {
        /// <summary>
        /// Генерирует 4-значный PIN-код: "0000"–"9999".
        /// </summary>
        public static string Generate()
        {
            int n = UnityEngine.Random.Range(0, 10000);
            return n.ToString("D4");
        }

        public static bool IsValid(string pin)
        {
            if (string.IsNullOrEmpty(pin)) return false;
            if (pin.Length != 4) return false;
            foreach (var c in pin)
                if (!char.IsDigit(c)) return false;
            return true;
        }
    }
}