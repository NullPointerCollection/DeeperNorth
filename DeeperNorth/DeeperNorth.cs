using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace DeeperNorth
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class DeeperNorth : BaseUnityPlugin
    {
        private const string ModName = "DeeperNorth";
        private const string ModVersion = "1.0.1";
        private const string ModGUID = "xyz.919lab.mofongolero.DeeperNorth";
        Harmony harmony = new(ModGUID);

        public void Awake()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(HeightmapBuilder), nameof(HeightmapBuilder.Build))]
    public class HeightmapBuilderPatch
    {
        static void Prefix()
        {
            WorldGeneratorPatch.Disable = true;
        }
        static void Postfix(HeightmapBuilder.HMBuildData data)
        {
            WorldGeneratorPatch.Disable = false;
            // After height calculations, need to use the logic from WorldGeneratorPatch.
            Vector3 vector = data.m_center + new Vector3(data.m_width * data.m_scale * -0.5f, 0f, data.m_width * data.m_scale * -0.5f);
            if (data.m_cornerBiomes[0] == Heightmap.Biome.Mountain && WorldGenerator.IsDeepnorth(vector.x, vector.z))
                data.m_cornerBiomes[0] = Heightmap.Biome.DeepNorth;
            if (data.m_cornerBiomes[1] == Heightmap.Biome.Mountain && WorldGenerator.IsDeepnorth(vector.x + data.m_width * data.m_scale, vector.z))
                data.m_cornerBiomes[1] = Heightmap.Biome.DeepNorth;
            if (data.m_cornerBiomes[2] == Heightmap.Biome.Mountain && WorldGenerator.IsDeepnorth(vector.x, vector.z + data.m_width * data.m_scale))
                data.m_cornerBiomes[2] = Heightmap.Biome.DeepNorth;
            if (data.m_cornerBiomes[3] == Heightmap.Biome.Mountain && WorldGenerator.IsDeepnorth(vector.x + data.m_width * data.m_scale, vector.z + data.m_width * data.m_scale))
                data.m_cornerBiomes[3] = Heightmap.Biome.DeepNorth;
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), new Type[] { typeof(float), typeof(float), typeof(float), typeof(bool) })]
    public static class WorldGeneratorPatch
    {
        public static bool Disable = false;
        public static void Postfix(ref Heightmap.Biome __result, float wx, float wy)
        {
            if (Disable) return;
            if (__result == Heightmap.Biome.Mountain && WorldGenerator.IsDeepnorth(wx, wy))
            {
                __result = Heightmap.Biome.DeepNorth;
            }
        }
    }

}
