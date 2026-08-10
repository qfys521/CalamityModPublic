using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class FlareBoltProjectile : ModProjectile, ILocalizedModType
    {
        public ref float time => ref Projectile.ai[0];
        public int wallBounces = 0;
        public float fadeIn = 0;
        public bool launch = true;
        public Color bColor = Color.OrangeRed;
        public int launchTime = 0;
        public Vector2 endPoint;
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Lighting.AddLight(Projectile.Center, bColor.ToVector3());
            Vector2 mouse = Owner.Calamity().mouseWorld;
            fadeIn = Utils.GetLerpValue(0, Owner.itemAnimationMax * 0.5f * Projectile.MaxUpdates, time, true);
            float velFade = Utils.GetLerpValue(1.5f, 6.5f, Projectile.velocity.Length(), true);
            Vector2 velocity = Utils.DirectionTo(Owner.Center, mouse) * 8f;
            Projectile.scale = fadeIn * 1.5f;
            if (fadeIn < 1) // Hold the projectile in front of the player while they case the spell
            {
                Projectile.Center = Owner.Center + velocity.SafeNormalize(Vector2.UnitX) * 48;
            }
            else
            {
                if (launch) // On launch, spawns some effects and launch the projectile at the mouse
                {
                    Projectile.tileCollide = true;

                    Vector2 staticSpeed = Utils.DirectionTo(Owner.Center, mouse) * Utils.Distance(Projectile.Center, Owner.ClampedMouseWorld()) * 0.0165f;
                    Projectile.velocity = staticSpeed;
                    endPoint = Owner.ClampedMouseWorld();

                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/HalleysInfernoShoot") with { Volume = 0.45f, Pitch = 0.3f }, Projectile.Center);
                    for (int i = 0; i < 20; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1.5f, 3.5f));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.85f, 1.4f);
                        dust.color = bColor;
                        dust.noLightEmittance = true;
                        if (i % 3 == 0)
                        {
                            float variance = Main.rand.NextFloat(-0.6f, 0.6f);
                            float fxScale = Main.rand.NextFloat(1.5f, 1.7f) - Math.Abs(variance);
                            Vector2 fxVelocity = Projectile.velocity.RotatedBy(variance) * Main.rand.NextFloat(0.6f, 1f) * (1 - Math.Abs(variance));

                            Particle fx = new CustomSpark(Projectile.Center, fxVelocity * 1.3f, "CalamityMod/Particles/FireTypeParticle", false, 22, fxScale, Color.Lerp(bColor, Color.Red, 0.5f), new Vector2(1.8f, 1f), true, false, shrinkSpeed: 0.2f);
                            GeneralParticleHandler.SpawnParticle(fx);
                        }
                    }
                    launch = false;
                }

                if (Main.rand.NextBool(8) && launchTime < 56)
                {
                    Particle fx = new CustomSpark(Projectile.Center + Main.rand.NextVector2Circular(8, 8), -Projectile.velocity * 0.9f, "CalamityMod/Particles/FireTypeParticle", false, 32, 1.15f, Color.Lerp(bColor, Color.Red, 0.5f), new Vector2(0.8f, 1f), true, false);
                    GeneralParticleHandler.SpawnParticle(fx);
                }
                if (Main.rand.NextBool(6))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<LightDust>(), -Projectile.velocity * 0.5f);
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.85f, 1.4f);
                    dust.color = bColor;
                    dust.noLightEmittance = true;
                }

                Particle trail = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.3f, "CalamityMod/Particles/BloomCircle", false, 11, 0.3f, bColor * 0.9f, new Vector2(1, 1f), true, false, shrinkSpeed: 0.7f * velFade);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            Projectile.rotation += 0.3f * Projectile.direction;
            time++;
            if (!launch)
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, endPoint, 0.035f);
                Projectile.velocity *= 0.9f;
                launchTime++;
                if (launchTime >= (Owner.itemAnimationMax + 28))
                    Projectile.Kill();
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(BuffID.OnFire, 90);
            float minMult = 0.1f;
            int hitsToMinMult = 5;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FireImplosion>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Projectile.DrawProjectileWithBackglow(bColor with { A = 0 }, Color.White, 2f * fadeIn);
            return false;
        }
    }
}
