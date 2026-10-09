using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI.Tabs
{
    public class BloodlinesTab : ITab
    {
        public string Name => "Бладлайны";

        private const int MaxExtraBonuses = 4;

        private Chronicle _chr;
        private VisualElement _root;
        private VisualElement _listPanel;
        private VisualElement _formPanel;
        private string _selectedId;

        public void Build(VisualElement root, Chronicle chronicle)
        {
            _chr = chronicle;
            _root = root;

            if (string.IsNullOrEmpty(_selectedId) && chronicle.Bloodlines.Count > 0)
                _selectedId = chronicle.Bloodlines[0].Id;

            _root.Add(UIRoot.MakeHeader("Бладлайны"));
            _root.Add(UIRoot.MakeMuted(
                "Каждый бладлайн относится к клану и получает все его бонусы и штрафы, " +
                "плюс собственное дополнительное проклятие и бонус. Требование по густоте крови " +
                "растёт только от дополнительных бонусов."));

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

            var title = UIRoot.MakeLabel("Бладлайны хроники", 15);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            _listPanel = new ScrollView();
            _listPanel.style.flexGrow = 1;
            _listPanel.style.marginTop = 6;
            _listPanel.style.marginBottom = 8;
            _listPanel.style.minHeight = 300;
            left.Add(_listPanel);

            left.Add(UIWidgets.MakeLongButton("＋  Добавить бладлайн", AddBloodline, UIRoot.AccentGreen));
            left.Add(UIWidgets.MakeLongButton("📚  Импорт из библиотеки", ImportFromLibrary, UIRoot.AccentNeutral));
            left.Add(UIWidgets.MakeLongButton("✕  Удалить выбранный", DeleteBloodline, UIRoot.AccentRed));
        }

        private void RefreshList()
        {
            _listPanel.Clear();

            if (_chr.Bloodlines.Count == 0)
            {
                _listPanel.Add(UIRoot.MakeMuted("Нет бладлайнов. Добавьте первый."));
                return;
            }

            var byClan = _chr.Bloodlines.GroupBy(b => b.ClanId);
            foreach (var grp in byClan)
            {
                var clan = _chr.FindClan(grp.Key);
                var header = UIRoot.MakeMuted(clan != null ? clan.Name : $"<{grp.Key}>", 11);
                header.style.marginTop = 8;
                header.style.marginBottom = 2;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                _listPanel.Add(header);

                foreach (var b in grp)
                {
                    var bl = b;
                    var btn = new Button(() =>
                    {
                        _selectedId = bl.Id;
                        RefreshList();
                        RefreshForm();
                    });
                    btn.text = bl.Name;
                    bool isSelected = bl.Id == _selectedId;
                    UIRoot.StyleButton(btn,
                        isSelected ? UIRoot.AccentBlue : UIRoot.AccentNeutral,
                        Color.white, 32);
                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    btn.style.marginTop = 2;
                    btn.style.marginBottom = 2;
                    _listPanel.Add(btn);
                }
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

            var bl = _chr.FindBloodline(_selectedId);
            if (bl == null)
            {
                _formPanel.Add(UIRoot.MakeMuted("Выберите бладлайн слева или добавьте новый."));
                return;
            }

            _formPanel.Add(UIRoot.MakeHeader(bl.Name));
            _formPanel.Add(UIRoot.MakeMuted($"Id: {bl.Id}"));

            // --- Основное ---
            var s1 = UIWidgets.Section("Основное");
            s1.Add(UIWidgets.MakeTextField("Название бладлайна", bl.Name, v =>
            {
                bl.Name = v;
                RefreshList();
            }));
            s1.Add(UIWidgets.MakeMultilineField("Описание", bl.Description, v => bl.Description = v, 80));
            s1.Add(UIWidgets.MakeMultilineField("Дополнительное проклятие", bl.AdditionalCurse,
                v => bl.AdditionalCurse = v, 80));

            var clanNames = _chr.Clans.Select(x => x.Name).ToList();
            var clanIds = _chr.Clans.Select(x => x.Id).ToList();
            int clanIdx = clanIds.IndexOf(bl.ClanId);
            if (clanIdx < 0) clanIdx = 0;

            s1.Add(UIWidgets.MakePickerBlock(
                "Родительский клан",
                clanNames.Count > 0 ? clanNames : new List<string> { "(нет кланов)" },
                clanIdx,
                i =>
                {
                    if (i >= 0 && i < clanIds.Count)
                    {
                        bl.ClanId = clanIds[i];
                        RefreshList();
                    }
                },
                width: 320));
            _formPanel.Add(s1);

            // --- Основной бонус ---
            var s2 = UIWidgets.Section("Основной бонус");
            s2.Add(UIRoot.MakeMuted(
                "Основной бонус не увеличивает требование густоты крови. " +
                "Дополнительные бонусы (ниже) — увеличивают."));

            if (bl.PrimaryBonus == null)
                bl.PrimaryBonus = new BloodlineBonus { Type = BloodlineBonusType.RegularDiscipline };
            s2.Add(BuildBonusEditor(bl, bl.PrimaryBonus, null));
            _formPanel.Add(s2);

            // --- Дополнительные бонусы ---
            var s3 = UIWidgets.Section($"Дополнительные бонусы (до {MaxExtraBonuses})");
            s3.Add(UIRoot.MakeMuted(
                "Вклад каждого: клановая дисциплина +2, обычная дисциплина +1, амальгама +2, черта +1."));

            if (bl.ExtraBonuses == null)
                bl.ExtraBonuses = new List<BloodlineBonus>();

            for (int i = 0; i < bl.ExtraBonuses.Count; i++)
            {
                int idx = i;
                var bonus = bl.ExtraBonuses[idx];

                var bonusBlock = new VisualElement();
                bonusBlock.style.marginBottom = 8;
                bonusBlock.style.paddingTop = 6;
                bonusBlock.style.paddingBottom = 6;
                bonusBlock.style.paddingLeft = 8;
                bonusBlock.style.paddingRight = 8;
                bonusBlock.style.backgroundColor = new Color(0.14f, 0.14f, 0.18f);
                bonusBlock.style.borderTopLeftRadius = 4;
                bonusBlock.style.borderTopRightRadius = 4;
                bonusBlock.style.borderBottomLeftRadius = 4;
                bonusBlock.style.borderBottomRightRadius = 4;

                var header = new VisualElement();
                header.style.flexDirection = FlexDirection.Row;
                header.style.alignItems = Align.Center;
                bonusBlock.Add(header);

                var lbl = new Label($"Бонус #{idx + 1}");
                lbl.style.fontSize = 13;
                lbl.style.color = UIRoot.TextMuted;
                lbl.style.flexGrow = 1;
                header.Add(lbl);

                var delBtn = new Button(() =>
                {
                    bl.ExtraBonuses.RemoveAt(idx);
                    RefreshForm();
                });
                delBtn.text = "✕";
                UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 26);
                delBtn.style.width = 36;
                header.Add(delBtn);

                bonusBlock.Add(BuildBonusEditor(bl, bonus, null));
                s3.Add(bonusBlock);
            }

            if (bl.ExtraBonuses.Count < MaxExtraBonuses)
            {
                var addBtn = new Button(() =>
                {
                    bl.ExtraBonuses.Add(new BloodlineBonus
                    {
                        Type = BloodlineBonusType.RegularDiscipline,
                        ReferenceId = ""
                    });
                    RefreshForm();
                });
                addBtn.text = $"＋  Добавить бонус ({bl.ExtraBonuses.Count}/{MaxExtraBonuses})";
                UIRoot.StyleButton(addBtn, UIRoot.AccentGreen, Color.white, 30);
                s3.Add(addBtn);
            }
            else
            {
                s3.Add(UIRoot.MakeMuted("Достигнут максимум дополнительных бонусов."));
            }
            _formPanel.Add(s3);

            // --- Требование густоты крови ---
            var s4 = UIWidgets.Section("Требование по густоте крови");
            int required = bl.GetRequiredBloodPotency();
            var reqLbl = new Label($"Требуется густота крови: {required}");
            reqLbl.style.fontSize = 16;
            reqLbl.style.color = new Color(0.95f, 0.8f, 0.35f);
            reqLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            reqLbl.style.marginBottom = 6;
            s4.Add(reqLbl);

            var breakdown = new List<string> { "Базовая: 1" };
            if (bl.ExtraBonuses != null)
            {
                for (int i = 0; i < bl.ExtraBonuses.Count; i++)
                    breakdown.Add($"Доп. бонус #{i + 1}: +{CostOf(bl.ExtraBonuses[i])}");
            }
            if (bl.ExtraBonuses == null || bl.ExtraBonuses.Count == 0)
                breakdown.Add("(доп. бонусов нет — требование не повышено)");
            s4.Add(UIRoot.MakeMuted(string.Join("; ", breakdown)));
            _formPanel.Add(s4);

            // --- Проверка и действия ---
            var s5 = UIWidgets.Section("Проверка и действия");

            var status = GetValidationStatus(bl);
            if (status.Count == 0)
            {
                var ok = new Label("✓ Всё заполнено корректно");
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
                    SessionContext.Library.AddOrUpdateBloodline(bl);
                    Debug.Log($"[BloodlinesTab] Бладлайн сохранён: {bl.Name} ({bl.Id})");
                },
                UIRoot.AccentGreen);
            s5.Add(saveLibBtn);
            _formPanel.Add(s5);
        }

        // ============================================================
        // Редактор бонуса
        // ============================================================

        private VisualElement BuildBonusEditor(Bloodline bl, BloodlineBonus bonus, string headerLabel)
        {
            var block = new VisualElement();
            block.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(headerLabel))
            {
                var h = new Label(headerLabel);
                h.style.fontSize = 13;
                h.style.unityFontStyleAndWeight = FontStyle.Bold;
                h.style.color = UIRoot.TextPrimary;
                h.style.marginBottom = 4;
                block.Add(h);
            }

            // Тип бонуса
            var typeChoices = new List<string>
            {
                "Клановая дисциплина",
                "Обычная дисциплина",
                "Амальгама",
                "Черта"
            };
            int typeIdx = (int)bonus.Type;

            block.Add(UIWidgets.MakePickerBlock(
                "Тип бонуса",
                typeChoices,
                typeIdx,
                i =>
                {
                    bonus.Type = (BloodlineBonusType)i;
                    bonus.ReferenceId = "";   // сбрасываем, т.к. пул ссылок разный
                    RefreshForm();
                },
                width: 280));

            // Ссылка на объект — с фильтрацией уже использованных
            var (rawIds, rawNames) = GetReferenceOptions(bonus.Type);
            var usedByOthers = CollectUsedReferenceIds(bl, bonus);

            var ids = new List<string>();
            var names = new List<string>();
            for (int i = 0; i < rawIds.Count; i++)
            {
                // Оставляем текущую ссылку даже если она «дубль» (на случай, если данные из старого сейва).
                if (usedByOthers.Contains(rawIds[i]) && rawIds[i] != bonus.ReferenceId)
                    continue;
                ids.Add(rawIds[i]);
                names.Add(rawNames[i]);
            }

            int refIdx = ids.IndexOf(bonus.ReferenceId);
            if (refIdx < 0) refIdx = 0;

            block.Add(UIWidgets.MakePickerBlock(
                $"Ссылка ({GetReferenceLabel(bonus.Type)})",
                names.Count > 0 ? names : new List<string> { "(нет доступных)" },
                refIdx,
                i =>
                {
                    if (i >= 0 && i < ids.Count)
                    {
                        bonus.ReferenceId = ids[i];
                        RefreshForm();
                    }
                },
                width: 320));

            if (ids.Count == 0)
            {
                var empty = UIRoot.MakeMuted(
                    $"Нет доступных опций: все подходящие {GetReferenceLabel(bonus.Type)} уже использованы.", 11);
                empty.style.marginTop = 2;
                block.Add(empty);
            }

            return block;
        }

        /// <summary>
        /// Возвращает множество id, уже занятых другими бонусами (не считая current).
        /// </summary>
        private HashSet<string> CollectUsedReferenceIds(Bloodline bl, BloodlineBonus current)
        {
            var used = new HashSet<string>();

            if (bl.PrimaryBonus != null && bl.PrimaryBonus != current &&
                !string.IsNullOrEmpty(bl.PrimaryBonus.ReferenceId))
                used.Add(bl.PrimaryBonus.ReferenceId);

            if (bl.ExtraBonuses != null)
            {
                foreach (var b in bl.ExtraBonuses)
                {
                    if (b == current) continue;
                    if (!string.IsNullOrEmpty(b.ReferenceId))
                        used.Add(b.ReferenceId);
                }
            }

            return used;
        }

        private (List<string> ids, List<string> names) GetReferenceOptions(BloodlineBonusType type)
        {
            switch (type)
            {
                case BloodlineBonusType.ClanDiscipline:
                case BloodlineBonusType.RegularDiscipline:
                    return (
                        _chr.Disciplines.Select(x => x.Id).ToList(),
                        _chr.Disciplines.Select(x => x.Name).ToList()
                    );
                case BloodlineBonusType.Amalgam:
                    return (
                        _chr.Disciplines.Where(x => x.IsAmalgam).Select(x => x.Id).ToList(),
                        _chr.Disciplines.Where(x => x.IsAmalgam).Select(x => x.Name).ToList()
                    );
                case BloodlineBonusType.Merit:
                    return (
                        _chr.Merits.Select(x => x.Id).ToList(),
                        _chr.Merits.Select(x => x.Name).ToList()
                    );
            }
            return (new List<string>(), new List<string>());
        }

        private static string GetReferenceLabel(BloodlineBonusType type)
        {
            switch (type)
            {
                case BloodlineBonusType.ClanDiscipline: return "клановая дисциплина";
                case BloodlineBonusType.RegularDiscipline: return "обычная дисциплина";
                case BloodlineBonusType.Amalgam: return "амальгама";
                case BloodlineBonusType.Merit: return "черта";
            }
            return "?";
        }

        private static int CostOf(BloodlineBonus b)
        {
            if (b == null) return 0;
            switch (b.Type)
            {
                case BloodlineBonusType.ClanDiscipline: return 2;
                case BloodlineBonusType.RegularDiscipline: return 1;
                case BloodlineBonusType.Amalgam: return 2;
                case BloodlineBonusType.Merit: return 1;
            }
            return 0;
        }

        // ============================================================
        // Валидация
        // ============================================================

        private List<string> GetValidationStatus(Bloodline b)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(b.Name))
                result.Add("Название бладлайна пустое.");
            if (string.IsNullOrWhiteSpace(b.AdditionalCurse))
                result.Add("Дополнительное проклятие не заполнено.");

            if (string.IsNullOrEmpty(b.ClanId) || _chr.FindClan(b.ClanId) == null)
                result.Add("Родительский клан не выбран.");

            if (b.PrimaryBonus == null || string.IsNullOrEmpty(b.PrimaryBonus.ReferenceId))
                result.Add("Основной бонус не выбран.");

            if (b.ExtraBonuses != null)
            {
                for (int i = 0; i < b.ExtraBonuses.Count; i++)
                {
                    if (string.IsNullOrEmpty(b.ExtraBonuses[i].ReferenceId))
                        result.Add($"Доп. бонус #{i + 1}: ссылка не выбрана.");
                }
            }

            return result;
        }

        // ============================================================
        // Действия
        // ============================================================

        private void AddBloodline()
        {
            if (_chr.Clans.Count == 0)
            {
                Debug.LogWarning("[BloodlinesTab] Нет кланов — сначала добавьте клан.");
                return;
            }

            var bl = new Bloodline
            {
                Id = JsonSaveSystem.NewId("bl"),
                Name = $"Новый бладлайн {_chr.Bloodlines.Count + 1}",
                Description = "",
                AdditionalCurse = "",
                ClanId = _chr.Clans[0].Id,
                PrimaryBonus = new BloodlineBonus
                {
                    Type = BloodlineBonusType.RegularDiscipline,
                    ReferenceId = _chr.Disciplines.FirstOrDefault()?.Id ?? ""
                }
            };
            _chr.Bloodlines.Add(bl);
            _selectedId = bl.Id;
            Debug.Log($"[BloodlinesTab] Добавлен бладлайн: {bl.Id}");
            RefreshList();
            RefreshForm();
        }

        private void DeleteBloodline()
        {
            var bl = _chr.FindBloodline(_selectedId);
            if (bl == null) return;

            _chr.Bloodlines.Remove(bl);
            Debug.LogWarning($"[BloodlinesTab] Удалён бладлайн: {bl.Name} ({bl.Id})");

            _selectedId = _chr.Bloodlines.Count > 0 ? _chr.Bloodlines[0].Id : null;
            RefreshList();
            RefreshForm();
        }

        private void ImportFromLibrary()
        {
            var lib = SessionContext.Library;
            if (lib.Bloodlines.Count == 0)
            {
                Debug.LogWarning("[BloodlinesTab] Библиотека пуста (нет бладлайнов).");
                return;
            }

            int added = 0;
            foreach (var bl in lib.Bloodlines)
            {
                if (_chr.Bloodlines.Any(x => x.Id == bl.Id)) continue;
                var json = JsonSaveSystem.Serialize(bl);
                var clone = JsonSaveSystem.Deserialize<Bloodline>(json);
                _chr.Bloodlines.Add(clone);
                added++;
            }

            Debug.Log($"[BloodlinesTab] Импортировано бладлайнов: {added}.");
            RefreshList();
            RefreshForm();
        }
    }
}