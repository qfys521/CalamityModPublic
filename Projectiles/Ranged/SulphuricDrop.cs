using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SulphuricDrop : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Environment/AcidDrop";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 840;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.DamageType = DamageClass.Ranged;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB);
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.4f, 0.65f);
                dust.velocity = -Projectile.velocity * 0.4f;
                dust.color = Color.SeaGreen;
            }
            if (Projectile.velocity.Y < 0f)
            {
                Projectile.velocity.Y *= 0.97f;
            }
            else
            {
                Projectile.velocity.Y *= 1.03f;
                if (Projectile.velocity.Y > 16f)
                {
                    Projectile.velocity.Y = 16f;
                }
            }
            if (Projectile.velocity.Y > -1f && Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = 1f;
                Projectile.velocity.Y = 1f;
            }
            Projectile.velocity.X *= 0.995f;
            if (Projectile.ai[0] >= 2f)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0)
                {
                    Projectile.alpha = 0;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
       public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 120);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; ++i)
            {
                int bloodLifetime = Main.rand.Next(22, 25);
                float bloodScale = Main.rand.NextFloat(0.6f, 0.8f);
                Color bloodColor = Color.Lerp(Color.SeaGreen, Color.Lime, Main.rand.NextFloat());
                bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                if (Main.rand.NextBool(20))
                    bloodScale *= 2f;

                float randomSpeedMultiplier = Main.rand.NextFloat(1.25f, 2.25f);
                Vector2 bloodVelocity = Main.rand.NextVector2Unit() * 2 * randomSpeedMultiplier;
                bloodVelocity.Y -= 5f;
                BloodParticle blood = new BloodParticle(Projectile.Center, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                GeneralParticleHandler.SpawnParticle(blood);
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], new Color(255, 255, 255, 127), 2);
            return false;
        }
    }
}

