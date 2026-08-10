using System;
using CalamityMod.Skies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.Backgrounds
{
    public class AstralSnowSurfaceBGStyle : ModSurfaceBackgroundStyle
    {
        readonly int FrontBGYOffset = 275;
        readonly int CloseBGYOffset = 175;
        readonly int MiddleBGYOffset = 475;

        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            for (int i = 0; i < fades.Length; i++)
            {
                if (i == Slot)
                {
                    fades[i] += transitionSpeed;
                    if (fades[i] > 1f)
                        fades[i] = 1f;
                }
                else
                {
                    fades[i] -= transitionSpeed;
                    if (fades[i] < 0f)
                        fades[i] = 0f;
                }
            }
        }

        public override int ChooseFarTexture() => BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralSurfaceHorizon");

        public override int ChooseMiddleTexture() => BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralSurfaceFar");

        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Skies/AstralSnowSurfaceMiddle");

        public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
        {
            //Please see AstralSurfaceBGStyle for comments, code is pretty much identical here
            float scAdj = Main.instance.scAdj;
            Color COSBMAplha = Main.ColorOfSurfaceBackgroundsModified;
            Color ColorOfSurfaceBackgroundsModified = new Color(63, 51, 90, COSBMAplha.A);
            bool canBGDraw = false;
            if ((!Main.remixWorld || (Main.gameMenu && !WorldGen.remixWorldGen)) && (!WorldGen.remixWorldGen || !WorldGen.drunkWorldGen))
                canBGDraw = true;
            if (Main.mapFullscreen)
                canBGDraw = false;
            int offset = 30;
            if (Main.gameMenu)
                offset = 0;
            if (WorldGen.drunkWorldGen)
                offset = -180;
            float surfacePosition = (float)Main.worldSurface;
            if (surfacePosition == 0f)
                surfacePosition = 1f;
            double backgroundTopMagicNumber = -(Main.screenPosition.Y - 300f) / (surfacePosition * 16f);
            float bgGlobalScaleMultiplier = 2f;
            int pushBGTopHack;
            int offset2 = -180;
            int menuOffset = 0;
            if (Main.gameMenu)
            {
                menuOffset -= offset2;
            }
            pushBGTopHack = menuOffset;
            pushBGTopHack += offset;
            pushBGTopHack += offset2;
            if (canBGDraw)
            {
                var bgScale = 1.25f;
                var bgParallax = 0.4;
                var bgTopY = (int)(backgroundTopMagicNumber * 1800.0 + 1500.0) + (int)scAdj + pushBGTopHack;
                bgScale *= bgGlobalScaleMultiplier;
                var bgWidthScaled = (int)((float)SkyTextureRefs.AstralSnowSurfaceMiddle.Value.Width * bgScale);
                SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1.2f / (float)bgParallax);
                var bgStartX = (int)(0.0 - Math.IEEERemainder((double)Main.screenPosition.X * bgParallax, bgWidthScaled) - (double)(bgWidthScaled / 2));
                if (Main.gameMenu)
                    bgTopY = 320 + pushBGTopHack;

                var bgLoops = Main.screenWidth / bgWidthScaled + 2;
                if ((double)Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0)
                {
                    for (int i = 0; i < bgLoops; i++)
                    {
                        Main.spriteBatch.Draw(SkyTextureRefs.AstralSnowSurfaceMiddle.Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + MiddleBGYOffset), new Rectangle(0, 0, SkyTextureRefs.AstralSnowSurfaceMiddle.Value.Width, SkyTextureRefs.AstralSnowSurfaceMiddle.Value.Height), ColorOfSurfaceBackgroundsModified, 0f, default(Vector2), bgScale, SpriteEffects.None, 0f);
                    }
                }

                bgScale = 1.31f;
                bgParallax = 0.43;
                bgTopY = (int)(backgroundTopMagicNumber * 1950.0 + 1750.0) + (int)scAdj + pushBGTopHack;
                bgScale *= bgGlobalScaleMultiplier;
                bgWidthScaled = (int)((float)SkyTextureRefs.AstralSurfaceClose.Value.Width * bgScale);
                SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1f / (float)bgParallax);
                bgStartX = (int)(0.0 - Math.IEEERemainder((double)Main.screenPosition.X * bgParallax, bgWidthScaled) - (double)(bgWidthScaled / 2));
                if (Main.gameMenu)
                {
                    bgTopY = 400 + pushBGTopHack;
                    bgStartX -= 80;
                }

                bgLoops = Main.screenWidth / bgWidthScaled + 2;
                if ((double)Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0)
                {
                    for (int i = 0; i < bgLoops; i++)
                    {
                        Main.spriteBatch.Draw(SkyTextureRefs.AstralSurfaceClose.Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + CloseBGYOffset), new Rectangle(0, 0, SkyTextureRefs.AstralSurfaceClose.Value.Width, SkyTextureRefs.AstralSurfaceClose.Value.Height), ColorOfSurfaceBackgroundsModified, 0f, default(Vector2), bgScale, SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(SkyTextureRefs.AstralSurfaceCloseGlow.Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + CloseBGYOffset), new Rectangle(0, 0, SkyTextureRefs.AstralSurfaceCloseGlow.Value.Width, SkyTextureRefs.AstralSurfaceCloseGlow.Value.Height), new Color(Color.White.R * 0.7f, Color.White.G * 0.7f, Color.White.B * 0.7f, COSBMAplha.A), 0f, default(Vector2), bgScale, SpriteEffects.None, 0f);
                    }
                }
                bgScale = 1.34f;
                bgParallax = 0.49;
                bgTopY = (int)(backgroundTopMagicNumber * 2100.0 + 2000.0) + (int)scAdj + pushBGTopHack;
                bgScale *= bgGlobalScaleMultiplier;
                bgWidthScaled = (int)(SkyTextureRefs.AstralSurfaceFront.Value.Width * bgScale);
                SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1f / (float)bgParallax);
                bgStartX = (int)(0.0 - Math.IEEERemainder((double)Main.screenPosition.X * bgParallax, bgWidthScaled) - (double)(bgWidthScaled / 2));
                if (Main.gameMenu)
                {
                    bgTopY = 480 + pushBGTopHack;
                    bgStartX -= 120;
                }

                bgLoops = Main.screenWidth / bgWidthScaled + 2;
                if ((double)Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0)
                {
                    for (int i = 0; i < bgLoops; i++)
                    {
                        Main.spriteBatch.Draw(SkyTextureRefs.AstralSurfaceFront.Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + FrontBGYOffset), new Rectangle(0, 0, SkyTextureRefs.AstralSurfaceFront.Value.Width, SkyTextureRefs.AstralSurfaceFront.Value.Height), ColorOfSurfaceBackgroundsModified, 0f, default(Vector2), bgScale, SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(SkyTextureRefs.AstralSurfaceFrontGlow.Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + FrontBGYOffset), new Rectangle(0, 0, SkyTextureRefs.AstralSurfaceFrontGlow.Value.Width, SkyTextureRefs.AstralSurfaceFrontGlow.Value.Height), new Color(Color.White.R * 0.9f, Color.White.G * 0.9f, Color.White.B * 0.9f, COSBMAplha.A), 0f, default(Vector2), bgScale, SpriteEffects.None, 0f);
                    }
                }
            }
            return false;
        }
    }
}
