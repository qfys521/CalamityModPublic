using System.IO;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Placeables.Banners;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.NormalNPCs
{
    public class CrawlerTopaz : ModNPC
    {
        private bool detected = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0.3f;
            NPC.aiStyle = -1;
            NPC.damage = 12;
            NPC.width = 44;
            NPC.height = 34;
            NPC.defense = 4;
            NPC.lifeMax = 90;
            NPC.knockBackResist = 0.85f;
            AIType = -1;
            NPC.value = Item.buyPrice(copper: 60);
            NPC.HitSound = SoundID.NPCHit33;
            NPC.DeathSound = SoundID.NPCDeath36;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<TopazCrawlerBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.GemCrawler")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(detected);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            detected = reader.ReadBoolean();
        }

        public override void FindFrame(int frameHeight)
        {
            if (!detected)
            {
                NPC.frame.Y = frameHeight * 4;
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.spriteDirection = -NPC.direction;
            NPC.frameCounter += (double)(NPC.velocity.Length() / 8f);
            if (NPC.frameCounter > 2.0)
            {
                NPC.frame.Y = NPC.frame.Y + frameHeight;
                NPC.frameCounter = 0.0;
            }
            if (NPC.frame.Y >= frameHeight * 3)
            {
                NPC.frame.Y = 0;
            }
        }

        public override void AI()
        {
            if (!detected)
                NPC.TargetClosest();
            if (((Main.player[NPC.target].Center - NPC.Center).Length() < 100f && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position,
                    Main.player[NPC.target].width, Main.player[NPC.target].height)) || NPC.justHit)
                detected = true;
            if (!detected)
                return;
            CalamityRegularEnemyAI.GemCrawlerAI(NPC, Mod, 4.5f, 0.045f);
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || spawner.Player.Calamity().InAnyCalamityBiome)
            {
                return 0f;
            }
            return SpawnCondition.Underground.Chance * 0.04f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Honey, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Honey, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("CrawlerTopaz").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("CrawlerTopaz2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Crawler").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Crawler2").Type, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.Topaz, 1, 2, 4);
            npcLoot.Add(ModContent.ItemType<ScuttlersJewel>(), 6);
            npcLoot.Add(ModContent.ItemType<XyksBlessingBlue>(), 25);
        }
    }
}
