using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackTesselation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        internal static Asset<Texture2D> Glow;

        // As Nanoblack Tesselations are not square, this is required for the glowmask to be rendered properly.
        private const int SpriteWidth = 52;

        private const int Lifetime = 60;
        private const int VanishTime = 12;
        internal const int MinDelay = 15;
        internal const int MaxDelay = 45;

        private const float TargetingRange = 600f;
        private const float FiringRange = 2000f;

        // Rotation speed is inherited directly from the scythe.
        private const float StartingRotationIncrement = NanoblackMain.RotationIncrement * NanoblackMain.UpdatesPerFrame;
        private const float DriftSpeed = 0.8f;

        private Player Owner => Main.player[Projectile.owner];
        internal ref float AttackDelay => ref Projectile.ai[0];
        internal ref float CurrentSpin => ref Projectile.ai[1];
        private bool IsVanishing => Projectile.timeLeft < VanishTime;

        public override void Load() => Glow = ModContent.Request<Texture2D>(Texture + "Glow");

        public override void SetStaticDefaults()
        {
            // Nanoblack Tesselations are not perfectly square and need assistance to spin properly.
            DrawOffsetX = -10;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.scale = 0.5f;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == Lifetime)
                FrameOneEffects();

            // Tesselations enable their owner's mouse listener so that the mouse state is synced.
            // This is necessary for their targeting algorithm.
            if (Owner.whoAmI == Main.myPlayer)
                Owner.Calamity().mouseWorldListener = true;

            // If the Tesselation cannot attack, then it immediately transitions to vanishing.
            bool shouldShutdown = false;
            if (!IsVanishing)
                shouldShutdown = AttemptAttackThisFrame();

            if (shouldShutdown)
            {
                Projectile.timeLeft = VanishTime - 1;

                // Spawn a particle when the Tesselation begins vanishing to make it look like it blinks out of existence.
                float orbScale = 1.5f;
                Color orbColor = NanoblackReaper.TesselationParticleColor;
                Particle vanishOrb = new GlowOrbParticle(Projectile.Center, Projectile.velocity, false, 15, orbScale, orbColor);
                GeneralParticleHandler.SpawnParticle(vanishOrb);
            }

            ProcessSpin();

            // Tesselations always screech to a near-halt during spindown, but drift gently afterwards to form a mesmerizing pattern.
            if (!IsVanishing && Projectile.velocity.LengthSquared() > DriftSpeed * DriftSpeed)
                Projectile.velocity *= 0.75f;

            // If the tesselation is vanishing, shrink by 12% every frame
            if (IsVanishing)
                Projectile.scale *= 0.88f;
        }

        private void FrameOneEffects()
        {
            // Sanity check the firing delay.
            if (AttackDelay < MinDelay)
                AttackDelay = MinDelay;
            else if (AttackDelay > MaxDelay)
                AttackDelay = MaxDelay;

            CurrentSpin = StartingRotationIncrement;

            // Create particles to visually flavor the spawning of the projectile.
            {
                float sparkSpeed = 2f;
                float baseRot = MathHelper.PiOver2;
                float scale = 0.018f;
                int lifetime = 15;
                Color color = NanoblackReaper.TesselationParticleColor;
                Vector2 squashStretch = new(1f, 0.3f);

                for (int i = 0; i < 6; ++i)
                {
                    float rot = baseRot + i * NanoblackReaper.PiOver3;
                    Vector2 sparkVel = sparkSpeed * rot.ToRotationVector2();
                    Particle spark = new GlowSparkParticle(Projectile.Center, sparkVel, false, lifetime, scale, color, squashStretch, true, true, 1f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }

        private void ProcessSpin()
        {
            // Visually spins the tesselation.
            float rotationIncrement = CurrentSpin * Projectile.spriteDirection;
            Projectile.rotation += rotationIncrement;

            // Spin slows down exponentially over time.
            CurrentSpin *= 0.93f;
        }

        // Returns whether or not the Tesselation should shut down.
        private bool AttemptAttackThisFrame()
        {
            // If the attack isn't ready yet, don't perform it.
            if (AttackDelay > 0f)
            {
                --AttackDelay;
                return false;
            }

            // As is Nanoblack tradition, Tesselations prefer to target bosses whenever possible.
            bool inFiringRange = false;
            NPC target = Owner.ClampedMouseWorld().ClosestNPCAt(TargetingRange, bossPriority: true);

            // If the first cursor-based targeting attempt fails, try again near the Tesselation itself.
            if (target is null || !target.active)
                target = Projectile.Center.ClosestNPCAt(TargetingRange, bossPriority: true);

            // Check firing range. Tesselations can fire across a whole 1080p screen, but it's not infinite distance.
            if (target is not null && target.active)
                inFiringRange = target.DistanceSQ(Projectile.Center) < FiringRange * FiringRange;
            if (!inFiringRange)
                return true;

            // At this point, the targeting is confirmed. Attack.
            PerformAttack(target);
            return true;
        }

        private void PerformAttack(NPC target)
        {
            // Exact visual offset of the zero-point energy strike is randomly chosen.
            // For consistent RNG across clients, this randomness is executed even if the result is not used.
            float xInterp = Main.rand.NextFloat();
            float yInterp = Main.rand.NextFloat();

            if (Main.myPlayer != Projectile.owner)
                return;

            // The "dartboard" is the majority, but not all, of the NPC's hitbox.
            Vector2 c = target.Center;
            float dartboardScale = 0.4f; // 0.5f would be the entire hitbox of the NPC
            Vector2 topLeft = c - dartboardScale * target.Size;
            Vector2 bottomRight = c + dartboardScale * target.Size;
            float dartboardX = MathHelper.Lerp(topLeft.X, bottomRight.X, xInterp);
            float dartboardY = MathHelper.Lerp(topLeft.Y, bottomRight.Y, yInterp);
            Vector2 strikeDest = new(dartboardX, dartboardY);
            Vector2 offset = strikeDest - c;

            bool stealthStrikeAttack = Projectile.Calamity().stealthStrike;

            // Stealth strike attack: Lightspeed carve
            if (stealthStrikeAttack)
            {
                int carveID = ModContent.ProjectileType<NanoblackPiercingStrike>();
                int carveDamage = Projectile.damage; // same damage ratio as the tesselation itself
                float carveKB = 0f;

                // Carves are spawned with a target X/Y coordinate stored in ai[1] and ai[2].
                // They can and will strike anything on the line indiscriminately.
                var source = Projectile.GetSource_FromThis();
                int carveIdx = Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, carveID, carveDamage, carveKB, Projectile.owner, ai1: dartboardX, ai2: dartboardY);
                if (carveIdx.WithinBounds(Main.maxProjectiles))
                {
                    Projectile carve = Main.projectile[carveIdx];
                    carve.ArmorPenetration += NanoblackReaper.LightspeedCarveArmorPenetration; // Add excessive armor penetration.
                }

                // There is no need to add visuals to the stealth strike. The carves are flashy enough.
            }

            // Standard attack: Hitscan zero-point energy strike
            else
            {
                int zpeID = ModContent.ProjectileType<NanoblackStrike>();
                int zpeDamage = Projectile.damage; // same damage ratio as the tesselation itself
                float zpeKB = 0f;

                var source = Projectile.GetSource_FromThis();
                int zpeIdx = Projectile.NewProjectile(source, strikeDest, Vector2.Zero, zpeID, zpeDamage, zpeKB, Projectile.owner, ai0: target.whoAmI, ai1: offset.X, ai2: offset.Y);
                if (zpeIdx.WithinBounds(Main.maxProjectiles))
                {
                    Projectile zpe = Main.projectile[zpeIdx];
                    zpe.ArmorPenetration += NanoblackReaper.ZeroPointArmorPenetration; // Add excessive armor penetration.

                    // This consistently orients the visuals of the hitscan attack for flair.
                    zpe.direction = zpe.spriteDirection = Projectile.spriteDirection;
                }

                // Draw a bright line of energy between the Tesselation and the spawned strike.
                Vector2 lineVel = 3f * Projectile.velocity;
                float xScale = 0.009f;
                float xShrink = 0.88f;
                Color lineColor = NanoblackReaper.ZeroPointLineColor;
                Particle energyLine = new StaticGlowLine(Projectile.Center, strikeDest, lineVel, 7, xScale, xShrink, lineColor, true);
                GeneralParticleHandler.SpawnParticle(energyLine);

                // Draw a stack of three glow orbs right at the start of the line. One glow orb was not glowy enough.
                int numOrbs = 3;
                float orbScale = 1.5f;
                Vector2 orbVel = lineVel;
                for (int i = 0; i < numOrbs; ++i)
                {
                    Particle energyOrb = new GlowOrbParticle(Projectile.Center, orbVel, false, 15, orbScale, lineColor);
                    GeneralParticleHandler.SpawnParticle(energyOrb);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        // Draws the tesselation's glowmask.
        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float fWidthOverTwo = SpriteWidth / 2f;
            float fHeightOverTwo = Projectile.height / 2f;

            // Make sure the glowmask matches the tesselation's own orientation
            SpriteEffects eff = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                eff = SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(fWidthOverTwo, fHeightOverTwo);
            Main.EntitySpriteDraw(Glow.Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, eff, 0);
        }
    }
}
