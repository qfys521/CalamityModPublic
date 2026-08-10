using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class BigMimicAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            // Set damage
            NPC.damage = NPC.defDamage;

            NPC.knockBackResist = 0.2f * GameDifficultyData.KnockbackToEnemiesMultiplier.Sample(Main.Difficulty);
            NPC.dontTakeDamage = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.reflectsProjectiles = false;
            if (NPC.ai[0] != 7f && Main.player[NPC.target].dead)
            {
                NPC.TargetClosest();
                if (Main.player[NPC.target].dead)
                {
                    NPC.ai[0] = 7f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }

            if (NPC.ai[0] == 0f)
            {
                NPC.TargetClosest();
                Vector2 mimicTargetDirection = Main.player[NPC.target].Center - NPC.Center;
                if (Main.netMode != NetmodeID.MultiplayerClient && (NPC.velocity.X != 0f || NPC.velocity.Y > 100f || NPC.justHit || mimicTargetDirection.Length() < 80f))
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                NPC.ai[1] += 1f;
                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] > 36f)
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 2f)
            {
                Vector2 mimicTargetDirection2 = Main.player[NPC.target].Center - NPC.Center;
                if (Main.netMode != NetmodeID.MultiplayerClient && mimicTargetDirection2.Length() > 600f)
                {
                    NPC.ai[0] = 5f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }

                if (NPC.velocity.Y == 0f)
                {
                    NPC.TargetClosest();
                    NPC.velocity.X *= 0.85f;
                    NPC.ai[1] += 1f;
                    float jumpDelay = 10f + (CalamityWorld.death ? 10f : 20f) * (NPC.life / (float)NPC.lifeMax);
                    float jumpXVelocity = 5f + (CalamityWorld.death ? 7f : 5f) * (1f - NPC.life / (float)NPC.lifeMax);
                    float jumpYVelocity = CalamityWorld.death ? 7f : 5f;
                    if (!Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                        jumpYVelocity += 2f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] > jumpDelay)
                    {
                        NPC.ai[3] += 1f;
                        if (NPC.ai[3] >= 3f)
                        {
                            NPC.ai[3] = 0f;
                            jumpYVelocity *= 2f;
                            jumpXVelocity /= 2f;
                        }

                        NPC.ai[1] = 0f;
                        NPC.velocity.Y -= jumpYVelocity;
                        NPC.velocity.X = jumpXVelocity * NPC.direction;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    NPC.knockBackResist = 0f;
                    NPC.velocity.X *= 0.99f;
                    if (NPC.direction < 0 && NPC.velocity.X > -1f)
                        NPC.velocity.X = -1f;

                    if (NPC.direction > 0 && NPC.velocity.X < 1f)
                        NPC.velocity.X = 1f;
                }

                NPC.ai[2] += 1f;
                if (NPC.ai[2] > (CalamityWorld.death ? 130f : 170f) && NPC.velocity.Y == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    switch (Main.rand.Next(3))
                    {
                        case 0:
                            NPC.ai[0] = 3f;
                            break;
                        case 1:
                            NPC.ai[0] = 4f;
                            NPC.noTileCollide = true;
                            NPC.velocity.Y = CalamityWorld.death ? -12f : -10f;
                            break;
                        case 2:
                            NPC.ai[0] = 6f;
                            break;
                        default:
                            NPC.ai[0] = 2f;
                            break;
                    }

                    if (Main.tenthAnniversaryWorld && NPC.type == NPCID.BigMimicJungle && NPC.ai[0] == 3f && Main.rand.NextBool())
                        NPC.ai[0] = 8f;

                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 3f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.velocity.X *= 0.85f;
                NPC.dontTakeDamage = true;
                NPC.ai[1] += 1f;
                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] >= (CalamityWorld.death ? 60f : 90f))
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }

                if (Main.expertMode)
                {
                    NPC.ReflectProjectiles(NPC.Hitbox);
                    NPC.reflectsProjectiles = true;
                }
            }
            else if (NPC.ai[0] == 4f)
            {
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.knockBackResist = 0f;
                if (NPC.velocity.X < 0f)
                    NPC.direction = -1;
                else
                    NPC.direction = 1;

                NPC.spriteDirection = NPC.direction;
                NPC.TargetClosest();
                Vector2 mimicTargetCenter = Main.player[NPC.target].Center;
                mimicTargetCenter.Y -= 350f;
                Vector2 mimicTargetDirection = mimicTargetCenter - NPC.Center;
                if (NPC.ai[2] == 1f)
                {
                    NPC.ai[1] += 1f;
                    mimicTargetDirection = Main.player[NPC.target].Center - NPC.Center;
                    mimicTargetDirection.Normalize();
                    mimicTargetDirection *= CalamityWorld.death ? 12f : 10f;
                    NPC.velocity = (NPC.velocity * 4f + mimicTargetDirection) / 5f;
                    if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] > 6f)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[0] = 4.1f;
                        NPC.ai[2] = 0f;
                        NPC.velocity = mimicTargetDirection;
                        NPC.netUpdate = true;
                    }
                }
                else if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) < 40f && NPC.Center.Y < Main.player[NPC.target].Center.Y - 300f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 1f;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    mimicTargetDirection.Normalize();
                    mimicTargetDirection *= CalamityWorld.death ? 16f : 14f;
                    NPC.velocity = (NPC.velocity * 5f + mimicTargetDirection) / 6f;
                }
            }
            else if (NPC.ai[0] == 4.1f)
            {
                NPC.knockBackResist = 0f;
                if (NPC.ai[2] == 0f && Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.ai[2] = 1f;

                if (NPC.position.Y + NPC.height >= Main.player[NPC.target].position.Y || NPC.velocity.Y <= 0f)
                {
                    NPC.ai[1] += 1f;
                    if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] > 10f)
                    {
                        NPC.ai[0] = 2f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                        if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                            NPC.ai[0] = 5f;
                    }
                }
                else if (NPC.ai[2] == 0f)
                {
                    NPC.noTileCollide = true;
                    NPC.noGravity = true;
                    NPC.knockBackResist = 0f;
                }

                NPC.velocity.Y += CalamityWorld.death ? 0.3f : 0.25f;
                if (NPC.velocity.Y > (CalamityWorld.death ? 24f : 20f))
                    NPC.velocity.Y = CalamityWorld.death ? 24f : 20f;
            }
            else if (NPC.ai[0] == 5f)
            {
                if (NPC.velocity.X > 0f)
                    NPC.direction = 1;
                else
                    NPC.direction = -1;

                NPC.spriteDirection = NPC.direction;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.knockBackResist = 0f;
                Vector2 chaseTargetDirection = Main.player[NPC.target].Center - NPC.Center;
                chaseTargetDirection.Y -= 4f;
                if (Main.netMode != NetmodeID.MultiplayerClient && chaseTargetDirection.Length() < 200f && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }

                if (chaseTargetDirection.Length() > 10f)
                {
                    chaseTargetDirection.Normalize();
                    chaseTargetDirection *= CalamityWorld.death ? 15f : 12.5f;
                }

                NPC.velocity = (NPC.velocity * 4f + chaseTargetDirection) / 5f;
            }
            else if (NPC.ai[0] == 6f)
            {
                NPC.knockBackResist = 0f;
                if (NPC.velocity.Y == 0f)
                {
                    NPC.TargetClosest();
                    NPC.velocity.X *= 0.8f;
                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] > 5f)
                    {
                        NPC.ai[1] = 0f;
                        NPC.velocity.Y -= 4f;
                        if (Main.player[NPC.target].position.Y + Main.player[NPC.target].height < NPC.Center.Y)
                            NPC.velocity.Y -= 1.25f;

                        if (Main.player[NPC.target].position.Y + Main.player[NPC.target].height < NPC.Center.Y - 40f)
                            NPC.velocity.Y -= 1.5f;

                        if (Main.player[NPC.target].position.Y + Main.player[NPC.target].height < NPC.Center.Y - 80f)
                            NPC.velocity.Y -= 1.75f;

                        if (Main.player[NPC.target].position.Y + Main.player[NPC.target].height < NPC.Center.Y - 120f)
                            NPC.velocity.Y -= 2f;

                        if (Main.player[NPC.target].position.Y + Main.player[NPC.target].height < NPC.Center.Y - 160f)
                            NPC.velocity.Y -= 2.25f;

                        if (Main.player[NPC.target].position.Y + Main.player[NPC.target].height < NPC.Center.Y - 200f)
                            NPC.velocity.Y -= 2.5f;

                        if (!Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                            NPC.velocity.Y -= 2f;

                        NPC.velocity.X = (CalamityWorld.death ? 16 : 14) * NPC.direction;
                        NPC.ai[2] += 1f;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    NPC.velocity.X *= 0.98f;
                    if (NPC.direction < 0 && NPC.velocity.X > -8f)
                        NPC.velocity.X = -8f;

                    if (NPC.direction > 0 && NPC.velocity.X < 8f)
                        NPC.velocity.X = 8f;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= 3f && NPC.velocity.Y == 0f)
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 7f)
            {
                NPC.damage = 0;
                NPC.life = NPC.lifeMax;
                NPC.defense = 9999;
                NPC.noTileCollide = true;
                NPC.alpha += 7;
                if (NPC.alpha > 255)
                    NPC.alpha = 255;

                NPC.velocity.X *= 0.98f;
            }
            else
            {
                if (NPC.ai[0] != 8f)
                    return false;

                NPC.velocity.X *= 0.85f;
                NPC.ai[1] += 1f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (!Main.tenthAnniversaryWorld || NPC.ai[1] >= 180f)
                    {
                        NPC.ai[0] = 2f;
                        NPC.ai[1] = 0f;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] % 20f == 0f)
                    {
                        int num = 10;
                        for (int i = 0; i < num; i++)
                        {
                            int itemID = ItemID.Sets.ItemsForStuffCannon[Main.rand.Next(ItemID.Sets.ItemsForStuffCannon.Length)];
                            int item = Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, itemID, 1, noBroadcast: false, -1, noGrabDelay: true);
                            float randomSpeed = Main.rand.Next(10, 26);
                            Vector2 vector = new Vector2(NPC.position.X + NPC.width * 0.5f, NPC.position.Y + NPC.height * 0.5f);
                            Vector2 vector2 = Main.player[NPC.target].Center - new Vector2(0f, 120f);
                            float targetXDist = vector2.X - vector.X;
                            float targetYDist = vector2.Y - vector.Y;
                            targetXDist += Main.rand.Next(-50, 51) * 0.1f;
                            targetYDist += Main.rand.Next(-50, 51) * 0.1f;
                            float targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);
                            targetDistance = randomSpeed / targetDistance;
                            targetXDist *= targetDistance;
                            targetYDist *= targetDistance;
                            targetXDist += Main.rand.Next(-50, 51) * 0.1f;
                            targetYDist += Main.rand.Next(-50, 51) * 0.1f;
                            Main.item[item].velocity.X = targetXDist;
                            Main.item[item].velocity.Y = targetYDist;
                            Main.item[item].noGrabDelay = 100;
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
                        }
                    }
                }
            }
            return false;
        }
    }
}
