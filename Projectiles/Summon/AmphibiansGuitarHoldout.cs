using System;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class AmphibiansGuitarHoldout : ModProjectile, ILocalizedModType
    {
        public override string LocalizationCategory => "Projectiles.Summon";

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<AmphibiansGuitar>();

        private Player Owner => Main.player[Projectile.owner];

        private ref float SpawnTimer => ref Projectile.ai[0];

        private static float ArmSwing => MathHelper.PiOver4 + Utils.Remap(MathF.Sin(Main.GlobalTimeWrappedHourly * 6.7f), -1f, 1f, -MathHelper.ToRadians(15f), MathHelper.ToRadians(15f));

        private static readonly int MinionType = ModContent.ProjectileType<AmphibiansGuitarMinion>();

        public override void SetStaticDefaults() => ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 50;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            SpawnTimer++;
            if (SpawnTimer > 19f && Owner.ownedProjectileCounts[MinionType] < 8 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Owner.MountedCenter,
                    Vector2.Zero,
                    MinionType,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    Owner.ownedProjectileCounts[MinionType]);
                SpawnTimer = 0f;
            }

            ManageHoldout();
        }

        private void ManageHoldout()
        {
            if (Owner.CantUseHoldout())
                Projectile.Kill();

            // The center of the player, taking into account if they have a mount or not.
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            // The vector between the player and the mouse, used for pointing the holdout.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - armPosition;

            // The direction this holdout's pointing at.
            float holdoutDirection = Projectile.velocity.ToRotation();

            int direction = MathF.Sign(ownerToMouse.X);

            Vector2 lengthOffset = Projectile.rotation.ToRotationVector2() * 15f;
            Projectile.Center = armPosition + lengthOffset;
            Projectile.velocity = holdoutDirection.AngleTowards(ownerToMouse.ToRotation(), 0.2f).ToRotationVector2();
            Projectile.rotation = holdoutDirection;

            Projectile.spriteDirection = direction;
            Owner.ChangeDir(direction);

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            // -Pi/2 because the arms rotation starts with arms pointing down.
            float armRotation = (Projectile.rotation - MathHelper.PiOver2) * Owner.gravDir + (Owner.gravDir == -1 ? MathHelper.Pi : 0f);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation + ArmSwing * direction);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);

            Projectile.timeLeft = 2;
            Projectile.ForceNetUpdate();
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage() => false;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            return false;
        }
    }
}
