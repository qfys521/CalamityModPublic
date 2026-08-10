using System;
using System.IO;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Melee
{
    public class ArkoftheCosmosBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        private bool initialized = false;
        public ref float Charge => ref Projectile.ai[0];

        const int maxStitches = 10;
        public int CurrentStitches => (int)Math.Ceiling((1 - (float)Math.Sqrt(1f - (float)Math.Pow(MathHelper.Clamp(StitchProgress * 3f, 0f, 1f), 2f))) * maxStitches);
        public float[] StitchRotations = new float[maxStitches];
        public float[] StitchLifetimes = new float[maxStitches];

        const float MaxTime = 70;
        const float SnapTime = 25f;
        const float HoldTime = 15f;

        public float SnapTimer => MaxTime - Projectile.timeLeft;
        public float HoldTimer => MaxTime - Projectile.timeLeft - SnapTime;
        public float StitchTimer => MaxTime - Projectile.timeLeft - SnapTime - (HoldTime / 2f);

        public float SnapProgress => MathHelper.Clamp(SnapTimer / SnapTime, 0, 1);
        public float HoldProgress => MathHelper.Clamp(HoldTimer / HoldTime, 0, 1);
        public float StitchProgress => MathHelper.Clamp(StitchTimer / (MaxTime - (SnapTime + (HoldTime / 2f))), 0, 1);

        public int CurrentAnimation => (MaxTime - Projectile.timeLeft) <= SnapTime ? 0 : (MaxTime - Projectile.timeLeft) <= SnapTime + HoldTime ? 1 : 2;

        public Vector2 scissorPosition => Projectile.Center + ThrustDisplaceRatio() * Projectile.velocity * 200f;

        public Player Owner => Main.player[Projectile.owner];

        public Particle PolarStar;


        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.width = Projectile.height = 300;
            Projectile.width = Projectile.height = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanDamage()
        {
            return HoldProgress > 0;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (HoldProgress == 0)
                return false;

            //The hitbox is simplified into a line collision.
            float collisionPoint = 0f;
            float bladeLength = ThrustDisplaceRatio() * 242f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (Projectile.velocity * bladeLength), 30, ref collisionPoint);
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            if (!initialized) //Initialization
            {
                Projectile.timeLeft = (int)MaxTime;
                var sound = SoundEngine.PlaySound(SoundID.Item84 with { Volume = SoundID.Item84.Volume * 0.3f }, Projectile.Center);

                Projectile.velocity.Normalize();
                Projectile.rotation = Projectile.velocity.ToRotation();

                initialized = true;
                Projectile.ForceNetUpdate();
            }

            //Manage position and rotation
            Projectile.scale = 1.4f;

            HandleParticles();

            if (StitchProgress == 0)
            {

                var p = BigRipMetaball.SpawnParticle(Projectile.Center + Projectile.velocity * MathHelper.Lerp(0f, ThrustDisplaceRatio() * 242f, 0.5f), Vector2.Zero, 242f);
                p.SizeScaling = 0f;
                p.TextureToUse = TextureAssets.Projectile[Type].Value;
                p.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                p.Scale = new Vector2(0.25f * ThrustDisplaceRatio(), 1f * ThrustDisplaceRatio());
            }
            if (SnapTimer == 5)
                SoundEngine.PlaySound(CommonCalamitySounds.MeatySlashSound with { Pitch = -0.5f, Volume = 0.5f, MaxInstances = 2}, Projectile.Center);
            //Spawn particles when the line appears
            if (HoldTimer == 1)
            {

                var p = BigRipMetaball.SpawnParticle(Projectile.Center + Projectile.velocity * MathHelper.Lerp(0f, ThrustDisplaceRatio() * 242f, 0.5f), Vector2.Zero, 242f);
                p.SizeScaling = 0.925f;
                p.TextureToUse = TextureAssets.Projectile[Type].Value;
                p.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                p.Scale = new Vector2(0.25f * ThrustDisplaceRatio(), 1f * ThrustDisplaceRatio());

                // Feel the power
                SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with {Pitch = 0f, MaxInstances = 2}, Projectile.Center);
                Main.LocalPlayer.SetScreenshake(7.5f);

                // Feel the particles
                for (int i = 0; i < 20; i++)
                {
                    float positionAlongLine = MathHelper.Lerp(0f, ThrustDisplaceRatio() * 242f, Main.rand.NextFloat(0f, 1f));
                    Vector2 particlePosition = Projectile.Center + Projectile.velocity * positionAlongLine;
                    Color particleColor = Main.rand.NextBool() ? Color.OrangeRed : Main.rand.NextBool() ? Color.White : Color.Orange;
                    float particleScale = Main.rand.NextFloat(0.05f, 0.4f) * (0.4f + 0.6f * (float)Math.Sin(positionAlongLine / (ThrustDisplaceRatio() * 242f) * MathHelper.Pi));

                    int particleType = Main.rand.Next(3);
                    Particle particle;

                    switch (particleType)
                    {
                        case 0:
                            particle = new StrongBloom(particlePosition, Vector2.UnitY * Main.rand.NextFloat(-4f, -1f), particleColor, particleScale, Main.rand.Next(20) + 10);
                            GeneralParticleHandler.SpawnParticle(particle);
                            break;
                        case 1:
                            particle = new GenericBloom(particlePosition, Vector2.UnitY * Main.rand.NextFloat(-4f, -1f), particleColor, particleScale, Main.rand.Next(20) + 10);
                            GeneralParticleHandler.SpawnParticle(particle);
                            break;
                        case 2:
                            particle = new CritSpark(particlePosition, Vector2.UnitY * Main.rand.NextFloat(-10f, -1f), Color.White, particleColor, particleScale * 7f, Main.rand.Next(20) + 10, 0.1f, 3);
                            GeneralParticleHandler.SpawnParticle(particle);
                            break;
                    }
                }

                // Feel the extra stars
                if (Owner.whoAmI == Main.myPlayer)
                {
                    int starAmt = 5;
                    for (int s = 1; s <= starAmt; s++)
                    {
                        float lerpRatio = s / (float)starAmt;
                        float positionAlongLine = MathHelper.Lerp(0f, ThrustDisplaceRatio() * 242f, lerpRatio);
                        Vector2 starPosition = Projectile.Center + Projectile.velocity * positionAlongLine;
                        Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), starPosition, Main.rand.NextVector2CircularEdge(28, 28), ProjectileType<EonBolt>(), (int)(ArkoftheCosmos.BlastBoltsDamageMultiplier * Projectile.damage), 0f, Owner.whoAmI, 0.55f, MathHelper.Pi * 0.07f);
                        blast.timeLeft = 100;
                    }
                }
            }
        }

        public void HandleParticles()
        {
            if (PolarStar == null)
            {
                PolarStar = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.White, Color.CornflowerBlue, Projectile.scale * 2f, 2, 0.1f, 5f, true);
                GeneralParticleHandler.SpawnParticle(PolarStar);
            }
            else if (HoldProgress <= 0.4f)
            {
                PolarStar.Time = 0;
                PolarStar.Position = scissorPosition + Projectile.velocity * SnapProgress * 150;
                PolarStar.Scale = Projectile.scale * 2f;
            }

            //Update stitches
            for (int i = 0; i < CurrentStitches; i++)
            {
                if (StitchRotations[i] == 0)
                {
                    StitchRotations[i] = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + MathHelper.PiOver2;

                    SoundStyle sewSound = i % 3 == 0 ? SoundID.Item63 : i % 3 == 1 ? SoundID.Item64 : SoundID.Item65;
                    SoundEngine.PlaySound(sewSound with { Volume = sewSound.Volume * 0.5f }, Owner.Center);

                    float positionAlongLine = (ThrustDisplaceRatio() * 242f / (float)maxStitches * 0.5f) + MathHelper.Lerp(0f, ThrustDisplaceRatio() * 242f, i / (float)maxStitches);
                    Vector2 stitchCenter = Projectile.Center + Projectile.velocity * positionAlongLine;


                    Particle spark = new CritSpark(stitchCenter, Vector2.Zero, Color.White, Color.Cyan, 3f, 8, 0.1f, 3);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                StitchLifetimes[i]++;
            }

            if (StitchProgress > 0)
            {
                for (int m = 0; m < 2; m++)
                {
                    float positionAlongLine = MathHelper.Lerp(0f, ThrustDisplaceRatio() * 242f, Main.rand.NextFloat());
                    Vector2 smokePosition = Projectile.Center + Projectile.velocity * positionAlongLine;
                    HeavySmokeParticle smoke = new(smokePosition, Main.rand.NextVector2CircularEdge(4f, 4f), new Color(117, 36, 32), 12, 0.8f, 0.6f, 0f, true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color pulseColor = Main.rand.NextBool() ? (Main.rand.NextBool() ? Color.Orange : Color.Coral) : (Main.rand.NextBool() ? Color.OrangeRed : Color.Gold);
            Particle pulse = new PulseRing(target.Center, Vector2.Zero, pulseColor, 0.05f, 0.2f + Main.rand.NextFloat(0f, 1f), 30);
            GeneralParticleHandler.SpawnParticle(pulse);

            for (int i = 0; i < 10; i++)
            {
                Vector2 particleSpeed = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.8f) * Main.rand.NextFloat(2.6f, 4f);
                Particle energyLeak = new SquishyLightParticle(target.Center, particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), Color.Red, 60, 1, 1.5f, hueShift: 0.002f);
                GeneralParticleHandler.SpawnParticle(energyLeak);
            }
        }

        //Animation keys
        public CurveSegment anticipation = new CurveSegment(EasingType.SineBump, 0f, 0.2f, -0.1f);
        public CurveSegment thrust = new CurveSegment(EasingType.PolyOut, 0.3f, 0.2f, 3f, 3);
        internal float ThrustDisplaceRatio() => PiecewiseAnimation(SnapProgress, new CurveSegment[] { anticipation, thrust });


        public CurveSegment openMore = new CurveSegment(EasingType.SineBump, 0f, 0f, -0.15f);
        public CurveSegment close = new CurveSegment(EasingType.PolyIn, 0.35f, 0f, 1f, 4);
        public CurveSegment stayClosed = new CurveSegment(EasingType.Linear, 0.5f, 1f, 0f);
        internal float RotationRatio() => PiecewiseAnimation(SnapProgress, new CurveSegment[] { openMore, close, stayClosed });

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            //Draw the scissors
            if (HoldProgress <= 0.4f)
            {
                Texture2D frontBlade = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsLeft").Value;
                Texture2D backBlade = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsRight").Value;

                float snippingRotation = Projectile.rotation + MathHelper.PiOver4;
                float drawRotation = MathHelper.Lerp(snippingRotation - MathHelper.PiOver4, snippingRotation, RotationRatio());
                float drawRotationBack = MathHelper.Lerp(snippingRotation + MathHelper.PiOver4, snippingRotation, RotationRatio());

                Vector2 drawOrigin = new Vector2(33, 86); //Right on the hole
                Vector2 drawOriginBack = new Vector2(44f, 86); //Right on the hole
                Vector2 drawPosition = scissorPosition - Main.screenPosition;

                float opacity = (0.4f - HoldProgress) / 0.4f;
                Color drawColor = Color.Tomato * opacity * 0.9f;
                Color drawColorBack = Color.DeepSkyBlue * opacity * 0.9f;

                Main.EntitySpriteDraw(backBlade, drawPosition, null, drawColorBack, drawRotationBack, drawOriginBack, Projectile.scale, 0f, 0);
                Main.EntitySpriteDraw(frontBlade, drawPosition, null, drawColor * opacity, drawRotation, drawOrigin, Projectile.scale, 0f, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initialized);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initialized = reader.ReadBoolean();
        }
    }
}
