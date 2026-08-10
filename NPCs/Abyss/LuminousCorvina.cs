using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.Abyss
{
    public class LuminousCorvina : ModNPC
    {
        public static Asset<Texture2D> GlowTexture;

        public static readonly SoundStyle ScreamSound = new("CalamityMod/Sounds/Custom/CorvinaScream");

        private bool hasBeenHit = false;
        private int screamTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 10;
            NPC.width = 68;
            NPC.height = 58;
            NPC.defense = 18;
            NPC.lifeMax = 800;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 10);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.85f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<LuminousCorvinaBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<AbyssLayer2Biome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.LuminousCorvina")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(hasBeenHit);
            writer.Write(screamTimer);
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            hasBeenHit = reader.ReadBoolean();
            screamTimer = reader.ReadInt32();
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void AI()
        {

            EnhancedDarknessSystem.lights.Add(new(NPC.Center,scale: 1f));

            NPC.spriteDirection = (NPC.direction > 0) ? 1 : -1;
            NPC.noGravity = true;
            if (NPC.direction == 0)
            {
                NPC.TargetClosest(true);
            }
            if (NPC.justHit)
            {
                hasBeenHit = true;
            }
            NPC.chaseable = hasBeenHit;
            if (NPC.wet)
            {
                bool canAttack = hasBeenHit;
                NPC.TargetClosest(false);
                if ((Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position,
                    Main.player[NPC.target].width, Main.player[NPC.target].height) && ((NPC.Center.X - 15f < Main.player[NPC.target].Center.X &&
                    NPC.direction == 1) || (NPC.Center.X + 15f > Main.player[NPC.target].Center.X && NPC.direction == -1))) ||
                    (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position,
                    Main.player[NPC.target].width, Main.player[NPC.target].height) && canAttack))
                {
                    ++screamTimer;

                    int screamLimit = CalamityWorld.death ? 60 : CalamityWorld.revenge ? 120 : 180;
                    if (screamTimer >= screamLimit)
                    {
                        if (screamTimer == screamLimit)
                        {
                            SoundEngine.PlaySound(ScreamSound, NPC.Center);
                            if (!Main.dedServ)
                            {
                                if (!Main.player[NPC.target].dead && Main.player[NPC.target].active)
                                {
                                    Main.player[NPC.target].AddBuff(ModContent.BuffType<FishAlert>(), 360);
                                }
                            }
                        }
                        if (screamTimer >= screamLimit + 60)
                        {
                            screamTimer = 0;
                        }
                        return;
                    }
                }
                if ((!Main.player[NPC.target].wet || Main.player[NPC.target].dead) && canAttack)
                {
                }
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
                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * 0.1f;
                if (NPC.velocity.X < -0.2f || NPC.velocity.X > 0.2f)
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
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.94f;
                    if ((double)NPC.velocity.X > -0.2 && (double)NPC.velocity.X < 0.2)
                    {
                        NPC.velocity.X = 0f;
                    }
                }
                NPC.velocity.Y = NPC.velocity.Y + 0.3f;
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
                NPC.ai[0] = 1f;
            }
            NPC.rotation = NPC.velocity.Y * (float)NPC.direction * 0.1f;
            if ((double)NPC.rotation < -0.2)
            {
                NPC.rotation = -0.2f;
            }
            if ((double)NPC.rotation > 0.2)
            {
                NPC.rotation = 0.2f;
            }
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return hasBeenHit;
            }
            return null;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter += 1;
                if (NPC.frameCounter > 6.0)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 5)
                {
                    NPC.frame.Y = 0;
                }
            }
            else
            {
                NPC.frameCounter += 1.0;
                if (NPC.frameCounter > 6.0)
                {
                    NPC.frameCounter = 0.0;
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                }
                if (screamTimer <= (CalamityWorld.death ? 60 : CalamityWorld.revenge ? 120 : 180))
                {
                    if (NPC.frame.Y > frameHeight * 5)
                    {
                        NPC.frame.Y = 0;
                    }
                }
                else
                {
                    if (NPC.frame.Y < frameHeight * 6)
                    {
                        NPC.frame.Y = frameHeight * 6;
                    }
                    if (NPC.frame.Y > frameHeight * 7)
                    {
                        NPC.frame.Y = frameHeight * 6;
                    }
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(GlowTexture.Value, NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY + 4),
                NPC.frame, Color.White * 0.5f, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, effects, 0);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 180);
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.Player.Calamity().ZoneAbyssLayer2 && spawner.waterTile)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<Voidstone>(), 1, 8, 15);
            var postLevi = npcLoot.DefineConditionalDropSet(DropHelper.PostLevi());
            postLevi.Add(DropHelper.NormalVsExpertQuantity(ModContent.ItemType<DepthCells>(), 2, 1, 2, 2, 3));
            postLevi.Add(ModContent.ItemType<Lumenyl>(), 2);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Confetti, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Confetti, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LuminousCorvina").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LuminousCorvina2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LuminousCorvina3").Type, 1f);
                }
            }
        }
    }
}
