using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using static Terraria.Player;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Graphics.Primitives;
using System.Linq;
using Terraria.Graphics.Shaders;
using static CalamityMod.CalamityUtils;
using CalamityMod.Sounds;


namespace CalamityMod.Projectiles.Melee
{
    public class LightspeedHoldout : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public override string Texture => "CalamityMod/Items/Weapons/Melee/Lightspeed";
        public ref float attackTimer => ref Projectile.ai[0];
        public Player Owner => Main.player[Projectile.owner];
        public int time = 0;

        // Sprite visuals
        public Vector2 innateOffset = new(23f, -0.1f);
        public Vector2 handPos;
        public float bladeRot = 0;
        public float baseScale = 1;

        // Primary
        public int primaryStabfireRate => 2;
        private int stabTimer;
        public int stabSoundTimer = 3;

        // Secondary
        public bool pressedRight = false;
        public bool firstSecondaryIteration = false;
        public int initialDirectionForThisAnim = 0;
        public ref float DashState => ref Projectile.ai[1];
        public ref float DashTimer => ref Projectile.ai[2];
        private const float DashPrepTime = 40f;
        private const float DashSpeed = 46f;
        private const float DashDuration = 38f;
        private const float DashAcceleration = 0.9865f;
        private float AltSpinRotation = 0f;
        public bool createdSmear = false;
        public float LungeProgression
        {
            get
            {
                float duration = DashDuration;
                float elapsed = DashDuration - (DashTimer * 2);
                return MathHelper.Clamp(elapsed / duration, 0f, 1f);
            }
        }

