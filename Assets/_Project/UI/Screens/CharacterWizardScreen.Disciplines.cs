using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI
{
    public partial class CharacterWizardScreen
    {
        // ============================================================
        // ШАГ 5: Дисциплины
        // ============================================================

        private void BuildStep_Disciplines(VisualElement parent)
        {
            parent.Add(UIRoot.MakeHeader("Шаг 5: Дисциплины"));

            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null)
            {
                parent.Add(UIRoot.MakeLabel("Ранг не выбран. Вернитесь на шаг 1.",
                    14, new Color(1f, 0.55f, 0.55f)));
                return;
            }

            EnsureDisciplinesInitialized();

            BuildFreeDiscSlotSection(parent);

            var clanDisciplines = ComputeClanDisciplineIds();
            var allAvailable = ComputeAvailableDisciplineIds(clanDisciplines);

            var counterSection = UIWidgets.Section("Очки дисциплин");
            _discCounterLabel = UIRoot.MakeLabel("", 15);
            counterSection.Add(_discCounterLabel);
            counterSection.Add(UIRoot.MakeMuted(
                "Дисциплины, полученные от черт, уже имеют уровень 1+ (помечены ★). " +
                "Сверху можно вкладывать очки. Пути требуют родителя с уровнем ≥ 1. " +
                "Амальгамы требуют изученные дисциплины.", 12));
            parent.Add(counterSection);

            // Клановые
            var clanSection = UIWidgets.Section("Клановые дисциплины");
            bool hasClan = false;
            foreach (var id in clanDisciplines)
            {
                var disc = Chr.FindDiscipline(id);
                if (disc == null) continue;
                if (disc.IsPath || disc.IsAmalgam) continue;
                clanSection.Add(BuildDisciplineRow(rank, disc));
                hasClan = true;
            }
            if (!hasClan)
                clanSection.Add(UIRoot.MakeMuted("(у выбранного клана нет клановых дисциплин)"));
            parent.Add(clanSection);

            // Другие
            var regularSection = UIWidgets.Section("Другие доступные дисциплины");
            bool hasRegular = false;
            foreach (var d in Chr.Disciplines)
            {
                if (d.IsPath || d.IsAmalgam) continue;
                if (clanDisciplines.Contains(d.Id)) continue;
                if (!allAvailable.Contains(d.Id)) continue;
                regularSection.Add(BuildDisciplineRow(rank, d));
                hasRegular = true;
            }
            if (!hasRegular)
                regularSection.Add(UIRoot.MakeMuted("(нет других доступных дисциплин)"));
            parent.Add(regularSection);

            // Пути
            var pathsSection = UIWidgets.Section("Пути");
            bool hasPaths = false;
            foreach (var path in Chr.Disciplines.FindAll(d => d.IsPath))
            {
                if (!allAvailable.Contains(path.ParentDisciplineId)) continue;
                pathsSection.Add(BuildPathRow(rank, path));
                hasPaths = true;
            }
            if (!hasPaths)
                pathsSection.Add(UIRoot.MakeMuted("(нет доступных путей)"));
            parent.Add(pathsSection);

            // Амальгамы
            var amalgamSection = UIWidgets.Section("Амальгамы");
            bool hasAmalgams = false;
            foreach (var am in Chr.Disciplines.FindAll(d => d.IsAmalgam))
            {
                bool inBase = allAvailable.Contains(am.Id);
                bool reqsOk = true;
                if (am.AmalgamRequirements != null)
                {
                    foreach (var req in am.AmalgamRequirements)
                    {
                        if (!allAvailable.Contains(req.DisciplineId)) { reqsOk = false; break; }
                    }
                }
                if (!inBase && !reqsOk) continue;
                amalgamSection.Add(BuildAmalgamRow(rank, am));
                hasAmalgams = true;
            }
            if (!hasAmalgams)
                amalgamSection.Add(UIRoot.MakeMuted("(нет доступных амальгам)"));
            parent.Add(amalgamSection);

            var totalSection = UIWidgets.Section("Итог");
            _totalDiscSummary = UIRoot.MakeMuted("", 13);
            totalSection.Add(_totalDiscSummary);
            parent.Add(totalSection);

            UpdateAllDisciplineLabels(rank);
        }

        // ============================================================
        // Учёт уровней от черт
        // ============================================================

        private int GetDisciplineMeritLevel(string discId)
        {
            if (string.IsNullOrEmpty(discId)) return 0;

            int total = 0;

            if (Draft.MeritIds != null)
            {
                foreach (var meritId in Draft.MeritIds)
                {
                    var merit = Chr.FindMerit(meritId);
                    if (merit?.Modifiers == null) continue;
                    foreach (var mod in merit.Modifiers)
                    {
                        if (mod.Target == "disc." + discId && mod.Value > 0)
                            total += mod.Value;
                    }
                }
            }

            foreach (var slot in CollectFreeDiscSlots())
            {
                if (Draft.FreeDiscChoices.TryGetValue(slot.Key, out var chosen) &&
                    chosen == discId)
                {
                    total += 1;
                }
            }

            return total;
        }

        private int GetEffectiveDisciplineLevel(string discId)
            => GetDisciplineLevel(discId) + GetDisciplineMeritLevel(discId);

        // ============================================================
        // Свободные слоты от черт — ключи теперь УНИКАЛЬНЫЕ
        // ============================================================

        private class FreeDiscSlot
        {
            public string Key;
            public string MeritId;
            public string MeritName;
            public bool IsClan;
        }

        private List<FreeDiscSlot> CollectFreeDiscSlots()
        {
            var result = new List<FreeDiscSlot>();
            if (Draft.MeritIds == null) return result;

            // Счётчик вхождений каждой черты, чтобы ключи были уникальны даже
            // при взятии мультиплицируемой черты несколько раз.
            var occurrences = new Dictionary<string, int>();

            foreach (var meritId in Draft.MeritIds)
            {
                int occ = occurrences.TryGetValue(meritId, out int o) ? o : 0;
                occurrences[meritId] = occ + 1;

                var merit = Chr.FindMerit(meritId);
                if (merit == null || merit.Modifiers == null) continue;

                for (int i = 0; i < merit.Modifiers.Count; i++)
                {
                    var mod = merit.Modifiers[i];
                    if (mod.Target == MeritSpecialTargets.FreeClanDiscipline ||
                        mod.Target == MeritSpecialTargets.FreeRegularDiscipline)
                    {
                        result.Add(new FreeDiscSlot
                        {
                            Key = $"{meritId}:{i}:{occ}",
                            MeritId = meritId,
                            MeritName = merit.Name,
                            IsClan = mod.Target == MeritSpecialTargets.FreeClanDiscipline
                        });
                    }
                }
            }
            return result;
        }

        private void BuildFreeDiscSlotSection(VisualElement parent)
        {
            var slots = CollectFreeDiscSlots();
            if (slots.Count == 0) return;

            var section = UIWidgets.Section("Дисциплины по выбору (от черт)");
            section.Add(UIRoot.MakeMuted(
                "Некоторые черты дают дисциплину на выбор. Выбранная сразу получает 1 уровень. " +
                "Клановая — считается клановой. Неклановая — обычной. " +
                "Уже доступные персонажу дисциплины в список не попадают."));

            foreach (var slot in slots)
            {
                var s = slot;

                // Собираем «занятые» дисциплины: доступные через клан, бладлайн,
                // конкретные черты disc.<id>, а также выбранные в ДРУГИХ слотах.
                var occupied = CollectOccupiedDisciplineIds(excludeSlotKey: s.Key);

                var candidates = Chr.Disciplines.FindAll(d =>
                    !d.IsPath && !d.IsAmalgam && !occupied.Contains(d.Id));

                var names = new List<string> { "— (не выбрано) —" };
                var ids = new List<string> { "" };
                foreach (var c in candidates)
                {
                    ids.Add(c.Id);
                    names.Add(c.Name);
                }

                // Если текущий выбор уже не проходит фильтр (например, только что
                // стал занят через другой источник), оставим его в списке,
                // чтобы игрок мог увидеть, что стоит, и при желании сбросить.
                string current = Draft.FreeDiscChoices.TryGetValue(s.Key, out var cur) ? cur : "";
                int idx = ids.IndexOf(current);
                if (idx < 0 && !string.IsNullOrEmpty(current))
                {
                    var currentDisc = Chr.FindDiscipline(current);
                    if (currentDisc != null)
                    {
                        ids.Add(current);
                        names.Add(currentDisc.Name + "  (сейчас)");
                        idx = ids.Count - 1;
                    }
                }
                if (idx < 0) idx = 0;

                string label = s.IsClan
                    ? $"Клановая по выбору — черта «{s.MeritName}»"
                    : $"Неклановая по выбору — черта «{s.MeritName}»";

                section.Add(UIWidgets.MakePickerBlock(
                    label,
                    names,
                    idx,
                    i =>
                    {
                        if (i < 0 || i >= ids.Count) return;

                        string newChosen = ids[i];

                        if (string.IsNullOrEmpty(newChosen))
                            Draft.FreeDiscChoices.Remove(s.Key);
                        else
                            Draft.FreeDiscChoices[s.Key] = newChosen;

                        ResetOrphanedDisciplines();
                        CleanupInvalidPaths();

                        Refresh();
                    },
                    width: 400));
            }

            parent.Add(section);
        }

        /// <summary>
        /// Собирает id всех дисциплин, которые уже «заняты» персонажем:
        ///   • базовые клановые выбранного клана,
        ///   • бонусы бладлайна (ClanDiscipline / RegularDiscipline / Amalgam),
        ///   • конкретные disc.&lt;id&gt; модификаторы выбранных черт,
        ///   • выборы в других свободных слотах (кроме excludeSlotKey).
        /// Плюс сама текущая дисциплина (если уже выбрана в этом слоте) НЕ входит
        /// в это множество (мы хотим видеть её в списке).
        /// </summary>
        private HashSet<string> CollectOccupiedDisciplineIds(string excludeSlotKey)
        {
            var set = new HashSet<string>();

            // 1. Базовые клановые клана.
            var clan = Chr.FindClan(Draft.ClanId);
            if (clan != null && clan.ClanDisciplineIds != null)
                foreach (var id in clan.ClanDisciplineIds)
                    if (!string.IsNullOrEmpty(id)) set.Add(id);

            // 2. Бонусы бладлайна.
            if (!string.IsNullOrEmpty(Draft.BloodlineId))
            {
                var bl = Chr.FindBloodline(Draft.BloodlineId);
                if (bl != null)
                {
                    AddBonusDiscipline(bl.PrimaryBonus, set);
                    if (bl.ExtraBonuses != null)
                        foreach (var b in bl.ExtraBonuses)
                            AddBonusDiscipline(b, set);
                }
            }

            // 3. Конкретные disc.<id> от черт.
            if (Draft.MeritIds != null)
            {
                foreach (var meritId in Draft.MeritIds)
                {
                    var merit = Chr.FindMerit(meritId);
                    if (merit == null || merit.Modifiers == null) continue;

                    foreach (var mod in merit.Modifiers)
                    {
                        if (string.IsNullOrEmpty(mod.Target)) continue;
                        if (!mod.Target.StartsWith("disc.")) continue;
                        if (mod.Value <= 0) continue;

                        string discId = mod.Target.Substring(5);
                        if (discId == "clan.free" || discId == "regular.free") continue;
                        if (!string.IsNullOrEmpty(discId))
                            set.Add(discId);
                    }
                }
            }

            // 4. Выборы в других свободных слотах.
            if (Draft.FreeDiscChoices != null)
            {
                foreach (var kv in Draft.FreeDiscChoices)
                {
                    if (kv.Key == excludeSlotKey) continue;
                    if (!string.IsNullOrEmpty(kv.Value))
                        set.Add(kv.Value);
                }
            }

            return set;
        }

        private void AddBonusDiscipline(BloodlineBonus bonus, HashSet<string> set)
        {
            if (bonus == null) return;
            if (bonus.Type == BloodlineBonusType.ClanDiscipline ||
                bonus.Type == BloodlineBonusType.RegularDiscipline ||
                bonus.Type == BloodlineBonusType.Amalgam)
            {
                if (!string.IsNullOrEmpty(bonus.ReferenceId))
                    set.Add(bonus.ReferenceId);
            }
        }

        // ============================================================
        // Доступность
        // ============================================================

        private bool BonusGivesDiscipline(BloodlineBonus bonus, string discId)
        {
            if (bonus == null) return false;
            if (bonus.Type == BloodlineBonusType.ClanDiscipline ||
                bonus.Type == BloodlineBonusType.RegularDiscipline ||
                bonus.Type == BloodlineBonusType.Amalgam)
                return bonus.ReferenceId == discId;
            return false;
        }

        private HashSet<string> ComputeClanDisciplineIds()
        {
            var clanSet = new HashSet<string>();

            var clan = Chr.FindClan(Draft.ClanId);
            if (clan != null && clan.ClanDisciplineIds != null)
                foreach (var id in clan.ClanDisciplineIds)
                    if (!string.IsNullOrEmpty(id)) clanSet.Add(id);

            if (!string.IsNullOrEmpty(Draft.BloodlineId))
            {
                var bl = Chr.FindBloodline(Draft.BloodlineId);
                if (bl != null)
                {
                    CollectClanBonusRef(bl.PrimaryBonus, clanSet);
                    if (bl.ExtraBonuses != null)
                        foreach (var b in bl.ExtraBonuses)
                            CollectClanBonusRef(b, clanSet);
                }
            }

            foreach (var slot in CollectFreeDiscSlots())
            {
                if (!slot.IsClan) continue;
                if (Draft.FreeDiscChoices.TryGetValue(slot.Key, out var chosen) &&
                    !string.IsNullOrEmpty(chosen))
                    clanSet.Add(chosen);
            }

            return clanSet;
        }

        private HashSet<string> ComputeAvailableDisciplineIds(HashSet<string> clanDisciplines)
        {
            var available = new HashSet<string>(clanDisciplines);

            if (!string.IsNullOrEmpty(Draft.BloodlineId))
            {
                var bl = Chr.FindBloodline(Draft.BloodlineId);
                if (bl != null)
                {
                    CollectAnyBonusRef(bl.PrimaryBonus, available);
                    if (bl.ExtraBonuses != null)
                        foreach (var b in bl.ExtraBonuses)
                            CollectAnyBonusRef(b, available);
                }
            }

            if (Draft.MeritIds != null)
            {
                foreach (var meritId in Draft.MeritIds)
                {
                    var merit = Chr.FindMerit(meritId);
                    if (merit == null || merit.Modifiers == null) continue;

                    foreach (var mod in merit.Modifiers)
                    {
                        if (string.IsNullOrEmpty(mod.Target)) continue;
                        if (!mod.Target.StartsWith("disc.")) continue;
                        if (mod.Value <= 0) continue;

                        string discId = mod.Target.Substring(5);
                        if (discId == "clan.free" || discId == "regular.free") continue;

                        if (!string.IsNullOrEmpty(discId))
                            available.Add(discId);
                    }
                }
            }

            foreach (var slot in CollectFreeDiscSlots())
            {
                if (slot.IsClan) continue;
                if (Draft.FreeDiscChoices.TryGetValue(slot.Key, out var chosen) &&
                    !string.IsNullOrEmpty(chosen))
                    available.Add(chosen);
            }

            return available;
        }

        private void CollectClanBonusRef(BloodlineBonus bonus, HashSet<string> set)
        {
            if (bonus == null) return;
            if (bonus.Type != BloodlineBonusType.ClanDiscipline) return;
            if (!string.IsNullOrEmpty(bonus.ReferenceId))
                set.Add(bonus.ReferenceId);
        }

        private void CollectAnyBonusRef(BloodlineBonus bonus, HashSet<string> set)
        {
            if (bonus == null) return;
            if (bonus.Type == BloodlineBonusType.ClanDiscipline ||
                bonus.Type == BloodlineBonusType.RegularDiscipline ||
                bonus.Type == BloodlineBonusType.Amalgam)
            {
                if (!string.IsNullOrEmpty(bonus.ReferenceId))
                    set.Add(bonus.ReferenceId);
            }
        }

        /// <summary>
        /// Сбрасывает очки, вложенные в дисциплины, которые больше недоступны
        /// (например, черта-источник удалена или выбор в слоте изменён).
        /// Не трогает пути и амальгамы — они обрабатываются отдельно.
        /// </summary>
        private void ResetOrphanedDisciplines()
        {
            if (Draft.Disciplines == null) return;

            var clanDisc = ComputeClanDisciplineIds();
            var available = ComputeAvailableDisciplineIds(clanDisc);

            foreach (var disc in Chr.Disciplines)
            {
                if (disc.IsPath || disc.IsAmalgam) continue;

                if (!available.Contains(disc.Id))
                {
                    if (Draft.Disciplines.TryGetValue(disc.Id, out int lvl) && lvl > 0)
                    {
                        Draft.Disciplines[disc.Id] = 0;
                        Debug.Log($"[Wizard] Сброшено {lvl} очков дисциплины «{disc.Name}» — больше не доступна.");
                    }
                }
            }
        }

        // ============================================================
        // Строки
        // ============================================================

        private VisualElement BuildDisciplineRow(Rank rank, Discipline disc)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            int meritLvl = GetDisciplineMeritLevel(disc.Id);

            var nameLbl = new Label(disc.Name);
            nameLbl.style.fontSize = 14;
            nameLbl.style.color = UIRoot.TextPrimary;
            nameLbl.style.minWidth = 240;
            nameLbl.style.marginRight = 12;
            row.Add(nameLbl);

            var minusBtn = new Button();
            minusBtn.text = "−";
            minusBtn.style.width = 36;
            minusBtn.style.height = 30;
            minusBtn.style.fontSize = 16;
            minusBtn.style.backgroundColor = UIRoot.AccentRed;
            minusBtn.style.color = Color.white;
            minusBtn.style.borderTopLeftRadius = 4;
            minusBtn.style.borderTopRightRadius = 4;
            minusBtn.style.borderBottomLeftRadius = 4;
            minusBtn.style.borderBottomRightRadius = 4;
            minusBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
            minusBtn.clicked += () => ChangeDisciplineLevel(rank, disc.Id, -1);
            row.Add(minusBtn);
            _discMinusButtons[disc.Id] = minusBtn;

            var valueLbl = new Label("0");
            valueLbl.style.fontSize = 16;
            valueLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueLbl.style.color = UIRoot.TextPrimary;
            valueLbl.style.minWidth = 80;
            valueLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueLbl.style.marginLeft = 6;
            valueLbl.style.marginRight = 6;
            row.Add(valueLbl);
            _discValueLabels[disc.Id] = valueLbl;

            var plusBtn = new Button();
            plusBtn.text = "+";
            plusBtn.style.width = 36;
            plusBtn.style.height = 30;
            plusBtn.style.fontSize = 16;
            plusBtn.style.backgroundColor = UIRoot.AccentGreen;
            plusBtn.style.color = Color.white;
            plusBtn.style.borderTopLeftRadius = 4;
            plusBtn.style.borderTopRightRadius = 4;
            plusBtn.style.borderBottomLeftRadius = 4;
            plusBtn.style.borderBottomRightRadius = 4;
            plusBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
            plusBtn.clicked += () => ChangeDisciplineLevel(rank, disc.Id, +1);
            row.Add(plusBtn);
            _discPlusButtons[disc.Id] = plusBtn;

            if (meritLvl > 0)
            {
                var meritLbl = UIRoot.MakeMuted($"★ от черты +{meritLvl}", 11);
                meritLbl.style.marginLeft = 12;
                meritLbl.style.color = new Color(0.95f, 0.8f, 0.35f);
                row.Add(meritLbl);
            }

            return row;
        }

        private VisualElement BuildPathRow(Rank rank, Discipline path)
        {
            var container = new VisualElement();
            container.style.marginBottom = 6;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            container.Add(row);

            int meritLvl = GetDisciplineMeritLevel(path.Id);

            var nameLbl = new Label(path.Name);
            nameLbl.style.fontSize = 14;
            nameLbl.style.color = UIRoot.TextPrimary;
            nameLbl.style.minWidth = 240;
            nameLbl.style.marginRight = 12;
            row.Add(nameLbl);

            int parentEffective = GetEffectiveDisciplineLevel(path.ParentDisciplineId);
            bool parentOk = parentEffective >= 1;

            var minusBtn = new Button();
            minusBtn.text = "−";
            minusBtn.style.width = 36;
            minusBtn.style.height = 30;
            minusBtn.style.fontSize = 16;
            minusBtn.style.backgroundColor = UIRoot.AccentRed;
            minusBtn.style.color = Color.white;
            minusBtn.style.borderTopLeftRadius = 4;
            minusBtn.style.borderTopRightRadius = 4;
            minusBtn.style.borderBottomLeftRadius = 4;
            minusBtn.style.borderBottomRightRadius = 4;
            minusBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
            minusBtn.clicked += () => ChangeDisciplineLevel(rank, path.Id, -1);
            row.Add(minusBtn);
            _discMinusButtons[path.Id] = minusBtn;

            var valueLbl = new Label("0");
            valueLbl.style.fontSize = 16;
            valueLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueLbl.style.color = UIRoot.TextPrimary;
            valueLbl.style.minWidth = 80;
            valueLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueLbl.style.marginLeft = 6;
            valueLbl.style.marginRight = 6;
            row.Add(valueLbl);
            _discValueLabels[path.Id] = valueLbl;

            var plusBtn = new Button();
            plusBtn.text = "+";
            plusBtn.style.width = 36;
            plusBtn.style.height = 30;
            plusBtn.style.fontSize = 16;
            plusBtn.style.backgroundColor = UIRoot.AccentGreen;
            plusBtn.style.color = Color.white;
            plusBtn.style.borderTopLeftRadius = 4;
            plusBtn.style.borderTopRightRadius = 4;
            plusBtn.style.borderBottomLeftRadius = 4;
            plusBtn.style.borderBottomRightRadius = 4;
            plusBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
            plusBtn.clicked += () => ChangeDisciplineLevel(rank, path.Id, +1);
            row.Add(plusBtn);
            _discPlusButtons[path.Id] = plusBtn;

            if (meritLvl > 0)
            {
                var meritLbl = UIRoot.MakeMuted($"★ от черты +{meritLvl}", 11);
                meritLbl.style.marginLeft = 12;
                meritLbl.style.color = new Color(0.95f, 0.8f, 0.35f);
                row.Add(meritLbl);
            }

            var parentDisc = Chr.FindDiscipline(path.ParentDisciplineId);
            string parentName = parentDisc != null ? parentDisc.Name : "?";
            var parentInfo = UIRoot.MakeMuted(
                parentOk
                    ? $"Путь «{parentName}» (уровень родителя: {parentEffective})"
                    : $"⚠ Требуется «{parentName}» уровня ≥ 1 (сейчас: {parentEffective})",
                11);
            parentInfo.style.marginLeft = 240;
            parentInfo.style.color = parentOk
                ? UIRoot.TextMuted
                : new Color(1f, 0.7f, 0.4f);
            container.Add(parentInfo);

            return container;
        }

        private VisualElement BuildAmalgamRow(Rank rank, Discipline am)
        {
            var container = new VisualElement();
            container.style.marginBottom = 8;
            container.style.paddingTop = 6;
            container.style.paddingBottom = 6;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.backgroundColor = new Color(0.14f, 0.14f, 0.18f);
            container.style.borderTopLeftRadius = 4;
            container.style.borderTopRightRadius = 4;
            container.style.borderBottomLeftRadius = 4;
            container.style.borderBottomRightRadius = 4;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            container.Add(row);

            var nameLbl = new Label(am.Name);
            nameLbl.style.fontSize = 14;
            nameLbl.style.color = UIRoot.TextPrimary;
            nameLbl.style.flexGrow = 1;
            row.Add(nameLbl);

            var costLbl = UIRoot.MakeMuted("(1 очко)", 11);
            costLbl.style.marginRight = 10;
            row.Add(costLbl);

            int meritLvl = GetDisciplineMeritLevel(am.Id);
            bool learned = GetEffectiveDisciplineLevel(am.Id) > 0;

            var toggleBtn = new Button();
            toggleBtn.text = learned
                ? (meritLvl > 0 ? "✓ от черты" : "✓ Изучена")
                : "Изучить";
            toggleBtn.style.height = 30;
            toggleBtn.style.minWidth = 130;
            toggleBtn.style.fontSize = 13;
            toggleBtn.style.paddingLeft = 12;
            toggleBtn.style.paddingRight = 12;
            toggleBtn.style.color = Color.white;
            toggleBtn.style.backgroundColor = learned
                ? UIRoot.AccentBlue
                : UIRoot.AccentGreen;
            toggleBtn.style.borderTopLeftRadius = 4;
            toggleBtn.style.borderTopRightRadius = 4;
            toggleBtn.style.borderBottomLeftRadius = 4;
            toggleBtn.style.borderBottomRightRadius = 4;
            toggleBtn.clicked += () => ToggleAmalgam(rank, am);
            row.Add(toggleBtn);
            _amalgamToggleButtons[am.Id] = toggleBtn;

            if (am.AmalgamRequirements != null && am.AmalgamRequirements.Count > 0)
            {
                var reqs = new System.Text.StringBuilder();
                bool allOk = true;
                foreach (var req in am.AmalgamRequirements)
                {
                    var reqDisc = Chr.FindDiscipline(req.DisciplineId);
                    string reqName = reqDisc != null ? reqDisc.Name : "?";
                    int have = GetEffectiveDisciplineLevel(req.DisciplineId);
                    bool ok = have >= req.MinLevel;
                    if (!ok) allOk = false;
                    if (reqs.Length > 0) reqs.Append("; ");
                    reqs.Append($"«{reqName}» ≥ {req.MinLevel} (у вас {have})");
                }
                var reqLbl = UIRoot.MakeMuted(
                    allOk ? $"Требования выполнены: {reqs}" : $"⚠ Требуется: {reqs}",
                    11);
                reqLbl.style.marginTop = 4;
                reqLbl.style.color = allOk
                    ? UIRoot.TextMuted
                    : new Color(1f, 0.7f, 0.4f);
                container.Add(reqLbl);
            }

            return container;
        }

        // ============================================================
        // Логика
        // ============================================================

        private void EnsureDisciplinesInitialized()
        {
            if (Draft.Disciplines == null)
                Draft.Disciplines = new Dictionary<string, int>();

            foreach (var d in Chr.Disciplines)
            {
                if (!Draft.Disciplines.ContainsKey(d.Id))
                    Draft.Disciplines[d.Id] = 0;
            }

            if (Draft.FreeDiscChoices == null)
                Draft.FreeDiscChoices = new Dictionary<string, string>();
        }

        private int GetDisciplineLevel(string discId)
        {
            if (string.IsNullOrEmpty(discId)) return 0;
            return Draft.Disciplines != null && Draft.Disciplines.TryGetValue(discId, out int v)
                ? v : 0;
        }

        private int GetDisciplineSpentTotal()
        {
            int total = 0;
            foreach (var kv in Draft.Disciplines)
            {
                if (kv.Value > 0)
                {
                    var disc = Chr.FindDiscipline(kv.Key);
                    if (disc != null && disc.IsAmalgam)
                        total += 1;
                    else
                        total += kv.Value;
                }
            }
            return total;
        }

        private void ChangeDisciplineLevel(Rank rank, string discId, int delta)
        {
            EnsureDisciplinesInitialized();
            var disc = Chr.FindDiscipline(discId);
            if (disc == null) return;

            int current = GetDisciplineLevel(discId);
            int newVal = current + delta;

            if (newVal < 0) return;

            int meritLvl = GetDisciplineMeritLevel(discId);
            int hardMax = GetDisciplineHardCap(rank);
            if (newVal + meritLvl > hardMax) return;

            if (delta > 0)
            {
                int spent = GetDisciplineSpentTotal();
                if (spent >= rank.DisciplinePoints) return;

                if (disc.IsPath)
                {
                    int parentEff = GetEffectiveDisciplineLevel(disc.ParentDisciplineId);
                    if (parentEff < 1) return;
                }
            }

            Draft.Disciplines[discId] = newVal;

            if (delta < 0)
                CleanupInvalidPaths();

            UpdateAllDisciplineLabels(rank);
        }

        private void ToggleAmalgam(Rank rank, Discipline am)
        {
            EnsureDisciplinesInitialized();
            int current = GetEffectiveDisciplineLevel(am.Id);
            bool learned = current > 0;
            int meritLvl = GetDisciplineMeritLevel(am.Id);

            if (learned)
            {
                if (meritLvl > 0) return;   // нельзя снять то, что дала черта
                Draft.Disciplines[am.Id] = 0;
            }
            else
            {
                int spent = GetDisciplineSpentTotal();
                if (spent >= rank.DisciplinePoints) return;

                if (!CheckAmalgamRequirements(am)) return;

                Draft.Disciplines[am.Id] = 1;
            }

            UpdateAllDisciplineLabels(rank);
        }

        private bool CheckAmalgamRequirements(Discipline am)
        {
            if (am.AmalgamRequirements == null || am.AmalgamRequirements.Count == 0)
                return true;

            foreach (var req in am.AmalgamRequirements)
            {
                if (GetEffectiveDisciplineLevel(req.DisciplineId) < req.MinLevel)
                    return false;
            }
            return true;
        }

        private void CleanupInvalidPaths()
        {
            if (Draft.Disciplines == null) return;

            foreach (var disc in Chr.Disciplines)
            {
                if (!disc.IsPath) continue;
                if (GetDisciplineLevel(disc.Id) <= 0) continue;

                if (GetEffectiveDisciplineLevel(disc.ParentDisciplineId) < 1)
                {
                    Draft.Disciplines[disc.Id] = 0;
                    Debug.Log($"[Wizard] Путь «{disc.Name}» сброшен (нет родителя уровня ≥ 1).");
                }
            }
        }

        private void UpdateAllDisciplineLabels(Rank rank)
        {
            int hardMax = GetDisciplineHardCap(rank);

            foreach (var disc in Chr.Disciplines)
            {
                int playerLvl = GetDisciplineLevel(disc.Id);
                int meritLvl = GetDisciplineMeritLevel(disc.Id);
                int effective = playerLvl + meritLvl;

                if (_discValueLabels.TryGetValue(disc.Id, out var vl))
                {
                    vl.text = meritLvl == 0
                        ? playerLvl.ToString()
                        : $"{effective} (★{meritLvl})";
                    vl.style.color = effective == 0
                        ? UIRoot.TextMuted
                        : UIRoot.TextPrimary;
                }

                if (_discMinusButtons.TryGetValue(disc.Id, out var mb))
                    mb.SetEnabled(playerLvl > 0);

                if (_discPlusButtons.TryGetValue(disc.Id, out var pb))
                {
                    bool hasRoom = GetDisciplineSpentTotal() < rank.DisciplinePoints;
                    bool withinHardMax = (playerLvl + 1 + meritLvl) <= hardMax;

                    bool parentOk = true;
                    if (disc.IsPath)
                        parentOk = GetEffectiveDisciplineLevel(disc.ParentDisciplineId) >= 1;

                    pb.SetEnabled(hasRoom && parentOk && withinHardMax);
                }

                if (_amalgamToggleButtons.TryGetValue(disc.Id, out var ab))
                {
                    bool learned = effective > 0;
                    bool hasRoom = GetDisciplineSpentTotal() < rank.DisciplinePoints;
                    bool reqsOk = CheckAmalgamRequirements(disc);
                    bool fromMerit = meritLvl > 0;

                    ab.text = learned
                        ? (fromMerit ? "✓ от черты" : "✓ Изучена")
                        : "Изучить";
                    ab.style.backgroundColor = learned
                        ? UIRoot.AccentBlue
                        : UIRoot.AccentGreen;

                    ab.SetEnabled(fromMerit ? false : (learned || (hasRoom && reqsOk)));
                }
            }

            if (_discCounterLabel != null)
            {
                int spent = GetDisciplineSpentTotal();
                int limit = rank.DisciplinePoints;
                _discCounterLabel.text = $"Потрачено очков дисциплин: {spent} / {limit}";
                _discCounterLabel.style.color = spent == limit
                    ? new Color(0.55f, 0.85f, 0.55f)
                    : new Color(0.95f, 0.7f, 0.3f);
            }

            if (_totalDiscSummary != null)
            {
                int spent = GetDisciplineSpentTotal();
                int limit = rank.DisciplinePoints;
                _totalDiscSummary.text = $"Всего распределено: {spent} / {limit} очков дисциплин.";
            }
        }

        // ============================================================
        // Валидация
        // ============================================================

        private bool ValidateDisciplinesStep(out string error)
        {
            error = "";
            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null) { error = "Ранг не выбран."; return false; }

            EnsureDisciplinesInitialized();

            foreach (var slot in CollectFreeDiscSlots())
            {
                if (!Draft.FreeDiscChoices.TryGetValue(slot.Key, out var chosen) ||
                    string.IsNullOrEmpty(chosen))
                {
                    string kind = slot.IsClan ? "клановую" : "неклановую";
                    error = $"Выберите {kind} дисциплину для черты «{slot.MeritName}».";
                    return false;
                }
            }

            int spent = GetDisciplineSpentTotal();
            if (spent > rank.DisciplinePoints)
            {
                error = $"Потрачено больше очков дисциплин, чем доступно ({spent}/{rank.DisciplinePoints}).";
                return false;
            }

            foreach (var disc in Chr.Disciplines)
            {
                int eff = GetEffectiveDisciplineLevel(disc.Id);
                if (eff <= 0) continue;

                if (disc.IsPath)
                {
                    if (GetEffectiveDisciplineLevel(disc.ParentDisciplineId) < 1)
                    {
                        error = $"Путь «{disc.Name}» требует родительскую дисциплину уровня ≥ 1.";
                        return false;
                    }
                }

                if (disc.IsAmalgam && !CheckAmalgamRequirements(disc))
                {
                    error = $"У амальгамы «{disc.Name}» не выполнены требования.";
                    return false;
                }
            }

            return true;
        }
    }
}