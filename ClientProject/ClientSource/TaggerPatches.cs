// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using System.Collections.Immutable;
using Barotrauma;
using Barotrauma.Steam;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Tagger
{
    [HarmonyPatch(typeof(MutableWorkshopMenu))]
    public static class TaggerPatches
    {
        public static readonly ImmutableArray<Identifier> CustomTags = [.. new[]
        {
            "submarine",
            "mission",
            "item",
            "item assembly",
            "monster",
            "language",
            "event set",
            "total conversion",
            "art",
            "environment",
        // Customs
            "outpost",
            "beacon station",
            "wreck",
            "ruin",
            "weapons",
            "equipment",
            "medical",
            "gameplay mechanics",
            "qol",
            "client-side",
            "server-side",
            "outdated",
            "game mode",
            "library",
            "modder tool",
            "misc"
            }.ToIdentifiers()];

        [HarmonyPatch("CreateTagsList")]
        [HarmonyPostfix]
        public static void Postfix_CreateTagsList(GUIListBox __result, bool canBeFocused)
        {
            if (__result == null || __result.Content == null) return;

            __result.ScrollBarEnabled = true;
            __result.ScrollBarVisible = true;
            __result.HideChildrenOutsideFrame = true;
            __result.Content.ClampMouseRectToParent = true;

            var existingTags = __result.Content.Children
                .Select(c =>
                {
                    if (c.UserData is Identifier id)
                    {
                        if (id == "serverside" || id == "server side") return "server-side".ToIdentifier();
                        if (id == "clientside" || id == "client side") return "client-side".ToIdentifier();
                        return id;
                    }
                    return Identifier.Empty;
                })
                .Where(id => id != Identifier.Empty)
                .ToHashSet();

            int injectedCount = 0;


            foreach (var tag in CustomTags)
            {
                if (existingTags.Contains(tag)) continue;

                var tagBtn = new GUIButton(
                    new RectTransform(new Vector2(0.25f, 1.0f / 8.0f), __result.Content.RectTransform, anchor: Anchor.TopLeft),
                    TextManager.Get($"workshop.contenttag.{tag.Value.RemoveWhitespace()}").Fallback(tag.Value.CapitaliseFirstInvariant()),
                    style: "GUIButtonRound")
                {
                    CanBeFocused = canBeFocused,
                    Selected = !canBeFocused,
                    UserData = tag
                };

                tagBtn.RectTransform.NonScaledSize = tagBtn.Font.MeasureString(tagBtn.Text).ToPoint() + new Point(GUI.IntScale(15), GUI.IntScale(5));
                tagBtn.RectTransform.IsFixedSize = true;
                tagBtn.ClampMouseRectToParent = false;

                injectedCount++;
            }

            if (injectedCount > 0)
            {
                __result.UpdateScrollBarSize();
                __result.BarScroll = 0.0f;

                __result.Content.RectTransform.RecalculateChildren(true);

                LuaCsLogger.LogMessage($"[Tagger] {injectedCount} tags added.");
            }
        }
    }
}