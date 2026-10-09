using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;
using VTR.Net;

namespace VTR.UI
{
    public partial class CharacterWizardScreen : IScreen
    {
        private VisualElement _root;
        private VisualElement _content;
        private Label _stepLabel;
        private VisualElement _navBar;
        private Label _errorLabel;

        private readonly Dictionary<string, Label> _attrValueLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, Label> _attrGroupCounters = new Dictionary<string, Label>();
        private readonly Dictionary<string, Button> _attrMinusButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Button> _attrPlusButtons = new Dictionary<string, Button>();
        private Label _totalAttrSummary;

        private readonly Dictionary<string, Label> _skillValueLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, Label> _skillGroupCounters = new Dictionary<string, Label>();
        private readonly Dictionary<string, Button> _skillMinusButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Button> _skillPlusButtons = new Dictionary<string, Button>();
        private Label _totalSkillSummary;

        private readonly Dictionary<string, Label> _discValueLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, Button> _discMinusButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Button> _discPlusButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Button> _amalgamToggleButtons = new Dictionary<string, Button>();
        private Label _totalDiscSummary;
        private Label _discCounterLabel;

        private VisualElement _specListContainer;
        private Label _specCounterLabel;

        private VisualElement _meritsListContainer;
        private Label _meritSpentLabel;
        private VisualElement _addonsListContainer;
        private Label _addonSpentLabel;

        private CharacterDraft Draft => SessionContext.CurrentDraft;
        private Chronicle Chr => SessionContext.CurrentChronicle;

        public void Build(VisualElement root)
        {
            _root = root;

            if (Draft == null || Chr == null)
            {
                root.Add(UIRoot.MakeLabel("Черновик или хроника не загружены.", 16, Color.red));
                return;
            }

            EnsureDefaults();

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.paddingTop = 16;
            container.style.paddingBottom = 16;
            container.style.paddingLeft = 24;
            container.style.paddingRight = 24;
            _root.Add(container);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 12;
            container.Add(header);

            var title = UIRoot.MakeHeader("Создание персонажа", 22);
            title.style.flexGrow = 1;
            header.Add(title);

            _stepLabel = UIRoot.MakeMuted($"Шаг {Draft.CurrentStep + 1} из 6", 14);
            header.Add(_stepLabel);

            _navBar = new VisualElement();
            _navBar.style.flexDirection = FlexDirection.Row;
            _navBar.style.flexWrap = Wrap.Wrap;
            _navBar.style.marginBottom = 12;
            container.Add(_navBar);

            _errorLabel = UIRoot.MakeLabel("", 13, new Color(1f, 0.55f, 0.55f));
            _errorLabel.style.marginBottom = 8;
            _errorLabel.style.display = DisplayStyle.None;
            container.Add(_errorLabel);

            _content = new ScrollView();
            _content.style.flexGrow = 1;
            container.Add(_content);

            var bottomBar = new VisualElement();
            bottomBar.style.flexDirection = FlexDirection.Row;
            bottomBar.style.marginTop = 12;
            container.Add(bottomBar);

            var backBtn = new Button(() =>
            {
                if (Draft.CurrentStep > 0)
                {
                    Draft.CurrentStep--;
                    Refresh();
                }
                else
                {
                    SessionContext.CurrentDraft = null;
                    UIRoot.Instance.ShowCharacterList();
                }
            });
            backBtn.text = "←  Назад";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 40);
            backBtn.style.width = 160;
            bottomBar.Add(backBtn);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            bottomBar.Add(spacer);

            var nextBtn = new Button(() =>
            {
                if (!ValidateStep(Draft.CurrentStep, out string error))
                {
                    ShowError(error);
                    return;
                }
                if (Draft.CurrentStep < 5)
                {
                    Draft.CurrentStep++;
                    Refresh();
                }
                else
                {
                    FinishCreation();
                }
            });
            nextBtn.text = "Далее  →";
            UIRoot.StyleButton(nextBtn, UIRoot.AccentGreen, Color.white, 40);
            nextBtn.style.width = 200;
            bottomBar.Add(nextBtn);

            Refresh();
        }

        private void EnsureDefaults()
        {
            if (string.IsNullOrEmpty(Draft.RankId))
            {
                var available = GetAvailableRanks();
                if (available.Count > 0)
                    Draft.RankId = available[0].Id;
            }
            if (string.IsNullOrEmpty(Draft.ClanId) && Chr.Clans.Count > 0)
                Draft.ClanId = Chr.Clans[0].Id;
            if (string.IsNullOrEmpty(Draft.Nature) && Chr.Natures.Count > 0)
                Draft.Nature = Chr.Natures[0];
            if (string.IsNullOrEmpty(Draft.Mask) && Chr.Masks.Count > 0)
                Draft.Mask = Chr.Masks[0];
            if (string.IsNullOrEmpty(Draft.MoralityId) && Chr.Moralites.Count > 0)
                Draft.MoralityId = Chr.Moralites[0].Id;

            if (string.IsNullOrEmpty(Draft.FavoredAttributeId))
            {
                var clan = Chr.FindClan(Draft.ClanId);
                if (clan != null && clan.FavoredAttributeIds.Count > 0)
                    Draft.FavoredAttributeId = clan.FavoredAttributeIds[0];
            }

            if (string.IsNullOrEmpty(Draft.PrimaryAttributeGroupId) && Chr.AttributeGroups.Count >= 1)
                Draft.PrimaryAttributeGroupId = Chr.AttributeGroups[0].Id;
            if (string.IsNullOrEmpty(Draft.SecondaryAttributeGroupId) && Chr.AttributeGroups.Count >= 2)
            {
                for (int i = 0; i < Chr.AttributeGroups.Count; i++)
                {
                    if (Chr.AttributeGroups[i].Id == Draft.PrimaryAttributeGroupId) continue;
                    Draft.SecondaryAttributeGroupId = Chr.AttributeGroups[i].Id;
                    break;
                }
            }
            if (string.IsNullOrEmpty(Draft.TertiaryAttributeGroupId) && Chr.AttributeGroups.Count >= 3)
            {
                for (int i = 0; i < Chr.AttributeGroups.Count; i++)
                {
                    var id = Chr.AttributeGroups[i].Id;
                    if (id == Draft.PrimaryAttributeGroupId) continue;
                    if (id == Draft.SecondaryAttributeGroupId) continue;
                    Draft.TertiaryAttributeGroupId = id;
                    break;
                }
            }

            if (string.IsNullOrEmpty(Draft.PrimarySkillGroupId) && Chr.SkillGroups.Count >= 1)
                Draft.PrimarySkillGroupId = Chr.SkillGroups[0].Id;
            if (string.IsNullOrEmpty(Draft.SecondarySkillGroupId) && Chr.SkillGroups.Count >= 2)
            {
                for (int i = 0; i < Chr.SkillGroups.Count; i++)
                {
                    if (Chr.SkillGroups[i].Id == Draft.PrimarySkillGroupId) continue;
                    Draft.SecondarySkillGroupId = Chr.SkillGroups[i].Id;
                    break;
                }
            }
            if (string.IsNullOrEmpty(Draft.TertiarySkillGroupId) && Chr.SkillGroups.Count >= 3)
            {
                for (int i = 0; i < Chr.SkillGroups.Count; i++)
                {
                    var id = Chr.SkillGroups[i].Id;
                    if (id == Draft.PrimarySkillGroupId) continue;
                    if (id == Draft.SecondarySkillGroupId) continue;
                    Draft.TertiarySkillGroupId = id;
                    break;
                }
            }
        }

        private void Refresh()
        {
            _stepLabel.text = $"Шаг {Draft.CurrentStep + 1} из 6";
            HideError();
            BuildNavBar();
            _content.Clear();

            _attrValueLabels.Clear();
            _attrGroupCounters.Clear();
            _attrMinusButtons.Clear();
            _attrPlusButtons.Clear();
            _skillValueLabels.Clear();
            _skillGroupCounters.Clear();
            _skillMinusButtons.Clear();
            _skillPlusButtons.Clear();
            _discValueLabels.Clear();
            _discMinusButtons.Clear();
            _discPlusButtons.Clear();
            _amalgamToggleButtons.Clear();

            switch (Draft.CurrentStep)
            {
                case 0: BuildStep_Identity(_content); break;
                case 1: BuildStep_Attributes(_content); break;
                case 2: BuildStep_Skills(_content); break;
                case 3: BuildStep_MeritsAndMorality(_content); break;
                case 4: BuildStep_Disciplines(_content); break;
                case 5: BuildStep_Biography(_content); break;
            }
        }

