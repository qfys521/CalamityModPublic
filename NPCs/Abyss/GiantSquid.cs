using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.Abyss
{
    public class GiantSquid : ModNPC
    {
        private bool hasBeenHit = false;
        public static Asset<Texture2D> GlowTexture;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 100;
            NPC.width = 62;
            NPC.height = 226;
            NPC.defense = 18;
            NPC.lifeMax = 1000;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 10);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<GiantSquidBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<AbyssLayer3Biome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.GiantSquid")
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
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.direction == 0)
            {
                NPC.TargetClosest(true);
            }
            if (!NPC.wet)
            {
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.98f;
                    if (NPC.velocity.X > -0.01 && NPC.velocity.X < 0.01)
                    {
                        NPC.velocity.X = 0f;
                    }
                }
                NPC.velocity.Y = NPC.velocity.Y + 0.2f;
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
                NPC.ai[0] = 1f;
                return;
            }
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.velocity.X * -1f;
                NPC.direction *= -1;
            }
            if (NPC.collideY)
            {
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
            NPC.TargetClosest(false);
            if ((Main.player[NPC.target].wet && !Main.player[NPC.target].dead &&
                Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) &&
                (Main.player[NPC.target].Center - NPC.Center).Length() < Main.player[NPC.target].Calamity().GetAbyssAggro(160f)) ||
                NPC.justHit)
            {
                hasBeenHit = true;
            }
            NPC.chaseable = hasBeenHit;
            NPC.rotation = NPC.velocity.X * 0.02f;
            if (hasBeenHit)
            {
                NPC.localAI[2] = 1f;
                NPC.velocity *= 0.975f;

                float lungeSpeed = CalamityWorld.death ? 24f : CalamityWorld.revenge ? 20f : 16f;
                if (NPC.velocity.Length() > lungeSpeed * 0.4f)
                    NPC.damage = NPC.defDamage;

                float lungeThreshold = 1.6f;
                if (NPC.velocity.X > -lungeThreshold && NPC.velocity.X < lungeThreshold && NPC.velocity.Y > -lungeThreshold && NPC.velocity.Y < lungeThreshold)
                {
                    NPC.TargetClosest(true);
                    Vector2 lungeNPCPos = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float lungeTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - lungeNPCPos.X;
                    float lungeTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - lungeNPCPos.Y;
                    float lungeTargetDist = (float)Math.Sqrt((double)(lungeTargetX * lungeTargetX + lungeTargetY * lungeTargetY));
                    lungeTargetDist = lungeSpeed / lungeTargetDist;
                    lungeTargetX *= lungeTargetDist;
                    lungeTargetY *= lungeTargetDist;
                    NPC.velocity.X = lungeTargetX;
                    NPC.velocity.Y = lungeTargetY;
                    return;
                }
            }
            else
            {
                NPC.localAI[2] = 0f;
                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * 0.02f;
                if (NPC.velocity.X < -1f || NPC.velocity.X > 1f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.95f;
                }
                if (NPC.ai[0] == -1f)
                {
                    NPC.velocity.Y = NPC.velocity.Y - 0.01f;
                    if (NPC.velocity.Y < -1f)
                    {
                        NPC.ai[0] = 1f;
                    }
                }
                else
                {
                    NPC.velocity.Y = NPC.velocity.Y + 0.01f;
                    if (NPC.velocity.Y > 1f)
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
                else
                {
                    NPC.ai[0] = 1f;
                }
                if ((double)NPC.velocity.Y > 1.2 || (double)NPC.velocity.Y < -1.2)
                {
                    NPC.velocity.Y = NPC.velocity.Y * 0.99f;
                }
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
            NPC.frameCounter += hasBeenHit ? 0.15f : 0.075f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
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

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.Player.Calamity().ZoneAbyssLayer3 && spawner.waterTile)
            {
                return Main.remixWorld ? 10.8f : SpawnCondition.CaveJellyfish.Chance * 1.2f;
            }
            return 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(ModContent.BuffType<CrushDepth>(), 180);
                target.AddBuff(BuffID.Darkness, 300);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var postLevi = npcLoot.DefineConditionalDropSet(DropHelper.PostLevi());
            npcLoot.Add(ItemDropRule.NormalvsExpert(ItemID.Blindfold, 100, 50));
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<Items.Accessories.InkBomb>(), 10, 5));
            postLevi.Add(DropHelper.NormalVsExpertQuantity(ModContent.ItemType<DepthCells>(), 2, 2, 4, 3, 6));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 30; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GiantSquid").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GiantSquid2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GiantSquid3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GiantSquid4").Type, 1f);
                }
            }
        }
    }
}
