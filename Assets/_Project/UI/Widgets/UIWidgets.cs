using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using DropdownField = UnityEngine.UIElements.DropdownField;
using IntegerField = UnityEngine.UIElements.IntegerField;
using Label = UnityEngine.UIElements.Label;
using ScrollView = UnityEngine.UIElements.ScrollView;
using TextElement = UnityEngine.UIElements.TextElement;
using TextField = UnityEngine.UIElements.TextField;
using Toggle = UnityEngine.UIElements.Toggle;
using VisualElement = UnityEngine.UIElements.VisualElement;

namespace VTR.UI
{
    public static class UIWidgets
    {
        // ============================================================
        // Секция (карточка) с заголовком
        // ============================================================

        public static VisualElement Section(string title = null)
        {
            var s = new VisualElement();
            s.style.flexDirection = FlexDirection.Column;
            s.style.marginBottom = 16;
            s.style.paddingTop = 12;
            s.style.paddingBottom = 12;
            s.style.paddingLeft = 14;
            s.style.paddingRight = 14;
            s.style.backgroundColor = UIRoot.BgPanel;
            s.style.borderTopLeftRadius = 6;
            s.style.borderTopRightRadius = 6;
            s.style.borderBottomLeftRadius = 6;
            s.style.borderBottomRightRadius = 6;

            if (!string.IsNullOrEmpty(title))
            {
                var h = new Label(title);
                h.style.fontSize = 17;
                h.style.unityFontStyleAndWeight = FontStyle.Bold;
                h.style.color = UIRoot.TextPrimary;
                h.style.marginBottom = 10;
                s.Add(h);
            }
            return s;
        }

        // ============================================================
        // TextField
        // ============================================================

        public static TextField MakeTextField(string label, string value, Action<string> onChange)
        {
            var f = new TextField(label) { value = value ?? "" };
            UIRoot.StyleTextField(f, label);
            f.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return f;
        }

        public static TextField MakeMultilineField(string label, string value, Action<string> onChange, int height = 90)
        {
            var f = new TextField(label) { value = value ?? "", multiline = true };
            UIRoot.StyleTextField(f, label);
            f.style.height = height;
            f.RegisterValueChangedCallback(e => onChange?.Invoke(e.newValue));
            return f;
        }

        // ============================================================
        // IntegerField
        // ============================================================

        public static IntegerField MakeIntField(string label, int value, Action<int> onChange,
            int min = int.MinValue, int max = int.MaxValue)
        {
            var f = new IntegerField(label) { value = value };
            StyleIntegerField(f, label);

            f.RegisterValueChangedCallback(e =>
            {
                int v = e.newValue;
                bool clamped = false;
                if (v < min) { v = min; clamped = true; }
                if (v > max) { v = max; clamped = true; }
                if (clamped) f.SetValueWithoutNotify(v);
                onChange?.Invoke(v);
            });

            return f;
        }

        private static void StyleIntegerField(IntegerField f, string label)
        {
            f.style.marginBottom = 8;
            f.style.marginTop = 4;
            f.style.marginLeft = 4;
            f.style.marginRight = 4;
            f.style.fontSize = 15;

            var labelEl = f.labelElement;
            if (labelEl != null)
            {
                labelEl.style.color = UIRoot.TextPrimary;
                labelEl.style.fontSize = 14;
                labelEl.style.unityFontStyleAndWeight = FontStyle.Bold;
                labelEl.style.minWidth = 240;
                labelEl.style.marginRight = 10;
                labelEl.style.unityTextAlign = TextAnchor.MiddleLeft;
            }

            var input = f.Q<VisualElement>(className: "unity-base-text-field__input")
                     ?? f.Q<VisualElement>(className: "unity-text-field__input")
                     ?? f.Q<VisualElement>(className: "unity-integer-field__input")
                     ?? f.Q<VisualElement>(className: "unity-base-field__input");

            if (input != null)
            {
                input.style.backgroundColor = UIRoot.BgInput;
                input.style.minHeight = 28;
                input.style.paddingLeft = 8;
                input.style.paddingRight = 8;
                input.style.borderTopLeftRadius = 4;
                input.style.borderTopRightRadius = 4;
                input.style.borderBottomLeftRadius = 4;
                input.style.borderBottomRightRadius = 4;
                input.style.borderLeftWidth = 1;
                input.style.borderRightWidth = 1;
                input.style.borderTopWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftColor = UIRoot.BorderSoft;
                input.style.borderRightColor = UIRoot.BorderSoft;
                input.style.borderTopColor = UIRoot.BorderSoft;
                input.style.borderBottomColor = UIRoot.BorderSoft;
            }

            f.Query<TextElement>().ForEach(t =>
            {
                if (labelEl != null && t == labelEl) return;
                t.style.color = UIRoot.TextPrimary;
                t.style.fontSize = 15;
            });

            f.RegisterCallback<FocusInEvent>(_ => { if (input != null) input.style.backgroundColor = UIRoot.BgInputFocus; });
            f.RegisterCallback<FocusOutEvent>(_ => { if (input != null) input.style.backgroundColor = UIRoot.BgInput; });
        }

