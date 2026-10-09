using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Core.Rules;
using VTR.Data;
using VTR.Net;

namespace VTR.UI
{
    public partial class CharacterSheetScreen
    {
        // ============================================================
        // РЕСУРСЫ
        // ============================================================

        private void GMChangeHealth(int delta)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;

            if (delta < 0)
            {
                for (int i = 0; i < -delta; i++)
                    HealthSystem.ApplyDamage(_c, DamageType.Bashing, 1);
            }
            else
            {
                var type = HealthSystem.GetTopHealPriority(_c);
                if (type != DamageType.None)
                {
                    float cost = GetHealCost(type);
                    HealthSystem.HealLevels(_c, type, delta, cost, free: true);
                }
            }

            SaveAndSyncGM();
        }

        private void GMChangeWillpower(int delta)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            int cur = _c.WillpowerCurrent;
            int newVal = Mathf.Clamp(cur + delta, 0, _c.WillpowerMax);
            if (newVal == cur) return;

            _c.WillpowerCurrent = newVal;
            SaveAndSyncGM();
        }

        private void GMChangeBlood(int delta)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            int cur = _c.BloodCurrent;
            int newVal = Mathf.Clamp(cur + delta, 0, _c.BloodMax);
            if (newVal == cur) return;

            _c.BloodCurrent = newVal;
            SaveAndSyncGM();
        }

        // ============================================================
        // ОПЫТ
        // ============================================================

        private static int _gmXpTotalDraft = -1;

        private void BuildGmExperienceSection(VisualElement parent)
        {
            var section = UIWidgets.Section("Опыт (мастер)");

            section.Add(MakeInfoRow("Потрачено", _c.XpSpent.ToString()));
            section.Add(MakeInfoRow("Доступно", _c.XpAvailable.ToString()));

            int draft = _gmXpTotalDraft < 0 ? _c.XpTotal : _gmXpTotalDraft;

            var field = UIWidgets.MakeIntField("Всего опыта", draft,
                v => _gmXpTotalDraft = v, 0, 99999);
            section.Add(field);

            section.Add(UIRoot.MakeMuted(
                "Задайте точное значение всего опыта и нажмите «Применить». " +
                "Опыт будет начислен или забран у игрока.", 11));

            var applyBtn = new Button(() =>
            {
                int newTotal = _gmXpTotalDraft < 0 ? _c.XpTotal : _gmXpTotalDraft;
                GMSetExperienceTotal(newTotal);
                _gmXpTotalDraft = -1;
            });
            applyBtn.text = "✓  Применить";
            UIRoot.StyleButton(applyBtn, UIRoot.AccentGreen, Color.white, 34);
            applyBtn.style.marginTop = 6;
            section.Add(applyBtn);

            parent.Add(section);
        }

        private void GMSetExperienceTotal(int newTotal)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;

            int delta = newTotal - _c.XpTotal;
            if (delta == 0) return;

            _c.XpTotal = Mathf.Max(0, newTotal);
            Debug.Log($"[GM] Опыт изменён на {delta:+#;-#;0}. Теперь всего: {_c.XpTotal}.");

            SaveAndSyncGM();
        }

        // ============================================================
        // ЧЕРТЫ
        // ============================================================

        private void GMAddMerit(string meritId)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            var merit = _chr.FindMerit(meritId);
            if (merit == null) return;

            if (_c.MeritIds == null) _c.MeritIds = new List<string>();
            _c.MeritIds.Add(meritId);

            Debug.Log($"[GM] Добавлена черта «{merit.Name}» игроку.");

            ValidateAndCleanupAfterGMEdit();
            SaveAndSyncGM();
        }

        private void GMRemoveMerit(string meritId)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            if (_c.MeritIds == null) return;

            int idx = _c.MeritIds.LastIndexOf(meritId);
            if (idx < 0) return;
            _c.MeritIds.RemoveAt(idx);

            var merit = _chr.FindMerit(meritId);
            string name = merit != null ? merit.Name : meritId;
            Debug.Log($"[GM] Удалена черта «{name}».");

            ValidateAndCleanupAfterGMEdit();
            SaveAndSyncGM();
        }

        private void ShowGMAddMeritDialog()
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
            dialog.style.width = 560;
            dialog.style.maxHeight = 660;
            dialog.style.paddingTop = 16;
            dialog.style.paddingBottom = 16;
            dialog.style.paddingLeft = 16;
            dialog.style.paddingRight = 16;
            dialog.style.backgroundColor = UIRoot.BgPanel;
            dialog.style.borderTopLeftRadius = 8;
            dialog.style.borderTopRightRadius = 8;
            dialog.style.borderBottomLeftRadius = 8;
            dialog.style.borderBottomRightRadius = 8;

            var title = new Label("Добавить черту персонажу");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = UIRoot.TextPrimary;
            title.style.marginBottom = 8;
            dialog.Add(title);

            var search = new TextField();
            search.value = "";
            UIRoot.StyleTextField(search, "");
            dialog.Add(search);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.minHeight = 260;
            scroll.style.maxHeight = 480;
            dialog.Add(scroll);

            void RebuildList(string q)
            {
                scroll.Clear();
                string query = (q ?? "").Trim().ToLowerInvariant();
                int shown = 0;
                foreach (var m in _chr.Merits)
                {
                    if (!string.IsNullOrEmpty(query) &&
                        !m.Name.ToLowerInvariant().Contains(query))
                        continue;

                    var merit = m;
                    var btn = new Button(() =>
                    {
                        GMAddMerit(merit.Id);
                        panelRoot.Remove(overlay);
                    });
                    btn.text = $"{merit.Name}  ({(merit.Cost > 0 ? "+" : "")}{merit.Cost})";
                    btn.style.height = 32;
                    btn.style.fontSize = 14;
                    btn.style.color = UIRoot.TextPrimary;
                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    btn.style.paddingLeft = 12;
                    btn.style.marginBottom = 3;
                    btn.style.backgroundColor = UIRoot.AccentNeutral;
                    btn.style.borderTopLeftRadius = 4;
                    btn.style.borderTopRightRadius = 4;
                    btn.style.borderBottomLeftRadius = 4;
                    btn.style.borderBottomRightRadius = 4;
                    scroll.Add(btn);
                    shown++;
                }
                if (shown == 0)
                    scroll.Add(UIRoot.MakeMuted("Ничего не найдено."));
            }

            search.RegisterValueChangedCallback(e => RebuildList(e.newValue));

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
            search.schedule.Execute(() => search.Focus()).ExecuteLater(20);
        }

        // ============================================================
        // ДИСЦИПЛИНЫ (амальгамы)
        // ============================================================

        private void GMAddDisciplineAsAmalgam(string discId)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            var disc = _chr.FindDiscipline(discId);
            if (disc == null) return;

            if (_c.Disciplines == null) _c.Disciplines = new Dictionary<string, int>();
            _c.Disciplines[discId] = 1;

            Debug.Log($"[GM] Добавлена амальгама «{disc.Name}».");
            SaveAndSyncGM();
        }

        private void GMRemoveDisciplineFully(string discId)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            if (_c.Disciplines == null) return;
            if (!_c.Disciplines.ContainsKey(discId)) return;

            _c.Disciplines[discId] = 0;

            var disc = _chr.FindDiscipline(discId);
            string name = disc != null ? disc.Name : discId;
            Debug.Log($"[GM] Убрана дисциплина «{name}».");

            SaveAndSyncGM();
        }

        // ============================================================
        // ВАЛИДАЦИЯ ПОСЛЕ ИЗМЕНЕНИЙ МАСТЕРА
        // ============================================================

        /// <summary>
        /// Пересчитывает возможные последствия от изменений мастера:
        ///   • удаляет специализации у навыков со значением 0,
        ///   • сбрасывает пути, если их родитель упал ниже 1,
        ///   • сбрасывает амальгамы без выполненных требований.
        /// </summary>
        private void ValidateAndCleanupAfterGMEdit()
        {
            if (_c == null || _chr == null) return;

            // Специализации у нулевых навыков.
            if (_c.Specializations != null && _c.Skills != null)
            {
                var keysToClear = new List<string>();
                foreach (var kv in _c.Specializations)
                {
                    if (!_c.Skills.TryGetValue(kv.Key, out int lvl) || lvl <= 0)
                        keysToClear.Add(kv.Key);
                }
                foreach (var k in keysToClear)
                    _c.Specializations[k].Clear();
            }

            // Пути без родителей.
            if (_c.Disciplines != null)
            {
                foreach (var disc in _chr.Disciplines)
                {
                    if (!disc.IsPath) continue;
                    if (!_c.Disciplines.TryGetValue(disc.Id, out int lvl) || lvl <= 0) continue;

                    int parentLvl = _c.Disciplines.TryGetValue(disc.ParentDisciplineId, out int pl) ? pl : 0;
                    int parentMerit = ComputeMeritDisciplineLevel(disc.ParentDisciplineId);
                    if (parentLvl + parentMerit < 1)
                    {
                        _c.Disciplines[disc.Id] = 0;
                        Debug.Log($"[GM] Путь «{disc.Name}» сброшен (нет родителя).");
                    }
                }
            }

            // Амальгамы без требований.
            if (_c.Disciplines != null)
            {
                foreach (var disc in _chr.Disciplines)
                {
                    if (!disc.IsAmalgam) continue;
                    if (!_c.Disciplines.TryGetValue(disc.Id, out int lvl) || lvl <= 0) continue;
                    if (disc.AmalgamRequirements == null) continue;

                    bool allOk = true;
                    foreach (var req in disc.AmalgamRequirements)
                    {
                        int have = _c.Disciplines.TryGetValue(req.DisciplineId, out int v) ? v : 0;
                        have += ComputeMeritDisciplineLevel(req.DisciplineId);
                        if (have < req.MinLevel) { allOk = false; break; }
                    }

                    if (!allOk)
                    {
                        _c.Disciplines[disc.Id] = 0;
                        Debug.Log($"[GM] Амальгама «{disc.Name}» сброшена (нет требований).");
                    }
                }
            }
        }
    }
}