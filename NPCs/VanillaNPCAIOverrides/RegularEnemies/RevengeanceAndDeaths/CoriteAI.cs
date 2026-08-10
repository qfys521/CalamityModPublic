using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class CoriteAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.TargetClosest(false);

            NPC.rotation = NPC.velocity.ToRotation();

            if (Math.Sign(NPC.velocity.X) != 0)
            {
                NPC.spriteDirection = -Math.Sign(NPC.velocity.X);
            }
            if (NPC.rotation < -MathHelper.PiOver2)
            {
                NPC.rotation += MathHelper.Pi;
            }
            if (NPC.rotation > MathHelper.PiOver2)
            {
                NPC.rotation -= MathHelper.Pi;
            }

            if (NPC.type == NPCID.SolarCorite)
            {
                NPC.spriteDirection = Math.Sign(NPC.velocity.X);
            }

            float npcKBResist = 0.4f;

            float upwardChargeSpeed = 12f;
            float idealUpwardDelta = 200f;
            float maximumDistanceBeforeCharge = 900f;
            float upwardMovementIntertia = 30f;

            float chargePhaseWait = 30f;
            float chargeWaitSlowdownMult = 0.95f;
            int chargeRandomness = 50;
            float chargeSpeed = 14f;
            float maximumChargeTime = 30f;
            float chargeDistanceCheck = 100f;
            float chargeIntertia = 20f;
            float chargeAcceleration = 0f;

            // Stops charging if speed is less than this when charging.
            float minimumChargeSpeed = 7f;

            bool hasCoolDustPhase = true;

            if (NPC.type == NPCID.SolarCorite)
            {
                npcKBResist = 0.3f;
                upwardChargeSpeed = 10f;
                idealUpwardDelta = 300f;
                maximumDistanceBeforeCharge = 1000f;
                upwardMovementIntertia = 60f;
                chargePhaseWait = 5f;
                chargeWaitSlowdownMult = 0.8f;
                chargeRandomness = 0;
                chargeSpeed = 10f;
                chargeDistanceCheck = 150f;
                chargeIntertia = 60f;
                chargeAcceleration = 0.333333343f;
                minimumChargeSpeed = 8f;
                hasCoolDustPhase = false;
            }

            chargeAcceleration *= chargeIntertia;

            if (CalamityWorld.death)
            {
                upwardChargeSpeed *= 1.25f;
                chargeSpeed *= 1.25f;
            }

            // Drone dust
            if (NPC.type == NPCID.MartianDrone && NPC.ai[0] != 3f)
            {
                int idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.5f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity = NPC.velocity / 5f;
                Vector2 rotationVector = new Vector2(-10f, 10f);

                if (NPC.spriteDirection == 1)
                {
                    rotationVector.X *= -1f;
                }

                rotationVector = rotationVector.RotatedBy(NPC.rotation);
                Main.dust[idx].position = NPC.Center + rotationVector;
            }

            if (NPC.type == NPCID.SolarCorite)
            {
                int dustSpawnChance = (NPC.ai[0] == 2f) ? 2 : 1;
                int dustSpawnAreaSize = (NPC.ai[0] == 2f) ? 30 : 20;
                for (int i = 0; i < 2; i++)
                {
                    if (Main.rand.Next(3) < dustSpawnChance)
                    {
                        int idx = Dust.NewDust(NPC.Center - new Vector2(dustSpawnAreaSize), dustSpawnAreaSize * 2, dustSpawnAreaSize * 2, DustID.Torch, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 90, default, 1.5f);
                        Main.dust[idx].noGravity = true;
                        Dust dust = Main.dust[idx];
                        dust.velocity *= 0.2f;
                        Main.dust[idx].fadeIn = 1f;
                    }
                }
            }

            // Move upward.
            if (NPC.ai[0] == 0f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.knockBackResist = npcKBResist;
                Vector2 playerDistanceNorm = Main.player[NPC.target].Center - NPC.Center;
                Vector2 upwardVelocity = playerDistanceNorm - Vector2.UnitY * idealUpwardDelta;
                float playerDistance = NPC.Distance(Main.player[NPC.target].Center);
                playerDistanceNorm = Vector2.Normalize(playerDistanceNorm) * upwardChargeSpeed;
                upwardVelocity = Vector2.Normalize(upwardVelocity) * upwardChargeSpeed;
                bool closeAngleDistance = Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1);

                if (NPC.ai[3] >= 120f)
                {
                    closeAngleDistance = true;
                }

                // In simpler terms, this means that we incoporate a bool which states:
                //    The angular distance between this and the player is greater than pi/8 or
                //    less than pi - pi/8.
                // into the already existing boolean.
                closeAngleDistance &= NPC.AngleTo(Main.player[NPC.target].Center) > (MathHelper.Pi / 8f) &&
                    NPC.AngleTo(Main.player[NPC.target].Center) < (MathHelper.Pi - MathHelper.Pi / 8f);

                // If in a relatively close area of the player, or we meet the angle criteria above, prepare for charge.
                if (playerDistance < maximumDistanceBeforeCharge || closeAngleDistance)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[2] = playerDistanceNorm.X;
                    NPC.ai[3] = playerDistanceNorm.Y;
                    NPC.netUpdate = true;
                }
                // Otherwise, resume upward movement.
                else
                {
                    NPC.velocity = (NPC.velocity * (upwardMovementIntertia - 1f) + upwardVelocity) / upwardMovementIntertia;

                    if (!closeAngleDistance)
                    {
                        NPC.ai[3] += 1f;
                        if (NPC.ai[3] == 120f)
                        {
                            NPC.netUpdate = true;
                        }
                    }
                    else
                    {
                        NPC.ai[3] = 0f;
                    }
                }
            }
            // Slow down and prepare for charge.
            else if (NPC.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.knockBackResist = 0f;

                bool decelerate = true;
                if (NPC.type == NPCID.SolarCorite)
                {
                    decelerate = NPC.velocity.Length() > 2f;
                    if (!decelerate && NPC.target >= 0 && !Main.player[NPC.target].dead && !Main.player[NPC.target].ghost)
                    {
                        Vector2 maxVelocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * 0.1f;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, maxVelocity, 0.25f);
                    }
                }

                if (decelerate)
                    NPC.velocity *= chargeWaitSlowdownMult;

                NPC.ai[1] += 1f;
                // If enough time has passed, charge.
                if (NPC.ai[1] >= chargePhaseWait)
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                    Vector2 velocity = new Vector2(NPC.ai[2], NPC.ai[3]) + new Vector2(Main.rand.Next(-chargeRandomness, chargeRandomness + 1), Main.rand.Next(-chargeRandomness, chargeRandomness + 1)) * 0.04f;
                    velocity.Normalize();
                    velocity *= chargeSpeed;
                    NPC.velocity = velocity;
                }

                // Spawn some cool dust.
                if (NPC.type == NPCID.MartianDrone && Main.rand.NextBool(4))
                {
                    int idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.5f);
                    Main.dust[idx].noGravity = true;
                    Dust dust = Main.dust[idx];
                    dust.velocity *= 2f;
                    Main.dust[idx].velocity = Main.dust[idx].velocity / 2f + Vector2.Normalize(Main.dust[idx].position - NPC.Center);
                }
            }
            else if (NPC.ai[0] == 2f)
            {
                // Set damage
                NPC.damage = NPC.defDamage;

                NPC.knockBackResist = 0f;

                NPC.ai[1] += 1f;
                bool aboveAndFar = Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) > chargeDistanceCheck && NPC.Center.Y > Main.player[NPC.target].Center.Y;
                // If time is up and the player is (relatively) far, or the velocity is (relatively) low, reset.
                if ((NPC.ai[1] >= maximumChargeTime & aboveAndFar) || NPC.velocity.Length() < minimumChargeSpeed)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.velocity /= 2f;
                    NPC.netUpdate = true;

                    if (NPC.type == NPCID.SolarCorite)
                    {
                        NPC.ai[1] = 45f;
                        NPC.ai[0] = 4f;
                    }
                }
                else
                {
                    Vector2 distanceNormalized = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);

                    if (distanceNormalized.HasNaNs())
                    {
                        distanceNormalized = new Vector2(NPC.direction, 0f);
                    }

                    NPC.velocity = (NPC.velocity * (chargeIntertia - 1f) + distanceNormalized * (NPC.velocity.Length() + chargeAcceleration)) / chargeIntertia;
                }

                if (hasCoolDustPhase && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.ai[1] -= 3f;
                if (NPC.ai[1] <= 0f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }

                NPC.velocity *= CalamityWorld.death ? 0.9f : 0.95f;
            }

            if (hasCoolDustPhase && NPC.ai[0] != 3f && Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) < 64f)
            {
                NPC.ai[0] = 3f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] = 0f;
                NPC.netUpdate = true;
            }

            // Explode
            if (NPC.ai[0] == 3f)
            {
                // Set damage
                NPC.damage = NPC.defDamage;

                NPC.position = NPC.Center;
                NPC.width = NPC.height = CalamityWorld.death ? 360 : 240;
                NPC.position -= NPC.Size;

                NPC.velocity = Vector2.Zero;

                NPC.alpha = 255;
                Lighting.AddLight((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16, 0.2f, 0.7f, 1.1f);

                for (int i = 0; i < 10; i++)
                {
                    int idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                    Dust dust = Main.dust[idx];
                    dust.velocity *= 1.4f;
                    Main.dust[idx].position = ((float)Main.rand.NextDouble() * MathHelper.TwoPi).ToRotationVector2() * ((float)Main.rand.NextDouble() * 96f) + NPC.Center;
                }

                for (int i = 0; i < 40; i++)
                {
                    int idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.5f);
                    Main.dust[idx].noGravity = true;
                    Dust dust = Main.dust[idx];
                    dust.velocity *= 2f;
                    Main.dust[idx].position = ((float)Main.rand.NextDouble() * MathHelper.TwoPi).ToRotationVector2() * ((float)Main.rand.NextDouble() * 96f) + NPC.Center;
                    Main.dust[idx].velocity = Main.dust[idx].velocity / 2f + Vector2.Normalize(Main.dust[idx].position - NPC.Center);

                    if (Main.rand.NextBool())
                    {
                        idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.9f);
                        Main.dust[idx].noGravity = true;
                        dust = Main.dust[idx];
                        dust.velocity *= 1.2f;
                        Main.dust[idx].position = ((float)Main.rand.NextDouble() * MathHelper.TwoPi).ToRotationVector2() * ((float)Main.rand.NextDouble() * 96f) + NPC.Center;
                        Main.dust[idx].velocity = Main.dust[idx].velocity / 2f + Vector2.Normalize(Main.dust[idx].position - NPC.Center);
                    }

                    if (Main.rand.NextBool(4))
                    {
                        idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.7f);
                        dust = Main.dust[idx];
                        dust.velocity *= 1.2f;
                        Main.dust[idx].position = ((float)Main.rand.NextDouble() * MathHelper.TwoPi).ToRotationVector2() * ((float)Main.rand.NextDouble() * 96f) + NPC.Center;
                        Main.dust[idx].velocity = Main.dust[idx].velocity / 2f + Vector2.Normalize(Main.dust[idx].position - NPC.Center);
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 3f)
                {
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    NPC.life = 0;
                    NPC.HitEffect(0, 10.0);
                    NPC.active = false;
                }
            }

            return false;
        }
    }
}
