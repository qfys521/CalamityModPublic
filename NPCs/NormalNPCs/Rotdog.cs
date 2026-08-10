using System;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class Rotdog : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 10;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Unicorn;
            NPC.damage = 18;
            NPC.width = 46;
            NPC.height = 30;
            NPC.defense = 4;
            NPC.lifeMax = 75;
            NPC.knockBackResist = 0.3f;
            AIType = NPCID.Wolf;
            NPC.value = Item.buyPrice(silver: 1, copper: 50);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath5;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<RotdogBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Rotdog")
            });
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 5)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                }
                if (NPC.frame.Y > frameHeight * 7)
                {
                    NPC.frame.Y = frameHeight;
                }
            }
            else
            {
                NPC.spriteDirection = NPC.direction;
                if (NPC.velocity.Y < 0f) // jump
                {
                    NPC.frame.Y = frameHeight * 8;
                    NPC.frameCounter = 0.0;
                }
                else if (NPC.velocity.Y > 0f) // fall
                {
                    NPC.frame.Y = frameHeight * 9;
                    NPC.frameCounter = 0.0;
                }
                else
                {
                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.4f;
                    if (NPC.frameCounter > 4)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y < frameHeight)
                        {
                            NPC.frame.Y = frameHeight;
                        }
                        if (NPC.frame.Y > frameHeight * 7)
                        {
                            NPC.frame.Y = frameHeight;
                        }
                    }
                }
            }
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || !NPC.downedBoss1 || spawner.Player.Calamity().ZoneSulphur)
            {
                return 0f;
            }
            return SpawnCondition.OverworldNightMonster.Chance * 0.045f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(BuffID.Bleeding, 180);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Rotdog1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Rotdog2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Rotdog3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Rotdog4").Type, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ItemID.AdhesiveBandage, 100, 50));
            npcLoot.Add(ModContent.ItemType<RottenDogtooth>(), 8);
        }
    }
}
