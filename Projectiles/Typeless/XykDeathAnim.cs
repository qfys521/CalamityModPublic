using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class XykDeathAnim : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public ref float time => ref Projectile.ai[0];
        public int endTime = 25;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 32;
            Projectile.scale = 0.55f; // Normally we don't put scale here, but it doesn't matter this time
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            bool Orange = Owner.Calamity().XykVisualsOrange;
            Color effectColor = Owner.Calamity().XykFXColor;

            if (time == 0)
            {
                SoundStyle die = new("CalamityMod/Sounds/Item/XykDie");
                SoundEngine.PlaySound(die with { Volume = 1f }, Projectile.Center);
            }
            if (time < endTime)
            {
                float fade = Utils.GetLerpValue(35, 0, time, true);
                float numberOfDusts = 3f;
                float rotFactor = 360f / numberOfDusts;
                for (int i = 0; i < numberOfDusts; i++)
                {
                    float rot = MathHelper.ToRadians(i * rotFactor);
                    Vector2 velOffset = CalamityUtils.RandomVelocity(100f, 70f, 250f, 0.04f);
                    velOffset *= Main.rand.NextFloat(15, 30) * fade;
                    Particle energy = new SparkParticle(Projectile.Center + velOffset * 2.5f, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, false, 14, Main.rand.NextFloat(1.1f, 1.25f) - 0.2f * fade, effectColor);
                    GeneralParticleHandler.SpawnParticle(energy);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + velOffset * 2.5f, DustID.FireworksRGB, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                    dust.noGravity = true;
                    dust.color = effectColor;
                }
            }
            if (time >= endTime)
            {
                Projectile.scale *= 1.15f;
                for (int i = 0; i < 3; i++)
                {
                    Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, effectColor, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.7f * (i + 1) * Projectile.scale, 1f * Projectile.scale, 18, true);
                    GeneralParticleHandler.SpawnParticle(blastRing);
                    Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.35f * (i + 1) * Projectile.scale, 0.5f * Projectile.scale, 18, true);
                    GeneralParticleHandler.SpawnParticle(blastRing2);
                }

                for (int i = 0; i < 6; i++)
                {
                    Particle spark = new GlowSparkParticle(Projectile.Center, new Vector2(Orange ? 0 : 1, Orange ? 1 : 0) * -5 * (i % 2 == 0 ? -1 : 1), false, 15, (0.08f - i * 0.01f) * Projectile.scale, effectColor, new Vector2(5, 0.8f), true, false, 1.2f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                if (time == endTime)
                {
                    if (Orange)
                    {
                        Particle spark1 = new CustomPulse(Projectile.Center, Vector2.Zero, effectColor, "CalamityMod/Particles/GlowSquareParticleBig", Vector2.One, MathHelper.PiOver4, 0, 2.2f, 22);
                        GeneralParticleHandler.SpawnParticle(spark1);

                        Particle spark2 = new CustomPulse(Projectile.Center, Vector2.Zero, effectColor, "CalamityMod/Particles/GlowSquareParticleBig", Vector2.One, MathHelper.PiOver4, 0, 1.2f, 47);
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                    else
                    {
                        Particle spark1 = new CustomPulse(Projectile.Center, Vector2.Zero, effectColor, "CalamityMod/Particles/HighResHollowCircleHardEdge", Vector2.One, MathHelper.PiOver4, 0, 0.3f, 22);
                        GeneralParticleHandler.SpawnParticle(spark1);
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Orange ? 267 : 278, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(2.5f, 15));
                        dust2.scale = Main.rand.NextFloat(0.85f, 1.15f) * (Orange ? 1 : 1.2f);
                        dust2.noGravity = Orange ? true : false;
                        dust2.color = Color.Lerp(Color.White, effectColor, 0.5f);

                        if (Orange)
                        {
                            Particle squarker = new CustomPulse(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5.5f, 20), effectColor, "CalamityMod/Particles/GlowSquareParticle", Vector2.One, MathHelper.PiOver4, Main.rand.NextFloat(1.3f, 3.8f), 0.2f, 38);
                            GeneralParticleHandler.SpawnParticle(squarker);
                        }
                        else
                        {
                            Particle sparker = new CustomSpark(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5.5f, 20), "CalamityMod/Particles/Sparkle", false, 38, Main.rand.NextFloat(2.2f, 4.8f), effectColor, new Vector2(0.4f, Main.rand.NextFloat(0.9f, 1.4f)), true, true, 0, false, false, 0.1f);
                            GeneralParticleHandler.SpawnParticle(sparker);
                        }
                    }
                }
            }
            time++;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => Projectile.damage > 0 ? null : false;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }
    }
}
