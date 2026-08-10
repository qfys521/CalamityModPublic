using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// Each Sunken Sea subbiome has a correspoding spawn condition boolean value and a biome type.
    /// </summary>
    public sealed class SunkenSeaBiomeCorrespondentDict : ModSystem
    {
        public static IDictionary<SunkenSeaBiomeFlags, (Func<NPC.Spawner, bool> SpawnCondition, int BiomeType)> Dict { get; private set; }

        public override void OnModLoad()
        {
            Dict = new Dictionary<SunkenSeaBiomeFlags, (Func<NPC.Spawner, bool> SpawnCondition, int BiomeType)>()
            {
                { SunkenSeaBiomeFlags.UndergroundDesert, (spawnInfo => spawnInfo.Player.ZoneDesert, -1 /* None needed. */) },
                { SunkenSeaBiomeFlags.TimelessShores, (spawnInfo => spawnInfo.Player.Calamity().ZoneTimelessShores, GetInstance<TimelessShoresBiome>().Type) },
                { SunkenSeaBiomeFlags.RadiantReefs, (spawnInfo => spawnInfo.Player.Calamity().ZoneRadiantReefs, GetInstance<RadiantReefsBiome>().Type) },
                { SunkenSeaBiomeFlags.PolypForest, (spawnInfo => spawnInfo.Player.Calamity().ZonePolypForest, GetInstance<PolypForestBiome>().Type) },
                { SunkenSeaBiomeFlags.GleamingBurrows, (spawnInfo => spawnInfo.Player.Calamity().ZoneGleamingBurrows, GetInstance<GleamingBurrowsBiome>().Type) },
                { SunkenSeaBiomeFlags.BasaltGully, (spawnInfo => spawnInfo.Player.Calamity().ZoneBasaltGully, GetInstance<BasaltGullyBiome>().Type) },
                { SunkenSeaBiomeFlags.ClamDen, (spawnInfo => spawnInfo.Player.Calamity().ZoneClamDen, GetInstance<ClamDenBiome>().Type) },
            };
        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }

        public static bool TryGet(SunkenSeaBiomeFlags flags, out Func<NPC.Spawner, bool> spawnCondition, out int biomeType)
        {
            if (!Dict.TryGetValue(flags, out var biomeInfoTuple))
            {
                spawnCondition = null;
                biomeType = default;
                return false;
            }

            spawnCondition = biomeInfoTuple.SpawnCondition;
            biomeType = biomeInfoTuple.BiomeType;
            return true;
        }
    }
}
