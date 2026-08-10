using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class VolterionHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Volterion>();
        public int Time = 0;
        public override float MaxOffsetLengthFromArm => 64f;
        public override float BaseOffsetY => -16f;
        public override float OffsetXUpwards => -16f;
        public override float OffsetXDownwards => 12f;
        public override float OffsetYUpwards => 6f;
        public override float OffsetYDownwards => 20f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.3f;
        public override string Texture => "CalamityMod/Projectiles/Magic/VolterionHoldout";

        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/VolterionFire") { Volume = 0.35f };
        public SlotId FireSoundSlot;

        public ref float FlashTimer => ref Projectile.ai[0];

        public static Asset<Texture2D> MuzzleFlash;
        public override void Load() => MuzzleFlash = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/VolterionHoldoutFlash");

        public override void SetStaticDefaults() => Main.projFrames[Type] = 15;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void KillHoldoutLogic()
        {
            // Override typical CantUseHoldout such that the weapon typically runs through the use animation while not channeled
            if (HeldItem.type != Owner.HeldItem.type || Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }

        public override void HoldoutAI()
        {
            Time++;

            // Update damage based on curent magic damage stat (so Mana Sickness affects it)
            Projectile.damage = HeldItem is null ? 0 : Owner.GetWeaponDamage(HeldItem);

            // Update muzzle flash time and frames (4 animation frames ran over 8 frames)
            if (FlashTimer > 0f)
                FlashTimer -= 0.5f;

            // Time between firing (as the player continues to hold)
            if (Projectile.frame == 14)
            {
                Projectile.frameCounter++;

                // Subtract a flat 42 from the use time because the firing animation consists of 14 frames at 20 FPS
                if (Projectile.frameCounter >= MathHelper.Clamp(Owner.HeldItem.useAnimation - 42, 0f, Owner.HeldItem.useAnimation))
                {
                    if (Owner.CantUseHoldout() || !Owner.CheckMana(Owner.HeldItem))
                        Projectile.Kill();

                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
            }
            // Firing -- this is the initial state the spawned projectile starts in
            else if (Projectile.frame == 0)
            {
                // Does not fire on frame one due to position updating fuckery
                if (Projectile.frameCounter != 1)
                {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= 3)
                    {
                        Projectile.frame++;
                        Projectile.frameCounter = 0;
                    }
                    return;
                }

                Projectile.frameCounter++;
                if (Owner.CheckMana(Owner.HeldItem, -1, true))
                {
                    FlashTimer = 4f;
                    FireSoundSlot = SoundEngine.PlaySound(FireSound, GunTipPosition);
                    Owner.SetScreenshake(3f);

                    // Start from slightly behind the tip
                    Vector2 offsetPos = GunTipPosition - Projectile.velocity.SafeNormalize(Vector2.UnitY) * 18f;
                    Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * HeldItem.shootSpeed;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), offsetPos, velocity, ModContent.ProjectileType<VolterionShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                    for (int i = 0; i < 8; i++)
                    {
                        float scale = Main.rand.NextFloat(0.6f, 1.25f);
                        Vector2 randVelocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(8f, 12f);
                        Particle point = new PointParticle(offsetPos, randVelocity, false, 15, scale, new Color(51, 197, 255));
                        GeneralParticleHandler.SpawnParticle(point);
                    }
                }
            }
            // The firing animation
            else if (Projectile.frame > 0)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= 3)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 0;
                }
            }

            if (SoundEngine.TryGetActiveSound(FireSoundSlot, out var currentSound) && currentSound.IsPlaying)
                currentSound.Position = GunTipPosition;
        }

        // Muzzle flash can deal damage
        public override bool? CanDamage() => FlashTimer > 0f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(GunTipPosition + Vector2.UnitX.RotatedBy(Projectile.rotation) * MuzzleFlash.Width() * 0.4f, 64f, targetHitbox);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Time < 2)
            {
                return false;
            }
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            float scale = Projectile.scale * Owner.gravDir;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), drawRotation, frame.Size() * 0.5f, scale, flipSprite);

            if (FlashTimer > 0f)
            {
                Texture2D flashTex = MuzzleFlash.Value;
                Vector2 flashPos = GunTipPosition + Vector2.UnitX.RotatedBy(Projectile.rotation) * MuzzleFlash.Width() * 0.5f - Main.screenPosition;
                Rectangle flashFrame = flashTex.Frame(verticalFrames: 4, frameY: (int)(4f - FlashTimer));
                Main.EntitySpriteDraw(flashTex, flashPos, flashFrame, Color.White, drawRotation, flashFrame.Size() * 0.5f, scale, flipSprite);
            }
            return false;
        }
    }
}
