using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Core.Rules;
using VTR.Data;

namespace VTR.UI
{
    public partial class CharacterSheetScreen
    {
        private int GetAttributeUpgradeCost(string attrId)
        {
            var rank = _chr.FindRank(_c.RankId);
            var sys = _chr.XpSystem;
            if (rank == null || sys == null) return 0;

            int cur = _c.Attributes.TryGetValue(attrId, out int v) ? v : 1;
            bool favored = IsFavoredAttribute(attrId);

            return XpCalculator.AttributeCost(sys, cur, favored);
        }

        private int GetSkillUpgradeCost(string skillId)
        {
            var rank = _chr.FindRank(_c.RankId);
            var sys = _chr.XpSystem;
            if (rank == null || sys == null) return 0;

            int cur = _c.Skills.TryGetValue(skillId, out int v) ? v : 0;
            bool favored = IsFavoredSkill(skillId);

            return XpCalculator.SkillCost(sys, cur, favored);
        }

        private int GetDisciplineUpgradeCost(string discId)
        {
            var rank = _chr.FindRank(_c.RankId);
            var sys = _chr.XpSystem;
            if (rank == null || sys == null) return 0;

            int cur = _c.Disciplines.TryGetValue(discId, out int v) ? v : 0;
            bool clan = IsClanDiscipline(discId);

            return XpCalculator.DisciplineCost(sys, cur, clan);
        }

        private bool CanUpgradeAttribute(string attrId, out string reason)
        {
            reason = "";
            var rank = _chr.FindRank(_c.RankId);
            if (rank == null) { reason = "нет ранга"; return false; }

            int baseVal = _c.Attributes.TryGetValue(attrId, out int v) ? v : 1;
            int bonus = ComputeMeritBonus("attr." + attrId);
            int newEffective = baseVal + 1 + bonus;

            int hardMax = GetAttributeHardCap(rank);
            if (newEffective > hardMax) { reason = $"макс. {hardMax}"; return false; }

            int cost = GetAttributeUpgradeCost(attrId);
            if (_c.XpAvailable < cost) { reason = "мало опыта"; return false; }

            return true;
        }

        private bool CanUpgradeSkill(string skillId, out string reason)
        {
            reason = "";
            var rank = _chr.FindRank(_c.RankId);
            if (rank == null) { reason = "нет ранга"; return false; }

            int baseVal = _c.Skills.TryGetValue(skillId, out int v) ? v : 0;
            int bonus = ComputeMeritBonus("skill." + skillId);
            int newEffective = baseVal + 1 + bonus;

            int hardMax = GetSkillHardCap(rank);
            if (newEffective > hardMax) { reason = $"макс. {hardMax}"; return false; }

            int cost = GetSkillUpgradeCost(skillId);
            if (_c.XpAvailable < cost) { reason = "мало опыта"; return false; }

            return true;
        }

        private bool CanUpgradeDiscipline(string discId, out string reason)
        {
            reason = "";
            var rank = _chr.FindRank(_c.RankId);
            if (rank == null) { reason = "нет ранга"; return false; }

            var disc = _chr.FindDiscipline(discId);
            if (disc == null) { reason = "нет дисциплины"; return false; }
            if (disc.IsAmalgam) { reason = "амальгама"; return false; }

            int baseVal = _c.Disciplines.TryGetValue(discId, out int v) ? v : 0;
            int meritLvl = ComputeMeritDisciplineLevel(discId);
            int newEffective = baseVal + 1 + meritLvl;

            int hardMax = GetBPMaxRating(rank);
            if (newEffective > hardMax) { reason = $"макс. {hardMax}"; return false; }

            if (disc.IsPath)
            {
                int parentLvl = 0;
                if (_c.Disciplines.TryGetValue(disc.ParentDisciplineId, out int pl)) parentLvl = pl;
                int parentMerit = ComputeMeritDisciplineLevel(disc.ParentDisciplineId);
                if (parentLvl + parentMerit < 1) { reason = "нет родителя"; return false; }
            }

            int cost = GetDisciplineUpgradeCost(discId);
            if (_c.XpAvailable < cost) { reason = "мало опыта"; return false; }

            return true;
        }

