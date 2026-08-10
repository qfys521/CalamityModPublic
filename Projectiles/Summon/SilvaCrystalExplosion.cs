using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SilvaCrystalExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public ref float OwnerCheck => ref Projectile.ai[1];
        public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
        }

        public override void AI()
        {
            Color colorToUse = Main.hslToRgb(Projectile.ai[0], 1f, 0.5f);
            if (OwnerCheck < 0 || OwnerCheck >= 1000 || (!Main.projectile[(int)OwnerCheck].active && Main.projectile[(int)OwnerCheck].type != ModContent.ProjectileType<SilvaCrystal>()))
            {
                Projectile.ai[1] = -1f;
            }
            else
            {
                DelegateMethods.v3_1 = colorToUse.ToVector3() * 0.5f;
                Utils.PlotTileLine(Projectile.Center, Main.projectile[(int)OwnerCheck].Center, 8f, DelegateMethods.CastLight);
            }

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = Main.rand.NextFloat(0.8f) + 0.8f;
                Projectile.direction = Main.rand.NextBool() ? 1 : -1;
            }
            Projectile.rotation = Projectile.localAI[1] / 20f * MathHelper.TwoPi * Projectile.direction;
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 16;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }
            if (Projectile.alpha == 0)
                Lighting.AddLight(Projectile.Center, colorToUse.ToVector3() * 0.5f);

            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextBool(10))
                {
                    Vector2 silvaDustVel = Vector2.UnitY.RotatedBy(i * MathHelper.Pi + Projectile.rotation) * 2.5f;
                    Dust silvaDust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, silvaDustVel, 225, colorToUse, Projectile.Opacity * Projectile.localAI[0]);
                    silvaDust.noGravity = true;
                    silvaDust.noLight = true;
                }
                if (Main.rand.NextBool(10))
                {
                    Vector2 silvaDustVel2 = Vector2.UnitY.RotatedBy(i * MathHelper.Pi) * 2.5f;
                    Dust silvaDust2 = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, silvaDustVel2, 225, colorToUse, Projectile.Opacity * Projectile.localAI[0]);
                    silvaDust2.noGravity = true;
                    silvaDust2.noLight = true;
                }
            }
            if (Main.rand.NextBool(10))
            {
                float dustVelMod = Main.rand.NextFloat(1f, 3f);
                float fadeIn = Main.rand.NextFloat(1f, 2f);
                float dustScale = Main.rand.NextFloat(1f, 2f);
                Vector2 randVector = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(20, 120f);

                Vector2 dustPos = Projectile.Center + randVector;
                Point dustCoords = dustPos.ToTileCoordinates();
                if (WorldGen.InWorld(dustCoords.X, dustCoords.Y) && !WorldGen.SolidTile(dustCoords.X, dustCoords.Y))
                {
                    Dust rainbowDust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, -Vector2.UnitY * Main.rand.NextFloat(1.6f, 7.5f), 127, colorToUse, dustScale);
                    rainbowDust.noGravity = true;
                    rainbowDust.fadeIn = fadeIn;
                    rainbowDust.noLight = true;

                    Dust rainbowDust2 = Dust.BetterCloneDust(rainbowDust);
                    rainbowDust2.scale *= 0.65f;
                    rainbowDust2.fadeIn *= 0.65f;
                    rainbowDust2.color = new Color(255, 255, 255, 255);
                }
            }
            Projectile.scale = Projectile.Opacity / 2f * Projectile.localAI[0];
            Projectile.velocity = Vector2.Zero;

            Projectile.localAI[1] += 1f;
            if (Projectile.localAI[1] >= 15f)
                Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 spinningpoint = -Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * 3f;
            int rando = Main.rand.Next(7, 12+1);
            Vector2 dustVel = new Vector2(2.1f, 2f) * Main.rand.NextFloat(0.8f, 1.2f);
            Color newColor = Main.hslToRgb(Projectile.ai[0], 1f, 0.5f);
            newColor.A = 255;

            for (int i = 0; i < rando; i++)
            {
                Dust dustID = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, spinningpoint.RotatedBy(MathHelper.TwoPi * i / rando) * dustVel, 0, newColor, 2f);
                dustID.noGravity = true;
                dustID.fadeIn = Main.rand.NextFloat() * 2f;

                Dust dustCloning = Dust.BetterCloneDust(dustID);
                dustCloning.scale /= 2f;
                dustCloning.fadeIn /= 2f;
                dustCloning.color = Color.White;
            }
            for (int j = 0; j < rando; j++)
            {
                Dust dustID = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, spinningpoint.RotatedBy(MathHelper.TwoPi * j / rando) * dustVel * Main.rand.NextFloat(0.8f), 0, newColor, Main.rand.NextFloat());
                dustID.noGravity = true;
                dustID.fadeIn = Main.rand.NextFloat() * 2f;

                Dust dustCloning2 = Dust.BetterCloneDust(dustID);
                dustCloning2.scale /= 2f;
                dustCloning2.fadeIn /= 2f;
                dustCloning2.color = Color.White;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.friendly = true;
                Projectile.usesIDStaticNPCImmunity = true;
                Projectile.idStaticNPCHitCooldown = 10;
                Projectile.ExpandHitboxBy(60);
                Projectile.Damage();
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 projPos = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Color projColor = Main.hslToRgb(Projectile.ai[0], 1f, 0.5f).MultiplyRGBA(new Color(255, 255, 255, 0));
            Vector2 origin = tex.Size() / 2f;
            SpriteEffects sp = SpriteEffects.None;

            Main.EntitySpriteDraw(tex, projPos, null, projColor, Projectile.rotation, origin, Projectile.scale * 2f, sp);
            Main.EntitySpriteDraw(tex, projPos, null, projColor, 0f, origin, Projectile.scale * 2f, sp);

            if (OwnerCheck != -1f && Projectile.Opacity > 0.3f)
            {
                Color colorArea = Lighting.GetColor((int)(Projectile.position.X + Projectile.width * 0.5) / 16, (int)((Projectile.position.Y + Projectile.height * 0.5) / 16.0));
                Color colorAlpha = Projectile.GetAlpha(colorArea);

                Vector2 projDirection = Main.projectile[(int)OwnerCheck].Center - Projectile.Center;
                Vector2 projDistance = new Vector2(1f, projDirection.Length() / (float)tex.Height);
                float drawRotation = projDirection.ToRotation() + MathHelper.PiOver2;
                float colorClamp = MathHelper.Clamp(MathHelper.Distance(15f, Projectile.localAI[1]) / 20f, 0f, 1f);
                if (colorClamp > 0f)
                {
                    Main.EntitySpriteDraw(tex, projPos + projDirection / 2f, null, projColor * colorClamp, drawRotation, origin, projDistance, sp);
                    Main.EntitySpriteDraw(tex, projPos + projDirection / 2f, null, colorAlpha * colorClamp, drawRotation, origin, projDistance / 2f, sp);
                }
            }
            return false;
        }
    }
}
