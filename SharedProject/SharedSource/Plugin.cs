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
            RLogger.LogDebug("[Tagger] Loaded Successfully.");
            RLogger.LogDebug("[Tagger] Debug Mode is enabled.");
        }

        public void PreInitPatching()
        {
            RLogger.LogDebug("[Tagger] Pre-Initialization phase started.");
        }

        public void Dispose()
        {
#if CLIENT
            DisposeClient();
#endif
            RLogger.LogDebug("[Tagger] Mod Unloaded.");
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

    public static class RLogger
    {
        [Conditional("DEBUG")]
        public static void LogDebug(LocalizedString message, Color? serverColor = null, Color? clientColor = null) => LuaCsLogger.LogMessage(message.Value, serverColor, clientColor);

        [Conditional("RELEASE")]
        public static void LogRelease(LocalizedString message, Color? serverColor = null, Color? clientColor = null) => LuaCsLogger.LogMessage(message.Value, serverColor, clientColor);

        public static void Log(LocalizedString message, Color? serverColor = null, Color? clientColor = null) => LuaCsLogger.LogMessage(message.Value, serverColor, clientColor);

        public static void Error(string message) => LuaCsLogger.LogError(message);

        public static void Error(string message, LuaCsMessageOrigin origin) => LuaCsLogger.LogError(message, origin);

        [Conditional("DEBUG")]
        public static void DebugError(string message) => LuaCsLogger.LogError(message);

        [Conditional("DEBUG")]
        public static void DebugError(string message, LuaCsMessageOrigin origin) => LuaCsLogger.LogError(message, origin);
    }
}
