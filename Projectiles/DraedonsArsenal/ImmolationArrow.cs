using System;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ImmolationArrow : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];

        public NPC chosenTarget;
        public bool stuckInTarget = false;
        public bool stuckInGround = false;
        public bool canDamage = true;
        public bool canStick = true;
        public int stuckTimer = 90;
        public Vector2 placementCenter;
        float placementDistance;
        Vector2 placementVelocity;
        public Vector2 storedVelocity;
        public bool collideWithTiles = true;
        public Vector2 startingVel;

        NPC closestTarget;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 4;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (closestTarget != null && closestTarget.life <= 0)
                closestTarget = null;

            if (time == 0)
            {
                startingVel = Projectile.velocity;
                closestTarget = (Projectile.Center + Projectile.velocity * 2).ClosestNPCAt(150);
            }
            else
            {
                NPC attemptGetTarget = (Projectile.Center + Projectile.velocity * 2).ClosestNPCAt(150);
                    if (attemptGetTarget != null)
                    closestTarget = attemptGetTarget;
            }

            if (stuckInGround)
            {
                Projectile.extraUpdates = 2;
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.PiOver2;
                stuckTimer--;

                if (stuckTimer <= 0)
                {
                    Projectile.Kill();
                }
            }
            if (!stuckInTarget && !stuckInGround)
            {
                storedVelocity = Projectile.velocity;
                Projectile.rotation = (storedVelocity).ToRotation() + MathHelper.PiOver2;
                if (time > 5 && !stuckInTarget)
                {
                    if (Main.rand.NextBool(3) && canStick)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalPlasmaDust, -Projectile.velocity);
                        dust.scale = Main.rand.NextFloat(0.6f, 1.4f);
                        dust.velocity = -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.7f);
                        dust.noGravity = true;
                        dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                    }
                    
                    if (targetDist < 1400f)
                    {
                        Particle spark = new SparkParticle(Projectile.Center, -Projectile.velocity, false, 13, 1.3f, Effects.ArsenalEffects.ArsenalPlasmaColor * 0.7f);
                        GeneralParticleHandler.SpawnParticle(spark);

                        if (Main.rand.NextBool(6))
                        {
                            Vector2 placement = Projectile.Center + Main.rand.NextVector2Circular(12, 12);
                            float speed = Main.rand.NextFloat(0.2f, 0.7f);
                            Particle spark2 = new GlowOrbParticle(placement, -Projectile.velocity * speed, false, 7, Main.rand.NextFloat(0.4f, 0.7f), Effects.ArsenalEffects.ArsenalPlasmaColor);
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                    }
                }
            }
            else if (stuckInTarget)
            {
                Projectile.extraUpdates = 2;
                Projectile.rotation = (storedVelocity).SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver2;

                placementCenter = chosenTarget.Center + placementVelocity * placementDistance;

                Projectile.Center = placementCenter;

                stuckTimer--;

                if (chosenTarget.life <= 0)
                    stuckTimer = 0;

                if (stuckTimer <= 0)
                {
                    Projectile.Kill();
                }
            }
            if (stuckInTarget || stuckInGround)
            {
                if (Main.rand.NextBool(8))
                { 
                    float speed = Main.rand.NextFloat(0.2f, 1.5f);
                    Particle spark = new SparkParticle(Projectile.Center, -storedVelocity * speed, false, 23, 0.7f * speed, Effects.ArsenalEffects.ArsenalPlasmaColor * 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), -Projectile.velocity);
                    dust.scale = Main.rand.NextFloat(0.6f, 1.4f);
                    dust.velocity = (new Vector2(35, 35).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.7f)) * Utils.GetLerpValue(90, 0, stuckTimer);
                    dust.noGravity = true;
                    dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                }
            }
            time++;

            if (collideWithTiles && Collision.SolidCollision(Projectile.Center, 4, 4)) // Vanilla tile collision messes with velocity so we use this instead
            {
                canDamage = false;
                stuckInGround = true;

                storedVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                SoundStyle sound = new("CalamityMod/Sounds/NPCHit/RavagerRockPillarHit1");
                SoundEngine.PlaySound(sound with { Volume = 0.25f, Pitch = Main.rand.NextFloat(-0.3f, -0.6f) }, Projectile.Center);
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.PiOver2;
                collideWithTiles = false;
                SoundStyle sound2 = new("CalamityMod/Sounds/Item/ImmolatorPreExplode");
                SoundEngine.PlaySound(sound2 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
                SoundEngine.PlaySound(HolofibreImmolator.PlasmaSound with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.3f, -0.4f) }, Projectile.Center);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.88f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;

            if (!stuckInTarget && canStick)
            {
                if (Projectile.timeLeft < 600)
                    Projectile.timeLeft = 600;
                collideWithTiles = false;
                canDamage = false;
                placementDistance = -Vector2.Distance(target.Center, Projectile.Center);
                placementVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                placementCenter = placementVelocity * (placementDistance * 0.01f);
                chosenTarget = target;
                stuckInTarget = true;
                storedVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                for (int i = 0; i < 12; i++)
                {
                    int dustStyle = Effects.ArsenalEffects.ArsenalPlasmaDust;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + storedVelocity.SafeNormalize(Vector2.UnitX) * 38 + Main.rand.NextVector2Circular(12, 12), dustStyle, Projectile.velocity);
                    dust.scale = Main.rand.NextFloat(0.7f, 1.3f);
                    dust.velocity = storedVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.4f, 1.5f);
                    dust.noGravity = false;
                    dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                    dust.fadeIn = 1.5f;
                }
                SoundStyle sound = new("CalamityMod/Sounds/Item/ImmolatorPreExplode");
                SoundEngine.PlaySound(sound with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
                SoundEngine.PlaySound(HolofibreImmolator.PlasmaSound with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.3f, -0.4f) }, Projectile.Center);
            }
        }
        public override void OnKill(int timeLeft)
        {
            float bonus = (stuckInGround ? 3f : 1.5f);
            float explosionDamage = (stuckInGround ? 2.3f : 0.5f);
            Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ImmolationBurst>(), (int)(Projectile.damage * explosionDamage), Projectile.knockBack * 2, Projectile.owner, 0, stuckInGround ? 1 : 0);
            blast.scale = (stuckInGround ? 2 : 1);
            Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, Effects.ArsenalEffects.ArsenalPlasmaColor * 0.75f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 1.2f * bonus, 26);
            GeneralParticleHandler.SpawnParticle(bolt2);

            Particle bolt3 = new CustomPulse(Projectile.Center, Vector2.Zero, Effects.ArsenalEffects.ArsenalPlasmaColor * 0.75f, "CalamityMod/Particles/WaterFoam", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.77f * bonus, 16);
            GeneralParticleHandler.SpawnParticle(bolt3);

            for (int i = 0; i < (int)(15 * bonus); i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, stuckInGround ? ModContent.DustType<SquashDust>() : Effects.ArsenalEffects.ArsenalPlasmaDust, -Projectile.velocity);
                dust.scale = Main.rand.NextFloat(0.9f, 1.8f);
                dust.velocity = (new Vector2(35, 35).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.7f)) * Utils.GetLerpValue(90, 0, stuckTimer);
                dust.noGravity = false;
                dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                if (dust.type == Effects.ArsenalEffects.ArsenalPlasmaDust)
                    dust.fadeIn = 2f;
            }

            SoundStyle sound = new("CalamityMod/Sounds/Item/PlasmaBig");
            SoundEngine.PlaySound(sound with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
        }
        public override bool? CanDamage() => canDamage ? null : false;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 1)
                return false;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/ImmolationArrow");
            Asset<Texture2D> bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            float randSize = Main.rand.NextFloat(0.9f, 1f);
            float fadeIn = (float)Math.Pow(Utils.GetLerpValue(90, 5, stuckTimer, true), 3);
            for (int i = 0; i < 6; i++)
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition - (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 6 * i, null, Color.Lerp(Color.White, Effects.ArsenalEffects.ArsenalPlasmaColor, i * 0.35f) with { A = 0 } * 0.5f, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.7f - 0.15f * i, 0.7f + 0.15f * i) * randSize * (0.6f + 0.25f * i) * 2, SpriteEffects.None);

            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 3, null, Color.Lerp(Color.White, Effects.ArsenalEffects.ArsenalPlasmaColor, 0.5f * i) with { A = 0 } * fadeIn * 0.3f, Projectile.rotation, bloom.Size() * 0.5f, new Vector2(1f - 0.2f * i, 1f + 0.35f * i) * randSize * (0.5f + 0.15f * i), SpriteEffects.None);

            Vector2 scale2 = 1.1f * new Vector2(0.5f, 1) * 1.5f * randSize;
            return false;
        }
    }
}
