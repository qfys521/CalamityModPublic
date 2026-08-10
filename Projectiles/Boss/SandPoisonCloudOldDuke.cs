using System;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class SandPoisonCloudOldDuke : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 10;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 45;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1800;
            Projectile.alpha = 80;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Projectile.spriteDirection = 1;
            if (Projectile.Center.X < Main.LocalPlayer.Center.X) Projectile.spriteDirection = -1;

            Lighting.AddLight(Projectile.Center, 0.1f, 0.7f, 0f);

            Projectile.ai[0] += 1f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.ai[0] < 1620f)
            {
                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Top + new Vector2(Main.rand.NextFloat(-12, 12), 6f), new Vector2(0, -Main.rand.NextFloat(2)), false, 20, Main.rand.NextFloat(0.5f, 1.2f), new Color(100, 255, 0)));

                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            if (Projectile.ai[0] > 1620f)
            {
                Projectile.damage = 0;
            }
            else if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.Kill();
            }

            Projectile.velocity *= 0.995f;

            if (Math.Abs(Projectile.velocity.X) > 0f)
            {
                Projectile.spriteDirection = -Projectile.direction;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor.R = (byte)(100 * Projectile.Opacity);
            lightColor.G = (byte)(155 * Projectile.Opacity);
            lightColor.B = (byte)(55 * Projectile.Opacity);
            lightColor.A = 0;
            CalamityUtils.DrawProjectileWithBackglow(Projectile, new Color(20, 60, 26, 0), Color.White, 2f);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 20f, targetHitbox);

        public override bool CanHitPlayer(Player target) => Projectile.ai[0] < 1620f;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Irradiated>(), 480);
        }
    }
}
