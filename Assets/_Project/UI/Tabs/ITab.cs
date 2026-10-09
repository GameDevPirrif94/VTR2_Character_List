using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI.Tabs
{
    public interface ITab
    {
        string Name { get; }
        void Build(VisualElement root, Chronicle chronicle);
    }
}