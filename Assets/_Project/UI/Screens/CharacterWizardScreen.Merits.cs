using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;

namespace VTR.UI
{
    public partial class CharacterWizardScreen
    {
        private const int ADDON_MAX_LEVEL = 5;

        // ============================================================
        // ШАГ 4: Черты, дополнения, мораль
        // ============================================================

        private void BuildStep_MeritsAndMorality(VisualElement parent)
        {
            parent.Add(UIRoot.MakeHeader("Шаг 4: Черты, дополнения, мораль"));

            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null)
            {
                parent.Add(UIRoot.MakeLabel("Ранг не выбран. Вернитесь на шаг 1.",
                    14, new Color(1f, 0.55f, 0.55f)));
                return;
            }

            BuildMeritsSection(parent, rank);
            BuildAddonsSection(parent, rank);
            BuildMoralitySection(parent);

            var totalSection = UIWidgets.Section("Итог");
            int meritSpent = GetSpentMeritPoints();
            int meritLimit = rank.MeritPoints;
            int addonSpent = GetSpentAddonPoints();
            int addonLimit = rank.AddonPoints;

            totalSection.Add(UIRoot.MakeMuted(
                $"Очки черт: {meritSpent} / {meritLimit}. " +
                $"Очки дополнений: {addonSpent} / {addonLimit}."));

            if (!string.IsNullOrEmpty(Draft.MoralityId))
            {
                var mor = Chr.FindMorality(Draft.MoralityId);
                if (mor != null)
                    totalSection.Add(UIRoot.MakeMuted($"Мораль: {mor.Name} (старт {mor.StartRating})"));
            }
            parent.Add(totalSection);

            RebuildMeritsList(rank);
            RebuildAddonsList(rank);
        }

        // ============================================================
        // ЧЕРТЫ
        // ============================================================

        private void BuildMeritsSection(VisualElement parent, Rank rank)
        {
            var section = UIWidgets.Section("Черты");

            _meritSpentLabel = UIRoot.MakeLabel("", 14);
            _meritSpentLabel.style.marginBottom = 6;
            section.Add(_meritSpentLabel);

            section.Add(UIRoot.MakeMuted(
                "Достоинства стоят положительных очков, недостатки дают отрицательные. " +
                "Суммарная стоимость всех выбранных черт не должна превышать лимит.", 12));

            _meritsListContainer = new VisualElement();
            section.Add(_meritsListContainer);

            parent.Add(section);
        }

