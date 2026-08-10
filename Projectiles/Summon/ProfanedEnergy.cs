using System;
using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class ProfanedEnergy : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/NPCs/NormalNPCs/ImpiousImmolator";

        private float count = 0f;
        public bool flapping = false;
        public NPC targeted;
        public int maxTargetDistance = 500; // Since they move, the targeting distance is lower than normal

        public int maxRechargeTime = 180;

        public ref float attackTimer => ref Projectile.ai[0];
        public ref float attackCooldown => ref Projectile.ai[1];
        public int attacks = 0;
        public int maxAttacks = 10;

        public int reactTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 60;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.frameCounter++;
            flapping = false;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                if (Projectile.frame == 2)
                    flapping = true;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }

            if (flapping)
                Projectile.velocity += -Vector2.UnitY * 4.6f;
            else
                Projectile.velocity += Vector2.UnitY * 0.2f;

            // Anticlump
            for (int x = 0; x < Main.maxProjectiles; x++)
            {
                Projectile projectile = Main.projectile[x];
                if (Vector2.Distance(Projectile.Center, projectile.Center) <= 70 && projectile.active && projectile.type == Projectile.type && projectile != Projectile)
                {
                    Projectile.velocity += Utils.DirectionFrom(Projectile.Center, projectile.Center) * 0.02f;
                }
            }

            float rate = Main.GlobalTimeWrappedHourly * 2;
            List<Color> eColors = new List<Color>()
            {
                Color.Gold,
                Color.Khaki
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            Color usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            Lighting.AddLight(Projectile.Center, usedColor.ToVector3() * 0.8f);

            if (targeted == null) // Find target or move closer to the player
            {
                Vector2 moveDir = Utils.DirectionTo(Projectile.Center, player.Center);
                // If there's no target and the player isnt too close, flap closer them a bit (woah non stationary sentry crazy)
                if (flapping && Utils.Distance(player.Center, Projectile.Center) > 300)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.3f, Pitch = 0.8f }, Projectile.Center);
                    Projectile.velocity += (moveDir * Main.rand.NextFloat(3f, 5f)).RotatedByRandom(0.4f);

                    for (int j = 0; j < 8; j++)
                    {
                        Dust c = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(35, 35), ModContent.DustType<LightDust>());
                        c.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(3, 7);
                        c.scale = Main.rand.NextFloat(0.5f, 0.7f);
                        c.noGravity = true;
                        c.color = Color.Orchid;
                        c.noLightEmittance = true;
                    }
                }

                if (reactTimer < 600)
                {
                    if (Utils.Distance(player.Center, Projectile.Center) < 120)
                        reactTimer++;
                }
                else
                {
                    SoundStyle happy = new("CalamityMod/Sounds/Item/SanctifiedSparkHappy");
                    SoundEngine.PlaySound(happy with { Volume = 0.5f, PitchVariance = 0.3f }, Projectile.Center);
                    for (int i = 0; i < 3; i++)
                    {
                        Particle spark = new CustomSpark(Projectile.Center, (-Vector2.UnitY * Main.rand.NextFloat(3f, 5f)).RotatedByRandom(0.6f), "CalamityMod/Particles/HeartParticle", false, 75, Main.rand.NextFloat(1.3f, 1.8f), Color.Lerp(Color.Goldenrod, Color.OrangeRed, i * 0.5f), Vector2.One, true, true, 0, false, false);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    reactTimer = Main.rand.Next(0, 120 + 1);
                }

                if (player.HasMinionAttackTargetNPC)
                {
                    targeted = Main.npc[player.MinionAttackTargetNPC];
                    if (!targeted.CanBeChasedBy(Projectile, false) || Utils.Distance(Projectile.Center, targeted.Center) > maxTargetDistance)
                        targeted = null;
                }
                else
                {
                    targeted = Projectile.Center.ClosestNPCAt(maxTargetDistance);
                }

                Projectile.spriteDirection = Math.Sign(-moveDir.X);
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, 0, 0.05f);
            }
            if (targeted != null && ((Utils.Distance(Projectile.Center, targeted.Center) > maxTargetDistance) || attackCooldown > 0 || targeted.active == false || targeted.life <= 0)) // Reset targeting if they leave aggro range or are on attack cooldown
            {
                targeted = null;
            }
            if (targeted != null) // Attacks
            {
                reactTimer = Main.rand.Next(0, 120 + 1);
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 shootVel = Utils.DirectionTo(Projectile.Center, targeted.Center) * 8;
                    Projectile.spriteDirection = Math.Sign(-shootVel.X);
                    if (attackTimer <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (shootVel * Main.rand.NextFloat(0.8f, 1.2f)).RotatedBy(0.75f * (attacks % 2 == 0 ? -1 : 1)).RotatedByRandom(0.4f), ModContent.ProjectileType<FlameBlast>(), Projectile.damage, 0, Projectile.owner);

                        Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Goldenrod, "CalamityMod/Particles/BloomRing", Vector2.One, 0f, 0, Main.rand.NextFloat(0.6f, 0.75f), 15);
                        GeneralParticleHandler.SpawnParticle(orb2);

                        attackTimer = 8;
                        attacks++;
                    }
                    else
                        attackTimer--;

                    if (attacks >= 18)
                    {
                        attackCooldown = maxRechargeTime;
                        attacks = 0;
                    }

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, shootVel.ToRotation() + (Projectile.spriteDirection == -1 ? 0 : MathHelper.ToRadians(180f)), 0.07f);
                }
            }
            if (attackTimer > 0)
                attackTimer--;
            if (attackCooldown > 0)
            {
                int healShots = (int)(player.maxMinions / 4);
                if (healShots != 0 && attackCooldown != maxRechargeTime && (attackCooldown % (maxRechargeTime / (healShots + 1)) == 0))
                {
                    Vector2 vel = ((Projectile.Center - player.Center) - player.velocity * 10).SafeNormalize(Vector2.UnitX) * -10;
                    Projectile healStar = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<HolyLight>(), 0, Projectile.knockBack, Projectile.owner, 0, 5, 5);
                    healStar.extraUpdates = 2;
                    SoundStyle fireHeal = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianDash");
                    SoundEngine.PlaySound(fireHeal with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);

                    Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(54, 209, 54), "CalamityMod/Particles/BloomRing", Vector2.One, 0f, 0, 1f, 24);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }
                attackCooldown--;
            }

            Projectile.velocity *= 0.97f;

            if (count == 0f) // Spawn effects
            {
                reactTimer += Main.rand.Next(0, 120 + 1);
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                for (int i = 0; i < 20; i++)
                {
                    Dust c = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>());
                    c.velocity = (MathHelper.TwoPi * i / 20f).ToRotationVector2() * 15.5f * (i % 2 == 0 ? 0.88f : 1f);
                    c.scale = Main.rand.NextFloat(1.3f, 1.6f) * 0.8f * (i % 2 == 0 ? 2.2f : 1.8f);
                    c.noGravity = true;
                    c.color = Color.Goldenrod;
                    c.noLightEmittance = true;
                }
                count += 1f;
            }
            
        }

        public override bool? CanDamage() => false;
    }
}