        // ============================================================
        // DropdownField — тёмная тема
        // ============================================================

        public static void StyleDropdown(DropdownField d, float width = -1)
        {
            if (width > 0) d.style.width = width;
            d.style.height = 30;
            d.style.fontSize = 14;
            d.style.marginBottom = 6;
            d.style.marginTop = 4;
            d.style.paddingLeft = 0;
            d.style.paddingRight = 0;

            var input = d.Q<VisualElement>(className: "unity-base-popup-field__input")
                     ?? d.Q<VisualElement>(className: "unity-popup-field__input")
                     ?? d.Q<VisualElement>(className: "unity-base-field__input");

            if (input != null)
            {
                input.style.backgroundColor = UIRoot.BgInput;
                input.style.minHeight = 30;
                input.style.paddingLeft = 10;
                input.style.paddingRight = 32;
                input.style.paddingTop = 4;
                input.style.paddingBottom = 4;
                input.style.borderTopLeftRadius = 4;
                input.style.borderTopRightRadius = 4;
                input.style.borderBottomLeftRadius = 4;
                input.style.borderBottomRightRadius = 4;

                input.style.borderLeftWidth = 1;
                input.style.borderRightWidth = 1;
                input.style.borderTopWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftColor = UIRoot.BorderSoft;
                input.style.borderRightColor = UIRoot.BorderSoft;
                input.style.borderTopColor = UIRoot.BorderSoft;
                input.style.borderBottomColor = UIRoot.BorderSoft;
            }

            var valueLabel = d.Q<Label>(className: "unity-base-popup-field__label")
                          ?? d.Q<Label>(className: "unity-popup-field__label");
            if (valueLabel != null)
            {
                valueLabel.style.color = UIRoot.TextPrimary;
                valueLabel.style.fontSize = 14;
                valueLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            }

            d.Query<TextElement>().ForEach(t =>
            {
                t.style.color = UIRoot.TextPrimary;
                t.style.fontSize = 14;
            });

            var arrow = d.Q<VisualElement>(className: "unity-base-popup-field__arrow")
                     ?? d.Q<VisualElement>(className: "unity-popup-field__arrow");
            if (arrow != null)
            {
                arrow.style.unityBackgroundImageTintColor = UIRoot.TextPrimary;
                arrow.style.width = 18;
                arrow.style.height = 18;
                arrow.style.marginRight = 8;
                arrow.style.alignSelf = Align.Center;

                var arrowImg = arrow as UnityEngine.UIElements.Image;
                if (arrowImg != null) arrowImg.tintColor = UIRoot.TextPrimary;
            }
        }

        // ============================================================
        // Кнопка с длинным текстом (перенос разрешён)
        // ============================================================

        public static Button MakeLongButton(string text, Action onClick, Color bg)
        {
            var b = new Button(onClick);
            b.text = text;
            b.style.minHeight = 40;
            b.style.backgroundColor = bg;
            b.style.color = Color.white;
            b.style.fontSize = 14;
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.whiteSpace = WhiteSpace.Normal;
            b.style.marginTop = 4;
            b.style.marginBottom = 4;
            b.style.marginLeft = 4;
            b.style.marginRight = 4;
            b.style.paddingLeft = 12;
            b.style.paddingRight = 12;
            b.style.paddingTop = 6;
            b.style.paddingBottom = 6;
            b.style.borderTopLeftRadius = 4;
            b.style.borderTopRightRadius = 4;
            b.style.borderBottomLeftRadius = 4;
            b.style.borderBottomRightRadius = 4;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            return b;
        }

