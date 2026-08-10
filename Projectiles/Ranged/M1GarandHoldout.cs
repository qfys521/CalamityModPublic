using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using System;
using ReLogic.Utilities;

namespace CalamityMod.Projectiles.Ranged
{
    public class M1GarandHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<M1Garand>();

        public ref float CurrentState => ref Projectile.ai[0];
        public ref float reloadTimeLeft => ref Projectile.ai[1];
        public ref float globalTimer => ref Projectile.ai[2];

        private float muzzleFlashTimer;
        private float flashVariant;

        private int pingAnimationTimer = 0;
        private const int PingAnimationDuration = 120;
        private bool shouldPing = false;

        public override float RecoilResolveSpeed => 0.13f;
        public override float MaxOffsetLengthFromArm => 28f;
        public override float BaseOffsetY => -4f;
        public override float OffsetXUpwards => -6f;
        public override float OffsetXDownwards => 0f;
        public override float OffsetYUpwards => 0f;
        public override float OffsetYDownwards => 0f;

        public override Vector2 GunTipPosition => (Projectile.Center + new Vector2(1.5f, -2.5f)) + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.425f;
        public override string Texture => "CalamityMod/Projectiles/Ranged/M1GarandHoldout";

        // Muzzle flash textures
        public static Asset<Texture2D> MuzzleFlash;
        public static Asset<Texture2D> HoldoutGlow;

        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/WulfrumBlunderbussFire") { PitchVariance = 0.1f }; 
        public static readonly SoundStyle PingSound = new("CalamityMod/Sounds/Item/M1GarandPing");
        public static readonly SoundStyle ReloadSound = new("CalamityMod/Sounds/Item/M1GarandReload");
        private SlotId reloadSoundSlot;

        public override void Load()
        {
            MuzzleFlash = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/M1GarandMuzzleFlash", AssetRequestMode.ImmediateLoad);
            HoldoutGlow = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/M1GarandHoldoutGlow", AssetRequestMode.ImmediateLoad);
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            Projectile.frame = Main.projFrames[Type] - 1;

        }
        public override void KillHoldoutLogic()
        {
            // Only allow the projectile to kill if we're not reloading
            if (CurrentState == 0 && HeldItem.type != Owner.HeldItem.type)
            {
                Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }

        public override void HoldoutAI()
        {
            globalTimer++;

            // Decrements the timer which controls the duration of the flash
            if (muzzleFlashTimer > 0f)
                muzzleFlashTimer -= 1f;

            if (shouldPing)
                pingAnimationTimer += 3;

            hereInstead: // Code reads from back here after getting the 'goto' set down below
            switch (CurrentState)
            {
                // Normal Firing State
                case 0:
                    // Time between firing (as the player continues to hold)
                    if (Projectile.frame == 5)
                    {
                        Projectile.frameCounter++;
                        if (Projectile.frameCounter >= MathHelper.Clamp(Owner.HeldItem.useAnimation - 18, 0f, Owner.HeldItem.useAnimation))
                        {
                            if (Owner.CantUseHoldout())
                            {
                                Projectile.Kill();
                                return;
                            }

                            if (Owner.Calamity().garandShots != 0)
                                Projectile.frame = 0;
                            else
                            {
                                CurrentState = 1; // Begin reloading
                                reloadTimeLeft = 120;
                                goto hereInstead;
                            }

                            Projectile.frameCounter = 0;
                        }
                    }
                    // Begin firing
                    else if (Projectile.frame == 0)
                    {
                        if (Owner.Calamity().garandShots == 0 && globalTimer < 3)
                        {
                            CurrentState = 1; // Begin reloading
                            reloadTimeLeft = 120;
                            goto hereInstead;
                        }

                        if (Projectile.frameCounter != 1)
                        {
                            Projectile.frameCounter++;
                            if (Projectile.frameCounter >= 3)
                            {
                                Projectile.frame++;
                                Projectile.frameCounter = 0;
                            }
                            return;
                        }

                        Projectile.frameCounter++;

                        // Check if the player has any shots left
                        if (Owner.Calamity().garandShots > 0)
                        {
                            // -- Firing Logic & FX --
                            muzzleFlashTimer = 2f;
                            flashVariant = Main.rand.Next(3);
                            SoundEngine.PlaySound(FireSound, GunTipPosition);

                            if (Owner.Calamity().garandShots == 1)
                            {
                                SoundEngine.PlaySound(PingSound, GunTipPosition);
                                shouldPing = true;
                            }
                            
                            OffsetLengthFromArm -= 3.5f;

                            // Consume a shot
                            Owner.Calamity().garandShots--;

                            Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * HeldItem.shootSpeed;

                            if (Main.myPlayer == Projectile.owner)
                            {
                                // Eject bullet casing
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity.RotatedBy(Main.rand.NextFloat(2.5f, 2.65f) * -Projectile.direction) * 0.5f, ModContent.ProjectileType<M1GarandBulletCasing>(), 0, 0, Projectile.owner);

                                if (Owner.Calamity().garandShots == 0)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity.RotatedBy(Main.rand.NextFloat(1.175f, 1.25f) * -Projectile.direction) * 0.285f, ModContent.ProjectileType<M1GarandEmptyClip>(), 0, 0, Projectile.owner);

                                    GenericSparkle sparker = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.PaleGoldenrod, Color.Gold * 0.25f, 1.25f, 5, Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.15f, 0.15f), 1f);
                                    GeneralParticleHandler.SpawnParticle(sparker);
                                }
                            }

