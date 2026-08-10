using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.NormalNPCs
{
    // This is intended to be a new variant for the vanilla Zombie
    public class BucketZombie : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
            NPCID.Sets.Zombies[Type] = true;
        }

        public override void SetDefaults()
        {
            AIType = NPCID.Zombie;
            AnimationType = NPCID.Zombie;
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.lifeMax = 60;
            NPC.defense = 15;
            NPC.damage = 16;
            NPC.knockBackResist = 0.45f;
            NPC.npcSlots = 1.15f; // Equal to the strongest variants
            NPC.value = Item.buyPrice(silver: 1);
            Banner = BannerSystem.NPCtoBanner(NPCID.Zombie);
            BannerItem = ItemID.ZombieBanner;
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
                new FlavorTextBestiaryInfoElement("CommonBestiaryFlavor.Zombie")
            });
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || spawner.Player.Calamity().ZoneSulphur)
            {
                return 0f;
            }
            return SpawnCondition.OverworldNightMonster.Chance * 0.05f;
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
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BucketZombie").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BucketZombie2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BucketZombie3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BucketZombie4").Type, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Copy over all loot from real Zombies
            List<IItemDropRule> zombieRules = Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false);
            foreach (var v in zombieRules)
            {
                npcLoot.Add(v);
            }
            npcLoot.Add(ItemID.EmptyBucket, 10);
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Takes greatly reduced damage from sentries because thats how it is in Plants versus Zombies
            // Pumpoem will be none the wiser
            if (projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])
            {
                modifiers.SourceDamage *= 0.05f;
                SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = -1, Volume = 0.8f }, NPC.Center);

                // Just straight up reflect them in gfb
                if (Main.zenithWorld)
                {
                    projectile.velocity *= -1;
                    modifiers.SetMaxDamage(1);
                }
            }
        }
    }
}
