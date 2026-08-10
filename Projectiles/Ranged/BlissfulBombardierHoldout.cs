using System;
using System.IO;
using CalamityMod.Dusts;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class BlissfulBombardierHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<BlissfulBombardier>();
        public override float RecoilResolveSpeed => 0.07f;
        public override float MaxOffsetLengthFromArm => 60f;
        public override float OffsetXUpwards => -18f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYUpwards => 23f;

        // The type of dust used in the effects and the color of the particles that it'll use depending on the type of rocket used.
        // It'll carry over to the projectiles that it shoots.
        public static int dustEffectsID { get; set; }
        public static Color effectsColor { get; set; }
        public static Color staticEffectsColor { get; set; } = Color.Khaki;

        public ref float shootingTimer => ref Projectile.ai[0];
        public ref float PostFireCooldown => ref Projectile.ai[1];
        public bool hasFired = false;

        public override void SendExtraAIHoldout(BinaryWriter writer)
        {
            writer.Write(shootingTimer);
        }
        public override void ReceiveExtraAIHoldout(BinaryReader reader)
        {
            shootingTimer = reader.ReadSingle();
        }

        public override void KillHoldoutLogic()
        {

        }

        public override void ManageHoldout()
        {
            // The center of the player, taking into account if they have a mount or not.
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            // The vector between the player a relative point in the sky slightly based on mouse position, used for pointing the holdout.
            Vector2 ownerToSky = new Vector2(MathHelper.Lerp(Owner.ClampedMouseWorld().X, Owner.Center.X, 0.55f), Owner.Center.Y) + new Vector2(0, -500) - Owner.Center;

            // The direction this holdout's pointing at.
            float holdoutDirection = Projectile.velocity.ToRotation();

            // A range from -1 to 1 for when the holdout is pointing downards of upwards, respectively.
            // Used for the offsets.
            float proximityLookingUpwards = Vector2.Dot(ownerToSky.SafeNormalize(Vector2.Zero), -Vector2.UnitY * Owner.gravDir);

            int direction = MathF.Sign(ownerToSky.X);

            Vector2 lengthOffset = Projectile.rotation.ToRotationVector2() * OffsetLengthFromArm;
            Vector2 armOffset = new Vector2(Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetXUpwards : OffsetXDownwards) * direction, BaseOffsetY * Owner.gravDir + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetYUpwards : OffsetYDownwards) * Owner.gravDir);
            Projectile.Center = armPosition + lengthOffset + armOffset;

            Projectile.velocity = holdoutDirection.AngleTowards(ownerToSky.ToRotation(), 0.2f).ToRotationVector2();
            
            Projectile.rotation = holdoutDirection;

            Projectile.spriteDirection = direction;
            Owner.ChangeDir(direction);

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            // -Pi/2 because the arms rotation starts with arms pointing down.
            float armRotation = (Projectile.rotation - MathHelper.PiOver2) * Owner.gravDir + (Owner.gravDir == -1 ? MathHelper.Pi : 0f);
            Owner.SetCompositeArmFront(true, FrontArmStretch, armRotation + ExtraFrontArmRotation * direction);
            Owner.SetCompositeArmBack(true, BackArmStretch, armRotation + ExtraBackArmRotation * direction);

            if (KeepRefreshingLifetime)
                Projectile.timeLeft = 2;

            if (OffsetLengthFromArm != MaxOffsetLengthFromArm)
                OffsetLengthFromArm = MathHelper.Lerp(OffsetLengthFromArm, MaxOffsetLengthFromArm, RecoilResolveSpeed);

            Projectile.ForceNetUpdate();
        }
        public override void HoldoutAI()
        {
            // If there's no player, or the player isn't holding fire, or the player's stunned, there'll be no holdout.
            // The holdout will persist if on cooldown, but it is absolutely imperative that it can disappear if dead, CCed, etc.
            if (Owner == null || !Owner.active || Owner.dead || Owner.CCed || Owner.noItems)
            {
                Projectile.Kill();
                return;
            }
            if (!Owner.channel && PostFireCooldown <= 0 && shootingTimer < (int)(Owner.itemAnimationMax * 0.8f))
            {
                Projectile.Kill();
                return;
            }

            if (!hasFired)
                shootingTimer++;

            if (shootingTimer == (int)(Owner.itemAnimationMax) && HeldItem.type == AssociatedItemID && !hasFired)
            {
                ShootRocket();

                for (int i = 0; i <= 25; i++)
                {
                    if (i % 2 == 0)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<LightDust>(), (Projectile.velocity * 12).RotatedByRandom(0.6f) * Main.rand.NextFloat(0.4f, 1.7f), 0, default, Main.rand.NextFloat(1.8f, 2.3f));
                        dust.noGravity = true;
                        dust.color = Main.rand.NextBool(3) ? Color.Orange : effectsColor;
                        dust.noLightEmittance = true;
                    }
                    else
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, DustID.FireworksRGB, (Projectile.velocity * 12).RotatedByRandom(0.6f) * Main.rand.NextFloat(0.4f, 1.7f), 0, default, Main.rand.NextFloat(1.2f, 1.6f));
                        dust.noGravity = false;
                        dust.color = Main.rand.NextBool(3) ? Color.Orange : staticEffectsColor;
                    }
                }

                hasFired = true;
            }

            if (PostFireCooldown > 0)
                PostFiringCooldown();
        }

        public void ShootRocket()
        {
            // We use the velocity of this projectile as its direction vector.
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            float velocityMultiplier = 0.9f;

            // Every time we shoot we use ammo.
            // With this method we also use the item's stats, like the shoot speed, or the type of ammo it was used.
            Owner.PickAmmo(HeldItem, out _, out float projSpeed, out int damage, out float knockback, out int rocketType);

            // Decides the color of the effects depending on the type of rocket used.
            switch (rocketType)
            {
                case ItemID.WetRocket:
                    dustEffectsID = 45;
                    effectsColor = Color.RoyalBlue;
                    break;
                case ItemID.LavaRocket:
                    dustEffectsID = DustID.Torch;
                    effectsColor = Color.Red;
                    break;
                case ItemID.HoneyRocket:
                    dustEffectsID = DustID.Honey;
                    effectsColor = Color.Yellow;
                    break;
                default:
                    dustEffectsID = 87;
                    effectsColor = Color.Goldenrod;
                    break;
            }

            // Spawns the projectile.
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootDirection * projSpeed * velocityMultiplier, ProjectileType<NukeOfBliss>(), damage, knockback, Projectile.owner, rocketType);
                PostFireCooldown = Owner.itemAnimationMax;
                Owner.SetScreenshake(5f);
            }

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            SoundStyle fire = new("CalamityMod/Sounds/Item/LauncherHeavyShot");
            SoundEngine.PlaySound(fire with { Volume = 0.9f, PitchVariance = 0.1f }, Projectile.Center);

            for (int k = 0; k < 6; k++)
            {
                float pulseScale = Main.rand.NextFloat(0.2f, 0.4f);
                DirectionalPulseRing pulse = new DirectionalPulseRing(GunTipPosition, (shootDirection * 20).RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1.2f), (Main.rand.NextBool(3) ? effectsColor : staticEffectsColor) * 0.8f, new Vector2(1, 1), pulseScale - 0.25f, pulseScale, 0f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            OffsetLengthFromArm -= 25f;
        }

        public void PostFiringCooldown()
        {
            Owner.channel = true;
            if (PostFireCooldown > 1)
            {
                PostFireCooldown--;
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel, staticEffectsColor, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.3f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(GunTipPosition, DustID.SteampunkSteam, smokeVel.RotatedByRandom(0.1f), 80, default, Main.rand.NextFloat(0.4f, 1.3f));
                dust.noGravity = false;
                dust.color = staticEffectsColor;
            }
            else
            {
                PostFireCooldown--;
                hasFired = false;
                shootingTimer = 0;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            FrontArmStretch = Player.CompositeArmStretchAmount.Quarter;
            ExtraBackArmRotation = MathHelper.ToRadians(15f);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (shootingTimer <= 1)
                return false;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/BlissfulBombardierGlow").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f) - (Owner.gravDir == -1 ? MathHelper.Pi * Owner.direction : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (hasFired)
            {
                float rumble = Utils.GetLerpValue(0f, Owner.itemAnimationMax, PostFireCooldown, true) * 8f;
                drawPosition += Main.rand.NextVector2Circular(rumble, rumble);
            }

            if (!hasFired)
            {
                float fade = Utils.GetLerpValue(0, Owner.itemAnimationMax, shootingTimer, true);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 5 * fade;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, staticEffectsColor with { A = 0 } * fade, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite, 0f);
                }
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            Main.EntitySpriteDraw(texture2, drawPosition, null, Color.White, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            return false;
        }
    }
}
