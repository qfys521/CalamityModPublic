using System;
using CalamityMod.ILEditing;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class ModLavaStyleSystem : ModSystem
    {
        public static ModLavaStyle[] LavaStyles = new ModLavaStyle[ModLavaStyleLoader.VanillaCount];
        public static Asset<Texture2D>[] Textures = new Asset<Texture2D>[ModLavaStyleLoader.VanillaCount];
        public static Asset<Texture2D>[] BlockTextures = new Asset<Texture2D>[ModLavaStyleLoader.VanillaCount];
        public static Asset<Texture2D>[] SlopeTextures = new Asset<Texture2D>[ModLavaStyleLoader.VanillaCount];
        public static Asset<Texture2D>[] WaterfallTextures = new Asset<Texture2D>[ModLavaStyleLoader.VanillaCount];
        public static float[] LavaAlpha = new float[ModLavaStyleLoader.VanillaCount];

        public static int LavaStyle = 2;

        public static bool Initialized { get; private set; } = false;
        public static bool TextureArrayReady { get; private set; } = false;

        public override void ResizeArrays()
        {
            var modLavaStyles = ModLavaStyleLoader.AllStyles;
            var totalCount = ModLavaStyleLoader.TotalCount;

            Array.Resize(ref LavaStyles, totalCount);
            Array.Resize(ref Textures, totalCount);
            Array.Resize(ref BlockTextures, totalCount);
            Array.Resize(ref SlopeTextures, totalCount);
            Array.Resize(ref WaterfallTextures, totalCount);
            Array.Resize(ref LavaAlpha, totalCount);

            foreach (var modLavaStyle in modLavaStyles)
            {
                var slot = modLavaStyle.Slot;
                LavaStyles[slot] = modLavaStyle;
                Textures[slot] = ModContent.Request<Texture2D>(modLavaStyle.Texture);
                BlockTextures[slot] = ModContent.Request<Texture2D>(modLavaStyle.BlockTexture);
                SlopeTextures[slot] = ModContent.Request<Texture2D>(modLavaStyle.SlopeTexture);
                WaterfallTextures[slot] = ModContent.Request<Texture2D>(modLavaStyle.WaterfallTexture);
            }

            LavaAlpha[0] = 1.0f; // Setting Vanilla Lava to Full Alpha
            Textures[0] = LiquidRenderer.Instance._liquidTextures[1];
            SlopeTextures[0] = TextureAssets.LiquidSlope[1];
            BlockTextures[0] = TextureAssets.Liquid[1];
            WaterfallTextures[0] = TerrariaInternals.WaterfallTextures(Main.instance.waterfallManager)[1];

            TextureArrayReady = true;
        }

        public override void Load()
        {
            base.Load();
            
            if (ExternalMods.biomeLava == null)
            {
                ManipulatorManager.ApplyEdits += ApplyEdits;
                Main.QueueMainThreadAction(PrepareRT);
                Main.OnPreDraw += UpdateRT;
                Initialized = true;
            }
        }

        public override void OnModUnload()
        {
            if (Initialized)
            {
                Main.QueueMainThreadAction(DisposeRT);
                Main.OnPreDraw -= UpdateRT;
                Initialized = false;
            }

            TextureArrayReady = false;
        }

        public override void PreUpdatePlayers()
        {
            LavaStyle = 0;
            foreach (var lavaStyle in ModLavaStyleLoader.AllStyles)
            {
                bool? flag = lavaStyle?.IsLavaActive();
                if (flag != null && flag == true)
                {
                    LavaStyle = lavaStyle.Slot;
                }
            }

            for (int type = 0; type < ModLavaStyleLoader.TotalCount; type++)
            {
                if (LavaStyle == type)
                {
                    LavaAlpha[type] += 0.125f;
                    if (LavaAlpha[type] > 1f)
                    {
                        LavaAlpha[type] = 1f;
                    }
                }
                else
                {
                    LavaAlpha[type] -= 0.125f;
                    if (LavaAlpha[type] < 0f)
                    {
                        LavaAlpha[type] = 0f;
                    }
                }
            }
        }
    }
}
