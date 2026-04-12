// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using System.Reflection;
using HarmonyLib;

namespace MoreModTags
{
    public partial class Plugin
    {
        public Harmony? HarmonyInstance { get; private set; }

        public void InitClient()
        {
            if (HarmonyInstance == null)
            {
                HarmonyInstance = new Harmony("com.tagger");

                try
                {
                    HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                    LuaCsLogger.LogMessage("[Tagger] Patches applied.");
                }
                catch (Exception e)
                {
                    LuaCsLogger.LogError($"[Tagger] Critical error applying patches: {e.Message}");
                }
            }

        }

        public static void DisposeClient()
        {
            //HarmonyInstance?.UnpatchSelf();
            //HarmonyInstance = null;
        }
    }
}