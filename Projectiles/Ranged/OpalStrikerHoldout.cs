using System;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class OpalStrikerHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<OpalStriker>();
        public override float MaxOffsetLengthFromArm => 25f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;

        private ref float CurrentChargingFrames => ref Projectile.ai[0];
        private bool FullyCharged => CurrentChargingFrames >= OpalStriker.FullChargeFrames;
        public SlotId OpalChargeSlot;
        public static float ChargedDamageMult = 5f;
        public static float ChargedKBMult = 3f;
        public static float BulletSpeed = 12f;
        public int time = 0;

        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.HeldItem.type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (SoundEngine.TryGetActiveSound(OpalChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout())
            {
                KeepRefreshingLifetime = false;

                if (Projectile.ai[1] != 1f)
                {
                    Projectile.timeLeft = OpalStriker.AftershotCooldownFrames;

                    ChargeSound?.Stop();
                    SoundEngine.PlaySound(FullyCharged ? OpalStriker.ChargedFire : OpalStriker.Fire, Projectile.Center);

                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                    if (FullyCharged)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<OpalChargedStrike>(), (int)(Projectile.damage * ChargedDamageMult), Projectile.knockBack * ChargedKBMult, Projectile.owner);
                        for (int i = 0; i <= 35; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>(), shootVelocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(1.6f, 2.9f), 0, default, Main.rand.NextFloat(1.6f, 2.5f));
                            dust.noGravity = true;
                            dust.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                            dust.fadeIn = 2.5f;
                            if (Main.rand.NextBool(4))
                            {
                                dust.scale = Main.rand.NextFloat(0.8f, 0.95f);
                                dust.fadeIn = -0.85f;
                                dust.velocity /= 2;
                            }
                        }
                        OffsetLengthFromArm -= 25f;
                        Owner.SetScreenshake(5f);

                        for (int i = 0; i < 2; i++)
                        {
                            Particle spark2 = new CustomSpark(GunTipPosition, Projectile.velocity * 18, "CalamityMod/Particles/BloomCircle", false, 18, 0.65f, Color.OrangeRed, new Vector2(1.2f, 0.8f), true, true, glowOpacity: 0.9f, shrinkSpeed: 0.6f);
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= 5; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>(), shootVelocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.6f, 1.9f), 0, default, Main.rand.NextFloat(1.2f, 2f));
                            dust.noGravity = true;
                            dust.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                            dust.fadeIn = 1.5f;
                        }
                        OffsetLengthFromArm -= 5f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<OpalStrike>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    Projectile.ai[1] = 1f;
                }
            }
            else
            {
                CurrentChargingFrames++;

                // Sounds
                if (FullyCharged)
                {
                    if ((CurrentChargingFrames - OpalStriker.FullChargeFrames) % OpalStriker.ChargeLoopSoundFrames == 0)
                        OpalChargeSlot = SoundEngine.PlaySound(OpalStriker.ChargeLoop, Projectile.Center);
                }
                else if (CurrentChargingFrames == 10)
                    OpalChargeSlot = SoundEngine.PlaySound(OpalStriker.Charge, Projectile.Center);

                // Charge-up visuals
                if (CurrentChargingFrames >= 10)
                {
                    float orbScale = MathHelper.Clamp(CurrentChargingFrames, 0f, OpalStriker.FullChargeFrames) / 200;
                    Lighting.AddLight(GunTipPosition, Color.Orange.ToVector3() * orbScale);
                }

                // Full charge dusts
                if (CurrentChargingFrames == OpalStriker.FullChargeFrames)
                {
                    SoundEngine.PlaySound(OpalStriker.Fire with { Pitch = 0.6f, Volume = 0.7f }, Projectile.Center);
                    for (int i = 0; i < 36; i++)
                    {
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 36f).ToRotationVector2() * Main.rand.NextFloat(7, 8.5f);
                        chargefull.scale = Main.rand.NextFloat(1f, 1.5f);
                        chargefull.noGravity = true;
                        chargefull.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                        chargefull.fadeIn = 1;
                    }

                }
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(OpalChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float chargeScale = KeepRefreshingLifetime ? Utils.GetLerpValue(0, OpalStriker.FullChargeFrames, CurrentChargingFrames, true) : 0;

            if (!Owner.CantUseHoldout())
            {
                float rumble = MathHelper.Clamp(CurrentChargingFrames, 0f, OpalStriker.FullChargeFrames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 30f, rumble / 30f);
            }

            float sine = (float)Math.Sin(time * 0.25f / MathHelper.Pi);
            for (int i = 0; i < 10; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * (3f + sine * 0.3f);
                Main.EntitySpriteDraw(texture, drawPosition + drawOffset * chargeScale, null, Color.OrangeRed with { A = 0 } * Math.Min(chargeScale, 1) * 0.4f, drawRotation + (Owner.gravDir == -1 ? MathHelper.Pi : 0), rotationPoint, Projectile.scale, flipSprite, 0);
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            // Handle glow copies
            SpriteEffects flipSpriteGlow = SpriteEffects.None;
            if (Owner.gravDir == -1f)
            {
                if (Projectile.spriteDirection == -1)
                    flipSpriteGlow = SpriteEffects.FlipVertically;
            }
            else
            {
                rotationPoint.Y = texture.Height - rotationPoint.Y;

                if (Projectile.spriteDirection == 1)
                    flipSpriteGlow = SpriteEffects.FlipVertically;
            }

            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(tex2.Value, GunTipPosition - Main.screenPosition, null, Color.Lerp(FullyCharged ? Color.OrangeRed : Color.Orange, Color.White, i * 0.25f) with { A = 0 } * 0.8f, Main.rand.NextFloat(-5, 5), tex2.Size() * 0.5f, new Vector2(1.35f, 1f) * Projectile.scale * chargeScale * (1 - 0.27f * i) * 0.3f * ((chargeScale >= 1 && CurrentChargingFrames <= OpalStriker.FullChargeFrames + 3) ? 1.75f : FullyCharged ? 1.4f : 1), flipSpriteGlow, 0);

            return false;
        }
    }
}
