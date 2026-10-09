using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;
using VTR.UI.Tabs;

namespace VTR.UI
{
    public class ChronicleEditorScreen : IScreen
    {
        private Chronicle _chronicle;
        private VisualElement _tabContent;
        private Button _activeTabButton;

        private readonly List<ITab> _tabs = new List<ITab>();

        public void Build(VisualElement root)
        {
            _chronicle = SessionContext.CurrentChronicle;
            if (_chronicle == null)
            {
                root.Add(UIRoot.MakeLabel("Хроника не загружена. Вернитесь в меню.", 16, Color.red));
                return;
            }

            _tabs.Clear();
            _tabs.Add(new MainTab());
            _tabs.Add(new RanksTab());
            _tabs.Add(new AttributesTab());
            _tabs.Add(new SkillsTab());
            _tabs.Add(new DisciplinesTab());
            _tabs.Add(new ClansTab());
            _tabs.Add(new BloodlinesTab());
            _tabs.Add(new MoralitiesTab());
            _tabs.Add(new MeritsTab());
            _tabs.Add(new AddonsTab());
            _tabs.Add(new XpTab());
            _tabs.Add(new BloodPotencyTab());
            _tabs.Add(new RollsTab());

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.flexDirection = FlexDirection.Column;
            root.Add(container);

            BuildTopBar(container);
            BuildTabBar(container);

            // Важно: обычный VisualElement, не ScrollView.
            // Каждая вкладка сама решает, где ей нужен скролл.
            _tabContent = new VisualElement();
            _tabContent.style.flexGrow = 1;
            _tabContent.style.flexDirection = FlexDirection.Column;
            _tabContent.style.paddingTop = 12;
            _tabContent.style.paddingBottom = 12;
            _tabContent.style.paddingLeft = 20;
            _tabContent.style.paddingRight = 20;
            container.Add(_tabContent);

            OpenTab(0, null);
        }

        private void BuildTopBar(VisualElement parent)
        {
            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.paddingTop = 10;
            bar.style.paddingBottom = 10;
            bar.style.paddingLeft = 16;
            bar.style.paddingRight = 16;
            bar.style.backgroundColor = UIRoot.BgPanel;
            bar.style.flexShrink = 0;
            parent.Add(bar);

            var title = UIRoot.MakeLabel($"Хроника: {_chronicle.Name}", 20);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.flexGrow = 1;
            bar.Add(title);

            var saveBtn = new Button(() =>
            {
                JsonSaveSystem.SaveChronicle(_chronicle);
                Debug.Log($"[Editor] Хроника сохранена: {_chronicle.Name}");
            });
            saveBtn.text = "💾  Сохранить";
            UIRoot.StyleButton(saveBtn, UIRoot.AccentGreen, Color.white, 34);
            saveBtn.style.width = 160;
            bar.Add(saveBtn);

            var backBtn = new Button(() => UIRoot.Instance.ShowChronicleList());
            backBtn.text = "←  К списку";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 34);
            backBtn.style.width = 140;
            bar.Add(backBtn);
        }

        private void BuildTabBar(VisualElement parent)
        {
            var bar = new ScrollView(ScrollViewMode.Horizontal);
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.height = 42;
            bar.style.backgroundColor = UIRoot.BgDark;
            bar.style.flexShrink = 0;
            parent.Add(bar);

            var inner = new VisualElement();
            inner.style.flexDirection = FlexDirection.Row;
            bar.Add(inner);

            for (int i = 0; i < _tabs.Count; i++)
            {
                int idx = i;
                string tabName = _tabs[i].Name;
                Button btn = null;
                btn = new Button(() => OpenTab(idx, btn));
                btn.text = tabName;
                btn.style.height = 40;
                btn.style.fontSize = 14;
                btn.style.color = Color.white;
                btn.style.backgroundColor = UIRoot.BgPanel;
                btn.style.marginLeft = 1;
                btn.style.marginRight = 1;
                btn.style.borderTopLeftRadius = 0;
                btn.style.borderTopRightRadius = 0;
                btn.style.borderBottomLeftRadius = 0;
                btn.style.borderBottomRightRadius = 0;
                inner.Add(btn);
            }
        }

        private void OpenTab(int idx, Button btn)
        {
            if (idx < 0 || idx >= _tabs.Count) return;

            if (_activeTabButton != null)
                _activeTabButton.style.backgroundColor = UIRoot.BgPanel;
            if (btn != null)
                btn.style.backgroundColor = UIRoot.AccentBlue;
            _activeTabButton = btn;

            _tabContent.Clear();
            _tabs[idx].Build(_tabContent, _chronicle);
        }
    }
}