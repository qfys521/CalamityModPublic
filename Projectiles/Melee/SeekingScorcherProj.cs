using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Melee
{
    public class SeekingScorcherProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/SeekingScorcher";

        public static int WindupSwingTime = 24;
        public static int MaxAirtime = 60;
        public static int CrystalTransformationTime = 30;
        public static int Fadetime = 30;
        public static int Lifetime = 600;
        public static float ExplosionDamageMult = 3f;
        public static float CrystalBounceDamageMult = 2f; // Exponential scaling for multiple bounces but capped at max bounces
        public static float CrystalBounceSpeedMult = 1.25f;
        public static int MaxBounces = 3;
        public const float MaxNPCHomingRange = 960f; // 60 tiles + up to 3x empowerment boosts
        public const float MaxAxeHomingRange = 640f; // 40 tiles + up to 2x empowerment boosts
        public const float BonusRangePerEmpowerment = 80f; // 5 tiles

        private List<int> PreviousNPCs = new List<int>() { -1 };
        public Player Owner => Main.player[Projectile.owner];

        public ref float WindupSwingProgress => ref Projectile.ai[0];
        public ref float PhaseTime => ref Projectile.ai[1];
        public ref float CrystalTimer => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = Lifetime + WindupSwingTime;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool ShouldUpdatePosition() => WindupSwingProgress > 1f;

        // Can deal damage if already thrown and not crystal
        public override bool? CanDamage() => WindupSwingProgress <= 1f || CrystalTimer >= CrystalTransformationTime ? false : base.CanDamage();

        // Swing animation keys
        public CurveSegment pullback = new CurveSegment(EasingType.PolyOut, 0f, 0f, MathHelper.PiOver4 * -1.2f, 2);
        public CurveSegment throwout = new CurveSegment(EasingType.PolyOut, 0.7f, MathHelper.PiOver4 * -1.2f, MathHelper.PiOver4 * 1.2f + MathHelper.PiOver2, 3);
        internal float ArmAnticipationMovement() => PiecewiseAnimation(WindupSwingProgress, new CurveSegment[] { pullback, throwout });

        public override void AI()
        {
            // Wind-up swing animation
            if (WindupSwingProgress <= 1f)
            {
                // Initialize the actual original projectile damage. This is used for the crystal boost later
                if (PhaseTime == 0f)
                    Projectile.originalDamage = Projectile.damage;

                WindupSwingProgress = Utils.GetLerpValue(0f, WindupSwingTime, PhaseTime, true);
                PhaseTime++;

                // Recalibrate and perform the throw when ready
                if (WindupSwingProgress == 1f)
                {
                    WindupSwingProgress = 2f;
                    PhaseTime = 0f;
                    Projectile.Center = Owner.MountedCenter + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 72f;
                    // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                    Projectile.velocity = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction) * Projectile.velocity.Length();
                    Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X >= 0f).ToDirectionInt();
                    Projectile.netUpdate = true;
                    SoundEngine.PlaySound(SeekingScorcher.ThrowSound, Projectile.Center);
                    return;
                }

                // Hold and swing
                float armRotation = ArmAnticipationMovement() * Owner.direction;
                Projectile.Center = Owner.MountedCenter + Vector2.UnitY.RotatedBy(armRotation * Owner.gravDir) * -72f * Owner.gravDir;
                Projectile.rotation = ((Owner.direction == 1 ? 0f : -MathHelper.PiOver2) - MathHelper.PiOver4 + armRotation) * Owner.gravDir;
                Projectile.spriteDirection = Projectile.direction = Owner.direction;

                Owner.heldProj = Projectile.whoAmI;
                Owner.ChangeDir(MathF.Sign(Main.MouseWorld.X - Owner.Center.X));
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);
                return;
            }
            // Crystal transformation and collision explosion
            if (CrystalTimer > 0f)
            {
                if (CrystalTimer >= CrystalTransformationTime)
                {
                    Projectile.velocity = Vector2.Zero;

                    foreach (Projectile proj in Main.ActiveProjectiles)
                    {
                        // Only allow other axes to hit the crystal axe
                        if (proj.type != Type)
                            continue;

                        // Check for if the axe is thrown and not turning crystal
                        if (proj.ai[0] > 1f && proj.ai[2] <= 0f && Projectile.Hitbox.Intersects(proj.Hitbox))
                        {
                            Explode();

                            // Cause the axe that just hit to be empowered and counts as a bounce
                            proj.damage = Math.Clamp((int)(proj.damage * CrystalBounceDamageMult), 0, (int)(proj.originalDamage * MathF.Pow(CrystalBounceDamageMult, MaxBounces)));
                            proj.velocity = proj.velocity.RotatedBy(Main.rand.NextBool() ? MathHelper.ToRadians(Main.rand.NextFloat(108f, 162f)) : MathHelper.ToRadians(Main.rand.NextFloat(-162f, -108f))) * CrystalBounceSpeedMult;
                            if (proj.MaxUpdates < 5)
                                proj.MaxUpdates++;

                            proj.ai[1] -= 30f;
                            proj.ai[2] = -1f;
                            proj.netUpdate = true;
                        }
                    }
                }
                else
                {
                    Projectile.velocity *= 0.85f;
                    Projectile.rotation += Projectile.direction * MathHelper.ToRadians(24f) * Utils.GetLerpValue(CrystalTransformationTime, 0f, CrystalTimer, true);
                    CrystalTimer++;
                }

                Projectile.Opacity = Utils.GetLerpValue(0f, Fadetime, Projectile.timeLeft, true);

                if (Projectile.timeLeft <= Fadetime || Main.rand.NextBool(20))
                {
                    Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(14f, 22f);
                    float scale = Main.rand.NextFloat(0.6f, 1.2f);
                    Particle spark = new CritSpark(Projectile.Center, velocity, Color.MistyRose, Color.Pink, scale, 30, 0.1f, 3f, Main.rand.NextFloat(0f, 0.01f));
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                return;
            }

            // Mid-throw AI and stuff
            // Stored value from hitting crystal axes which performs the bounce
            if (CrystalTimer == -1f)
            {
                SharpBounce();
                Projectile.numHits++;
                CrystalTimer--;
            }

            PhaseTime++;
            Projectile.rotation += Projectile.direction * MathHelper.ToRadians(24f);

            // Begin transformation
            if (PhaseTime >= MaxAirtime)
            {
                // Empowered axes return instead
                if (Projectile.numHits >= MaxBounces || CrystalTimer < 0f)
                    ReturnToOwner();
                else
                    CrystalTimer++;
            }

            // Trails
            if (Main.rand.NextBool())
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.CopperCoin, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            if (CrystalTimer < 0f)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, Color.MistyRose, 24, Main.rand.NextFloat(1.6f, 2f), 0.25f, Main.rand.NextFloat(-0.1f, 0.1f), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
        // Glowmask
        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 200);
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);

            // Disallow the NPC to be targeted again
            PreviousNPCs.Add(target.whoAmI);
            SharpBounce();
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SeekingScorcher.HitSound, Projectile.Center);

            Particle boom = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OrangeRed * 0.6f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.1f, 24, true);
            GeneralParticleHandler.SpawnParticle(boom);

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(6f, 8f);
                float sparkScale = Main.rand.NextFloat(1f, 1.4f);
                SparkParticle spark = new SparkParticle(Projectile.Center, sparkVelocity, false, 24, sparkScale, Main.rand.NextBool() ? Color.Gold : Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(spark);

                Vector2 smokeVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(3f, 6f);
                float smokeScale = Main.rand.NextFloat(1.6f, 2f);
                Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVelocity, Color.MistyRose, 24, smokeScale, 0.25f, Main.rand.NextFloat(-0.1f, 0.1f), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // Empowered axe hits
            if (CrystalTimer < 0f)
            {
                for (int i = 0; i < 7; i++)
                {
                    Vector2 maxScale = new Vector2(Main.rand.NextFloat(1.5f, 4f), Main.rand.NextFloat(0.6f, 1.5f));
                    CustomPulse crystal = new CustomPulse(Projectile.Center, Vector2.One, Color.MistyRose, "CalamityMod/Particles/BlastCone", maxScale, Main.rand.NextFloat(MathHelper.TwoPi), 1f, 0.5f, 18);
                    GeneralParticleHandler.SpawnParticle(crystal);
                }

                for (int i = 0; i < 8; i++)
                {
                    Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(16f, 28f);
                    float scale = Main.rand.NextFloat(0.6f, 1.6f);
                    Particle spark = new CritSpark(Projectile.Center, velocity, Color.MistyRose, Color.Pink, scale, 30, 0.1f, 3f, Main.rand.NextFloat(0f, 0.01f));
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.8f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override void OnKill(int timeLeft)
        {
            if (CrystalTimer >= CrystalTransformationTime && PhaseTime > 0f)
            {
                SoundEngine.PlaySound(SeekingScorcher.LightShatterSound, Projectile.Center);
                for (int i = 0; i < 9; i++)
                {
                    Vector2 maxScale = new Vector2(Main.rand.NextFloat(2f, 4.5f), Main.rand.NextFloat(1.4f, 2f));
                    CustomPulse crystal = new CustomPulse(Projectile.Center, Vector2.One, Color.MistyRose, "CalamityMod/Particles/BlastCone", maxScale, Main.rand.NextFloat(MathHelper.TwoPi), 1f, 0.5f, 18);
                    GeneralParticleHandler.SpawnParticle(crystal);
                }

                for (int i = 0; i < 15; i++)
                {
                    Vector2 smokeVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(3f, 8f);
                    float smokeScale = Main.rand.NextFloat(1.6f, 2f);
                    Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVelocity, Color.MistyRose, 24, smokeScale, 0.25f, Main.rand.NextFloat(-0.1f, 0.1f), true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }

        public void SharpBounce()
        {
            // Return if exceeding max number of bounces
            if (Projectile.numHits >= MaxBounces)
            {
                PhaseTime = MaxAirtime;
                ReturnToOwner();
                return;
            }

            float range = MaxNPCHomingRange + BonusRangePerEmpowerment * (Projectile.MaxUpdates - 2);
            int targetNPC = -1;
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!target.CanBeChasedBy(Projectile) || PreviousNPCs.Contains(target.whoAmI))
                    continue;

                float distance = Vector2.Distance(target.Center, Projectile.Center);
                if (distance < range)
                {
                    range = distance;
                    targetNPC = target.whoAmI;
                }
            }

            if (targetNPC != -1)
            {
                // Will only try to bounce towards nearby axes if there are nearby enemies as to not waste axe hits
                // The last usable pierce will never be spent on an axe since it would then just return to the player
                float range2 = MaxAxeHomingRange + BonusRangePerEmpowerment * (Projectile.MaxUpdates - 2);
                int targetAxe = -1;
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.type != Type)
                        continue;

                    // Avoid crystal axes that are fading out
                    float distance = Vector2.Distance(proj.Center, Projectile.Center);
                    if (distance < range2 && proj.ai[2] >= CrystalTransformationTime && proj.timeLeft > Fadetime)
                    {
                        range2 = distance;
                        targetAxe = proj.whoAmI;
                    }
                }

                // Partially restore airtime
                PhaseTime = -12f;
                if (targetAxe != -1 && Projectile.numHits < MaxBounces - 1)
                    Projectile.velocity = Projectile.SafeDirectionTo(Main.projectile[targetAxe].Center) * Projectile.velocity.Length();
                else
                    Projectile.velocity = Projectile.SafeDirectionTo(Main.npc[targetNPC].Center) * Projectile.velocity.Length();
            }
            // If no enemies left, empowered axes just return immediately
            // This also applies to the final bounce since that counts as exhausting all bounces
            // Otherwise, they will simply retain their original velocity
            else if (CrystalTimer < 0f || Projectile.numHits == MaxBounces - 1)
            {
                PhaseTime = MaxAirtime;
                ReturnToOwner();
            }
        }

        public void ReturnToOwner()
        {
            // Swiftly move back towards the player; also make it avoid naturally despawning
            Projectile.velocity = Projectile.SafeDirectionTo(Owner.Center) * Projectile.velocity.Length();
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 2;

            // Delete the Projectile if it touches its owner or too far away.
            if (Projectile.Hitbox.Intersects(Owner.Hitbox) || Vector2.Distance(Projectile.Center, Owner.Center) >= 3000f)
                Projectile.Kill();
        }

        public void Explode()
        {
            PhaseTime = 0f;
            Projectile.Kill();
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ScorcherExplosion>(), (int)(Projectile.damage * ExplosionDamageMult), Projectile.knockBack, Projectile.owner);

            SoundEngine.PlaySound(SeekingScorcher.HitSound, Projectile.Center);
            SoundEngine.PlaySound(SeekingScorcher.ShatterSound, Projectile.Center);

            Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.LightPink, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.65f + 0.1f, 25);
            GeneralParticleHandler.SpawnParticle(explosion);
            Particle explosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumVioletRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.2f, 30);
            GeneralParticleHandler.SpawnParticle(explosion2);
            Particle explosion3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.2f, 30);
            GeneralParticleHandler.SpawnParticle(explosion3);
            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 1f, 6f, 25, true);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0, 0.2f, 25, true, 0.9f);
            GeneralParticleHandler.SpawnParticle(blastRing2);

            for (int i = 0; i < 24; i++)
            {
                CritSpark spark2 = new CritSpark(Projectile.Center, new Vector2(40, 40).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f), Color.White, Color.Orchid, 0.9f, 35, 2f, 2.2f);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
            for (int i = 0; i < 2; i++)
            {
                Particle blast = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.3f, 1.5f, 25, true);
                GeneralParticleHandler.SpawnParticle(blast);
            }
            for (int i = 0; i < 2; i++)
            {
                Particle blast2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.2f, 1.1f, 25, true);
                GeneralParticleHandler.SpawnParticle(blast2);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            //Gain a glow based on how high the charge timer is
            if (CrystalTimer != 0f)
            {
                float fade = Projectile.Opacity * CrystalTimer > 0f ? Utils.GetLerpValue(0, CrystalTransformationTime, CrystalTimer, true) : 1f;
                for (int i = 0; i < 10; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 5 * fade;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, Color.MistyRose with { A = 0 } * fade, drawRotation, rotationPoint, Projectile.scale, flipSprite, 0f);
                }
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
