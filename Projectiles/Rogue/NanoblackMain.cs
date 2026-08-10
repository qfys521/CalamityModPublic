using System.IO;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackMain : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/NanoblackReaper";

        internal const int UpdatesPerFrame = 3;
        private const int Lifetime = 240;
        private static int InternalLifetime => Lifetime * UpdatesPerFrame;
        private const float BoomerangReturnTime = 16f;

        private const int BaseTesselationDelay = 4;
        private static int InternalTesselationDelay => BaseTesselationDelay * UpdatesPerFrame;
        private const float TesselationSpawnSpeed = 24f;

        internal const float RotationIncrement = 0.22f;

        private static readonly SoundStyle LightspeedMissSound = new("CalamityMod/Sounds/Item/NanoblackReaper/NanoblackReaper_LightspeedMiss")
        {
            Volume = 0.8f,
            PitchVariance = 0.1f,
            MaxInstances = 8,
        };

        private static readonly SoundStyle LightspeedPerfectMissSound = new("CalamityMod/Sounds/Item/NanoblackReaper/NanoblackReaper_LightspeedMissPerfect")
        {
            Volume = 0.9f,
            PitchVariance = 0.08f,
            MaxInstances = 8,
        };

        private static readonly SoundStyle LightspeedSlashBaseSound = new("CalamityMod/Sounds/Item/NanoblackReaper/NanoblackReaper_LightspeedSlash")
        {
            Volume = 0.85f,
            PitchVariance = 0.08f,
            MaxInstances = 10,
        };

        private static readonly SoundStyle LightspeedSlashVariantSound = new("CalamityMod/Sounds/Item/NanoblackReaper/NanoblackReaper_LightspeedSlash", 3)
        {
            Volume = 0.65f,
            PitchVariance = 0.12f,
            MaxInstances = 10,
        };

        private static readonly SoundStyle LightspeedPerfectSlashSound = new("CalamityMod/Sounds/Item/NanoblackReaper/NanoblackReaper_PerfectLightspeedSlash")
        {
            Volume = 0.95f,
            PitchVariance = 0.06f,
            MaxInstances = 8,
        };

        private Player Owner => Main.player[Projectile.owner];
        internal ref float RealFrameCounter => ref Projectile.ai[0];
        internal ref float TesselationSpawnCooldown => ref Projectile.ai[1];

        // 0f = Initial state
        // 1f = Mouse button still held: Can be pulled back, but isn't in the perfect window
        // 2f = Mouse button still held: Can be pulled back, will be perfect
        // 3f = Mouse button no longer held
        internal ref float LightspeedCarveState => ref Projectile.ai[2];
        internal const float LightspeedCarveState_Initial = 0f;
        internal const float LightspeedCarveState_CanImperfect = 1f;
        internal const float LightspeedCarveState_CanPerfect = 2f;
        internal const float LightspeedCarveState_Performed = 3f;

        internal bool Returning
        {
            get => Projectile.localAI[0] != 0f;
            set => Projectile.localAI[0] = (value ? 1f : 0f);
        }

        // The frame at which the scythe started rebounding. This is used to time out the Perfect Lightspeed Carve window.
        internal ref float ReboundStartFrame => ref Projectile.localAI[1];

        public override void SetStaticDefaults()
        {
            // Nanoblack Reaper does not spin exactly on the center of its sprite.
            DrawOffsetX = -11;
            DrawOriginOffsetY = -4;
            DrawOriginOffsetX = 0;

            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.MaxUpdates = UpdatesPerFrame;
            Projectile.timeLeft = InternalLifetime;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6 * UpdatesPerFrame;

            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Returning);
        public override void ReceiveExtraAI(BinaryReader reader) => Returning = reader.ReadBoolean();

        // Nanoblack Reaper's AI has been converted into a trenchcoat function due to the needed expansion of the sub-functions.
        public override void AI()
        {
            if (Projectile.timeLeft == InternalLifetime)
                FrameOneEffects();

            // Nanoblack Reaper enables its owner's mouse listener so that the mouse state is synced.
            // This is necessary for its targeting algorithm.
            if (Owner.whoAmI == Main.myPlayer)
                Owner.Calamity().mouseWorldListener = true;

            InFlightVisualEffects();
            UpdateAIVariables();

            // On the frame the scythe begins returning, send a net update.
            if (RealFrameCounter >= BoomerangReturnTime && RealFrameCounter < BoomerangReturnTime + 1f)
            {
                Returning = true;
                ReboundStartFrame = RealFrameCounter;

                // Also on this frame: The window for a Perfect Lightspeed Carve starts.
                if (LightspeedCarveState == LightspeedCarveState_Initial)
                    LightspeedCarveState = LightspeedCarveState_CanPerfect;

                Projectile.netUpdate = true;
            }

            // The scythe runs its returning AI if the frame counter is greater than ReboundTime.
            if (Returning)
            {
                // During this time, make sure the Perfect Lightspeed Carve window expires properly.
                if (LightspeedCarveState == LightspeedCarveState_CanPerfect)
                {
                    bool perfectWindowPassed = RealFrameCounter > ReboundStartFrame + NanoblackReaper.PerfectLightspeedCarveFrames;
                    if (perfectWindowPassed)
                        LightspeedCarveState = LightspeedCarveState_CanImperfect;
                }

                // Also ensure that the Imperfect Lightspeed Carve window expires properly.
                else if (LightspeedCarveState == LightspeedCarveState_CanImperfect)
                {
                    bool imperfectWindowPassed = RealFrameCounter > ReboundStartFrame + NanoblackReaper.PerfectLightspeedCarveFrames + NanoblackReaper.ImperfectLightspeedCarveFrames;
                    if (imperfectWindowPassed)
                        LightspeedCarveState = LightspeedCarveState_Performed;
                }

                BoomerangMovement();
            }

            // Spawn Nanoblack Tesselations at a consistent and overwhelming rate while in flight.
            // PENDING TESTING: Tesselations are not spawned if a scythe has been pulled for a Lightspeed Carve.
            bool spawnTesselations = Projectile.Calamity().stealthStrike || /* LightspeedCarveState != LightspeedCarveState_Performed */ true;
            if (spawnTesselations && TesselationSpawnCooldown <= 0f)
            {
                SpawnTesselation();
                TesselationSpawnCooldown = InternalTesselationDelay;
            }

            RotateScytheInFlight();
        }

        private void FrameOneEffects()
        {
            // If you set these values, you were a fool. Nanoblack Reaper does not care.
            RealFrameCounter = 0f;
            TesselationSpawnCooldown = InternalTesselationDelay;

            // Lightspeed Carves are not enabled on Focus Flurry attacks.
            LightspeedCarveState = Projectile.Calamity().stealthStrike ? LightspeedCarveState_Performed : LightspeedCarveState_Initial;
        }

        // Produces electricity and green firework sparks constantly while in flight.
        private void InFlightVisualEffects()
        {
            if (!Main.rand.NextBool(UpdatesPerFrame))
                return;

            int dustType = Main.rand.NextBool(5) ? ModContent.DustType<VoidDust>() : ModContent.DustType<VoidDustInverted>();

            Vector2 position = Main.rand.NextVector2FromRectangle(Projectile.Hitbox);
            float scale = Main.rand.NextFloat(0.8f, 1.1f);
            float velocityMult = Main.rand.NextFloat(0.3f, 0.6f);

            Dust d = Dust.NewDustPerfect(position, dustType, Vector2.Zero, Scale: scale);
            if (d is null || d.dustIndex == Main.maxDust)
                return;

            d.color = NanoblackReaper.NanoblackDustColor1;
            d.noGravity = true;
            d.velocity = velocityMult * Projectile.velocity;
        }

        private void UpdateAIVariables()
        {
            // Only increment the real frame counter once per frame, on the final extra update of that frame.
            if (Projectile.FinalExtraUpdate())
            {
                ++RealFrameCounter;
            }

            // Tesselation spawn cooldown decrements every update so that it may be out of sync with gameplay frames if needed.
            --TesselationSpawnCooldown;
        }

        private void BoomerangMovement()
        {
            Player owner = Owner;
            Vector2 toOwner = Projectile.SafeDirectionTo(owner.Center, -Vector2.UnitY);
            float baseReturnSpeed = NanoblackReaper.Speed;
            float currentReturnSpeed = baseReturnSpeed;

            // Nanoblack Reaper's return speed increases sharply if it remains in flight for too long.
            float returnSpeedIncreaseTime = BoomerangReturnTime * 2f;
            if (RealFrameCounter >= returnSpeedIncreaseTime)
                currentReturnSpeed *= 1f + 0.05f * (RealFrameCounter - returnSpeedIncreaseTime);

            // Lerp into the desired velocity every update.
            Vector2 desiredVelocity = currentReturnSpeed * toOwner;
            float returnSharpness = 0.04f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, returnSharpness);

            // Delete the projectile if it touches its owner.
            if (Main.myPlayer == Projectile.owner)
                if (Projectile.Hitbox.Intersects(owner.Hitbox))
                    Projectile.Kill();
        }

        // Spawns a triangular lattice of three Nanoblack Tesselations. All tesselations emit from the blade of the scythe.
        private void SpawnTesselation()
        {
            int numTessSpawns = 3;

            // Each tesselation spawns with a random delay before it chooses to fire.
            // For consistent RNG iteration, these RNG values are obtained even if they are not needed.
            static float GetStrikeDelay() => Main.rand.NextFloat(NanoblackTesselation.MinDelay, NanoblackTesselation.MaxDelay);
            float[] zeroPointStrikeDelays = new float[numTessSpawns];
            for (int i = 0; i < numTessSpawns; ++i)
                zeroPointStrikeDelays[i] = GetStrikeDelay();

            if (Main.myPlayer != Projectile.owner)
                return;

            int tessID = ModContent.ProjectileType<NanoblackTesselation>();
            int tessDamage = (int)(NanoblackReaper.TesselationDamageRatio * Projectile.damage);
            float tessKB = NanoblackReaper.TesselationKnockback;

            // The blade of Nanoblack Reaper is close enough to straight-right +X that using the rotation directly is fine.
            float scytheBladeRotation = Projectile.rotation * Projectile.spriteDirection;
            Vector2 spawnOffsetDir = scytheBladeRotation.ToRotationVector2();
            Vector2 tessPos = Projectile.Center + spawnOffsetDir * 14f;
            Vector2 tessVelDir = spawnOffsetDir.RotatedBy(-MathHelper.PiOver4); // close enough to a blade-egress vector
            Vector2 tessBaseVel = tessVelDir * TesselationSpawnSpeed;

            var source = Projectile.GetSource_FromThis();

            for (int i = 0; i < numTessSpawns; ++i)
            {
                Vector2 tessVel = tessBaseVel.RotatedBy(i * NanoblackReaper.TwoPiOver3);
                float delay = zeroPointStrikeDelays[i];
                int tessIdx = Projectile.NewProjectile(source, tessPos, tessVel, tessID, tessDamage, tessKB, Projectile.owner, ai0: delay);

                // The spin direction and stealth strike status of the scythe transfers to the tesselations.
                if (tessIdx.WithinBounds(Main.maxProjectiles))
                {
                    Projectile tess = Main.projectile[tessIdx];
                    tess.direction = tess.spriteDirection = Projectile.spriteDirection;
                    tess.Calamity().stealthStrike = Projectile.Calamity().stealthStrike;
                }
            }
        }

        internal void AttemptLightspeedCarve()
        {
            var lcs = LightspeedCarveState;
            if (lcs == LightspeedCarveState_Performed)
                return;

            if (lcs != LightspeedCarveState_Initial && Projectile.owner == Main.myPlayer)
            {
                int projType = ModContent.ProjectileType<NanoblackLightspeedCarve>();
                int damage = Projectile.damage * 6;
                float kb = NanoblackReaper.LightspeedCarveKnockback;
                var source = Projectile.GetSource_FromThis();

                Vector2 pos = Projectile.Center;

                // As is Nanoblack tradition, Lightspeed Carves prefer to target bosses whenever possible.
                NPC target = Owner.ClampedMouseWorld().ClosestNPCAt(NanoblackLightspeedCarve.TargetingRange, bossPriority: true);

                // If the first cursor-based targeting attempt fails, try again near the Tesselation itself.
                if (target is null || !target.active)
                    target = Projectile.Center.ClosestNPCAt(NanoblackLightspeedCarve.TargetingRange, bossPriority: true);

                bool foundTarget = target is not null && target.active;
                if (foundTarget)
                    pos = target.Center;
                else
                {
                    SoundStyle missSound = lcs == LightspeedCarveState_CanPerfect ? LightspeedPerfectMissSound : LightspeedMissSound;
                    SoundEngine.PlaySound(missSound, Projectile.Center);
                }

                float fuzz = NanoblackLightspeedCarve.PlacementRandomness;
                pos += Main.rand.NextVector2Circular(fuzz, fuzz);

                if (foundTarget)
                    PlayLightspeedCarveSounds(lcs == LightspeedCarveState_CanPerfect, pos);

                if (lcs == LightspeedCarveState_CanPerfect)
                {
                    Projectile perf = Projectile.NewProjectileDirect(source, pos, Vector2.Zero, projType, damage, kb, Projectile.owner, ai0: 1f);
                    var cgp = perf.Calamity();
                    cgp.supercritHits = -1;
                    cgp.bonusCritDamage += 1f;
                }
                else if (lcs == LightspeedCarveState_CanImperfect)
                {
                    Projectile.NewProjectile(source, pos, Vector2.Zero, projType, damage, kb, Projectile.owner, ai0: 0f);
                }
            }

            // In all cases, all further attempts are blocked permanently.
            LightspeedCarveState = LightspeedCarveState_Performed;
            Projectile.netUpdate = true;
        }

        private static void PlayLightspeedCarveSounds(bool perfect, Vector2 position)
        {
            if (perfect)
                SoundEngine.PlaySound(LightspeedPerfectSlashSound, position);
            else
            {
                SoundEngine.PlaySound(LightspeedSlashVariantSound, position);
            }
        }

        private void RotateScytheInFlight()
        {
            float spin = Projectile.direction <= 0 ? -1f : 1f;
            Projectile.rotation += spin * RotationIncrement;

            // When thrown left, Nanoblack Reaper is still thrown scythe-head first.
            Projectile.spriteDirection = Projectile.direction;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // No gameplay effects; just spawns slash impact particles at a slightly random angle.
            Color color = NanoblackReaper.NanoblackSlashColor1;
            float scale = 0.12f;
            Vector2 slashDir = -Projectile.velocity.SafeNormalize(-Vector2.UnitY);
            Vector2 vel = 0.01f * slashDir.RotatedByRandom(MathHelper.Pi / 8f);

            // scale of void sparks is arbitrarily multiplied by 0.357f. thanks!
            float voidScale = scale / 0.357f;
            Particle blackSpark = new VoidSparkParticle(Projectile.Center, vel, false, 12, voidScale, color, 1f);
            GeneralParticleHandler.SpawnParticle(blackSpark);

            float glowScale = scale * 0.333f;
            Vector2 squashStretch = new(1.3333f, 0.8f);
            Particle innerSpark = new GlowSparkParticle(Projectile.Center, vel, false, 11, glowScale, color, squashStretch, true, true, 1f);
            GeneralParticleHandler.SpawnParticle(innerSpark);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
