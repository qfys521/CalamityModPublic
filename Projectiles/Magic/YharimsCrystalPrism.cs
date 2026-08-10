using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class YharimsCrystalPrism : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public const int NumBeams = 6;
        public const float MaxCharge = 180f;
        public const float DamageStart = 30f;
        private const float DustStart = 30f;
        private const float AimResponsiveness = 0.89f; // Last Prism is 0.92f. Lower makes the prism turn faster.
        private const int SoundInterval = 20;
        private const float MaxManaConsumptionDelay = 15f;
        private const float MinManaConsumptionDelay = 5f;

        public ref float FrameCounter => ref Projectile.ai[0];
        public ref float ManaConsumptionFrame => ref Projectile.ai[1];
        public ref float ManaConsumptionDelay => ref Projectile.localAI[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.NeedsUUID[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 22;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, true);

            // Update damage based on curent magic damage stat (so Mana Sickness affects it)
            Projectile.damage = player.HeldItem is null ? 0 : player.GetWeaponDamage(player.HeldItem);

            FrameCounter += 1f;
            float chargeRatio = MathHelper.Clamp(FrameCounter / MaxCharge, 0f, 1f);

            // Update the crystal's animation, with the animation accelerating as the crystal charges
            Projectile.frameCounter++;
            int framesPerAnimationUpdate = FrameCounter >= MaxCharge ? 2 : FrameCounter >= (MaxCharge * 0.66f) ? 3 : 4;
            if (Projectile.frameCounter >= framesPerAnimationUpdate)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 6)
                    Projectile.frame = 0;
            }

            // Make sound intermittently while the crystal is in use
            if (Projectile.soundDelay <= 0)
            {
                Projectile.soundDelay = SoundInterval;
                // Don't play the continuous beam sound the first time around
                if (FrameCounter > 1f)
                    SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
            }

            // Once the crystal reaches a certain charge, start producing dust. More charge = more dust.
            if (FrameCounter > DustStart && Main.rand.NextFloat() < chargeRatio)
                SpawnEjectionDust(chargeRatio);

            UpdatePlayerVisuals(player, rrp);

            // Update the crystal's existence: project beams on frame 1, and despawn if out of mana.
            if (Projectile.owner == Main.myPlayer)
            {
                // Scale seemingly never changes, so this just scales with shoot speed (Yharim's Crystal is 30 by default)
                float speedTimesScale = player.HeldItem.shootSpeed * Projectile.scale;
                UpdateAim(rrp, speedTimesScale);

                // CheckMana returns true if the mana cost can be paid. If mana isn't consumed this frame, the CheckMana short-circuits out of being evaluated.
                bool allowContinuedUse = !ShouldConsumeMana() || player.CheckMana(player.HeldItem, -1, true, false);
                bool crystalStillInUse = !player.CantUseHoldout() && allowContinuedUse;

                // The beams are only projected once (on frame 1).
                if (crystalStillInUse && FrameCounter == 1f)
                {
                    Vector2 beamVelocity = Vector2.Normalize(Projectile.velocity);
                    if (beamVelocity.HasNaNs())
                        beamVelocity = -Vector2.UnitY;

                    int damage = Projectile.damage;
                    float kb = Projectile.knockBack; // should always be 0
                    for (int b = 0; b < NumBeams; b++)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, beamVelocity, ModContent.ProjectileType<YharimsCrystalBeam>(), damage, kb, Projectile.owner, b, Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI));
                    Projectile.netUpdate = true;
                }
                else if (!crystalStillInUse)
                    Projectile.Kill();
            }

            // Ensures the crystal will disappear immediately if anything goes wrong
            Projectile.timeLeft = 2;
        }

        private bool ShouldConsumeMana()
        {
            // If the mana consumption timer hasn't been initialized yet, initialize it and consume mana on frame 1.
            if (ManaConsumptionDelay == 0f)
            {
                ManaConsumptionFrame = ManaConsumptionDelay = MaxManaConsumptionDelay;
                return true;
            }
            bool consume = FrameCounter == ManaConsumptionFrame;
            if (consume)
            {
                ManaConsumptionDelay = MathHelper.Clamp(ManaConsumptionDelay - 1f, MinManaConsumptionDelay, MaxManaConsumptionDelay);
                ManaConsumptionFrame += ManaConsumptionDelay;
            }
            return consume;
        }

        // Gently adjusts the aim vector of the crystal to point towards the mouse.
        private void UpdateAim(Vector2 source, float speed)
        {
            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Vector2 aimVector = Vector2.Normalize(Main.MouseWorld - source);
            if (aimVector.HasNaNs())
                aimVector = -Vector2.UnitY;
            aimVector = Vector2.Normalize(Vector2.Lerp(aimVector, Vector2.Normalize(Projectile.velocity), AimResponsiveness));
            aimVector *= speed;

            if (aimVector != Projectile.velocity)
                Projectile.netUpdate = true;
            Projectile.velocity = aimVector;
        }

        private void UpdatePlayerVisuals(Player player, Vector2 rrp)
        {
            // Place the projectile directly into the player's hand at all times
            Projectile.Center = rrp;
            // The beam comes out of the tip of the crystal, not the side
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.direction;

            // The crystal is a holdout projectile, so change the player's variables to reflect that
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            // Multiplying by projectile.direction is required due to vanilla spaghetti.
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private void SpawnEjectionDust(float charge)
        {
            Vector2 projDir = Vector2.Normalize(Projectile.velocity);
            int dustType = 90;
            float dustAngle = MathHelper.Pi * 0.76f * (Main.rand.NextBool() ? 1f : -1f);
            float scale = Main.rand.NextFloat(0.9f, 1.2f);
            float speed = 18f * charge;
            Vector2 dustVel = projDir.RotatedBy(dustAngle) * speed;
            float dustForwardOffset = 11f;
            Vector2 dustOrigin = Projectile.Center + dustForwardOffset * projDir;
            Dust d = Dust.NewDustDirect(dustOrigin, 1, 1, dustType, dustVel.X, dustVel.Y);
            d.position += Main.rand.NextVector2Circular(2f, 2f);
            d.noGravity = true;
            d.scale = scale;
        }

        // Completely custom drawcode because it's a holdout projectile. The projectile is also fullbright.
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            SpriteEffects eff = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int frameHeight = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int texYOffset = frameHeight * Projectile.frame;
            Vector2 sheetInsertVec = (Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
            Main.spriteBatch.Draw(tex, sheetInsertVec, new Rectangle?(new Rectangle(0, texYOffset, tex.Width, frameHeight)), Color.White, Projectile.rotation, new Vector2(tex.Width / 2f, frameHeight / 2f), Projectile.scale, eff, 0f);
            return false;
        }
    }
}
