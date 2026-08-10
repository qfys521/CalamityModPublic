using System;
using CalamityMod.Dusts;
using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class SlagfireDouserHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<SlagfireDouser>();

        private static readonly Vector2 CustomHoldoutOffset = new Vector2(25f, -5f);

        public static Asset<Texture2D> pistilTexture;

        public override string Texture => "CalamityMod/Projectiles/Ranged/SlagfireDouserHoldout";
        public static string TexturePathPistil => "CalamityMod/Projectiles/Ranged/SlagfireDouserPistil";
        private float pistilJigglePhysicsTimer = 0;

        // Positioning overrides
        public override Vector2 GunTipPosition
        {
            get
            {
                Vector2 baseTip = base.GunTipPosition;

                baseTip.X += CustomHoldoutOffset.X * Owner.direction;
                baseTip.Y += CustomHoldoutOffset.Y;

                baseTip -= Vector2.UnitX.RotatedBy(Projectile.rotation) * 24f * Owner.gravDir;

                return baseTip;
            }
        }
        public override float MaxOffsetLengthFromArm => 12f;
        public override float OffsetXUpwards => -25f;
        public override float OffsetXDownwards => -25f;
        public override float OffsetYUpwards => -24f;
        public override float OffsetYDownwards => 24f;
        public override float BaseOffsetY => -2f;

        public override float RecoilResolveSpeed => 0.12f;
        private float frontArmRotation = 0.14f;
        public ref float ShootingTimer => ref Projectile.ai[0];

        private const int BurstProjectiles = 4; // 4 Slagfire shots per burst
        private const int DelayBetweenShotsInBurst = 4;

        public static Color EffectsColor { get; set; } = Color.MediumVioletRed * 1.3f;
        public static Color StaticEffectsColor { get; set; } = Color.MediumVioletRed * 1.3f;

        public override void HoldoutAI()
        {
            if (!Owner.channel)
            {
                Projectile.Kill();
                return;
            }
            if (ShootingTimer % (HeldItem.useAnimation + BurstProjectiles * DelayBetweenShotsInBurst) == 0 && ShootingTimer > 0)
            {
                ShootingTimer = 0;
            }

            // Calc current shot index
            int currentShotInBurst = (int)((ShootingTimer % (HeldItem.useAnimation + BurstProjectiles * DelayBetweenShotsInBurst) - HeldItem.useAnimation) / DelayBetweenShotsInBurst);

            if (ShootingTimer >= HeldItem.useAnimation && currentShotInBurst >= 0 && currentShotInBurst < BurstProjectiles)
            {
                if ((ShootingTimer - HeldItem.useAnimation) % DelayBetweenShotsInBurst == 0)
                {
                    Shoot(HeldItem);
                }
            }

            ShootingTimer++; // Once per tick

            pistilJigglePhysicsTimer = MathHelper.Clamp(pistilJigglePhysicsTimer - 0.04f, 0, 1);

            // Set front arm to be angled slightly up toward the trigger
            ExtraFrontArmRotation = frontArmRotation;
        }

        public void Shoot(Item item)
        {
            // Create random spread 
            Vector2 projectileDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 finalProjectileVelocity = projectileDirection.RotatedByRandom(MathHelper.ToRadians(11f)) * item.shootSpeed;

            // Spawn the main projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, finalProjectileVelocity, ModContent.ProjectileType<Slagfire>(), Owner.GetWeaponDamage(item), Owner.GetWeaponKnockback(item, item.knockBack), Projectile.owner);

            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(SoundID.Item61 with { Volume = 0.9f, Pitch = 0.125f }, Projectile.Center);

            // Translates arm back and upward to simulate recoil
            OffsetLengthFromArm -= 1f;

            pistilJigglePhysicsTimer += 1;


            int dustAmount = Main.rand.Next(3, 6);
            for (int i = 0; i < dustAmount; i++)
            {
                Dust shootDust = Dust.NewDustPerfect(
                GunTipPosition,
                ModContent.DustType<LightDust>(),
                projectileDirection.RotatedByRandom(MathHelper.PiOver4 * 1.2f) * Main.rand.NextFloat(2.5f, 9f),
                newColor: Color.Red);
                shootDust.noGravity = false;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            pistilTexture ??= Request<Texture2D>(TexturePathPistil);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float pistilPow = MathF.Pow(pistilJigglePhysicsTimer, 2);
            Vector2 pistilJiggleScale = new Vector2(1 - 0.25f * pistilPow, 1 + 0.5f * pistilPow);

            drawPosition.X += 25 * Owner.direction;

            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            float pistilJiggleRotOffset = (MathF.Sin(MathF.Pow(pistilJigglePhysicsTimer * 3.4f, 2)) * 0.2f + MathF.Sin(Main.LocalPlayer.miscCounter * MathHelper.Pi) * 0.1f) * Projectile.spriteDirection;
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            Main.EntitySpriteDraw(pistilTexture.Value, drawPosition, null, drawColor, drawRotation + pistilJiggleRotOffset, pistilTexture.Size() * 0.5f, Projectile.scale * Owner.gravDir * pistilJiggleScale, flipSprite);

            return false;
        }
    }
}