        // General
        public bool gotEnergyThisSwing = false;
        public override bool? CanDamage() => false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 120;
        }

        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.DamageType = TrueMeleeNoSpeedDamageClass.Instance;
            Projectile.ContinuouslyUpdateDamageStats = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
        }

        public void Positioning(Vector2 toMouse) // Hand and holdout positioning
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;

            bool isDashing = DashState == 2;
            Vector2 baseDir = isDashing ? Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction) : toMouse;
            Owner.ChangeDir(Math.Sign(baseDir.X));

            float playerArmRotation = new Vector2(baseDir.X, baseDir.Y * Owner.gravDir).ToRotation();
            float worldArmRotation = baseDir.ToRotation(); // The absolute world angle to point the projectile

            float compositeArmRotation;

            if (isDashing)
            {
                bladeRot = 0;
                compositeArmRotation = playerArmRotation + (MathHelper.TwoPi * 0.75f);
                handPos = Owner.GetFrontHandPosition(CompositeArmStretchAmount.Full, compositeArmRotation);

                Projectile.Center = handPos;

                // Keep projectile rotation in world space, but invert the offset direction when flipped
                Projectile.rotation = worldArmRotation + (Owner.direction == 1 ? MathHelper.PiOver4 : MathHelper.Pi * 0.75f) * Owner.gravDir;

                Owner.itemRotation = playerArmRotation + MathHelper.PiOver4 - (Owner.direction != 1 ? MathHelper.TwoPi * 0.75f : 0f);
            }
            else // If not in dash
            {
                compositeArmRotation = playerArmRotation + bladeRot - MathHelper.PiOver2;

                Vector2 actualOffset = innateOffset;
                if (Owner.direction == -1)
                {
                    actualOffset.X += 1f;
                    actualOffset.Y += 2f;
                }

                actualOffset.Y *= Owner.gravDir;
                handPos = Owner.GetFrontHandPosition(CompositeArmStretchAmount.Full, compositeArmRotation) + actualOffset.RotatedBy(worldArmRotation);
                Projectile.velocity = toMouse;

                // Adjust sprite rotation logic to compensate for the vertical flip applied in PreDraw
                float rotOffset = MathHelper.PiOver4 * Owner.gravDir;
                if (Owner.direction == -1)
                    rotOffset -= (Owner.gravDir == 1f ? MathHelper.TwoPi * 0.75f : MathHelper.PiOver2);

                Projectile.rotation = worldArmRotation + bladeRot + rotOffset;
                Projectile.Center = handPos + toMouse * Projectile.scale;

                // Simplified: The messy itemRotation calculation mathematically reduces to just this!
                Owner.itemRotation = compositeArmRotation;
            }

            Owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, compositeArmRotation);
            Owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, 0f);
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            var modPlayer = player.GetModPlayer<LightspeedPlayer>();
            baseScale = MathHelper.Lerp(baseScale, player.GetMeleeScale(), 0.3f / Projectile.MaxUpdates);
            Projectile.scale = baseScale;

            if (!Owner.channel && DashState == 0)
            {
                Projectile.Kill();
                return;
            }

            Vector2 toMouse = Utils.DirectionTo(Owner.MountedCenter, Owner.ClampedMouseWorld());
            Positioning(toMouse);

            if (DashState == 0)
            {
                if (Owner.altFunctionUse == 2 && Owner.Calamity().mouseRight)
                {
                    // Check if the player has enough EM
                    if (modPlayer.elementalMastery < 100)
                    {
                        Projectile.Kill();
                        return;
                    }

                    DashState = 1;
                    DashTimer = DashPrepTime;
                    Projectile.localAI[0] = Owner.direction;
                    modPlayer.elementalMastery = 0; // Reset EM to zero
                }
            }

            if (Owner.altFunctionUse == 0 && DashState == 0)
            {
                Projectile.Center += (Utils.DirectionTo(Owner.MountedCenter, Owner.ClampedMouseWorld()) * Main.rand.NextFloat(-5f, 8f)); // When using M1, randomly offset position
                UsePrimary(toMouse);
            }

            if (DashState > 0)
            {
                UseSecondary();
            }
        }

        private void UsePrimary(Vector2 toMouse)
        {
            stabTimer++;
            if (stabTimer % primaryStabfireRate != 0)
                return;

            float offset = Main.rand.NextFloat(-MathHelper.ToRadians(10f), MathHelper.ToRadians(6f));
            Vector2 stabDir = toMouse.RotatedBy(offset);

            Vector2 stabOrigin = Projectile.Center;
            Vector2 stabTip = stabOrigin + stabDir * 62f * Projectile.scale;

            // Spawn spawks extending out of the blade which represent the hitbox
            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPos = stabTip + Main.rand.NextVector2Circular(18f, 12f) * Projectile.scale;
                Vector2 vel = stabDir * Main.rand.NextFloat(5f, 19f);

                Particle spark = new GlowSparkParticle(spawnPos, vel, false, Main.rand.Next(5, 8), Projectile.scale * Main.rand.NextFloat(0.02f, 0.07f), Color.Lerp(Color.Aqua, Color.OrangeRed, Main.rand.NextFloat(1f)) * 0.55f, new Vector2(Main.rand.NextFloat(0.475f, 0.535f), Main.rand.NextFloat(1.2f, 1.3f)), true, false);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            bool flipParticle = Owner.gravDir == -1 ? Owner.direction == 1 : Owner.direction == -1;

            float rotOffset = Owner.gravDir == -1 ? -MathHelper.PiOver4 : MathHelper.PiOver4;
            Vector2 afterImageDir = Owner.direction == -1 ? stabDir.RotatedBy(rotOffset) : stabDir.RotatedBy(-rotOffset);

            Particle afterImage = new CustomSpark(
                Projectile.Center + (stabDir * 10f) + Main.rand.NextVector2Circular(4f, 11f),
                afterImageDir,
                new("CalamityMod/Items/Weapons/Melee/Lightspeed"),
                false,
                Main.rand.Next(5, 9),
                0.6f * Main.rand.NextFloat(0.9f, 1.02f),
                Color.White * Main.rand.NextFloat(0.66f, 0.825f),
                new Vector2(1, 1),
                true,
                false,
                flipHorizontal: flipParticle
            );
            GeneralParticleHandler.SpawnParticle(afterImage);

            // Make blade randomly vibrate and draw with quirks
            bladeRot = Main.rand.NextFloat(-0.12f, 0.3f) * Owner.direction;
            Projectile.scale *= Main.rand.NextFloat(0.75f, 1f);
            Projectile.Center += stabDir + Main.rand.NextVector2Circular(7f, 1.5f);
            Projectile.Opacity = 0.925f;

            Owner.itemRotation += bladeRot * 0.1f;
            // Make arm randomly vibrate
            if (Main.rand.NextBool())
                Owner.SetCompositeArmFront(true, Main.rand.NextBool() ? CompositeArmStretchAmount.ThreeQuarters : CompositeArmStretchAmount.Quarter, Owner.itemRotation);

            stabSoundTimer++;
            if (stabSoundTimer % 3 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.65f, MaxInstances = -1 }, Projectile.Center);
                stabSoundTimer = 0;
            }

            // Spawn the hitbox
            Projectile attack = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Owner.MountedCenter + toMouse * 20 * Projectile.scale, toMouse, ModContent.ProjectileType<LightspeedM1Hitbox>(), Projectile.damage, 0, Projectile.owner);
            attack.scale = Projectile.scale;
        }

        private void UseSecondary()
        {
            Vector2 stabOrigin = Owner.MountedCenter;
            Vector2 stabOffset = stabOrigin + Utils.DirectionTo(Owner.MountedCenter, Owner.ClampedMouseWorld()) * 30f;
            Projectile.Center = stabOffset;


            if (DashState == 1) // Initial delay
            {
                DashTimer--;
                Owner.heldProj = Projectile.whoAmI;
                Owner.itemTime = Owner.itemAnimation = 2;

                Projectile.scale = baseScale * 0.6f;

                if (!firstSecondaryIteration)
                {
                    SoundEngine.PlaySound(CommonCalamitySounds.MeatySlashSound with { Volume = 0.4f, Pitch = -0.05f }, Projectile.Center);
                    initialDirectionForThisAnim = Owner.direction; // Set for rest of the pre-dash anim so weird stuff doesnt happen when changing directions
                    firstSecondaryIteration = true;
                }

                // Spin the blade around
                if (DashTimer > 6)
                {
                    float duration = DashPrepTime - 6;
                    float elapsed = duration - DashTimer;

                    float t = MathHelper.Clamp(elapsed / duration, 0f, 1f);
                    float eased = MathF.Pow(t, 1.2f);

                    AltSpinRotation = eased * (4f * MathHelper.Pi * 0.7175f);

                    float orbitalAngle = (AltSpinRotation * initialDirectionForThisAnim + (initialDirectionForThisAnim == -1 ? MathHelper.Pi : 0)) * Owner.gravDir;
                    float orbitRadius = 40f;

                    Projectile.Center = Owner.MountedCenter + orbitalAngle.ToRotationVector2() * orbitRadius;

                    // Proper arm positioning
                    float rotOffset = MathHelper.PiOver4 * Owner.gravDir;
                    if (initialDirectionForThisAnim == -1)
                        rotOffset -= (Owner.gravDir == 1f ? MathHelper.TwoPi * 0.75f : MathHelper.PiOver2);

                    Projectile.rotation = orbitalAngle + rotOffset;

                    Vector2 armDirection = Projectile.Center - Owner.MountedCenter;
                    float playerArmRotation = new Vector2(armDirection.X, armDirection.Y * Owner.gravDir).ToRotation();

                    float compositeArmRotation = playerArmRotation + (initialDirectionForThisAnim == 1 ? MathHelper.TwoPi * 0.75f : -MathHelper.PiOver2);
                    Owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, compositeArmRotation);
                    Owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, 0f);

                    Owner.itemRotation = MathHelper.WrapAngle(compositeArmRotation);
                }

                // Mid-spin VFX
                if (DashTimer < 32 && DashTimer > 8)
                {
                    if (DashTimer % 4 == 0)
                    {
                        Vector2 pos = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * 60f;
                        Particle sparkle = new CritSpark(pos, new Vector2(7f, 0).RotatedBy(Projectile.rotation), Color.Lerp(Color.Aqua, Color.MediumPurple, Main.rand.NextFloat(1f)), Color.White * 0.33f, 1.2f, 12, 0.3f, 1.2f, hueShift: 0.06f);
                        GeneralParticleHandler.SpawnParticle(sparkle);
                    }

                    Particle smear = new CircularSmearVFX(Owner.MountedCenter, Color.Aqua * 0.4f, Projectile.rotation, Projectile.scale * 1.66f);
                    GeneralParticleHandler.SpawnParticle(smear);
                }

                // Initiate dash
                if (DashTimer <= 0)
                {
                    DashState = 2;
                    DashTimer = DashDuration;

                    Vector2 toMouse = Owner.MountedCenter.DirectionTo(Owner.ClampedMouseWorld());
                    Projectile.velocity = toMouse * DashSpeed;

                    Owner.mount?.Dismount(Owner);
                    Owner.RemoveAllGrapplingHooks();

                    SoundEngine.PlaySound(Exoblade.DashSound, Owner.MountedCenter);

                    SoundStyle otherSound = new("CalamityMod/Sounds/Item/OmicronBeam");
                    SoundEngine.PlaySound(otherSound with { Volume = 0.6f, Pitch = Main.rand.NextFloat(0.2f, 0.25f) }, Projectile.Center);

                    Owner.immune = true;
                    Owner.immuneNoBlink = true;
                    Owner.immuneTime = (int) DashDuration + 2;
                    for (int k = 0; k < Owner.hurtCooldowns.Length; k++)
                        Owner.hurtCooldowns[k] = Owner.immuneTime;

                    Projectile dash = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Owner.MountedCenter, Projectile.velocity, ModContent.ProjectileType<LightspeedDashHitbox>(), Projectile.damage * 24, Projectile.knockBack * 4, Projectile.owner);
                    dash.scale = Projectile.scale;
                }
            }

            else if (DashState == 2) // Dashing
            {
                DashTimer--;

                Vector2 dashVelocity = Projectile.velocity;
                Owner.velocity = dashVelocity;
                Owner.ChangeDir(Math.Sign(dashVelocity.X));
                Owner.Calamity().LungingDown = true;

                Projectile.velocity *= DashAcceleration;

                Projectile.scale = baseScale * MathHelper.Lerp(1f, 0.4f, MathF.Pow(1f - DashTimer / DashDuration, 5));

                Owner.heldProj = Projectile.whoAmI;
                Owner.itemTime = Owner.itemAnimation = 2;

                if (DashTimer <= 0)
                {
                    Owner.velocity *= 0.1f;
                    Owner.Calamity().LungingDown = false;
                    Projectile.Kill();
                }
            }
        }

        // Drawcode below is mostly based on Exoblade's dash.
        public float PierceWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float width = Utils.GetLerpValue(0f, 0.2f, completionRatio, true) * Projectile.scale * 24f;
            // Fade it out starkly near the end of the lunge
            width *= (1 - (float)Math.Pow(LungeProgression, 4));
            return width;
        }

        public Color PierceColorFunction(float completionRatio, Vector2 vertexPos) => Color.White * Projectile.Opacity; // The trail color doesnt matter here

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Owner.direction == -1)
                spriteEffects |= SpriteEffects.FlipHorizontally;

            // Account for inverse gravity
            if (Owner.gravDir == -1f)
                spriteEffects |= SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, 0.6f, spriteEffects, 0);

            if (DashState == 2)
                DrawPierceTrail();

            return false;
        }

        public void DrawPierceTrail()
        {
            if (DashState != 2)
                return;

            Main.spriteBatch.EnterShaderRegion();

            Color mainColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f) % 1, Color.Aqua, Color.MediumAquamarine, Color.DarkOrange, Color.OrangeRed);
            Color secondaryColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + 0.2f) % 1, Color.Aqua, Color.MediumAquamarine, Color.DarkOrange, Color.OrangeRed);

            mainColor = Color.Lerp(Color.White, mainColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));

            Vector2 trailOffset = Projectile.Size * 0.5f;
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(mainColor);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();

            // Lightspeed tracks 120 positions in oldPos.
            // Provide 60 points for smoothing, but only render 30
            int numPointsRendered = 30;
            int numPointsProvided = 60;
            var positionsToUse = Projectile.oldPos.Take(numPointsProvided).ToArray();
            PrimitiveRenderer.RenderTrail(positionsToUse, new(PierceWidthFunction, PierceColorFunction, (_,_) => trailOffset, shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), numPointsRendered);

            Main.spriteBatch.ExitShaderRegion();
        }
        public override void OnKill(int timeLeft)
        {
            DashState = 0;
            DashTimer = 0;
            Projectile.scale = baseScale * 0.6f;
        }
    }
}
