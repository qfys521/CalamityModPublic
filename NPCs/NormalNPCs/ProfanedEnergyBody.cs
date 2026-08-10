using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.NormalNPCs
{
    public class ProfanedEnergyBody : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NeedsExpertScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                //Preferably would have two animated lanterns, but this static one-headded wiki image will do for now
                PortraitPositionYOverride = 0,
                CustomTexturePath = "CalamityMod/ExtraTextures/Bestiary/ProfanedEnergy_Bestiary"
            };
            value.Position.Y += 30;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.npcSlots = 3f;
            NPC.width = 72;
            NPC.height = 36;
            NPC.defense = 50;
            NPC.lifeMax = 2500;
            NPC.knockBackResist = 0f;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 50);
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit52;
            NPC.DeathSound = SoundID.NPCDeath55;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ProfanedEnergyBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.ProfanedEnergyBody")
            });
        }

        public override void AI()
        {
            CalamityGlobalNPC.energyFlame = NPC.whoAmI;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.localAI[0] == 0f)
                {
                    NPC.localAI[0] = 1f;
                    for (int num723 = 0; num723 < 2; num723++)
                    {
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<ProfanedEnergyLantern>(), NPC.whoAmI, 0f, 0f, 0f, 0f, 255);
                    }
                }
            }
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || !NPC.downedMoonlord || spawner.Player.Calamity().ZoneCalamity || (!spawner.Player.ZoneUnderworldHeight && !spawner.Player.ZoneHallow) || Main.pumpkinMoon || Main.snowMoon || Main.eclipse)
                return 0f;

            // Keep this as a separate if check, because it's a loop and we don't want to be checking it constantly.
            if (NPC.AnyNPCs(NPC.type))
                return 0f;

            if (SpawnCondition.Underworld.Chance > 0f)
                return SpawnCondition.Underworld.Chance / 6f;

            return SpawnCondition.OverworldHallow.Chance / 6f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.ProfanedFire, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.ProfanedFire, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ProfanedEnergyBody").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ProfanedEnergyBody2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ProfanedEnergyBody3").Type, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ModContent.ItemType<UnholyEssence>(), 1, 2, 4);
    }
}
