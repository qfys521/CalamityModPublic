using CalamityMod.Graphics.Metaballs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SpeedBlasterShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public bool DashShot => Projectile.ai[1] == 3f; // the big shot
        public bool PostDashShot => Projectile.ai[1] == 2f; // the higher velocity post dash shots
        public Color MainColor;
        public bool PostHit = false;
        public int time = 0;

        public static readonly SoundStyle ShotImpact = new("CalamityMod/Sounds/Item/SplatshotImpact") { PitchVariance = 0.3f, Volume = 2.5f };
        public static readonly SoundStyle ShotImpactBig = new("CalamityMod/Sounds/Item/SplatshotBigImpact") { PitchVariance = 0.3f, Volume = 4f };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 150 * Projectile.MaxUpdates;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (PostHit)
            {
                Projectile.velocity = Projectile.velocity * 0.01f;
                Projectile.scale *= 0.95f;
                Projectile.tileCollide = false;
            }
            switch (Projectile.ai[0])
            {
                case 0:
                default:
                    MainColor = Color.Cyan;
                    break;
                case 1:
                    MainColor = Color.Blue;
                    break;
                case 2:
                    MainColor = Color.Magenta;
                    break;
                case 3:
                    MainColor = Color.Lime;
                    break;
                case 4:
                    MainColor = Color.Yellow;
                    break;
            }

            if (Projectile.ai[2] != 1f)
            {
                if (DashShot)
                {
                    Projectile.scale = 2f;
                    Projectile.penetrate = 4;
                    Projectile.extraUpdates = 90;
                    Projectile.timeLeft = 120 * Projectile.extraUpdates;
                    Projectile.velocity *= 0.3f;
                }
                else if (PostDashShot)
                    Projectile.MaxUpdates = 3;

                Projectile.ai[2] = 1f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (!PostHit && time > (PostDashShot ? 35 : 25))
            {
                Projectile.velocity *= (DashShot ? 1f : 0.945f);
                Projectile.velocity.Y += (DashShot ? 0f : 0.6f);
            }
            Color ColorUsed = GetColor(Projectile.ai[0]);

            if (Main.rand.NextBool(20) && !DashShot && !PostHit)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Ghost);
                dust.noLight = true;
                dust.noGravity = false;
                dust.scale = 1.2f;
                dust.velocity = new Vector2(Main.rand.Next(-1, 1), 3);
                dust.color = ColorUsed;
                dust.alpha = 75;
            }
            if (DashShot)
            {
                Player Owner = Main.player[Projectile.owner];
                float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
                if (targetDist < 1400f)
                {
                    //GlowOrbParticle spark = new GlowOrbParticle(Projectile.Center, -Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), false, 15, 1.35f, MainColor, true, false, false);
                    //GeneralParticleHandler.SpawnParticle(spark);
                }
                if (targetDist < 1400f && Main.rand.NextBool())
                {
                    //GlowOrbParticle spark = new GlowOrbParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2f, 2f), -Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), false, 15, 1.35f, MainColor, true, false, false);
                    //GeneralParticleHandler.SpawnParticle(spark);
                }
                if (targetDist < 1400)
                {
                    Particle spark2 = new CustomSpark(Projectile.Center, Projectile.velocity * 0.01f, "CalamityMod/Particles/SmallBloom", false, 11, 0.15f, MainColor, new Vector2(0.7f, 1.1f), false, false, 0, false, false, 0.65f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }

                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Ghost);
                dust.noLight = true;
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(1.3f, 1.5f);
                dust.velocity = new Vector2(Main.rand.Next(-1, 1), Main.rand.Next(0, 8)).RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.05f, 0.3f);
                dust.color = ColorUsed;
                dust.alpha = Main.rand.Next(145, 240);
            }
            if (Projectile.timeLeft == 300 && DashShot)
            {
                for (int i = 0; i <= 10; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Ghost, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(0.4f, 1.2f));
                    dust.noGravity = true;
                    dust.color = ColorUsed;
                    dust.alpha = Main.rand.Next(40, 90);
                    dust.scale = Main.rand.NextFloat(1.2f, 2.3f);
                    dust.noLight = true;
                }
            }
            if (Projectile.timeLeft == 300 && !DashShot)
            {
                for (int i = 0; i <= 7; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Ghost, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(PostDashShot ? 13f : 23f)) * Main.rand.NextFloat(0.4f, 1.2f));
                    dust.noGravity = true;
                    dust.color = ColorUsed;
                    dust.alpha = Main.rand.Next(40, 90);
                    dust.scale = Main.rand.NextFloat(0.7f, 1.6f);
                    dust.noLight = true;
                }
            }
            time++;
        }
        public override bool? CanHitNPC(NPC target) => !PostHit ? null : false;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color ColorUsed = GetColor(Projectile.ai[0]);
            if (DashShot)
            {
                if (Projectile.numHits == 0)
                    SoundEngine.PlaySound(ShotImpactBig, Projectile.position);

                Vector2 BurstFXDirection = new Vector2(5, 0).RotatedByRandom(100);
                for (int i = 0; i < 2; i++)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center, (BurstFXDirection) * (i + 1), false, 7, 1.5f - i * 0.6f, MainColor * 0.8f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int i = 0; i < 2; i++)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center, (-BurstFXDirection) * (i + 1), false, 7, 1.5f - i * 0.6f, MainColor * 0.8f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                Particle orb = new GenericBloom(Projectile.Center, Vector2.Zero, MainColor, 0.6f, 13, false);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new GenericBloom(Projectile.Center, Vector2.Zero, Color.White, 0.5f, 12, false);
                GeneralParticleHandler.SpawnParticle(orb2);
            }
            else
            {
                for (int i = 0; i <= 8; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(7, 7), DustID.Ghost, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(0.05f, 0.45f), 0, default, Main.rand.NextFloat(0.6f, 1.2f));
                    dust.noLight = true;
                    dust.noGravity = false;
                    dust.color = GetColor(Projectile.ai[0]);
                    dust.alpha = 75;
                }
                Projectile.timeLeft = 15;
                PostHit = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 paintPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f) + (Projectile.velocity.SafeNormalize(Vector2.UnitY)).RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(4f, 20f);
                float paintSize = Main.rand.NextFloat(40f, 70f);
                switch (Projectile.ai[0])
                {
                    case 0:
                    default:
                        ModContent.GetInstance<CyanPaint>().SpawnParticle(paintPos, paintSize);
                        break;
                    case 1:
                        ModContent.GetInstance<BluePaint>().SpawnParticle(paintPos, paintSize);
                        break;
                    case 2:
                        ModContent.GetInstance<MagentaPaint>().SpawnParticle(paintPos, paintSize);
                        break;
                    case 3:
                        ModContent.GetInstance<LimePaint>().SpawnParticle(paintPos, paintSize);
                        break;
                    case 4:
                        ModContent.GetInstance<YellowPaint>().SpawnParticle(paintPos, paintSize);
                        break;
                }
            }
            Projectile.timeLeft = 15;
            PostHit = true;
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            if (!DashShot)
                SoundEngine.PlaySound(ShotImpact, Projectile.position);
        }

        public static Color GetColor(float type) => type == 0 ? Color.Aqua : type == 1 ? Color.Blue : type == 2 ? Color.Fuchsia : type == 3 ? Color.Lime : Color.Yellow;

        public override Color? GetAlpha(Color drawColor) => GetColor(Projectile.ai[0]) * drawColor.A * Projectile.Opacity;
        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => (1f - completionRatio) * Projectile.scale * 6f;
        internal Color ColorFunction(float completionRatio, Vector2 vertexPos) => GetColor(Projectile.ai[0]) * Projectile.Opacity;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f), 20);
            return true;
        }
    }
}
