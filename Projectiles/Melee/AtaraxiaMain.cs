using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class AtaraxiaMain : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        private static int NumAnimationFrames = 5;
        private static int AnimationFrameTime = 9;
        public int time = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = NumAnimationFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 5;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            DrawOffsetX = -40;
            DrawOriginOffsetY = -3;
            DrawOriginOffsetX = 18;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Light
            Lighting.AddLight(Projectile.Center, 0.45f, 0.1f, 0.1f);

            if (time > 8f && targetDist < 1400f)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 1.5f, -Projectile.velocity * Main.rand.NextFloat(0.3f, 1.5f), false, 8, 0.8f, Color.Lerp(Color.DarkOrchid, Color.IndianRed, Main.rand.NextFloat(0, 1)) * 0.7f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (time > 8f && targetDist < 1400f)
            {
                Particle spark = new SparkParticle(Projectile.Center + Projectile.velocity * 2f, Projectile.velocity, false, 2, 1.9f, Color.Lerp(Color.DarkOrchid, Color.IndianRed, Main.rand.NextFloat(0, 1)) * 0.85f);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (time > 8 && Main.rand.NextBool())
            {
                Vector2 dustvel = -Projectile.velocity;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, dustvel * Main.rand.NextFloat(0.1f, 1.2f), 0, default, Main.rand.NextFloat(0.7f, 0.9f));
                dust.noGravity = true;
                dust.color = Color.Lerp(Color.DarkOrchid, Color.IndianRed, Main.rand.NextFloat(0, 1));
            }

            // Update animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > AnimationFrameTime)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= NumAnimationFrames)
                Projectile.frame = 0;

            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 180);
        }

        // Explodes like Exoblade's Exobeams
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath55, Projectile.Center);

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AtaraxiaBoom>(), Projectile.damage / 2, 0, Projectile.owner, 1f, 0f, 0f);
            }
            for (int k = 0; k < 10; k++)
            {
                Vector2 velocity = new Vector2(20, 20).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.2f);
                float colorRando = Main.rand.NextFloat(0, 1);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + velocity, DustID.FireworksRGB, velocity * Main.rand.NextFloat(0.2f, 1f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.3f, 0.65f);
                dust.color = Color.Lerp(Color.DarkOrchid, Color.IndianRed, colorRando);
                dust.noLight = true;
                dust.noLightEmittance = true;
            }
            for (int k = 0; k < 10; k++)
            {
                Vector2 velocity = new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.2f);
                float colorRando = Main.rand.NextFloat(0, 1);
                Particle spark = new GlowSparkParticle(Projectile.Center + velocity, velocity, false, 11, Main.rand.NextFloat(0.015f, 0.025f), Color.Lerp(Color.DarkOrchid, Color.IndianRed, colorRando), new Vector2(2.2f, 0.9f), true);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            for (float k = 0; k < 3; k++)
            {
                float colorRando = Main.rand.NextFloat(0, 1);
                int partLifetime = Main.rand.Next(13, 15 + 1);
                float scale = Main.rand.NextFloat(0.12f, 0.18f);
                Vector2 spawnPos = Projectile.Center + (Main.rand.NextVector2Circular(20, 20) * (k + 1));
                Particle blastRing = new CustomPulse(spawnPos, Vector2.Zero, Color.Lerp(Color.DarkOrchid, Color.IndianRed, colorRando) * 0.6f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.07f, scale, partLifetime);
                GeneralParticleHandler.SpawnParticle(blastRing);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 30, targetHitbox);
    }
}
