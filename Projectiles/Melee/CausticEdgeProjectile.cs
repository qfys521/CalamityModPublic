using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class CausticEdgeProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        private const float ColorAlternateTime = 30f;

        private const int TimeLeft = 600;

        private const int MaxAlpha = 128;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = TimeLeft;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = MaxAlpha;
        }

        public override void AI()
        {
            Projectile.ai[2] += 1f;
            if (Projectile.ai[2] >= ColorAlternateTime)
                Projectile.ai[2] = -ColorAlternateTime;

            Vector3 colorOne = new Vector3(0.9f, 0f, 0.9f);
            Vector3 colorTwo = new Vector3(0.6f, 1.2f, 0f);
            float colorScalar = Math.Abs(Projectile.ai[2]) / ColorAlternateTime;
            Vector3 lightColor = Vector3.Lerp(colorOne, colorTwo, colorScalar);
            Lighting.AddLight(Projectile.Center, lightColor.X, lightColor.Y, lightColor.Z);

            if (Projectile.localAI[1] > 7f)
            {
                float dustScale = 1.25f * Projectile.scale;
                int maxRandom = 10;
                int chanceOfGreenDust = (int)Math.Round(MathHelper.Lerp(0f, (float)maxRandom, colorScalar));
                int dustType = Main.rand.Next(maxRandom) < chanceOfGreenDust ? DustID.GreenFairy : DustID.Venom;
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - Projectile.velocity.X + 2f, Projectile.position.Y + 2f - Projectile.velocity.Y), 8, 8, dustType, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= -0.25f;
                if (dustType == DustID.Venom)
                    Main.dust[dust].fadeIn = 1.5f;

                dustType = Main.rand.Next(maxRandom) < chanceOfGreenDust ? DustID.GreenFairy : DustID.Venom;
                dust = Dust.NewDust(new Vector2(Projectile.position.X - Projectile.velocity.X + 2f, Projectile.position.Y + 2f - Projectile.velocity.Y), 8, 8, dustType, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= -0.25f;
                Main.dust[dust].position -= Projectile.velocity * 0.5f;
                if (dustType == DustID.Venom)
                    Main.dust[dust].fadeIn = 1.5f;
            }

            if (Projectile.localAI[1] < 15f)
            {
                Projectile.localAI[1] += 1f;
            }
            else
            {
                if (Projectile.localAI[0] == 0f)
                {
                    Projectile.scale -= 0.02f;
                    Projectile.alpha += 15;
                    if (Projectile.alpha >= MaxAlpha - 5)
                    {
                        Projectile.alpha = MaxAlpha;
                        Projectile.localAI[0] = 1f;
                    }
                }
                else if (Projectile.localAI[0] == 1f)
                {
                    Projectile.scale += 0.02f;
                    Projectile.alpha -= 15;
                    if (Projectile.alpha <= 0)
                    {
                        Projectile.alpha = 0;
                        Projectile.localAI[0] = 0f;
                    }
                }
            }

            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            }

            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver4;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft > TimeLeft - 5)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Vector3 colorOne = new Vector3(100f, 0f, 100f);
            Vector3 colorTwo = new Vector3(67f, 133f, 0f);
            Vector3 newLightColor = Vector3.Lerp(colorOne, colorTwo, Math.Abs(Projectile.ai[2]) / ColorAlternateTime);
            return new Color((int)newLightColor.X, (int)newLightColor.Y, (int)newLightColor.Z, 0);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;

                Projectile.velocity *= 0.5f;

                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 60);
            Projectile.velocity *= 0.5f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int i = 4; i < 31; i++)
            {
                float oldXVel = Projectile.oldVelocity.X * (30f / (float)i);
                float oldYVel = Projectile.oldVelocity.Y * (30f / (float)i);

                float dustScale = 1.8f * Projectile.scale;
                int dustType = Main.rand.NextBool() ? DustID.GreenFairy : DustID.Venom;
                int dust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - oldXVel * 0.5f, Projectile.oldPosition.Y - oldYVel * 0.5f), 8, 8, dustType, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                if (dustType == DustID.Venom)
                    Main.dust[dust].fadeIn = 1.5f;

                dustScale = 1.4f * Projectile.scale;
                dustType = Main.rand.NextBool() ? DustID.GreenFairy : DustID.Venom;
                dust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - oldXVel * 0.5f, Projectile.oldPosition.Y - oldYVel * 0.5f), 8, 8, dustType, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.1f;
                if (dustType == DustID.Venom)
                    Main.dust[dust].fadeIn = 1.5f;
            }
        }
    }
}
