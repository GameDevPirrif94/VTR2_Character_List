using System;
using System.Collections.Generic;

namespace VTR.Core.Models
{
    [Serializable]
    public class MoralitySin
    {
        public int Level;  // 1..10, где 10 — самый лёгкий грех
        public string Text;
    }

    [Serializable]
    public class Morality
    {
        public string Id;
        public string Name;
        public int StartRating = 7;

        public List<MoralitySin> Sins = new List<MoralitySin>();

        public string AuraName;
        public List<string> AuraAffectedParams = new List<string>();
    }
}