                            // Direct smoke
                            for (int i = 0; i < 8; i++)
                            {
                                Particle smoke = new HeavySmokeParticle(GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * 7f * Main.rand.NextFloat(0.4f, 1.5f), Color.Gray, Main.rand.Next(30, 50), Main.rand.NextFloat(0.22f, 0.44f), 0.35f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                                GeneralParticleHandler.SpawnParticle(smoke);
                            }

                            // "Smoke" ring
                            Particle shootPulse = new DirectionalPulseRing(GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * 4f, Color.Gray * 0.5f, new Vector2(0.5f, 1f), Projectile.velocity.ToRotation(), 0.1f, 0.475f, 18);
                            GeneralParticleHandler.SpawnParticle(shootPulse);

                            // Shot starts from slightly behind the tip
                            if (Main.myPlayer == Projectile.owner)
                            {
                                Vector2 offsetPos = GunTipPosition - Projectile.velocity.SafeNormalize(Vector2.UnitY) * 18f;
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), offsetPos, velocity, ModContent.ProjectileType<M1GarandShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                            }
                        }
                    }
                    // The firing animation
                    else if (Projectile.frame > 0)
                    {
                        Projectile.frameCounter++;
                        if (Projectile.frameCounter >= 3)
                        {
                            Projectile.frame++;
                            Projectile.frameCounter = 0;

                        }
                    }
                    break;

                // Reloading
                case 1:
                    reloadTimeLeft--;

                    // Keep the player from switching weapons
                    Owner.channel = true;

                    // The gun points slightly downwards during the first part
                    if (reloadTimeLeft <= 120 && reloadTimeLeft > 75)
                    {
                        Projectile.rotation += 0.15f * Projectile.spriteDirection;
                    }
                    // Lerp back to the normal pos
                    else if (reloadTimeLeft < 75 && reloadTimeLeft >= 64)
                    {
                        float progress = (75f - reloadTimeLeft) / (75f - 64f);
                        float lerpedOffset = MathHelper.Lerp(0.15f, 0f, progress);
                        Projectile.rotation += lerpedOffset * Projectile.spriteDirection;
                    }
                    else if (reloadTimeLeft == 39)
                    {
                        OffsetLengthFromArm -= 2f;
                    }

                    // Hold frame 3 until this point
                    if (reloadTimeLeft > 40)
                    {
                        Projectile.frame = 3;
                        Projectile.frameCounter = 0;
                    }

                    else if (Projectile.frame < Main.projFrames[Type] - 1)
                    {
                        Projectile.frameCounter++;
                        if (Projectile.frameCounter >= 3)
                        {
                            Projectile.frame++;
                            Projectile.frameCounter = 0;
                        }
                    }

                    else
                    {
                        // Hold
                        Projectile.frame = Main.projFrames[Type] - 1;
                    }

                    if (reloadTimeLeft == 110) 
                    {
                        reloadSoundSlot = SoundEngine.PlaySound(ReloadSound, Projectile.Center);
                    }

                    if (SoundEngine.TryGetActiveSound(reloadSoundSlot, out var sound) && sound.IsPlaying)
                    {
                        // Update the sound's position to the current projectile center
                        sound.Position = Projectile.Center;
                    }

                    // Reload and cull the projectile
                    if (reloadTimeLeft <= 0)
                    {
                        // Stop when sound is complete
                        if (SoundEngine.TryGetActiveSound(reloadSoundSlot, out var soundToStop) && soundToStop.IsPlaying)
                        {
                            soundToStop.Stop();
                        }

                        Owner.Calamity().garandShots += 8;
                        globalTimer = 0;
                        Projectile.Kill();
                        Projectile.netUpdate = true;
                    }

                    break;
            }
        }

        // Can damage when firing.
        public override bool? CanDamage() => muzzleFlashTimer > 0f;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(GunTipPosition + Vector2.UnitX.RotatedBy(Projectile.rotation) * MuzzleFlash.Width() * 0.5f, 64f, targetHitbox);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // This class doesn't draw correctly on the first 2 frames.
            if (globalTimer < 3)
                return false;

            int chosenFrame = Math.Clamp(Projectile.frame, 0, Main.projFrames[Type] - 1);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: chosenFrame);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            float scale = Projectile.scale * Owner.gravDir;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if ((CurrentState != 1 && Owner.Calamity().garandShots == 0 && Projectile.frame >= 3)) // After firing the last shot, hold frame 3 after it increments to it to match the start of the reload animation
                frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: 3);

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), drawRotation, frame.Size() * 0.5f, scale, flipSprite);

            // Flash
            if (muzzleFlashTimer > 0f)
            {
                Texture2D flashTex = MuzzleFlash.Value;
                int flashFrameY = (int)flashVariant;
                Vector2 flashPos = GunTipPosition + Vector2.UnitX.RotatedBy(Projectile.rotation) * MuzzleFlash.Width() * 0.5f - Main.screenPosition;
                Rectangle flashFrame = flashTex.Frame(verticalFrames: 3, frameY: flashFrameY);
                Main.EntitySpriteDraw(flashTex, flashPos, flashFrame, Color.White, drawRotation, flashFrame.Size() * 0.5f, scale * 0.8f, flipSprite);
            }

            // PING!
            if (shouldPing)
            {
                if (pingAnimationTimer <= PingAnimationDuration)
                {
                    Texture2D pingTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/M1GarandPingText").Value;
                    Vector2 origin = pingTexture.Size() * 0.5f;
                    Vector2 pingDrawPosition;

                    Vector2 finalPosition = Owner.Center + new Vector2(40f * -Owner.direction, -40f * Owner.gravDir);
                    float progress = (float)pingAnimationTimer / PingAnimationDuration;

                    if (pingAnimationTimer < 15)
                    {
                        float popOutProgress = (float)pingAnimationTimer / 15;
                        pingDrawPosition = Vector2.Lerp(Owner.Center, finalPosition, popOutProgress);
                    }
                    else
                    {
                        pingDrawPosition = finalPosition;
                    }

                    pingDrawPosition -= Main.screenPosition;

                    float stretchFactorY = 0f;

                    if (progress < 0.25f) // Stretch Up
                    {
                        float stretchProgress = progress / 0.25f;
                        stretchFactorY = MathHelper.Lerp(-1.4f, 0.5f, (float)Math.Sin(stretchProgress * MathHelper.PiOver2));
                    }
                    else if (progress < 0.5f) // Squash Down and overadjust
                    {
                        float squashProgress = (progress - 0.25f) / 0.3f;
                        stretchFactorY = MathHelper.Lerp(0.5f, -0.25f, squashProgress * squashProgress);
                    }
                    else if (progress < 0.8f) // Stretch up again at a slower pace to a bit above the normal scale
                    {
                        float stretchProgress = (progress - 0.6f) / 0.2f;
                        stretchFactorY = MathHelper.Lerp(-0.25f, 0f, stretchProgress * stretchProgress);
                    }
                    else // Return to normal
                    {
                        stretchFactorY = 0f;
                    }

                    // Draw without any additions
                    Color drawColor = Color.White;
                    int fadeDuration = 30;
                    int fadeStartFrame = PingAnimationDuration - fadeDuration;

                    // Fade out over 30 frames
                    if (pingAnimationTimer >= fadeStartFrame)
                    {
                        float fadeProgress = (float)(pingAnimationTimer - fadeStartFrame) / fadeDuration;
                        drawColor = Color.White * MathHelper.Lerp(1f, 0f, fadeProgress);
                    }

                    float basePingDrawScale = 1f;
                    float stretchScaleY = 1f + stretchFactorY;
                    float stretchScaleX = 1f - (stretchFactorY * 0.5f);

                    // Calculate scale to display with all factors combined
                    Vector2 finalPingScale = new Vector2(basePingDrawScale * stretchScaleX, basePingDrawScale * stretchScaleY);
                    Main.EntitySpriteDraw(pingTexture, pingDrawPosition, null, drawColor, (Owner.direction == 1 ? -0.25f : 0.25f) + (Owner.gravDir == -1 ? MathHelper.Pi * Owner.direction : 0f), origin, finalPingScale, (Owner.gravDir == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0f);
                }
                else
                {
                    // Reset the flag and timer once the animation is complete.
                    shouldPing = false;
                    pingAnimationTimer = 0;
                }
            }

            return false;
        }

        // Glowmask.
        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // This class doesn't draw correctly on the first 2 frames.
            if (CurrentState == 1 || globalTimer < 3)
                return;

            int chosenFrame = Math.Clamp(Projectile.frame, 0, Main.projFrames[Type] - 1);

            if (globalTimer > 2 && globalTimer < 6) // If the holdout just spawned in and isn't in the reload state, play a belated flash animation.
                chosenFrame = 0;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: chosenFrame);
            Texture2D glowTexture = HoldoutGlow.Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 origin = frame.Size() * 0.5f;
            float scale = Projectile.scale * Owner.gravDir;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(glowTexture, drawPosition, frame, Color.White, drawRotation, origin, scale, flipSprite);
        }
    }
}
