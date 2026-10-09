using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI.Tabs
{
    public class PlaceholderTab : ITab
    {
        public string Name { get; }

        public PlaceholderTab(string name) { Name = name; }

        public void Build(VisualElement root, Chronicle chronicle)
        {
            root.Add(UIRoot.MakeHeader(Name));
            root.Add(UIRoot.MakeMuted("Раздел будет реализован на следующем шаге Фазы 2."));
        }
    }
}