        private void RebuildMeritsList(Rank rank)
        {
            if (_meritsListContainer == null) return;
            _meritsListContainer.Clear();

            if (Draft.MeritIds == null) Draft.MeritIds = new List<string>();

            int spent = GetSpentMeritPoints();
            int limit = rank.MeritPoints;

            if (_meritSpentLabel != null)
            {
                _meritSpentLabel.text = $"Потрачено: {spent} / {limit}";
                _meritSpentLabel.style.color = spent <= limit
                    ? (spent == limit
                        ? new Color(0.55f, 0.85f, 0.55f)
                        : new Color(0.95f, 0.7f, 0.3f))
                    : new Color(1f, 0.55f, 0.55f);
            }

            if (Draft.MeritIds.Count == 0)
            {
                _meritsListContainer.Add(UIRoot.MakeMuted("(пока черты не выбраны)"));
            }
            else
            {
                // Группируем одинаковые черты для отображения счётчика.
                var counts = new Dictionary<string, int>();
                var order = new List<string>();
                foreach (var id in Draft.MeritIds)
                {
                    if (!counts.ContainsKey(id))
                    {
                        counts[id] = 0;
                        order.Add(id);
                    }
                    counts[id]++;
                }

                foreach (var meritId in order)
                {
                    string idLocal = meritId;
                    var merit = Chr.FindMerit(idLocal);
                    if (merit == null) continue;

                    int count = counts[idLocal];

                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.Center;
                    row.style.marginBottom = 4;
                    row.style.paddingTop = 4;
                    row.style.paddingBottom = 4;
                    row.style.paddingLeft = 10;
                    row.style.paddingRight = 10;
                    row.style.backgroundColor = new Color(0.16f, 0.16f, 0.20f);
                    row.style.borderTopLeftRadius = 4;
                    row.style.borderTopRightRadius = 4;
                    row.style.borderBottomLeftRadius = 4;
                    row.style.borderBottomRightRadius = 4;

                    var nameLbl = new Label(merit.Name + (count > 1 ? $" ×{count}" : ""));
                    nameLbl.style.flexGrow = 1;
                    nameLbl.style.fontSize = 14;
                    nameLbl.style.color = UIRoot.TextPrimary;
                    row.Add(nameLbl);

                    if (MeritHasZeroAttrWarning(merit))
                    {
                        var warnLbl = new Label("⚠");
                        warnLbl.style.color = new Color(1f, 0.55f, 0.55f);
                        warnLbl.style.fontSize = 14;
                        warnLbl.style.marginRight = 6;
                        row.Add(warnLbl);
                    }

                    string costStr = merit.Cost > 0 ? $"+{merit.Cost}" :
                                     (merit.Cost < 0 ? merit.Cost.ToString() : "0");
                    var costLbl = new Label(costStr);
                    costLbl.style.fontSize = 14;
                    costLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                    costLbl.style.color = merit.Cost > 0
                        ? new Color(0.95f, 0.7f, 0.4f)
                        : (merit.Cost < 0
                            ? new Color(0.55f, 0.85f, 0.55f)
                            : UIRoot.TextMuted);
                    costLbl.style.marginRight = 12;
                    row.Add(costLbl);

                    var descBtn = new Button(() => ShowMeritDescriptionDialog(merit));
                    descBtn.text = "?";
                    descBtn.style.width = 28;
                    descBtn.style.height = 24;
                    descBtn.style.fontSize = 12;
                    descBtn.style.backgroundColor = UIRoot.AccentNeutral;
                    descBtn.style.color = Color.white;
                    descBtn.style.marginRight = 4;
                    descBtn.style.borderTopLeftRadius = 4;
                    descBtn.style.borderTopRightRadius = 4;
                    descBtn.style.borderBottomLeftRadius = 4;
                    descBtn.style.borderBottomRightRadius = 4;
                    row.Add(descBtn);

                    var xBtn = new Button(() => RemoveMerit(idLocal));
                    xBtn.text = "✕";
                    xBtn.style.width = 28;
                    xBtn.style.height = 24;
                    xBtn.style.fontSize = 12;
                    xBtn.style.backgroundColor = UIRoot.AccentRed;
                    xBtn.style.color = Color.white;
                    xBtn.style.borderTopLeftRadius = 4;
                    xBtn.style.borderTopRightRadius = 4;
                    xBtn.style.borderBottomLeftRadius = 4;
                    xBtn.style.borderBottomRightRadius = 4;
                    row.Add(xBtn);

                    _meritsListContainer.Add(row);
                }
            }

            // Доступные: черта не взята ИЛИ может быть взята ещё раз.
            var availableMerits = new List<Merit>();
            foreach (var m in Chr.Merits)
            {
                bool alreadyTaken = Draft.MeritIds.Contains(m.Id);
                if (alreadyTaken && !m.CanBeLearnedMultipleTimes) continue;
                availableMerits.Add(m);
            }

            bool canAdd = availableMerits.Count > 0;
            var addBtn = new Button(() => ShowAddMeritDialog(rank, availableMerits));
            addBtn.text = "＋  Добавить черту";
            UIRoot.StyleButton(addBtn, canAdd ? UIRoot.AccentGreen : UIRoot.AccentNeutral,
                Color.white, 32);
            addBtn.style.marginTop = 6;
            addBtn.SetEnabled(canAdd);
            _meritsListContainer.Add(addBtn);
        }

        private void ShowMeritDescriptionDialog(Merit merit)
        {
            var panelRoot = UIRoot.PanelRoot;
            if (panelRoot == null) return;

            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.55f);
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;

            var dialog = new VisualElement();
            dialog.style.width = 520;
            dialog.style.maxHeight = 640;
            dialog.style.paddingTop = 18;
            dialog.style.paddingBottom = 18;
            dialog.style.paddingLeft = 18;
            dialog.style.paddingRight = 18;
            dialog.style.backgroundColor = UIRoot.BgPanel;
            dialog.style.borderTopLeftRadius = 8;
            dialog.style.borderTopRightRadius = 8;
            dialog.style.borderBottomLeftRadius = 8;
            dialog.style.borderBottomRightRadius = 8;

