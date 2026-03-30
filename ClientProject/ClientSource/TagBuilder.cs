// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using Barotrauma;
using Microsoft.Xna.Framework;

namespace Tagger
{
    public enum TagType { Vanilla, Hidden, Preset, Custom, Unrecognized, NewPlus }

    public static class TagBuilder
    {
        private static readonly Dictionary<TagType, Color> CategoryColors = new()
        {
            { TagType.Vanilla,      new Color(121, 165, 245) /* blue */         },
            { TagType.Hidden,       new Color(141, 171, 211) /* blue-grey */    },
            { TagType.Preset,       new Color(77, 220, 163) /* green */         },
            { TagType.Custom,       new Color(209, 163, 88) /* orange */        },
            { TagType.Unrecognized, new Color(240, 87, 87) /* red */            },
            { TagType.NewPlus,      new Color(175, 110, 230) /* purple */       }
        };

        public static TagType GetCategory(Identifier id)
        {
            if (TaggerPatches.SteamHiddenTags.Contains(id)) return TagType.Hidden;
            if (TaggerPatches.ImmutableCustomTags.Contains(id)) return TagType.Preset;
            if (TaggerDataManager.GetCustomTags().Any(t => t.Name == id)) return TagType.Custom;
            if (Barotrauma.Steam.SteamManager.Workshop.Tags.Contains(id)) return TagType.Vanilla;
            return TagType.Unrecognized;
        }

        public static GUIButton CreateButton(GUIComponent parent, Identifier id, TagType category, bool canFocus, string? extraDesc = null)
        {
            string labelText = id.Value;
            if (category != TagType.Custom)
                labelText = TextSOS.Get($"workshop.tag.{id.Value.RemoveWhitespace()}", id.Value.CapitaliseFirstInvariant()).Value;

            var btn = new GUIButton(new RectTransform(new Vector2(0.25f, 1.0f / 8.0f), parent.RectTransform), labelText, style: "GUIButtonRound")
            {
                CanBeFocused = canFocus,
                UserData = id
            };

            ApplyStyle(btn, category, extraDesc);

            btn.RectTransform.NonScaledSize = btn.Font.MeasureString(btn.Text).ToPoint() + new Point(GUI.IntScale(18), GUI.IntScale(6));
            btn.RectTransform.IsFixedSize = true;

            btn.OnClicked = (b, _) => { b.Selected = !b.Selected; return true; };

            if (category == TagType.Custom || category == TagType.Unrecognized)
            {
                btn.OnSecondaryClicked = (_, _) =>
                {
                    TaggerPatches.ShowTagContextMenu(parent, btn, id, category == TagType.Custom);
                    return true;
                };
            }

            RLogger.LogDebug($"[Tagger] Created button for {id} ({category})");
            return btn;
        }

        public static void ApplyStyle(GUIButton btn, TagType category, string? extraDesc = null)
        {
            Color baseColor = CategoryColors[category];
            btn.Color = baseColor * 1.2f;
            btn.HoverColor = Color.Lerp(baseColor, Color.White, 0.30f);
            btn.SelectedColor = Color.Lerp(baseColor, Color.White, 0.70f);

            LocalizedString title = TextSOS.Get($"tagger.title.{category.ToString().ToLower()}", $"{category} Tag");
            string desc = string.IsNullOrEmpty(extraDesc)
                ? TextSOS.Get($"tagger.tag.{((Identifier)btn.UserData).Value.RemoveWhitespace()}.desc", "Categorization tag.").Value
                : extraDesc;

            btn.ToolTip = RichString.Rich($"{title.SetBold().SetColor(Color.Violet)}\n‖linebreak‖\n{desc}");
        }
    }
}