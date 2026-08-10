using System;
using System.IO;
using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using static CalamityMod.Items.Weapons.Summon.EnchantedKnifeStaff;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Summon
{
    public class EnchantedKnifeSummon : BaseMinionProjectile
    {
        public override int AssociatedProjectileTypeID => ProjectileType<EnchantedKnifeSummon>();

        public override int AssociatedBuffTypeID => BuffType<EnchantedKnifeStaffBuff>();

        public override ref bool AssociatedMinionBool => ref ModdedOwner.EnchantedKnifeStaffBool;

        public override bool PreHardmodeMinionTileVision => true;

        private Action CurrentState
        {
            get => _currentState;
            set
            {
                OnStateChange(value);
                _currentState = value;
            }
        }
        private Action _currentState;


        private ref float SwingTimer => ref Projectile.ai[1];

        private ref float SwingDirection => ref Projectile.ai[2];

        private Vector2 IdlePosition => Owner.MountedCenter +
            new Vector2(50f * MathF.Ceiling(Projectile.minionPos / 2f) * (Projectile.minionPos % 2 == 0 ? -1 : 1), -60f + MathF.Ceiling(Projectile.minionPos / 2f) * 15f);

        private Vector2 SwingCenter;

        private float RandomRotationOffset;

        private bool HasStartedSwinging;

        private bool HasSpawned;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = Projectile.height = 32;
        }

        public override bool? CanDamage() => SwingTimer >= SwingWait && SwingTimer <= SwingWait + SwingTime ? base.CanDamage() : false;
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float damageMult = MathHelper.Clamp(Utils.GetLerpValue(3, 0, Projectile.numHits), 0.25f, 1);
            modifiers.SourceDamage *= damageMult;
        }

        #region AI

        public override void MinionAI()
        {
            if (!HasSpawned)
            {
                IFrames = -1;
                TrailCacheLength = 4;
                Projectile.velocity = Projectile.DirectionFrom(Owner.Center);
                CurrentState = GoToOwnerState;
                HasSpawned = true;
                NetUpdate();
            }
            if (Main.rand.NextBool(6))
            {
                Vector2 vel = new Vector2(2, 2).RotatedByRandom(100);
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center + vel * Main.rand.NextFloat(0.5f, 3f), Main.rand.NextBool() ? Main.rand.NextBool() ? 57 : 58 : 15, vel * Main.rand.NextFloat(0.5f, 1f));
                trailDust.noGravity = true;
                trailDust.scale = Main.rand.NextFloat(0.6f, 1.1f);
            }

            CurrentState.Invoke();

            Projectile.MinionAntiClump(0.25f);

            if (!Main.dedServ)
                Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.4f);
        }

        private void GoToOwnerState()
        {
            if (Target is not null)
            {
                CurrentState = AttackState;
                return;
            }

            if (Projectile.DistanceSQ(IdlePosition) > 3000f * 3000f)
                Projectile.Center = Owner.Center;

            if (Projectile.DistanceSQ(IdlePosition) < 80f * 80f)
                CurrentState = IdleState;

            Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.DirectionTo(IdlePosition).ToRotation(), 0.08f).ToRotationVector2() * 10f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        private void IdleState()
        {
            if (Target is not null)
            {
                CurrentState = AttackState;
                return;
            }

            if (Projectile.DistanceSQ(IdlePosition) > 160f * 160f)
                CurrentState = GoToOwnerState;

            float speed = Utils.Remap(Projectile.DistanceSQ(IdlePosition), 0f, 160f * 160f, 0.25f, 15f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.velocity.ToRotation().AngleTowards(Projectile.DirectionTo(IdlePosition).ToRotation(), 0.4f).ToRotationVector2() * speed, 0.2f);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.ToRadians(RandomSineFunction(Main.GlobalTimeWrappedHourly + RandomRotationOffset) * 3f) - MathHelper.PiOver2, 0.1f);
        }

        private void AttackState()
        {
            if (Target is null)
            {
                CurrentState = GoToOwnerState;
                return;
            }

            if (float.IsNaN(SwingCenter.X) || float.IsNaN(SwingCenter.Y) || float.IsNaN(Projectile.velocity.X) || float.IsNaN(Projectile.velocity.Y))
            {
                SwingCenter = Target.Center;
                Projectile.velocity = Vector2.Zero;
            }

            SwingCenter += Projectile.velocity;
            Projectile.velocity *= 0.9f;

            var easing = new CalamityUtils.CurveSegment(CalamityUtils.PolyInOutEasing, 0f, 0f, 1f, 3);
            float xDifference = Target.Center.X - SwingCenter.X;
            int signDirection = float.IsNaN(xDifference) ? 1 : MathF.Sign(xDifference);
            float swingRotation = MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, CalamityUtils.PiecewiseAnimation(Utils.GetLerpValue(SwingWait, SwingWait + SwingTime, SwingTimer, true), easing)) * signDirection;
            Vector2 predictiveDirection = CalamityUtils.CalculatePredictiveAimToTarget(SwingCenter, Target, ProjectileSpeed);
            
            if (float.IsNaN(predictiveDirection.X) || float.IsNaN(predictiveDirection.Y))
                predictiveDirection = Projectile.DirectionTo(Target.Center);

            Projectile.Center = SwingCenter + (predictiveDirection.ToRotation() + swingRotation).ToRotationVector2() * 40f;
            Projectile.rotation = Projectile.DirectionFrom(SwingCenter).ToRotation();

            if (SwingTimer == SwingWait + SwingTime * 0.5f && Main.myPlayer == Projectile.owner)
            {
                Vector2 spawnPosition = SwingCenter + predictiveDirection.SafeNormalize(-Vector2.UnitY) * 40f;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    predictiveDirection,
                    ProjectileType<EnchantedKnifeStaffProjectile>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner);
            }
            if ((SwingTimer == SwingWait && SwingDirection == 1) || (SwingTimer == SwingWait + SwingTime && SwingDirection == -1))
            {
                Projectile.ResetLocalNPCHitImmunity();
                Projectile.numHits = 0;
            }                

            if (Vector2.Distance(SwingCenter, Target.Center) > 48f + MathF.Min(Target.width, Target.height) * 0.8f)
            {
                Vector2 dashVelocity = CalamityUtils.CalculatePredictiveAimToTarget(SwingCenter, Target, ChaseSpeed);
                if (!float.IsNaN(dashVelocity.X) && !float.IsNaN(dashVelocity.Y))
                    Projectile.velocity = dashVelocity;
            }
            else if (Vector2.Distance(SwingCenter, Target.Center) < 40f + MathF.Min(Target.width, Target.height) * 0.5f)
            {
                Vector2 dashVelocity = CalamityUtils.CalculatePredictiveAimToTarget(SwingCenter, Target, RecoilSpeed);
                if (!float.IsNaN(dashVelocity.X) && !float.IsNaN(dashVelocity.Y))
                    Projectile.velocity = dashVelocity;
            }
            else
                Projectile.velocity *= 0.5f;

            // Cancel out choppy movement
            if (Projectile.velocity.Length() < 1f)
                Projectile.velocity *= 0f;

            SwingTimer += 1f * SwingDirection;
            if (SwingTimer == 0f || SwingTimer == SwingWait + SwingTime + SwingWait)
            {
                if (HasStartedSwinging)
                    SwingDirection *= -1f;
                else
                    HasStartedSwinging = true;
            }
        }

        private void OnStateChange(Action newState)
        {
            if (newState == IdleState)
            {
                RandomRotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                NetUpdate();
            }

            if (newState == AttackState)
            {
                if (Target is null)
                    return;

                float targetSize = Target.Size.Length();
                Vector2 previousCenter = Projectile.Center;
                Vector2 newCenter = Target.Center + Main.rand.NextVector2CircularEdge(targetSize * 0.5f + 160f, targetSize * 0.5f + 160f);

                int maxAttempts = 10;
                int attempts = 0;
                while (Main.tile[newCenter.ToSafeTileCoordinates()].IsTileSolid() && attempts < maxAttempts)
                {
                    newCenter = Target.Center + Main.rand.NextVector2CircularEdge(targetSize * 0.5f + 160f, targetSize * 0.5f + 160f);
                    attempts++;
                }

                if (float.IsNaN(newCenter.X) || float.IsNaN(newCenter.Y))
                    newCenter = Target.Center;

                Projectile.Center = newCenter;
                SwingCenter = Projectile.Center;

                HasStartedSwinging = false;
                SwingDirection = Main.rand.NextBool().ToDirectionInt();
                int totalSwingTime = (int)(2f * SwingWait + SwingTime);
                SwingTimer = SwingDirection == 1 ? Main.rand.Next(-30, -2) : Main.rand.Next(totalSwingTime + 1, totalSwingTime + 30 + 1);

                NetUpdate();

                if (!Main.dedServ)
                {
                    int dustAmount = Main.rand.Next(30, 40);
                    for (int i = 0; i < dustAmount; i++)
                    {
                        float angle = MathHelper.TwoPi / dustAmount * i;
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f);
                        Dust tpDustStart = Dust.NewDustPerfect(previousCenter, DustID.ShimmerSpark, velocity);
                        Dust tpDustEnd = Dust.NewDustPerfect(SwingCenter, DustID.ShimmerSpark, velocity);
                        tpDustStart.noGravity = true;
                        tpDustEnd.noGravity = true;
                    }
                }
            }
        }

        private float RandomSineFunction(float x) => MathF.Sin(3f * x) + MathF.Cos(5f * x) + 2f * MathF.Sin(0.5f * x);

        #endregion

        #region Drawing

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + MathHelper.PiOver2;
            Vector2 anchorPoint = texture.Size() * 0.5f;

            if (CurrentState == AttackState)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 afterimageDrawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                    Color afterimageDrawColor = Color.Cyan * ((4f - i) / 4f) * 0.8f;
                    float afterimageDrawRotation = Projectile.oldRot[i] + MathHelper.PiOver2;
                    Main.EntitySpriteDraw(texture, afterimageDrawPosition, null, afterimageDrawColor, afterimageDrawRotation, anchorPoint, Projectile.scale, SpriteEffects.None);
                }
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, anchorPoint, Projectile.scale, SpriteEffects.None);

            return false;
        }

        #endregion

        #region Syncing

        private void NetUpdate(int netSpam = 0)
        {
            Projectile.netUpdate = true;
            Projectile.netSpam = netSpam;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(SwingCenter);

        public override void ReceiveExtraAI(BinaryReader reader) => SwingCenter = reader.ReadVector2();

        #endregion
    }
}
