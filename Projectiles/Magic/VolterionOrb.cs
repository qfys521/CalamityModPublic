using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Magic
{
    public class VolterionOrb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";

        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/VolterionOrbShot") { Volume = 0.6f };
        public static readonly SoundStyle ExplosionSound = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeImpact") { Volume = 0.5f };

        // Explosion
        public static float ExplosionTime = 150f; // Time taken to explode
        public static float ExplosionLifetime = 18f;
        public static float MaxScale = 7.5f;

        // Lightning
        public static float AttackRate = 60f;
        public static float AttackRange = 800f;
        public static float LightningDamageMult = 0.25f;

        public ref float OrbType => ref Projectile.ai[0];
        public ref float AttackTimer => ref Projectile.ai[1];
        public ref float ExplosionTimer => ref Projectile.ai[2];

        public Player Owner => Main.player[Projectile.owner];

        public static Asset<Texture2D> Bloom;
        public static Asset<Texture2D> Explosion;
        public override void Load()
        {
            Bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
            Explosion = ModContent.Request<Texture2D>("CalamityMod/Particles/PlasmaExplosion");
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 4 % Main.projFrames[Type];

            Projectile.velocity *= 0.8f;
            Projectile.rotation += 0.01f;
            Lighting.AddLight(Projectile.Center, GetColor(OrbType).ToVector3());

            // Each orb fires at a slightly different timing from the previous
            AttackTimer++;
            float offset = 9f * OrbType;
            if (AttackTimer > AttackRate * 2f)
            {
                // Explode
                if (AttackTimer > ExplosionTime)
                {
                    if (ExplosionTimer == 0f)
                        SoundEngine.PlaySound(ExplosionSound, Projectile.Center);

                    float scaleLevel = PiecewiseAnimation(ExplosionTimer / ExplosionLifetime, new CurveSegment[] { new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4) });
                    Projectile.scale = MathHelper.Lerp(1f, MaxScale, scaleLevel);
                    Projectile.Opacity = MathF.Sin(MathHelper.PiOver2 + MathHelper.PiOver2 * ExplosionTimer / ExplosionLifetime);

                    if (ExplosionTimer++ > ExplosionLifetime)
                        Projectile.Kill();
                }
                // Builds up dust readying up to explode
                else if (AttackTimer > MathHelper.Lerp(AttackRate * 2f, ExplosionTime, 0.8f) || AttackTimer % 3 == 2)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(8f, 10f));
                    Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, velocity);
                    spark.noLight = true;
                    spark.noGravity = Main.rand.NextBool();
                    spark.color = GetColor(OrbType);
                }
            }
            else if (AttackTimer % AttackRate == (AttackRate - offset - 1f))
            {
                // Orbs should be extremely slow by the time they fire already but make sure it's not moving to keep the visuals right
                Projectile.velocity *= 0f;

                NPC target = Projectile.Center.ClosestNPCAt(AttackRange);
                if (target != null)
                {
                    SoundEngine.PlaySound(FireSound, Projectile.Center);

                    Vector2 velocity = Projectile.SafeDirectionTo(target.Center) * 16f;
                    Projectile shot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<VolterionShot>(), (int)(Projectile.damage * LightningDamageMult), Projectile.knockBack * LightningDamageMult, Projectile.owner, OrbType + 1f);
                    shot.tileCollide = false;

                    // Create a lightning bolt-like particle in the direction of the shot and 3 random hue-shifted ones by the side
                    Particle bolt = new CrackParticle(Projectile.Center, velocity * 0.5f, GetColor(OrbType), Vector2.One, 0, 0, Main.rand.NextFloat(0.8f, 1f), 12);
                    GeneralParticleHandler.SpawnParticle(bolt);

                    for (int i = 1; i < 4; i++)
                    {
                        Particle bolt2 = new CrackParticle(Projectile.Center, velocity.RotatedBy(MathHelper.PiOver2 * i + MathHelper.ToRadians(Main.rand.NextFloat(-40f, 40f))) * 0.5f, GetColor(4f - OrbType) * 0.6f, Vector2.One, 0, 0, Main.rand.NextFloat(0.5f, 0.6f), 12);
                        GeneralParticleHandler.SpawnParticle(bolt2);
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Electrified, 120);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Electrified, 120);

        public static Color GetColor(float type) => Color.Lerp(new Color(51, 197, 255), new Color(143, 51, 255), 0.2f + 0.15f * type + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 10f));

        public override Color? GetAlpha(Color lightColor) => GetColor(OrbType);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = Projectile.GetAlpha(lightColor);

            if (ExplosionTimer > 0f)
            {
                Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
                Texture2D explosionTex = Explosion.Value;
                Main.EntitySpriteDraw(explosionTex, drawPos, null, color * Projectile.Opacity, Projectile.rotation, explosionTex.Size() * 0.5f, 0.02f * Projectile.scale, SpriteEffects.None);
                Main.spriteBatch.ExitShaderRegion();
                return false;
            }

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D bloomTex = Bloom.Value;
            Main.EntitySpriteDraw(bloomTex, drawPos, null, color * 0.5f, 0, bloomTex.Size() * 0.5f, 0.42f, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();
            return true;
        }

        public override bool? CanDamage() => ExplosionTimer > 0f ? base.CanDamage() : false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, 20 * Projectile.scale, targetHitbox);
    }
}
