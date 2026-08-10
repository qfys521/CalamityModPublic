using System.IO;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonHoldout : BaseGunHoldoutProjectile
    {
        #region Fields & Properties

        private ref float Timer => ref Projectile.ai[0];

        private bool HasShotBeam
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value == true ? 1f : 0f;
        }

        private enum AIState { Level0, Level1, Level2, Level3 };

        private const float TimePerCharge = 60f;
        private float ChargeLV1 => TimePerCharge;
        private float ChargeLV2 => TimePerCharge * 2;
        private float ChargeLV3 => TimePerCharge * 3;

        private AIState State => Timer switch
        {
            >= TimePerCharge * 3 => AIState.Level3,
            >= TimePerCharge * 2 => AIState.Level2,
            >= TimePerCharge => AIState.Level1,
            _ => AIState.Level0,
        };

        private const int CoolingDownTime = 40;

        public bool hasReachedLV1 = false;
        public bool hasReachedLV2 = false;
        public bool hasReachedLV3 = false;

        public Color color1 = Color.MediumTurquoise;
        public Color color2 = Color.Coral;

        #region Overriden Base Holdout Members

        public override int AssociatedItemID => ItemType<TauCannon>();
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitY.RotatedBy(Projectile.rotation) * 3f * Projectile.spriteDirection;
        public override float WeaponTurnSpeed => HasShotBeam ? Utils.Remap(Timer, 0f, TimePerCharge * 2f, 0.4f, 0.018f, true) : 0.4f;
        public override float MaxOffsetLengthFromArm => 30f;
        public override float OffsetXUpwards => -15f;
        public override float OffsetXDownwards => 5f;
        public override float BaseOffsetY => -15f;
        public override float OffsetYUpwards => 10f;
        public override float OffsetYDownwards => 15f;

        #endregion

        #region SoundStyles & SlotsIDs

        public static readonly SoundStyle ChargeLV1Sound = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV1") { Volume = 0.6f };
        public static readonly SoundStyle ChargeLV2Sound = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV2") { Volume = 0.6f };
        public static readonly SoundStyle OrbSound = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLoop");
        public static readonly SoundStyle BoltShootSound = new("CalamityMod/Sounds/Custom/ExoMechs/ExoLaserShoot") { Volume = 0.1f, PitchVariance = 0.3f };
        public static readonly SoundStyle SmallBeamSound = new("CalamityMod/Sounds/Custom/AstrumDeus/AstrumDeusMine") { Volume = 0.2f, PitchVariance = 0.1f };
        public static readonly SoundStyle BigBeamSound = new("CalamityMod/Sounds/Item/PhotoUseSound");
        public static readonly SoundStyle CoolingDownSound = new("CalamityMod/Sounds/Custom/ExoMechs/ThanatosVent") { Volume = 0.025f };
        private SlotId OrbSoundSlot;
        private SlotId CoolingDownSoundSlot;

        #endregion 

        #endregion

        public override void KillHoldoutLogic()
        {
            // If the holdout hasn't reached any charge and the player's isn't holding the weapon anymore: kill it.
            if (State == AIState.Level0 && Owner.CantUseHoldout())
                Projectile.Kill();

            // At all times, don't allow the holdout to persist if the player dies or is no longer active.
            if (!Owner.active || Owner.dead)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            CalamityGlobalItem modItem = Owner.HeldItem.Calamity();

            if (Owner.CantUseHoldout() && KeepRefreshingLifetime)
            {
                KeepRefreshingLifetime = false;
                Projectile.timeLeft = State switch
                {
                    AIState.Level1 => 54 + CoolingDownTime + 1,
                    AIState.Level2 => 30 + CoolingDownTime + 1,
                    AIState.Level3 => 95 + CoolingDownTime + 1,
                    _ => 0,
                };
                Projectile.netUpdate = true;
            }

            if (KeepRefreshingLifetime == false && Main.myPlayer == Projectile.owner)
            {
                switch (State)
                {
                    case AIState.Level1:

                        if (Projectile.timeLeft % 4 == 0 && Projectile.timeLeft > CoolingDownTime && Main.myPlayer == Projectile.owner)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                GunTipPosition,
                                Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.25f) * HeldItem.shootSpeed,
                                ProjectileType<TauCannonBolt>(),
                                Projectile.damage,
                                Projectile.knockBack,
                                Projectile.owner);
                            OffsetLengthFromArm -= 5f;
                            SoundEngine.PlaySound(BoltShootSound with { Volume = 0.3f, Pitch = 0.8f }, GunTipPosition);
                        }

                        if (!HasShotBeam)
                        {
                            HasShotBeam = true;
                        }

                        break;

                    case AIState.Level2:

                        if (!HasShotBeam && Main.myPlayer == Projectile.owner)
                        {
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                GunTipPosition,
                                Projectile.velocity,
                                ProjectileType<TauCannonBeam>(),
                                (int)(Projectile.damage * 13f),
                                Projectile.knockBack * 3,
                                Projectile.owner,
                                ai1: Projectile.whoAmI);
                            OffsetLengthFromArm -= 25f;
                            HasShotBeam = true;
                            SoundEngine.PlaySound(SmallBeamSound, GunTipPosition);
                        }

                        break;

                    case AIState.Level3:

                        if (!HasShotBeam)
                        {
                            if (Projectile.owner == Main.myPlayer)
                            {
                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    GunTipPosition + Projectile.velocity * 25f,
                                    Projectile.velocity,
                                    ProjectileType<TauCannonBeam>(),
                                    (int)(Projectile.damage * 0.7f),
                                    Projectile.knockBack * 5,
                                    Projectile.owner,
                                    ai1: Projectile.whoAmI,
                                    ai2: 1f);

                                int randomPortalAmount = Main.rand.Next(4, 5+1);
                                for (int i = 0; i < randomPortalAmount; i++)
                                {
                                    Projectile.NewProjectile(
                                        Projectile.GetSource_FromThis(),
                                        Owner.Center + Main.rand.NextVector2CircularEdge(480f, 480f) * Main.rand.NextFloat(0.8f, 1.2f),
                                        Vector2.Zero,
                                        ProjectileType<TauCannonPortal>(),
                                        Projectile.damage,
                                        Projectile.knockBack,
                                        Projectile.owner);
                                }
                            }

                            HasShotBeam = true;
                        }

                        if (Projectile.timeLeft > CoolingDownTime)
                        {
                            if (Projectile.timeLeft % 6f == 0f)
                            {
                                Particle ring = new DirectionalPulseRing(GunTipPosition - Projectile.velocity * 10f, Projectile.velocity * 5f, Main.rand.NextBool(3) ? color2 : color1, new Vector2(0.5f, 1f), Projectile.velocity.ToRotation(), 0.1f, 0.6f, 15);
                                GeneralParticleHandler.SpawnParticle(ring);
                            }

                            if (Main.rand.NextBool(2))
                            {
                                Particle smoke = new HeavySmokeParticle(GunTipPosition, Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(4f, 9f), Main.rand.NextBool(3) ? color2 : color1, 17, Main.rand.NextFloat(0.9f, 1.7f), 0.7f, 0, true);
                                GeneralParticleHandler.SpawnParticle(smoke);
                            }
                        }

                        break;
                }
            }

            if (Timer == ChargeLV1 && !hasReachedLV1 || Timer == ChargeLV2 && !hasReachedLV2 || Timer == ChargeLV3 && !hasReachedLV3)
            {
                if (Timer == ChargeLV1 && !hasReachedLV1)
                {
                    PerLevelChargeEffect(State);
                    hasReachedLV1 = true;
                }
                if (Timer == ChargeLV2 && !hasReachedLV2)
                {
                    PerLevelChargeEffect(State);
                    hasReachedLV2 = true;
                }
                if (Timer == ChargeLV3 && !hasReachedLV3)
                {
                    PerLevelChargeEffect(State);
                    hasReachedLV3 = true;
                }
            }

            if (Projectile.timeLeft < CoolingDownTime && !KeepRefreshingLifetime && Timer > TimePerCharge)
            {
                if (Projectile.timeLeft % 3 == 0)
                {
                    Particle heat = new MediumMistParticle(
                        Projectile.Center - Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.4f,
                        -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(5f, 12f),
                        Color.DarkGray,
                        Color.Transparent,
                        Main.rand.NextFloat(0.6f, 0.9f),
                        Main.rand.NextFloat(300f, 400f));
                    GeneralParticleHandler.SpawnParticle(heat);
                }
                SoundStyle steam = new("CalamityMod/Sounds/Item/HeliumFlashSteamRelease");
                if (!SoundEngine.TryGetActiveSound(CoolingDownSoundSlot, out var sound))
                    CoolingDownSoundSlot = SoundEngine.PlaySound(steam with { Volume = 0.5f, Pitch = -0.5f }, GunTipPosition);
                else
                    sound.Position = Projectile.Center;
            }

            if (KeepRefreshingLifetime)
            {
                if (Main.rand.NextBool(4))
                {
                    float randomRadius = Utils.Remap(Timer, 0f, TimePerCharge * 3f, 5f, 15f);
                    Dust orbDust = Dust.NewDustPerfect(
                        GunTipPosition + Main.rand.NextVector2Circular(randomRadius, randomRadius),
                        DustID.FireworksRGB,
                        -Vector2.UnitY.RotatedByRandom(100) * Main.rand.NextFloat(2f, 5f) * Utils.Remap(Timer, 0f, TimePerCharge * 3f, 1f, 5f),
                        Scale: Main.rand.NextFloat(0.3f, 0.9f));
                    orbDust.noGravity = true;
                    orbDust.color = Main.rand.NextBool(3) ? color2 : color1;
                }

                Particle orb = new GenericBloom(GunTipPosition, Projectile.velocity, color1, Utils.Remap(Timer, 0f, TimePerCharge * 3f, 0f, 1f), 2, false);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new GenericBloom(GunTipPosition, Projectile.velocity, Color.White, Utils.Remap(Timer, 0f, TimePerCharge * 3f, 0f, 0.8f), 2, false);
                GeneralParticleHandler.SpawnParticle(orb2);

                float distanceFromTip = Utils.Remap(Timer, 0f, TimePerCharge * 3f, 25f, 300f);
                float sizeIncrease = Utils.Remap(Timer, 0f, TimePerCharge * 3f, 0.05f, 0.4f);
                Particle lineCharge = new ManaDrainStreak(Owner, sizeIncrease, Main.rand.NextVector2Circular(distanceFromTip, distanceFromTip), Main.rand.NextFloat(5f, 10f), Color.White, Main.rand.NextBool(3) ? color2 : color1, Main.rand.Next(10, 21), GunTipPosition);
                GeneralParticleHandler.SpawnParticle(lineCharge);

                if (!SoundEngine.TryGetActiveSound(OrbSoundSlot, out var sound))
                    OrbSoundSlot = SoundEngine.PlaySound(OrbSound with { Volume = 0.01f, Pitch = -1, IsLooped = true }, GunTipPosition);
                else
                {
                    sound.Position = Projectile.Center;
                    sound.Volume = Utils.GetLerpValue(0, ChargeLV2, Timer, true) * 100;
                    sound.Pitch = 1 * Utils.GetLerpValue(0, ChargeLV2, Timer, true);
                }
            }

            if (KeepRefreshingLifetime == true)
                Timer++;

            if (Projectile.timeLeft <= 1 || (KeepRefreshingLifetime == false))
            {
                if (SoundEngine.TryGetActiveSound(OrbSoundSlot, out var ChargeSound))
                    ChargeSound?.Stop();
            }
        }

        private void PerLevelChargeEffect(AIState state)
        {
            int dustAmount = state switch
            {
                AIState.Level3 => 36,
                AIState.Level2 => 24,
                AIState.Level1 => 12,
                _ => 0
            };

            for (int i = 0; i < dustAmount; i++)
            {
                float angle = MathHelper.TwoPi / dustAmount * i;
                Vector2 velocity = angle.ToRotationVector2() * 8f;
                Dust chargeDust = Dust.NewDustPerfect(GunTipPosition, DustID.RainbowMk2, velocity, Scale: Utils.Remap(dustAmount, 10f, 30f, 1.5f, 2.2f));
                chargeDust.noGravity = true;
                chargeDust.noLight = true;
                chargeDust.noLightEmittance = true;
                chargeDust.color = Main.rand.NextBool(3) ? color2 : color1;
            }

            Particle chargeRing = new DirectionalPulseRing(
                GunTipPosition,
                Vector2.Zero,
                color1,
                Vector2.One,
                0f,
                1f,
                0f,
                30);
            GeneralParticleHandler.SpawnParticle(chargeRing);

            SoundStyle chargeSound = state switch
            {
                AIState.Level3 => ChargeLV2Sound,
                AIState.Level2 => ChargeLV1Sound,
                AIState.Level1 => ChargeLV1Sound,
                _ => ChargeLV1Sound
            };

            SoundEngine.PlaySound(chargeSound, GunTipPosition);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Timer < 2)
                return false;
            Texture2D texture = Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value;
            Texture2D glowTexture = Request<Texture2D>(GlowTexture, AssetRequestMode.AsyncLoad).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color glowDrawColor = Color.Lerp(Color.Black, Color.White, Utils.GetLerpValue(0f, TimePerCharge * 3f, Timer, true));
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (Owner.gravDir == -1f)
            {
                flipSprite |= SpriteEffects.FlipVertically;
            }

            if (Projectile.timeLeft > CoolingDownTime || KeepRefreshingLifetime == true)
            {
                float shake = KeepRefreshingLifetime ? Utils.Remap(Timer, TimePerCharge, TimePerCharge * 3f, 0f, 4f) : 3f;
                drawPosition += Main.rand.NextVector2Circular(shake, shake);
            }
            if (Timer >= ChargeLV2 && KeepRefreshingLifetime == true)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(texture, drawPosition + Main.rand.NextVector2Circular(12, 12) * Utils.GetLerpValue(ChargeLV2, ChargeLV3, Timer, true), null, color1 with { A = 0 } * Utils.GetLerpValue(ChargeLV2, ChargeLV3, Timer, true), drawRotation, rotationPoint, Projectile.scale, flipSprite);
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale, flipSprite);
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, glowDrawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            return false;
        }

        public override void SendExtraAIHoldout(BinaryWriter writer) => writer.Write7BitEncodedInt(Projectile.timeLeft);

        public override void ReceiveExtraAIHoldout(BinaryReader reader) => Projectile.timeLeft = reader.Read7BitEncodedInt();
    }
}