        private void BuildNavBar()
        {
            _navBar.Clear();
            string[] names = { "Основное", "Атрибуты", "Навыки", "Черты/Доп./Мораль", "Дисциплины", "Биография" };

            for (int i = 0; i < names.Length; i++)
            {
                int idx = i;
                var btn = new Button(() =>
                {
                    if (idx > Draft.CurrentStep)
                    {
                        for (int s = Draft.CurrentStep; s < idx; s++)
                        {
                            if (!ValidateStep(s, out string err))
                            {
                                ShowError($"Шаг {s + 1}: {err}");
                                return;
                            }
                        }
                    }
                    Draft.CurrentStep = idx;
                    Refresh();
                });
                btn.text = names[i];
                btn.style.height = 30;
                btn.style.fontSize = 12;
                btn.style.marginRight = 4;
                btn.style.marginBottom = 4;
                btn.style.paddingLeft = 10;
                btn.style.paddingRight = 10;
                btn.style.borderTopLeftRadius = 4;
                btn.style.borderTopRightRadius = 4;
                btn.style.borderBottomLeftRadius = 4;
                btn.style.borderBottomRightRadius = 4;
                btn.style.color = Color.white;
                btn.style.backgroundColor = i == Draft.CurrentStep
                    ? UIRoot.AccentBlue
                    : (i < Draft.CurrentStep ? new Color(0.35f, 0.5f, 0.35f) : UIRoot.AccentNeutral);
                _navBar.Add(btn);
            }
        }

        // ============================================================
        // Хардкапы по густоте крови
        // ============================================================

        private int GetEffectiveBloodPotency(Rank rank)
        {
            if (rank == null) return 1;
            int bonus = GetMeritBonus("blood.potency");
            int val = rank.StartBloodPotency + bonus;
            if (val < 0) val = 0;
            if (val > 10) val = 10;
            return val;
        }

        private int GetMaxBloodPool(Rank rank)
        {
            int bp = GetEffectiveBloodPotency(rank);
            if (Chr.BloodPotencyBloodMax != null &&
                Chr.BloodPotencyBloodMax.TryGetValue(bp, out int max) && max > 0)
                return max;
            return 10;
        }

        private int GetBPMaxRating(Rank rank)
        {
            int bp = GetEffectiveBloodPotency(rank);
            if (Chr.BloodPotencyMaxRating != null &&
                Chr.BloodPotencyMaxRating.TryGetValue(bp, out int cap) && cap > 0)
                return cap;
            return 5;
        }

        private int GetAttributeHardCap(Rank rank)
            => Mathf.Max(rank.AttributeMax, GetBPMaxRating(rank));

        private int GetSkillHardCap(Rank rank)
            => Mathf.Max(rank.SkillMax, GetBPMaxRating(rank));

        private int GetDisciplineHardCap(Rank rank)
            => GetBPMaxRating(rank);

        // ============================================================
        // Сброс распределения при смене приоритетов
        // ============================================================

        private void ResetAllAttributeSpending()
        {
            foreach (var attr in Chr.Attributes)
            {
                int baseVal = attr.Id == Draft.FavoredAttributeId ? 2 : 1;
                Draft.Attributes[attr.Id] = baseVal;
            }
        }

        private void ResetAllSkillSpending()
        {
            foreach (var skill in Chr.Skills)
                Draft.Skills[skill.Id] = 0;

            if (Draft.Specializations != null)
            {
                foreach (var key in new List<string>(Draft.Specializations.Keys))
                    Draft.Specializations[key].Clear();
            }
        }

        // ============================================================
        // ШАГ 1: Основное
        // ============================================================

        private void BuildStep_Identity(VisualElement parent)
        {
            parent.Add(UIRoot.MakeHeader("Шаг 1: Основное"));

            var nameSection = UIWidgets.Section("Имя");
            nameSection.Add(UIWidgets.MakeTextField("Имя персонажа", Draft.Name,
                v => Draft.Name = v));
            parent.Add(nameSection);

            var rankSection = UIWidgets.Section("Ранг вампира");
            rankSection.Add(UIRoot.MakeMuted("Ранги с достигнутым лимитом игроков недоступны."));

            var availableRanks = GetAvailableRanks();
            var rankNames = new List<string>();
            var rankIds = new List<string>();
            foreach (var r in availableRanks)
            {
                rankIds.Add(r.Id);
                rankNames.Add(r.Name);
            }

            if (rankIds.Count == 0)
            {
                rankSection.Add(UIRoot.MakeLabel("⚠ Нет доступных рангов.", 13, new Color(1f, 0.55f, 0.55f)));
            }
            else
            {
                int rankIdx = rankIds.IndexOf(Draft.RankId);
                if (rankIdx < 0) rankIdx = 0;
                rankSection.Add(UIWidgets.MakePickerBlock(
                    "Ранг", rankNames, rankIdx,
                    i =>
                    {
                        if (i >= 0 && i < rankIds.Count)
                        {
                            Draft.RankId = rankIds[i];
                            ApplyRankDefaults();
                            Refresh();
                        }
                    },
                    width: 320));

                var rank = Chr.FindRank(Draft.RankId);
                if (rank != null)
                    rankSection.Add(UIRoot.MakeMuted(GetRankSummary(rank), 12));
            }
            parent.Add(rankSection);

            var clanSection = UIWidgets.Section("Клан");
            var clanIds = new List<string>();
            var clanNames = new List<string>();
            foreach (var c in Chr.Clans)
            {
                clanIds.Add(c.Id);
                clanNames.Add(c.Name);
            }

            if (clanIds.Count == 0)
            {
                clanSection.Add(UIRoot.MakeLabel("⚠ Нет кланов в хронике.", 13, new Color(1f, 0.55f, 0.55f)));
            }
            else
            {
                int clanIdx = clanIds.IndexOf(Draft.ClanId);
                if (clanIdx < 0) clanIdx = 0;
                clanSection.Add(UIWidgets.MakePickerBlock(
                    "Клан", clanNames, clanIdx,
                    i =>
                    {
                        if (i >= 0 && i < clanIds.Count)
                        {
                            Draft.ClanId = clanIds[i];

                            if (!string.IsNullOrEmpty(Draft.BloodlineId))
                            {
                                var bl = Chr.FindBloodline(Draft.BloodlineId);
                                if (bl == null || bl.ClanId != Draft.ClanId)
                                    Draft.BloodlineId = "";
                            }

                            var clan = Chr.FindClan(Draft.ClanId);
                            if (clan != null && clan.FavoredAttributeIds.Count > 0)
                            {
                                if (!clan.FavoredAttributeIds.Contains(Draft.FavoredAttributeId))
                                    Draft.FavoredAttributeId = clan.FavoredAttributeIds[0];
                            }
                            Refresh();
                        }
                    },
                    width: 320));

                var clanSel = Chr.FindClan(Draft.ClanId);
                if (clanSel != null)
                    clanSection.Add(UIRoot.MakeMuted($"Проклятие: {clanSel.Curse}", 12));
            }
            parent.Add(clanSection);

            var blSection = UIWidgets.Section("Бладлайн (опционально)");
            blSection.Add(UIRoot.MakeMuted("Доступны только бладлайны выбранного клана."));

            if (string.IsNullOrEmpty(Draft.ClanId))
            {
                blSection.Add(UIRoot.MakeMuted("Сначала выберите клан."));
            }
            else
            {
                var bloodlines = Chr.Bloodlines.FindAll(b => b.ClanId == Draft.ClanId);
                if (bloodlines.Count == 0)
                {
                    blSection.Add(UIRoot.MakeMuted("У этого клана нет бладлайнов."));
                }
                else
                {
                    var blIds = new List<string> { "" };
                    var blNames = new List<string> { "— (не выбран) —" };
                    foreach (var b in bloodlines)
                    {
                        blIds.Add(b.Id);
                        blNames.Add(b.Name);
                    }

                    int blIdx = blIds.IndexOf(Draft.BloodlineId);
                    if (blIdx < 0) blIdx = 0;
                    blSection.Add(UIWidgets.MakePickerBlock(
                        "Бладлайн", blNames, blIdx,
                        i => { if (i >= 0 && i < blIds.Count) { Draft.BloodlineId = blIds[i]; Refresh(); } },
                        width: 400));

                    if (!string.IsNullOrEmpty(Draft.BloodlineId))
                    {
                        var bl = Chr.FindBloodline(Draft.BloodlineId);
                        if (bl != null)
                        {
                            int required = bl.GetRequiredBloodPotency();
                            blSection.Add(UIRoot.MakeMuted(
                                $"Требование: густота крови не ниже {required}. " +
                                $"Проклятие: {bl.AdditionalCurse}", 12));
                        }
                    }
                }
            }
            parent.Add(blSection);

            var natureSection = UIWidgets.Section("Натура");
            if (Chr.Natures.Count == 0)
                natureSection.Add(UIRoot.MakeMuted("В хронике нет натур."));
            else
            {
                int natureIdx = Chr.Natures.IndexOf(Draft.Nature);
                if (natureIdx < 0) natureIdx = 0;
                natureSection.Add(UIWidgets.MakePickerBlock(
                    "Натура", Chr.Natures, natureIdx,
                    i => { if (i >= 0 && i < Chr.Natures.Count) Draft.Nature = Chr.Natures[i]; },
                    width: 320));
            }
            parent.Add(natureSection);

            var maskSection = UIWidgets.Section("Маска");
            maskSection.Add(UIRoot.MakeMuted("Может совпадать с натурой."));
            if (Chr.Masks.Count == 0)
                maskSection.Add(UIRoot.MakeMuted("В хронике нет масок."));
            else
            {
                int maskIdx = Chr.Masks.IndexOf(Draft.Mask);
                if (maskIdx < 0) maskIdx = 0;
                maskSection.Add(UIWidgets.MakePickerBlock(
                    "Маска", Chr.Masks, maskIdx,
                    i => { if (i >= 0 && i < Chr.Masks.Count) Draft.Mask = Chr.Masks[i]; },
                    width: 320));
            }
            parent.Add(maskSection);

            var favAttrSection = UIWidgets.Section("Любимый атрибут");
            if (string.IsNullOrEmpty(Draft.ClanId))
            {
                favAttrSection.Add(UIRoot.MakeMuted("Сначала выберите клан."));
            }
            else
            {
                var clan = Chr.FindClan(Draft.ClanId);
                if (clan == null || clan.FavoredAttributeIds.Count == 0)
                {
                    favAttrSection.Add(UIRoot.MakeMuted("У клана нет любимых атрибутов."));
                }
                else
                {
                    var favIds = new List<string>();
                    var favNames = new List<string>();
                    foreach (var id in clan.FavoredAttributeIds)
                    {
                        var attr = Chr.FindAttribute(id);
                        if (attr != null) { favIds.Add(id); favNames.Add(attr.Name); }
                    }

                    if (favIds.Count == 0)
                        favAttrSection.Add(UIRoot.MakeMuted("Любимые атрибуты клана не найдены."));
                    else
                    {
                        int favIdx = favIds.IndexOf(Draft.FavoredAttributeId);
                        if (favIdx < 0) favIdx = 0;
                        favAttrSection.Add(UIWidgets.MakePickerBlock(
                            "Любимый атрибут (+1 на старте)",
                            favNames, favIdx,
                            i => { if (i >= 0 && i < favIds.Count) Draft.FavoredAttributeId = favIds[i]; },
                            width: 400));
                    }
                }
            }
            parent.Add(favAttrSection);
        }

