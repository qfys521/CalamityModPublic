using System;
using System.IO;
using System.Linq;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    [PierceResistException]
    public class PhaseslayerProjectile : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Phaseslayer>();
        public override string Texture => "CalamityMod/Items/Weapons/DraedonsArsenal/Phaseslayer";

        // The "average" or "expected" swing speed which the sword's damage balance is based off of.
        // This is rotation EVERY FRAME. The "average" swing speed is 180 degrees in one second, aka pi/60 radians per frame.
        public const float StandardSwingSpeed = MathHelper.Pi / 60f;

        // How quickly the sword's damage updates to reflect its current speed. Higher values make it change damage more quickly.
        public const float DamageUpdateResponsiveness = 0.08f;

        public const int SwordBeamCooldown = 15;
        public const float SwordBeamDamageMultiplier = 0.2f;
        private const float MaximumMouseRange = 360f;
        private const float ProjCenterOffset = 36f;

        public bool IsSmall => false; //Phaseslayer no longer uses charge so no longer gets small. I don't care to redo the code to remove this entirely so it's just being set to false.

        // ai[0] wrapper. Stores a rolling lerped average of angular momentum which is used as the swing speed damage multiplier.
        public float AngularDamageFactor
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // ai[1] wrapper. Stores the sword's vanishing timer.
        public float FadeoutTime
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int BladeFrameX => IsSmall ? 1 : Projectile.frame / 7;
        public int BladeFrameY => IsSmall ? Projectile.frame % 3 : Projectile.frame % 7;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 13;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46; // Collision logic is done manually.
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        // Vanilla Terraria doesn't sync projectile rotation, but it does sync velocity.
        // Rather than hacking velocity to reflect rotation, I'd rather just send the rotation directly.
        // - Ozzatron (09/21/2020)
        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.rotation);
        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.rotation = reader.ReadSingle();

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityGlobalItem modItem = player.HeldItem.Calamity();

            // Angles are wrapped to be 0 to 2pi instead of -pi to pi for convenience with absolute values.
            float rotationAdjusted = MathHelper.WrapAngle(Projectile.rotation) + MathHelper.Pi;
            float oldRotationAdjusted = MathHelper.WrapAngle(Projectile.oldRot[1]) + MathHelper.Pi;
            float deltaAngle = Math.Abs(MathHelper.WrapAngle(rotationAdjusted - oldRotationAdjusted));

            // Frame 1 effect: Prevent the sword from instantly firing a sword beam.
            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = 1f;
                Projectile.soundDelay = SwordBeamCooldown;
            }

            ManipulatePlayer(player, modItem);

            bool wasBig = !IsSmall;
            bool justShrunk = IsSmall && wasBig;
            if (justShrunk)
                OnShrinkEffects();

            AdjustCurrentDamage(player, deltaAngle);
            ManipulateFrames();
            HandleSwordBeams(player, modItem, deltaAngle);
            HandleFadeout();
        }

        private void ManipulatePlayer(Player player, CalamityGlobalItem modItem)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                // In addition to typical channel cancellation criteria, the sword fizzles out if it runs out of charge.
                Item playerItem = player.HeldItem;
                bool hasCharge = true; // Phaseslayer no longer uses charge so always true.
                if (!player.CantUseHoldout() && playerItem.type == ModContent.ItemType<Phaseslayer>() && hasCharge)
                {
                    // 14NOV2024: Ozzatron: clamped mouse position unnecessary, the effect is capped separately
                    // The distance ratio ranges from 0 (your mouse is directly on the player) to 1 (your mouse is at the max range considered, or any further distance).
                    float mouseDistance = Projectile.Distance(Main.MouseWorld);
                    float distRatio = Utils.GetLerpValue(0f, MaximumMouseRange, mouseDistance, true);

                    // This formula ensures that the sword has a sudden and extremely harsh responsiveness penalty when the mouse is close to the player.
                    // Otherwise it controls perfectly fine.
                    float aimResponsiveness = 0.035f + 0.3f * MathF.Pow(distRatio, 1 / 3f);

                    // Update the sword's angle with the responsiveness determined by mouse position.
                    // Also flag for netcode sync if applicable (this is the only way the sword can rotate in multiplayer).
                    float newRotation = Projectile.rotation.AngleLerp(player.AngleTo(Main.MouseWorld), aimResponsiveness);
                    if (Projectile.rotation != newRotation)
                        Projectile.ForceNetUpdate(); // You cannot stop Phaseslayer from sending packets.
                    Projectile.rotation = newRotation;
                }

                // If the player is not wielding the sword, determine whether it should fade out or instantly vanish.
                else if (FadeoutTime == 0f)
                {
                    // If the player voluntarily stopped holding left mouse, start the fadeout timer.
                    if (!player.channel)
                        FadeoutTime = 10f;
                    // If the player was killed, frozen, cursed, etc. just delete it immediately.
                    else
                        Projectile.Kill();
                }
            }

            // This line ensures the sword stays glued to the player, even for other players in multiplayer.
            Projectile.Center = player.MountedCenter + Projectile.rotation.ToRotationVector2() * ProjCenterOffset;

            // These lines ensure the player and their arm are rotated the correct direction to hold the sword.
            Projectile.direction = (Math.Cos(Projectile.rotation) > 0).ToDirectionInt();
            player.ChangeDir(Projectile.direction);
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = CalamityUtils.WrapAngle90Degrees(Projectile.rotation);
        }

        private void OnShrinkEffects()
        {
            SoundEngine.PlaySound(Karasawa.FireSound, Projectile.Center);
            if (Main.dedServ)
                return;

            for (int i = 0; i < 60; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.Brimstone);
                dust.velocity = Main.rand.NextVector2Circular(20f, 20f);
                dust.scale = 2.5f;
                dust.fadeIn = 1.2f;
                dust.noGravity = true;
            }
        }

        private void AdjustCurrentDamage(Player player, float deltaAngle)
        {
            // Update the rolling "blade angular momentum" average by gently lerping in the newest data point.
            AngularDamageFactor = MathHelper.Lerp(AngularDamageFactor, deltaAngle, DamageUpdateResponsiveness);

            // 0x   expected speed gives you 100.0% damage.
            // 1x   expected speed gives you 126.2% damage.
            // 1.5x expected speed gives you 136.9% damage.
            // 2x   expected speed gives you 146.5% damage.
            // 3x   expected speed gives you 163.1% damage.
            // 4x   expected speed gives you 177.1% damage.
            // 5x   expected speed gives you 189.3% damage.
            float speedDamageScalar = MathF.Log(AngularDamageFactor / StandardSwingSpeed + 3f, 3f);

            // Get the underlying sword item's current damage. This takes into account the player's stats and the sword's current charge.
            int damageWithChargeAndStats = player.GetWeaponDamage(player.HeldItem);
            float sizeDamageScalar = IsSmall ? Phaseslayer.SmallDamageMultiplier : 1f;
            Projectile.damage = (int)(damageWithChargeAndStats * speedDamageScalar * sizeDamageScalar);
        }

        private void ManipulateFrames()
        {
            Projectile.frame = 0;

            if (IsSmall)
            {
                // Fadeout frames.
                if (FadeoutTime > 5)
                    Projectile.frame = 1;
                else if (FadeoutTime > 0)
                    Projectile.frame = 2;
            }
            else
            {
                Projectile.frameCounter++;
                int adjustFrameCounter = Projectile.frameCounter % 120;

                // Idle lightning frames.
                if (adjustFrameCounter >= 50 && adjustFrameCounter <= 78)
                    Projectile.frame = (int)MathHelper.Lerp(1, 9, Utils.GetLerpValue(50, 75, adjustFrameCounter, true));
                if (adjustFrameCounter >= 90 && adjustFrameCounter <= 120)
                    Projectile.frame = (int)MathHelper.Lerp(10, 18, Utils.GetLerpValue(90, 117, adjustFrameCounter, true));

                // Fadeout frames.
                if (FadeoutTime > 5)
                    Projectile.frame = 19;
                else if (FadeoutTime > 0)
                    Projectile.frame = 20;
            }
        }

        private void HandleSwordBeams(Player player, CalamityGlobalItem modItem, float deltaAngle)
        {
            // Producing a sword beam takes a bit higher of a speed than the "typical" speed the sword is balanced around.
            if (Projectile.soundDelay <= 0 && deltaAngle >= 1.3f * StandardSwingSpeed)
            {
                // Sword beams cost a noticeable amount of energy, but deal the blade's current damage. Swing harder to get more damage!
                if (Main.myPlayer == player.whoAmI)
                {
                    Vector2 velocity = Projectile.rotation.ToRotationVector2() * 20f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<PhaseslayerBeam>(), (int)(Projectile.damage * SwordBeamDamageMultiplier), 0f, player.whoAmI);
                }

                // The sound delay doubles as the sword beam's cooldown.
                Projectile.soundDelay = SwordBeamCooldown;
                SoundEngine.PlaySound(CommonCalamitySounds.ELRFireSound, Projectile.Center);
            }
        }

        private void HandleFadeout()
        {
            if (FadeoutTime > 0)
            {
                FadeoutTime--;
                if (FadeoutTime <= 0f)
                    Projectile.Kill();
            }
        }

        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float averageRotation = Projectile.oldRot.Take(20).Average(angle => MathHelper.WrapAngle(angle) + MathHelper.Pi);
            float deltaAngle = Math.Abs(averageRotation - (MathHelper.WrapAngle(Projectile.rotation) + MathHelper.Pi));
            float opacity = Projectile.Opacity;
            opacity *= Utils.GetLerpValue(StandardSwingSpeed * 0.7f, StandardSwingSpeed, AngularDamageFactor, true);
            opacity *= MathF.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4f);
            opacity *= MathF.Pow(Utils.GetLerpValue(0.9f, 1.1f, deltaAngle, true), 2f);

            float rotationAdjusted = MathHelper.WrapAngle(Projectile.rotation) + MathHelper.Pi;
            float oldRotationAdjusted = MathHelper.WrapAngle(Projectile.oldRot[1]) + MathHelper.Pi;
            deltaAngle = Math.Abs(MathHelper.WrapAngle(rotationAdjusted - oldRotationAdjusted));

            if (deltaAngle < 0.04f)
                opacity = 0f;

            return Color.Lerp(Color.Red, Color.PaleVioletRed * completionRatio, MathHelper.Clamp(completionRatio * 0.8f, 0f, 1f)) * opacity;
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => (IsSmall ? 101f : 127f) * (1f - completionRatio) * 0.8f;

        public override bool PreDraw(Player renderingPlayer, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D bladeTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/PhaseslayerBlade").Value;
            Texture2D hiltTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            if (IsSmall)
                bladeTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/PhaseslayerBladeSmall").Value;

            float bladeLength = (IsSmall ? 90f : 132f) * Projectile.scale;
            Vector2 bladeOffset = Projectile.rotation.ToRotationVector2() * bladeLength;
            Vector2 origin = bladeTexture.Size() * 0.5f;
            origin /= IsSmall ? new Vector2(1f, 3f) : new Vector2(3f, 7f);

            Rectangle frame = IsSmall ? bladeTexture.Frame(1, 3, 0, BladeFrameY) : bladeTexture.Frame(3, 7, BladeFrameX, BladeFrameY);

            GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SwordSlashTexture"));

            Player player = Main.player[Projectile.owner];
            float swingAngularDirection = Math.Sign(MathHelper.WrapAngle(Projectile.rotation - Projectile.oldRot[1]));

            Vector2[] drawPoints = new Vector2[Projectile.oldPos.Length];
            Vector2 perpendicularDirection = bladeOffset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < drawPoints.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float swingFactor = MathHelper.Min(1f, AngularDamageFactor);
                float offsetFactor = i * -swingAngularDirection * MathHelper.Min(0.8f, swingFactor) * 100f;
                float angularTurn = i * swingAngularDirection * -0.09f;
                drawPoints[i] = Projectile.position + perpendicularDirection.RotatedBy(angularTurn) * offsetFactor;
            }
            PrimitiveRenderer.RenderTrail(drawPoints, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f + bladeOffset, shader: GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"]), 50);

            Main.EntitySpriteDraw(bladeTexture, Projectile.Center + bladeOffset - Main.screenPosition, frame, Color.White, Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(hiltTexture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation + MathHelper.PiOver2, hiltTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center + Projectile.rotation.ToRotationVector2() * 28f;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (IsSmall ? 202f : 254f) * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 60f * Projectile.scale, ref _);
        }
    }
}
