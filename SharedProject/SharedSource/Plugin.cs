// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using Barotrauma;

using System.Runtime.CompilerServices;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Barotrauma.LuaCs;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace MoreModTags
{
    public partial class Plugin : IAssemblyPlugin
    {
        public void Initialize()
        {
#if CLIENT
            InitClient();
#endif
        }

        public void OnLoadCompleted()
        {
#if DEBUG
            LuaCsLogger.LogMessage(TextSOS.Get("tagger.shared.loaded", "[Tagger] Loaded Successfully.").Value);
            LuaCsLogger.LogMessage(TextSOS.Get("tagger.shared.debugmode", "[Tagger] Debug Mode is enabled.").Value);
#endif
        }

        public void PreInitPatching()
        {
            LuaCsLogger.LogMessage(TextSOS.Get("tagger.shared.preinit", "[Tagger] Pre-Initialization phase started.").Value);
        }

        public void Dispose()
        {
#if CLIENT
            DisposeClient();
#endif
#if DEBUG
            LuaCsLogger.LogMessage(TextSOS.Get("tagger.shared.unloaded", "[Tagger] Mod Unloaded.").Value);
#endif
            GC.SuppressFinalize(this);
        }
    }

    public static class TextSOS
    {
        public static LocalizedString Get(string key, string fallback = "")
        {
            var text = TextManager.Get(key);

            if (!string.IsNullOrEmpty(fallback))
            {
#if DEBUG
                return text.Fallback("[NT]" + fallback); // NT=NOT-TRANSLATED
#else
                return text.Fallback(fallback);
#endif
            }
            return text;
        }
    }
}
