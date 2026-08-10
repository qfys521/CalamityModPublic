using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PrismBack : ModNPC
    {
        public static Asset<Texture2D> GlowTexture;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitPositionXOverride = 0
            };
            value.Position.X += 15;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = Main.hardMode ? 40 : 20;
            NPC.width = 72;
            NPC.height = 58;
            NPC.defense = Main.hardMode ? 25 : 15;
            NPC.lifeMax = Main.hardMode ? 1000 : 500;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(0, 0, 2, 0);
            NPC.HitSound = SoundID.NPCHit24;
            NPC.DeathSound = SoundID.NPCDeath27;
            NPC.knockBackResist = 0.15f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<PrismBackBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PrismBack")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void AI()
        {
            if ((NPC.Center.Y + 10f) > Main.player[NPC.target].Center.Y)
            {
                if (CalamityWorld.death)
                {
                    NPC.damage = NPC.defDamage * 3;
                }
                else if (CalamityWorld.revenge)
                {
                    NPC.damage = (int)Math.Round(NPC.defDamage * 2.75);
                }
                else if (Main.expertMode)
                {
                    NPC.damage = (int)Math.Round(NPC.defDamage * 2.5);
                }
                else
                {
                    NPC.damage = (int)Math.Round(NPC.defDamage * 1.25);
                }
            }
            else
            {
                if (CalamityWorld.death)
                {
                    NPC.damage = (int)Math.Round(NPC.defDamage * 2.5);
                }
                else if (CalamityWorld.revenge)
                {
                    NPC.damage = (int)Math.Round(NPC.defDamage * 2.25);
                }
                else if (Main.expertMode)
                {
                    NPC.damage = NPC.defDamage * 2;
                }
                else
                {
                    NPC.damage = NPC.defDamage;
                }
            }
            Lighting.AddLight(NPC.Center, (255 - NPC.alpha) * 0f / 255f, (255 - NPC.alpha) * 0.75f / 255f, (255 - NPC.alpha) * 0.75f / 255f);
            CalamityRegularEnemyAI.PassiveSwimmingAI(NPC, Mod, 2, 0f, 0f, 0f, 0f, 0f, 0.1f);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += (NPC.wet || NPC.IsABestiaryIconDummy) ? 0.1f : 0f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Vector2 center = new Vector2(NPC.Center.X, NPC.Center.Y);
            Vector2 halfSizeTexture = new Vector2((float)(TextureAssets.Npc[NPC.type].Value.Width / 2), (float)(TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2));
            Vector2 vector = center - screenPos;
            vector -= new Vector2((float)GlowTexture.Value.Width, (float)(GlowTexture.Value.Height / Main.npcFrameCount[NPC.type])) * 1f / 2f;
            vector += halfSizeTexture * 1f + new Vector2(0f, 4f + NPC.gfxOffY);
            Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(Color.Blue);
            Main.spriteBatch.Draw(GlowTexture.Value, vector,
                new Microsoft.Xna.Framework.Rectangle?(NPC.frame), color, NPC.rotation, halfSizeTexture, 1f, spriteEffects, 0f);
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return NPC.chaseable;
            }
            return null;
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.Player.Calamity().ZoneSunkenSea && spawner.waterTile && !spawner.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.9f;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule postDS = npcLoot.DefineConditionalDropSet(DropHelper.PostDS());
            postDS.Add(ModContent.ItemType<PrismShard>(), 1, 1, 3);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismBackGore1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismBackGore2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismBackGore3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismBackGore4").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismBackGore5").Type, 1f);
                }
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }
    }
}
