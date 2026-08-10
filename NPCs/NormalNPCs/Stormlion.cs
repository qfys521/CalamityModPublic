using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Summon;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.NormalNPCs
{
    public class Stormlion : ModNPC
    {
        public static readonly SoundStyle IdleSound = new("CalamityMod/Sounds/Custom/StormlionIdle");
        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/NPCHit/StormlionHit");
        public static readonly SoundStyle DeathSound = new("CalamityMod/Sounds/NPCKilled/StormlionDeath");
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.damage = 20;
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.width = 33;
            NPC.height = 31;
            NPC.defense = 8;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0.2f;
            AnimationType = NPCID.WalkingAntlion;
            NPC.value = Item.buyPrice(silver: 2);
            NPC.HitSound = HitSound;
            NPC.DeathSound = DeathSound;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<StormlionBanner>();
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = false;
            NPC.Calamity().VulnerableToWater = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundDesert,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Stormlion")
            });
        }

        public override void AI()
        {
            if (Main.rand.NextBool(800))
            {
                SoundEngine.PlaySound(IdleSound, NPC.Center);
            }
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
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Stormlion").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Stormlion2").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Stormlion3").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Stormlion4").Type, NPC.scale);
                }
            }
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms ||
                spawner.Player.Calamity().ZoneSunkenSea ||
                spawner.Player.PillarZone() ||
                spawner.Player.InAstral() ||
                spawner.Player.ZoneCorrupt ||
                spawner.Player.ZoneCrimson ||
                spawner.Player.ZoneOldOneArmy ||
                Main.eclipse ||
                Main.snowMoon ||
                Main.pumpkinMoon ||
                Main.invasionType != InvasionID.None)
            {
                return 0f;
            }
            if (Main.IsItStorming && spawner.Player.ZoneDesert)
            {
                return SpawnCondition.OverworldDayDesert.Chance;
            }
            return SpawnCondition.DesertCave.Chance * 0.3f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<StaticDischarge>(), 120);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<StormlionMandible>());
            npcLoot.Add(ModContent.ItemType<StormjawStaff>(), 5);
            npcLoot.Add(ItemID.ThunderSpear, 25);
            npcLoot.Add(ItemID.ThunderStaff, 25);
        }
    }
}
