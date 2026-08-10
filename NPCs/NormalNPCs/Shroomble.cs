using System;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.NormalNPCs
{
    public class Shroomble : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.chaseable = false;
            NPC.damage = 0;
            NPC.width = 28;
            NPC.height = 24;
            NPC.lifeMax = 25; // This is how much Mushrooms heal with Calamity
            NPC.aiStyle = NPCAIStyleID.Passive;
            AIType = NPCID.Squirrel;
            NPC.knockBackResist = 0.5f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            DrawOffsetY = -2;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ShroombleBanner>();
            NPC.catchItem = (short)ModContent.ItemType<ShroombleItem>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void AI()
        {
            bool walking = NPC.ai[0] == 1;
            // Do a cute lil' hop randomly
            int jumpChance = walking ? 600 : 100;
            if (NPC.velocity.Y == 0 && Main.rand.NextBool(jumpChance) && NPC.ai[2] <= 0)
            {
                SoundEngine.PlaySound(HarvestStaffMinion.JumpSound, NPC.Center);
                NPC.velocity.Y = Main.rand.NextFloat(-6, -4);
                // Cooldown
                NPC.ai[2] = walking ? 180 : 90;
                // Ai[1] is time left for a NPCAIStyleID.Passive critter to cycle between standing still and walking
                // Increment it here so that it doesn't suddenly start walking when doing it's baby hop
                NPC.ai[1] += 60;
            }

            // Randomly look around while standing still
            if (!walking && Main.rand.NextBool(200) && NPC.velocity.Y == 0)
            {
                NPC.direction *= -1;
            }

            // Decrement cooldown
            if (NPC.ai[2] > 0)
            {
                NPC.ai[2]--;
            }
            // Don't idle for too long
            if (NPC.ai[1] > 300)
            {
                NPC.ai[1] = 300;
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Shroomble")
            });
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (!spawner.Player.ZonePurity)
            {
                return 0f;
            }
            return (Main.remixWorld ? SpawnCondition.Cavern.Chance : SpawnCondition.OverworldDay.Chance) * 0.1f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity.Y == 0f)
            {
                if (!NPC.IsABestiaryIconDummy)
                {
                    if (NPC.direction == 1)
                    {
                        NPC.spriteDirection = -1;
                    }
                    if (NPC.direction == -1)
                    {
                        NPC.spriteDirection = 1;
                    }

                    if (NPC.velocity.X == 0f)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0.0;
                        return;
                    }
                }
                NPC.frameCounter += NPC.IsABestiaryIconDummy ? 0.6f : Math.Abs(NPC.velocity.X) * 0.5f;
                NPC.frameCounter += 1.0;
                if (NPC.frameCounter > 12.0)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y / frameHeight >= Main.npcFrameCount[Type] - 1)
                {
                    NPC.frame.Y = frameHeight;
                }
            }
            else
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y = frameHeight * (Main.npcFrameCount[Type] - 1);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ItemID.Mushroom);

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Pixie, hit.HitDirection * 0.5f, -0.5f, 0, default, 0.5f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Pixie, hit.HitDirection * 0.5f, -0.5f, 0, default, 0.5f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("Shroomble").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("Shroomble2").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("Shroomble3").Type);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            // Psychedelic in gfb
            if (Main.zenithWorld)
            {
                Color lightColor = new Color(Main.DiscoR, Main.DiscoB, Main.DiscoG, drawColor.A);
                return lightColor * NPC.Opacity;
            }
            else return null;
        }
    }
}
