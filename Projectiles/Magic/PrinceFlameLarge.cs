using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class PrinceFlameLarge : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float Time => ref Projectile.ai[0];
        public const int Lifetime = 60;
        public const int FadeoutTime = 24;
        public static Color FlameColor => new Color(255, 180, 80);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.penetrate = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 11;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            // Create rose petals.
            if (Time == 0f)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust rose = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RosePiece>());
                    rose.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.61f) * 2.5f;
                    rose.velocity.Y += Main.rand.NextFloat(-2.4f, 1.6f);
                    rose.velocity *= 0.4f;
                    rose.scale = Main.rand.NextFloat(1.2f, 1.7f);
                    rose.noGravity = Main.rand.NextBool();
                }
            }

            // Explode before dissipating.
            if (Projectile.timeLeft == FadeoutTime)
                ExplodeIntoFireballs();

            bool dissipating = Projectile.timeLeft < FadeoutTime;

            // Dissipate at the end of the projectile's lifetime.
            if (dissipating)
            {
                Projectile.frame = (int)Math.Round(MathHelper.Lerp(4f, 7f, Utils.GetLerpValue(FadeoutTime, 0f, Projectile.timeLeft, true)));
                Projectile.velocity *= 0.95f;

                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), ModContent.DustType<LightDust>(), (-Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.2f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 1f);
                    dust.color = FlameColor * 0.8f;

                    Particle mist = new MediumMistParticle(Projectile.Center, Projectile.velocity * 0.5f, FlameColor, Color.DarkSlateGray, Main.rand.NextFloat(0.4f, 0.6f), 140, Main.rand.NextFloat(-0.1f, 0.1f));
                    GeneralParticleHandler.SpawnParticle(mist);
                }

                Particle dyingSmoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, Color.Lerp(FlameColor, Color.DarkSlateGray, 0.3f), 12, Main.rand.NextFloat(0.3f, 0.4f), 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), true);
                GeneralParticleHandler.SpawnParticle(dyingSmoke);
                return;
            }

            // Create bursts of fire dust
            if (Time % 2f == 1f && Time > 5f)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), ModContent.DustType<LightDust>(), (-Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.3f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.5f);
                dust.color = FlameColor;

                Particle mist = new MediumMistParticle(Projectile.Center, Projectile.velocity * 0.5f, FlameColor, Color.DarkSlateGray, Main.rand.NextFloat(1f, 1.5f), 180, Main.rand.NextFloat(-0.1f, 0.1f));
                GeneralParticleHandler.SpawnParticle(mist);
            }

            Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, FlameColor, 24, Main.rand.NextFloat(0.6f, 1f), 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), true);
            GeneralParticleHandler.SpawnParticle(smoke);

            Time++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % 4;
        }

        public void ExplodeIntoFireballs()
        {
            // Play a fizzle sound.
            SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite, Projectile.Center);
            if (Main.myPlayer != Projectile.owner)
                return;

            // And explode into a burst of fire.
            int damage = (int)(Projectile.damage * 0.66f);
            float kb = Projectile.knockBack * 0.4f;
            float offsetAngle = Main.rand.NextFloatDirection() * 0.31f;
            for (float i = 0f; i < MathHelper.TwoPi; i += 0.05f)
            {
                Vector2 velocity = (i + offsetAngle + MathHelper.ToRadians(45f)).ToRotationVector2() * (0.5f + (MathF.Sin(6f * i) + 1f) * 8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), velocity);
                dust.noGravity = true;
                dust.color = Main.hslToRgb(Main.rand.NextFloat(0.05f, 0.15f), 1f, 0.66f);
            }
            for (int i = 0; i < ThePrince.FlameSplitCount; i++)
            {
                Vector2 shootVelocity = (MathHelper.TwoPi * i / ThePrince.FlameSplitCount + offsetAngle).ToRotationVector2() * 8f;
                Vector2 flameSpawnPosition = Projectile.Center + shootVelocity;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), flameSpawnPosition, shootVelocity, ModContent.ProjectileType<PrinceFlameSmall>(), damage, kb, Projectile.owner);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.8f);
            lightColor.A /= 4;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (timeLeft > FadeoutTime)
                ExplodeIntoFireballs();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);
        }
    }
}
