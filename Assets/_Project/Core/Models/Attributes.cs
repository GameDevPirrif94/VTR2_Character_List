using System;

namespace VTR.Core.Models
{
    /// <summary>
    /// Группа атрибутов или навыков (Ментальные/Физические/Социальные).
    /// </summary>
    [Serializable]
    public class GroupDef
    {
        public string Id;
        public string Name;
    }

    [Serializable]
    public class AttributeDef
    {
        public string Id;
        public string GroupId;
        public string Name;
    }
}