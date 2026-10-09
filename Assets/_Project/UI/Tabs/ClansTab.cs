using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class ClansTab : ITab
    {
        public string Name => "Кланы";

        private const int ClanDisciplines = 3;
        private const int FavoredAttributes = 2;
        private const int FavoredSkills = 4;

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (string.IsNullOrEmpty(_selectedId) && chronicle.Clans.Count > 0)
                _selectedId = chronicle.Clans[0].Id;

            _root.Add(UIRoot.MakeHeader("Кланы"));
            _root.Add(UIRoot.MakeMuted(
                "Кланы хроники: название, описание, проклятие, три клановые дисциплины, " +
                "два любимых атрибута и четыре любимых навыка."));

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

        // ============================================================
        // Левая панель
        // ============================================================

        private void BuildLeftPanel(VisualElement parent)
        {
            var left = new VisualElement();
            left.style.width = 280;
            left.style.minWidth = 240;
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

            var title = UIRoot.MakeLabel("Кланы хроники", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            left.Add(UIWidgets.MakeLongButton("＋  Добавить клан", AddClan, UIRoot.AccentGreen));
            left.Add(UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral));
            left.Add(UIWidgets.MakeLongButton("✕  Удалить выбранный", DeleteClan, UIRoot.AccentRed));
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Clans.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет кланов. Добавьте первый."));
                return;
            }

            foreach (var clan in _chr.Clans)
            {
                var c = clan;
                var btn = new Button(() =>
                {
                    _selectedId = c.Id;
                    RefreshList();
                    RefreshForm();
                });
                btn.text = c.Name;
                bool isSelected = c.Id == _selectedId;
                UIRoot.StyleButton(btn,
                    isSelected ? UIRoot.AccentBlue : UIRoot.AccentNeutral,
                    Color.white, 32);
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.marginTop = 2;
                btn.style.marginBottom = 2;
                _listPanel.Add(btn);
            }
        }

        // ============================================================
        // Правая панель
        // ============================================================

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

            var c = _chr.FindClan(_selectedId);
            if (c == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите клан слева или добавьте новый."));
                return;
            }

            _formPanel.Add(UIRoot.MakeHeader(c.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {c.Id}"));

            // --- Основное ---
            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название клана", c.Name, v =>
            {
                c.Name = v;
                RefreshList();
            }));
            s1.Add(UIWidgets.MakeMultilineField("Описание", c.Description, v => c.Description = v, 80));
            s1.Add(UIWidgets.MakeMultilineField("Проклятие (текстом)", c.Curse, v => c.Curse = v, 80));
            _formPanel.Add(s1);

            // --- Клановые дисциплины ---
            var s2 = UIWidgets.Section($"Клановые дисциплины ({ClanDisciplines})");
            s2.Add(UIRoot.MakeMuted(
                "Выберите ровно 3 дисциплины из списка дисциплин хроники."));

            var discIds = _chr.Disciplines.Select(x => x.Id).ToList();
            var discNames = _chr.Disciplines.Select(x => x.Name).ToList();
            s2.Add(UIWidgets.MakeMultiPickerBlock(
                "Клановые дисциплины",
                discIds, discNames,
                c.ClanDisciplineIds,
                ClanDisciplines,
                () => RefreshForm()));
            _formPanel.Add(s2);

            // --- Любимые атрибуты ---
            var s3 = UIWidgets.Section($"Любимые атрибуты ({FavoredAttributes})");
            s3.Add(UIRoot.MakeMuted(
                "Выберите ровно 2 атрибута. Игрок получает +1 к одному из них на старте."));

            var attrIds = _chr.Attributes.Select(x => x.Id).ToList();
            var attrNames = _chr.Attributes.Select(x => x.Name).ToList();
            s3.Add(UIWidgets.MakeMultiPickerBlock(
                "Любимые атрибуты",
                attrIds, attrNames,
                c.FavoredAttributeIds,
                FavoredAttributes,
                () => RefreshForm()));
            _formPanel.Add(s3);

            // --- Любимые навыки ---
            var s4 = UIWidgets.Section($"Любимые навыки ({FavoredSkills})");
            s4.Add(UIRoot.MakeMuted("Выберите ровно 4 навыка."));

            var skillIds = _chr.Skills.Select(x => x.Id).ToList();
            var skillNames = _chr.Skills.Select(x => _chr.GetSkillName(x)).ToList();
            s4.Add(UIWidgets.MakeMultiPickerBlock(
                "Любимые навыки",
                skillIds, skillNames,
                c.FavoredSkillIds,
                FavoredSkills,
                () => RefreshForm()));
            _formPanel.Add(s4);

            // --- Валидация + действия ---
            var s5 = UIWidgets.Section("Проверка и действия");

            var status = GetValidationStatus(c);
            if (status.Count == 0)
            {
                var ok = new Label("✓ Все параметры заполнены корректно");
                ok.style.color = new Color(0.55f, 0.85f, 0.55f);
                ok.style.fontSize = 13;
                s5.Add(ok);
            }
            else
            {
                foreach (var p in status)
                {
                    var l = new Label("• " + p);
                    l.style.color = new Color(0.95f, 0.7f, 0.3f);
                    l.style.fontSize = 13;
                    s5.Add(l);
                }
            }

            var saveLibBtn = UIWidgets.MakeLongButton(
                "📚  Сохранить в библиотеку",
                () =>
                {
                    SessionContext.Library.AddOrUpdateClan(c);
                    Debug.Log($"[ClansTab] Клан сохранён в библиотеку: {c.Name} ({c.Id})");
                },
                UIRoot.AccentGreen);
            s5.Add(saveLibBtn);

            _formPanel.Add(s5);
        }

        // ============================================================
        // Валидация
        // ============================================================

        private List<string> GetValidationStatus(Clan c)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(c.Name))
                result.Add("Название клана пустое.");
            if (string.IsNullOrWhiteSpace(c.Curse))
                result.Add("Проклятие не заполнено.");

            if (c.ClanDisciplineIds.Count != ClanDisciplines)
                result.Add($"Клановых дисциплин: {c.ClanDisciplineIds.Count}/{ClanDisciplines}.");

            if (c.FavoredAttributeIds.Count != FavoredAttributes)
                result.Add($"Любимых атрибутов: {c.FavoredAttributeIds.Count}/{FavoredAttributes}.");

            if (c.FavoredSkillIds.Count != FavoredSkills)
                result.Add($"Любимых навыков: {c.FavoredSkillIds.Count}/{FavoredSkills}.");

            return result;
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddClan()
        {
            var c = new Clan
            {
                Id = JsonSaveSystem.NewId("clan"),
                Name = $"Новый клан {_chr.Clans.Count + 1}",
                Description = "",
                Curse = ""
            };
            _chr.Clans.Add(c);
            _selectedId = c.Id;
            Debug.Log($"[ClansTab] Добавлен клан: {c.Id}");
            RefreshList();
            RefreshForm();
        }

        private void DeleteClan()
        {
            var c = _chr.FindClan(_selectedId);
            if (c == null) return;

            var refs = new List<string>();
            foreach (var b in _chr.Bloodlines)
                if (b.ClanId == c.Id) refs.Add($"бладлайн «{b.Name}»");

            if (refs.Count > 0)
            {
                Debug.LogWarning($"[ClansTab] Нельзя удалить «{c.Name}»: используется в {string.Join(", ", refs)}");
                return;
            }

            _chr.Clans.Remove(c);
            Debug.LogWarning($"[ClansTab] Удалён клан: {c.Name} ({c.Id})");

            _selectedId = _chr.Clans.Count > 0 ? _chr.Clans[0].Id : null;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.Clans.Count == 0)
            {
                Debug.LogWarning("[ClansTab] Библиотека пуста (нет кланов).");
                return;
            }

            int added = 0;
            foreach (var clan in lib.Clans)
            {
                if (_chr.Clans.Any(x => x.Id == clan.Id)) continue;
                var json = JsonSaveSystem.Serialize(clan);
                var clone = JsonSaveSystem.Deserialize<Clan>(json);
                _chr.Clans.Add(clone);
                added++;
            }

            Debug.Log($"[ClansTab] Импортировано из библиотеки кланов: {added}.");
            RefreshList();
            RefreshForm();
        }
    }
}