            var title = new Label(merit.Name);
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = UIRoot.TextPrimary;
            title.style.marginBottom = 4;
            dialog.Add(title);

            string costStr = merit.Cost > 0 ? $"+{merit.Cost}" :
                             (merit.Cost < 0 ? merit.Cost.ToString() : "0");
            string multiStr = merit.CanBeLearnedMultipleTimes ? "  (можно взять несколько раз)" : "";
            var costLbl = new Label($"Стоимость: {costStr}{multiStr}");
            costLbl.style.fontSize = 13;
            costLbl.style.color = merit.Cost > 0
                ? new Color(0.95f, 0.7f, 0.4f)
                : (merit.Cost < 0
                    ? new Color(0.55f, 0.85f, 0.55f)
                    : UIRoot.TextMuted);
            costLbl.style.marginBottom = 10;
            dialog.Add(costLbl);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.minHeight = 150;
            scroll.style.maxHeight = 460;
            dialog.Add(scroll);

            if (!string.IsNullOrEmpty(merit.Description))
            {
                var descLbl = new Label(merit.Description);
                descLbl.style.fontSize = 14;
                descLbl.style.color = UIRoot.TextPrimary;
                descLbl.style.whiteSpace = WhiteSpace.Normal;
                descLbl.style.marginBottom = 10;
                scroll.Add(descLbl);
            }

            if (merit.Modifiers != null && merit.Modifiers.Count > 0)
            {
                var header = new Label("Модификаторы:");
                header.style.fontSize = 14;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = UIRoot.TextPrimary;
                header.style.marginTop = 6;
                header.style.marginBottom = 4;
                scroll.Add(header);

                foreach (var mod in merit.Modifiers)
                {
                    string valStr = mod.Value > 0 ? $"+{mod.Value}" :
                                    (mod.Value < 0 ? mod.Value.ToString() : "0");
                    string text = $"• {DescribeModifierTarget(mod.Target)} {valStr}";
                    var l = new Label(text);
                    l.style.fontSize = 13;
                    l.style.color = mod.Value > 0
                        ? new Color(0.55f, 0.85f, 0.55f)
                        : (mod.Value < 0
                            ? new Color(0.95f, 0.55f, 0.55f)
                            : UIRoot.TextMuted);
                    l.style.whiteSpace = WhiteSpace.Normal;
                    l.style.marginBottom = 2;
                    scroll.Add(l);
                }
            }

            if (MeritHasZeroAttrWarning(merit))
            {
                var warnLbl = new Label("⚠ Один из атрибутов персонажа станет равен 0. Крайне не рекомендуется.");
                warnLbl.style.fontSize = 13;
                warnLbl.style.color = new Color(1f, 0.55f, 0.55f);
                warnLbl.style.whiteSpace = WhiteSpace.Normal;
                warnLbl.style.marginTop = 12;
                warnLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                scroll.Add(warnLbl);
            }

            var closeBtn = new Button(() => panelRoot.Remove(overlay));
            closeBtn.text = "Закрыть";
            UIRoot.StyleButton(closeBtn, UIRoot.AccentNeutral, Color.white, 34);
            closeBtn.style.marginTop = 10;
            dialog.Add(closeBtn);

