using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Summon.SquirrelSquireStaff;

namespace CalamityMod.Projectiles.Summon
{
    public class SquirrelSquireAcorn : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        public override void SetStaticDefaults() => ProjectileID.Sets.SentryShot[Type] = true;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = ProjectileTimeAlive;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        public override void AI()
        {
            if (Projectile.timeLeft < ProjectileTimeAlive - TimeBeforeFalling)
                Projectile.velocity.Y += ProjectileGravity;

            Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.X * 3f);

            if (!Main.dedServ)
            {
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 7 : 79);
                    dust.noGravity = true;
                    dust.scale = 0.85f;
                    dust.velocity = Projectile.velocity * 0.4f;
                    dust.noLight = true;
                    dust.noLightEmittance = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.ExpandHitboxBy(ProjectileAoERadiusSize * 2);
            if (Main.myPlayer == Projectile.owner)
                Projectile.Damage();

            if (Main.dedServ)
                return;

            for (int k = 0; k < 6; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 7 : 167);
                dust.scale = Main.rand.NextFloat(0.6f, 1.1f);
                dust.velocity = new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.8f);
                dust.noGravity = false;
                Particle spark = new AltLineParticle(Projectile.Center, new Vector2(8, 8).RotatedByRandom(100) * Main.rand.NextFloat(0.4f, 0.9f), true, 15, 0.7f, Color.Lerp(Color.White, Color.Brown, Main.rand.NextFloat(0.3f, 0.7f)) * 0.5f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
