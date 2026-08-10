using System.IO;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class CondemnationHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Condemnation>();
        public override float MaxOffsetLengthFromArm => 25f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;

        private ref float CurrentChargingFrames => ref Projectile.ai[0];
        private ref float ArrowsLoaded => ref Projectile.ai[1];
        private ref float FramesToLoadNextArrow => ref Projectile.localAI[0];

        private float storedVelocity = 1f;
        public const float velocityMultiplier = 1.2f;
        public bool homing = false;
        public bool playShootSound = true;

        public override void KillHoldoutLogic()
        {
            // Fire arrows if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout())
            {
                // No arrows left to shoot? The bow disappears.
                if (ArrowsLoaded <= 0f)
                {
                    Projectile.Kill();
                    return;
                }
                if (homing && playShootSound)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot with { Pitch = 0.2f, MaxInstances = 2 }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot with { Pitch = -0.4f, MaxInstances = 2 }, Projectile.Center);
                    playShootSound = false;
                }

                // Fire one charged arrow every frame until you're out of arrows. 
                ShootProjectiles(homing);
                --ArrowsLoaded;
                if (ArrowsLoaded == 0 && homing == true)
                    homing = false;
            }
        }

        public override void HoldoutAI()
        {
            // Does not run if the player stops channeling
            if (Owner.CantUseHoldout())
                return;

            // Frame 1 effects: Record how fast the Condemnation item being used is, to determine how fast to load arrows.
            if (FramesToLoadNextArrow == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                FramesToLoadNextArrow = HeldItem.useAnimation;
            }

            // If no arrows are loaded, spawn a bit of dust to indicate it's not ready yet.
            // Spawn the same dust if the max number of arrows have been loaded or the player ran out of ammos to load.
            if (ArrowsLoaded <= 0f || ArrowsLoaded >= Condemnation.MaxLoadedArrows || !Owner.HasAmmo(Owner.HeldItem))
                SpawnCannotLoadArrowsDust(GunTipPosition);

            if (Owner.HasAmmo(HeldItem))
            {
                // Actually make progress towards loading more arrows.
                ++CurrentChargingFrames;

                // If it is time to load an arrow, produce a pulse of dust and add an arrow.
                // Also accelerate charging, because it's fucking awesome.
                // Take the ammo here as well
                if (CurrentChargingFrames >= FramesToLoadNextArrow && ArrowsLoaded < Condemnation.MaxLoadedArrows)
                {
                    // Save the stats here for later
                    Owner.PickAmmo(HeldItem, out _, out float shootSpeed, out int damage, out float knockback, out _);
                    Projectile.damage = damage;
                    Projectile.knockBack = knockback;
                    storedVelocity = shootSpeed * velocityMultiplier;

                    SpawnArrowLoadedDust();
                    CurrentChargingFrames = 0f;
                    ++ArrowsLoaded;
                    FramesToLoadNextArrow = MathHelper.Clamp(FramesToLoadNextArrow--, 1f, HeldItem.useAnimation);

                    // Play a sound for additional notification that an arrow has been loaded.
                    var loadSound = SoundEngine.PlaySound(SoundID.Item108 with { Volume = SoundID.Item108.Volume * 0.4f, Pitch = -0.3f + ArrowsLoaded * 0.1f });

                    if (ArrowsLoaded >= Condemnation.MaxLoadedArrows)
                    {
                        SoundStyle fire = new("CalamityMod/Sounds/Custom/AbilitySounds/BrimflameRecharge");
                        for (int i = 0; i < 3; i++)
                            SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = i * 0.25f, MaxInstances = 3 });
                        homing = true;
                    }
                }
            }
        }

        public void SpawnArrowLoadedDust()
        {
            if (Main.dedServ)
                return;

            //Special visuals for the final loaded arrow
            if (ArrowsLoaded >= Condemnation.MaxLoadedArrows - 1f)
            {
                //Star
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.Pi * 1.5f - i * MathHelper.TwoPi / 5f;
                    float nextAngle = MathHelper.Pi * 1.5f - (i + 2) * MathHelper.TwoPi / 5f;
                    Vector2 start = angle.ToRotationVector2();
                    Vector2 end = nextAngle.ToRotationVector2();
                    for (int j = 0; j < 40; j++)
                    {
                        Dust starDust = Dust.NewDustPerfect(GunTipPosition, DustID.RainbowMk2);
                        starDust.scale = 2.5f;
                        starDust.velocity = Vector2.Lerp(start, end, j / 40f) * 16f;
                        starDust.color = Color.Crimson;
                        starDust.noGravity = true;
                    }
                }
                return;
            }

            for (int i = 0; i < 36; i++)
            {
                Dust chargeMagic = Dust.NewDustPerfect(GunTipPosition, DustID.RainbowMk2);
                chargeMagic.velocity = (MathHelper.TwoPi * i / 36f).ToRotationVector2() * 5f + Owner.velocity;
                chargeMagic.scale = Main.rand.NextFloat(1f, 1.5f);
                chargeMagic.color = Color.Violet;
                chargeMagic.noGravity = true;
            }
        }

        public void SpawnCannotLoadArrowsDust(Vector2 GunTipPosition)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 2; i++)
            {
                Dust chargeMagic = Dust.NewDustPerfect(GunTipPosition + Main.rand.NextVector2Circular(20f, 20f), DustID.RainbowMk2);
                chargeMagic.velocity = (GunTipPosition - chargeMagic.position) * 0.1f + Owner.velocity;
                chargeMagic.scale = Main.rand.NextFloat(1f, 1.5f);
                chargeMagic.color = Projectile.GetAlpha(Color.White);
                chargeMagic.noGravity = true;
            }
        }

        public void ShootProjectiles(bool homing)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * storedVelocity;
            int ArrowType = homing ? ModContent.ProjectileType<CondemnationArrowHoming>() : ModContent.ProjectileType<CondemnationArrow>();
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ArrowType, (int)(Projectile.damage * 1.35f), Projectile.knockBack, Projectile.owner);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float randSize = Main.rand.NextFloat(0.9f, 1f);
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.direction == -1 ? MathHelper.Pi : 0f) - (Owner.gravDir == -1 ? MathHelper.Pi * Owner.direction : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (homing)
            {
                for (int i = 0; i < 22; i++)
                {
                    Color auraColor = Color.Lerp(Color.Red, Color.White, i * 0.01f) with { A = 0 } * 0.6f;
                    Vector2 drawOffset = ((MathHelper.TwoPi * i / 22f).ToRotationVector2() * 5) + Main.rand.NextVector2Circular(7, 7);
                    Main.EntitySpriteDraw(texture, drawPosition + drawOffset, null, auraColor, drawRotation, rotationPoint, Projectile.scale * Main.rand.NextFloat(0.7f, 1.1f), flipSprite);
                }
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);
            
            return false;
        }
        public override void SendExtraAIHoldout(BinaryWriter writer)
        {
            writer.Write(FramesToLoadNextArrow);
            writer.Write(storedVelocity);
            writer.Write(homing);
        }

        public override void ReceiveExtraAIHoldout(BinaryReader reader)
        {
            FramesToLoadNextArrow = reader.ReadSingle();
            storedVelocity = reader.ReadSingle();
            homing = reader.ReadBoolean();
        }
    }
}
