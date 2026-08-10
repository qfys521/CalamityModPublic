using System;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyAura : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 210;
        }

        public override void AI()
        {
            int provIndex = CalamityGlobalNPC.holyBoss;
            if (provIndex < 0 || provIndex >= Main.maxNPCs || !Main.npc[provIndex].active)
                return;

            Projectile.Center = Main.npc[provIndex].Center;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float time = Main.GlobalTimeWrappedHourly % 10f / 10f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            int drawnAmt = 45;
            float[] posX = new float[drawnAmt];
            float[] posY = new float[drawnAmt];
            float[] hue = new float[drawnAmt];
            float[] size = new float[drawnAmt];
            int totalTime = 210;
            float colorChangeAmt = Utils.GetLerpValue(0f, 60f, Projectile.timeLeft, true) * Utils.GetLerpValue(totalTime, totalTime - 60, Projectile.timeLeft, true);
            float colorChangeAmt2 = Utils.GetLerpValue(0f, 60f, Projectile.timeLeft, true) * Utils.GetLerpValue(totalTime, 90f, Projectile.timeLeft, true);
            colorChangeAmt2 = Utils.GetLerpValue(0.2f, 0.5f, colorChangeAmt2, true);
            float sizeScale = 0.8f;
            float sizeScalar = (1f - sizeScale) / drawnAmt;
            float yPosOffset = 60f;
            float xPosOffset = 400f;
            Vector2 scale = new Vector2(12f, 12f);

            float amount2 = CalamityUtils.SineBumpEasing((float)Projectile.timeLeft / (float)totalTime, 1);

            for (int i = 0; i < drawnAmt; i++)
            {
                float timeScalar = (float)Math.Sin(time * MathHelper.TwoPi + (float)Math.PI / 2f + i / 2f);

                posX[i] = timeScalar * (xPosOffset - i * 3f) * amount2;

                posY[i] = (float)Math.Sin(time * MathHelper.TwoPi * 4f + (float)Math.PI / 3f + i) * yPosOffset * 2;
                posY[i] -= i * 3f;

                hue[i] = i / (float)drawnAmt * 2f + time;
                hue[i] = (timeScalar * 0.5f + 0.5f) * 0.6f + time;

                size[i] = sizeScale + (i + 1) * sizeScalar;
                size[i] *= 0.3f;

                float a = (float)Math.Sin(amount2 / 20) + 1;

                Color color = Color.Lerp(ProvUtils.GetProjectileColor(0, true), ProvUtils.GetProjectileColor(0, false), a);

                bool underworld = Projectile.ai[0] == 2f;
                if (!Main.zenithWorld)
                {
                    if (ProvUtils.StandardAI())
                    {
                        color.R = 255;
                        if (underworld)
                            color.B = 0;
                    }
                    else
                    {
                        byte blueValue = (byte)(MathHelper.Clamp(MathHelper.Lerp(0, 255, Main.npc[CalamityGlobalNPC.holyBoss].Calamity().newAI[3] / 120f), 0, 255));
                        if (blueValue > 255) blueValue = 255;
                        color.B = blueValue;
                        if (underworld)
                            color.G = (byte)(255 - blueValue);
                        else
                            color.R = (byte)(255 - blueValue);
                    }
                }

                color.A = 0;
                {
                    if (color.R > 0)
                        color.R = (byte)MathHelper.Lerp(0, color.R, amount2);
                    if (color.G > 0)
                        color.G = (byte)MathHelper.Lerp(0, color.G, amount2);
                    if (color.B > 0)
                        color.B = (byte)MathHelper.Lerp(0, color.B, amount2);

                    color.A = (byte)MathHelper.Lerp(0, color.A, amount2);
                }

                float rotation = MathHelper.PiOver2 + timeScalar * MathHelper.PiOver4 * -0.3f;

                Main.EntitySpriteDraw(texture, drawPosition + new Vector2(posX[i], posY[i]), null, color, rotation, origin, new Vector2(size[i], size[i]) * scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
