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
    public class ArcNovaDiffuserHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<ArcNovaDiffuser>();
        public override float MaxOffsetLengthFromArm => 24f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;
        public override float RecoilResolveSpeed => (KeepRefreshingLifetime || CurrentChargingFrames < ArcNovaDiffuser.Charge2Frames) ? 0.4f : 0.04f;

        public static int FramesPerLoad = 13;
        public static int MaxLoadableShots = 20;
        public static float BulletSpeed = 12f;
        public SlotId NovaChargeSlot;

        public Vector2 effectSpot;
        public float effectScale = 0;
        public Vector2 effectSpot2;
        public float effectScale2 = 0;
        public Vector2 effectSpot3;
        public float effectScale3 = 0;
        public int time = 0;

        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public ref float ShotsLoaded => ref Projectile.ai[1];
        public ref float ShootRecoilTimer => ref Projectile.ai[2]; // Dual functions for rapid fire shooting cooldown and recoil
        public bool ChargeLV1 => CurrentChargingFrames >= ArcNovaDiffuser.Charge1Frames;
        public bool ChargeLV2 => CurrentChargingFrames >= ArcNovaDiffuser.Charge2Frames;

        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.HeldItem.type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (SoundEngine.TryGetActiveSound(NovaChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout())
            {
                KeepRefreshingLifetime = false;

                // Big Shot mode
                if (ChargeLV2)
                {
                    if (ShotsLoaded > 0)
                    {
                        // Set lifespan to double of remaining cooldown so that it can finish the recoil animation
                        Projectile.timeLeft = ArcNovaDiffuser.AftershotCooldownFrames * 2;

                        ChargeSound?.Stop();
                        SoundEngine.PlaySound(ArcNovaDiffuser.BigShot, Projectile.Center);

                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                        int charge2Damage = (int)(Projectile.damage * 9f);
                        float charge2KB = Projectile.knockBack * 3f;
                        if (Main.myPlayer == Projectile.owner)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<NovaChargedShot>(), charge2Damage, charge2KB, Projectile.owner);
                        Owner.SetScreenshake(6.5f);
                        for (int i = 0; i <= 45; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, ModContent.DustType<SquashDust>(), shootVelocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.6f, 1.9f), 0, default, Main.rand.NextFloat(2f, 3.3f));
                            dust.noGravity = true;
                            dust.color = Main.rand.NextBool(3) ? Color.Lime : ArcNovaDiffuser.mainColor;
                            dust.fadeIn = 2.5f;
                            if (Main.rand.NextBool(4))
                            {
                                dust.scale = Main.rand.NextFloat(0.8f, 0.95f);
                                dust.fadeIn = -0.85f;
                                dust.velocity /= 2;
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            Particle spark2 = new CustomSpark(GunTipPosition, Projectile.velocity * 25, "CalamityMod/Particles/BloomCircle", false, 16, 0.95f, ArcNovaDiffuser.mainColor, new Vector2(1.8f, 0.8f), true, true, glowOpacity: 0.9f, shrinkSpeed: 0.8f);
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                        Particle spark = new CustomSpark(GunTipPosition, Projectile.velocity * 2, "CalamityMod/Particles/BloomRing", false, 12, 0.7f, ArcNovaDiffuser.mainColor, new Vector2(0.5f, 1.5f), true, false, extraRotation: -MathHelper.PiOver2, noShrink: true);
                        GeneralParticleHandler.SpawnParticle(spark);

                        ShotsLoaded = 0;
                        ShootRecoilTimer = Projectile.timeLeft = 55;
                        OffsetLengthFromArm -= 25f;
                    }
                    // Retracting recoil
                    else if (ShootRecoilTimer > 0)
                        ShootRecoilTimer -= 2;
                }
                // Rapid Fire mode
                else if (ShotsLoaded > 0)
                {
                    float lvCompLerp = Math.Min(CurrentChargingFrames / ArcNovaDiffuser.Charge1Frames, 1);

                    // While bullets are remaining, refresh the lifespan; it will not refresh again after bullets run out
                    Projectile.timeLeft = ArcNovaDiffuser.AftershotCooldownFrames;

                    // Retract recoil & shoot faster if charged
                    ShootRecoilTimer -= (ChargeLV1 ? 2.8f : 2f) * MathHelper.Lerp(0.4f, 1f, MathF.Pow(lvCompLerp, 2.5f));

                    if (ShootRecoilTimer <= 0f)
                    {
                        ChargeSound?.Stop();
                        SoundEngine.PlaySound(ArcNovaDiffuser.SmallShot, Projectile.Center);

                        Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                        Vector2 fireVec = shootVelocity.RotatedByRandom(MathHelper.ToRadians(2f));
                        int damage = (int)(Projectile.damage * MathHelper.Lerp(0.5f, 1f, lvCompLerp));
                        if (Main.myPlayer == Projectile.owner)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, fireVec * (ChargeLV1 ? 1 : 0.7f), ModContent.ProjectileType<NovaShot>(), damage, Projectile.knockBack, Projectile.owner);
                        for (int i = 0; i <= 4; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, ModContent.DustType<SquashDust>(), shootVelocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(1.2f, 1.6f), 0, default, Main.rand.NextFloat(0.8f, 1.8f));
                            dust.noGravity = true;
                            dust.color = Main.rand.NextBool(3) ? Color.Lime : ArcNovaDiffuser.mainColor;
                            dust.fadeIn = 1.5f;
                            if (Main.rand.NextBool(6))
                            {
                                dust.scale = Main.rand.NextFloat(0.4f, 0.5f);
                                dust.fadeIn = -0.85f;
                                dust.velocity /= 2;
                            }
                        }

                        ShotsLoaded--;
                        ShootRecoilTimer = (int)MathHelper.Lerp(23, 13, lvCompLerp);
                        OffsetLengthFromArm -= 6f;
                    }
                }
                // Retracting any remaining recoil
                else if (ShootRecoilTimer > 0)
                    ShootRecoilTimer -= 2;
            }
            else
            {
                // Loads shots until maxed out
                if (ShotsLoaded < MaxLoadableShots && CurrentChargingFrames % FramesPerLoad == 0)
                    ShotsLoaded++;

                if (ChargeLV1)
                    CurrentChargingFrames += 2;
                else
                    CurrentChargingFrames++;

                // Sounds
                if (ChargeLV1)
                {
                    // Pulse sounds play independently of the loop
                    if (CurrentChargingFrames == ArcNovaDiffuser.Charge2Frames)
                        SoundEngine.PlaySound(ArcNovaDiffuser.ChargeLV2, Projectile.Center);
                    else if (CurrentChargingFrames == ArcNovaDiffuser.Charge1Frames)
                    {
                        SoundEngine.PlaySound(ArcNovaDiffuser.ChargeLV1, Projectile.Center);
                        ShotsLoaded = MaxLoadableShots;
                    }

                    if ((CurrentChargingFrames - ArcNovaDiffuser.Charge1Frames) % (ArcNovaDiffuser.ChargeLoopSoundFrames * 2) == 0)
                        NovaChargeSlot = SoundEngine.PlaySound(ArcNovaDiffuser.ChargeLoop, Projectile.Center);
                }
                else if (CurrentChargingFrames == 10)
                    NovaChargeSlot = SoundEngine.PlaySound(ArcNovaDiffuser.ChargeStart, Projectile.Center);

                // Charge-up visuals
                if (CurrentChargingFrames >= 10)
                {
                    float particleScale = MathHelper.Clamp(CurrentChargingFrames, 0f, ArcNovaDiffuser.Charge2Frames);
                    float strength = particleScale / 45f;
                    Vector3 DustLight = new Vector3(0.000f, 0.255f, 0.000f);
                    Lighting.AddLight(GunTipPosition, DustLight * strength);
                }

                // Full charge dusts
                if (CurrentChargingFrames == ArcNovaDiffuser.Charge1Frames)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * Main.rand.NextFloat(8, 9.5f);
                        chargefull.scale = Main.rand.NextFloat(1.2f, 1.5f);
                        chargefull.noGravity = true;
                        chargefull.color = Main.rand.NextBool(3) ? Color.Lime : ArcNovaDiffuser.mainColor;
                        chargefull.fadeIn = 1;
                    }
                }
                if (CurrentChargingFrames == ArcNovaDiffuser.Charge2Frames)
                {
                    for (int i = 0; i < 35; i++)
                    {
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 35f).ToRotationVector2() * Main.rand.NextFloat(11, 13.5f);
                        chargefull.scale = Main.rand.NextFloat(1.7f, 1.9f);
                        chargefull.noGravity = true;
                        chargefull.color = Main.rand.NextBool(3) ? Color.Lime : ArcNovaDiffuser.mainColor;
                        chargefull.fadeIn = 1;
                    }
                }
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(NovaChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 tipDrawPosition = GunTipPosition - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color color = ArcNovaDiffuser.mainColor;

            float chargeScale = KeepRefreshingLifetime ? Math.Min(Utils.GetLerpValue(0, ArcNovaDiffuser.Charge1Frames, CurrentChargingFrames, false), 2) : 0;
            if (!Owner.CantUseHoldout())
            {
                float rumble = MathHelper.Clamp(CurrentChargingFrames, 0f, ArcNovaDiffuser.Charge2Frames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 70f, rumble / 70f);
            }

            float sine = (float)Math.Sin(time * 0.15f / MathHelper.Pi);
            for (int i = 0; i < 10; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * (2f + sine * 0.3f);
                Main.EntitySpriteDraw(texture, drawPosition + drawOffset * chargeScale, null, color with { A = 0 } * Math.Min(chargeScale, 1) * 0.3f, drawRotation, rotationPoint, Projectile.scale, flipSprite, 0);
            }
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
            Asset<Texture2D> tex3 = ModContent.Request<Texture2D>("CalamityMod/Particles/ForwardSmear");

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            for (int i = 0; i < 4; i++)
            {
                Vector2 place = Vector2.One.RotateRandom(MathHelper.TwoPi) * Main.rand.NextFloat(1, 5.5f) * Math.Min(chargeScale, 1);
                Vector2 vel = Projectile.velocity.RotatedByRandom(0.3f);
                Main.EntitySpriteDraw(tex3.Value, tipDrawPosition + place - Vector2.Lerp(place, -Projectile.velocity, 0.9f) * Main.rand.NextFloat(15, 35) + Projectile.velocity * -8 * (6 - chargeScale * 2), null, (Main.rand.NextBool(3) ? Color.Lime : color) with { A = 0 } * 0.8f * Math.Min(chargeScale, 1), vel.ToRotation() - MathHelper.PiOver2, new Vector2(tex3.Width() / 2, tex3.Height()), new Vector2(0.25f * chargeScale, (1.7f + (Main.rand.NextBool(4) ? 1.8f : 0))) * Projectile.scale * 0.05f * Math.Min(chargeScale, 1), SpriteEffects.None, 0);
            }
            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(tex2.Value, tipDrawPosition, null, Color.Lerp(color, Color.White, i * 0.25f) with { A = 0 } * 0.8f, Main.rand.NextFloat(-5, 5), tex2.Size() * 0.5f, new Vector2(1.35f, 1f) * Projectile.scale * chargeScale * (1 - 0.27f * i) * 0.23f, SpriteEffects.None, 0);
            if (CurrentChargingFrames >= ArcNovaDiffuser.Charge2Frames)
            {
                for (int y = 0; y < 3; y++)
                {
                    Vector2 angleVel = (MathHelper.TwoPi * y / 3f).ToRotationVector2().RotatedBy(time * 0.18f);
                    Vector2 drawOffset = (new Vector2(angleVel.X * 0.7f, angleVel.Y * 1.2f)).RotatedBy(Projectile.rotation) * chargeScale * 12;
                    for (int i = 0; i < 2; i++)
                        Main.EntitySpriteDraw(tex2.Value, tipDrawPosition + drawOffset, null, Color.Lerp(color, Color.White, i) with { A = 0 } * 0.8f, Main.rand.NextFloat(-5, 5), tex2.Size() * 0.5f, new Vector2(1f, 1f) * Projectile.scale * chargeScale * (1 - 0.25f * i) * 0.085f * (CurrentChargingFrames <= ArcNovaDiffuser.Charge2Frames + 3 ? 3 : 1), SpriteEffects.None, 0);
                }
            }
            return false;
        }
    }
}
