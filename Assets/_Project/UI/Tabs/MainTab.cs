using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI.Tabs
{
    public class MainTab : ITab
    {
        public string Name => "Основное";

        public void Build(VisualElement root, Chronicle chr)
        {
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            root.Add(scroll);

            scroll.Add(UIRoot.MakeHeader("Основное"));
            scroll.Add(UIRoot.MakeMuted($"Id: {chr.Id}"));

            var s1 = UIWidgets.Section("Настройки хроники");
            s1.Add(UIWidgets.MakeTextField("Название хроники", chr.Name, v => chr.Name = v));
            s1.Add(UIWidgets.MakeMultilineField("Описание", chr.Description, v => chr.Description = v));
            s1.Add(UIWidgets.MakeIntField("Максимум игроков", chr.MaxPlayers, v => chr.MaxPlayers = v, 1, 32));
            scroll.Add(s1);

            var s2 = UIWidgets.Section("Временные рамки");
            var tfButtons = new VisualElement();
            tfButtons.style.flexDirection = FlexDirection.Row;
            tfButtons.style.flexWrap = Wrap.Wrap;
            s2.Add(tfButtons);

            foreach (var tf in chr.TimeFrames)
            {
                string tfLocal = tf;
                var b = new Button(() =>
                {
                    chr.SelectedTimeFrame = tfLocal;
                    Debug.Log($"[MainTab] Временная рамка: {tfLocal}");
                    root.Clear();
                    Build(root, chr);
                });
                b.text = tfLocal;
                UIRoot.StyleButton(b, chr.SelectedTimeFrame == tfLocal
                    ? UIRoot.AccentBlue
                    : UIRoot.AccentNeutral, Color.white, 32);
                b.style.width = 70;
                tfButtons.Add(b);
            }

            s2.Add(UIRoot.MakeMuted($"Текущая: {chr.SelectedTimeFrame}"));
            scroll.Add(s2);
        }
    }
}