using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    [PierceResistException]
    public class RelicOfDeliveranceSpear : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<RelicOfDeliverance>();
        public ref float time => ref Projectile.ai[0];

        public Vector2 idealVel;
        public int driftFrames = 0;
        public bool flying = false;
        public Player Owner => Main.player[Projectile.owner];

        public int boostTimer = 0;
        public bool boosted => boostTimer > 0;

        public int driftTimer = 0;
        public bool drifting => Main.mouseLeft;
        public int driftPower = 1;
        public float driftPowerScaling = 1;
        public float driftBadMult = 1;
        public bool killed = false;

        public float iframeLevel = 0.2f; // Decides the level of dash iframes it gives when starting a dash, lower is less

        public float velX;
        public float velY;

        public Vector2 respawnPoint;
        public int respawnTimer = 0;
        public bool inTiles = false;
        public float respawnMult = 0;

        public Color bColor = Color.White;
        public SlotId digSoundSlot;
        public int digFXCooldown = 0;
        public float ramLerp;

        public float damageMult = 1;
        public int hitCountDamageSource = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.drawLayer = Terraria.ID.ProjectileDrawLayerID.OverWiresUI;
            Projectile.width = 68;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50 * Projectile.MaxUpdates;
            Projectile.extraUpdates = 3;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }
        public override void AI()
        {
            if (killed)
                return;

            ramLerp = Utils.GetLerpValue(120 * driftPower, 0, boostTimer, true);
            Owner.Calamity().rOfDelivarenceRam = false;

            float rate = Main.GlobalTimeWrappedHourly * 7;
            Color powerColor = Color.Khaki;
            List<Color> eColors = new List<Color>()
            {
                Color.Goldenrod,
                Color.OrangeRed,
                Color.Orange,
                Color.Gold
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            bColor = Color.Lerp(Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f), powerColor, Utils.Remap(ramLerp, iframeLevel, 0.17f, 0.5f, 0, true) * (driftPower == 1 ? 0 : 1));

            if (Owner.dead || Owner.Calamity().mouseRight || !WorldGen.InWorld(Owner.Center.ToTileCoordinates().X, Owner.Center.ToTileCoordinates().Y, 20)) // Die if Owner dies or right clicks to exit
            {
                KillProj();
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4 * Projectile.MaxUpdates)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            // Immediately die if the Owner is not holding the spear
            if (Owner.HeldItem == null)
            {
                KillProj();
                return;
            }
            if (Owner.HeldItem.type != ModContent.ItemType<RelicOfDeliverance>())
            {
                KillProj();
                return;
            }

            float speed = (CalamityPlayer.areThereAnyDamnBosses ? 3f : 6f) * driftBadMult * (inTiles ? 0.65f : 1);

            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Vector2 aimDir = Owner.Calamity().mouseWorld;
            
            idealVel = Utils.DirectionTo(Owner.Center, aimDir) * speed * driftPower;
            driftPowerScaling = MathHelper.Lerp(driftPowerScaling, driftPower * 0.7f, (driftPowerScaling > driftPower * 0.7f) ? 0.005f : 0.04f);

            if (time == 0)
            {
                Projectile.velocity = idealVel * 0.2f;
                velX = Projectile.velocity.X;
                velY = Projectile.velocity.Y;
            }

            float lerpPower = (drifting ? 0 : (boosted && driftPower > 1) ? 0.01f * ramLerp : 0.02f);
            velX = MathHelper.Lerp(Projectile.velocity.X, idealVel.X, lerpPower);
            velY = MathHelper.Lerp(Projectile.velocity.Y, idealVel.Y, lerpPower);
            
            Projectile.velocity = new Vector2(velX, velY);
            if (Projectile.velocity.Length() <= speed && !drifting)
            {
                Projectile.velocity *= 1.05f;
            }

            if (drifting)
            {
                if (driftTimer == 0)
                    driftPower = 1;

                boostTimer = 0;

                if (time % Projectile.MaxUpdates == 0)
                {
                    if (Projectile.velocity.Length() > 1)
                    {
                        driftBadMult = 1;
                        driftTimer++;
                        driftPower = (driftTimer > 100 ? 3 : driftTimer > 40 ? 2 : 1);
                    }
                    else
                    {
                        driftPower = 1;
                        driftTimer = (int)(MathHelper.Clamp(driftTimer * 0.94f, 1, 300));
                    }
                    Projectile.velocity *= (1f - 0.02f * (CalamityPlayer.areThereAnyDamnBosses ? 0.5f : 1));
                }

                Projectile.extraUpdates = 3;
                float sparkPower = Utils.GetLerpValue(0, 100, driftTimer);
                if (time % Projectile.MaxUpdates * 2 == 0)
                {
                    SoundStyle sound = new("CalamityMod/Sounds/Item/NullImpact");
                    SoundEngine.PlaySound(sound with { Volume = 0.045f, Pitch = (Main.rand.NextFloat(-0.2f, -0.4f) + sparkPower * 0.4f) + (driftPower == 2 ? 0.3f : driftPower == 3 ? 0.7f : 0), MaxInstances = -1 }, Projectile.Center);
                }
                for (int i = 0; i < 2; i++)
                {
                    Color sparkColor = (driftPower == 2 ? Color.Orange : driftPower == 3 ? Color.OrangeRed : Color.Khaki);
                    Vector2 baseVel = Vector2.Lerp(Projectile.velocity, -idealVel, sparkPower);
                    Vector2 vel = baseVel.RotatedByRandom(1.5f - sparkPower * 0.5f) * Main.rand.NextFloat(0.5f, 3f);
                    Particle energy = new VelChangingSpark(Projectile.Center + idealVel * 3, vel + baseVel.SafeNormalize(Vector2.UnitX) * 15, vel, "CalamityMod/Particles/BloomCircle", Main.rand.Next(6, 9 + 1), Main.rand.NextFloat(0.25f, 0.65f) * sparkPower, Color.Lerp(sparkColor, Color.Goldenrod, Main.rand.NextFloat(0, 0.3f)), new Vector2(0.2f, 1f), lerpRate: 0.35f, shrinkSpeed: 0.3f);
                    GeneralParticleHandler.SpawnParticle(energy);
                }
            }
            if (!drifting)
            {
                // Iframes for the ram
                if (driftPower > 1 && ramLerp < iframeLevel && !killed)
                {
                    Owner.Calamity().rOfDelivarenceRam = true;
                    if (Main.rand.NextBool() || driftPower == 3)
                    {
                        Particle spark = new CustomSpark(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 30 + Main.rand.NextVector2Circular(30, 30) * driftPowerScaling, Projectile.velocity * Main.rand.NextFloat(0.2f, 5), "CalamityMod/Particles/FullStar", false, Main.rand.Next(15, 25 + 1), Main.rand.NextFloat(0.6f, 1.2f), Color.Khaki, new Vector2(2, 1), true, false, 0, false, false, 0.8f);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

                if (driftPower > 2)
                    Projectile.extraUpdates = 5;
                else
                    Projectile.extraUpdates = 3;

                if (driftPower == 1 && driftBadMult == 1 && driftTimer > 0)
                {
                    // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                    Projectile.velocity = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld) * (speed * 0.1f);
                    driftBadMult = 0.1f;
                }

                if (time % Projectile.MaxUpdates == 0)
                {
                    if (driftBadMult < 1)
                        driftBadMult += 0.005f;

                    if (boostTimer == 0 && driftTimer > 0)
                    {
                        Projectile.netUpdate = true;
                        driftBadMult = (driftPower > 1 ? 1 : time < 60 ? 0 : 0.35f);
                        driftTimer = 0;

                        // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                        Projectile.velocity = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld) * speed * driftPower;

                        Particle pulse2 = new CustomPulse(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3.1f * driftPower, Color.Goldenrod * 0.8f, "CalamityMod/Particles/BloomRing", new Vector2(0.4f, 1f), Projectile.velocity.ToRotation(), 0f, 1.33f * driftPower, 25);
                        GeneralParticleHandler.SpawnParticle(pulse2);
                        Particle pulse = new CustomPulse(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8.4f * driftPower, Color.OrangeRed * 0.8f, "CalamityMod/Particles/BloomRing", new Vector2(0.4f, 1f), Projectile.velocity.ToRotation(), 0f, 0.92f * driftPower, 25);
                        GeneralParticleHandler.SpawnParticle(pulse);

                        float volume = 0.1f + 0.3f * driftPower;
                        SoundStyle sound = new("CalamityMod/Sounds/Item/HolyColliderSmallHit");
                        SoundEngine.PlaySound(sound with { Volume = volume, Pitch = (-0.7f + 0.1f * driftPower) }, Projectile.Center);
                        if (driftPower > 1)
                        {
                            SoundStyle sound2 = new("CalamityMod/Sounds/Item/LauncherHeavyShot");
                            SoundEngine.PlaySound(sound2 with { Volume = volume * 2, Pitch = (-0.3f - 0.1f * driftPower) }, Projectile.Center);
                        }
                        if (driftPower > 2)
                        {
                            SoundStyle sound3 = new("CalamityMod/Sounds/Item/OpalChargedFire");
                            for (int i = 0; i < 3; i++)
                                SoundEngine.PlaySound(sound3 with { Volume = 0.7f, Pitch = 0.4f, MaxInstances = 3 }, Projectile.Center);
                        }
                        if (driftPower > 1)
                            Owner.SetScreenshake(driftPower == 2 ? 6 : 9);
                        else if (driftBadMult > 0.15f)
                            driftBadMult -= 0.15f;
                            

                        boostTimer = 120 * driftPower;
                        Projectile.numHits = 0;
                        hitCountDamageSource = 0;
                    }
                    if (boostTimer > 0)
                        boostTimer--;
                    if (boostTimer == 0)
                        driftPower = 1;
                }

                if (driftBadMult > 0.2f)
                {
                    float sine = (float)Math.Sin(time * 0.085f / MathHelper.Pi);
                    float mult = driftBadMult * driftPowerScaling;
                    Vector2 tipPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 125 * mult;
                    Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 15.5f;
                    for (int i = 0; i < 2; i++)
                    {
                        offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 15.5f * (i == 0 ? -1 : 1);
                        Particle spark = new VelChangingSpark(tipPos + offset, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) + offset * 0.05f) * 20.5f * mult, -Projectile.velocity.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/SmallBloom", 15, 0.11f * mult, bColor * 0.65f, new Vector2(1, 3.8f), true, false, shrinkSpeed: 0.42f, lerpRate: 0.4f);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    if (driftPower == 3)
                    {
                        Vector2 vel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1, 10);
                        Particle energy = new VelChangingSpark(Projectile.Center + vel, -Projectile.velocity.SafeNormalize(Vector2.UnitX), -Projectile.velocity.SafeNormalize(Vector2.UnitX) + vel * Main.rand.NextFloat(0.01f, 0.02f), "CalamityMod/Particles/BloomCircle", Main.rand.Next(12, 19 + 1), Main.rand.NextFloat(0.55f, 0.95f), Color.Lerp(Color.OrangeRed, Color.Goldenrod, Main.rand.NextFloat(0, 1f)), new Vector2(0.2f, 1f), lerpRate: 0.25f, shrinkSpeed: 0.35f);
                        GeneralParticleHandler.SpawnParticle(energy);
                    }
                    if (time % Projectile.MaxUpdates == 0)
                    {
                        Vector2 vel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) + (offset * (Main.rand.NextBool() ? 1 : -1)) * 0.03f) * 50.5f * mult;
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center + (offset * (Main.rand.NextBool() ? 1 : -1)).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.6f, 1.4f), ModContent.DustType<LightDust>());
                        dust2.scale = Main.rand.NextFloat(0.7f, 1.2f) * mult;
                        dust2.noGravity = true;
                        dust2.velocity = vel * Main.rand.NextFloat(0.6f, 1.4f);
                        dust2.color = bColor;
                    }
                }
            }

            if (Collision.SolidCollision(Projectile.Center, 20, 20))
                inTiles = true;
            else
                inTiles = false;
            if (inTiles)
            {
                if (respawnTimer == 0 && time % Projectile.MaxUpdates == 0)
                {
                    SoundStyle digSound = new("CalamityMod/Sounds/Item/HeavyDig");
                    digSoundSlot = SoundEngine.PlaySound(digSound with { Volume = 0.7f, IsLooped = true }, Projectile.Center);
                    if (digFXCooldown == 0)
                    {
                        SoundStyle sound = new("CalamityMod/Sounds/Item/MagicRockImpact");
                        SoundEngine.PlaySound(sound with { Volume = 0.4f, Pitch = Main.rand.NextFloat(0.3f, 0.7f) }, Projectile.Center);
                        for (int i = 0; i < 13; i++)
                        {
                            Particle spark = new CustomSpark(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 24f), "CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard6", true, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.85f, 2.3f), Color.White, new Vector2(1.1f, 0.8f), false, false, Main.rand.NextFloat(-5, 5), false, false);
                            GeneralParticleHandler.SpawnParticle(spark);
                            if (i % 2 == 0)
                            {
                                MediumMistParticle RockCloud = new MediumMistParticle(Projectile.Center + Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1f, 7f), -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.6f) * Main.rand.NextFloat(1, 12), Color.Peru, Color.Sienna, Main.rand.NextFloat(1.4f, 2.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                                GeneralParticleHandler.SpawnParticle(RockCloud);
                            }
                        }
                        digFXCooldown = 5;
                    }
                }

                if (time % Projectile.MaxUpdates == 0)
                {
                    respawnTimer++;
                    Projectile.soundDelay--;
                }
                if (respawnTimer >= 300)
                {
                    Projectile.netUpdate = true;
                    Projectile.Center = respawnPoint;
                    Owner.Center = respawnPoint;
                    Projectile.velocity *= 0.01f;
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi * i / 15f).ToRotationVector2() * 15.5f * (i % 4 == 0 ? 0.88f : 1f) * Main.rand.NextFloat(0.8f, 1f);
                        float scale = Main.rand.NextFloat(1.3f, 1.6f) * 0.6f * (i % 4 == 0 ? 2.2f : 1.8f);

                        Particle aura = new VelChangingSpark(respawnPoint, vel.RotatedBy(0.02f) * 3, -vel.RotatedBy(-0.04f) * 4, "CalamityMod/Particles/SmallBloom", 27, 0.2f, Color.Goldenrod, new Vector2(1.8f, 1.5f), lerpRate: 0.07f, shrinkSpeed: 0.25f);
                        GeneralParticleHandler.SpawnParticle(aura);

                    }
                    Particle orb2 = new CustomPulse(respawnPoint, Vector2.Zero, Color.Khaki, "CalamityMod/Particles/BloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 2, 0.5f, 28);
                    GeneralParticleHandler.SpawnParticle(orb2);

                    SoundStyle sound = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianHeal");
                    SoundEngine.PlaySound(sound with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.7f, -0.9f) }, respawnPoint);

                    KillProj();
                }
                if (respawnPoint == Vector2.Zero)
                    respawnPoint = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 60;

                if (SoundEngine.TryGetActiveSound(digSoundSlot, out var zound) && zound.IsPlaying)
                {
                    zound.Position = Projectile.Center;
                    zound.Pitch = 0.4f * respawnMult - (drifting ? 0.5f : 0);
                    zound.Volume = (drifting ? 0.6f : 1f);
                }

                respawnMult = Utils.GetLerpValue(60, 300, respawnTimer);
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(digSoundSlot, out var zound))
                    zound?.Stop();
                respawnPoint = Projectile.Center;
                if (respawnTimer > 0)
                {
                    if (digFXCooldown == 0)
                    {
                        SoundStyle sound = new("CalamityMod/Sounds/Item/MagicRockSound");
                        SoundEngine.PlaySound(sound with { Volume = 0.4f, Pitch = Main.rand.NextFloat(0.3f, 0.7f) }, Projectile.Center);
                        for (int i = 0; i < 13; i++)
                        {
                            Particle spark = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 24f), "CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard6", true, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.85f, 2.3f), Color.White, new Vector2(1.1f, 0.8f), false, false, Main.rand.NextFloat(-5, 5), false, false);
                            GeneralParticleHandler.SpawnParticle(spark);
                            if (i % 2 == 0)
                            {
                                MediumMistParticle RockCloud = new MediumMistParticle(Projectile.Center + Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1f, 7f), Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.6f) * Main.rand.NextFloat(1, 12), Color.Peru, Color.Sienna, Main.rand.NextFloat(1.4f, 2.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                                GeneralParticleHandler.SpawnParticle(RockCloud);
                            }
                        }
                        digFXCooldown = 5;
                    }
                }
                respawnTimer = 0;
                respawnMult = 0f;

                if (time % Projectile.MaxUpdates * 2 == 0 && !drifting)
                {
                    SoundStyle sound = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianDash");
                    SoundEngine.PlaySound(sound with { Volume = 0.065f * driftPowerScaling, Pitch = Main.rand.NextFloat(-0.4f, -0.9f) + (driftPower == 3 ? 1.4f : 0), MaxInstances = -1 }, Owner.Center);
                }
            }
            if (time % Projectile.MaxUpdates == 0 && digFXCooldown > 0)
                digFXCooldown--;

            Owner.Center = Projectile.Center;
            Owner.velocity = Projectile.velocity * Projectile.MaxUpdates;
            Owner.dashDelay = 0;
            Projectile.timeLeft++;
            Owner.RemoveAllGrapplingHooks();

            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * (driftPowerScaling + 0.5f));

            Owner.mount?.Dismount(Owner);
            Owner.ChangeDir(Math.Sign(drifting ? idealVel.X : Projectile.velocity.X) <= 0 ? -1 : 1);

            if (!killed)
            {
                float idealRot = (drifting ? idealVel : Projectile.velocity).ToRotation();
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, idealRot, (boostTimer > 0 ? 1 : 0.15f));

                Owner.fullRotationOrigin = Owner.Center - Owner.position;
                Owner.fullRotation = (Owner.direction == -1 ? MathHelper.ToRadians(180f) : 0) + Projectile.rotation + MathHelper.ToRadians(65f * Owner.direction);

                float rot = (drifting ? idealVel : Projectile.velocity).ToRotation() + (Owner.direction == -1 ? MathHelper.ToRadians(270f) : MathHelper.ToRadians(-90f));
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot - Owner.fullRotation);
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rot - Owner.fullRotation);

            }

            time++;
        }
        public void KillProj()
        {
            if (SoundEngine.TryGetActiveSound(digSoundSlot, out var sound))
                sound?.Stop();

            if (inTiles && respawnPoint != Vector2.Zero)
            {
                Projectile.Center = respawnPoint;
                if (!Owner.dead)
                    Owner.Center = respawnPoint;
            }
            killed = true;
            Owner.Calamity().rOfDelivarenceRam = false;
            Projectile.netUpdate = true;
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer && SoundEngine.TryGetActiveSound(digSoundSlot, out var sound))
                sound.Stop();

            Owner.fullRotationOrigin = Owner.Center - Owner.position;
            Owner.fullRotation = 0f;

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.SyncPlayer, number: Owner.whoAmI);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            bool crit = Main.rand.Next(0, 100 + 1) < Owner.GetTotalCritChance(Owner.GetBestClass());
            if (crit)
                modifiers.SetCrit();

            float minMult = 0.15f;
            int hitsToMinMult = 15;
            float damageMult = Utils.Remap(hitCountDamageSource, 0, hitsToMinMult, 1, minMult, true);

            float driftMult = (driftPower == 1 ? 0.3f : driftPower == 2 ? 1.6f : 2.5f) * damageMult; // Drift power scaling is lower than regular
            
            float totalMult = driftMult * ((hitCountDamageSource == 0 && driftPower > 1) ? 1.9f : 1) * (Owner.Calamity().profanedSoulRelicBuff ? 8 : 1);
            modifiers.SourceDamage *= totalMult;

            if (Projectile.numHits == 0 && driftPower > 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    Particle spark = new GlowSparkParticle(Projectile.Center, Projectile.velocity, false, 18, 0.05f * driftPower, Color.Lerp(Color.White, Color.Orange, i * 0.2f) * 0.85f, new Vector2(4 + i * 0.55f, 0.4f + i * 0.1f), true, false, 1f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                SoundStyle sound = new("CalamityMod/Sounds/Item/FinalDawnSlash");
                SoundEngine.PlaySound(sound with { Volume = 1, Pitch = Main.rand.NextFloat(0.2f, 0.4f) * damageMult }, Projectile.Center);
            }
            if (Projectile.soundDelay <= 0)
            {
                SoundStyle sound = new("CalamityMod/Sounds/Item/HolyColliderBigHit");
                SoundEngine.PlaySound(sound with { Volume = 1, Pitch = Main.rand.NextFloat(-0.2f, -0.4f) * damageMult }, Projectile.Center);
                Projectile.soundDelay = 25;
            }
        }
        public override bool? CanDamage() => (Projectile.velocity == Vector2.Zero || drifting) ? false : null;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 30 * driftPowerScaling, 60 * driftPowerScaling, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector2 drawPos = Owner.MountedCenter + (drifting ? idealVel : Projectile.velocity).SafeNormalize(Vector2.UnitX) * (55 - 35 * Utils.GetLerpValue(0, 25, driftTimer, true));
            Projectile.DrawProjectileWithBackglow(Color.Goldenrod with { A = 0 }, Color.White, 3f * driftPowerScaling, effects: Math.Sign((drifting ? idealVel.X : Projectile.velocity.X)) == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None, xPos: drawPos.X, yPos: drawPos.Y);
            //Asset<Texture2D> texBase = ModContent.Request<Texture2D>(Texture);
            //Main.EntitySpriteDraw(texBase.Value, drawPos, null, Color.White, Projectile.rotation, texBase.Size() * 0.5f, Projectile.scale, Math.Sign((drifting ? idealVel.X : Projectile.velocity.X)) == -1 ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically : Microsoft.Xna.Framework.Graphics.SpriteEffects.None);

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;
            Texture2D texture3 = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            if (inTiles)
            {
                float randSize = Main.rand.NextFloat(0.9f, 1.1f);
                Vector2 vel = Utils.DirectionTo(Projectile.Center, respawnPoint);
                for (int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition + vel * 80 * respawnMult, null, Color.Goldenrod with { A = 0 } * respawnMult, vel.ToRotation() + MathHelper.ToRadians(90f), texture2.Size() * 0.5f, new Vector2(1.2f - i * 0.2f, (1.2f + i * 0.3f) * respawnMult) * (0.7f + i * 0.07f) * 0.05f, SpriteEffects.None, 0);
                for (int i = 0; i < 3; i++)
                    Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, (i == 0 ? Color.White : Color.Goldenrod) with { A = 0 } * respawnMult, vel.ToRotation(), texture.Size() * 0.5f, (0.43f + i * 0.08f) * respawnMult, SpriteEffects.None, 0);

                for (int i = 0; i < 2; i++)
                {
                    Main.EntitySpriteDraw(texture, respawnPoint - Main.screenPosition, null, bColor with { A = 0 } * MathHelper.Clamp(respawnMult + 0.5f, 0.5f, 1), vel.ToRotation(), texture.Size() * 0.5f, 0.15f + respawnMult * 0.5f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(texture3, respawnPoint - Main.screenPosition, null, bColor with { A = 0 }, i == 0 ? MathHelper.ToRadians(90f) : 0, texture3.Size() * 0.5f, new Vector2(0.8f, 1.5f) * 1.65f * randSize, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(texture3, respawnPoint - Main.screenPosition, null, Color.White with { A = 0 } * 0.65f, i == 0 ? MathHelper.ToRadians(90f) : 0, texture3.Size() * 0.5f, new Vector2(0.8f, 1.5f) * 1.25f * randSize, SpriteEffects.None, 0);
                }
            }
            if (!drifting)
            {
                Vector2 placement = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * (-20 + 80 * (driftPowerScaling <= 1 ? driftPowerScaling : 1));
                Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmear");
                Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/SemiCircularSmearSwipe");
                for (int i = 0; i < 6; i++)
                {
                    Vector2 scale = new Vector2(0.5f - i * 0.1f, (1.5f + i * 0.15f) * driftBadMult) * (0.75f * driftPowerScaling * Main.rand.NextFloat(0.9f, 1.1f) + 0.25f);
                    Main.EntitySpriteDraw(tex.Value, placement - Main.screenPosition, null, bColor with { A = 0 } * 0.5f * driftBadMult, Projectile.rotation + MathHelper.ToRadians(90f), tex.Size() * 0.5f, scale, SpriteEffects.None);
                }
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool onKill = (target.life <= 0 && target.realLife == -1);
            if (!onKill)
                hitCountDamageSource++;
            else
                hitCountDamageSource -= 3;

            if (hitCountDamageSource < 0)
                hitCountDamageSource = 0;
        }

        // Force the spear to have "priority" when drawing so that it draws over the player.
    }
}
