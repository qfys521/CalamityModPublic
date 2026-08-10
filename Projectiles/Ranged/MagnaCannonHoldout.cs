using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class MagnaCannonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<MagnaCannon>();
        public override float MaxOffsetLengthFromArm => 28f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;

        private ref float CurrentChargingFrames => ref Projectile.ai[0];
        private ref float ShotsLoaded => ref Projectile.ai[1];
        private ref float ShootTimer => ref Projectile.ai[2];
        private bool FullyCharged => CurrentChargingFrames >= MagnaCannon.FullChargeFrames;
        public SlotId MagnaChargeSlot;
        public static int FramesPerLoad = 9;
        public static int MaxLoadableShots = 20;
        public static float BulletSpeed = 12f;
        public int time = 0;

        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.HeldItem.type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (SoundEngine.TryGetActiveSound(MagnaChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout())
            {
                KeepRefreshingLifetime = false;

                if (ShotsLoaded > 0)
                {
                    // While bullets are remaining, refresh the lifespan; it will not refresh again after bullets run out
                    Projectile.timeLeft = MagnaCannon.AftershotCooldownFrames;
                    ShootTimer--;
                }

                if (ShootTimer <= 0f)
                {
                    ChargeSound?.Stop();
                    SoundEngine.PlaySound(MagnaCannon.Fire, Projectile.position);

                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                    if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(9f)), ModContent.ProjectileType<MagnaShot>(), Projectile.damage, Projectile.knockBack * (FullyCharged ? 3 : 1), Projectile.owner);
                    for (int i = 0; i <= 3; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>(), shootVelocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.2f, 0.9f), 0, default, Main.rand.NextFloat(1.3f, 1.6f));
                        dust.noGravity = false;
                        dust.color = Main.rand.NextBool(3) ? Color.DodgerBlue : Color.RoyalBlue;
                        dust.fadeIn = 0.3f;
                    }
                    OffsetLengthFromArm -= 5f;

                    ShotsLoaded--;
                    ShootTimer = FullyCharged ? 4f : 5f;
                }
            }
            else
            {
                // Loads shots until maxed out
                if (ShotsLoaded < MaxLoadableShots && CurrentChargingFrames % FramesPerLoad == 0)
                    ShotsLoaded++;

                CurrentChargingFrames++;

                // Sounds
                if (FullyCharged)
                {
                    ShotsLoaded = MaxLoadableShots;
                    if (CurrentChargingFrames == MagnaCannon.FullChargeFrames)
                        MagnaChargeSlot = SoundEngine.PlaySound(MagnaCannon.ChargeFull, Projectile.Center);
                    else if ((CurrentChargingFrames - MagnaCannon.FullChargeFrames - MagnaCannon.ChargeFullSoundFrames) % MagnaCannon.ChargeLoopSoundFrames == 0)
                        MagnaChargeSlot = SoundEngine.PlaySound(MagnaCannon.ChargeLoop, Projectile.Center);
                }
                else if (CurrentChargingFrames == 10)
                    MagnaChargeSlot = SoundEngine.PlaySound(MagnaCannon.ChargeStart, Projectile.Center);

                // Charge-up visuals
                if (CurrentChargingFrames >= 10)
                {
                    float orbScale = MathHelper.Clamp(CurrentChargingFrames, 0f, MagnaCannon.FullChargeFrames) / 200;
                    Lighting.AddLight(GunTipPosition, Color.DodgerBlue.ToVector3() * orbScale);
                }

                // Full charge dusts
                if (CurrentChargingFrames == MagnaCannon.FullChargeFrames)
                {
                    for (int i = 0; i < 36; i++)
                    {
                        Dust chargefull = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<SquashDust>());
                        chargefull.velocity = (MathHelper.TwoPi * i / 36f).ToRotationVector2() * Main.rand.NextFloat(6, 7.5f);
                        chargefull.scale = Main.rand.NextFloat(2f, 2.5f);
                        chargefull.noGravity = true;
                        chargefull.color = Main.rand.NextBool(3) ? Color.Cyan : Color.RoyalBlue;
                        chargefull.fadeIn = 1;
                    }
                }
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(MagnaChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (!Owner.CantUseHoldout())
            {
                float rumble = MathHelper.Clamp(CurrentChargingFrames, 0f, MagnaCannon.FullChargeFrames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 43f, rumble / 43f);
            }

            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            float chargeScale = KeepRefreshingLifetime ? Utils.GetLerpValue(0, MagnaCannon.FullChargeFrames, CurrentChargingFrames, true) : 0;
            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(tex2.Value, GunTipPosition - Main.screenPosition, null, Color.Lerp(FullyCharged ? Color.DodgerBlue : Color.RoyalBlue, Color.White, i * 0.25f) with { A = 0 } * 0.8f, Main.rand.NextFloat(-5, 5), tex2.Size() * 0.5f, new Vector2(1.35f, 1f) * Projectile.scale * chargeScale * (1 - 0.27f * i) * 0.25f * ((chargeScale >= 1 && CurrentChargingFrames <= MagnaCannon.FullChargeFrames + 3) ? 1.75f : FullyCharged ? 1.4f : 1), SpriteEffects.None, 0);

            return false;
        }
    }
}
