using CalamityMod.CalPlayer;
using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Systems;
using CalamityMod.Waters;
using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.BiomeManagers
{
    public class AbyssLayer3Biome : ModBiome
    {
        public override int Music
        {
            get
            {
                if (CalamityPlayer.areThereAnyDamnBosses)
                    return Main.curMusic;

                int? musicSlot = CalamityClientConfig.Instance.AbyssLayer3Alt ? 
                    CalamityMod.Instance.GetMusicFromMusicMod("AbyssLayer3Alt") : 
                    CalamityMod.Instance.GetMusicFromMusicMod("AbyssLayer3");
                    
                return musicSlot ?? MusicID.Underworld;
            }
        }

        public override ModWaterStyle WaterStyle => MiddleAbyssWater.Instance;
        public override int BiomeTorchItemType => ModContent.ItemType<ThermalTorch>();
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override string BestiaryIcon => "CalamityMod/BiomeManagers/AbyssLayer3Icon";
        public override string BackgroundPath => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer23";
        public override string MapBackground => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer23";

        public override bool IsBiomeActive(Player player)
        {
            if (Main.remixWorld)
            {
                return AbyssLayer1Biome.MeetsBaseAbyssRequirement(player, out int playerYTileCoords) && BiomeTileCounterSystem.Layer3Tiles >= 200 &&
                playerYTileCoords <= SulphurousSea.YStart - (int)(Main.UnderworldLayer * 0.4f) && playerYTileCoords > SulphurousSea.YStart - (int)(Main.UnderworldLayer * 0.6f);
            }

            return AbyssLayer1Biome.MeetsBaseAbyssRequirement(player, out int playerYTileCoords2) && BiomeTileCounterSystem.Layer3Tiles >= 200 &&
            playerYTileCoords2 > Main.rockLayer + Main.maxTilesY * 0.143 && playerYTileCoords2 <= Main.rockLayer + Main.maxTilesY * 0.268;
        }
    }
}
