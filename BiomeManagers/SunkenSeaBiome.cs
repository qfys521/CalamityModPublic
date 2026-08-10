using Terraria;
using Terraria.ModLoader;
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using CalamityMod.Systems;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace CalamityMod.BiomeManagers
{
    //this is just a global sunken sea biome to check if you are in any of the existing biomes
    public class SunkenSeaBiome : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override int Music => CalamityMod.Instance.GetMusicFromMusicMod("SunkenSea") ?? MusicID.OceanNight;
        public override void Load()
        {
            //apply the drawblack edits here since all sunken sea biomes will have custom backgrounds
            IL_Main.DrawBlack += ChangeBlackThreshold;
            On_Main.DrawBlack += ForceDrawBlack;
        }

        private void ForceDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool intoRenderTargets, bool force)
        {
            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.SunkenSeaBiome>()) && Main.BackgroundEnabled)
            {
                orig(self, intoRenderTargets, true);
            }
            else
            {
                orig(self, intoRenderTargets, force);
            }
        }

        private float NewThreshold(float orig)
        {
            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.SunkenSeaBiome>()) && Main.BackgroundEnabled)
            {
                return 0.1f;
            }
            else
            {
                return orig;
            }
        }

        private void ChangeBlackThreshold(ILContext il)
        {
            if (!Main.BackgroundEnabled)
                return;

            var c = new ILCursor(il);
            int brightnessLocalIndex = -1;
            int thresholdLocalIndex = -1;

            // Find the threshold through its comparison with Lighting.Brightness instead of relying on
            // local indices, which changed when DrawBlack gained its render-target parameter.
            bool foundThreshold = c.TryGotoNext(
                i => i.MatchCall(typeof(Lighting), nameof(Lighting.Brightness)),
                i => i.MatchStloc(out brightnessLocalIndex)) &&
                c.TryGotoNext(
                    i => i.MatchLdloc(brightnessLocalIndex),
                    i => i.MatchLdloc(out thresholdLocalIndex));

            c.Index = 0;
            bool foundInsertionPoint = c.TryGotoNext(MoveType.After,
                i => i.MatchCall(typeof(TileDrawing), nameof(TileDrawing.GetScreenDrawArea)));

            if (!foundThreshold || thresholdLocalIndex < 0 || !foundInsertionPoint)
            {
                CalamityMod.Instance.Logger.Warn("Could not apply the Sunken Sea DrawBlack threshold IL edit.");
                return;
            }

            c.Emit(OpCodes.Ldloc, thresholdLocalIndex);
            c.EmitDelegate<Func<float, float>>(NewThreshold);
            c.Emit(OpCodes.Stloc, thresholdLocalIndex);
        }
        
        public override string BestiaryIcon => "CalamityMod/BiomeManagers/SunkenSeaIcon";
        // Placeholder until we get a dedicated Sunken Sea background
        public override string BackgroundPath => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";
        public override string MapBackground => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";

        public override bool IsBiomeActive(Player player)
        {
            return BiomeTileCounterSystem.SunkenSeaBurrowsTiles > 200 || BiomeTileCounterSystem.SunkenSeaPolypTiles > 200 ||
            BiomeTileCounterSystem.SunkenSeaReefsTiles > 200 || BiomeTileCounterSystem.SunkenSeaShoresTiles > 200;
        }
    }
}
