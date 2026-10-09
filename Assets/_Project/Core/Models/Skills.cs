using System;
using System.Collections.Generic;
using UnityEngine;

namespace VTR.Core.Models
{
    [Serializable]
    public class SkillDef
    {
        public string Id;
        public string GroupId;
        public string Name;

        [SerializeField]
        public Dictionary<string, string> AltByTimeFrame = new Dictionary<string, string>();
    }
}