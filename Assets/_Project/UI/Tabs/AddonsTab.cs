using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class AddonsTab : ITab
    {
        public string Name => "Дополнения";

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (string.IsNullOrEmpty(_selectedId) && chronicle.Addons.Count > 0)
                _selectedId = chronicle.Addons[0].Id;

            _root.Add(UIRoot.MakeHeader("Дополнения"));
            _root.Add(UIRoot.MakeMuted(
                "Дополнения — свободные записи. Игрок при создании персонажа раскидывает по ним очки " +
                "(0–5 в каждое, общий лимит — очки дополнений ранга). " +
                "Отметка «Доступно со старта» показывает дополнение игрокам."));

            var split = new VisualElement();
            split.style.flexDirection = FlexDirection.Row;
            split.style.flexGrow = 1;
            split.style.flexBasis = 0;
            _root.Add(split);

            BuildLeftPanel(split);
            BuildRightPanel(split);

            RefreshList();
            RefreshForm();
        }

        private void BuildLeftPanel(VisualElement parent)
        {
            var left = new VisualElement();
            left.style.width = 320;
            left.style.minWidth = 260;
            left.style.flexShrink = 0;
            left.style.marginRight = 14;
            left.style.paddingTop = 12;
            left.style.paddingBottom = 12;
            left.style.paddingLeft = 12;
            left.style.paddingRight = 12;
            left.style.backgroundColor = UIRoot.BgPanel;
            left.style.borderTopLeftRadius = 6;
            left.style.borderTopRightRadius = 6;
            left.style.borderBottomLeftRadius = 6;
            left.style.borderBottomRightRadius = 6;
            parent.Add(left);

            var title = UIRoot.MakeLabel("Дополнения хроники", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            left.Add(UIWidgets.MakeLongButton("＋  Добавить дополнение", AddAddon, UIRoot.AccentGreen));
            left.Add(UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral));
            left.Add(UIWidgets.MakeLongButton("✕  Удалить выбранное", DeleteAddon, UIRoot.AccentRed));
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Addons.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет дополнений. Добавьте первое."));
                return;
            }

            foreach (var a in _chr.Addons)
            {
                var addon = a;
                var btn = new Button(() =>
                {
                    _selectedId = addon.Id;
                    RefreshList();
                    RefreshForm();
                });
                string prefix = addon.AvailableFromStart ? "✓ " : "✗ ";
                btn.text = prefix + (string.IsNullOrEmpty(addon.Name) ? "(без имени)" : addon.Name);
                bool isSelected = addon.Id == _selectedId;
                UIRoot.StyleButton(btn,
                    isSelected ? UIRoot.AccentBlue : UIRoot.AccentNeutral,
                    Color.white, 32);
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.marginTop = 2;
                btn.style.marginBottom = 2;
                _listPanel.Add(btn);
            }
        }

        private void BuildRightPanel(VisualElement parent)
        {
            _formPanel = new ScrollView();
            _formPanel.style.flexGrow = 1;
            _formPanel.style.flexBasis = 0;
            _formPanel.style.paddingLeft = 4;
            _formPanel.style.paddingRight = 4;
            parent.Add(_formPanel);
        }

        private void RefreshForm()
        {
            _formPanel.Clear();

            var a = _chr.FindAddon(_selectedId);
            if (a == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите дополнение слева или добавьте новое."));
                return;
            }

            _formPanel.Add(UIRoot.MakeHeader(string.IsNullOrEmpty(a.Name) ? "(без имени)" : a.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {a.Id}"));

            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название дополнения", a.Name, v =>
            {
                a.Name = v;
                RefreshList();
            }));

            var toggle = new Toggle("Доступно со старта") { value = a.AvailableFromStart };
            UIWidgets.StyleToggle(toggle);
            toggle.RegisterValueChangedCallback(e =>
            {
                a.AvailableFromStart = e.newValue;
                RefreshList();
            });
            s1.Add(toggle);
            s1.Add(UIRoot.MakeMuted(
                "Если включено — дополнение показывается игроку при создании персонажа.", 11));

            _formPanel.Add(s1);

            var s2 = UIWidgets.Section("Проверка и действия");

            if (string.IsNullOrWhiteSpace(a.Name))
            {
                var warn = new Label("• Название дополнения пустое.");
                warn.style.color = new Color(0.95f, 0.7f, 0.3f);
                warn.style.fontSize = 13;
                s2.Add(warn);
            }
            else
            {
                var ok = new Label("✓ Всё заполнено корректно");
                ok.style.color = new Color(0.55f, 0.85f, 0.55f);
                ok.style.fontSize = 13;
                s2.Add(ok);
            }

            var saveLibBtn = UIWidgets.MakeLongButton(
                "📚  Сохранить в библиотеку",
                () =>
                {
                    SessionContext.Library.AddOrUpdateAddon(a);
                    Debug.Log($"[AddonsTab] Дополнение сохранено в библиотеку: {a.Name} ({a.Id})");
                },
                UIRoot.AccentGreen);
            s2.Add(saveLibBtn);

            _formPanel.Add(s2);
        }

        private void AddAddon()
        {
            var a = new Addon
            {
                Id = JsonSaveSystem.NewId("addon"),
                Name = $"Новое дополнение {_chr.Addons.Count + 1}",
                AvailableFromStart = true
            };
            _chr.Addons.Add(a);
            _selectedId = a.Id;
            Debug.Log($"[AddonsTab] Добавлено дополнение: {a.Id}");
            RefreshList();
            RefreshForm();
        }

        private void DeleteAddon()
        {
            var a = _chr.FindAddon(_selectedId);
            if (a == null) return;

            _chr.Addons.Remove(a);
            Debug.LogWarning($"[AddonsTab] Удалено дополнение: {a.Name} ({a.Id})");

            _selectedId = _chr.Addons.Count > 0 ? _chr.Addons[0].Id : null;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.Addons == null || lib.Addons.Count == 0)
            {
                Debug.LogWarning("[AddonsTab] Библиотека пуста (нет дополнений).");
                return;
            }

            int added = 0;
            foreach (var addon in lib.Addons)
            {
                if (_chr.Addons.Any(x => x.Id == addon.Id)) continue;
                var json = JsonSaveSystem.Serialize(addon);
                var clone = JsonSaveSystem.Deserialize<Addon>(json);
                _chr.Addons.Add(clone);
                added++;
            }

            Debug.Log($"[AddonsTab] Импортировано из библиотеки: {added} дополнений.");
            RefreshList();
            RefreshForm();
        }
    }
}