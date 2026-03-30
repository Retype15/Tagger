// Copyright (c) 2026 Retype15
// This file is licensed under the GNU GPLv3.
// See the LICENSE file in the project root for details.

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using System.Reflection;
using Barotrauma;
using HarmonyLib;

namespace Tagger
{
    public partial class Plugin : IAssemblyPlugin
    {
        private static Harmony? HarmonyInstance;

        public static void InitClient()
        {
            if (HarmonyInstance == null && !Harmony.HasAnyPatches("com.tagger"))
            {
                HarmonyInstance = new Harmony("com.tagger");

                try
                {
                    HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                    RLogger.Log(TextSOS.Get("tagger.client.loaded", "[Tagger] Patches applied.").Value);
                }
                catch (Exception e)
                {
                    RLogger.Error(TextSOS.Get("tagger.error.critical", "[Tagger] Critical error applying patches: [error]").Value.Replace("[error]", e.Message));
                }
            }
            else RLogger.Log(TextSOS.Get("tagger.client.patched", "[Tagger] Already patched.").Value);

        }

        public static void DisposeClient()
        {
            //HarmonyInstance?.UnpatchSelf();
            //HarmonyInstance = null;
        }
    }
}