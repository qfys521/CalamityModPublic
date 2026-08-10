using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.Ores;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class BloomSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Slime;
            AIType = NPCID.ToxicSludge;
            NPC.damage = 80;
            NPC.width = 40;
            NPC.height = 30;
            NPC.defense = 40;
            NPC.lifeMax = 2000;
            NPC.knockBackResist = 0.3f;
            AnimationType = NPCID.CorruptSlime;
            NPC.value = Item.buyPrice(silver: 25);
            NPC.alpha = 50;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<BloomSlimeBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.BloomSlime")
            });
        }

        public override void AI()
        {
            NPC.damage = (NPC.velocity.Y == 0f || NPC.velocity.Length() < 3f) ? 0 : NPC.defDamage;
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || !DownedBossSystem.downedProvidence || spawner.Player.Calamity().ZoneAbyss ||
                spawner.Player.Calamity().ZoneSunkenSea)
            {
                return 0f;
            }
            return SpawnCondition.Cavern.Chance * 0.08f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.SulphurousSeaAcid, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.SulphurousSeaAcid, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ModContent.ItemType<UelibloomOre>(), 1, 10, 26);
    }
}
