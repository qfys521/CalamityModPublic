using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class FirestormCannonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<FirestormCannon>();
        public override float MaxOffsetLengthFromArm => 16f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.35f;

        public static readonly SoundStyle WarningSound = new SoundStyle("CalamityMod/Sounds/Item/FireImplosion") with { Volume = 0.75f };
        public static readonly SoundStyle OverheatSound = new("CalamityMod/Sounds/Item/HeliumFlashSteamRelease");
        public SlotId WarningSlot;
        public ref float Timer => ref Projectile.ai[0];
        private int BuiltHeat => (Owner.HeldItem.ModItem as FirestormCannon).BuiltUpHeat;
        private bool displayOverheat = false;
        private const int WarningTime = FirestormCannon.OverheatLevel - 100;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void KillHoldoutLogic()
        {
            // What kills the holdout:
            // Dead, CCed, Cursed, etc.
            // Not holding the item
            // Right-clicking (Can kill it early) (Does not work if in overheat state)
            // Completely cooling while not firing
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.HeldItem.type ||
                (Owner.Calamity().mouseRight && !displayOverheat) || (BuiltHeat == 0 && !Main.mouseLeft))
            {
                if (SoundEngine.TryGetActiveSound(WarningSlot, out var warn))
                    warn.Stop();

                Projectile.Kill();
            }
        }

        public override void HoldoutAI()
        {
            // Timer should only increment while firing and not overheated
            if (Main.mouseLeft && Owner.Calamity().flareGunOverheat == 0)
                Timer++;
            else
                Timer = 0;

            // Once holding the fire button down long enough, start actually firing
            if (Timer >= 30)
            {
                // For some reason using HeldItem here breaks its functionality while being held on the cursor
                (Owner.HeldItem.ModItem as FirestormCannon).BuiltUpHeat++;

                // Overheat yourself if you fire too long
                if (BuiltHeat >= FirestormCannon.OverheatLevel)
                {
                    WarningSlot = SoundEngine.PlaySound(OverheatSound, Owner.Center);
                    for (int e = 0; e < 7; e++)
                    {
                        Vector2 dustVel = -Projectile.rotation.ToRotationVector2().RotatedByRandom(MathHelper.Pi * 0.15f) * Main.rand.NextFloat(3.8f, 5.5f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Flare, dustVel, Scale: 1.5f);
                        dust.noGravity = true;
                    }

                    // Overheat damage ignores defense and is undodgeable, also inflicts On Fire
                    Owner.Hurt(PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.FlareGunOverheat").ToNetworkText(Owner.name)), FirestormCannon.OverheatDamage, 1, dodgeable: false, scalingArmorPenetration: 1f, knockback: 0f);
                    Owner.AddBuff(BuffID.OnFire, FirestormCannon.OverheatCooldown);
                    Owner.Calamity().flareGunOverheat = FirestormCannon.OverheatCooldown;
                    displayOverheat = true;
                    return;
                }

                // Play a warning sound
                if (BuiltHeat == WarningTime)
                {
                    WarningSlot = SoundEngine.PlaySound(WarningSound, Owner.Center);
                }

                // Controls the escalating firing speed
                float firingLerp = Utils.GetLerpValue(0, 90, Timer - 30, true);
                int firingFrequency = (int)MathHelper.Lerp(HeldItem.useTime, HeldItem.useTime / 2, firingLerp);
                // Actually fire shtuff
                if (Timer % firingFrequency == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.8f }, Owner.Center);
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Owner.PickAmmo(HeldItem, out int flareType, out _, out _, out _, out _, Main.rand.Next(100) >= 70);
                        Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.Pi * 0.04f * (1f + firingLerp * 0.5f)) * Owner.HeldItem.shootSpeed;
                        Projectile flare = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, velocity, flareType, Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f, 1f);
                        flare.penetrate = 3;
                        flare.MaxUpdates = 2;
                        flare.timeLeft = 600;
                        flare.DamageType = DamageClass.Ranged;
                        flare.usesIDStaticNPCImmunity = false;
                        flare.usesLocalNPCImmunity = true;
                        flare.localNPCHitCooldown = 30 * 2;
                    }
                }
            }

            // Reset overheat draw color once the heat passes below the warning indicator
            if (BuiltHeat < WarningTime)
                displayOverheat = false;
            // Draw smoke effect while overheated
            if (displayOverheat && Main.rand.NextBool(3))
            {
                HeavySmokeParticle smoke = new(GunTipPosition, -Vector2.UnitY * 7.5f, Color.DarkGray, 25, 0.4f, 0.75f);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            // Constantly move the warning sound on top of the player
            if (SoundEngine.TryGetActiveSound(WarningSlot, out var warning) && warning.IsPlaying)
                warning.Position = Projectile.Center;
        }

        // Pan-searing mechanic; reduces heat a bit
        public override bool? CanDamage() => BuiltHeat >= 120 && Owner.Calamity().flareGunOverheat == 0;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 300);
            (Owner.HeldItem.ModItem as FirestormCannon).BuiltUpHeat -= 6;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color tintColor = displayOverheat ? Color.Black :
                BuiltHeat >= WarningTime ? Color.Lerp(Color.Tomato, Color.White, MathF.Abs(MathF.Sin(Owner.miscCounter * MathHelper.Pi / 30f))) : Color.Tomato;

            CalamityUtils.EnterShaderRegion(Main.spriteBatch);
            GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(Utils.GetLerpValue(0, WarningTime, BuiltHeat, true) * 0.45f);
            GameShaders.Misc["CalamityMod:BasicTint"].UseColor(tintColor);
            GameShaders.Misc["CalamityMod:BasicTint"].Apply();

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            CalamityUtils.ExitShaderRegion(Main.spriteBatch);
            return false;
        }
    }
}
