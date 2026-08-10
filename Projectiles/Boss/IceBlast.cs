using System;
using CalamityMod.NPCs.Cryogen;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class IceBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        private const int TimeLeft = 600;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1.2f;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.coldDamage = true;
            Projectile.timeLeft = TimeLeft;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 1f)
            {
                float spreadOutCutoffTime = TimeLeft - 90;
                float homeInCutoffTime = TimeLeft - 165;
                float minAcceleration = 0.05f;
                float maxAcceleration = 0.1f;
                float homingVelocity = 20f;

                if (Projectile.timeLeft > homeInCutoffTime && Projectile.timeLeft <= spreadOutCutoffTime)
                {
                    int playerIndex = (int)Projectile.ai[0];
                    Vector2 velocity = Projectile.velocity;
                    if (Main.player.IndexInRange(playerIndex))
                    {
                        Player player = Main.player[playerIndex];
                        velocity = Projectile.DirectionTo(player.Center) * homingVelocity;
                    }

                    float amount = MathHelper.Lerp(minAcceleration, maxAcceleration, Utils.GetLerpValue(spreadOutCutoffTime, 30f, Projectile.timeLeft, clamped: true));
                    Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, velocity, amount);
                }
            }

            Lighting.AddLight((int)((Projectile.position.X + (Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (Projectile.height / 2)) / 16f), 0f, 0.25f, 0.25f);

            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 0.6f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(1f, 1f, 1f, 1f) * Projectile.Opacity;

        public override void OnKill(int timeLeft)
        {
            //SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                int iceDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, 0f, 0f, 0, default, 1f);
                if (!Main.rand.NextBool(3))
                {
                    Dust dust = Main.dust[iceDust];
                    dust.velocity *= 2f;
                    Main.dust[iceDust].noGravity = true;
                    dust = Main.dust[iceDust];
                    dust.scale *= 1.75f;
                }
                else
                {
                    Dust dust = Main.dust[iceDust];
                    dust.scale *= 0.5f;
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(BuffID.Frostburn, 120);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Projectile.DrawProjectileWithBackglow(Cryogen.BackglowColor, lightColor, 4f);
            return false;
        }
    }
}