        // ============================================================
        // Checkbox — тёмная тема
        // ============================================================

        // ============================================================
        // Toggle (чекбокс) — тёмная тема
        // ============================================================

        public static void StyleToggle(Toggle t)
        {
            t.style.color = UIRoot.TextPrimary;
            t.style.marginLeft = 4;
            t.style.marginRight = 4;
            t.style.marginTop = 4;
            t.style.marginBottom = 4;
            t.style.fontSize = 14;

            t.Query<TextElement>().ForEach(x =>
            {
                x.style.color = UIRoot.TextPrimary;
                x.style.fontSize = 14;
            });

            var check = t.Q<VisualElement>(className: "unity-toggle__checkmark")
                     ?? t.Q<VisualElement>(className: "unity-checkbox__checkmark");
            if (check != null)
            {
                check.style.backgroundColor = UIRoot.BgInput;
                check.style.borderLeftWidth = 1;
                check.style.borderRightWidth = 1;
                check.style.borderTopWidth = 1;
                check.style.borderBottomWidth = 1;
                check.style.borderLeftColor = UIRoot.BorderSoft;
                check.style.borderRightColor = UIRoot.BorderSoft;
                check.style.borderTopColor = UIRoot.BorderSoft;
                check.style.borderBottomColor = UIRoot.BorderSoft;
            }
        }

        // ============================================================
        // Разделитель
        // ============================================================

        public static VisualElement Divider()
        {
            var d = new VisualElement();
            d.style.height = 1;
            d.style.backgroundColor = UIRoot.BorderSoft;
            d.style.marginTop = 8;
            d.style.marginBottom = 8;
            return d;
        }

        // ============================================================
        // Строка: label + поле справа
        // ============================================================

        public static VisualElement Row(string label, VisualElement field)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            var lbl = new Label(label);
            lbl.style.color = UIRoot.TextPrimary;
            lbl.style.fontSize = 14;
            lbl.style.minWidth = 240;
            lbl.style.marginRight = 10;
            row.Add(lbl);
            row.Add(field);
            return row;
        }


        // ============================================================
        // Кастомный пикер — надёжнее DropdownField (у него ломается попап
        // внутри вложенных ScrollView + flex-wrap).
        // ============================================================

        /// <summary>
        /// Блок «Label сверху + кнопка со значением снизу». По клику открывает
        /// модальное окно со списком опций.
        /// </summary>
        /// <summary>
        /// Блок «Label сверху + кнопка со значением снизу». По клику открывает
        /// модальное окно со списком опций. После выбора текст кнопки обновляется.
        /// </summary>
        /// <summary>
        /// Блок «Label сверху + кнопка со значением снизу». По клику открывает
        /// модальное окно со списком опций. После выбора текст кнопки обновляется.
        /// </summary>
        public static VisualElement MakePickerBlock(
            string label,
            List<string> options,
            int selectedIndex,
            Action<int> onSelected,
            float width = 200)
        {
            var block = new VisualElement();
            block.style.flexDirection = FlexDirection.Column;
            block.style.flexShrink = 0;
            block.style.alignItems = Align.FlexStart;
            block.style.marginRight = 12;
            block.style.marginBottom = 10;
            block.style.marginTop = 4;

            if (!string.IsNullOrEmpty(label))
            {
                var lbl = new Label(label);
                lbl.style.fontSize = 12;
                lbl.style.color = UIRoot.TextMuted;
                lbl.style.marginBottom = 4;
                lbl.style.height = 16;
                lbl.style.flexShrink = 0;
                block.Add(lbl);
            }

            var btn = new Button();
            btn.style.width = width;
            btn.style.height = 30;
            btn.style.minHeight = 30;
            btn.style.flexShrink = 0;
            btn.style.fontSize = 14;
            btn.style.color = UIRoot.TextPrimary;
            btn.style.backgroundColor = UIRoot.BgInput;
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;
            btn.style.paddingLeft = 10;
            btn.style.paddingRight = 10;
            btn.style.borderTopLeftRadius = 4;
            btn.style.borderTopRightRadius = 4;
            btn.style.borderBottomLeftRadius = 4;
            btn.style.borderBottomRightRadius = 4;
            btn.style.borderLeftWidth = 1;
            btn.style.borderRightWidth = 1;
            btn.style.borderTopWidth = 1;
            btn.style.borderBottomWidth = 1;
            btn.style.borderLeftColor = UIRoot.BorderSoft;
            btn.style.borderRightColor = UIRoot.BorderSoft;
            btn.style.borderTopColor = UIRoot.BorderSoft;
            btn.style.borderBottomColor = UIRoot.BorderSoft;

            int currentIndex = selectedIndex;

            void RefreshLabel()
            {
                string txt = (currentIndex >= 0 && currentIndex < options.Count)
                    ? options[currentIndex]
                    : "(нет)";
                btn.text = txt + "   ▾";
            }

            RefreshLabel();

            btn.clicked += () =>
            {
                ShowPickerDialog(label, options, currentIndex, newIndex =>
                {
                    currentIndex = newIndex;
                    RefreshLabel();
                    onSelected?.Invoke(newIndex);
                });
            };

            block.Add(btn);
            return block;
        }

