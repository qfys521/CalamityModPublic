using System;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.NormalNPCs
{
    public class CladCrab : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.chaseable = false;
            NPC.damage = 12;
            NPC.defense = 7;
            NPC.width = 56;
            NPC.height = 54;
            NPC.lifeMax = 100;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0.4f;
            NPC.value = Item.buyPrice(silver: 2);
            NPC.HitSound = SoundID.NPCHit41 with { Pitch = 0.6f };
            NPC.DeathSound = SoundID.NPCDeath36 with { Pitch = -0.4f };
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<CladCrabBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            float movementSpeed = 1;

            // Enrage if target is close enough
            if (NPC.HasPlayerTarget)
            {
                Player target = Main.player[NPC.target];
                // Enrage if the player is close enough, not too far above, and in its line of sight
                if (target.Distance(NPC.Center) < 320 && target.Bottom.Y > NPC.Top.Y - 40 && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1))
                {
                    // Enter attack mode
                    if (NPC.ai[0] != 2)
                    {
                        NPC.ai[1] = 60;
                        NPC.ai[0] = 2;
                    }
                    // Set calm down timer to 0 as long as these conditions are met
                    NPC.ai[2] = 0;
                }
                else
                {
                    // Calm down
                    NPC.ai[2]++;
                    // After 5 seconds of no valid target, go back to passive behaviour
                    if (NPC.ai[2] > 300)
                    {
                        NPC.ai[0] = Main.rand.Next(0, 2);
                        NPC.ai[1] = Main.rand.Next(120, 301);
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                    }
                }
            }

            if (NPC.direction == 0)
            {
                NPC.direction = Main.rand.NextBool() ? -1 : 1;
            }

            // If not enraged and the phase timer is at 0, change phase
            if (NPC.ai[1] <= 0 && NPC.ai[0] != 2)
            {
                NPC.ai[0] = NPC.ai[0] == 0 ? 1 : 0;
                NPC.ai[1] = Main.rand.Next(120, 301); // Set the phase timer
                NPC.direction = Main.rand.NextBool() ? -1 : 1; // Flip direction randomly
            }

            // Passive walking
            if (NPC.ai[0] == 0)
            {
                // If it bumps into something, turn around
                if (NPC.ai[3] <= 0 && NPC.velocity.X == 0)
                {
                    NPC.direction *= -1;
                    NPC.ai[3] = 30;
                }
                // Move
                if (!NPC.justHit)
                    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * movementSpeed, 0.05f);
                // Don't get stuck on 1 block obstacles 
                NPC.StepUpBlocks();
            }
            // Stand still
            else if (NPC.ai[0] == 1)
            {
                // Slow to a stop if not falling/being moved up
                if (NPC.velocity.Y == 0)
                    NPC.velocity.X *= 0;
                // Start running if hit
                // This behavior is mostly only here for if it gets hurt but you aren't within its aggro range
                if (NPC.justHit)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = Main.rand.Next(120, 301);
                }
            }
            // Enrage
            else
            {
                Player target = Main.player[NPC.target];
                // If it bumps into something, turn around
                if (NPC.ai[3] <= 0 && NPC.velocity.X == 0)
                {
                    NPC.direction *= -1;
                    NPC.ai[3] = 30;
                }
                // If it's too far from the player, turn around
                else if ((NPC.ai[3] <= 0 && target.Distance(NPC.Center) > 160) || NPC.ai[3] < -180)
                {
                    int dir = Math.Sign(Main.player[NPC.target].Center.X - NPC.Center.X);
                    NPC.direction = dir;
                    NPC.ai[3] = 120;
                }
                // Move
                if (!NPC.justHit)
                    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * movementSpeed, 0.05f);
                NPC.StepUpBlocks();
            }

            // Phase timer 
            if (NPC.ai[1] > 0)
            {
                NPC.ai[1]--;
            }
            // Change direction cooldown
            NPC.ai[3]--;

            NPC.spriteDirection = -NPC.direction;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.CladCrab")
            });
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.Player.Calamity().ZoneSulphur || spawner.Player.Calamity().ZoneSunkenSea || !spawner.Player.InZonePurity())
            {
                return 0f;
            }
            return (Main.remixWorld ? SpawnCondition.Cavern.Chance : SpawnCondition.OverworldDaySlime.Chance) * 0.1f;
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

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.StoneBlock, 1, 10, 20);
            npcLoot.Add(ItemID.Daybloom, 1, 1, 4);
            npcLoot.Add(ItemID.Blinkroot, 1, 1, 3);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, hit.HitDirection, -1f, 0, default, 1f);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 7; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, hit.HitDirection, -1f, 0, default, 1f);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("CladCrab").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("CladCrab2").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("CladCrab3").Type);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("CladCrab4").Type);
                }
            }
        }
    }
}
