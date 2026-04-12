// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using Barotrauma;
using Barotrauma.IO;
using System.Xml.Linq;
using Directory = Barotrauma.IO.Directory;

namespace MoreModTags
{
    public record CustomTag(Identifier Name, string Description);

    public static class TaggerDataManager
    {
        public const string DataPath = "Data/tagger_info.xml";
        private static List<CustomTag>? _cachedTags;

        public static List<CustomTag> GetCustomTags()
        {
            if (_cachedTags == null)
            {
                RLogger.LogDebug("[Tagger] Cache is empty, loading from disk...");
                _cachedTags = LoadFromDisk();
            }
            return _cachedTags;
        }

        private static List<CustomTag> LoadFromDisk()
        {
            var tags = new List<CustomTag>();
            if (!Barotrauma.IO.File.Exists(DataPath))
            {
                RLogger.LogDebug("[Tagger] No custom tags file found at " + DataPath);
                return tags;
            }

            try
            {
                var doc = XDocument.Load(DataPath);
                foreach (var el in doc.Root?.Elements("Tag") ?? Enumerable.Empty<XElement>())
                {
                    var name = el.Attribute("name")?.Value ?? el.Value;
                    var desc = el.Attribute("description")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(name))
                        tags.Add(new CustomTag(name.ToIdentifier(), desc));
                }
                RLogger.LogDebug($"[Tagger] Successfully loaded {tags.Count} tags from disk.");
            }
            catch (Exception e)
            {
                RLogger.Error(TextSOS.Get("tagger.error.xmlload", "[Tagger] XML Load Error: [error]").Value.Replace("[error]", e.Message));
            }
            return tags;
        }

        public static void SaveCustomTag(CustomTag newTag)
        {
            RLogger.LogDebug($"[Tagger] Saving/Updating tag: {newTag.Name}");
            var tags = GetCustomTags();
            tags.RemoveAll(t => t.Name == newTag.Name);
            tags.Add(newTag);
            SaveToDisk(tags);
        }

        public static void RemoveCustomTag(Identifier tagToRemove)
        {
            RLogger.LogDebug($"[Tagger] Removing tag: {tagToRemove}");
            var tags = GetCustomTags();
            if (tags.RemoveAll(t => t.Name == tagToRemove) > 0)
                SaveToDisk(tags);
        }

        private static void SaveToDisk(List<CustomTag> tags)
        {
            try
            {
                if (!Directory.Exists("Data"))
                {
                    RLogger.LogDebug("[Tagger] Data directory does not exist, creating...");
                    Directory.CreateDirectory("Data");
                }

                var doc = new XDocument(
                    new XElement("TaggerData",
                        tags.Select(t => new XElement("Tag",
                            new XAttribute("name", t.Name.Value),
                            new XAttribute("description", t.Description)
                        ))
                    )
                );
                doc.SaveSafe(DataPath);
                _cachedTags = tags;
                RLogger.LogDebug("[Tagger] Custom tags successfully saved to disk.");
            }
            catch (Exception e)
            {
                RLogger.Error(TextSOS.Get("tagger.error.xmlsave", "[Tagger] XML Save Error: [error]").Value.Replace("[error]", e.Message));
            }
        }
    }
}
