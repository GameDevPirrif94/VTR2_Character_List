using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI
{
    public class CharacterListScreen : IScreen
    {
        public void Build(VisualElement root)
        {
            var chronicle = SessionContext.CurrentChronicle;
            if (chronicle == null)
            {
                root.Add(UIRoot.MakeLabel("Хроника не загружена.", 16, Color.red));
                return;
            }

            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.paddingTop = 20;
            container.style.paddingBottom = 20;
            container.style.paddingLeft = 30;
            container.style.paddingRight = 30;
            root.Add(container);

            container.Add(UIRoot.MakeHeader("Мои персонажи", 26));
            container.Add(UIRoot.MakeMuted($"Хроника: {chronicle.Name}"));

            var createBtn = new Button(() => StartNewCharacter(chronicle));
            createBtn.text = "＋  Создать нового персонажа";
            UIRoot.StyleButton(createBtn, UIRoot.AccentGreen, Color.white, 44);
            container.Add(createBtn);

            var list = new ScrollView();
            list.style.flexGrow = 1;
            list.style.marginTop = 16;
            container.Add(list);

            var myChars = LoadMyCharacters(chronicle.Id);
            if (myChars.Count == 0)
            {
                list.Add(UIRoot.MakeMuted("У вас пока нет персонажей в этой хронике."));
            }
            else
            {
                foreach (var c in myChars)
                    list.Add(MakeCharacterRow(c));
            }

            var backBtn = new Button(() => UIRoot.Instance.ShowJoinGame());
            backBtn.text = "←  Назад";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 36);
            backBtn.style.marginTop = 12;
            container.Add(backBtn);
        }

        // ============================================================
        // Создание нового черновика
        // ============================================================

        private void StartNewCharacter(Chronicle chronicle)
        {
            var draft = new CharacterDraft();

            // Ранг — первый с ненулевым лимитом игроков.
            for (int i = 0; i < chronicle.Ranks.Count; i++)
            {
                if (chronicle.Ranks[i].MaxPlayers > 0)
                {
                    draft.RankId = chronicle.Ranks[i].Id;
                    break;
                }
            }

            if (chronicle.Clans.Count > 0)
                draft.ClanId = chronicle.Clans[0].Id;
            if (chronicle.Natures.Count > 0)
                draft.Nature = chronicle.Natures[0];
            if (chronicle.Masks.Count > 0)
                draft.Mask = chronicle.Masks[0];
            if (chronicle.Moralites.Count > 0)
                draft.MoralityId = chronicle.Moralites[0].Id;

            if (!string.IsNullOrEmpty(draft.ClanId))
            {
                var clan = chronicle.FindClan(draft.ClanId);
                if (clan != null && clan.FavoredAttributeIds.Count > 0)
                    draft.FavoredAttributeId = clan.FavoredAttributeIds[0];
            }

            SessionContext.CurrentDraft = draft;
            UIRoot.Instance.ShowCharacterWizard();
        }

        // ============================================================
        // Строка персонажа
        // ============================================================

        private VisualElement MakeCharacterRow(Character c)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.backgroundColor = UIRoot.BgPanel;
            row.style.marginBottom = 8;
            row.style.paddingTop = 10;
            row.style.paddingBottom = 10;
            row.style.paddingLeft = 14;
            row.style.paddingRight = 14;
            row.style.borderTopLeftRadius = 4;
            row.style.borderTopRightRadius = 4;
            row.style.borderBottomLeftRadius = 4;
            row.style.borderBottomRightRadius = 4;

            var info = new VisualElement();
            info.style.flexGrow = 1;
            info.Add(UIRoot.MakeLabel(c.Name, 18));

            var chronicle = SessionContext.CurrentChronicle;
            var clan = chronicle.FindClan(c.ClanId);
            var rank = chronicle.FindRank(c.RankId);
            string subtitle = $"{(clan != null ? clan.Name : "?")} • {(rank != null ? rank.Name : "?")}";
            info.Add(UIRoot.MakeMuted(subtitle, 12));
            row.Add(info);

            var openBtn = new Button(() =>
            {
                SessionContext.CurrentCharacter = c;
                UIRoot.Instance.ShowCharacterSheet();
            });
            openBtn.text = "Открыть";
            UIRoot.StyleButton(openBtn, UIRoot.AccentBlue, Color.white, 34);
            openBtn.style.width = 120;
            row.Add(openBtn);

            return row;
        }

        // ============================================================
        // Загрузка
        // ============================================================

        private List<Character> LoadMyCharacters(string chronicleId)
        {
            var result = new List<Character>();
            JsonSaveSystem.EnsureFolders();
            if (!Directory.Exists(JsonSaveSystem.CharactersPath)) return result;

            foreach (var f in Directory.GetFiles(JsonSaveSystem.CharactersPath, "*.json"))
            {
                try
                {
                    var c = JsonSaveSystem.LoadCharacter(Path.GetFileNameWithoutExtension(f));
                    if (c != null && c.ChronicleId == chronicleId)
                        result.Add(c);
                }
                catch { /* skip broken */ }
            }
            return result;
        }
    }
}