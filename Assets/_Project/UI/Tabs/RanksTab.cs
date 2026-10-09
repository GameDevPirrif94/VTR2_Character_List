using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class RanksTab : ITab
    {
        public string Name => "Ранги";

        private Chronicle _chr;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            if (string.IsNullOrEmpty(_selectedId) && chronicle.Ranks.Count > 0)
                _selectedId = chronicle.Ranks[0].Id;

            var split = new VisualElement();
            split.style.flexDirection = FlexDirection.Row;
            split.style.flexGrow = 1;
            root.Add(split);

            // --- Левая панель ---
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
            split.Add(left);

            var listTitle = UIRoot.MakeLabel("Список рангов", 16);
            listTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(listTitle);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 200;
            left.Add(_listPanel);

            var addBtn = new Button(AddRank);
            addBtn.text = "＋  Добавить ранг";
            UIRoot.StyleButton(addBtn, UIRoot.AccentGreen, Color.white, 34);
            left.Add(addBtn);

            var delBtn = new Button(DeleteRank);
            delBtn.text = "✕  Удалить выбранный";
            UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 34);
            left.Add(delBtn);

            // --- Правая панель ---
            _formPanel = new ScrollView();
            _formPanel.style.flexGrow = 1;
            _formPanel.style.flexBasis = 0;
            _formPanel.style.paddingLeft = 4;
            _formPanel.style.paddingRight = 4;
            split.Add(_formPanel);

            RefreshList();
            RefreshForm();
        }

        // ============================================================
        // Список
        // ============================================================

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Ranks.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет рангов. Добавьте первый."));
                return;
            }

            foreach (var rank in _chr.Ranks)
            {
                var r = rank;
                var btn = new Button(() =>
                {
                    _selectedId = r.Id;
                    RefreshList();
                    RefreshForm();
                });
                btn.text = r.Name;
                bool isSelected = r.Id == _selectedId;
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
        // Форма
        // ============================================================

        private void RefreshForm()
        {
            _formPanel.Clear();

            var rank = _chr.FindRank(_selectedId);
            if (rank == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите ранг слева или добавьте новый."));
                return;
            }

            _formPanel.Add(UIRoot.MakeHeader($"Ранг: {rank.Name}"));

            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название ранга", rank.Name, v =>
            {
                rank.Name = v;
                RefreshList();
            }));
            s1.Add(UIWidgets.MakeIntField("Максимум игроков (0 = ранг недоступен)",
                rank.MaxPlayers, v => rank.MaxPlayers = v, 0, 100));
            _formPanel.Add(s1);

            var s2 = UIWidgets.Section("Атрибуты");
            s2.Add(UIWidgets.MakeIntField("Очки: первичные",
                rank.AttributePoints.Primary, v => rank.AttributePoints.Primary = v, 0, 50));
            s2.Add(UIWidgets.MakeIntField("Очки: вторичные",
                rank.AttributePoints.Secondary, v => rank.AttributePoints.Secondary = v, 0, 50));
            s2.Add(UIWidgets.MakeIntField("Очки: третичные",
                rank.AttributePoints.Tertiary, v => rank.AttributePoints.Tertiary = v, 0, 50));
            s2.Add(UIWidgets.MakeIntField("Максимум атрибута при создании",
                rank.AttributeMax, v => rank.AttributeMax = v, 1, 10));
            _formPanel.Add(s2);

            var s3 = UIWidgets.Section("Навыки");
            s3.Add(UIWidgets.MakeIntField("Очки: первичные",
                rank.SkillPoints.Primary, v => rank.SkillPoints.Primary = v, 0, 60));
            s3.Add(UIWidgets.MakeIntField("Очки: вторичные",
                rank.SkillPoints.Secondary, v => rank.SkillPoints.Secondary = v, 0, 60));
            s3.Add(UIWidgets.MakeIntField("Очки: третичные",
                rank.SkillPoints.Tertiary, v => rank.SkillPoints.Tertiary = v, 0, 60));
            s3.Add(UIWidgets.MakeIntField("Максимум навыка при создании",
                rank.SkillMax, v => rank.SkillMax = v, 1, 10));
            s3.Add(UIWidgets.MakeIntField("Очки специализаций",
                rank.SpecializationPoints, v => rank.SpecializationPoints = v, 0, 20));
            _formPanel.Add(s3);

            var s4 = UIWidgets.Section("Дисциплины, черты, дополнения");
            s4.Add(UIWidgets.MakeIntField("Очки дисциплин",
                rank.DisciplinePoints, v => rank.DisciplinePoints = v, 0, 20));
            s4.Add(UIWidgets.MakeIntField("Очки черт",
                rank.MeritPoints, v => rank.MeritPoints = v, 0, 50));
            s4.Add(UIWidgets.MakeIntField("Очки дополнений",
                rank.AddonPoints, v => rank.AddonPoints = v, 0, 50));
            _formPanel.Add(s4);

            var s5 = UIWidgets.Section("Стартовые значения");
            s5.Add(UIWidgets.MakeIntField("Стартовая густота крови",
                rank.StartBloodPotency, v => rank.StartBloodPotency = v, 0, 10));
            s5.Add(UIWidgets.MakeIntField("Стартовая мораль (человечность)",
                rank.StartHumanity, v => rank.StartHumanity = v, 0, 10));
            _formPanel.Add(s5);

            var s6 = UIWidgets.Section("Бонус первого персонажа игрока в хронике");
            var b = rank.FirstCharBonus;
            s6.Add(UIWidgets.MakeIntField("Опыт", b.Xp, v => b.Xp = v, 0, 1000));
            s6.Add(UIWidgets.MakeIntField("Доп. очки атрибутов",
                b.ExtraAttributePoints, v => b.ExtraAttributePoints = v, 0, 50));
            s6.Add(UIWidgets.MakeIntField("Доп. очки навыков",
                b.ExtraSkillPoints, v => b.ExtraSkillPoints = v, 0, 60));
            s6.Add(UIWidgets.MakeIntField("Доп. очки дисциплин",
                b.ExtraDisciplinePoints, v => b.ExtraDisciplinePoints = v, 0, 20));
            _formPanel.Add(s6);

            var summary = UIWidgets.Section("Сводка");
            summary.Add(UIRoot.MakeMuted(
                $"Стартовые очки: атрибуты {rank.AttributePoints.Primary}/{rank.AttributePoints.Secondary}/{rank.AttributePoints.Tertiary}, " +
                $"навыки {rank.SkillPoints.Primary}/{rank.SkillPoints.Secondary}/{rank.SkillPoints.Tertiary}, " +
                $"дисциплины {rank.DisciplinePoints}, черты {rank.MeritPoints}, дополнения {rank.AddonPoints}"));
            _formPanel.Add(summary);
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddRank()
        {
            var newRank = new Rank
            {
                Id = JsonSaveSystem.NewId("rank"),
                Name = $"Новый ранг {_chr.Ranks.Count + 1}"
            };
            _chr.Ranks.Add(newRank);
            _selectedId = newRank.Id;
            RefreshList();
            RefreshForm();
            Debug.Log($"[RanksTab] Добавлен ранг: {newRank.Id}");
        }

        private void DeleteRank()
        {
            if (string.IsNullOrEmpty(_selectedId)) return;

            int idx = _chr.Ranks.FindIndex(r => r.Id == _selectedId);
            if (idx < 0) return;

            var removed = _chr.Ranks[idx];
            _chr.Ranks.RemoveAt(idx);
            Debug.LogWarning($"[RanksTab] Удалён ранг: {removed.Name} ({removed.Id})");

            _selectedId = _chr.Ranks.Count > 0 ? _chr.Ranks[0].Id : null;
            RefreshList();
            RefreshForm();
        }
    }
}