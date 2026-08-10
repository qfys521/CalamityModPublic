using System;
using CalamityMod.Buffs.Pets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class DaawnlightSpiritOriginMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";

        private Player Owner => Main.player[Projectile.owner];

        public enum AnimationState { Idle, Pointing }

        public AnimationState CurrentAnimation
        {
            get => (AnimationState)Projectile.ai[0];
            set
            {
                Projectile.ai[0] = (float)value;

                switch (value)
                {
                    case AnimationState.Idle:
                    {
                        _animationFrames = 5;
                        _delayPerAnimationFrame = 9;
                        Projectile.frame = 0;
                        Projectile.frameCounter = 0;
                        break;
                    }

                    case AnimationState.Pointing:
                    {
                        _animationFrames = 9;
                        _delayPerAnimationFrame = 5;
                        Projectile.frame = 0;
                        Projectile.frameCounter = 0;
                        break;
                    }
                }

                Projectile.netUpdate = true;
            }
        }

        private int _animationFrames = 5;

        private int _delayPerAnimationFrame = 9;

        private Vector2 _smoothedBobble;

        public override void SetStaticDefaults() => Main.projFrames[Type] = 9;

        public override void SetDefaults()
        {
            Projectile.width = 138;
            Projectile.height = 218;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }

        public override void OnSpawn(IEntitySource source) => _smoothedBobble = Projectile.Center - Main.screenPosition;

        public override void AI()
        {
            ShouldPetExist();
            DoMovement();
            DoAnimation();
        }

        /// <summary>
        /// A method that is checked every frame to see if the pet should be exist at any given moment.
        /// </summary>
        private void ShouldPetExist()
        {
            // If the owner just disappeared, the pet should vanish.
            // For example if in a multiplayer game, the player disconnected.
            if (!Owner.active)
            {
                Projectile.active = false;
                return;
            }

            // If the owner just died, or doesn't have DSO equipped, or decided to turn off the vanity,
            // the pet should not appear.
            if (Owner.dead || !Owner.Calamity().spiritOrigin && !Owner.Calamity().spiritOriginVanity)
                Owner.Calamity().spiritOriginPet = false;

            // If the pet is not supposed to appear, if there's one, should die immediately.
            if (Owner.Calamity().spiritOriginPet)
                Projectile.timeLeft = 2;
        }

        /// <summary>
        /// A method that contains how the pet is supposed to move around.
        /// </summary>
        private void DoMovement()
        {
            if (Projectile.WithinRange(Owner.Center , 100f))
                Projectile.velocity *= 0.975f;
            else
            {
                float flySpeed = MathHelper.Clamp(11f + Projectile.Distance(Owner.Center) * 0.015f, 11f, 25f);
                Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.SafeDirectionTo(Owner.Center) * flySpeed , flySpeed * 0.02f);
                if (!Projectile.WithinRange(Owner.Center , 2200f))
                {
                    Projectile.Center = Owner.Center;
                    Projectile.velocity = -Vector2.UnitY * 4f;
                }
            }

            if (MathHelper.Distance(Projectile.Center.X , Owner.Center.X) > 80f)
                Projectile.spriteDirection = (Projectile.Center.X > Owner.Center.X).ToDirectionInt();
        }

        /// <summary>
        /// A method that contains how this pet should be animated.
        /// </summary>
        public void DoAnimation()
        {
            if (CurrentAnimation == AnimationState.Idle)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter == _delayPerAnimationFrame)
                {
                    Projectile.frame = (Projectile.frame + 1) % _animationFrames;
                    Projectile.frameCounter = 0;
                }
            }
            else
            {
                if (Projectile.frame == 6 && Projectile.frameCounter != 8)
                {
                    Projectile.frameCounter++;
                    return;
                }

                if (Owner.miscCounter % (Projectile.frame > 5 ? 8 : 4) == 0)
                    Projectile.frame = Math.Min(Projectile.frame + 1 , _animationFrames - 1);

                if (Projectile.frame == _animationFrames - 1)
                    CurrentAnimation = AnimationState.Idle;
            }
        }

        // This pet cannot damage anything and cannot interact with the enviroment (Breaking pots, vines, etc).
        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            if (Owner.FindBuffIndex(ModContent.BuffType<ArcherofLunamoon>()) != -1)
                Owner.ClearBuff(ModContent.BuffType<ArcherofLunamoon>());
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(horizontalFrames: 2, verticalFrames: 9, frameX: (int)CurrentAnimation, frameY: Projectile.frame);

            drawPosition += Vector2.UnitY * CalamityUtils.Convert01To010(Utils.GetLerpValue(0 , _animationFrames - 1 , Projectile.frame , clamped: true)) * 12f;
            _smoothedBobble = Vector2.Lerp(_smoothedBobble , drawPosition , 0.05f);

            Main.EntitySpriteDraw(
                texture ,
                _smoothedBobble ,
                frame ,
                Projectile.GetAlpha(lightColor) ,
                Projectile.rotation ,
                frame.Size() * 0.5f ,
                Projectile.scale ,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            return false;
        }
    }
}
