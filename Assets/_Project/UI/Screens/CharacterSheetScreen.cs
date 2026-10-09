using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Core.Rules;
using VTR.Data;
using VTR.Net;

namespace VTR.UI
{
    public partial class CharacterSheetScreen : IScreen
    {
        private Character _c;
        private Chronicle _chr;
        private VisualElement _root;

        private static bool _xpMode;
        private static bool _gmEditMode;

        public void Build(VisualElement root)
        {
            _root = root;
            _c = SessionContext.CurrentCharacter;
            _chr = SessionContext.CurrentChronicle;

            if (_c == null || _chr == null)
            {
                root.Add(UIRoot.MakeLabel("Персонаж или хроника не загружены.",
                    16, Color.red));
                return;
            }

            if (_c.Attributes == null) _c.Attributes = new Dictionary<string, int>();
            if (_c.Skills == null) _c.Skills = new Dictionary<string, int>();
            if (_c.Disciplines == null) _c.Disciplines = new Dictionary<string, int>();
            if (_c.Specializations == null) _c.Specializations = new Dictionary<string, List<string>>();
            if (_c.MeritIds == null) _c.MeritIds = new List<string>();
            if (_c.AddonLevels == null) _c.AddonLevels = new Dictionary<string, int>();

            RecalculateHealth();

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.flexDirection = FlexDirection.Column;
            _root.Add(container);

            BuildTopBar(container);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.paddingTop = 14;
            scroll.style.paddingBottom = 14;
            scroll.style.paddingLeft = 20;
            scroll.style.paddingRight = 20;
            container.Add(scroll);

            var twoCols = new VisualElement();
            twoCols.style.flexDirection = FlexDirection.Row;
            twoCols.style.flexWrap = Wrap.Wrap;
            scroll.Add(twoCols);

            var leftCol = new VisualElement();
            leftCol.style.width = 420;
            leftCol.style.marginRight = 16;
            twoCols.Add(leftCol);

            var rightCol = new VisualElement();
            rightCol.style.flexGrow = 1;
            rightCol.style.minWidth = 500;
            twoCols.Add(rightCol);

            BuildPortraitAndBasics(leftCol);
            BuildResources(leftCol);
            BuildMoralityAndAura(leftCol);
            if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                BuildGmExperienceSection(leftCol);

            BuildAttributes(rightCol);
            BuildSkills(rightCol);
            BuildDisciplines(rightCol);
            BuildMerits(rightCol);
            BuildAddons(rightCol);
        }

        private void RecalculateHealth()
        {
            int stamina = _c.Attributes.TryGetValue("stamina", out int st) ? st : 1;
            int fortitude = 0;
            if (_c.Disciplines.TryGetValue("fortitude", out int f))
                fortitude = f;
            int newMax = 5 + stamina + fortitude;
            if (newMax < 1) newMax = 1;

            _c.HealthMax = newMax;
            _c.EnsureHealthBoxes();
            _c.RecalculateHealthCurrent();
        }

        // ============================================================
        // Хелперы
        // ============================================================

        private float GetHealCost(DamageType type)
        {
            int bp = _c.BloodPotency;
            if (_chr.BloodPotencyHealCosts != null &&
                _chr.BloodPotencyHealCosts.TryGetValue(bp, out var cfg) && cfg != null)
            {
                switch (type)
                {
                    case DamageType.Bashing: return cfg.Bashing;
                    case DamageType.Lethal: return cfg.Lethal;
                    case DamageType.Aggravated: return cfg.Aggravated;
                }
            }
            switch (type)
            {
                case DamageType.Bashing: return 1f;
                case DamageType.Lethal: return 1f;
                case DamageType.Aggravated: return 5f;
            }
            return 1f;
        }

        // ============================================================
        // Верхняя панель
        // ============================================================

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
            parent.Add(bar);

            var title = UIRoot.MakeLabel(_c.Name, 22);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.flexGrow = 1;
            bar.Add(title);

            var xpLbl = UIRoot.MakeLabel(
                $"Опыт: {_c.XpAvailable}  (всего {_c.XpTotal}, потрачено {_c.XpSpent})", 13);
            xpLbl.style.marginRight = 16;
            xpLbl.style.color = _c.XpAvailable > 0
                ? new Color(0.55f, 0.85f, 0.55f)
                : UIRoot.TextMuted;
            bar.Add(xpLbl);

            if (!SessionContext.IsGMViewingCharacter)
            {
                var xpToggle = new Toggle("Тратить опыт") { value = _xpMode };
                UIWidgets.StyleToggle(xpToggle);
                xpToggle.style.marginRight = 12;
                xpToggle.RegisterValueChangedCallback(e =>
                {
                    _xpMode = e.newValue;
                    UIRoot.Instance.ShowCharacterSheet();
                });
                bar.Add(xpToggle);

                if (NetworkSession.IsPlayer && NetworkGameManager.Instance != null)
                {
                    var sendBtn = new Button(() =>
                    {
                        string json = JsonSaveSystem.Serialize(_c);
                        NetworkGameManager.Instance.SendMyCharacter(json);
                    });
                    sendBtn.text = "📤  Отправить мастеру";
                    UIRoot.StyleButton(sendBtn, UIRoot.AccentBlue, Color.white, 34);
                    sendBtn.style.width = 200;
                    sendBtn.style.marginRight = 8;
                    bar.Add(sendBtn);
                }
            }
            else
            {
                var gmToggle = new Toggle("Редактировать (мастер)") { value = _gmEditMode };
                UIWidgets.StyleToggle(gmToggle);
                gmToggle.style.marginRight = 12;
                gmToggle.RegisterValueChangedCallback(e =>
                {
                    _gmEditMode = e.newValue;
                    UIRoot.Instance.ShowCharacterSheet();
                });
                bar.Add(gmToggle);

                var badge = UIRoot.MakeMuted(
                    _gmEditMode ? "✏ Режим редактирования мастера" : "👁 Режим мастера (только чтение)",
                    12);
                badge.style.marginRight = 12;
                badge.style.color = _gmEditMode
                    ? new Color(0.95f, 0.7f, 0.4f)
                    : UIRoot.TextMuted;
                bar.Add(badge);
            }

