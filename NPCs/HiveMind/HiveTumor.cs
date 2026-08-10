using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.HiveMind
{
    public class HiveTumor : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0f;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 30; //324
            NPC.height = 30; //216
            NPC.defense = 0;
            NPC.lifeMax = Main.zenithWorld ? 10 : 1000;
            NPC.knockBackResist = 0f;
            NPC.chaseable = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.rarity = 2;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().ProvidesProximityRage = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundCorruption,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.HiveTumor")
            });
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override void AI()
        {
            float timeToSpawn = 120f;

            if (Main.zenithWorld && NPC.AnyNPCs(ModContent.NPCType<HiveMind>()))
            {
                // Passively spawns random enemies
                NPC.ai[0]++;

                if (NPC.ai[0] >= timeToSpawn)
                {
                    int spawnRandomizer = Main.rand.Next(0, 5);
                    int type = NPCID.EaterofSouls;

                    switch (spawnRandomizer)
                    {
                        case 0:
                        case 1:
                            break;
                        case 2:
                            type = NPCID.DevourerHead;
                            break;
                        case 3:
                            type = ModContent.NPCType<DankCreeper>();
                            break;
                        case 4:
                            type = ModContent.NPCType<HiveBlob>();
                            break;
                        default:
                            break;
                    }
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, type);
                    NPC.ai[0] = 0f;
                }
            }
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (CalamityGlobalNPC.AnyEvents(spawner.Player) || !spawner.Player.ZoneCorrupt)
                return 0f;

            if (spawner.Player.Calamity().disableHiveCystSpawns)
                return 0f;

            bool corrupt = TileID.Sets.Corrupt[spawner.SpawnTileType] || spawner.SpawnTileType == TileID.Demonite;
            if (spawner.noWorms || !corrupt)
                return 0f;

            // Keep this as a separate if check, because it's a loop and we don't want to be checking it constantly.
            if (NPC.AnyNPCs(NPC.type))
                return 0f;

            // Keep this as a separate if check, because it's a loop and we don't want to be checking it constantly.
            if (NPC.AnyNPCs(ModContent.NPCType<HiveMind>()))
                return 0f;

            if (NPC.downedBoss2 && !DownedBossSystem.downedHiveMind)
                return 1.5f;

            return Main.hardMode ? 0.05f : 0.5f;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = 2000;

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Demonite, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Demonite, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (Main.netMode != NetmodeID.MultiplayerClient && (!NPC.AnyNPCs(ModContent.NPCType<HiveMind>()) || Main.zenithWorld))
                {
                    Vector2 spawnAt = NPC.Bottom;
                    NPC.NewNPC(NPC.GetSource_Death(), (int)spawnAt.X, (int)spawnAt.Y, ModContent.NPCType<HiveMind>());
                }
            }
        }
    }
}