        private void TryUpgradeAttribute(string attrId)
        {
            if (!CanUpgradeAttribute(attrId, out string reason))
            {
                Debug.LogWarning($"[XP] Нельзя поднять атрибут: {reason}");
                return;
            }

            var attr = _chr.FindAttribute(attrId);
            string name = attr != null ? attr.Name : attrId;
            int cost = GetAttributeUpgradeCost(attrId);

            ShowConfirmDialog(
                $"Повысить «{name}»?",
                $"Стоимость: {cost} опыта. Останется: {_c.XpAvailable - cost}.",
                () =>
                {
                    _c.Attributes[attrId] = (_c.Attributes.TryGetValue(attrId, out int v) ? v : 1) + 1;
                    _c.XpSpent += cost;
                    JsonSaveSystem.SaveCharacter(_c);
                    SendCharacterUpdate();
                    UIRoot.Instance.ShowCharacterSheet();
                });
        }

        private void TryUpgradeSkill(string skillId)
        {
            if (!CanUpgradeSkill(skillId, out string reason))
            {
                Debug.LogWarning($"[XP] Нельзя поднять навык: {reason}");
                return;
            }

            var skill = _chr.FindSkill(skillId);
            string name = skill != null ? _chr.GetSkillName(skill) : skillId;
            int cost = GetSkillUpgradeCost(skillId);

            ShowConfirmDialog(
                $"Повысить «{name}»?",
                $"Стоимость: {cost} опыта. Останется: {_c.XpAvailable - cost}.",
                () =>
                {
                    _c.Skills[skillId] = (_c.Skills.TryGetValue(skillId, out int v) ? v : 0) + 1;
                    _c.XpSpent += cost;
                    JsonSaveSystem.SaveCharacter(_c);
                    SendCharacterUpdate();
                    UIRoot.Instance.ShowCharacterSheet();
                });
        }

        private void TryUpgradeDiscipline(string discId)
        {
            if (!CanUpgradeDiscipline(discId, out string reason))
            {
                Debug.LogWarning($"[XP] Нельзя поднять дисциплину: {reason}");
                return;
            }

            var disc = _chr.FindDiscipline(discId);
            if (disc == null) return;

            int cost = GetDisciplineUpgradeCost(discId);

            ShowConfirmDialog(
                $"Повысить «{disc.Name}»?",
                $"Стоимость: {cost} опыта. Останется: {_c.XpAvailable - cost}.",
                () =>
                {
                    _c.Disciplines[discId] = (_c.Disciplines.TryGetValue(discId, out int v) ? v : 0) + 1;
                    _c.XpSpent += cost;
                    JsonSaveSystem.SaveCharacter(_c);
                    SendCharacterUpdate();
                    UIRoot.Instance.ShowCharacterSheet();
                });
        }

        private void SendCharacterUpdate()
        {
            try
            {
                if (VTR.Net.NetworkSession.IsPlayer && VTR.Net.NetworkGameManager.Instance != null)
                {
                    string json = JsonSaveSystem.Serialize(_c);
                    VTR.Net.NetworkGameManager.Instance.SendMyCharacter(json);
                    Debug.Log("[XP] Обновление персонажа отправлено мастеру.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[XP] Не удалось отправить персонажа: {e.Message}");
            }
        }

        private void GMChangeAttribute(string attrId, int delta)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            var rank = _chr.FindRank(_c.RankId);

            int cur = _c.Attributes.TryGetValue(attrId, out int v) ? v : 1;
            int newVal = cur + delta;
            if (newVal < 0) return;

            int hardMax = rank != null ? GetAttributeHardCap(rank) : 10;
            if (newVal + ComputeMeritBonus("attr." + attrId) > hardMax) return;

            _c.Attributes[attrId] = newVal;
            SaveAndSyncGM();
        }

        private void GMChangeSkill(string skillId, int delta)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            var rank = _chr.FindRank(_c.RankId);

            int cur = _c.Skills.TryGetValue(skillId, out int v) ? v : 0;
            int newVal = cur + delta;
            if (newVal < 0) return;

            int hardMax = rank != null ? GetSkillHardCap(rank) : 10;
            if (newVal + ComputeMeritBonus("skill." + skillId) > hardMax) return;

            _c.Skills[skillId] = newVal;

            if (newVal == 0 && _c.Specializations.ContainsKey(skillId))
                _c.Specializations[skillId].Clear();

            SaveAndSyncGM();
        }

        private void GMChangeDiscipline(string discId, int delta)
        {
            if (!SessionContext.IsGMViewingCharacter || !_gmEditMode) return;
            var rank = _chr.FindRank(_c.RankId);
            if (rank == null) return;

            var disc = _chr.FindDiscipline(discId);
            if (disc == null) return;

            int cur = _c.Disciplines.TryGetValue(discId, out int v) ? v : 0;
            int newVal = cur + delta;
            if (newVal < 0) return;

            int hardMax = GetBPMaxRating(rank);
            if (newVal + ComputeMeritDisciplineLevel(discId) > hardMax) return;

            _c.Disciplines[discId] = newVal;
            SaveAndSyncGM();
        }