            overlay.Add(dialog);
            overlay.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == overlay) panelRoot.Remove(overlay);
            });
            panelRoot.Add(overlay);
        }

        private string DescribeModifierTarget(string target)
        {
            if (string.IsNullOrEmpty(target)) return "?";

            if (target == MeritSpecialTargets.FreeClanDiscipline)
                return "Клановая дисциплина (выбор игрока)";
            if (target == MeritSpecialTargets.FreeRegularDiscipline)
                return "Неклановая дисциплина (выбор игрока)";

            if (target.StartsWith("attr."))
            {
                var a = Chr.FindAttribute(target.Substring(5));
                return $"Атрибут: {(a != null ? a.Name : target.Substring(5))}";
            }
            if (target.StartsWith("skill."))
            {
                var s = Chr.FindSkill(target.Substring(6));
                return $"Навык: {(s != null ? Chr.GetSkillName(s) : target.Substring(6))}";
            }
            if (target.StartsWith("disc."))
            {
                var d = Chr.FindDiscipline(target.Substring(5));
                return $"Дисциплина: {(d != null ? d.Name : target.Substring(5))}";
            }

            switch (target)
            {
                case "willpower.max": return "Система: Макс. сила воли";
                case "willpower.current": return "Система: Текущая сила воли";
                case "blood.max": return "Система: Макс. запас крови";
                case "blood.current": return "Система: Текущий запас крови";
                case "morality": return "Система: Мораль";
                case "aura": return "Система: Аура";
                case "health.max": return "Система: Макс. здоровье";
                case "blood.potency": return "Система: Густота крови";
            }
            return target;
        }

        private bool MeritHasZeroAttrWarning(Merit merit)
        {
            if (merit.Modifiers == null) return false;
            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null) return false;

            foreach (var mod in merit.Modifiers)
            {
                if (string.IsNullOrEmpty(mod.Target)) continue;
                if (!mod.Target.StartsWith("attr.")) continue;

                string attrId = mod.Target.Substring(5);
                int baseVal = Draft.Attributes.TryGetValue(attrId, out var b)
                    ? b : GetAttrBaseValue(attrId);
                int currentBonus = GetMeritBonus(mod.Target);
                int newEffective = baseVal + currentBonus + mod.Value;

                if (newEffective == 0) return true;
            }
            return false;
        }

        // ============================================================
        // Модалка выбора черты
        // ============================================================

        private void ShowAddMeritDialog(Rank rank, List<Merit> available)
        {
            var panelRoot = UIRoot.PanelRoot;
            if (panelRoot == null) return;

            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.55f);
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;

            var dialog = new VisualElement();
            dialog.style.width = 620;
            dialog.style.maxHeight = 680;
            dialog.style.paddingTop = 16;
            dialog.style.paddingBottom = 16;
            dialog.style.paddingLeft = 16;
            dialog.style.paddingRight = 16;
            dialog.style.backgroundColor = UIRoot.BgPanel;
            dialog.style.borderTopLeftRadius = 8;
            dialog.style.borderTopRightRadius = 8;
            dialog.style.borderBottomLeftRadius = 8;
            dialog.style.borderBottomRightRadius = 8;

            var title = new Label("Выбор черты");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = UIRoot.TextPrimary;
            title.style.marginBottom = 8;
            dialog.Add(title);

            var searchField = new TextField();
            searchField.value = "";
            UIRoot.StyleTextField(searchField, "");
            dialog.Add(searchField);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.minHeight = 260;
            scroll.style.maxHeight = 460;
            dialog.Add(scroll);

            void RebuildList(string query)
            {
                scroll.Clear();
                string q = (query ?? "").Trim().ToLowerInvariant();

                int shown = 0;
                foreach (var m in available)
                {
                    if (!string.IsNullOrEmpty(q) &&
                        !m.Name.ToLowerInvariant().Contains(q))
                        continue;

                    var merit = m;
                    bool canTake = CanTakeMerit(merit, out string reason, out bool hasWarning);

                    var row = new Button(() =>
                    {
                        if (!canTake)
                        {
                            Debug.LogWarning($"[Wizard] Нельзя взять «{merit.Name}»: {reason}");
                            return;
                        }
                        AddMerit(merit.Id);
                        panelRoot.Remove(overlay);
                    });
                    int takenCount = 0;
                    foreach (var id in Draft.MeritIds)
                        if (id == merit.Id) takenCount++;
                    string countStr = takenCount > 0 ? $" [взято {takenCount}]" : "";
                    row.text = $"{merit.Name}{countStr}   ({(merit.Cost > 0 ? "+" : "")}{merit.Cost})";
                    if (!canTake) row.text += $"   ⚠ {reason}";
                    else if (hasWarning) row.text += "   ⚠ обнулит атрибут";

                    row.style.height = 32;
                    row.style.fontSize = 14;
                    row.style.color = UIRoot.TextPrimary;
                    row.style.unityTextAlign = TextAnchor.MiddleLeft;
                    row.style.paddingLeft = 12;
                    row.style.marginBottom = 3;
                    row.style.backgroundColor = canTake
                        ? UIRoot.AccentNeutral
                        : new Color(0.22f, 0.16f, 0.16f);
                    row.style.borderTopLeftRadius = 4;
                    row.style.borderTopRightRadius = 4;
                    row.style.borderBottomLeftRadius = 4;
                    row.style.borderBottomRightRadius = 4;
                    row.SetEnabled(canTake);
                    scroll.Add(row);
                    shown++;
                }

                if (shown == 0)
                    scroll.Add(UIRoot.MakeMuted("Ничего не найдено."));
            }

            searchField.RegisterValueChangedCallback(e => RebuildList(e.newValue));

            var cancelBtn = new Button(() => panelRoot.Remove(overlay));
            cancelBtn.text = "Отмена";
            UIRoot.StyleButton(cancelBtn, UIRoot.AccentNeutral, Color.white, 32);
            cancelBtn.style.marginTop = 8;
            dialog.Add(cancelBtn);

            overlay.Add(dialog);
            overlay.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == overlay) panelRoot.Remove(overlay);
            });
            panelRoot.Add(overlay);

            RebuildList("");
            searchField.schedule.Execute(() => searchField.Focus()).ExecuteLater(20);
        }

        private void AddMerit(string meritId)
        {
            var merit = Chr.FindMerit(meritId);
            if (merit == null) return;

            if (!CanTakeMerit(merit, out string error, out bool warning))
            {
                Debug.LogWarning($"[Wizard] Нельзя взять «{merit.Name}»: {error}");
                return;
            }

            if (Draft.MeritIds == null) Draft.MeritIds = new List<string>();
            Draft.MeritIds.Add(meritId);

            if (warning)
                Debug.LogWarning($"[Wizard] Внимание: черта «{merit.Name}» обнулит один из атрибутов.");

            // Черта могла добавить новые дисциплины/слоты — и это не должно ничего ломать,
            // но на всякий случай перепроверим пути.
            CleanupInvalidPaths();

            var rank = Chr.FindRank(Draft.RankId);
            if (rank != null) RebuildMeritsList(rank);
        }

        private void RemoveMerit(string meritId)
        {
            if (Draft.MeritIds == null) return;

            int idx = Draft.MeritIds.LastIndexOf(meritId);
            if (idx >= 0) Draft.MeritIds.RemoveAt(idx);

            // 1. Чистим выборы в свободных слотах (по актуальному набору слотов).
            CleanupOrphanedFreeDiscChoices();

            // 2. Обнуляем очки дисциплин, которые стали недоступны.
            ResetOrphanedDisciplines();

            // 3. Сбрасываем пути, у которых родитель упал ниже 1.
            CleanupInvalidPaths();

            var rank = Chr.FindRank(Draft.RankId);
            if (rank != null) RebuildMeritsList(rank);
        }

        // ============================================================
        // CanTakeMerit
        // ============================================================

        private bool CanTakeMerit(Merit merit, out string error, out bool warning)
        {
            error = "";
            warning = false;

            if (merit == null || merit.Modifiers == null) return true;

            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null) { error = "ранг не выбран"; return false; }

            // Если черта не мультиплицируема и уже взята — блок.
            bool alreadyTaken = Draft.MeritIds != null && Draft.MeritIds.Contains(merit.Id);
            if (alreadyTaken && !merit.CanBeLearnedMultipleTimes)
            {
                error = "уже взята";
                return false;
            }

            int hardMaxAttr = GetAttributeHardCap(rank);
            int hardMaxSkill = GetSkillHardCap(rank);

            // Считаем, сколько раз берётся (для мультиплицируемых).
            int multiplier = 1;
            if (alreadyTaken && merit.CanBeLearnedMultipleTimes)
                multiplier = 1; // добавляем ещё одно вхождение

            foreach (var mod in merit.Modifiers)
            {
                if (string.IsNullOrEmpty(mod.Target)) continue;

                int totalModValue = mod.Value * multiplier;

                if (mod.Target.StartsWith("attr."))
                {
                    string attrId = mod.Target.Substring(5);
                    int cur = Draft.Attributes.TryGetValue(attrId, out var b)
                        ? b : GetAttrBaseValue(attrId);
                    int curBonus = GetMeritBonus(mod.Target);
                    int final = cur + curBonus + totalModValue;

                    var a = Chr.FindAttribute(attrId);
                    string name = a != null ? a.Name : attrId;

                    if (final < 0) { error = $"«{name}» станет {final}"; return false; }
                    if (final > hardMaxAttr) { error = $"«{name}» = {final} > {hardMaxAttr}"; return false; }
                    if (final == 0) warning = true;
                }
                else if (mod.Target.StartsWith("skill."))
                {
                    string skillId = mod.Target.Substring(6);
                    int cur = Draft.Skills.TryGetValue(skillId, out var b) ? b : 0;
                    int curBonus = GetMeritBonus(mod.Target);
                    int final = cur + curBonus + totalModValue;

                    var s = Chr.FindSkill(skillId);
                    string name = s != null ? Chr.GetSkillName(s) : skillId;

                    if (final < 0) { error = $"навык «{name}» станет {final}"; return false; }
                    if (final > hardMaxSkill) { error = $"навык «{name}» = {final} > {hardMaxSkill}"; return false; }
                }
                else if (mod.Target == "blood.potency")
                {
                    int cur = GetEffectiveBloodPotency(rank);
                    int final = cur + totalModValue;
                    if (final < 0) { error = "густота крови станет < 0"; return false; }
                    if (final > 10) { error = "густота крови > 10"; return false; }
                }
            }

            return true;
        }

        private int GetMeritBonus(string target)
        {
            int sum = 0;
            if (Draft.MeritIds == null || string.IsNullOrEmpty(target)) return 0;

            foreach (var meritId in Draft.MeritIds)
            {
                var m = Chr.FindMerit(meritId);
                if (m == null || m.Modifiers == null) continue;

                foreach (var mod in m.Modifiers)
                {
                    if (mod.Target == target) sum += mod.Value;
                }
            }
            return sum;
        }

        // ============================================================
        // ДОПОЛНЕНИЯ — теперь как очки 0..5
        // ============================================================

        private void BuildAddonsSection(VisualElement parent, Rank rank)
        {
            var section = UIWidgets.Section("Дополнения");

            _addonSpentLabel = UIRoot.MakeLabel("", 14);
            _addonSpentLabel.style.marginBottom = 6;
            section.Add(_addonSpentLabel);

            section.Add(UIRoot.MakeMuted(
                "Раскидайте очки дополнений по доступным позициям (0–5 в каждое). " +
                "Общий лимит — очки дополнений ранга.", 12));

            _addonsListContainer = new VisualElement();
            section.Add(_addonsListContainer);

            parent.Add(section);
        }

        private void RebuildAddonsList(Rank rank)
        {
            if (_addonsListContainer == null) return;
            _addonsListContainer.Clear();

            if (Draft.AddonLevels == null)
                Draft.AddonLevels = new Dictionary<string, int>();

            int spent = GetSpentAddonPoints();
            int limit = rank.AddonPoints;

            if (_addonSpentLabel != null)
            {
                _addonSpentLabel.text = $"Потрачено: {spent} / {limit}";
                _addonSpentLabel.style.color = spent <= limit
                    ? (spent == limit
                        ? new Color(0.55f, 0.85f, 0.55f)
                        : new Color(0.95f, 0.7f, 0.3f))
                    : new Color(1f, 0.55f, 0.55f);
            }

            var available = new List<Addon>();
            foreach (var a in Chr.Addons)
            {
                if (a.AvailableFromStart)
                    available.Add(a);
            }

            if (available.Count == 0)
            {
                _addonsListContainer.Add(UIRoot.MakeMuted(
                    "Мастер не разрешил ни одного дополнения на старте."));
                return;
            }

            foreach (var addon in available)
            {
                var a = addon;
                int level = Draft.AddonLevels.TryGetValue(a.Id, out int lv) ? lv : 0;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var nameLbl = new Label(a.Name);
                nameLbl.style.fontSize = 14;
                nameLbl.style.color = UIRoot.TextPrimary;
                nameLbl.style.minWidth = 260;
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
                minusBtn.clicked += () => ChangeAddonLevel(rank, a.Id, -1);
                row.Add(minusBtn);

                var valueLbl = new Label("0");
                valueLbl.style.fontSize = 16;
                valueLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                valueLbl.style.color = UIRoot.TextPrimary;
                valueLbl.style.minWidth = 50;
                valueLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                valueLbl.style.marginLeft = 6;
                valueLbl.style.marginRight = 6;
                valueLbl.text = level.ToString();
                row.Add(valueLbl);

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
                plusBtn.clicked += () => ChangeAddonLevel(rank, a.Id, +1);
                row.Add(plusBtn);

                var maxLbl = UIRoot.MakeMuted($"(макс. {ADDON_MAX_LEVEL})", 11);
                maxLbl.style.marginLeft = 12;
                row.Add(maxLbl);

                // Кнопки управления активны по ситуации
                minusBtn.SetEnabled(level > 0);
                plusBtn.SetEnabled(level < ADDON_MAX_LEVEL && spent < limit);

                _addonsListContainer.Add(row);
            }
        }

        private int GetSpentAddonPoints()
        {
            int sum = 0;
            if (Draft.AddonLevels == null) return 0;
            foreach (var kv in Draft.AddonLevels)
                sum += kv.Value;
            return sum;
        }

        private void ChangeAddonLevel(Rank rank, string addonId, int delta)
        {
            if (Draft.AddonLevels == null)
                Draft.AddonLevels = new Dictionary<string, int>();

            int current = Draft.AddonLevels.TryGetValue(addonId, out int v) ? v : 0;
            int newVal = current + delta;

            if (newVal < 0) return;
            if (newVal > ADDON_MAX_LEVEL) return;

            if (delta > 0)
            {
                if (GetSpentAddonPoints() >= rank.AddonPoints) return;
            }

            Draft.AddonLevels[addonId] = newVal;
            if (newVal == 0)
                Draft.AddonLevels.Remove(addonId);

            RebuildAddonsList(rank);
        }

        // ============================================================
        // МОРАЛЬ
        // ============================================================

        private void BuildMoralitySection(VisualElement parent)
        {
            var section = UIWidgets.Section("Мораль");

            section.Add(UIRoot.MakeMuted(
                "Выберите одну из моралей, доступных в хронике."));

            if (Chr.Moralites.Count == 0)
            {
                section.Add(UIRoot.MakeLabel("⚠ В хронике нет моралей.", 13,
                    new Color(1f, 0.55f, 0.55f)));
                parent.Add(section);
                return;
            }

            var ids = new List<string>();
            var names = new List<string>();
            foreach (var m in Chr.Moralites)
            {
                ids.Add(m.Id);
                names.Add($"{m.Name} (старт {m.StartRating})");
            }

            int idx = ids.IndexOf(Draft.MoralityId);
            if (idx < 0) idx = 0;

            section.Add(UIWidgets.MakePickerBlock(
                "Мораль",
                names,
                idx,
                i =>
                {
                    if (i >= 0 && i < ids.Count)
                        Draft.MoralityId = ids[i];
                },
                width: 400));

            var mor = Chr.FindMorality(Draft.MoralityId);
            if (mor != null)
                section.Add(UIRoot.MakeMuted($"Аура: {mor.AuraName}", 12));

            parent.Add(section);
        }

        // ============================================================
        // Служебное
        // ============================================================

        private int GetSpentMeritPoints()
        {
            int sum = 0;
            if (Draft.MeritIds == null) return 0;
            foreach (var id in Draft.MeritIds)
            {
                var m = Chr.FindMerit(id);
                if (m != null) sum += m.Cost;
            }
            return sum;
        }

        private void CleanupOrphanedFreeDiscChoices()
        {
            if (Draft.FreeDiscChoices == null) return;

            var validKeys = new HashSet<string>();
            foreach (var slot in CollectFreeDiscSlots())
                validKeys.Add(slot.Key);

            var toRemove = new List<string>();
            foreach (var kv in Draft.FreeDiscChoices)
                if (!validKeys.Contains(kv.Key))
                    toRemove.Add(kv.Key);

            foreach (var k in toRemove)
                Draft.FreeDiscChoices.Remove(k);
        }

        // ============================================================
        // Валидация шага 4
        // ============================================================

        private bool ValidateMeritsStep(out string error)
        {
            error = "";
            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null) { error = "Ранг не выбран."; return false; }

            int meritSpent = GetSpentMeritPoints();
            if (meritSpent > rank.MeritPoints)
            {
                error = $"Черты стоят больше очков, чем доступно ({meritSpent}/{rank.MeritPoints}).";
                return false;
            }

            int addonSpent = GetSpentAddonPoints();
            if (addonSpent > rank.AddonPoints)
            {
                error = $"Дополнения стоят больше очков, чем доступно ({addonSpent}/{rank.AddonPoints}).";
                return false;
            }

            if (string.IsNullOrEmpty(Draft.MoralityId))
            {
                error = "Выберите мораль.";
                return false;
            }

            return true;
        }
    }
}