        /// <summary>
        /// Модальное окно со списком опций. По центру экрана, поверх всего.
        /// </summary>
        /// <summary>
        /// Модальное окно со списком опций. По центру экрана, поверх всего.
        /// С полем поиска: ввод фильтрует список по подстроке (регистронезависимо).
        /// Enter выбирает первый отфильтрованный вариант, Esc закрывает.
        /// </summary>
        public static void ShowPickerDialog(
            string title,
            List<string> options,
            int selectedIndex,
            Action<int> onSelected)
        {
            var panelRoot = UIRoot.PanelRoot;
            if (panelRoot == null)
            {
                Debug.LogWarning("[PickerDialog] PanelRoot == null");
                return;
            }

            // --- Затемнение ---
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.55f);
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;
            overlay.focusable = true;

            // --- Окно ---
            var dialog = new VisualElement();
            dialog.style.width = 460;
            dialog.style.maxHeight = 640;
            dialog.style.backgroundColor = UIRoot.BgPanel;
            dialog.style.borderTopLeftRadius = 8;
            dialog.style.borderTopRightRadius = 8;
            dialog.style.borderBottomLeftRadius = 8;
            dialog.style.borderBottomRightRadius = 8;
            dialog.style.paddingTop = 14;
            dialog.style.paddingBottom = 14;
            dialog.style.paddingLeft = 14;
            dialog.style.paddingRight = 14;

            var titleLbl = new Label(string.IsNullOrEmpty(title) ? "Выберите значение" : title);
            titleLbl.style.fontSize = 16;
            titleLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLbl.style.color = UIRoot.TextPrimary;
            titleLbl.style.marginBottom = 8;
            dialog.Add(titleLbl);

            // --- Поле поиска ---
            var searchField = new TextField();
            searchField.value = "";
            UIRoot.StyleTextField(searchField, "");
            searchField.style.marginBottom = 6;
            searchField.style.marginTop = 0;
            searchField.style.marginLeft = 0;
            searchField.style.marginRight = 0;
            dialog.Add(searchField);

            // --- Список ---
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.minHeight = 220;
            scroll.style.maxHeight = 420;
            dialog.Add(scroll);

            // Список отфильтрованных индексов (индекс в оригинальном массиве options).
            var filteredIndices = new List<int>();

