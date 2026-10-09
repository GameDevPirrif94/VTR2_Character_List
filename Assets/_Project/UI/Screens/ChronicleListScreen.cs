using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI
{
    /// <summary>
    /// Экран со списком сохранённых хроник мастера.
    /// </summary>
    public class ChronicleListScreen : IScreen
    {
        public void Build(VisualElement root)
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.paddingTop = 20;
            container.style.paddingBottom = 20;
            container.style.paddingLeft = 30;
            container.style.paddingRight = 30;
            root.Add(container);

            container.Add(UIRoot.MakeHeader($"Хроники мастера: {SessionContext.PlayerName}", 28));

            var newBtn = new Button(OnCreateNew);
            newBtn.text = "+ Создать новую хронику";
            UIRoot.StyleButton(newBtn, UIRoot.AccentGreen, Color.white, 44);
            container.Add(newBtn);

            var list = new ScrollView();
            list.style.flexGrow = 1;
            list.style.marginTop = 16;
            container.Add(list);

            var chronicleIds = LoadChronicleIds();
            if (chronicleIds.Count == 0)
            {
                list.Add(UIRoot.MakeMuted("Нет сохранённых хроник. Создайте первую."));
            }
            else
            {
                foreach (var id in chronicleIds)
                {
                    var chr = JsonSaveSystem.LoadChronicle(id);
                    if (chr == null) continue;
                    list.Add(MakeChronicleRow(chr));
                }
            }

            var backBtn = new Button(() => UIRoot.Instance.ShowMainMenu());
            backBtn.text = "←  Назад";
            UIRoot.StyleButton(backBtn, UIRoot.AccentNeutral, Color.white, 36);
            backBtn.style.marginTop = 12;
            container.Add(backBtn);
        }

        private VisualElement MakeChronicleRow(Chronicle chr)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.backgroundColor = UIRoot.BgPanel;
            row.style.marginBottom = 8;
            row.style.paddingTop = 8;
            row.style.paddingBottom = 8;
            row.style.paddingLeft = 12;
            row.style.paddingRight = 12;
            row.style.borderTopLeftRadius = 4;
            row.style.borderTopRightRadius = 4;
            row.style.borderBottomLeftRadius = 4;
            row.style.borderBottomRightRadius = 4;

            var info = new VisualElement();
            info.style.flexGrow = 1;
            info.Add(UIRoot.MakeLabel(chr.Name, 18));
            info.Add(UIRoot.MakeMuted(
                $"Время: {chr.SelectedTimeFrame} • Ранги: {chr.Ranks.Count} • Дисциплины: {chr.Disciplines.Count}",
                12));
            row.Add(info);

            var startBtn = new Button(() =>
            {
                Debug.Log($"[ChronicleList] Запускаем игру по хронике: {chr.Name}");
                UIRoot.Instance.ShowHostGame(chr);
            });
            startBtn.text = "▶ Начать игру";
            UIRoot.StyleButton(startBtn, UIRoot.AccentGreen, Color.white, 36);
            startBtn.style.width = 160;
            row.Add(startBtn);

            var openBtn = new Button(() =>
            {
                Debug.Log($"[ChronicleList] Открываем редактор: {chr.Name} ({chr.Id})");
                UIRoot.Instance.ShowChronicleEditor(chr);
            });
            openBtn.text = "Редактор";
            UIRoot.StyleButton(openBtn, UIRoot.AccentBlue, Color.white, 36);
            openBtn.style.width = 110;
            row.Add(openBtn);

            var delBtn = new Button(() =>
            {
                Debug.LogWarning($"[ChronicleList] TODO: подтверждение удаления {chr.Id}");
            });
            delBtn.text = "✕";
            UIRoot.StyleButton(delBtn, UIRoot.AccentRed, Color.white, 36);
            delBtn.style.width = 44;
            row.Add(delBtn);

            return row;
        }

        private void OnCreateNew()
        {
            var chr = DefaultsLoader.CreateNewChronicle("Новая хроника");
            JsonSaveSystem.SaveChronicle(chr);
            Debug.Log($"[ChronicleList] Создана хроника {chr.Id}, открываем редактор.");
            UIRoot.Instance.ShowChronicleEditor(chr);
        }

        private static List<string> LoadChronicleIds()
        {
            var result = new List<string>();
            JsonSaveSystem.EnsureFolders();
            if (!Directory.Exists(JsonSaveSystem.ChroniclesPath)) return result;
            foreach (var f in Directory.GetFiles(JsonSaveSystem.ChroniclesPath, "*.json"))
                result.Add(Path.GetFileNameWithoutExtension(f));
            return result;
        }
    }
}