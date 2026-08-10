using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SpectralstormCannonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<SpectralstormCannon>();
        public override float MaxOffsetLengthFromArm => 16f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.35f;

        public static readonly SoundStyle OverheatSound = new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/OmegaBlueAbility") with { Pitch = -0.5f };
        public SlotId WarningSlot;
        public ref float Timer => ref Projectile.ai[0];
        public ref float SoulTimer => ref Projectile.ai[1];
        private int BuiltHeat => (Owner.HeldItem.ModItem as SpectralstormCannon).BuiltUpHeat;
        private bool displayOverheat = false;
        private const int WarningTime = SpectralstormCannon.OverheatLevel - 100;

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
            if (Owner.Calamity().flareGunOverheat == 0)
            {
                if (Main.mouseLeft)
                {
                    Timer++;
                    SoulTimer = 0;
                }
                else
                {
                    Timer = 0;
                    SoulTimer++;
                }
            }
            else
            {
                Timer = 0;
                SoulTimer = 0;
            }

            // Once holding the fire button down long enough, start actually firing
            if (Timer >= 30)
            {
                // For some reason using HeldItem here breaks its functionality while being held on the cursor
                (Owner.HeldItem.ModItem as SpectralstormCannon).BuiltUpHeat++;

                // Overheat yourself if you fire too long
                if (BuiltHeat >= SpectralstormCannon.OverheatLevel)
                {
                    WarningSlot = SoundEngine.PlaySound(OverheatSound, Owner.Center);
                    for (int e = 0; e < 7; e++)
                    {
                        Vector2 dustVel = -Projectile.rotation.ToRotationVector2().RotatedByRandom(MathHelper.Pi * 0.15f) * Main.rand.NextFloat(3.8f, 5.5f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Flare, dustVel, Scale: 1.5f);
                        dust.noGravity = true;
                    }

                    // Overheat damage ignores defense and is undodgeable, also inflicts Nightwither
                    Owner.Hurt(PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.FlareGunOverheat").ToNetworkText(Owner.name)), SpectralstormCannon.OverheatDamage, 1, dodgeable: false, scalingArmorPenetration: 1f, knockback: 0f);
                    Owner.AddBuff(ModContent.BuffType<Nightwither>(), SpectralstormCannon.OverheatCooldown);
                    Owner.Calamity().flareGunOverheat = SpectralstormCannon.OverheatCooldown;
                    displayOverheat = true;
                    // Spectralstorm Cannon's overheat immediately resets heat after triggered
                    (Owner.HeldItem.ModItem as SpectralstormCannon).BuiltUpHeat = 1;

                    // Spawn a burst of souls on overheat
                    if (Main.myPlayer == Projectile.owner)
                    {
                        for (int i = 0; i < 25; i++)
                        {
                            Vector2 soulVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, 7.5f);
                            Projectile soul = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Owner.Center, soulVel, ModContent.ProjectileType<LostSoulFriendly>(), (int)(Projectile.damage * 1.15f), 0f, Projectile.owner);
                            soul.timeLeft = 250;
                            soul.DamageType = DamageClass.Ranged;
                            soul.frame = Main.rand.Next(4);
                        }
                    }
                    return;
                }

                // Play a warning sound
                if (BuiltHeat == WarningTime)
                {
                    WarningSlot = SoundEngine.PlaySound(FirestormCannonHoldout.WarningSound, Owner.Center);
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
                        Owner.PickAmmo(HeldItem, out _, out _, out _, out _, out _);
                        Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.Pi * 0.015f * (1f + firingLerp * 0.25f)) * Owner.HeldItem.shootSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, velocity, ModContent.ProjectileType<SpectralFlare>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                }
            }

            // Fire souls while dissipating heat
            if (SoulTimer % 8 == 4f)
            {
                if (SoulTimer % 16 == 4f)
                    SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.7f }, Owner.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.Pi * 0.03f) * Owner.HeldItem.shootSpeed * 0.8f;
                    Projectile soul = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, velocity, ModContent.ProjectileType<LostSoulFriendly>(), Projectile.damage, 0f, Projectile.owner, 2f);
                    soul.DamageType = DamageClass.Ranged;
                    soul.frame = Main.rand.Next(4);
                }
            }

            // Reset overheat draw color once the overheat ends
            if (Owner.Calamity().flareGunOverheat == 0)
                displayOverheat = false;
            // Draw smoke effect while overheated
            if (displayOverheat && Main.rand.NextBool(3))
            {
                HeavySmokeParticle smoke = new(GunTipPosition, -Vector2.UnitY * 7.5f, Color.DarkCyan, 25, 0.4f, 0.75f);
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
            target.AddBuff(BuffID.OnFire3, 300);
            (Owner.HeldItem.ModItem as SpectralstormCannon).BuiltUpHeat -= 6;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f) - (Owner.gravDir == -1 ? MathHelper.Pi * Owner.direction : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color tintColor = displayOverheat ? Color.Black :
                BuiltHeat >= WarningTime ? Color.Lerp(Color.LightSalmon, Color.White, MathF.Abs(MathF.Sin(Owner.miscCounter * MathHelper.Pi / 30f))) : Color.LightSalmon;
            float opacity = Utils.GetLerpValue(0, WarningTime, BuiltHeat, true);

            for (int i = 0; i < 16; i++)
            {
                Color auraColor = Color.HotPink * opacity * 0.6f;
                Vector2 drawOffset = ((MathHelper.TwoPi * i / 16f).ToRotationVector2() * 5);
                Main.EntitySpriteDraw(texture, drawPosition + drawOffset, null, auraColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);
            }

            CalamityUtils.EnterShaderRegion(Main.spriteBatch);
            GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity((displayOverheat ? 1f : opacity) * 0.45f);
            GameShaders.Misc["CalamityMod:BasicTint"].UseColor(tintColor);
            GameShaders.Misc["CalamityMod:BasicTint"].Apply();

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale, flipSprite);
            CalamityUtils.ExitShaderRegion(Main.spriteBatch);
            return false;
        }
    }
}
