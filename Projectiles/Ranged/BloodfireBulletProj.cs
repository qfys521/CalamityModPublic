using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class BloodfireBulletProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private const int Lifetime = 1200;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 12;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.scale = 0.75f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.direction;
            if (Projectile.localAI[0] == 0)
            {
                Projectile.velocity *= 0.7f;
            }
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            // Lighting
            Lighting.AddLight(Projectile.Center, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.Red).ToVector3() * 0.5f);

            // Dust
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 6f)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, !ChildSafety.Disabled ? DustID.Cloud : (Main.rand.NextBool(3) ? 130 : 60), -Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.01f, 0.3f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 0.9f);
                    if (dust.type == 130)
                        dust.scale = Main.rand.NextFloat(0.35f, 0.55f);
                }
                if (targetDist < 1400f)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity, -Projectile.velocity * 0.01f, false, 4, 0.4f, !ChildSafety.Disabled ? Color.CornflowerBlue : Color.Firebrick);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }

        // These bullets glow in the dark.
        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 100);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.localAI[0] > 6f)
            {
                CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.Red));
            }
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= OnHitEffect(Main.player[Projectile.owner], target);

        // Returns the amount of bonus damage that should be dealt. Boosts life regeneration appropriately as a side effect.
        private float OnHitEffect(Player owner, NPC target)
        {
            // Adds 3 frames to natural life regen on each hit
            owner.lifeRegenTime += 3;

            // Provides up to 10% damage boost based on life regen time, caps at 3600
            float lifeRegenTimeContribution = Utils.GetLerpValue(0, 3600, owner.lifeRegenTime, true) * 0.1f;
            float finalDamageBoost = 1 + lifeRegenTimeContribution;

            if (lifeRegenTimeContribution == 0.1f) // Special hit visual if the bonus damage is at the cap.
            {
                for (int k = 0; k < 3; k++)
                {
                    BloodParticle blood = new BloodParticle(Projectile.Center, new Vector2(6.5f, 6.5f).RotatedByRandom(100) * Main.rand.NextFloat(0.8f, 1.2f), Main.rand.Next(8, 10 + 1), Main.rand.NextFloat(0.7f, 0.9f), !ChildSafety.Disabled ? Color.CornflowerBlue : Color.Red);
                    GeneralParticleHandler.SpawnParticle(blood);

                    int dustType = ModContent.DustType<DiamondDust>();
                    float velMulti = Main.rand.NextFloat(0.1f, 0.75f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, (-Projectile.velocity * 4).RotatedByRandom(0.4) * velMulti);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.55f, 0.65f);
                    dust.color = !ChildSafety.Disabled ? Color.CornflowerBlue : Color.Firebrick;
                    dust.noLightEmittance = true;
                    dust.noLight = true;
                    dust.fadeIn = 15;
                }
            }
            return finalDamageBoost;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 3; k++)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center, -Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(1f, 3f), false, Main.rand.Next(5, 7 + 1), Main.rand.NextFloat(0.4f, 0.6f), !ChildSafety.Disabled ? Color.CornflowerBlue : Color.Red);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            for (int k = 0; k < 8; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, (!ChildSafety.Disabled ? DustID.Cloud : (Main.rand.NextBool(3) ? 130 : 60)), new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 0.9f);
                if (dust.type == 130)
                    dust.scale = Main.rand.NextFloat(0.35f, 0.55f);
            }
        }
    }
}
