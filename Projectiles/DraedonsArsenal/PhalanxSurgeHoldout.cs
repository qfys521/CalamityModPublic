using System;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class PhalanxSurgeHoldout : BaseGunHoldoutProjectile
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override int AssociatedItemID => ModContent.ItemType<PhalanxSurge>();

        public float offsetBase = 30;
        public override float MaxOffsetLengthFromArm => doingChargeAttack ? 100f : offsetBase;
        public override float RecoilResolveSpeed => doingChargeAttack ? 2f : (shootingTimer < 0) ? 0.2f : 0.1f;
        public override float OffsetXUpwards => 0f;
        public override float BaseOffsetY => 0f;
        public override float OffsetYDownwards => 0f;
        public override float WeaponTurnSpeed => (shootingTimer < 0 ? 0 : 0.35f);
        public SlotId ChargeSlot;
        public SlotId StartSlot;
        public bool charging => Projectile.ai[2] == 5;
        public ref float shootingTimer => ref Projectile.ai[0];
        public ref float chargeTimer => ref Projectile.ai[1];
        public int fireRate => Owner.HeldItem.useAnimation;
        public int chargeRate => 65;
        public bool fullyCharged = false;
        public bool doingChargeAttack = false;
        public bool startChargeLoop = true;
        public int time = 0;
        float charge = 0; // multiplier for vfx based on charge of the weapon
        public bool didDash = false;

        public override void KillHoldoutLogic()
        {
            if (Owner.dead)
            {
                StopSounds();
                Projectile.Kill();
            }
        }
        public void StopSounds()
        {
            if (SoundEngine.TryGetActiveSound(StartSlot, out var audio) && audio.IsPlaying)
            {
                audio?.Stop();
            }
            if (SoundEngine.TryGetActiveSound(ChargeSlot, out var audio2) && audio2.IsPlaying)
            {
                audio2?.Stop();
            }
        }
        public override void HoldoutAI()
        {
            if (time == 0)
            {
                OffsetLengthFromArm = offsetBase;
                if (!charging)
                    shootingTimer = fireRate;
            }

            if (shootingTimer < 0)
            {
                if (Projectile.localAI[0] > 0)
                {
                    Owner.velocity = -Projectile.velocity * (didDash ? 25 : (5 + 10 * Projectile.localAI[0]));
                    didDash = false;
                    Projectile.localAI[0] = 0;
                }
                if (didDash)
                {
                    Owner.velocity = Projectile.velocity * 25;
                    if (!Collision.SolidCollision(GunTipPosition, 10, 10))
                    {
                        Owner.Center += Projectile.velocity * 40;
                        Projectile.Center += Projectile.velocity * 40;
                    }
                    float fxScale = Utils.GetLerpValue(10, 25, Owner.velocity.Length(), true);
                    for (int i = 0; i < 3; i++)
                    {
                        Particle spark2 = new CustomSpark(Owner.Center + Main.rand.NextVector2Circular(20, 20), -Owner.velocity * Main.rand.NextFloat(0.4f, 1.2f), "CalamityMod/Particles/BloomLineSoftEdge", false, 12, Main.rand.NextFloat(0.02f, 0.03f) * fxScale, Effects.ArsenalEffects.ArsenalLaserColor * 0.8f, new Vector2(1, 1), true, false, 0, false, false, 1f * fxScale);
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                }
                Owner.direction = Projectile.direction;
                shootingTimer++;
                if (shootingTimer == 0)
                {
                    didDash = false;
                    shootingTimer = fireRate;
                }
                return;
            }

            if (HeldItem.type != Owner.HeldItem.type)
            {
                StopSounds();
                Projectile.Kill();
                return;
            }
            if (!charging)
            {
                StopSounds();
            }
            if (shootingTimer == 0)
            {
                if (!charging || Owner.Calamity().arsenalCooldown > 0)
                {
                    if (Owner.Calamity().mouseRight)
                    {
                        Projectile.ai[2] = 5;
                        shootingTimer = 1; // This is just so you dont fire when you stop using it
                        return;
                    }
                    Shoot(false);
                }
                else // Charging mode
                {
                    if (Owner.Calamity().mouseRight && !doingChargeAttack) // Charge up the skewer
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 place = GunTipPosition + Main.rand.NextVector2Circular(5, 5) + Projectile.velocity * 12;
                            int dir = Main.rand.NextBool() ? 1 : -1;
                            Particle chargeParticles = new CustomSpark(place, -Projectile.velocity.RotatedBy(0.42f * dir).RotatedByRandom(0.1f) * Main.rand.NextFloat(8, 30), "CalamityMod/Particles/BloomLineSoftEdge", false, 6, 0.015f * charge, Effects.ArsenalEffects.ArsenalLaserColor, new Vector2(1f, 0.7f), shrinkSpeed: 1.4f);
                            GeneralParticleHandler.SpawnParticle(chargeParticles);

                            if (i % 2 == 0)
                            {
                                Dust dust = Dust.NewDustPerfect(place, Effects.ArsenalEffects.ArsenalLaserDust, -Projectile.velocity.RotatedBy(0.42f * dir).RotatedByRandom(0.1f) * Main.rand.NextFloat(4, 20), 0, default, Main.rand.NextFloat(0.4f, 0.9f) * charge);
                                dust.noGravity = true;
                                dust.color = Effects.ArsenalEffects.ArsenalLaserColor;
                                dust.alpha = 100;
                            }
                        }

                        bool soundPlaying = (SoundEngine.TryGetActiveSound(StartSlot, out var audio) && audio != null);
                        if (chargeTimer == chargeRate && (!SoundEngine.TryGetActiveSound(ChargeSlot, out var audio3) || !audio3.IsPlaying))
                        {
                            SoundStyle sound = new("CalamityMod/Sounds/Item/PhalanxSurgeChargeLoop");
                            ChargeSlot = SoundEngine.PlaySound(sound with { Volume = 0.7f, IsLooped = true }, Projectile.Center);
                        }
                        if ((!soundPlaying || !audio.IsPlaying) && startChargeLoop)
                        {
                            SoundStyle sound = new("CalamityMod/Sounds/Item/PhalanxSurgeCharge");
                            StartSlot = SoundEngine.PlaySound(sound with { Volume = 0.9f }, Projectile.Center);
                            startChargeLoop = false;
                        }
                        if (chargeTimer == chargeRate) // Reached max charge
                        {
                            SoundStyle sound = new("CalamityMod/Sounds/Item/PhalanxSurgeChargeMax");
                            StartSlot = SoundEngine.PlaySound(sound with { Volume = 0.8f }, Projectile.Center);

                            fullyCharged = true;
                            chargeTimer = 0;
                            OffsetLengthFromArm -= 7f;
                        }
                        if (!fullyCharged)
                            chargeTimer++;
                    }
                    else
                    {
                        if (fullyCharged)
                        {
                            doingChargeAttack = true;
                            int chargeAnimMax = (int)(chargeRate * 0.2f);
                            float completion = (chargeTimer / chargeAnimMax);
                            
                            if (chargeTimer <= chargeAnimMax) // Do the pullback pre fire animation here
                            {
                                float inMin = 10;
                                float outMax = 60;
                                float inOutPoint = 0.5f;
                                if (completion >= inOutPoint)
                                {
                                    float completionLerp = (float)Math.Pow(Utils.GetLerpValue(inOutPoint, 1f, completion, true), 5);
                                    OffsetLengthFromArm = MathHelper.Lerp(inMin, outMax, completionLerp);
                                }
                                else
                                {
                                    float completionLerp = (float)Math.Pow(Utils.GetLerpValue(0f, inOutPoint, completion, true), 3);
                                    OffsetLengthFromArm = MathHelper.Lerp(offsetBase, inMin, completionLerp);
                                }
                            }
                            if (chargeTimer == chargeAnimMax)
                            {
                                Shoot(true);
                                fullyCharged = false;
                                startChargeLoop = true;
                                doingChargeAttack = false;
                                chargeTimer = 0;
                                Projectile.ai[2] = 0; // Return to regular firing mode
                            }
                            chargeTimer++;
                        }
                        else // Failure to full charge
                        {
                            shootingTimer = (int)(-fireRate * 0.5f);
                            chargeTimer = 0;
                            startChargeLoop = true;
                            Projectile.ai[2] = 0; // Return to regular firing mode
                        }
                    }
                }
            }
            else
            {
                if (!Owner.Calamity().mouseRight && !Main.mouseLeft && shootingTimer > 0)
                {
                    StopSounds();
                    Projectile.Kill();
                }
            }

            if (shootingTimer > 0)
                shootingTimer--;

            time++;

            if (SoundEngine.TryGetActiveSound(ChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;
            if (SoundEngine.TryGetActiveSound(StartSlot, out var StartSound) && StartSound.IsPlaying)
            {
                StartSound.Position = Projectile.Center;
            }
            charge = (float)(fullyCharged ? 1 : Math.Pow(chargeTimer / chargeRate, 4));
        }
        public void Shoot(bool isSkewer)
        {
            if (isSkewer)
            {
                if (Main.mouseLeft)
                {
                    SoundStyle sound2 = new("CalamityMod/Sounds/Item/LauncherHeavyShot");
                    SoundEngine.PlaySound(sound2 with { Volume = 0.9f, Pitch = 0.4f }, Projectile.Center);
                    Particle pulse3 = new CustomSpark(Owner.Center, Projectile.velocity * 7, "CalamityMod/Particles/HighResHollowCircleHardEdgeAlt", false, 26, 0.09f, Effects.ArsenalEffects.ArsenalLaserColor, new Vector2(2f, 0.7f), shrinkSpeed: 0.1f);
                    GeneralParticleHandler.SpawnParticle(pulse3);
                    didDash = true;
                }
                
                StopSounds();
                Owner.Calamity().arsenalCooldown = 300;
                Owner.AddCooldown(ArsenalPower.ID, 300);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                if (Main.myPlayer == Projectile.owner)
                {
                    int chargeDamage = (int)(Projectile.damage * (35 + (didDash ? 10 : 0))); // Used to be 50x - 60x, skewed the damage way too much to the lance even with it being the special
                    float chargeKB = Projectile.knockBack * 3f;
                    Projectile skewer = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity * 2, ModContent.ProjectileType<PhalanxSurgeLance>(), chargeDamage, chargeKB, Projectile.owner, 0, 0, (didDash ? 5 : 0));
                    skewer.timeLeft = didDash ? 25 : 10;
                }
                SoundStyle sound = new("CalamityMod/Sounds/Item/PhalanxSurgeChargeShoot");
                SoundEngine.PlaySound(sound with { Volume = 0.9f }, Projectile.Center);
                Owner.SetScreenshake(6f);

                float mult = didDash ? 2 : 1;
                for (int i = 0; i < 20 * mult; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, Effects.ArsenalEffects.ArsenalLaserDust, (shootVelocity * 20).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.2f, 1.2f) * mult, 0, default, Main.rand.NextFloat(0.5f, 1.3f) * mult);
                    dust.noGravity = true;
                    dust.color = Effects.ArsenalEffects.ArsenalLaserColor;
                    dust.alpha = 100;
                }
                
                shootingTimer = didDash ? -25 : -10;
            }
            else
            {
                OffsetLengthFromArm -= 10;
                SoundStyle sound = new("CalamityMod/Sounds/Item/PhalanxSurgeShoot");
                SoundEngine.PlaySound(sound with { Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 6f;
                if (Main.myPlayer == Projectile.owner)
                {
                    float angle1 = MathHelper.ToRadians(2f);
                    int projType = ModContent.ProjectileType<PhalanxSurgeLaser>();
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 corVel1 = shootVelocity.RotatedBy(angle1 * 1.25f) * 0.8f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - shootVelocity * 2, corVel1, projType, Projectile.damage, Projectile.knockBack, Projectile.owner, 1);
                        Vector2 corVel2 = shootVelocity.RotatedBy(angle1 * 0.75f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - shootVelocity * 10, corVel2, projType, Projectile.damage, Projectile.knockBack, Projectile.owner, 0.65f);
                        angle1 *= -1;
                    }
                    float angle2 = MathHelper.ToRadians(5f);
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 corVel3 = shootVelocity.RotatedBy(angle2) * 0.9f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - shootVelocity * 4, corVel3, projType, Projectile.damage, Projectile.knockBack, Projectile.owner, 0.8f);
                        angle2 *= -1;
                    }
                }

                for (int i = 0; i < 9; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, Effects.ArsenalEffects.ArsenalLaserDust, (shootVelocity * 5).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.5f, 0.8f));
                    dust.noGravity = true;
                    dust.color = Effects.ArsenalEffects.ArsenalLaserColor;
                    dust.alpha = 100;
                }

                shootingTimer = fireRate;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(ChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
            if (SoundEngine.TryGetActiveSound(StartSlot, out var StartSound))
                StartSound?.Stop();
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 2)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/PhalanxSurge").Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/PhalanxSurgeGlow").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0, -3);


            float randSize = Main.rand.NextFloat(0.9f, 1f);
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.direction == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/ArchSmear").Value;
            Texture2D pointTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;

            if (charging)
            {
                float rumble = (fullyCharged ? 0 : charge);
                drawPosition += Main.rand.NextVector2Circular(5 * rumble, 5 * rumble);

                for (int i = 0; i < 8; i++)
                {
                    float thin = (1 - i * 0.15f);
                    float fat = (1 + i * 0.3f) * charge;
                    Main.EntitySpriteDraw(rechargeTexture, Projectile.Center - Main.screenPosition + Projectile.velocity * 10, null, Effects.ArsenalEffects.ArsenalLaserColor with { A = 0 } * 0.35f * charge, Projectile.rotation + MathHelper.PiOver2, rechargeTexture.Size() * 0.5f, new Vector2(thin, fat) * 0.3f * randSize, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(rechargeTexture, Projectile.Center - Main.screenPosition + Projectile.velocity * 10, null, Color.White with { A = 0 } * 0.25f * charge, Projectile.rotation + MathHelper.PiOver2, rechargeTexture.Size() * 0.5f, new Vector2(thin, fat) * 0.2f * randSize, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            if (charging)
            {
                for (int i = 0; i < 4; i++)
                {
                    Color auraColor = Color.White with { A = 0 } * 0.6f * charge;
                    Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 48f).ToRotationVector2() * charge;
                    rotationalDrawOffset *= 3;
                    Main.EntitySpriteDraw(glowTexture, drawPosition + rotationalDrawOffset, null, auraColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);
                }
            }
            

            Main.EntitySpriteDraw(glowTexture, drawPosition, null, Color.White, drawRotation, rotationPoint, Projectile.scale, flipSprite);
            return false;
        }
    }
}
