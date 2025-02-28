using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace DeeperNorth
{
    //Class to check for ExpandWorldSize and act accordingly to replace DN mountains only in DN area
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    public static class EWSFejdStartupPatch
    {
        internal static Assembly? EWSAssembly;
        internal static PropertyInfo? EWSRadius;
        internal static bool EWSActive;
        internal static bool EWSConfigValid;
        internal static Vector3 offsetVector3 = new(0, 0, 0);

        internal static void Postfix()
        {
            if (!Chainloader.PluginInfos.TryGetValue("expand_world_size", out var info))
            {
                DeeperNorth.DeeperNorthLogger.LogInfo(DeeperNorth.ModName + ": Expand_World_Size not detected. Proceeding without EWS integration."); return;
            }
            EWSActive = true;
            DeeperNorth.DeeperNorthLogger.LogInfo(DeeperNorth.ModName + ": Expand_World_Size detected. Proceeding with EWS integration.");
            EWSAssembly = info.Instance.GetType().Assembly;
            var type = EWSAssembly.GetType("ExpandWorldSize.Configuration");
            if (type == null)
            {
                DeeperNorth.DeeperNorthLogger.LogError(DeeperNorth.ModName + ": Error reading Expand_World_Size configuration.");
                return;
            }
            EWSRadius = AccessTools.Property(type, "WorldRadius");
            if (EWSRadius == null)
            {
                DeeperNorth.DeeperNorthLogger.LogError(DeeperNorth.ModName + ": Error reading Expand_World_Size WorldRadius.");
                return;
            }
            EWSConfigValid = true;
        }

        internal static bool IsBiomeEWSDeepNorth(float wx, float wy)
        {
            if (EWSActive && !EWSConfigValid) return false;
            float radius = EWSRadius == null ? 10000 : (float)EWSRadius.GetValue(null);
            float edgeBiomeOffset = .4f * radius;
            float edgeBiomeDistance = 1.2f * radius;
            offsetVector3.z = -edgeBiomeOffset;
            if ((Vector3.Distance(new Vector3(wx, wy), offsetVector3) < edgeBiomeDistance) || (wy <= 0)) return false;
            if (wy <= 0) return false;
            return true;
        }
    }
}
