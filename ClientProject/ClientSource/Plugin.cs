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
        private static Harmony? HarmonyInstance;

        public static void InitClient()
        {
            if (HarmonyInstance == null && !Harmony.HasAnyPatches("com.Retype15.mte"))
            {
                HarmonyInstance = new Harmony("com.Retyp15.mte");

                try
                {
                    HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                    RLogger.Log(TextSOS.Get("mte.client.loaded", "[MTE] Patches applied.").Value);
                }
                catch (Exception e)
                {
                    RLogger.Error(TextSOS.Get("mte.error.critical", "[MTE] Critical error applying patches: [error]").Value.Replace("[error]", e.Message));
                }
            }
            else RLogger.Log(TextSOS.Get("mte.client.patched", "[MTE] Already patched.").Value);

        }

        public static void DisposeClient()
        {
            //HarmonyInstance?.UnpatchSelf();
            //HarmonyInstance = null;
        }
    }
}