            var backBtn = new Button(() =>
            {
                if (SessionContext.IsGMViewingCharacter)
                {
                    _gmEditMode = false;
                    SessionContext.IsGMViewingCharacter = false;
                    SessionContext.GMViewingOwnerId = 0;
                    SessionContext.CurrentCharacter = null;
                    UIRoot.Instance.ShowGMScreen();
                }
                else
                {
                    _xpMode = false;
                    SessionContext.CurrentCharacter = null;
                    UIRoot.Instance.ShowCharacterList();
                }
            });
            backBtn.text = "←  К списку";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 34);
            backBtn.style.width = 140;
            bar.Add(backBtn);
        }

        // ============================================================
        // Портрет и основное
        // ============================================================

        private void BuildPortraitAndBasics(VisualElement parent)
        {
            var section = UIWidgets.Section("Основное");

            if (!string.IsNullOrEmpty(_c.PortraitPath) && File.Exists(_c.PortraitPath))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(_c.PortraitPath);
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(bytes);

                    var img = new Image();
                    img.image = tex;
                    img.scaleMode = ScaleMode.ScaleToFit;
                    img.style.width = 200;
                    img.style.height = 200;
                    img.style.alignSelf = Align.Center;
                    img.style.marginBottom = 10;
                    img.style.backgroundColor = UIRoot.BgInput;
                    section.Add(img);
                }
                catch { }
            }

            section.Add(MakeInfoRow("Имя", _c.Name));

            var clan = _chr.FindClan(_c.ClanId);
            section.Add(MakeInfoRow("Клан", clan != null ? clan.Name : "—"));

            if (!string.IsNullOrEmpty(_c.BloodlineId))
            {
                var bl = _chr.FindBloodline(_c.BloodlineId);
                if (bl != null)
                    section.Add(MakeInfoRow("Бладлайн", bl.Name));
            }

            var rank = _chr.FindRank(_c.RankId);
            section.Add(MakeInfoRow("Ранг", rank != null ? rank.Name : "—"));

            section.Add(MakeInfoRow("Натура", _c.Nature));
            section.Add(MakeInfoRow("Маска", _c.Mask));

            if (clan != null && !string.IsNullOrEmpty(clan.Curse))
                section.Add(MakeInfoRow("Проклятие клана", clan.Curse));

            if (!string.IsNullOrEmpty(_c.BloodlineId))
            {
                var bl = _chr.FindBloodline(_c.BloodlineId);
                if (bl != null && !string.IsNullOrEmpty(bl.AdditionalCurse))
                    section.Add(MakeInfoRow("Проклятие бладлайна", bl.AdditionalCurse));
            }

            parent.Add(section);
        }

        private VisualElement MakeInfoRow(string label, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 3;

            var l = new Label(label + ":");
            l.style.fontSize = 13;
            l.style.color = UIRoot.TextMuted;
            l.style.minWidth = 130;
            l.style.marginRight = 6;
            row.Add(l);

            var v = new Label(value ?? "—");
            v.style.fontSize = 13;
            v.style.color = UIRoot.TextPrimary;
            v.style.whiteSpace = WhiteSpace.Normal;
            v.style.flexGrow = 1;
            row.Add(v);

            return row;
        }

        // ============================================================
        // Ресурсы
        // ============================================================

        private void BuildResources(VisualElement parent)
        {
            var section = UIWidgets.Section("Ресурсы");

            section.Add(MakeHealthBoxesRow());
            section.Add(MakeHealButtonRow());

            bool canDamage = !SessionContext.IsGMViewingCharacter
                || (SessionContext.IsGMViewingCharacter && _gmEditMode);
            if (canDamage)
                section.Add(MakeDamageButtonsRow());

            var div = new VisualElement();
            div.style.height = 1;
            div.style.backgroundColor = UIRoot.BorderSoft;
            div.style.marginTop = 6;
            div.style.marginBottom = 6;
            section.Add(div);

            section.Add(MakeBarRow("Сила воли", _c.WillpowerCurrent, _c.WillpowerMax,
                new Color(0.55f, 0.55f, 0.85f)));
            section.Add(MakeBarRow("Кровь", _c.BloodCurrent, _c.BloodMax,
                new Color(0.75f, 0.2f, 0.2f)));

            section.Add(MakeRestoreButtonsRow());

            section.Add(MakeInfoRow("Густота крови", _c.BloodPotency.ToString()));
            section.Add(MakeInfoRow("Стоимость лечения (кр/HP)",
                $"ударный {GetHealCost(DamageType.Bashing):0.##}, " +
                $"летальный {GetHealCost(DamageType.Lethal):0.##}, " +
                $"агрег. {GetHealCost(DamageType.Aggravated):0.##}"));

            parent.Add(section);
        }

        // ------------------------------------------------------------
        // Клетки здоровья — с сортировкой
        // ------------------------------------------------------------

        private VisualElement MakeHealthBoxesRow()
        {
            var wrapper = new VisualElement();
            wrapper.style.marginBottom = 6;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 4;
            wrapper.Add(header);

            var l = new Label("Здоровье");
            l.style.fontSize = 13;
            l.style.color = UIRoot.TextPrimary;
            l.style.minWidth = 130;
            l.style.marginRight = 8;
            header.Add(l);

            int damaged = _c.CountDamaged();
            int max = _c.HealthMax;

            var num = new Label($"{max - damaged} / {max}");
            num.style.fontSize = 13;
            num.style.color = UIRoot.TextPrimary;
            num.style.flexGrow = 1;
            num.style.unityTextAlign = TextAnchor.MiddleRight;
            header.Add(num);

            var boxes = new VisualElement();
            boxes.style.flexDirection = FlexDirection.Row;
            boxes.style.flexWrap = Wrap.Wrap;
            wrapper.Add(boxes);

            _c.EnsureHealthBoxes();

            var entries = new List<DamageType>();
            for (int i = 0; i < max && i < _c.HealthBoxes.Length; i++)
                entries.Add(_c.HealthBoxes[i]);

            entries.Sort((a, b) => DamageRank(a).CompareTo(DamageRank(b)));

            foreach (var type in entries)
            {
                var box = new VisualElement();
                box.style.width = 32;
                box.style.height = 32;
                box.style.marginRight = 4;
                box.style.marginBottom = 4;
                box.style.alignItems = Align.Center;
                box.style.justifyContent = Justify.Center;
                box.style.borderTopLeftRadius = 4;
                box.style.borderTopRightRadius = 4;
                box.style.borderBottomLeftRadius = 4;
                box.style.borderBottomRightRadius = 4;
                box.style.borderLeftWidth = 1;
                box.style.borderRightWidth = 1;
                box.style.borderTopWidth = 1;
                box.style.borderBottomWidth = 1;
                box.style.borderLeftColor = UIRoot.BorderSoft;
                box.style.borderRightColor = UIRoot.BorderSoft;
                box.style.borderTopColor = UIRoot.BorderSoft;
                box.style.borderBottomColor = UIRoot.BorderSoft;

                string symbol = "·";
                Color bg = UIRoot.BgInput;
                Color fg = UIRoot.TextMuted;

                switch (type)
                {
                    case DamageType.None:
                        symbol = "·";
                        bg = new Color(0.20f, 0.30f, 0.20f);
                        fg = new Color(0.55f, 0.85f, 0.55f);
                        break;
                    case DamageType.Aggravated:
                        symbol = "✱";
                        bg = new Color(0.35f, 0.05f, 0.05f);
                        fg = new Color(1f, 0.30f, 0.30f);
                        break;
                    case DamageType.Lethal:
                        symbol = "X";
                        bg = new Color(0.55f, 0.20f, 0.15f);
                        fg = new Color(0.95f, 0.55f, 0.45f);
                        break;
                    case DamageType.Bashing:
                        symbol = "/";
                        bg = new Color(0.45f, 0.40f, 0.15f);
                        fg = new Color(0.95f, 0.85f, 0.35f);
                        break;
                }

                box.style.backgroundColor = bg;

                var symLbl = new Label(symbol);
                symLbl.style.fontSize = 18;
                symLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                symLbl.style.color = fg;
                symLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                box.Add(symLbl);

                boxes.Add(box);
            }

            var legend = new VisualElement();
            legend.style.flexDirection = FlexDirection.Row;
            legend.style.flexWrap = Wrap.Wrap;
            legend.style.marginTop = 4;
            legend.style.marginLeft = 130;
            wrapper.Add(legend);

            legend.Add(MakeLegendItem("· чисто", new Color(0.55f, 0.85f, 0.55f)));
            legend.Add(MakeLegendItem("✱ агрегированный", new Color(1f, 0.30f, 0.30f)));
            legend.Add(MakeLegendItem("X летальный", new Color(0.95f, 0.55f, 0.45f)));
            legend.Add(MakeLegendItem("/ ударный", new Color(0.95f, 0.85f, 0.35f)));

            return wrapper;
        }

        private static int DamageRank(DamageType t)
        {
            switch (t)
            {
                case DamageType.None: return 0;
                case DamageType.Aggravated: return 1;
                case DamageType.Lethal: return 2;
                case DamageType.Bashing: return 3;
            }
            return 4;
        }

        private VisualElement MakeLegendItem(string text, Color color)
        {
            var l = new Label(text);
            l.style.fontSize = 11;
            l.style.color = color;
            l.style.marginRight = 10;
            return l;
        }

        // ------------------------------------------------------------
        // Кнопка лечения
        // ------------------------------------------------------------

        private VisualElement MakeHealButtonRow()
        {
            var wrapper = new VisualElement();
            wrapper.style.flexDirection = FlexDirection.Row;
            wrapper.style.flexWrap = Wrap.Wrap;
            wrapper.style.marginLeft = 130;
            wrapper.style.marginTop = 4;

            bool gm = SessionContext.IsGMViewingCharacter;
            bool free = gm && _gmEditMode;

            var type = HealthSystem.GetTopHealPriority(_c);
            if (type == DamageType.None)
            {
                var ok = new Label("— нет ран —");
                ok.style.fontSize = 11;
                ok.style.color = new Color(0.55f, 0.85f, 0.55f);
                ok.style.marginBottom = 4;
                wrapper.Add(ok);
                return wrapper;
            }

            float cost = GetHealCost(type);
            int have = HealthSystem.CountType(_c, type);

            int perClick = HealthSystem.LevelsPerClick(cost);
            perClick = Mathf.Min(perClick, have);

            int bloodCost = 0;
            int wpCost = 0;

            if (!free)
            {
                while (perClick > 0)
                {
                    int bc = HealthSystem.BloodCostForLevels(perClick, cost);
                    int wc = HealthSystem.WillpowerCostForLevels(type, perClick);
                    if (bc <= _c.BloodCurrent && wc <= _c.WillpowerCurrent) break;
                    perClick--;
                }
                if (perClick > 0)
                {
                    bloodCost = HealthSystem.BloodCostForLevels(perClick, cost);
                    wpCost = HealthSystem.WillpowerCostForLevels(type, perClick);
                }
            }

            string symbol = type == DamageType.Bashing ? "/" :
                            type == DamageType.Lethal ? "X" : "✱";

            var b = new Button(() => OnHealClicked(type, perClick, cost, free));
            b.style.height = 28;
            b.style.fontSize = 12;
            b.style.marginRight = 6;
            b.style.marginBottom = 4;
            b.style.paddingLeft = 10;
            b.style.paddingRight = 10;
            b.style.color = Color.white;
            b.style.borderTopLeftRadius = 4;
            b.style.borderTopRightRadius = 4;
            b.style.borderBottomLeftRadius = 4;
            b.style.borderBottomRightRadius = 4;
            b.style.backgroundColor = type == DamageType.Bashing
                ? new Color(0.45f, 0.40f, 0.15f)
                : (type == DamageType.Lethal
                    ? new Color(0.55f, 0.20f, 0.15f)
                    : new Color(0.35f, 0.05f, 0.05f));

            if (perClick <= 0)
            {
                b.text = $"Лечить {symbol} — недостаточно ресурсов";
                b.SetEnabled(false);
            }
            else
            {
                string label = $"Лечить {symbol} ({perClick} за клик, всего {have})";
                if (free)
                    label += " — чит-режим";
                else
                {
                    if (bloodCost > 0) label += $" — {bloodCost} кр";
                    if (wpCost > 0) label += $" + {wpCost} воля";
                }
                b.text = label;
            }

            wrapper.Add(b);
            return wrapper;
        }

        // ------------------------------------------------------------
        // Кнопки нанесения урона
        // ------------------------------------------------------------

        private VisualElement MakeDamageButtonsRow()
        {
            var wrapper = new VisualElement();
            wrapper.style.flexDirection = FlexDirection.Row;
            wrapper.style.flexWrap = Wrap.Wrap;
            wrapper.style.marginLeft = 130;
            wrapper.style.marginTop = 4;

            var lbl = new Label("Нанести урон:");
            lbl.style.fontSize = 11;
            lbl.style.color = UIRoot.TextMuted;
            lbl.style.marginRight = 6;
            lbl.style.alignSelf = Align.Center;
            wrapper.Add(lbl);

            var b1 = new Button(() => OnDamageClicked(DamageType.Bashing));
            b1.text = "/";
            StyleResourceButton(b1, new Color(0.45f, 0.40f, 0.15f));
            wrapper.Add(b1);

            var b2 = new Button(() => OnDamageClicked(DamageType.Lethal));
            b2.text = "X";
            StyleResourceButton(b2, new Color(0.55f, 0.20f, 0.15f));
            wrapper.Add(b2);

            var b3 = new Button(() => OnDamageClicked(DamageType.Aggravated));
            b3.text = "✱";
            StyleResourceButton(b3, new Color(0.35f, 0.05f, 0.05f));
            wrapper.Add(b3);

            return wrapper;
        }

        private void StyleResourceButton(Button b, Color bg)
        {
            b.style.height = 28;
            b.style.fontSize = 14;
            b.style.marginRight = 6;
            b.style.marginBottom = 4;
            b.style.paddingLeft = 12;
            b.style.paddingRight = 12;
            b.style.backgroundColor = bg;
            b.style.color = Color.white;
            b.style.borderTopLeftRadius = 4;
            b.style.borderTopRightRadius = 4;
            b.style.borderBottomLeftRadius = 4;
            b.style.borderBottomRightRadius = 4;
        }

        // ------------------------------------------------------------
        // Действия
        // ------------------------------------------------------------

        private void OnDamageClicked(DamageType type)
        {
            string actor = SessionContext.IsGMViewingCharacter ? "Мастер" : "Игрок";
            int beforeHp = _c.HealthCurrent;

            HealthSystem.ApplyDamage(_c, type, 1);

            int afterHp = _c.HealthCurrent;
            string typeName = DamageTypeName(type);

            LogAction($"{actor} нанёс урон ({typeName}): HP {beforeHp} → {afterHp}");

            JsonSaveSystem.SaveCharacter(_c);
            SaveAndSyncGM();
        }

        private void OnHealClicked(DamageType type, int perClick, float cost, bool free)
        {
            if (perClick <= 0)
            {
                Debug.LogWarning("[Heal] Нечего лечить или недостаточно ресурсов.");
                return;
            }

            int bloodCost = free ? 0 : HealthSystem.BloodCostForLevels(perClick, cost);
            int wpCost = free ? 0 : HealthSystem.WillpowerCostForLevels(type, perClick);

            string title = $"Лечение ({DamageTypeName(type)})";
            string msg;
            if (free)
                msg = $"Мастер (чит-режим): вылечить {perClick} уровень(ей) бесплатно?";
            else
            {
                msg = $"Вылечить {perClick} уровень(ей)?";
                if (bloodCost > 0) msg += $"\nСпишется: {bloodCost} крови";
                if (wpCost > 0) msg += $", {wpCost} силы воли";
            }

            ShowConfirmDialog(title, msg, () =>
            {
                int bloodBefore = _c.BloodCurrent;
                int wpBefore = _c.WillpowerCurrent;

                int healed = HealthSystem.HealLevels(_c, type, perClick, cost, free);

                if (healed > 0)
                {
                    string actor = SessionContext.IsGMViewingCharacter ? "Мастер" : "Игрок";
                    string costStr = free
                        ? "(бесплатно, чит-режим)"
                        : $"(кровь {bloodBefore} → {_c.BloodCurrent}" +
                          (wpCost > 0 ? $", воля {wpBefore} → {_c.WillpowerCurrent}" : "") + ")";
                    LogAction($"{actor} вылечил {healed} ур. ({DamageTypeName(type)}) {costStr}");

                    JsonSaveSystem.SaveCharacter(_c);
                    SaveAndSyncGM();
                }
            });
        }

        private string DamageTypeName(DamageType t)
        {
            switch (t)
            {
                case DamageType.Bashing: return "ударный";
                case DamageType.Lethal: return "летальный";
                case DamageType.Aggravated: return "агрегированный";
            }
            return "?";
        }

        // ------------------------------------------------------------
        // Восстановление воли/крови
        // ------------------------------------------------------------

        private VisualElement MakeRestoreButtonsRow()
        {
            var wrapper = new VisualElement();
            wrapper.style.flexDirection = FlexDirection.Row;
            wrapper.style.flexWrap = Wrap.Wrap;
            wrapper.style.marginLeft = 130;
            wrapper.style.marginTop = 4;

            bool gm = SessionContext.IsGMViewingCharacter;
            bool free = gm && _gmEditMode;

            if (_c.WillpowerCurrent < _c.WillpowerMax)
            {
                var b = new Button(OnRestoreWillpowerClicked);
                b.text = "Восстановить волю (+1)";
                StyleResourceButton(b, new Color(0.30f, 0.30f, 0.55f));
                wrapper.Add(b);
            }

            if (_c.BloodCurrent < _c.BloodMax)
            {
                var b = new Button(OnRestoreBloodClicked);
                b.text = "Восстановить кровь (+1)";
                StyleResourceButton(b, new Color(0.55f, 0.15f, 0.15f));
                wrapper.Add(b);
            }

            if (free && _c.BloodCurrent < _c.BloodMax)
            {
                var b = new Button(() =>
                {
                    int before = _c.BloodCurrent;
                    _c.BloodCurrent = _c.BloodMax;
                    LogAction($"Мастер (чит): кровь {before} → {_c.BloodCurrent} (полностью)");
                    JsonSaveSystem.SaveCharacter(_c);
                    SaveAndSyncGM();
                });
                b.text = $"Восстановить кровь → {_c.BloodMax}";
                StyleResourceButton(b, new Color(0.55f, 0.15f, 0.15f));
                wrapper.Add(b);
            }

            return wrapper;
        }

        private void OnRestoreWillpowerClicked()
        {
            if (_c.WillpowerCurrent >= _c.WillpowerMax) return;

            int before = _c.WillpowerCurrent;
            _c.WillpowerCurrent = Mathf.Min(_c.WillpowerMax, _c.WillpowerCurrent + 1);

            string actor = SessionContext.IsGMViewingCharacter ? "Мастер" : "Игрок";
            LogAction($"{actor} восстановил силу воли: {before} → {_c.WillpowerCurrent}");

            JsonSaveSystem.SaveCharacter(_c);
            SaveAndSyncGM();
        }

        private void OnRestoreBloodClicked()
        {
            if (_c.BloodCurrent >= _c.BloodMax) return;

            int before = _c.BloodCurrent;
            _c.BloodCurrent = Mathf.Min(_c.BloodMax, _c.BloodCurrent + 1);

            string actor = SessionContext.IsGMViewingCharacter ? "Мастер" : "Игрок";
            LogAction($"{actor} восстановил кровь: {before} → {_c.BloodCurrent}");

            JsonSaveSystem.SaveCharacter(_c);
            SaveAndSyncGM();
        }

        // ------------------------------------------------------------
        // Логирование
        // ------------------------------------------------------------

        private void LogAction(string message)
        {
            string playerName = SessionContext.PlayerName ?? "?";
            if (!SessionContext.IsGMViewingCharacter)
                Debug.Log($"[PlayerAction] {playerName} ({_c.Name}): {message}");
            else
                Debug.Log($"[GMAction] {playerName}: {message} над «{_c.Name}»");
        }

        // ============================================================
        // Мораль и аура
        // ============================================================

        private void BuildMoralityAndAura(VisualElement parent)
        {
            var section = UIWidgets.Section("Мораль и аура");

            var mor = _chr.FindMorality(_c.MoralityId);
            if (mor != null)
                section.Add(MakeInfoRow("Мораль", mor.Name));
            section.Add(MakeInfoRow("Рейтинг морали", _c.MoralityRating.ToString()));
            section.Add(MakeInfoRow("Аура", _c.Aura.ToString("+#;-#;0")));

            if (mor != null && mor.Sins != null && mor.Sins.Count > 0)
            {
                var shown = new List<MoralitySin>();
                foreach (var s in mor.Sins)
                    if (s.Level >= 1 && s.Level <= _c.MoralityRating)
                        shown.Add(s);
                shown.Sort((a, b) => a.Level.CompareTo(b.Level));

                if (shown.Count > 0)
                {
                    var header = new Label("Грехи (от 1 до текущего рейтинга):");
                    header.style.fontSize = 13;
                    header.style.unityFontStyleAndWeight = FontStyle.Bold;
                    header.style.color = UIRoot.TextMuted;
                    header.style.marginTop = 8;
                    header.style.marginBottom = 4;
                    section.Add(header);

                    foreach (var s in shown)
                    {
                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.marginBottom = 2;

                        var lvlLbl = new Label($"{s.Level}.");
                        lvlLbl.style.fontSize = 12;
                        lvlLbl.style.color = new Color(0.95f, 0.7f, 0.4f);
                        lvlLbl.style.minWidth = 28;
                        lvlLbl.style.marginRight = 6;
                        lvlLbl.style.unityTextAlign = TextAnchor.MiddleRight;
                        row.Add(lvlLbl);

                        var txtLbl = new Label(s.Text);
                        txtLbl.style.fontSize = 12;
                        txtLbl.style.color = UIRoot.TextPrimary;
                        txtLbl.style.whiteSpace = WhiteSpace.Normal;
                        txtLbl.style.flexGrow = 1;
                        row.Add(txtLbl);

                        section.Add(row);
                    }
                }
            }

            parent.Add(section);
        }

        // ============================================================
        // Атрибуты
        // ============================================================

        private void BuildAttributes(VisualElement parent)
        {
            var section = UIWidgets.Section("Атрибуты");
            if (_xpMode && !SessionContext.IsGMViewingCharacter)
                section.Add(UIRoot.MakeMuted(
                    "Режим траты опыта: нажмите «+», чтобы повысить атрибут.", 11));
            if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                section.Add(UIRoot.MakeMuted(
                    "Режим мастера: меняйте значения кнопками «+» / «−». Опыт не тратится.", 11));

            var rank = _chr.FindRank(_c.RankId);

            foreach (var group in _chr.AttributeGroups)
            {
                var attrs = _chr.Attributes.FindAll(a => a.GroupId == group.Id);
                if (attrs.Count == 0) continue;

                var header = new Label(group.Name);
                header.style.fontSize = 14;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = UIRoot.TextMuted;
                header.style.marginTop = 8;
                header.style.marginBottom = 4;
                section.Add(header);

                foreach (var attr in attrs)
                {
                    int baseVal = _c.Attributes.TryGetValue(attr.Id, out int b) ? b : 1;
                    int bonus = ComputeMeritBonus("attr." + attr.Id);
                    int effective = baseVal + bonus;
                    bool isFavored = attr.Id == _c.FavoredAttributeId;

                    if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                    {
                        int hardMax = rank != null ? GetAttributeHardCap(rank) : 10;
                        bool canMinus = baseVal > 0;
                        bool canPlus = rank != null && (baseVal + 1 + bonus) <= hardMax;

                        string idLocal = attr.Id;
                        string disp = attr.Name + (isFavored ? "  ★" : "");
                        section.Add(BuildGMEditRow(
                            disp, effective.ToString(),
                            () => GMChangeAttribute(idLocal, -1),
                            () => GMChangeAttribute(idLocal, +1),
                            canMinus, canPlus));
                    }
                    else
                    {
                        System.Action onUpgrade = null;
                        string upgradeLabel = null;
                        if (_xpMode && !SessionContext.IsGMViewingCharacter)
                        {
                            int cost = GetAttributeUpgradeCost(attr.Id);
                            bool canUpgrade = CanUpgradeAttribute(attr.Id, out _);
                            if (canUpgrade)
                                onUpgrade = () => TryUpgradeAttribute(attr.Id);
                            upgradeLabel = canUpgrade ? $"Повысить за {cost} оп." : "макс.";
                        }

                        section.Add(MakeStatRow(attr.Name, effective.ToString(),
                            bonus, isFavored, onUpgrade, upgradeLabel));
                    }
                }
            }

            parent.Add(section);
        }

        private void BuildSkills(VisualElement parent)
        {
            var section = UIWidgets.Section("Навыки");
            if (_xpMode && !SessionContext.IsGMViewingCharacter)
                section.Add(UIRoot.MakeMuted(
                    "Режим траты опыта: нажмите «+», чтобы повысить навык.", 11));
            if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                section.Add(UIRoot.MakeMuted(
                    "Режим мастера: меняйте значения кнопками «+» / «−». Опыт не тратится.", 11));

            var rank = _chr.FindRank(_c.RankId);

            foreach (var group in _chr.SkillGroups)
            {
                var skills = _chr.Skills.FindAll(s => s.GroupId == group.Id);
                if (skills.Count == 0) continue;

                var shownSkills = new List<SkillDef>();
                foreach (var s in skills)
                {
                    int baseVal = _c.Skills.TryGetValue(s.Id, out int b) ? b : 0;
                    int bonus = ComputeMeritBonus("skill." + s.Id);
                    bool hasSpecs = _c.Specializations.TryGetValue(s.Id, out var sp) && sp.Count > 0;
                    if (baseVal > 0 || bonus != 0 || hasSpecs ||
                        (_xpMode && !SessionContext.IsGMViewingCharacter) ||
                        (SessionContext.IsGMViewingCharacter && _gmEditMode))
                        shownSkills.Add(s);
                }

                if (shownSkills.Count == 0) continue;

                var header = new Label(group.Name);
                header.style.fontSize = 14;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = UIRoot.TextMuted;
                header.style.marginTop = 8;
                header.style.marginBottom = 4;
                section.Add(header);

                foreach (var skill in shownSkills)
                {
                    int baseVal = _c.Skills.TryGetValue(skill.Id, out int b) ? b : 0;
                    int bonus = ComputeMeritBonus("skill." + skill.Id);
                    int effective = baseVal + bonus;
                    string skillName = _chr.GetSkillName(skill);

                    if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                    {
                        int hardMax = rank != null ? GetSkillHardCap(rank) : 10;
                        bool canMinus = baseVal > 0;
                        bool canPlus = rank != null && (baseVal + 1 + bonus) <= hardMax;

                        string idLocal = skill.Id;
                        section.Add(BuildGMEditRow(
                            skillName, effective.ToString(),
                            () => GMChangeSkill(idLocal, -1),
                            () => GMChangeSkill(idLocal, +1),
                            canMinus, canPlus));
                    }
                    else
                    {
                        System.Action onUpgrade = null;
                        string upgradeLabel = null;
                        if (_xpMode && !SessionContext.IsGMViewingCharacter)
                        {
                            int cost = GetSkillUpgradeCost(skill.Id);
                            bool canUpgrade = CanUpgradeSkill(skill.Id, out _);
                            if (canUpgrade)
                                onUpgrade = () => TryUpgradeSkill(skill.Id);
                            upgradeLabel = canUpgrade ? $"Повысить за {cost} оп." : "макс.";
                        }

                        section.Add(MakeStatRow(skillName, effective.ToString(), bonus,
                            false, onUpgrade, upgradeLabel));
                    }

                    if (_c.Specializations.TryGetValue(skill.Id, out var specs) && specs.Count > 0)
                    {
                        var specLbl = new Label("Специализации: " + string.Join(", ", specs));
                        specLbl.style.fontSize = 12;
                        specLbl.style.color = UIRoot.AccentBlue;
                        specLbl.style.marginLeft = 20;
                        specLbl.style.marginBottom = 4;
                        specLbl.style.whiteSpace = WhiteSpace.Normal;
                        section.Add(specLbl);
                    }
                }
            }

            parent.Add(section);
        }

        private void BuildDisciplines(VisualElement parent)
        {
            var section = UIWidgets.Section("Дисциплины");
            if (_xpMode && !SessionContext.IsGMViewingCharacter)
                section.Add(UIRoot.MakeMuted(
                    "Режим траты опыта: нажмите «+», чтобы повысить дисциплину.", 11));
            if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                section.Add(UIRoot.MakeMuted(
                    "Режим мастера: 0-дисциплины тоже видны — нажмите «+», чтобы добавить.", 11));

            var rank = _chr.FindRank(_c.RankId);

            var all = new List<(Discipline disc, int playerLvl, int meritLvl)>();
            foreach (var disc in _chr.Disciplines)
            {
                int playerLvl = _c.Disciplines.TryGetValue(disc.Id, out int v) ? v : 0;
                int meritLvl = ComputeMeritDisciplineLevel(disc.Id);
                if (playerLvl + meritLvl > 0 ||
                    (SessionContext.IsGMViewingCharacter && _gmEditMode))
                    all.Add((disc, playerLvl, meritLvl));
            }

            if (all.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("(нет изученных дисциплин)"));
                parent.Add(section);
                return;
            }

            var mainList = all.FindAll(x => !x.disc.IsPath && !x.disc.IsAmalgam);
            var pathList = all.FindAll(x => x.disc.IsPath);
            var amalgamList = all.FindAll(x => x.disc.IsAmalgam);

            if (mainList.Count > 0)
            {
                var h = new Label("Основные");
                h.style.fontSize = 14;
                h.style.unityFontStyleAndWeight = FontStyle.Bold;
                h.style.color = UIRoot.TextMuted;
                h.style.marginTop = 6;
                h.style.marginBottom = 4;
                section.Add(h);

                foreach (var item in mainList)
                {
                    var disc = item.disc;
                    int eff = item.playerLvl + item.meritLvl;

                    if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                    {
                        int hardMax = rank != null ? GetBPMaxRating(rank) : 5;
                        bool canMinus = item.playerLvl > 0;
                        bool canPlus = rank != null &&
                            (item.playerLvl + 1 + item.meritLvl) <= hardMax;

                        string idLocal = disc.Id;
                        section.Add(BuildGMEditRow(
                            disc.Name, eff.ToString(),
                            () => GMChangeDiscipline(idLocal, -1),
                            () => GMChangeDiscipline(idLocal, +1),
                            canMinus, canPlus));
                    }
                    else
                    {
                        System.Action onUpgrade = null;
                        string upgradeLabel = null;
                        if (_xpMode && !SessionContext.IsGMViewingCharacter)
                        {
                            int cost = GetDisciplineUpgradeCost(disc.Id);
                            bool canUpgrade = CanUpgradeDiscipline(disc.Id, out _);
                            if (canUpgrade)
                                onUpgrade = () => TryUpgradeDiscipline(disc.Id);
                            upgradeLabel = canUpgrade ? $"Повысить за {cost} оп." : "макс.";
                        }

                        section.Add(MakeStatRow(disc.Name, eff.ToString(),
                            item.meritLvl, false, onUpgrade, upgradeLabel));
                    }
                }
            }

            if (pathList.Count > 0)
            {
                var h = new Label("Пути");
                h.style.fontSize = 14;
                h.style.unityFontStyleAndWeight = FontStyle.Bold;
                h.style.color = UIRoot.TextMuted;
                h.style.marginTop = 8;
                h.style.marginBottom = 4;
                section.Add(h);

                foreach (var item in pathList)
                {
                    var disc = item.disc;
                    var parentDisc = _chr.FindDiscipline(disc.ParentDisciplineId);
                    string parentName = parentDisc != null ? parentDisc.Name : "?";
                    int eff = item.playerLvl + item.meritLvl;

                    if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                    {
                        int hardMax = rank != null ? GetBPMaxRating(rank) : 5;
                        bool canMinus = item.playerLvl > 0;
                        bool canPlus = rank != null &&
                            (item.playerLvl + 1 + item.meritLvl) <= hardMax;

                        string idLocal = disc.Id;
                        section.Add(BuildGMEditRow(
                            disc.Name + $"  (путь «{parentName}»)", eff.ToString(),
                            () => GMChangeDiscipline(idLocal, -1),
                            () => GMChangeDiscipline(idLocal, +1),
                            canMinus, canPlus));
                    }
                    else
                    {
                        System.Action onUpgrade = null;
                        string upgradeLabel = null;
                        if (_xpMode && !SessionContext.IsGMViewingCharacter)
                        {
                            int cost = GetDisciplineUpgradeCost(disc.Id);
                            bool canUpgrade = CanUpgradeDiscipline(disc.Id, out _);
                            if (canUpgrade)
                                onUpgrade = () => TryUpgradeDiscipline(disc.Id);
                            upgradeLabel = canUpgrade ? $"Повысить за {cost} оп." : "макс.";
                        }

                        section.Add(MakeStatRow(disc.Name + $"  (путь «{parentName}»)",
                            eff.ToString(), item.meritLvl, false, onUpgrade, upgradeLabel));
                    }
                }
            }

            if (amalgamList.Count > 0)
            {
                var h = new Label("Амальгамы");
                h.style.fontSize = 14;
                h.style.unityFontStyleAndWeight = FontStyle.Bold;
                h.style.color = UIRoot.TextMuted;
                h.style.marginTop = 8;
                h.style.marginBottom = 4;
                section.Add(h);

                foreach (var item in amalgamList)
                {
                    if (SessionContext.IsGMViewingCharacter && _gmEditMode)
                    {
                        string idLocal = item.disc.Id;
                        bool learned = item.playerLvl + item.meritLvl > 0;
                        section.Add(BuildGMEditRow(
                            item.disc.Name, learned ? "✓" : "—",
                            () => GMRemoveDisciplineFully(idLocal),
                            () => GMAddDisciplineAsAmalgam(idLocal),
                            learned, !learned));
                    }
                    else
                    {
                        section.Add(MakeStatRow(item.disc.Name, "✓",
                            item.meritLvl, false, null, null));
                    }
                }
            }

            parent.Add(section);
        }

        private void BuildMerits(VisualElement parent)
        {
            var section = UIWidgets.Section("Черты");
            bool gm = SessionContext.IsGMViewingCharacter && _gmEditMode;

            if (_c.MeritIds.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("(нет черт)"));
                if (gm)
                    section.Add(UIWidgets.MakeLongButton(
                        "＋  Добавить черту", ShowGMAddMeritDialog, UIRoot.AccentGreen));
                parent.Add(section);
                return;
            }

            var counts = new Dictionary<string, int>();
            var order = new List<string>();
            foreach (var id in _c.MeritIds)
            {
                if (!counts.ContainsKey(id)) { counts[id] = 0; order.Add(id); }
                counts[id]++;
            }

            int totalCost = 0;

            foreach (var meritId in order)
            {
                var merit = _chr.FindMerit(meritId);
                if (merit == null) continue;

                int count = counts[meritId];
                totalCost += merit.Cost * count;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var nameLbl = new Label(merit.Name + (count > 1 ? $" ×{count}" : ""));
                nameLbl.style.fontSize = 13;
                nameLbl.style.color = UIRoot.TextPrimary;
                nameLbl.style.flexGrow = 1;
                row.Add(nameLbl);

                string costStr = merit.Cost > 0 ? $"+{merit.Cost}" :
                                 (merit.Cost < 0 ? merit.Cost.ToString() : "0");
                if (count > 1) costStr += $" ×{count} = {merit.Cost * count:+#;-#;0}";
                var costLbl = new Label(costStr);
                costLbl.style.fontSize = 13;
                costLbl.style.color = merit.Cost > 0
                    ? new Color(0.95f, 0.7f, 0.4f)
                    : (merit.Cost < 0
                        ? new Color(0.55f, 0.85f, 0.55f)
                        : UIRoot.TextMuted);
                costLbl.style.minWidth = 80;
                costLbl.style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(costLbl);

                if (gm)
                {
                    string idLocal = meritId;
                    var xBtn = new Button(() => GMRemoveMerit(idLocal));
                    xBtn.text = "✕";
                    xBtn.style.width = 26;
                    xBtn.style.height = 22;
                    xBtn.style.fontSize = 11;
                    xBtn.style.backgroundColor = UIRoot.AccentRed;
                    xBtn.style.color = Color.white;
                    xBtn.style.marginLeft = 8;
                    xBtn.style.borderTopLeftRadius = 4;
                    xBtn.style.borderTopRightRadius = 4;
                    xBtn.style.borderBottomLeftRadius = 4;
                    xBtn.style.borderBottomRightRadius = 4;
                    row.Add(xBtn);
                }

                section.Add(row);
            }

            var totalRow = new VisualElement();
            totalRow.style.flexDirection = FlexDirection.Row;
            totalRow.style.marginTop = 6;
            totalRow.style.paddingTop = 4;
            totalRow.style.borderTopWidth = 1;
            totalRow.style.borderTopColor = UIRoot.BorderSoft;
            var totalLbl = new Label("Итого");
            totalLbl.style.flexGrow = 1;
            totalLbl.style.fontSize = 13;
            totalLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            totalLbl.style.color = UIRoot.TextPrimary;
            totalRow.Add(totalLbl);
            var totalVal = new Label(totalCost.ToString("+#;-#;0"));
            totalVal.style.fontSize = 13;
            totalVal.style.unityFontStyleAndWeight = FontStyle.Bold;
            totalVal.style.color = totalCost >= 0
                ? new Color(0.95f, 0.7f, 0.4f)
                : new Color(0.55f, 0.85f, 0.55f);
            totalVal.style.minWidth = 80;
            totalVal.style.unityTextAlign = TextAnchor.MiddleRight;
            totalRow.Add(totalVal);
            section.Add(totalRow);

            if (gm)
                section.Add(UIWidgets.MakeLongButton(
                    "＋  Добавить черту", ShowGMAddMeritDialog, UIRoot.AccentGreen));

            parent.Add(section);
        }

        private void BuildAddons(VisualElement parent)
        {
            var section = UIWidgets.Section("Дополнения");

            if (_c.AddonLevels == null || _c.AddonLevels.Count == 0)
            {
                section.Add(UIRoot.MakeMuted("(нет дополнений)"));
                parent.Add(section);
                return;
            }

            int total = 0;
            foreach (var kv in _c.AddonLevels)
            {
                if (kv.Value <= 0) continue;
                var addon = _chr.FindAddon(kv.Key);
                string name = addon != null ? addon.Name : kv.Key;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 3;

                var l = new Label(name);
                l.style.fontSize = 13;
                l.style.color = UIRoot.TextPrimary;
                l.style.flexGrow = 1;
                row.Add(l);

                var v = new Label(kv.Value.ToString());
                v.style.fontSize = 13;
                v.style.color = UIRoot.AccentBlue;
                v.style.minWidth = 30;
                v.style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(v);

                section.Add(row);
                total += kv.Value;
            }

            var totalRow = new VisualElement();
            totalRow.style.flexDirection = FlexDirection.Row;
            totalRow.style.marginTop = 6;
            totalRow.style.paddingTop = 4;
            totalRow.style.borderTopWidth = 1;
            totalRow.style.borderTopColor = UIRoot.BorderSoft;
            var tl = new Label("Итого");
            tl.style.flexGrow = 1;
            tl.style.fontSize = 13;
            tl.style.unityFontStyleAndWeight = FontStyle.Bold;
            tl.style.color = UIRoot.TextPrimary;
            totalRow.Add(tl);
            var tv = new Label(total.ToString());
            tv.style.fontSize = 13;
            tv.style.unityFontStyleAndWeight = FontStyle.Bold;
            tv.style.color = UIRoot.AccentBlue;
            tv.style.minWidth = 30;
            tv.style.unityTextAlign = TextAnchor.MiddleRight;
            totalRow.Add(tv);
            section.Add(totalRow);

            parent.Add(section);
        }

        // ============================================================
        // Вспомогательные методы рендера
        // ============================================================

        private VisualElement MakeBarRow(string label, int current, int max, Color fill)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 6;

            var l = new Label(label);
            l.style.fontSize = 13;
            l.style.color = UIRoot.TextPrimary;
            l.style.minWidth = 130;
            l.style.marginRight = 8;
            row.Add(l);

            var barBg = new VisualElement();
            barBg.style.flexGrow = 1;
            barBg.style.height = 18;
            barBg.style.backgroundColor = UIRoot.BgInput;
            barBg.style.borderTopLeftRadius = 4;
            barBg.style.borderTopRightRadius = 4;
            barBg.style.borderBottomLeftRadius = 4;
            barBg.style.borderBottomRightRadius = 4;
            row.Add(barBg);

            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            var barFill = new VisualElement();
            barFill.style.position = Position.Absolute;
            barFill.style.left = 0;
            barFill.style.top = 0;
            barFill.style.bottom = 0;
            barFill.style.width = Length.Percent(ratio * 100f);
            barFill.style.backgroundColor = fill;
            barFill.style.borderTopLeftRadius = 4;
            barFill.style.borderBottomLeftRadius = 4;
            barBg.Add(barFill);

            var num = new Label($"{current} / {max}");
            num.style.fontSize = 13;
            num.style.color = UIRoot.TextPrimary;
            num.style.minWidth = 70;
            num.style.marginLeft = 8;
            num.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(num);

            return row;
        }

        private VisualElement MakeStatRow(string label, string value, int bonus, bool isFavored,
            System.Action onUpgrade = null, string upgradeLabel = null)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 3;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;
            row.style.paddingLeft = 6;
            row.style.paddingRight = 6;

            var l = new Label(label + (isFavored ? "  ★" : ""));
            l.style.fontSize = 13;
            l.style.color = isFavored
                ? new Color(0.95f, 0.8f, 0.35f)
                : UIRoot.TextPrimary;
            l.style.flexGrow = 1;
            row.Add(l);

            if (bonus != 0)
            {
                var b = new Label($"({(bonus > 0 ? "+" : "")}{bonus})");
                b.style.fontSize = 12;
                b.style.color = bonus > 0
                    ? new Color(0.55f, 0.85f, 0.55f)
                    : new Color(0.95f, 0.55f, 0.55f);
                b.style.minWidth = 40;
                b.style.unityTextAlign = TextAnchor.MiddleRight;
                row.Add(b);
            }

            var v = new Label(value);
            v.style.fontSize = 14;
            v.style.unityFontStyleAndWeight = FontStyle.Bold;
            v.style.color = UIRoot.TextPrimary;
            v.style.minWidth = 40;
            v.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(v);

            if (onUpgrade != null && !string.IsNullOrEmpty(upgradeLabel))
            {
                var btn = new Button(onUpgrade);
                btn.text = upgradeLabel;
                btn.style.height = 26;
                btn.style.fontSize = 11;
                btn.style.marginLeft = 8;
                btn.style.paddingLeft = 8;
                btn.style.paddingRight = 8;
                btn.style.backgroundColor = UIRoot.AccentGreen;
                btn.style.color = Color.white;
                btn.style.borderTopLeftRadius = 4;
                btn.style.borderTopRightRadius = 4;
                btn.style.borderBottomLeftRadius = 4;
                btn.style.borderBottomRightRadius = 4;
                row.Add(btn);
            }
            else if (onUpgrade == null && !string.IsNullOrEmpty(upgradeLabel))
            {
                var blocked = new Label(upgradeLabel);
                blocked.style.fontSize = 11;
                blocked.style.color = UIRoot.TextMuted;
                blocked.style.marginLeft = 8;
                row.Add(blocked);
            }

            return row;
        }

        private VisualElement BuildGMEditRow(string label, string value,
            System.Action onMinus, System.Action onPlus,
            bool canMinus, bool canPlus)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 3;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;
            row.style.paddingLeft = 6;
            row.style.paddingRight = 6;

            var l = new Label(label);
            l.style.fontSize = 13;
            l.style.color = UIRoot.TextPrimary;
            l.style.flexGrow = 1;
            row.Add(l);

            var minusBtn = new Button(onMinus);
            minusBtn.text = "−";
            minusBtn.style.width = 32;
            minusBtn.style.height = 26;
            minusBtn.style.fontSize = 15;
            minusBtn.style.backgroundColor = UIRoot.AccentRed;
            minusBtn.style.color = Color.white;
            minusBtn.style.marginRight = 6;
            minusBtn.style.borderTopLeftRadius = 4;
            minusBtn.style.borderTopRightRadius = 4;
            minusBtn.style.borderBottomLeftRadius = 4;
            minusBtn.style.borderBottomRightRadius = 4;
            minusBtn.SetEnabled(canMinus);
            row.Add(minusBtn);

            var valLbl = new Label(value);
            valLbl.style.fontSize = 15;
            valLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            valLbl.style.color = UIRoot.TextPrimary;
            valLbl.style.minWidth = 40;
            valLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            row.Add(valLbl);

            var plusBtn = new Button(onPlus);
            plusBtn.text = "+";
            plusBtn.style.width = 32;
            plusBtn.style.height = 26;
            plusBtn.style.fontSize = 15;
            plusBtn.style.backgroundColor = UIRoot.AccentGreen;
            plusBtn.style.color = Color.white;
            plusBtn.style.marginLeft = 6;
            plusBtn.style.borderTopLeftRadius = 4;
            plusBtn.style.borderTopRightRadius = 4;
            plusBtn.style.borderBottomLeftRadius = 4;
            plusBtn.style.borderBottomRightRadius = 4;
            plusBtn.SetEnabled(canPlus);
            row.Add(plusBtn);

            return row;
        }

        private int ComputeMeritBonus(string target)
        {
            if (_c.MeritIds == null || string.IsNullOrEmpty(target)) return 0;
            int sum = 0;
            foreach (var meritId in _c.MeritIds)
            {
                var merit = _chr.FindMerit(meritId);
                if (merit?.Modifiers == null) continue;
                foreach (var mod in merit.Modifiers)
                {
                    if (mod.Target == target) sum += mod.Value;
                }
            }
            return sum;
        }

        private int ComputeMeritDisciplineLevel(string discId)
        {
            if (string.IsNullOrEmpty(discId)) return 0;
            int sum = 0;
            if (_c.MeritIds != null)
            {
                foreach (var meritId in _c.MeritIds)
                {
                    var merit = _chr.FindMerit(meritId);
                    if (merit?.Modifiers == null) continue;
                    foreach (var mod in merit.Modifiers)
                    {
                        if (mod.Target == "disc." + discId && mod.Value > 0)
                            sum += mod.Value;
                    }
                }
            }
            return sum;
        }
    }
}