            void RebuildList(string query)
            {
                scroll.Clear();
                filteredIndices.Clear();

                string q = (query ?? "").Trim().ToLowerInvariant();

                for (int i = 0; i < options.Count; i++)
                {
                    if (!string.IsNullOrEmpty(q) &&
                        !options[i].ToLowerInvariant().Contains(q))
                        continue;

                    filteredIndices.Add(i);

                    int captured = i;
                    var item = new Button(() =>
                    {
                        onSelected?.Invoke(captured);
                        panelRoot.Remove(overlay);
                    });
                    item.text = options[i];
                    item.style.height = 32;
                    item.style.fontSize = 14;
                    item.style.color = UIRoot.TextPrimary;
                    item.style.unityTextAlign = TextAnchor.MiddleLeft;
                    item.style.paddingLeft = 12;
                    item.style.marginBottom = 3;
                    item.style.borderTopLeftRadius = 4;
                    item.style.borderTopRightRadius = 4;
                    item.style.borderBottomLeftRadius = 4;
                    item.style.borderBottomRightRadius = 4;
                    item.style.backgroundColor = (captured == selectedIndex)
                        ? UIRoot.AccentBlue
                        : UIRoot.AccentNeutral;
                    scroll.Add(item);
                }

                if (filteredIndices.Count == 0)
                {
                    var empty = UIRoot.MakeMuted("Ничего не найдено.");
                    empty.style.marginTop = 8;
                    empty.style.marginLeft = 4;
                    scroll.Add(empty);
                }
            }

