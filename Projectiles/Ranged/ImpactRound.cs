using CalamityMod.Particles;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ImpactRound : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/AMRShot";

        private bool initialized = false;
        public static int Lifetime = 600;
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.light = 0.5f;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 7;
            Projectile.scale = 1.18f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.BulletHighVelocity;
            Projectile.penetrate = 5;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (!initialized && Projectile.CountsAsClass<RangedDamageClass>()) //Ranged check prevents quiver splits triggering the sound
            {
                initialized = true;

                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(CommonCalamitySounds.LargeWeaponFireSound with { Volume = CommonCalamitySounds.LargeWeaponFireSound.Volume * 0.45f }, Projectile.Center);
                }
            }
            if (Projectile.timeLeft == Lifetime - 4)
            {
                for (int i = 0; i <= 4; i++) //Dragon's Breath shot particles my beloved
                {
                    Vector2 sparkVelocity = Projectile.velocity * 0.5f;

                    float sparkScale1 = Main.rand.NextFloat(0.3f, 0.8f);
                    Vector2 sparkvelocity1 = sparkVelocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.4f, 0.95f);
                    SparkParticle spark1 = new SparkParticle(Projectile.Center, sparkvelocity1, false, 6, sparkScale1, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark1);

                    float sparkScale2 = Main.rand.NextFloat(0.4f, 1f);
                    Vector2 sparkvelocity2 = sparkVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(1.1f, 3.1f);
                    SparkParticle spark2 = new SparkParticle(Projectile.Center, sparkvelocity2, false, 6, sparkScale2, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
            if (Projectile.timeLeft < Lifetime - 3 && Projectile.timeLeft > Lifetime - 150)
            {
                AltSparkParticle spark = new AltSparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 15, 1f, Color.OrangeRed * 0.1f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.Center, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Red, Color.OrangeRed, Utils.GetLerpValue(0, 3, 1, true)), "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.05f, 10);
            GeneralParticleHandler.SpawnParticle(explosion);
            for (int i = 0; i <= 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 90 : 183, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.5f), 0, default, Main.rand.NextFloat(1.6f, 2.2f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage += 0.25f;
            for (int i = 0; i <= 2; i++)
            {
                LineParticle spark = new LineParticle(Projectile.Center, -Projectile.velocity.RotatedBy(Main.rand.NextFloat(0.18f, 0.44f)) * Main.rand.NextFloat(0.4f, 2.5f), false, 8, 0.9f, Main.rand.NextBool() ? Color.Red : Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(spark);
                LineParticle spark2 = new LineParticle(Projectile.Center, -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.18f, -0.44f)) * Main.rand.NextFloat(0.4f, 2.5f), false, 8, 0.9f, Main.rand.NextBool() ? Color.Red : Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(spark2);
            }

            for (int i = 0; i <= 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 90 : 183, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.5f), 0, default, Main.rand.NextFloat(1.6f, 2.2f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => Projectile.timeLeft < 600;
    }
}
