using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;
using VTR.Net;

namespace VTR.UI
{
    public class GMScreen : IScreen
    {
        private VisualElement _root;
        private VisualElement _listContainer;
        private Label _pinLabel;
        private Label _countLabel;
        private Chronicle _chr;

        public void Build(VisualElement root)
        {
            _root = root;
            _chr = SessionContext.CurrentChronicle;

            if (_chr == null)
            {
                root.Add(UIRoot.MakeLabel("Хроника не загружена.", 16, Color.red));
                return;
            }

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.paddingTop = 16;
            container.style.paddingBottom = 16;
            container.style.paddingLeft = 24;
            container.style.paddingRight = 24;
            _root.Add(container);

            var topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.alignItems = Align.Center;
            topBar.style.marginBottom = 14;
            container.Add(topBar);

            var title = UIRoot.MakeHeader("Экран мастера", 22);
            title.style.flexGrow = 1;
            topBar.Add(title);

            _pinLabel = UIRoot.MakeLabel("", 14);
            _pinLabel.style.marginRight = 16;
            _pinLabel.style.color = new Color(0.95f, 0.8f, 0.35f);
            topBar.Add(_pinLabel);

            var backBtn = new Button(() =>
            {
                UIRoot.Instance.ShowHostGame(_chr);
            });
            backBtn.text = "←  В лобби";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 34);
            backBtn.style.width = 140;
            topBar.Add(backBtn);

            var section = UIWidgets.Section("Персонажи игроков");
            section.Add(UIRoot.MakeMuted(
                "Персонажи появляются здесь после того, как игроки создадут их и отправят. " +
                "Список обновляется автоматически."));

            _countLabel = UIRoot.MakeLabel("", 14);
            _countLabel.style.marginBottom = 8;
            _countLabel.style.marginTop = 6;
            section.Add(_countLabel);

            _listContainer = new VisualElement();
            section.Add(_listContainer);

            container.Add(section);

            RebuildList();

            _root.schedule.Execute(() =>
            {
                if (_root.panel != null)
                    RebuildList();
            }).Every(2000);
        }

        private void RebuildList()
        {
            if (_listContainer == null) return;
            _listContainer.Clear();

            var mgr = NetworkGameManager.Instance;
            if (mgr == null)
            {
                _listContainer.Add(UIRoot.MakeMuted("Сеть не запущена."));
                return;
            }

            if (_pinLabel != null)
            {
                if (!string.IsNullOrEmpty(NetworkSession.Pin))
                    _pinLabel.text = $"PIN: {NetworkSession.Pin}";
                else
                    _pinLabel.text = "";
            }

            int totalPlayers = mgr.PlayerCount.Value;
            int withChars = mgr.PlayerCharacters.Count;

            if (_countLabel != null)
                _countLabel.text = $"Подключено: {totalPlayers}, с персонажами: {withChars}";

            if (mgr.PlayerCharacters.Count == 0)
            {
                _listContainer.Add(UIRoot.MakeMuted(
                    "Пока ни один игрок не прислал персонажа."));
                return;
            }

            var ids = new List<ulong>(mgr.PlayerCharacters.Keys);
            ids.Sort();

            int idx = 1;
            foreach (var id in ids)
            {
                string json = mgr.PlayerCharacters[id];
                _listContainer.Add(BuildCharacterCard(id, idx, json));
                idx++;
            }
        }

        private VisualElement BuildCharacterCard(ulong clientId, int index, string json)
        {
            var card = new VisualElement();
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;
            card.style.marginBottom = 6;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.backgroundColor = new Color(0.16f, 0.16f, 0.20f);
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;

            Character ch = null;
            try
            {
                ch = JsonSaveSystem.Deserialize<Character>(json);
            }
            catch
            {
                var errLbl = UIRoot.MakeLabel(
                    $"Игрок {index} (clientId={clientId}): некорректный JSON",
                    13, new Color(1f, 0.55f, 0.55f));
                errLbl.style.flexGrow = 1;
                card.Add(errLbl);
                return card;
            }

            var info = new VisualElement();
            info.style.flexGrow = 1;

            var nameLbl = new Label(ch.Name);
            nameLbl.style.fontSize = 16;
            nameLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLbl.style.color = UIRoot.TextPrimary;
            info.Add(nameLbl);

            string clanName = "?";
            string rankName = "?";
            if (_chr != null)
            {
                var clan = _chr.FindClan(ch.ClanId);
                var rank = _chr.FindRank(ch.RankId);
                if (clan != null) clanName = clan.Name;
                if (rank != null) rankName = rank.Name;
            }

            string subtitle = $"{clanName} • {rankName} • Игрок {index} (clientId={clientId})";
            var subLbl = UIRoot.MakeMuted(subtitle, 12);
            info.Add(subLbl);

            card.Add(info);

            ulong capturedId = clientId;
            Character capturedCh = ch;
            var openBtn = new Button(() =>
            {
                SessionContext.GMViewingOwnerId = capturedId;
                UIRoot.Instance.ShowGMScreenCharacterSheet(capturedCh);
            });
            openBtn.text = "Открыть";
            UIRoot.StyleButton(openBtn, UIRoot.AccentBlue, Color.white, 34);
            openBtn.style.width = 110;
            card.Add(openBtn);

            return card;
        }
    }
}