using System.IO;
using CalamityMod.Effects;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using CalamityMod.Skies;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class DoGTeleportRift : ModProjectile, ILocalizedModType
    {
        public static readonly SoundStyle CrackSound = new("CalamityMod/Sounds/NPCHit/CryogenHit", 3);

        public static readonly SoundStyle BreakSound = new("CalamityMod/Sounds/NPCHit/CryogenPhaseTransitionCrack");

        // GFB exclusive; DoG can spawn fake rifts on that seed to confuse the player.
        public bool FakeRift;

        public int RiftLifetime;

        public NPC DoGHead => Main.npc[(int)Projectile.ai[0]];

        public ref float Timer => ref Projectile.ai[1];

        public ref float CrackScale => ref Projectile.ai[2];

        public ref float AIState => ref Projectile.localAI[0];

        public ref float CrackExposure => ref Projectile.localAI[1];

        public ref float MaxExposure => ref Projectile.localAI[2];

        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.Opacity = 0f;
            Projectile.scale = 0f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = RiftLifetime;
        }

        public override bool? CanDamage() => false;

        public override bool ShouldUpdatePosition() => false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(FakeRift);
            writer.Write(RiftLifetime);
            for (int i = 0; i < Projectile.localAI.Length; i++)
                writer.Write(Projectile.localAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            FakeRift = reader.ReadBoolean();
            RiftLifetime = reader.ReadInt32();
            for (int i = 0; i < Projectile.localAI.Length; i++)
                Projectile.localAI[i] = reader.ReadSingle();
        }

        public override void AI()
        {
            if (DoGHead is null || !DoGHead.active)
            {
                Projectile.Kill();
                return;
            }

            // Pre-explosion, charge-up.
            if (AIState == 0f)
            {
                int riftCrackInterval = (int)(RiftLifetime / 3f);
                if (Timer % riftCrackInterval == 0f)
                {
                    // Increase the intensity of certain effects as the crack gets bigger.
                    float effectsLifetimeInterpolant = MathHelper.Lerp(1f, 2f, Timer / RiftLifetime);

                    // Only let the real rift on GFB perform these additional effects.
                    if (!FakeRift)
                    {
                        float crackPitch = MathHelper.Lerp(-0.75f, 0f, Timer / RiftLifetime);
                        SoundEngine.PlaySound(CrackSound with { Pitch = crackPitch }, Projectile.Center);

                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 sparkVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(8f, 12f) * effectsLifetimeInterpolant * 0.7f;
                            float sparkScale = Main.rand.NextFloat(1.2f, 1.6f) * effectsLifetimeInterpolant * 0.62f;
                            Color sparkColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, DoGSky.DoGLightBlue, DoGSky.DoGTwlight), Color.White, 0.65f);
                            SparkParticle crackSpark = new(Projectile.Center, sparkVelocity, false, Main.rand.Next(30, 45), sparkScale, sparkColor);
                            GeneralParticleHandler.SpawnParticle(crackSpark);
                        }

                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 lightVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(12f, 14f) * effectsLifetimeInterpolant * 0.5f;
                            float lightScale = Main.rand.NextFloat(1.2f, 1.6f) * effectsLifetimeInterpolant * 0.62f;
                            Color lightColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, DoGSky.DoGLightBlue, DoGSky.DoGTwlight), Color.White, 0.65f);
                            SquishyLightParticle crackLight = new(Projectile.Center, lightVelocity, lightScale, lightColor, Main.rand.Next(30, 45));
                            GeneralParticleHandler.SpawnParticle(crackLight);
                        }

                        CalamityUtils.AddScreenshakeAt(Projectile.Center, 6f * effectsLifetimeInterpolant * 0.8f);
                    }

                    float brightnessMultiplier = FakeRift ? 0.75f : 1f;
                    CustomPulse shineExplosion2 = new(Projectile.Center, Vector2.Zero, Color.White * brightnessMultiplier, "CalamityMod/Particles/ShineExplosion2", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.5f * effectsLifetimeInterpolant, 45);
                    GeneralParticleHandler.SpawnParticle(shineExplosion2);

                    // Increase the scale and exposure of the actual crack at each interval.
                    CrackScale += 0.525f * effectsLifetimeInterpolant * 0.75f;
                    MaxExposure = MathHelper.Clamp(MaxExposure + 0.25f * effectsLifetimeInterpolant, 0f, 0.95f);
                }

                // Perform the explosion right at the point where DoG emerges.
                if (Timer >= RiftLifetime)
                    SwitchAIStates();

                if (Projectile.timeLeft < 45)
                    Projectile.timeLeft = 45;

                CrackExposure = MathHelper.Lerp(CrackExposure, MaxExposure, 0.075f);
                Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.05f, 0f, 1f);
                Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.02f, 0f, 1f);
                Projectile.ForceNetUpdate();
            }

            // Post-explosion, slowly fade out.
            if (AIState == 1f)
            {
                Projectile.Opacity = MathHelper.Lerp(1f, 0f, Timer / 45f);
                CrackExposure = MathHelper.Lerp(MaxExposure, 0f, Timer / 45f);
            }

            Timer++;
        }

        public void SwitchAIStates()
        {
            AIState = 1f;
            Timer = 0f;
            CrackScale += 3f;
            Projectile.timeLeft = 30;
            SpawnExplosionVisuals();
            Projectile.netUpdate = true;
        }

        public void SpawnExplosionVisuals()
        {
            float brightnessMultiplier = CalamityClientConfig.Instance.Photosensitivity ? 0.6f : 1f;
            if (!FakeRift)
            {
                // Spawn the rift cracks.
                for (int i = 0; i < 25; i++)
                {
                    Vector2 crackOffset = Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / 25) * Main.rand.NextFloat(20f, 80f);
                    Vector2 crackDestination = Projectile.Center + crackOffset;
                    Vector2 crackLength = crackDestination - Projectile.Center;
                    float riftWidth = Main.rand.NextFloat(20f, 30f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, crackLength, ModContent.ProjectileType<DoGRiftCrack>(), 0, 0f, ai1: riftWidth);
                }

                // Spawn a bunch of square-shaped metaballs at the impact zone as well.
                for (int i = 0; i < 35; i++)
                    DoGDistortionMetaball.SpawnParticle(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f), Vector2.Zero, 500f, true, Main.rand.NextFloat(MathHelper.TwoPi));

                // More metaballs flying away from the impact zone for extra flavor.
                int metaballAmount = Main.rand.Next(15, 21);
                for (int i = 0; i < metaballAmount; i++)
                    DoGDistortionMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(25f, 35f), Main.rand.NextFloat(30f, 50f));

                // Addtional particles and effects.
                for (int i = 0; i < 25; i++)
                {
                    Vector2 sparkVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(12f, 16f);
                    float sparkScale = Main.rand.NextFloat(1.8f, 2f);
                    Color sparkColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, DoGSky.DoGLightBlue, DoGSky.DoGTwlight), Color.White, 0.65f);
                    SparkParticle crackSpark = new(Projectile.Center, sparkVelocity, false, Main.rand.Next(30, 45), sparkScale, sparkColor);
                    GeneralParticleHandler.SpawnParticle(crackSpark);
                }

                for (int i = 0; i < 20; i++)
                {
                    Vector2 lightVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(16f, 20f);
                    float lightScale = Main.rand.NextFloat(1.8f, 2f);
                    Color lightColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, DoGSky.DoGLightBlue, DoGSky.DoGTwlight), Color.White, 0.65f);
                    SquishyLightParticle crackLight = new(Projectile.Center, lightVelocity, lightScale, lightColor, Main.rand.Next(30, 45));
                    GeneralParticleHandler.SpawnParticle(crackLight);
                }

                // Explosion visuals.
                for (int i = 0; i < 3; i++)
                {
                    CustomPulse plasmaExplosion = new(Projectile.Center, Vector2.Zero, DoGSky.DoGTwlight * 0.8f * brightnessMultiplier, "CalamityMod/Particles/PlasmaExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 1.25f + i * 0.05f, 45);
                    GeneralParticleHandler.SpawnParticle(plasmaExplosion);
                }

                CustomPulse shineExplosion = new(Projectile.Center, Vector2.Zero, DoGSky.DoGTwlight * brightnessMultiplier, "CalamityMod/Particles/ShineExplosion1", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.75f, 45);
                GeneralParticleHandler.SpawnParticle(shineExplosion);
                CustomPulse shineExplosion2 = new(Projectile.Center, Vector2.Zero, Color.White * 0.6f * brightnessMultiplier, "CalamityMod/Particles/ShineExplosion2", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.5f, 45);
                GeneralParticleHandler.SpawnParticle(shineExplosion2);
                StrongBloom bloom = new(Projectile.Center, Vector2.Zero, Color.White * 0.8f * brightnessMultiplier, 8f, 30);
                GeneralParticleHandler.SpawnParticle(bloom);

                SoundEngine.PlaySound(BreakSound, Projectile.Center);
                CalamityUtils.AddScreenshakeAt(Projectile.Center, 14f);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    PulseRing fakerDeathRing = new(Projectile.Center, Vector2.Zero, Color.White * 0.45f * brightnessMultiplier, 0f, 3f + i * 0.1f, 30 + i * 15);
                    GeneralParticleHandler.SpawnParticle(fakerDeathRing);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D crackTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/CrackedGlass_Glowing").Value;
            Texture2D starTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            Texture2D bloomRingTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing").Value;

            float chargeInterpolant = CalamityUtils.SineInOutEasing(Timer / RiftLifetime, 1);
            float brightnessMultiplier = FakeRift ? 0.6f : 1f;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 verticalStarPartScale = new Vector2(1f, 8f) * chargeInterpolant * Projectile.scale * 3.25f;
            Vector2 horizontalStarPartScale = new Vector2(1f, 5f) * chargeInterpolant * Projectile.scale * 3.25f;

            Main.spriteBatch.SetBlendState(BlendState.Additive);

            // Draw the glowing star.
            Main.EntitySpriteDraw(starTexture, drawPosition, null, Color.White * 0.7f * brightnessMultiplier * chargeInterpolant, 0f, starTexture.Size() * 0.5f, verticalStarPartScale, 0);
            Main.EntitySpriteDraw(starTexture, drawPosition, null, Color.White * 0.7f * brightnessMultiplier * chargeInterpolant, MathHelper.PiOver2, starTexture.Size() * 0.5f, horizontalStarPartScale, 0);

            // Draw a smaller, twlight colored star at the center if this isn't a fake rift.
            if (!FakeRift)
            {
                Main.EntitySpriteDraw(starTexture, drawPosition, null, DoGSky.DoGTwlight * 0.9f * brightnessMultiplier * chargeInterpolant, 0f, starTexture.Size() * 0.5f, verticalStarPartScale * 0.8f, 0);
                Main.EntitySpriteDraw(starTexture, drawPosition, null, DoGSky.DoGTwlight * 0.9f * brightnessMultiplier * chargeInterpolant, MathHelper.PiOver2, starTexture.Size() * 0.5f, horizontalStarPartScale * 0.8f, 0);
            }

            // Draw the crack.
            Effect crackShader = CalamityShaders.DoGRealityCrackShader.Value;
            float crackOpcity = (AIState == 1f) ? Projectile.Opacity * 0.1f : 0.1f;
            Color darkerPixelColor = FakeRift ? Color.White : DoGSky.DoGTwlight;

            crackShader.Parameters["opacityCutoffValue"].SetValue(CrackExposure);
            crackShader.Parameters["fadeoutPower"].SetValue(1f);
            crackShader.Parameters["overallOpacity"].SetValue(crackOpcity * brightnessMultiplier);
            crackShader.Parameters["minBrightnessValue"].SetValue(0.75f);
            crackShader.Parameters["darkerPixelColor"].SetValue(darkerPixelColor.ToVector3());
            crackShader.Parameters["brighterPixelColor"].SetValue(Color.White.ToVector3());
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, crackShader, Main.GameViewMatrix.TransformationMatrix);
                int crackCount = FakeRift ? 1 : 3;
                for (int i = 0; i < crackCount; i++)
                {
                    float crackRotation = i * MathHelper.TwoPi / crackCount;
                    Main.EntitySpriteDraw(crackTexture, drawPosition + Vector2.UnitY * 8f, null, Color.White * Projectile.Opacity, crackRotation, crackTexture.Size() * 0.5f, CrackScale, 0);
                }
                Main.spriteBatch.End();
            }

            if (AIState != 1f)
            {
                // Draw the bloom ring which closes inwards.
                float ringScale = MathHelper.Lerp(8f, 0f, chargeInterpolant);
                Color ringColor = FakeRift ? Color.White : Color.Lerp(Color.White, DoGSky.DoGTwlight, chargeInterpolant);
                Main.EntitySpriteDraw(bloomRingTexture, drawPosition, null, ringColor * brightnessMultiplier * chargeInterpolant * 0.6f, 0f, bloomRingTexture.Size() * 0.5f, ringScale, 0);
            }

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return false;
        }
    }
}