        // ============================================================
        // ШАГ 2: Атрибуты
        // ============================================================

        private void BuildStep_Attributes(VisualElement parent)
        {
            parent.Add(UIRoot.MakeHeader("Шаг 2: Атрибуты"));

            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null)
            {
                parent.Add(UIRoot.MakeLabel("Ранг не выбран. Вернитесь на шаг 1.",
                    14, new Color(1f, 0.55f, 0.55f)));
                return;
            }

            EnsureAttributesInitialized(rank);
            BuildAttributePrioritiesSection(parent, rank);
            BuildAttributeDistributionSection(parent, rank);

            var totalSection = UIWidgets.Section("Итог");
            _totalAttrSummary = UIRoot.MakeMuted("", 13);
            totalSection.Add(_totalAttrSummary);
            parent.Add(totalSection);

            UpdateAllAttributeLabels(rank);
        }

        private void BuildAttributePrioritiesSection(VisualElement parent, Rank rank)
        {
            var section = UIWidgets.Section("Приоритеты групп атрибутов");
            section.Add(UIRoot.MakeMuted(
                "Первичная группа получит больше всего очков, вторичная — среднее, третичная — меньшее."));

            var groupNames = new List<string>();
            var groupIds = new List<string>();
            foreach (var g in Chr.AttributeGroups) { groupIds.Add(g.Id); groupNames.Add(g.Name); }

            if (groupIds.Count < 3)
            {
                section.Add(UIRoot.MakeLabel("⚠ Нужно минимум 3 группы атрибутов.",
                    13, new Color(1f, 0.55f, 0.55f)));
                parent.Add(section);
                return;
            }

            int primIdx = groupIds.IndexOf(Draft.PrimaryAttributeGroupId);
            if (primIdx < 0) primIdx = 0;
            section.Add(UIWidgets.MakePickerBlock(
                $"Первичная ({rank.AttributePoints.Primary} очков)",
                groupNames, primIdx,
                i =>
                {
                    if (i < 0 || i >= groupIds.Count) return;
                    string newPrim = groupIds[i];
                    string oldPrim = Draft.PrimaryAttributeGroupId;
                    if (newPrim == oldPrim) return;

                    if (Draft.SecondaryAttributeGroupId == newPrim)
                        Draft.SecondaryAttributeGroupId = oldPrim;
                    else if (Draft.TertiaryAttributeGroupId == newPrim)
                        Draft.TertiaryAttributeGroupId = oldPrim;

                    Draft.PrimaryAttributeGroupId = newPrim;
                    ResetAllAttributeSpending();
                    Refresh();
                },
                width: 400));

            var secNames = new List<string>();
            var secIds = new List<string>();
            for (int i = 0; i < groupIds.Count; i++)
            {
                if (groupIds[i] == Draft.PrimaryAttributeGroupId) continue;
                secIds.Add(groupIds[i]);
                secNames.Add(groupNames[i]);
            }

            int secIdx = secIds.IndexOf(Draft.SecondaryAttributeGroupId);
            if (secIdx < 0) secIdx = 0;
            if (secIds.Count > 0)
            {
                section.Add(UIWidgets.MakePickerBlock(
                    $"Вторичная ({rank.AttributePoints.Secondary} очков)",
                    secNames, secIdx,
                    i =>
                    {
                        if (i < 0 || i >= secIds.Count) return;
                        string newSec = secIds[i];
                        string oldSec = Draft.SecondaryAttributeGroupId;
                        if (newSec == oldSec) return;

                        if (Draft.TertiaryAttributeGroupId == newSec)
                            Draft.TertiaryAttributeGroupId = oldSec;

                        Draft.SecondaryAttributeGroupId = newSec;
                        ResetAllAttributeSpending();
                        Refresh();
                    },
                    width: 400));
            }

            string terId = "";
            foreach (var g in groupIds)
                if (g != Draft.PrimaryAttributeGroupId && g != Draft.SecondaryAttributeGroupId)
                { terId = g; break; }
            Draft.TertiaryAttributeGroupId = terId;

            string terName = "";
            foreach (var g in Chr.AttributeGroups)
                if (g.Id == terId) terName = g.Name;

            if (!string.IsNullOrEmpty(terName))
                section.Add(UIRoot.MakeMuted(
                    $"Третичная: {terName} ({rank.AttributePoints.Tertiary} очков). Определяется автоматически.", 12));

            parent.Add(section);
        }

        private void BuildAttributeDistributionSection(VisualElement parent, Rank rank)
        {
            BuildAttributeGroupBlock(parent, rank, Draft.PrimaryAttributeGroupId,
                "Первичная", rank.AttributePoints.Primary);
            BuildAttributeGroupBlock(parent, rank, Draft.SecondaryAttributeGroupId,
                "Вторичная", rank.AttributePoints.Secondary);
            BuildAttributeGroupBlock(parent, rank, Draft.TertiaryAttributeGroupId,
                "Третичная", rank.AttributePoints.Tertiary);
        }

        private void BuildAttributeGroupBlock(VisualElement parent, Rank rank, string groupId,
            string roleLabel, int limit)
        {
            if (string.IsNullOrEmpty(groupId)) return;

            var group = Chr.AttributeGroups.Find(g => g.Id == groupId);
            if (group == null) return;

            var section = UIWidgets.Section($"{roleLabel}: {group.Name}");

            var counter = UIRoot.MakeMuted("", 13);
            section.Add(counter);
            _attrGroupCounters[groupId] = counter;

            var attrs = Chr.Attributes.FindAll(a => a.GroupId == groupId);
            if (attrs.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("(в группе нет атрибутов)"));
                parent.Add(section);
                return;
            }

            foreach (var attr in attrs)
                section.Add(BuildAttributeRow(rank, attr));

            parent.Add(section);
        }

        private VisualElement BuildAttributeRow(Rank rank, AttributeDef attr)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;

            var nameLbl = new Label(attr.Name);
            nameLbl.style.fontSize = 14;
            nameLbl.style.color = UIRoot.TextPrimary;
            nameLbl.style.minWidth = 220;
            nameLbl.style.marginRight = 12;
            if (attr.Id == Draft.FavoredAttributeId)
            {
                nameLbl.text += "  ★";
                nameLbl.style.color = new Color(0.95f, 0.8f, 0.35f);
            }
            row.Add(nameLbl);

            var minusBtn = new Button();
            minusBtn.text = "−";
            minusBtn.style.width = 36;
            minusBtn.style.height = 32;
            minusBtn.style.fontSize = 18;
            minusBtn.style.backgroundColor = UIRoot.AccentRed;
            minusBtn.style.color = Color.white;
            minusBtn.style.borderTopLeftRadius = 4;
            minusBtn.style.borderTopRightRadius = 4;
            minusBtn.style.borderBottomLeftRadius = 4;
            minusBtn.style.borderBottomRightRadius = 4;
            minusBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
            minusBtn.clicked += () => ChangeAttribute(rank, attr.Id, -1);
            row.Add(minusBtn);
            _attrMinusButtons[attr.Id] = minusBtn;

            var valueLbl = new Label("1");
            valueLbl.style.fontSize = 18;
            valueLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueLbl.style.color = UIRoot.TextPrimary;
            valueLbl.style.minWidth = 60;
            valueLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueLbl.style.marginLeft = 6;
            valueLbl.style.marginRight = 6;
            row.Add(valueLbl);
            _attrValueLabels[attr.Id] = valueLbl;

            var plusBtn = new Button();
            plusBtn.text = "+";
            plusBtn.style.width = 36;
            plusBtn.style.height = 32;
            plusBtn.style.fontSize = 18;
            plusBtn.style.backgroundColor = UIRoot.AccentGreen;
            plusBtn.style.color = Color.white;
            plusBtn.style.borderTopLeftRadius = 4;
            plusBtn.style.borderTopRightRadius = 4;
            plusBtn.style.borderBottomLeftRadius = 4;
            plusBtn.style.borderBottomRightRadius = 4;
            plusBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
            plusBtn.clicked += () => ChangeAttribute(rank, attr.Id, +1);
            row.Add(plusBtn);
            _attrPlusButtons[attr.Id] = plusBtn;

            int hardMax = GetAttributeHardCap(rank);
            var maxLbl = UIRoot.MakeMuted($"(создание: до {rank.AttributeMax}, итог до {hardMax})", 11);
            maxLbl.style.marginLeft = 12;
            row.Add(maxLbl);

            var zeroWarnLbl = UIRoot.MakeLabel("⚠ станет 0, крайне не рекомендуется!",
                11, new Color(1f, 0.4f, 0.4f));
            zeroWarnLbl.style.marginLeft = 10;
            zeroWarnLbl.style.display = DisplayStyle.None;
            zeroWarnLbl.name = $"zero-warn-{attr.Id}";
            row.Add(zeroWarnLbl);

            return row;
        }

        private void EnsureAttributesInitialized(Rank rank)
        {
            if (Draft.Attributes == null) Draft.Attributes = new Dictionary<string, int>();
            foreach (var attr in Chr.Attributes)
            {
                if (!Draft.Attributes.ContainsKey(attr.Id))
                    Draft.Attributes[attr.Id] = (attr.Id == Draft.FavoredAttributeId) ? 2 : 1;
            }
            if (!string.IsNullOrEmpty(Draft.FavoredAttributeId))
            {
                if (Draft.Attributes.TryGetValue(Draft.FavoredAttributeId, out int v) && v < 2)
                    Draft.Attributes[Draft.FavoredAttributeId] = 2;
            }
        }

        private int GetAttrBaseValue(string attrId)
            => attrId == Draft.FavoredAttributeId ? 2 : 1;

        private int GetSpentInAttrGroup(string groupId)
        {
            int spent = 0;
            foreach (var attr in Chr.Attributes)
            {
                if (attr.GroupId != groupId) continue;
                if (!Draft.Attributes.TryGetValue(attr.Id, out int val)) continue;
                int delta = val - GetAttrBaseValue(attr.Id);
                if (delta > 0) spent += delta;
            }
            return spent;
        }

        private int GetTotalAttrSpent()
        {
            int sum = 0;
            foreach (var group in Chr.AttributeGroups)
                sum += GetSpentInAttrGroup(group.Id);
            return sum;
        }

        private int GetTotalAttrLimit(Rank rank)
            => rank.AttributePoints.Primary + rank.AttributePoints.Secondary + rank.AttributePoints.Tertiary;

        private void ChangeAttribute(Rank rank, string attrId, int delta)
        {
            if (!Draft.Attributes.TryGetValue(attrId, out int val)) val = 1;
            int newVal = val + delta;

            int baseVal = GetAttrBaseValue(attrId);
            if (newVal < baseVal) return;
            if (newVal > rank.AttributeMax) return;

            int bonus = GetMeritBonus("attr." + attrId);
            int newEffective = newVal + bonus;
            int hardMax = GetAttributeHardCap(rank);
            if (newEffective < 0) return;
            if (newEffective > hardMax) return;

            if (delta > 0)
            {
                var attr = Chr.FindAttribute(attrId);
                if (attr == null) return;

                int limit;
                if (attr.GroupId == Draft.PrimaryAttributeGroupId) limit = rank.AttributePoints.Primary;
                else if (attr.GroupId == Draft.SecondaryAttributeGroupId) limit = rank.AttributePoints.Secondary;
                else if (attr.GroupId == Draft.TertiaryAttributeGroupId) limit = rank.AttributePoints.Tertiary;
                else return;

                if (GetSpentInAttrGroup(attr.GroupId) >= limit) return;
            }

            Draft.Attributes[attrId] = newVal;
            UpdateAllAttributeLabels(rank);
        }

        private void UpdateAllAttributeLabels(Rank rank)
        {
            int hardMax = GetAttributeHardCap(rank);

            foreach (var attr in Chr.Attributes)
            {
                if (!Draft.Attributes.TryGetValue(attr.Id, out int val)) continue;

                int bonus = GetMeritBonus("attr." + attr.Id);
                int effective = val + bonus;

                if (_attrValueLabels.TryGetValue(attr.Id, out var lbl))
                {
                    lbl.text = bonus == 0
                        ? effective.ToString()
                        : $"{effective} ({(bonus > 0 ? "+" : "")}{bonus})";
                    lbl.style.color = effective == 0
                        ? new Color(1f, 0.4f, 0.4f)
                        : UIRoot.TextPrimary;
                }

                var zeroWarn = _content.Q<Label>($"zero-warn-{attr.Id}");
                if (zeroWarn != null)
                {
                    zeroWarn.style.display = effective == 0
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }

                int baseVal = GetAttrBaseValue(attr.Id);
                if (_attrMinusButtons.TryGetValue(attr.Id, out var mb))
                    mb.SetEnabled(val > baseVal);
                if (_attrPlusButtons.TryGetValue(attr.Id, out var pb))
                {
                    int limit = 0;
                    if (attr.GroupId == Draft.PrimaryAttributeGroupId) limit = rank.AttributePoints.Primary;
                    else if (attr.GroupId == Draft.SecondaryAttributeGroupId) limit = rank.AttributePoints.Secondary;
                    else if (attr.GroupId == Draft.TertiaryAttributeGroupId) limit = rank.AttributePoints.Tertiary;

                    bool hasRoom = GetSpentInAttrGroup(attr.GroupId) < limit;
                    bool withinCreationMax = (val + 1) <= rank.AttributeMax;
                    bool withinHardMax = (val + 1 + bonus) <= hardMax;
                    pb.SetEnabled(hasRoom && withinCreationMax && withinHardMax);
                }
            }

            UpdateAttrGroupCounter(Draft.PrimaryAttributeGroupId, rank.AttributePoints.Primary);
            UpdateAttrGroupCounter(Draft.SecondaryAttributeGroupId, rank.AttributePoints.Secondary);
            UpdateAttrGroupCounter(Draft.TertiaryAttributeGroupId, rank.AttributePoints.Tertiary);

            if (_totalAttrSummary != null)
            {
                int spent = GetTotalAttrSpent();
                int limit = GetTotalAttrLimit(rank);
                _totalAttrSummary.text = $"Всего распределено: {spent} / {limit} очков.";
            }
        }

        private void UpdateAttrGroupCounter(string groupId, int limit)
        {
            if (string.IsNullOrEmpty(groupId)) return;
            if (!_attrGroupCounters.TryGetValue(groupId, out var counter)) return;
            int spent = GetSpentInAttrGroup(groupId);
            counter.text = $"Потрачено: {spent} / {limit}";
            counter.style.color = spent == limit
                ? new Color(0.55f, 0.85f, 0.55f)
                : new Color(0.95f, 0.7f, 0.3f);
        }

        // ============================================================
        // ШАГ 3: Навыки
        // ============================================================

        private void BuildStep_Skills(VisualElement parent)
        {
            parent.Add(UIRoot.MakeHeader("Шаг 3: Навыки"));

            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null)
            {
                parent.Add(UIRoot.MakeLabel("Ранг не выбран. Вернитесь на шаг 1.",
                    14, new Color(1f, 0.55f, 0.55f)));
                return;
            }

            EnsureSkillsInitialized(rank);
            BuildSkillPrioritiesSection(parent, rank);
            BuildSkillDistributionSection(parent, rank);

            var totalSection = UIWidgets.Section("Итог");
            _totalSkillSummary = UIRoot.MakeMuted("", 13);
            totalSection.Add(_totalSkillSummary);
            parent.Add(totalSection);

            BuildSpecializationsSection(parent, rank);

            UpdateAllSkillLabels(rank);
        }

        private void BuildSkillPrioritiesSection(VisualElement parent, Rank rank)
        {
            var section = UIWidgets.Section("Приоритеты групп навыков");
            section.Add(UIRoot.MakeMuted(
                "Первичная группа получит больше всего очков, вторичная — среднее, третичная — меньшее."));

            var groupNames = new List<string>();
            var groupIds = new List<string>();
            foreach (var g in Chr.SkillGroups) { groupIds.Add(g.Id); groupNames.Add(g.Name); }

            if (groupIds.Count < 3)
            {
                section.Add(UIRoot.MakeLabel("⚠ Нужно минимум 3 группы навыков.",
                    13, new Color(1f, 0.55f, 0.55f)));
                parent.Add(section);
                return;
            }

            int primIdx = groupIds.IndexOf(Draft.PrimarySkillGroupId);
            if (primIdx < 0) primIdx = 0;
            section.Add(UIWidgets.MakePickerBlock(
                $"Первичная ({rank.SkillPoints.Primary} очков)",
                groupNames, primIdx,
                i =>
                {
                    if (i < 0 || i >= groupIds.Count) return;
                    string newPrim = groupIds[i];
                    string oldPrim = Draft.PrimarySkillGroupId;
                    if (newPrim == oldPrim) return;

                    if (Draft.SecondarySkillGroupId == newPrim)
                        Draft.SecondarySkillGroupId = oldPrim;
                    else if (Draft.TertiarySkillGroupId == newPrim)
                        Draft.TertiarySkillGroupId = oldPrim;

                    Draft.PrimarySkillGroupId = newPrim;
                    ResetAllSkillSpending();
                    Refresh();
                },
                width: 400));

            var secNames = new List<string>();
            var secIds = new List<string>();
            for (int i = 0; i < groupIds.Count; i++)
            {
                if (groupIds[i] == Draft.PrimarySkillGroupId) continue;
                secIds.Add(groupIds[i]);
                secNames.Add(groupNames[i]);
            }

            int secIdx = secIds.IndexOf(Draft.SecondarySkillGroupId);
            if (secIdx < 0) secIdx = 0;
            if (secIds.Count > 0)
            {
                section.Add(UIWidgets.MakePickerBlock(
                    $"Вторичная ({rank.SkillPoints.Secondary} очков)",
                    secNames, secIdx,
                    i =>
                    {
                        if (i < 0 || i >= secIds.Count) return;
                        string newSec = secIds[i];
                        string oldSec = Draft.SecondarySkillGroupId;
                        if (newSec == oldSec) return;

                        if (Draft.TertiarySkillGroupId == newSec)
                            Draft.TertiarySkillGroupId = oldSec;

                        Draft.SecondarySkillGroupId = newSec;
                        ResetAllSkillSpending();
                        Refresh();
                    },
                    width: 400));
            }

            string terId = "";
            foreach (var g in groupIds)
                if (g != Draft.PrimarySkillGroupId && g != Draft.SecondarySkillGroupId)
                { terId = g; break; }
            Draft.TertiarySkillGroupId = terId;

            string terName = "";
            foreach (var g in Chr.SkillGroups)
                if (g.Id == terId) terName = g.Name;

            if (!string.IsNullOrEmpty(terName))
                section.Add(UIRoot.MakeMuted(
                    $"Третичная: {terName} ({rank.SkillPoints.Tertiary} очков). Определяется автоматически.", 12));

            parent.Add(section);
        }

        private void BuildSkillDistributionSection(VisualElement parent, Rank rank)
        {
            BuildSkillGroupBlock(parent, rank, Draft.PrimarySkillGroupId,
                "Первичная", rank.SkillPoints.Primary);
            BuildSkillGroupBlock(parent, rank, Draft.SecondarySkillGroupId,
                "Вторичная", rank.SkillPoints.Secondary);
            BuildSkillGroupBlock(parent, rank, Draft.TertiarySkillGroupId,
                "Третичная", rank.SkillPoints.Tertiary);
        }

        private void BuildSkillGroupBlock(VisualElement parent, Rank rank, string groupId,
            string roleLabel, int limit)
        {
            if (string.IsNullOrEmpty(groupId)) return;

            var group = Chr.SkillGroups.Find(g => g.Id == groupId);
            if (group == null) return;

            var section = UIWidgets.Section($"{roleLabel}: {group.Name}");

            var counter = UIRoot.MakeMuted("", 13);
            section.Add(counter);
            _skillGroupCounters[groupId] = counter;

            var skills = Chr.Skills.FindAll(s => s.GroupId == groupId);
            if (skills.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("(в группе нет навыков)"));
                parent.Add(section);
                return;
            }

            foreach (var skill in skills)
                section.Add(BuildSkillRow(rank, skill));

            parent.Add(section);
        }

        private VisualElement BuildSkillRow(Rank rank, SkillDef skill)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            string skillName = Chr.GetSkillName(skill);
            var nameLbl = new Label(skillName);
            nameLbl.style.fontSize = 14;
            nameLbl.style.color = UIRoot.TextPrimary;
            nameLbl.style.minWidth = 220;
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
            minusBtn.clicked += () => ChangeSkill(rank, skill.Id, -1);
            row.Add(minusBtn);
            _skillMinusButtons[skill.Id] = minusBtn;

            var valueLbl = new Label("0");
            valueLbl.style.fontSize = 16;
            valueLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueLbl.style.color = UIRoot.TextPrimary;
            valueLbl.style.minWidth = 60;
            valueLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueLbl.style.marginLeft = 6;
            valueLbl.style.marginRight = 6;
            row.Add(valueLbl);
            _skillValueLabels[skill.Id] = valueLbl;

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
            plusBtn.clicked += () => ChangeSkill(rank, skill.Id, +1);
            row.Add(plusBtn);
            _skillPlusButtons[skill.Id] = plusBtn;

            int hardMax = GetSkillHardCap(rank);
            var maxLbl = UIRoot.MakeMuted($"(создание: до {rank.SkillMax}, итог до {hardMax})", 11);
            maxLbl.style.marginLeft = 12;
            row.Add(maxLbl);

            return row;
        }

        private void BuildSpecializationsSection(VisualElement parent, Rank rank)
        {
            var section = UIWidgets.Section("Специализации");
            section.Add(UIRoot.MakeMuted(
                "Специализации можно добавить к любому навыку со значением 1 и больше. " +
                "Максимум специализаций у одного навыка: 10 + Интеллект + значение навыка."));

            _specCounterLabel = UIRoot.MakeLabel("", 14);
            _specCounterLabel.style.marginBottom = 8;
            section.Add(_specCounterLabel);

            _specListContainer = new VisualElement();
            section.Add(_specListContainer);

            parent.Add(section);

            RebuildSpecializationList(rank);
        }

        private void RebuildSpecializationList(Rank rank)
        {
            if (_specListContainer == null) return;
            _specListContainer.Clear();

            int used = CountSpecializations();
            int limit = rank.SpecializationPoints;
            _specCounterLabel.text = $"Использовано специализаций: {used} / {limit}";
            _specCounterLabel.style.color = used == limit
                ? new Color(0.55f, 0.85f, 0.55f)
                : (used > limit
                    ? new Color(1f, 0.55f, 0.55f)
                    : new Color(0.95f, 0.7f, 0.3f));

            var eligible = new List<SkillDef>();
            foreach (var s in Chr.Skills)
            {
                if (Draft.Skills.TryGetValue(s.Id, out int val) && val >= 1)
                    eligible.Add(s);
            }

            if (eligible.Count == 0)
            {
                _specListContainer.Add(UIRoot.MakeMuted(
                    "Пока ни один навык не имеет значения 1 — специализации недоступны."));
                return;
            }

            foreach (var skill in eligible)
            {
                var skillLocal = skill;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;
                row.style.paddingTop = 4;
                row.style.paddingBottom = 4;

                var skillLbl = new Label(Chr.GetSkillName(skillLocal));
                skillLbl.style.fontSize = 14;
                skillLbl.style.color = UIRoot.TextPrimary;
                skillLbl.style.minWidth = 220;
                skillLbl.style.marginRight = 12;
                row.Add(skillLbl);

                var specsList = Draft.Specializations.TryGetValue(skillLocal.Id, out var lst)
                    ? lst : new List<string>();

                int maxForSkill = 10 + GetAttributeValue("intelligence") + GetSkillValue(skillLocal.Id);

                if (specsList.Count > 0)
                {
                    var chips = new VisualElement();
                    chips.style.flexDirection = FlexDirection.Row;
                    chips.style.flexWrap = Wrap.Wrap;
                    chips.style.flexGrow = 1;
                    foreach (var spec in specsList)
                    {
                        string specLocal = spec;
                        var chip = new VisualElement();
                        chip.style.flexDirection = FlexDirection.Row;
                        chip.style.alignItems = Align.Center;
                        chip.style.marginRight = 6;
                        chip.style.marginBottom = 2;
                        chip.style.paddingLeft = 10;
                        chip.style.paddingRight = 4;
                        chip.style.paddingTop = 3;
                        chip.style.paddingBottom = 3;
                        chip.style.backgroundColor = UIRoot.AccentBlue;
                        chip.style.borderTopLeftRadius = 12;
                        chip.style.borderTopRightRadius = 12;
                        chip.style.borderBottomLeftRadius = 12;
                        chip.style.borderBottomRightRadius = 12;

                        var specLbl = new Label(specLocal);
                        specLbl.style.fontSize = 12;
                        specLbl.style.color = Color.white;
                        specLbl.style.marginRight = 6;
                        chip.Add(specLbl);

                        var xBtn = new Button(() =>
                        {
                            specsList.Remove(specLocal);
                            RebuildSpecializationList(rank);
                        });
                        xBtn.text = "✕";
                        xBtn.style.width = 20;
                        xBtn.style.height = 20;
                        xBtn.style.fontSize = 11;
                        xBtn.style.color = Color.white;
                        xBtn.style.backgroundColor = new Color(0.45f, 0.15f, 0.15f);
                        xBtn.style.marginLeft = 0;
                        xBtn.style.marginRight = 0;
                        xBtn.style.marginTop = 0;
                        xBtn.style.marginBottom = 0;
                        xBtn.style.paddingLeft = 0;
                        xBtn.style.paddingRight = 0;
                        xBtn.style.borderTopLeftRadius = 10;
                        xBtn.style.borderTopRightRadius = 10;
                        xBtn.style.borderBottomLeftRadius = 10;
                        xBtn.style.borderBottomRightRadius = 10;
                        xBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
                        chip.Add(xBtn);

                        chips.Add(chip);
                    }
                    row.Add(chips);
                }
                else
                {
                    var spacer = new VisualElement();
                    spacer.style.flexGrow = 1;
                    row.Add(spacer);
                }

                var countLbl = UIRoot.MakeMuted($"{specsList.Count}/{maxForSkill}", 11);
                countLbl.style.marginRight = 8;
                row.Add(countLbl);

                bool canAdd = used < limit && specsList.Count < maxForSkill;
                var addBtn = new Button(() => ShowSpecializationInputDialog(skillLocal, rank));
                addBtn.text = "＋ Добавить";
                addBtn.style.height = 28;
                addBtn.style.fontSize = 12;
                addBtn.style.paddingLeft = 10;
                addBtn.style.paddingRight = 10;
                addBtn.style.backgroundColor = canAdd ? UIRoot.AccentGreen : UIRoot.AccentNeutral;
                addBtn.style.color = Color.white;
                addBtn.style.borderTopLeftRadius = 4;
                addBtn.style.borderTopRightRadius = 4;
                addBtn.style.borderBottomLeftRadius = 4;
                addBtn.style.borderBottomRightRadius = 4;
                addBtn.SetEnabled(canAdd);
                row.Add(addBtn);

                _specListContainer.Add(row);
            }
        }

        private void ShowSpecializationInputDialog(SkillDef skill, Rank rank)
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
            dialog.style.width = 420;
            dialog.style.paddingTop = 18;
            dialog.style.paddingBottom = 18;
            dialog.style.paddingLeft = 18;
            dialog.style.paddingRight = 18;
            dialog.style.backgroundColor = UIRoot.BgPanel;
            dialog.style.borderTopLeftRadius = 8;
            dialog.style.borderTopRightRadius = 8;
            dialog.style.borderBottomLeftRadius = 8;
            dialog.style.borderBottomRightRadius = 8;

            var title = new Label($"Специализация для «{Chr.GetSkillName(skill)}»");
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = UIRoot.TextPrimary;
            title.style.marginBottom = 10;
            dialog.Add(title);

            var input = new TextField("Название");
            input.value = "";
            UIRoot.StyleTextField(input, "Название");
            dialog.Add(input);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            btnRow.style.justifyContent = Justify.FlexEnd;
            dialog.Add(btnRow);

            var cancelBtn = new Button(() => panelRoot.Remove(overlay));
            cancelBtn.text = "Отмена";
            UIRoot.StyleButton(cancelBtn, UIRoot.AccentNeutral, Color.white, 32);
            cancelBtn.style.width = 100;
            btnRow.Add(cancelBtn);

            var okBtn = new Button(() =>
            {
                string name = (input.value ?? "").Trim();
                if (string.IsNullOrEmpty(name)) return;

                if (!Draft.Specializations.ContainsKey(skill.Id))
                    Draft.Specializations[skill.Id] = new List<string>();
                Draft.Specializations[skill.Id].Add(name);

                panelRoot.Remove(overlay);
                RebuildSpecializationList(rank);
            });
            okBtn.text = "Добавить";
            UIRoot.StyleButton(okBtn, UIRoot.AccentGreen, Color.white, 32);
            okBtn.style.width = 100;
            btnRow.Add(okBtn);

            overlay.Add(dialog);
            overlay.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == overlay) panelRoot.Remove(overlay);
            });
            panelRoot.Add(overlay);

            input.schedule.Execute(() => input.Focus()).ExecuteLater(20);
        }

        private int CountSpecializations()
        {
            int sum = 0;
            foreach (var kv in Draft.Specializations)
                sum += kv.Value.Count;
            return sum;
        }

        private int GetAttributeValue(string attrId)
        {
            if (Draft.Attributes != null && Draft.Attributes.TryGetValue(attrId, out int v))
                return v;
            return 1;
        }

        private int GetSkillValue(string skillId)
        {
            if (Draft.Skills != null && Draft.Skills.TryGetValue(skillId, out int v))
                return v;
            return 0;
        }

        private void EnsureSkillsInitialized(Rank rank)
        {
            if (Draft.Skills == null) Draft.Skills = new Dictionary<string, int>();
            foreach (var skill in Chr.Skills)
            {
                if (!Draft.Skills.ContainsKey(skill.Id))
                    Draft.Skills[skill.Id] = 0;
            }
            if (Draft.Specializations == null)
                Draft.Specializations = new Dictionary<string, List<string>>();
        }

        private int GetSpentInSkillGroup(string groupId)
        {
            int spent = 0;
            foreach (var skill in Chr.Skills)
            {
                if (skill.GroupId != groupId) continue;
                if (!Draft.Skills.TryGetValue(skill.Id, out int val)) continue;
                if (val > 0) spent += val;
            }
            return spent;
        }

        private int GetTotalSkillSpent()
        {
            int sum = 0;
            foreach (var group in Chr.SkillGroups)
                sum += GetSpentInSkillGroup(group.Id);
            return sum;
        }

        private int GetTotalSkillLimit(Rank rank)
            => rank.SkillPoints.Primary + rank.SkillPoints.Secondary + rank.SkillPoints.Tertiary;

        private void ChangeSkill(Rank rank, string skillId, int delta)
        {
            if (!Draft.Skills.TryGetValue(skillId, out int val)) val = 0;
            int newVal = val + delta;

            if (newVal < 0) return;
            if (newVal > rank.SkillMax) return;

            int bonus = GetMeritBonus("skill." + skillId);
            int newEffective = newVal + bonus;
            int hardMax = GetSkillHardCap(rank);
            if (newEffective > hardMax) return;

            if (delta > 0)
            {
                var skill = Chr.FindSkill(skillId);
                if (skill == null) return;

                int limit;
                if (skill.GroupId == Draft.PrimarySkillGroupId) limit = rank.SkillPoints.Primary;
                else if (skill.GroupId == Draft.SecondarySkillGroupId) limit = rank.SkillPoints.Secondary;
                else if (skill.GroupId == Draft.TertiarySkillGroupId) limit = rank.SkillPoints.Tertiary;
                else return;

                if (GetSpentInSkillGroup(skill.GroupId) >= limit) return;
            }

            Draft.Skills[skillId] = newVal;

            if (newVal == 0 && Draft.Specializations.ContainsKey(skillId))
                Draft.Specializations[skillId].Clear();

            UpdateAllSkillLabels(rank);
            RebuildSpecializationList(rank);
        }

        private void UpdateAllSkillLabels(Rank rank)
        {
            int hardMax = GetSkillHardCap(rank);

            foreach (var skill in Chr.Skills)
            {
                if (!Draft.Skills.TryGetValue(skill.Id, out int val)) continue;

                int bonus = GetMeritBonus("skill." + skill.Id);
                int effective = val + bonus;

                if (_skillValueLabels.TryGetValue(skill.Id, out var lbl))
                {
                    lbl.text = bonus == 0
                        ? effective.ToString()
                        : $"{effective} ({(bonus > 0 ? "+" : "")}{bonus})";
                    lbl.style.color = effective == 0
                        ? new Color(1f, 0.4f, 0.4f)
                        : UIRoot.TextPrimary;
                }

                if (_skillMinusButtons.TryGetValue(skill.Id, out var mb))
                    mb.SetEnabled(val > 0);
                if (_skillPlusButtons.TryGetValue(skill.Id, out var pb))
                {
                    int limit = 0;
                    if (skill.GroupId == Draft.PrimarySkillGroupId) limit = rank.SkillPoints.Primary;
                    else if (skill.GroupId == Draft.SecondarySkillGroupId) limit = rank.SkillPoints.Secondary;
                    else if (skill.GroupId == Draft.TertiarySkillGroupId) limit = rank.SkillPoints.Tertiary;

                    bool hasRoom = GetSpentInSkillGroup(skill.GroupId) < limit;
                    bool withinCreationMax = (val + 1) <= rank.SkillMax;
                    bool withinHardMax = (val + 1 + bonus) <= hardMax;
                    pb.SetEnabled(hasRoom && withinCreationMax && withinHardMax);
                }
            }

            UpdateSkillGroupCounter(Draft.PrimarySkillGroupId, rank.SkillPoints.Primary);
            UpdateSkillGroupCounter(Draft.SecondarySkillGroupId, rank.SkillPoints.Secondary);
            UpdateSkillGroupCounter(Draft.TertiarySkillGroupId, rank.SkillPoints.Tertiary);

            if (_totalSkillSummary != null)
            {
                int spent = GetTotalSkillSpent();
                int limit = GetTotalSkillLimit(rank);
                _totalSkillSummary.text = $"Всего распределено: {spent} / {limit} очков.";
            }
        }

        private void UpdateSkillGroupCounter(string groupId, int limit)
        {
            if (string.IsNullOrEmpty(groupId)) return;
            if (!_skillGroupCounters.TryGetValue(groupId, out var counter)) return;
            int spent = GetSpentInSkillGroup(groupId);
            counter.text = $"Потрачено: {spent} / {limit}";
            counter.style.color = spent == limit
                ? new Color(0.55f, 0.85f, 0.55f)
                : new Color(0.95f, 0.7f, 0.3f);
        }

        // ============================================================
        // Заглушка
        // ============================================================

        private void BuildPlaceholder(VisualElement parent, string title, string note)
        {
            parent.Add(UIRoot.MakeHeader(title));
            parent.Add(UIRoot.MakeMuted(note));
        }

        // ============================================================
        // Ранги / утилиты
        // ============================================================

        private List<Rank> GetAvailableRanks()
        {
            var occupied = CollectRankOccupancy(Chr.Id);
            var result = new List<Rank>();
            foreach (var r in Chr.Ranks)
            {
                if (r.MaxPlayers <= 0) continue;
                int taken = occupied.TryGetValue(r.Id, out var n) ? n : 0;
                if (taken < r.MaxPlayers) result.Add(r);
            }
            return result;
        }

        private Dictionary<string, int> CollectRankOccupancy(string chronicleId)
        {
            var result = new Dictionary<string, int>();
            JsonSaveSystem.EnsureFolders();
            if (!Directory.Exists(JsonSaveSystem.CharactersPath)) return result;

            foreach (var f in Directory.GetFiles(JsonSaveSystem.CharactersPath, "*.json"))
            {
                try
                {
                    var c = JsonSaveSystem.LoadCharacter(Path.GetFileNameWithoutExtension(f));
                    if (c == null || c.ChronicleId != chronicleId) continue;
                    if (string.IsNullOrEmpty(c.RankId)) continue;
                    if (result.TryGetValue(c.RankId, out int n)) result[c.RankId] = n + 1;
                    else result[c.RankId] = 1;
                }
                catch { }
            }
            return result;
        }

        private void ApplyRankDefaults()
        {
            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null) return;
            if (string.IsNullOrEmpty(Draft.MoralityId) && Chr.Moralites.Count > 0)
                Draft.MoralityId = Chr.Moralites[0].Id;
        }

        private string GetRankSummary(Rank rank)
        {
            return $"Стартовая мораль: {rank.StartHumanity}, " +
                   $"стартовая густота: {rank.StartBloodPotency}, " +
                   $"очки атрибутов: {rank.AttributePoints.Primary}/{rank.AttributePoints.Secondary}/{rank.AttributePoints.Tertiary}";
        }

        // ============================================================
        // Валидация
        // ============================================================

        private bool ValidateStep(int step, out string error)
        {
            error = "";
            switch (step)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(Draft.Name)) { error = "Введите имя персонажа."; return false; }
                    if (string.IsNullOrEmpty(Draft.RankId)) { error = "Выберите ранг."; return false; }
                    if (string.IsNullOrEmpty(Draft.ClanId)) { error = "Выберите клан."; return false; }
                    if (string.IsNullOrEmpty(Draft.Nature)) { error = "Выберите натуру."; return false; }
                    if (string.IsNullOrEmpty(Draft.Mask)) { error = "Выберите маску."; return false; }
                    if (string.IsNullOrEmpty(Draft.FavoredAttributeId)) { error = "Выберите любимый атрибут."; return false; }
                    if (!string.IsNullOrEmpty(Draft.BloodlineId))
                    {
                        var bl = Chr.FindBloodline(Draft.BloodlineId);
                        if (bl == null || bl.ClanId != Draft.ClanId)
                        { error = "Бладлайн не относится к выбранному клану."; return false; }
                    }
                    return true;

                case 1:
                    var rankA = Chr.FindRank(Draft.RankId);
                    if (rankA == null) { error = "Ранг не выбран."; return false; }
                    if (string.IsNullOrEmpty(Draft.PrimaryAttributeGroupId))
                    { error = "Выберите первичную группу атрибутов."; return false; }
                    if (string.IsNullOrEmpty(Draft.SecondaryAttributeGroupId))
                    { error = "Выберите вторичную группу атрибутов."; return false; }
                    if (string.IsNullOrEmpty(Draft.TertiaryAttributeGroupId))
                    { error = "Третичная группа не определена."; return false; }

                    int spentA = GetTotalAttrSpent();
                    int limitA = GetTotalAttrLimit(rankA);
                    if (spentA != limitA)
                    { error = $"Распределите все очки атрибутов: {spentA} из {limitA}."; return false; }

                    foreach (var attr in Chr.Attributes)
                    {
                        if (!Draft.Attributes.TryGetValue(attr.Id, out int val)) continue;
                        if (val > rankA.AttributeMax)
                        { error = $"Атрибут «{attr.Name}» превышает максимум ({rankA.AttributeMax})."; return false; }
                    }
                    return true;

                case 2:
                    var rankS = Chr.FindRank(Draft.RankId);
                    if (rankS == null) { error = "Ранг не выбран."; return false; }
                    if (string.IsNullOrEmpty(Draft.PrimarySkillGroupId))
                    { error = "Выберите первичную группу навыков."; return false; }
                    if (string.IsNullOrEmpty(Draft.SecondarySkillGroupId))
                    { error = "Выберите вторичную группу навыков."; return false; }
                    if (string.IsNullOrEmpty(Draft.TertiarySkillGroupId))
                    { error = "Третичная группа не определена."; return false; }

                    int spentS = GetTotalSkillSpent();
                    int limitS = GetTotalSkillLimit(rankS);
                    if (spentS != limitS)
                    { error = $"Распределите все очки навыков: {spentS} из {limitS}."; return false; }

                    foreach (var skill in Chr.Skills)
                    {
                        if (!Draft.Skills.TryGetValue(skill.Id, out int val)) continue;
                        if (val > rankS.SkillMax)
                        { error = $"Навык «{Chr.GetSkillName(skill)}» превышает максимум ({rankS.SkillMax})."; return false; }
                    }

                    int usedSpec = CountSpecializations();
                    if (usedSpec > rankS.SpecializationPoints)
                    { error = $"Специализаций больше, чем доступно ({usedSpec}/{rankS.SpecializationPoints})."; return false; }

                    return true;

                case 3:
                    return ValidateMeritsStep(out error);

                case 4:
                    return ValidateDisciplinesStep(out error);

                default:
                    return true;
            }
        }

        private void ShowError(string msg)
        {
            _errorLabel.text = $"⚠ {msg}";
            _errorLabel.style.display = DisplayStyle.Flex;
            Debug.LogWarning($"[Wizard] {msg}");
        }

        private void HideError()
        {
            if (_errorLabel != null)
                _errorLabel.style.display = DisplayStyle.None;
        }

        // ============================================================
        // Завершение создания
        // ============================================================

        private void FinishCreation()
        {
            var rank = Chr.FindRank(Draft.RankId);
            if (rank == null)
            {
                ShowError("Ранг не выбран.");
                return;
            }

            for (int s = 0; s <= 4; s++)
            {
                if (!ValidateStep(s, out string err))
                {
                    ShowError($"Шаг {s + 1}: {err}");
                    Draft.CurrentStep = s;
                    Refresh();
                    return;
                }
            }

            var chr = new Character
            {
                Id = JsonSaveSystem.NewId("char"),
                PlayerId = SessionContext.PlayerName,
                PlayerName = SessionContext.PlayerName,
                ChronicleId = Chr.Id,

                Name = Draft.Name,
                ClanId = Draft.ClanId,
                BloodlineId = Draft.BloodlineId,
                RankId = Draft.RankId,
                Nature = Draft.Nature,
                Mask = Draft.Mask,
                MoralityId = Draft.MoralityId,
                FavoredAttributeId = Draft.FavoredAttributeId,

                Attributes = new Dictionary<string, int>(Draft.Attributes),
                Skills = new Dictionary<string, int>(Draft.Skills),
                Specializations = CloneSpecs(Draft.Specializations),
                Disciplines = new Dictionary<string, int>(Draft.Disciplines),
                MeritIds = new List<string>(Draft.MeritIds),
                AddonLevels = new Dictionary<string, int>(Draft.AddonLevels),

                BirthDate = Draft.BirthDate,
                DeathDate = Draft.DeathDate,
                VisualAge = Draft.VisualAge,
                RealAge = Draft.RealAge,
                Nationality = Draft.Nationality,
                Gender = Draft.Gender,
                Build = Draft.Build,
                Height = Draft.Height,
                Weight = Draft.Weight,
                Hair = Draft.Hair,
                Eyes = Draft.Eyes,
                NotableFeatures = Draft.NotableFeatures,
                Backstory = Draft.Backstory,
                Haven = Draft.Haven,
            };

            var mor = Chr.FindMorality(chr.MoralityId);
            chr.MoralityRating = mor != null ? mor.StartRating : 7;

            chr.Aura = 0;
            try
            {
                var auraTable = DefaultsLoader.LoadAuraTable();
                chr.Aura = Core.Rules.AuraCalculator.Calculate(
                    auraTable, chr.MoralityRating, AuraType.Regular);
            }
            catch { }

            int resolve = chr.Attributes.TryGetValue("resolve", out int r) ? r : 1;
            int composure = chr.Attributes.TryGetValue("composure", out int c) ? c : 1;
            chr.WillpowerMax = Core.Rules.AuraCalculator.CalculateMaxWillpower(composure, resolve);
            chr.WillpowerCurrent = chr.WillpowerMax;

            chr.BloodPotency = GetEffectiveBloodPotency(rank);
            chr.BloodMax = GetMaxBloodPool(rank);
            chr.BloodCurrent = chr.BloodMax;

            int stamina = chr.Attributes.TryGetValue("stamina", out int st) ? st : 1;
            int fortitude = 0;
            if (chr.Disciplines.TryGetValue("fortitude", out int f))
                fortitude = f;
            chr.HealthMax = 5 + stamina + fortitude;
            chr.HealthBoxes = new DamageType[chr.HealthMax];
            for (int i = 0; i < chr.HealthMax; i++)
                chr.HealthBoxes[i] = DamageType.None;
            chr.HealthCurrent = chr.HealthMax;

            chr.XpTotal = rank.FirstCharBonus != null ? rank.FirstCharBonus.Xp : 0;
            chr.XpSpent = 0;

            if (!string.IsNullOrEmpty(Draft.PortraitPath) && File.Exists(Draft.PortraitPath))
            {
                try
                {
                    string ext = Path.GetExtension(Draft.PortraitPath);
                    if (string.IsNullOrEmpty(ext)) ext = ".png";
                    string dest = JsonSaveSystem.PortraitFile(chr.Id, ext.TrimStart('.'));
                    File.Copy(Draft.PortraitPath, dest, overwrite: true);
                    chr.PortraitPath = dest;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Wizard] Не удалось сохранить портрет: {e.Message}");
                }
            }

            JsonSaveSystem.SaveCharacter(chr);
            Debug.Log($"[Wizard] Персонаж создан: {chr.Name} ({chr.Id})");

            try
            {
                if (NetworkSession.IsPlayer && NetworkGameManager.Instance != null)
                {
                    string json = JsonSaveSystem.Serialize(chr);
                    NetworkGameManager.Instance.SendMyCharacter(json);
                    Debug.Log("[Wizard] Персонаж отправлен мастеру.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Wizard] Не удалось отправить персонажа мастеру: {e.Message}");
            }

            SessionContext.CurrentDraft = null;
            SessionContext.CurrentCharacter = chr;
            UIRoot.Instance.ShowCharacterList();
        }
    }
}