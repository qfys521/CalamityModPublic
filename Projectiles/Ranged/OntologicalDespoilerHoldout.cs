using System;
using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class OntologicalDespoilerHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<OntologicalDespoiler>();
        public override float MaxOffsetLengthFromArm => 29f;
        public override float OffsetXUpwards => -8f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 8f;
        public override float RecoilResolveSpeed => 0.5f;
        public override Vector2 GunTipPosition => Projectile.Center + (Projectile.velocity * 45).RotatedBy(0.18f * Projectile.direction);

        public int FramesPerLoad = 9;
        public int MaxLoadableShots = 23;
        public float BulletSpeed = 6.5f;
        public SlotId OntologicalChargeSlot;
        public SlotId OntologicalChargeSlotDeep;
        public int Time = 0;

        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public float ShotsLoaded = 1;
        public Color baseColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
        public Color color1;
        public Color color2;
        public Color color3;
        public Color color4;

        public bool hasBegunFiring = false;
        public bool Positive => (Projectile.ai[2] == 0 || Projectile.ai[2] == 10);
        public ref float ShootRecoilTimer => ref Projectile.ai[1]; // Dual functions for rapid fire shooting cooldown and recoil

        public int AftershotCooldownFrames = 8;
        public int Charge1Frames = 156;
        public int Charge2Frames = 312;
        public bool hasReachedLV1 = false;
        public bool hasReachedLV2 = false;

        public int fireFrameCounter = 0;
        public int fireFrame = 0;
        public int flashFrameCounter = 0;
        public int flashFrame = 0;
        public bool showFlash = false;
        public Vector2 flashPos;
        public float flashRot;

        // Distortion effect visuals
        public Vector2 voidPlacement;
        public int inverseTimer = 14;
        public bool drawInverse = false;
        public bool ChargeLV1 => CurrentChargingFrames >= Charge1Frames;
        public bool ChargeLV2 => CurrentChargingFrames >= Charge2Frames;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.HeldItem.type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            color1 = Owner.shirtColor;
            color2 = Color.Lerp(Owner.shirtColor, Color.Black, 0.3f);
            color3 = Color.Lerp(Owner.shirtColor, Color.White, 0.2f);
            color4 = Color.Lerp(Owner.shirtColor, Color.White, 0.4f);

            if (SoundEngine.TryGetActiveSound(OntologicalChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;
            if (SoundEngine.TryGetActiveSound(OntologicalChargeSlotDeep, out var ChargeSound6) && ChargeSound6.IsPlaying)
                ChargeSound6.Position = Projectile.Center;

            if (Owner.shirtColor != Color.White)
            {
                float rate = (Main.GlobalTimeWrappedHourly * 15);
                List<Color> eColors = new List<Color>()
                {
                    color1,
                    color2,
                    color3,
                    color4
                };
                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                baseColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);
            }

            if ((!Positive && Projectile.ai[2] < 10) || Projectile.ai[2] == 15) // Color is white when Negative
                baseColor = Color.White;
            else if (Owner.shirtColor == Color.White)
                baseColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

            #region Frame control
            Projectile.frameCounter++;
            if (Projectile.frameCounter > (Projectile.ai[2] >= 10 ? 2 : 1))
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5)
            {
                Projectile.frame = 0;
            }

            fireFrameCounter++;
            if (fireFrameCounter > (Projectile.ai[2] >= 10 ? 2 : 1))
            {
                fireFrame++;
                fireFrameCounter = 0;
            }
            if (fireFrame > 5)
                fireFrame = 0;

            if (showFlash)
            {
                flashFrameCounter++;
                if (flashFrameCounter > (flashFrame == 2 ? 7 : 1))
                {
                    flashFrame++;
                    flashFrameCounter = 0;
                }
                if (flashFrame > 6)
                    showFlash = false;
            }
            #endregion

            if (Time % 2 == 0)
                voidPlacement = Main.rand.NextVector2Circular(10.5f, 10.5f);

            if (inverseTimer <= 9)
            {
                drawInverse = true;
                if (inverseTimer == 0)
                {
                    drawInverse = false;
                    inverseTimer = Main.rand.Next(20, 40 + 1);
                }
            }
            if (ChargeLV2)
                inverseTimer--;

            // Changing Mode
            if (Projectile.ai[2] == 20)
            {
                Projectile.frame = 6;
                ShotsLoaded = 0;
                if (Time == 0)
                {
                    Projectile.timeLeft = (int)(AftershotCooldownFrames * 1.5f);
                    OffsetLengthFromArm = 45f;
                }
            }
            else if (Projectile.ai[2] >= 10)
            {
                ShotsLoaded = 0;
                if (Time == 0)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        Color useColor = GetRandomColor();

                        Dust dust = Dust.NewDustPerfect(Owner.Center, !Positive ? (Main.rand.NextBool() ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ModContent.DustType<LightDust>(), new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.9f, 1.25f);
                        dust.color = Projectile.ai[2] == 15 ? baseColor : useColor;
                    }
                    Projectile.timeLeft = (int)(AftershotCooldownFrames * 2.5f * (!Owner.Calamity().despoilerNerf ? 3 : 1)); // If you're swapping with no penalty, the swap takes longer
                    Owner.Calamity().despoilerNerf = false; // Remove charge speed penalty if you swap
                    OffsetLengthFromArm = 45f;
                }
                if (Main.rand.NextBool() && Time > 1)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - (Projectile.velocity * 60).RotatedBy(0.15f * Projectile.direction), Projectile.ai[2] == 15 ? (Main.rand.NextBool() ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ModContent.DustType<LightDust>(), (Projectile.velocity * 14).RotatedBy(MathHelper.ToRadians(-135f) * Projectile.direction).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.6f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 0.9f) * Utils.GetLerpValue(-10, 15, Projectile.timeLeft);
                    dust.color = baseColor;
                }
            }

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout() || Projectile.ai[2] >= 10)
            {
                if (Projectile.ai[2] < 10 && ChargeLV1) // Give charge speed nerf if you use any form of charge
                    Owner.Calamity().despoilerNerf = true;
                hasBegunFiring = true;

                if (SoundEngine.TryGetActiveSound(OntologicalChargeSlot, out var ChargeSound3))
                    ChargeSound3?.Stop();
                if (SoundEngine.TryGetActiveSound(OntologicalChargeSlotDeep, out var ChargeSound4))
                    ChargeSound4?.Stop();

                KeepRefreshingLifetime = false;

                // Big Shot mode
                if (ChargeLV2)
                {
                    if (ShotsLoaded > 0)
                    {
                        // Set lifespan to double of remaining cooldown so that it can finish the recoil animation
                        Projectile.timeLeft = AftershotCooldownFrames * 2;

                        ChargeSound?.Stop();

                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                        int charge2DamagePos = (int)(Projectile.damage * 0.5f); // Most of the damage comes from the explosion
                        int charge2DamageNeg = Projectile.damage * 20; // Big charge time? Check. No pierce? Check. 20x damage? Yes!

                        if (Main.myPlayer == Projectile.owner)
                        {
                            if (Positive)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity * 2.5f, ModContent.ProjectileType<OntologicalDespoilerGrenade>(), charge2DamagePos, Projectile.knockBack, Projectile.owner);
                                SoundEngine.PlaySound(OntologicalDespoiler.BigShot, Projectile.Center);
                            }
                            else
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity * 0.6f, ModContent.ProjectileType<OntologicalDespoilerBeam>(), charge2DamageNeg, Projectile.knockBack * 3, Projectile.owner);
                                Owner.SetScreenshake(12f);
                                SoundEngine.PlaySound(OntologicalDespoiler.BigShot2 with { Pitch = 0.2f }, Projectile.Center);
                                SoundEngine.PlaySound(OntologicalDespoiler.BigShot with { Pitch = -0.3f }, Projectile.Center);
                            }
                        }

                        // Setting stats for the flash visual effect so it doesn't stick to the gun
                        flashPos = GunTipPosition + Projectile.velocity * 90;
                        flashRot = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
                        showFlash = true;

                        for (int i = 0; i < 20; i++)
                        {
                            Color useColor = GetRandomColor();

                            float rand = Main.rand.NextFloat(0, 2.5f);
                            Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, (i % 2 == 0 ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()), shootVelocity.RotatedByRandom(0.12f + rand * 0.2f) * (Main.rand.NextFloat(2.5f, 6.3f) - rand));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(1.55f, 1.85f);
                            dust.color = Positive ? useColor : baseColor;
                        }

                        ShotsLoaded = 0;
                        ShootRecoilTimer = 34f;
                        OffsetLengthFromArm -= 35f;
                    }
                    // Retracting recoil
                    else if (ShootRecoilTimer > 0)
                        ShootRecoilTimer -= 2;
                }
                // Rapid Fire mode
                else if (ShotsLoaded > 0)
                {
                    // While bullets are remaining, refresh the lifespan; it will not refresh again after bullets run out
                    Projectile.timeLeft = (ChargeLV1 ? AftershotCooldownFrames : (int)(AftershotCooldownFrames * 1.5f));

                    // Retract recoil & shoot faster if charged
                    ShootRecoilTimer -= ChargeLV1 ? 3f : 2f;

                    if (ShootRecoilTimer <= 0f)
                    {
                        ChargeSound?.Stop();

                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Vector2 fireVec = shootVelocity * Main.rand.NextFloat(0.9f, 1.1f);
                            if (Positive)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    float angle = i == 0 ? -0.25f : i == 2 ? 0.25f : 0;
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, fireVec.RotatedBy(angle) * (1 - Math.Abs(angle * 0.25f)), ModContent.ProjectileType<OntologicalDespoilerShot>(), (int)(Projectile.damage), Projectile.knockBack, Projectile.owner, 0, 0, i);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, fireVec.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.8f, 1f), ModContent.ProjectileType<OntologicalDespoilerShot>(), (int)(Projectile.damage / 4f), Projectile.knockBack, Projectile.owner, 0, 0, 5);
                                }
                            }
                        }

                        SoundEngine.PlaySound(OntologicalDespoiler.SmallShot with { Pitch = Positive ? 0 : -0.2f, MaxInstances = Positive ? 1 : -1, Volume = Positive ? 1 : 0.5f }, Projectile.Center);
                        if (!Positive)
                            SoundEngine.PlaySound(MagnaCannon.Fire with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
                        for (int i = 0; i < 3; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, !Positive ? (Main.rand.NextBool() ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ModContent.DustType<LightDust>(), shootVelocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 2.3f));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(1.15f, 1.35f);
                            dust.color = baseColor;
                        }

                        ShotsLoaded -= (Positive ? 2 : 1);
                        ShootRecoilTimer = Positive ? 16 : 12f;
                        OffsetLengthFromArm -= 3.2f;
                    }
                }
                // Retracting any remaining recoil
                else if (ShootRecoilTimer > 0)
                    ShootRecoilTimer -= 2;
            }
            else
            {
                // Loads shots until maxed out
                ShotsLoaded = Utils.Remap(CurrentChargingFrames, 0, Charge1Frames, 1, MaxLoadableShots / 2);
                if (ChargeLV1)
                    ShotsLoaded = MaxLoadableShots;

                CurrentChargingFrames += ((ChargeLV1 && !Positive) ? 0.5f : 2) * (Owner.Calamity().despoilerNerf ? 0.4f : 1); // Charges slower if nerfed, charge LV2 for Negative is slower

                // Sounds
                if (!hasBegunFiring)
                {
                    if (SoundEngine.TryGetActiveSound(OntologicalChargeSlot, out var ChargeSound2))
                    {
                        ChargeSound2.Volume = Utils.Remap(CurrentChargingFrames, 0, Charge1Frames, 0, 0.8f, true) * 100;
                        ChargeSound2.Pitch = Utils.Remap(CurrentChargingFrames, 0, Charge2Frames, -1, 0.45f, true);
                    }
                    else
                        OntologicalChargeSlot = SoundEngine.PlaySound(OntologicalDespoiler.ChargeLoop with { Volume = 0.01f, Pitch = 0f, IsLooped = true }, Projectile.Center);

                    if (SoundEngine.TryGetActiveSound(OntologicalChargeSlotDeep, out var ChargeSound5))
                    {
                        ChargeSound5.Volume = Positive ? 0 : Utils.Remap(CurrentChargingFrames, Charge1Frames, Charge2Frames, 0, 1.8f, true) * 100;
                        ChargeSound5.Pitch = Utils.Remap(CurrentChargingFrames, Charge2Frames * 0.9f, Charge2Frames, 1f, (CurrentChargingFrames >= Charge2Frames ? -0.1f : 0.2f), true);
                    }
                    else
                        OntologicalChargeSlotDeep = SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/MagnaCannonChargeLoop") with { Volume = 0.01f, Pitch = 0f, IsLooped = true }, Projectile.Center);
                }

                // Charge-up visuals
                if (CurrentChargingFrames >= 10 && (Positive || (!Positive && !ChargeLV2)))
                {
                    float particleScale = MathHelper.Clamp(CurrentChargingFrames, 0f, Charge2Frames);
                    for (int i = 0; i < (ChargeLV2 ? 3 : ChargeLV1 ? 2 : 1); i++)
                    {
                        Vector2 dustVel = -Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(1.1f, 15.8f);
                        Vector2 addedPlace = Positive ? Vector2.Zero : dustVel * 3;
                        bool dustChance = Main.rand.NextBool((int)(30 * Utils.GetLerpValue(Charge2Frames, Charge1Frames, CurrentChargingFrames, true) + 1));
                        int dustType = !Positive ? (Main.rand.NextBool() ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ChargeLV1 ? (dustChance ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>()) : ModContent.DustType<LightDust>();
                        
                        Dust dust = Dust.NewDustPerfect(GunTipPosition + (Positive ? Vector2.Zero : (dustVel * 15 * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true))), dustType, dustVel - addedPlace * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.5f, 1.25f) * Utils.GetLerpValue(0, Charge1Frames, CurrentChargingFrames, true);
                        dust.color = baseColor;
                    }
                    Particle orb = new GenericBloom(GunTipPosition, Projectile.velocity, Positive ? Color.Lerp(Color.White, Color.Black, Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true)) : Color.Black * Utils.GetLerpValue(0, Charge1Frames, CurrentChargingFrames, true), (particleScale / 400f) * Main.rand.NextFloat(0.9f, 1.1f), 2, false, false);
                    GeneralParticleHandler.SpawnParticle(orb);

                    float strength = particleScale / 45f;
                    Vector3 DustLight = baseColor.ToVector3() * 0.2f;
                    Lighting.AddLight(GunTipPosition, DustLight * strength);
                }
                if (ChargeLV2 && !Positive)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Particle orb = new CustomSpark(GunTipPosition, Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(1.1f, 15.8f), "CalamityMod/Particles/PearlParticleGlow", false, 3, Main.rand.NextFloat(2.3f, 2.6f), Color.Black, new Vector2(0.3f, 1.35f), false, false);
                        GeneralParticleHandler.SpawnParticle(orb);
                        bool color = Main.rand.NextBool(4);
                        Vector2 dustVel = -Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(1.1f, 15.8f);
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, (color ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()), dustVel);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.5f, 1.25f);
                        dust.color = color ? Color.Black : baseColor;
                    }
                }

                // Full charge dusts and sounds
                if (ChargeLV1 && !hasReachedLV1)
                {
                    SoundEngine.PlaySound(OntologicalDespoiler.ChargeLV1, Projectile.Center);
                    for (int i = 0; i < 16; i++)
                    {
                        Color useColor = GetRandomColor();

                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, !Positive ? (i % 2 == 0 ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ModContent.DustType<LightDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 16f).ToRotationVector2() * 16f * (i % 2 == 0 ? 0.7f : 1);
                        chargefull.scale = Main.rand.NextFloat(1.1f, 1.3f);
                        chargefull.noGravity = true;
                        chargefull.color = Positive ? useColor : baseColor;
                    }
                    Particle orb2 = new CustomPulse(GunTipPosition, Vector2.Zero, Positive ? baseColor : Color.Black, "CalamityMod/Particles/SmallBloomRingLayered", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.65f, 0.85f, 11, Positive);
                    GeneralParticleHandler.SpawnParticle(orb2);
                    hasReachedLV1 = true;
                }
                if (ChargeLV2 && !hasReachedLV2)
                {
                    SoundEngine.PlaySound(OntologicalDespoiler.ChargeLV2, Projectile.Center);
                    for (int i = 0; i < 24; i++)
                    {
                        Color useColor = GetRandomColor();
                        
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, !Positive ? (i % 2 == 0 ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ModContent.DustType<LightDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * 22f * (i % 2 == 0 ? 0.7f : 1);
                        chargefull.scale = Main.rand.NextFloat(1.45f, 1.55f);
                        chargefull.noGravity = true;
                        chargefull.color = Positive ? useColor : baseColor;
                    }
                    Particle orb2 = new CustomPulse(GunTipPosition, Vector2.Zero, Positive ? baseColor : Color.Black, "CalamityMod/Particles/SmallBloomRingLayered", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.85f, 1.05f, 11, Positive);
                    GeneralParticleHandler.SpawnParticle(orb2);
                    hasReachedLV2 = true;
                }
            }
            if (!hasBegunFiring && Projectile.ai[2] < 10) // Prevents firing animation from playing when not firing or swapping types
            {
                Projectile.frame = 6;
                fireFrame = 6;
            }
            else
            {
                Lighting.AddLight(Projectile.Center, baseColor.ToVector3() * 0.75f);
            }
            Time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(OntologicalChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
            if (SoundEngine.TryGetActiveSound(OntologicalChargeSlotDeep, out var ChargeSound2))
                ChargeSound2?.Stop();
        }
        public Color GetRandomColor()
        {
            Color useColor = Main.rand.Next(4) switch
            {
                0 => color1,
                1 => color2,
                2 => color3,
                _ => color4,
            };
            if (Owner.shirtColor == Color.White)
                useColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
            return useColor;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Time < 3)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerHoldout").Value;
            Texture2D textureAlt = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerHoldout2").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (!Owner.CantUseHoldout())
            {
                float rumble = (Projectile.ai[2] == 20 ? 200 : MathHelper.Clamp(CurrentChargingFrames, 0f, Charge2Frames));
                drawPosition += Main.rand.NextVector2Circular(rumble / 120f, rumble / 120f);
            }

            Texture2D fireTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerFlame").Value;
            Texture2D fireTexture2 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerFlame2").Value;
            Rectangle frame2 = fireTexture.Frame(1, 7, 0, fireFrame);
            Vector2 rotationPoint2 = frame2.Size() * 0.5f;

            Texture2D flashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerFlash").Value;
            Rectangle frame3 = fireTexture.Frame(1, 6, 0, flashFrame);
            Vector2 rotationPoint3 = frame3.Size() * 0.5f;

            // Glow Orb
            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/FlameExplosion").Value;
            Texture2D rechargeTexture2 = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;
            float randSize = Main.rand.NextFloat(0.9f, 1.1f);
            if (!hasBegunFiring)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(rechargeTexture, GunTipPosition - Main.screenPosition, null, baseColor with { A = 0 }, Projectile.rotation + Main.rand.NextFloat(-30, 30), rechargeTexture.Size() * 0.5f, 0.028f * Utils.GetLerpValue(0, Charge2Frames, CurrentChargingFrames, true) * randSize, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(rechargeTexture2, GunTipPosition - Main.screenPosition, null, Positive ? Color.Lerp(Color.White, Color.Black, Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true)) : Color.Black, Projectile.rotation + Main.rand.NextFloat(-30, 30), rechargeTexture2.Size() * 0.5f, 0.58f * Utils.GetLerpValue(0, Charge2Frames, CurrentChargingFrames, true) * randSize, SpriteEffects.None, 0);
            }

            if (ChargeLV1 && !Positive && !hasBegunFiring)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 placement = drawPosition + voidPlacement;
                    Main.EntitySpriteDraw(texture, placement, frame, baseColor with { A = 0 } * 0.3f * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true), drawRotation, rotationPoint, new Vector2(Main.rand.NextFloat(0.7f, 1f), Main.rand.NextFloat(0.8f, 1.2f)) * Projectile.scale * Owner.gravDir * 1.1f, flipSprite);
                    Main.EntitySpriteDraw(texture, placement, frame, Color.Black * Utils.GetLerpValue(Charge1Frames, Charge2Frames, CurrentChargingFrames, true), drawRotation, rotationPoint, new Vector2(Main.rand.NextFloat(0.7f, 1f), Main.rand.NextFloat(0.6f, 1f)) * Projectile.scale * Owner.gravDir, flipSprite);
                }
            }
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            if (drawInverse && !Positive)
                Main.EntitySpriteDraw(textureAlt, drawPosition, frame, Color.White * Utils.GetLerpValue(0, 5, inverseTimer, true), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            if (Projectile.frame < 6)
            {
                for (int i = 0; i< 4; i++)
                    Main.EntitySpriteDraw(fireTexture, drawPosition + Main.rand.NextVector2Circular(4.5f, 4.5f), frame2, baseColor with { A = 0 } * 0.3f, drawRotation, rotationPoint2, Projectile.scale * Owner.gravDir, flipSprite);
                Main.EntitySpriteDraw(Positive ? fireTexture : fireTexture2, drawPosition, frame2, Positive ? baseColor with { A = 0 } : baseColor, drawRotation, rotationPoint2, Projectile.scale * Owner.gravDir, flipSprite);
            }
            if (showFlash)
            {
                for (int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(flashTexture, flashPos - Main.screenPosition + Main.rand.NextVector2Circular(4.5f, 4.5f), frame3, baseColor with { A = 0 } * 0.3f, flashRot, rotationPoint3, new Vector2(3, 0.8f) * Projectile.scale * Owner.gravDir, flipSprite);
                Main.EntitySpriteDraw(flashTexture, flashPos - Main.screenPosition, frame3, Color.Black, flashRot, rotationPoint3, new Vector2(3, 0.8f) * Projectile.scale * Owner.gravDir, flipSprite);
            }

            return false;
        }
    }
}
