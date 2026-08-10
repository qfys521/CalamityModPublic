using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SulphurousSea
{
    public class Trasher : ModNPC
    {
        private bool hasBeenHit = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1,
                Scale = 0.65f,
                PortraitPositionXOverride = 0f,
                Position = Vector2.UnitX * 20f
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 50;
            NPC.width = 150;
            NPC.height = 40;
            NPC.defense = 22;
            NPC.lifeMax = 200;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 3);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0.15f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<TrasherBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SulphurousSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Trasher")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(hasBeenHit);
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            hasBeenHit = reader.ReadBoolean();
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void AI()
        {
            NPC.spriteDirection = (NPC.direction > 0) ? -1 : 1;
            NPC.chaseable = hasBeenHit;
            if (NPC.justHit)
            {
                hasBeenHit = true;
            }
            if (NPC.wet)
            {
                if (NPC.direction == 0)
                {
                    NPC.TargetClosest(true);
                }
                NPC.noTileCollide = false;
                bool canAttack = hasBeenHit || Main.zenithWorld;
                NPC.TargetClosest(false);
                if (Main.player[NPC.target].wet && !Main.player[NPC.target].dead &&
                    Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) &&
                    (Main.player[NPC.target].Center - NPC.Center).Length() < 200f)
                {
                    canAttack = true;
                }
                if (Main.player[NPC.target].dead && canAttack)
                {
                    canAttack = false;
                }
                if (!canAttack)
                {
                    if (NPC.collideX)
                    {
                        NPC.velocity.X = NPC.velocity.X * -1f;
                        NPC.direction *= -1;
                        NPC.netUpdate = true;
                    }
                    if (NPC.collideY)
                    {
                        NPC.netUpdate = true;
                        if (NPC.velocity.Y > 0f)
                        {
                            NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
                            NPC.directionY = -1;
                            NPC.ai[0] = -1f;
                        }
                        else if (NPC.velocity.Y < 0f)
                        {
                            NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
                            NPC.directionY = 1;
                            NPC.ai[0] = 1f;
                        }
                    }
                }
                if (canAttack)
                {
                    NPC.TargetClosest(true);
                    if (Main.zenithWorld)
                        NPC.noTileCollide = true;
                    NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * (CalamityWorld.death ? 0.6f : CalamityWorld.revenge ? 0.45f : 0.3f);
                    NPC.velocity.Y = NPC.velocity.Y + (float)NPC.directionY * (CalamityWorld.death ? 0.2f : CalamityWorld.revenge ? 0.15f : 0.1f);
                    float velocityX = CalamityWorld.death ? 20f : CalamityWorld.revenge ? 15f : 10f;
                    float velocityY = CalamityWorld.death ? 10f : CalamityWorld.revenge ? 7.5f : 5f;
                    if (NPC.velocity.X > velocityX)
                    {
                        NPC.velocity.X = velocityX;
                    }
                    if (NPC.velocity.X < -velocityX)
                    {
                        NPC.velocity.X = -velocityX;
                    }
                    if (NPC.velocity.Y > velocityY)
                    {
                        NPC.velocity.Y = velocityY;
                    }
                    if (NPC.velocity.Y < -velocityY)
                    {
                        NPC.velocity.Y = -velocityY;
                    }
                }
                else
                {
                    NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * 0.1f;
                    if (NPC.velocity.X < -1.5f || NPC.velocity.X > 1.5f)
                    {
                        NPC.velocity.X = NPC.velocity.X * 0.95f;
                    }
                    if (NPC.ai[0] == -1f)
                    {
                        NPC.velocity.Y = NPC.velocity.Y - 0.01f;
                        if ((double)NPC.velocity.Y < -0.3)
                        {
                            NPC.ai[0] = 1f;
                        }
                    }
                    else
                    {
                        NPC.velocity.Y = NPC.velocity.Y + 0.01f;
                        if ((double)NPC.velocity.Y > 0.3)
                        {
                            NPC.ai[0] = -1f;
                        }
                    }
                }
                int npcTileX = (int)(NPC.position.X + (float)(NPC.width / 2)) / 16;
                int npcTileY = (int)(NPC.position.Y + (float)(NPC.height / 2)) / 16;
                if (Main.tile[npcTileX, npcTileY - 1].LiquidAmount > 128)
                {
                    if (Main.tile[npcTileX, npcTileY + 1].HasTile)
                    {
                        NPC.ai[0] = -1f;
                    }
                    else if (Main.tile[npcTileX, npcTileY + 2].HasTile)
                    {
                        NPC.ai[0] = -1f;
                    }
                }
                if ((double)NPC.velocity.Y > 0.4 || (double)NPC.velocity.Y < -0.4)
                {
                    NPC.velocity.Y = NPC.velocity.Y * 0.95f;
                }
            }
            else
            {
                if (Collision.WetCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.noTileCollide = false;
                    NPC.netUpdate = true;
                    return;
                }
                NPC.noTileCollide = false;
                float velocityBoost = 1f;
                NPC.TargetClosest(true);
                bool closeToTargetX = false;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.5 || CalamityWorld.death)
                {
                    velocityBoost = 1.5f;
                }
                if ((double)NPC.life < (double)NPC.lifeMax * 0.25 || CalamityWorld.death)
                {
                    velocityBoost = 2.5f;
                }
                if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) < 20f)
                {
                    closeToTargetX = true;
                }
                if (closeToTargetX)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.9f;
                    if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                    {
                        NPC.velocity.X = 0f;
                    }
                }
                else
                {
                    if (NPC.direction > 0)
                    {
                        NPC.velocity.X = (NPC.velocity.X * 20f + velocityBoost) / 21f;
                    }
                    if (NPC.direction < 0)
                    {
                        NPC.velocity.X = (NPC.velocity.X * 20f - velocityBoost) / 21f;
                    }
                }
                if (NPC.velocity.Y >= 0f)
                {
                    int fallFaceDirection = 0;
                    if (NPC.velocity.X < 0f)
                    {
                        fallFaceDirection = -1;
                    }
                    if (NPC.velocity.X > 0f)
                    {
                        fallFaceDirection = 1;
                    }
                    Vector2 trashPosition = NPC.position;
                    trashPosition.X += NPC.velocity.X;
                    int xTileBelow = (int)((trashPosition.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * fallFaceDirection)) / 16f);
                    int yTileBelow = (int)((trashPosition.Y + (float)NPC.height - 1f) / 16f);
                    if ((float)(xTileBelow * 16) < trashPosition.X + (float)NPC.width && (float)(xTileBelow * 16 + 16) > trashPosition.X && ((Main.tile[xTileBelow, yTileBelow].HasUnactuatedTile && !Main.tile[xTileBelow, yTileBelow].TopSlope && !Main.tile[xTileBelow, yTileBelow - 1].TopSlope && Main.tileSolid[(int)Main.tile[xTileBelow, yTileBelow].TileType] && !Main.tileSolidTop[(int)Main.tile[xTileBelow, yTileBelow].TileType]) || (Main.tile[xTileBelow, yTileBelow - 1].IsHalfBlock && Main.tile[xTileBelow, yTileBelow - 1].HasUnactuatedTile)) && (!Main.tile[xTileBelow, yTileBelow - 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[xTileBelow, yTileBelow - 1].TileType] || Main.tileSolidTop[(int)Main.tile[xTileBelow, yTileBelow - 1].TileType] || (Main.tile[xTileBelow, yTileBelow - 1].IsHalfBlock && (!Main.tile[xTileBelow, yTileBelow - 4].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[xTileBelow, yTileBelow - 4].TileType] || Main.tileSolidTop[(int)Main.tile[xTileBelow, yTileBelow - 4].TileType]))) && (!Main.tile[xTileBelow, yTileBelow - 2].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[xTileBelow, yTileBelow - 2].TileType] || Main.tileSolidTop[(int)Main.tile[xTileBelow, yTileBelow - 2].TileType]) && (!Main.tile[xTileBelow, yTileBelow - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[xTileBelow, yTileBelow - 3].TileType] || Main.tileSolidTop[(int)Main.tile[xTileBelow, yTileBelow - 3].TileType]) && (!Main.tile[xTileBelow - fallFaceDirection, yTileBelow - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[xTileBelow - fallFaceDirection, yTileBelow - 3].TileType]))
                    {
                        float yPixelDistance = (float)(yTileBelow * 16);
                        if (Main.tile[xTileBelow, yTileBelow].IsHalfBlock)
                        {
                            yPixelDistance += 8f;
                        }
                        if (Main.tile[xTileBelow, yTileBelow - 1].IsHalfBlock)
                        {
                            yPixelDistance -= 8f;
                        }
                        if (yPixelDistance < trashPosition.Y + (float)NPC.height)
                        {
                            float percentageTileRisen = trashPosition.Y + (float)NPC.height - yPixelDistance;
                            float fullTileAmt = 16.1f;
                            if (percentageTileRisen <= fullTileAmt)
                            {
                                NPC.gfxOffY += NPC.position.Y + (float)NPC.height - yPixelDistance;
                                NPC.position.Y = yPixelDistance - (float)NPC.height;
                                if (percentageTileRisen < 9f)
                                {
                                    NPC.stepSpeed = 1f;
                                }
                                else
                                {
                                    NPC.stepSpeed = 2f;
                                }
                            }
                        }
                    }
                    if (NPC.oldPosition == NPC.position)
                    {
                        NPC.velocity.Y = -10f;
                        NPC.netUpdate = true;
                    }
                }
                NPC.velocity.Y = NPC.velocity.Y + (Main.zenithWorld ? 0.5f : 0.55f); // Lets them jump up walls infinitely in GFB
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
            }
            NPC.rotation = NPC.velocity.Y * (float)NPC.direction * 0.05f;
            NPC.rotation = MathHelper.Clamp(NPC.rotation, -0.1f, 0.1f);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += hasBeenHit ? 1.25 : 1.0;
            if (NPC.frameCounter < 6.0)
            {
                NPC.frame.Y = 0;
            }
            else if (NPC.frameCounter < 12.0)
            {
                NPC.frame.Y = frameHeight;
            }
            else if (NPC.frameCounter < 18.0)
            {
                NPC.frame.Y = frameHeight * 2;
            }
            else if (NPC.frameCounter < 24.0)
            {
                NPC.frame.Y = frameHeight * 3;
            }
            else
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y = 0;
            }
            if (!NPC.wet)
            {
                NPC.frame.Y = NPC.frame.Y + frameHeight * 4;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms)
            {
                return 0f;
            }
            if (spawner.Player.Calamity().ZoneSulphur || (spawner.Player.Calamity().ZoneSulphur && spawner.waterTile))
            {
                return 0.05f;
            }
            return 0f;
        }

        public override void OnKill()
        {
            if (!NPC.savedAngler && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!NPC.AnyNPCs(NPCID.Angler) && !NPC.AnyNPCs(NPCID.SleepingAngler))
                {
                    NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.Angler);
                    NPC.savedAngler = true;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.DivingHelmet, 20);
            npcLoot.Add(ModContent.ItemType<TrashmanTrashcan>(), 20);
            npcLoot.AddIf(() => Main.hardMode, ItemID.Gatligator, 10);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Trasher").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Trasher2").Type, 1f);
                }
            }
        }
    }
}
