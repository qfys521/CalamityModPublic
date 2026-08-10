using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Summon.SquirrelSquireStaff;

namespace CalamityMod.Projectiles.Summon
{
    public class SquirrelSquireMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        private enum AIState { Idle, Attack }

        private AIState State
        {
            get => (AIState)Projectile.ai[0];
            set
            {
                Projectile.ai[0] = (float)value;
                Projectile.ForceNetUpdate();
            }
        }

        private bool OnSpawnCheck
        {
            get => Projectile.ai[2] == 1f;
            set => Projectile.ai[2] = value == true ? 1f : 0f;
        }

        private Player Owner;

        private NPC Target;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 14;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;

            Projectile.width = 40;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.sentry = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            if (!OnSpawnCheck)
            {
                ActualOnSpawn();
                OnSpawnCheck = true;
            }

            SetTarget();
            ApplyGravity();
            DoAnimation();

            switch (State)
            {
                case AIState.Idle:
                    IdleBehavior();
                    break;
                case AIState.Attack:
                    AttackBehavior();
                    break;
            }
        }

        #region AI Methods

        private void ActualOnSpawn()
        {
            Owner = Main.player[Projectile.owner];
            Projectile.spriteDirection = Main.rand.NextBool().ToDirectionInt();
        }

        private void SetTarget() => Target = Projectile.Center.MinionHoming(960f, Owner, false);

        private void ApplyGravity()
        {
            float speedY = Projectile.velocity.Y;
            if (speedY < 20f)
                speedY = MathF.Min(speedY + 0.4f, 20f);
            Projectile.velocity.Y = speedY;
        }

        private void DoAnimation()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 7;
            }
        }

        private void IdleBehavior()
        {
            if (Target is not null)
            {
                State = AIState.Attack;
                return;
            }

            if (Projectile.Distance(Owner.Center) < 60f)
                Projectile.spriteDirection = MathF.Sign(Owner.Center.X - Projectile.Center.X);
        }

        private void AttackBehavior()
        {
            if (Target is null)
            {
                State = AIState.Idle;
                return;
            }

            if (Projectile.frame == 0 && Projectile.frameCounter == 0 && Main.myPlayer == Projectile.owner)
            {
                Vector2 spawnPosition = Projectile.spriteDirection == -1 ? Projectile.Left : Projectile.Right;
                Vector2 shootVelocity = CalamityUtils.CalculatePredictiveAimToTarget(spawnPosition, Target, ProjectileVelocity);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    shootVelocity,
                    ModContent.ProjectileType<SquirrelSquireAcorn>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner);
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.5f, Pitch = 0.8f, PitchVariance = 0.1f }, spawnPosition);
                Projectile.ForceNetUpdate();
            }

            Projectile.spriteDirection = MathF.Sign(Target.Center.X - Projectile.Center.X);
        }
        #endregion

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Top - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
            Rectangle frame = texture.Frame(2, 7, (int)State, Projectile.frame);
            Color drawColor = Projectile.GetAlpha(lightColor);
            Vector2 anchorPoint = new(frame.Size().X * 0.5f, 0f);
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, frame, drawColor, Projectile.rotation, anchorPoint, Projectile.scale, flipSprite);

            return false;
        }
    }
}