            // Enter — выбрать первый найденный.
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    if (filteredIndices.Count > 0)
                    {
                        int chosen = filteredIndices[0];
                        onSelected?.Invoke(chosen);
                        panelRoot.Remove(overlay);
                        evt.StopPropagation();
                    }
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    panelRoot.Remove(overlay);
                    evt.StopPropagation();
                }
            });

            // Фильтрация при вводе.
            searchField.RegisterValueChangedCallback(e => RebuildList(e.newValue));

            // --- Отмена ---
            var cancelBtn = new Button(() => panelRoot.Remove(overlay));
            cancelBtn.text = "Отмена";
            UIRoot.StyleButton(cancelBtn, UIRoot.AccentRed, Color.white, 32);
            cancelBtn.style.marginTop = 10;
            dialog.Add(cancelBtn);

            overlay.Add(dialog);

            // Клик по затемнению = закрыть.
            overlay.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == overlay) panelRoot.Remove(overlay);
            });

            panelRoot.Add(overlay);

            // Первичное наполнение.
            RebuildList("");

            // Фокус на поле поиска — чтобы можно было сразу печатать.
            searchField.schedule.Execute(() =>
            {
                searchField.Focus();
            }).ExecuteLater(20);
        }

        // ============================================================
        // Мультивыбор: список выбранных с крестиками + кнопка добавить
        // ============================================================

        /// <summary>
        /// Блок мультивыбора. Показывает выбранные элементы как чипы с ✕,
        /// и кнопку «+ Добавить», которая открывает пикер с ещё не выбранными опциями.
        /// selectedIds — ссылка на список в модели (Add/Remove меняют его напрямую).
        /// onChanged — вызывается после добавления/удаления (клиент перерисовывает форму).
        /// </summary>
        public static VisualElement MakeMultiPickerBlock(
            string label,
            List<string> allOptionIds,
            List<string> allOptionNames,
            List<string> selectedIds,
            int maxCount,
            Action onChanged,
            float width = 400)
        {
            var block = new VisualElement();
            block.style.flexDirection = FlexDirection.Column;
            block.style.marginBottom = 10;
            block.style.marginTop = 6;

            // --- Заголовок + счётчик ---
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 4;

            var lbl = new Label(label);
            lbl.style.fontSize = 14;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.style.color = UIRoot.TextPrimary;
            lbl.style.marginRight = 10;
            header.Add(lbl);

            var counter = UIRoot.MakeMuted($"{selectedIds.Count} / {maxCount}", 12);
            header.Add(counter);
            block.Add(header);

            // --- Список выбранных ---
            var chipsRow = new VisualElement();
            chipsRow.style.flexDirection = FlexDirection.Row;
            chipsRow.style.flexWrap = Wrap.Wrap;
            chipsRow.style.minHeight = 34;
            chipsRow.style.paddingTop = 4;
            chipsRow.style.paddingBottom = 4;
            chipsRow.style.paddingLeft = 6;
            chipsRow.style.paddingRight = 6;
            chipsRow.style.backgroundColor = UIRoot.BgInput;
            chipsRow.style.borderTopLeftRadius = 4;
            chipsRow.style.borderTopRightRadius = 4;
            chipsRow.style.borderBottomLeftRadius = 4;
            chipsRow.style.borderBottomRightRadius = 4;
            chipsRow.style.borderLeftWidth = 1;
            chipsRow.style.borderRightWidth = 1;
            chipsRow.style.borderTopWidth = 1;
            chipsRow.style.borderBottomWidth = 1;
            chipsRow.style.borderLeftColor = UIRoot.BorderSoft;
            chipsRow.style.borderRightColor = UIRoot.BorderSoft;
            chipsRow.style.borderTopColor = UIRoot.BorderSoft;
            chipsRow.style.borderBottomColor = UIRoot.BorderSoft;
            block.Add(chipsRow);

            if (selectedIds.Count == 0)
            {
                var empty = UIRoot.MakeMuted("(ничего не выбрано)", 12);
                empty.style.marginTop = 4;
                empty.style.marginLeft = 4;
                chipsRow.Add(empty);
            }
            else
            {
                foreach (var id in selectedIds.ToList())
                {
                    string idLocal = id;
                    int nameIdx = allOptionIds.IndexOf(idLocal);
                    string name = nameIdx >= 0 ? allOptionNames[nameIdx] : idLocal;

                    var chip = new VisualElement();
                    chip.style.flexDirection = FlexDirection.Row;
                    chip.style.alignItems = Align.Center;
                    chip.style.marginRight = 6;
                    chip.style.marginTop = 2;
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

                    var nameLbl = new Label(name);
                    nameLbl.style.fontSize = 13;
                    nameLbl.style.color = Color.white;
                    nameLbl.style.marginRight = 6;
                    chip.Add(nameLbl);

                    var xBtn = new Button(() =>
                    {
                        selectedIds.Remove(idLocal);
                        onChanged?.Invoke();
                    });
                    xBtn.text = "✕";
                    xBtn.style.width = 22;
                    xBtn.style.height = 22;
                    xBtn.style.fontSize = 12;
                    xBtn.style.color = Color.white;
                    xBtn.style.backgroundColor = new Color(0.45f, 0.15f, 0.15f);
                    xBtn.style.marginLeft = 2;
                    xBtn.style.marginRight = 0;
                    xBtn.style.marginTop = 0;
                    xBtn.style.marginBottom = 0;
                    xBtn.style.paddingLeft = 0;
                    xBtn.style.paddingRight = 0;
                    xBtn.style.borderTopLeftRadius = 11;
                    xBtn.style.borderTopRightRadius = 11;
                    xBtn.style.borderBottomLeftRadius = 11;
                    xBtn.style.borderBottomRightRadius = 11;
                    xBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
                    chip.Add(xBtn);

                    chipsRow.Add(chip);
                }
            }

            // --- Кнопка «Добавить» ---
            var addBtn = new Button();
            addBtn.text = "＋  Добавить";
            addBtn.style.marginTop = 6;
            addBtn.style.height = 30;
            addBtn.style.fontSize = 13;
            addBtn.style.color = Color.white;
            addBtn.style.backgroundColor = UIRoot.AccentGreen;
            addBtn.style.borderTopLeftRadius = 4;
            addBtn.style.borderTopRightRadius = 4;
            addBtn.style.borderBottomLeftRadius = 4;
            addBtn.style.borderBottomRightRadius = 4;
            addBtn.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Соберём доступные опции
            var availableIds = new List<string>();
            var availableNames = new List<string>();
            for (int i = 0; i < allOptionIds.Count; i++)
            {
                if (!selectedIds.Contains(allOptionIds[i]))
                {
                    availableIds.Add(allOptionIds[i]);
                    availableNames.Add(allOptionNames[i]);
                }
            }

            bool canAdd = selectedIds.Count < maxCount && availableIds.Count > 0;
            addBtn.SetEnabled(canAdd);

            if (canAdd)
            {
                addBtn.clicked += () =>
                {
                    ShowPickerDialog(label, availableNames, -1, idx =>
                    {
                        if (idx >= 0 && idx < availableIds.Count)
                        {
                            selectedIds.Add(availableIds[idx]);
                            onChanged?.Invoke();
                        }
                    });
                };
            }

            block.Add(addBtn);
            return block;
        }
    }
}