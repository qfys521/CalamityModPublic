using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class DragonsBreathHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<DragonsBreath>();
        public override float MaxOffsetLengthFromArm => 60f;
        public override float OffsetXUpwards => -10f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 5f;
        public override Vector2 GunTipPosition => base.GunTipPosition - Projectile.velocity.RotatedBy(0.6f * Projectile.direction) * 10;

        public ref float Time => ref Projectile.ai[0];
        public ref float fireTimer => ref Projectile.ai[1];
        public int fireSpeed = 1;
        public bool firingBeam = false;
        public int weldingTimer = 0;
        public SlotId WeldSoundSlot;
        public bool hasLaunchedMag = false;
        public float fade = 0;

        public override void HoldoutAI()
        {
            fade = MathHelper.Lerp(fade, 0, 0.25f);
            if (Time == 0)
            {
                weldingTimer = Owner.itemAnimationMax * 6;
            }
            if (firingBeam)
            {
                if (weldingTimer > 0)
                {
                    if (SoundEngine.TryGetActiveSound(WeldSoundSlot, out var WeldSound) && WeldSound.IsPlaying)
                        WeldSound.Position = Projectile.Center;
                    if (Owner.Calamity().DragonsBreathAudioCooldown2 == 0)
                    {
                        Owner.Calamity().DragonsBreathAudioCooldown2 = 30;
                        WeldSoundSlot = SoundEngine.PlaySound(DragonsBreath.WeldingShoot, Projectile.Center);
                    }

                    if (weldingTimer % 2 == 0 && Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5, ModContent.ProjectileType<DragonsBreathFlames>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 1, weldingTimer, (Time % 15 < 10 ? 18 : 0));

                    weldingTimer--;
                    if (Time % 3 == 0)
                    {
                        OffsetLengthFromArm = 54f;
                    }
                }
                else
                {
                    Time = -1;
                    firingBeam = false;
                    fireSpeed = 1;
                    SoundStyle bigShot = new("CalamityMod/Sounds/Item/DudFire");
                    SoundEngine.PlaySound(bigShot with { PitchVariance = 0.15f, Volume = 0.75f }, Projectile.Center);

                    if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 6).RotatedBy(-2.3f * Projectile.direction), ModContent.ProjectileType<DragonsBreathMag>(), 1, Projectile.knockBack, Projectile.owner, 1);
                    hasLaunchedMag = true;
                }
            }
            else
            {
                // If the time reaches Item.useTime, it'll shoot the projectile, fire and reset the timer.
                if (fireTimer >= Owner.itemTimeMax && Main.myPlayer == Projectile.owner)
                {
                    Owner.PickAmmo(Owner.HeldItem, out _, out float shootSpeed, out int damage, out float knockback, out _);

                    for (int i = 0; i < 3; i++)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.15f * i * 0.5f) * shootSpeed * (i == 0 ? 0.5f : i == 1 ? 0.7f : 1) * Main.rand.NextFloat(0.8f, 1f), ModContent.ProjectileType<DragonsBreathFlames>(), damage, knockback, Projectile.owner);

                    SoundEngine.PlaySound(DragonsBreath.FireballSound, Projectile.Center);
                    hasLaunchedMag = false;
                    fade = 1;

                    OffsetLengthFromArm = 52f;
                    fireSpeed++;
                    fireTimer = 0;
                    if (fireSpeed == 20)
                    {
                        OffsetLengthFromArm = 32f;
                        firingBeam = true;
                        SoundEngine.PlaySound(DragonsBreath.WeldingStart, Projectile.Center);
                    }
                }

                fireTimer += 1 + (fireSpeed * 0.15f);
            }
            Time++;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Time < 2)
                return false;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (firingBeam)
            {
                for (int i = 0; i < 3; i++)
                    Main.EntitySpriteDraw(texture, position + Main.rand.NextVector2Circular(10, 10), null, Color.Orange with { A = 0 } * 0.6f, rotation, origin, Projectile.scale * Owner.gravDir, effects);
            }
            Main.EntitySpriteDraw(texture, position, null, Projectile.GetAlpha(lightColor), rotation + (hasLaunchedMag ? (-0.5f * Utils.GetLerpValue(Owner.itemAnimationMax * 0.2f, 0, fireTimer, true) * Projectile.direction) : 0), origin, Projectile.scale * Owner.gravDir, effects);

            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Vector2 drawPosition = GunTipPosition - Main.screenPosition + Projectile.velocity * 3;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            // Glow Orb
            Main.EntitySpriteDraw(rechargeTexture, drawPosition, null, Color.OrangeRed with { A = 0 } * fade, Projectile.rotation, rechargeTexture.Size() * 0.5f, 0.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(rechargeTexture, drawPosition, null, Color.Orange with { A = 0 } * 0.75f * fade, Projectile.rotation, rechargeTexture.Size() * 0.5f, 0.25f, SpriteEffects.None, 0);

            return false;
        }
    }
}
