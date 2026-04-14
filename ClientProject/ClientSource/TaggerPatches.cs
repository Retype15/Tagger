// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Barotrauma;
using Barotrauma.Steam;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace MoreModTags
{
    [HarmonyPatch(typeof(MutableWorkshopMenu))]
    public static partial class MTEPatches
    {
        // MARK: Steam Hidden Tags
        public static readonly ImmutableArray<Identifier> SteamHiddenTags = [.. new[]
        {
            "outpost",
            "beacon station",
            "wreck",
            "ruin",
            "weapons",
            "equipment",
            "medical",
            "gameplay mechanics",
            "qol", "client-side",
            "server-side",
            "outdated",
            "game mode",
            "library"
        }.ToIdentifiers()];

        // MARK: Preset Tags
        public static readonly ImmutableArray<Identifier> ImmutableCustomTags = [.. new[]
        {
            "modder tool",
            "misc"
        }.ToIdentifiers()];

        // MARK: Harmoy patches

        [HarmonyPatch("CreateTagsList")]
        [HarmonyPostfix]
        public static void Postfix_CreateTagsList(GUIListBox __result, bool canBeFocused)
        {
            if (__result?.Content == null) return;

            __result.ScrollBarEnabled = true;
            __result.ScrollBarVisible = true;
            __result.HideChildrenOutsideFrame = true;
            __result.Content.ClampMouseRectToParent = true;

            var selectedTags = __result.Content.Children
                .Where(c => c is GUIButton { Selected: true } && c.UserData is Identifier)
                .Select(c => (Identifier)c.UserData).ToHashSet();

            __result.Content.ClearChildren();
            var processedTags = new HashSet<Identifier>();

            RLogger.LogDebug($"[MTE] Building tags list. Found {SteamManager.Workshop.Tags.Length} Steam tags.");

            foreach (var tag in SteamManager.Workshop.Tags)
            {
                var btn = TagBuilder.CreateButton(__result.Content, tag, TagBuilder.GetCategory(tag), canBeFocused);
                if (selectedTags.Contains(tag)) btn.Selected = true;
                processedTags.Add(tag);
            }

            RLogger.LogDebug($"[MTE] Injecting {SteamHiddenTags.Length} hidden and {ImmutableCustomTags.Length} preset tags.");
            InjectCategory(SteamHiddenTags, TagType.Hidden);
            InjectCategory(ImmutableCustomTags, TagType.Preset);

            var customTags = MTEDataManager.GetCustomTags();
            RLogger.LogDebug($"[MTE] Injecting {customTags.Count} custom user tags.");
            foreach (var customTag in customTags)
            {
                if (processedTags.Contains(customTag.Name)) continue;
                var btn = TagBuilder.CreateButton(__result.Content, customTag.Name, TagType.Custom, canBeFocused, customTag.Description);
                if (selectedTags.Contains(customTag.Name)) btn.Selected = true;
                processedTags.Add(customTag.Name);
            }

            if (canBeFocused) { InjectNewPlusButton(__result.Content, canBeFocused); }

            RLogger.LogDebug($"[MTE] Total buttons in list: {__result.Content.CountChildren}");

            __result.UpdateScrollBarSize();
            __result.Content.RectTransform.RecalculateChildren(true);

            void InjectCategory(IEnumerable<Identifier> list, TagType type)
            {
                foreach (var id in list)
                {
                    if (processedTags.Contains(id)) continue;
                    var btn = TagBuilder.CreateButton(__result.Content, id, type, canBeFocused);
                    if (selectedTags.Contains(id)) btn.Selected = true;
                    processedTags.Add(id);
                }
            }
        }

        public static void ShowTagContextMenu(GUIComponent parent, GUIButton btn, Identifier id, bool isCustom)
        {
            if (isCustom)
            {
                GUIContextMenu.CreateContextMenu(
                    new ContextMenuOption(TextSOS.Get("mte.button.edit", "Edit").Value, isEnabled: true, onSelected: () => ShowTagEditor(parent, btn, id)),
                    new ContextMenuOption(TextSOS.Get("mte.button.delete", "Delete").Value, isEnabled: true, onSelected: () => ShowDeleteConfirmation(parent, btn, id))
                );
            }
            else
            {
                GUIContextMenu.CreateContextMenu(
                    new ContextMenuOption(TextSOS.Get("mte.button.savetag", "Save Tag").Value, isEnabled: true, onSelected: () => ShowTagEditor(parent, btn, id, isConverting: true))
                );
            }
        }

        private static void ShowDeleteConfirmation(GUIComponent parent, GUIButton tagBtn, Identifier tag)
        {
            var msgBox = new GUIMessageBox(
                TextManager.Get("Delete"),
                TextManager.GetWithVariable("WorkshopItemDeleteVerification", "[itemname]", tagBtn.Text),
                [TextManager.Get("Yes"), TextManager.Get("No")]);

            msgBox.Buttons[0].OnClicked = (yesBtn, _) =>
            {
                MTEDataManager.RemoveCustomTag(tag);
                parent.RemoveChild(tagBtn);
                parent.RectTransform.RecalculateChildren(true);
                msgBox.Close();
                return true;
            };
            msgBox.Buttons[1].OnClicked = (_, _) => { msgBox.Close(); return true; };
        }

        private static void ShowTagEditor(GUIComponent parent, GUIButton? tagBtn, Identifier? oldTag = null, bool isConverting = false)
        {
            bool isEditing = oldTag != null && tagBtn != null && !isConverting;
            var header = isEditing ? TextSOS.Get("mte.header.edit", "Edit Tag") : (isConverting ? TextSOS.Get("mte.header.convert", "Add to Custom") : TextSOS.Get("mte.header.newtag", "New Custom Tag"));

            var msgBox = new GUIMessageBox(header.Value, string.Empty, buttons: [TextManager.Get("ok"), TextManager.Get("cancel")], relativeSize: new Vector2(0.3f, 0.5f));
            var mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.75f), msgBox.Content.RectTransform, Anchor.TopCenter)) { Stretch = true, RelativeSpacing = 0.02f };

            _ = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.05f), mainLayout.RectTransform), TextSOS.Get("mte.label.name", "TAG NAME").Value, font: GUIStyle.SubHeadingFont);
            var nameBox = new GUITextBox(new RectTransform(new Vector2(1.0f, 0.15f), mainLayout.RectTransform),
                text: (isEditing || isConverting) ? oldTag!.Value.Value : "")
            { MaxTextLength = 25 };

            _ = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.05f), mainLayout.RectTransform), TextSOS.Get("mte.label.description", "DESCRIPTION").Value, font: GUIStyle.SubHeadingFont);
            string currentDesc = isEditing ? (MTEDataManager.GetCustomTags().FirstOrDefault(t => t.Name == oldTag)?.Description ?? "") : "";
            var descBox = MTEMenu.ScrollableTextBox(mainLayout, 6.0f, currentDesc);

            msgBox.Buttons[0].OnClicked = (okBtn, _) =>
            {
                var newNameStr = nameBox.Text.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(newNameStr)) { nameBox.Flash(GUIStyle.Red); return false; }

                if (!CheckText().IsMatch(input: newNameStr))
                {
                    nameBox.Flash(GUIStyle.Red);
                    GUI.AddMessage(TextSOS.Get("mte.error.invalidtagname", "Invalid tag name. Only letters, numbers, spaces, and underscores are allowed.").Value, Color.Red, 10.0f);
                    return false;
                }

                var newName = newNameStr.ToIdentifier();
                var newDesc = descBox.Text.Trim();

                RLogger.LogDebug($"[MTE] Confirming Save/Update: {newName} (Converted: {isConverting})");
                if (isEditing) { MTEDataManager.RemoveCustomTag(oldTag!.Value); }
                MTEDataManager.SaveCustomTag(new CustomTag(newName, newDesc));

                if (isEditing || isConverting)
                {
                    if (tagBtn != null) parent.RemoveChild(tagBtn);
                }

                var nbtn = TagBuilder.CreateButton(parent, newName, TagType.Custom, canFocus: true, newDesc);
                nbtn.Selected = true;

                var plusBtn = parent.Children.FirstOrDefault(c => c.UserData is Identifier id && id == "mte_add_btn_placeholder");
                plusBtn?.RectTransform.SetAsLastChild();

                RLogger.LogDebug($"[MTE] Tag list UI updated.");
                parent.RectTransform.RecalculateChildren(true);
                parent.ForceLayoutRecalculation();
                msgBox.Close();
                return true;
            };
            msgBox.Buttons[1].OnClicked = (_, _) => { msgBox.Close(); return true; };
            nameBox.Select();
        }

        private static void InjectNewPlusButton(GUIComponent parent, bool canBeFocused)
        {
            var plusBtn = new GUIButton(new RectTransform(new Vector2(0.25f, 1.0f / 8.0f), parent.RectTransform), TextSOS.Get("mte.button.newplus", "New +").Value, style: "GUIButtonRound")
            {
                CanBeFocused = canBeFocused,
                UserData = "mte_add_btn_placeholder".ToIdentifier()
            };
            TagBuilder.ApplyStyle(plusBtn, TagType.NewPlus, TextSOS.Get("mte.tooltip.addnew", "Click to create a new custom user tag.").Value);
            plusBtn.RectTransform.NonScaledSize = plusBtn.Font.MeasureString(plusBtn.Text).ToPoint() + new Point(GUI.IntScale(20), GUI.IntScale(5));
            plusBtn.RectTransform.IsFixedSize = true;
            plusBtn.OnClicked = (_, _) => { ShowTagEditor(parent, null, null); return true; };
        }

        [GeneratedRegex(@"^[a-zA-Z0-9_ ]+$")]
        private static partial Regex CheckText();
    }
}