        private void SaveAndSyncGM()
        {
            JsonSaveSystem.SaveCharacter(_c);

            var mgr = VTR.Net.NetworkGameManager.Instance;
            if (mgr == null)
            {
                UIRoot.Instance.ShowCharacterSheet();
                return;
            }

            string json = JsonSaveSystem.Serialize(_c);

            if (SessionContext.GMViewingOwnerId != 0)
            {
                mgr.MasterSetCharacter(SessionContext.GMViewingOwnerId, json);
                Debug.Log($"[GM] Персонаж «{_c.Name}» обновлён мастером и отправлен игроку {SessionContext.GMViewingOwnerId}.");
            }
            else
            {
                mgr.SendMyCharacter(json);
            }

            UIRoot.Instance.ShowCharacterSheet();
        }

        private void ShowConfirmDialog(string title, string message, System.Action onConfirm)
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
            dialog.style.width = 460;
            dialog.style.paddingTop = 18;
            dialog.style.paddingBottom = 18;
            dialog.style.paddingLeft = 18;
            dialog.style.paddingRight = 18;
            dialog.style.backgroundColor = UIRoot.BgPanel;
            dialog.style.borderTopLeftRadius = 8;
            dialog.style.borderTopRightRadius = 8;
            dialog.style.borderBottomLeftRadius = 8;
            dialog.style.borderBottomRightRadius = 8;

            var titleLbl = new Label(title);
            titleLbl.style.fontSize = 16;
            titleLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLbl.style.color = UIRoot.TextPrimary;
            titleLbl.style.marginBottom = 8;
            titleLbl.style.whiteSpace = WhiteSpace.Normal;
            dialog.Add(titleLbl);

            var msgLbl = new Label(message);
            msgLbl.style.fontSize = 13;
            msgLbl.style.color = UIRoot.TextMuted;
            msgLbl.style.whiteSpace = WhiteSpace.Normal;
            msgLbl.style.marginBottom = 14;
            dialog.Add(msgLbl);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            btnRow.style.justifyContent = Justify.FlexEnd;
            dialog.Add(btnRow);

            var cancelBtn = new Button(() => panelRoot.Remove(overlay));
            cancelBtn.text = "Отмена";
            UIRoot.StyleButton(cancelBtn, UIRoot.AccentNeutral, Color.white, 34);
            cancelBtn.style.width = 110;
            btnRow.Add(cancelBtn);

            var okBtn = new Button(() =>
            {
                panelRoot.Remove(overlay);
                onConfirm?.Invoke();
            });
            okBtn.text = "Подтвердить";
            UIRoot.StyleButton(okBtn, UIRoot.AccentGreen, Color.white, 34);
            okBtn.style.width = 140;
            btnRow.Add(okBtn);

            overlay.Add(dialog);
            overlay.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == overlay) panelRoot.Remove(overlay);
            });
            panelRoot.Add(overlay);
        }

        private bool IsFavoredAttribute(string attrId)
            => _c.FavoredAttributeId == attrId;

        private bool IsFavoredSkill(string skillId)
        {
            var clan = _chr.FindClan(_c.ClanId);
            if (clan?.FavoredSkillIds == null) return false;
            return clan.FavoredSkillIds.Contains(skillId);
        }

        private bool IsClanDiscipline(string discId)
        {
            var clan = _chr.FindClan(_c.ClanId);
            if (clan?.ClanDisciplineIds == null) return false;
            return clan.ClanDisciplineIds.Contains(discId);
        }

        private int GetEffectiveBloodPotency(Rank rank)
        {
            if (rank == null) return 1;
            int bonus = ComputeMeritBonus("blood.potency");
            int val = rank.StartBloodPotency + bonus;
            if (val < 0) val = 0;
            if (val > 10) val = 10;
            return val;
        }

        private int GetBPMaxRating(Rank rank)
        {
            int bp = GetEffectiveBloodPotency(rank);
            if (_chr.BloodPotencyMaxRating != null &&
                _chr.BloodPotencyMaxRating.TryGetValue(bp, out int cap) && cap > 0)
                return cap;
            return 5;
        }

        private int GetAttributeHardCap(Rank rank)
            => Mathf.Max(rank.AttributeMax, GetBPMaxRating(rank));

        private int GetSkillHardCap(Rank rank)
            => Mathf.Max(rank.SkillMax, GetBPMaxRating(rank));
    }
}