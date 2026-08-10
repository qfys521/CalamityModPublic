using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class SeashineSwordProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        private const int TimeLeft = 120;

        private const int MaxAlpha = 255;

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
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = TimeLeft;
            Projectile.alpha = MaxAlpha;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, (byte.MaxValue - Projectile.alpha) * 0.5f / byte.MaxValue, (byte.MaxValue - Projectile.alpha) * 0.5f / byte.MaxValue);

            if (Projectile.localAI[1] < 7f)
            {
                Projectile.localAI[1] += 1f;
            }
            else
            {
                float dustScale = 1.8f * Projectile.scale;
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - Projectile.velocity.X + 2f, Projectile.position.Y + 2f - Projectile.velocity.Y), 8, 8, DustID.Flare_Blue, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= -0.25f;

                dust = Dust.NewDust(new Vector2(Projectile.position.X - Projectile.velocity.X + 2f, Projectile.position.Y + 2f - Projectile.velocity.Y), 8, 8, DustID.Flare_Blue, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= -0.25f;
                Main.dust[dust].position -= Projectile.velocity * 0.5f;

                if (Projectile.localAI[0] == 0f)
                {
                    Projectile.scale -= 0.02f;
                    Projectile.alpha += 10;
                    if (Projectile.alpha >= MaxAlpha - 5)
                    {
                        Projectile.alpha = MaxAlpha;
                        Projectile.localAI[0] = 1f;
                    }
                }
                else if (Projectile.localAI[0] == 1f)
                {
                    Projectile.scale += 0.02f;
                    Projectile.alpha -= 10;
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

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 2);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(128, byte.MaxValue, byte.MaxValue);

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int i = 4; i < 31; i++)
            {
                float oldXVel = Projectile.oldVelocity.X * (30f / (float)i);
                float oldYVel = Projectile.oldVelocity.Y * (30f / (float)i);

                float dustScale = 3.6f * Projectile.scale;
                int dust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - oldXVel * 0.5f, Projectile.oldPosition.Y - oldYVel * 0.5f), 8, 8, DustID.Flare_Blue, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;

                dustScale = 3f * Projectile.scale;
                dust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - oldXVel * 0.5f, Projectile.oldPosition.Y - oldYVel * 0.5f), 8, 8, DustID.Flare_Blue, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, Projectile.alpha, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.1f;
            }
        }
    }
}
