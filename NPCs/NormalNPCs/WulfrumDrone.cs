using System.IO;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Sounds;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.NormalNPCs
{
    internal enum DroneAIState
    {
        Searching = 0,
        Charging = 1
    }

    public class WulfrumDrone : ModNPC
    {
        internal DroneAIState AIState
        {
            get => (DroneAIState)(int)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }

        public float HorizontalChargeTime
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public float Time
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        public float SuperchargeTimer
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        public bool Supercharged => SuperchargeTimer > 0;
        public ref float FlyAwayTimer => ref NPC.localAI[0];
        public const float TotalHorizontalChargeTime = 75f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            AIType = -1;
            NPC.aiStyle = -1;
            NPC.damage = 16;
            NPC.width = 32;
            NPC.height = 32;
            NPC.defense = 4;
            NPC.lifeMax = 25;
            NPC.knockBackResist = 0.35f;
            NPC.value = Item.buyPrice(copper: 80);
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = WulfrumAmplifier.Hit;
            NPC.DeathSound = CommonCalamitySounds.WulfrumNPCDeathSound;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<WulfrumDroneBanner>();
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.WulfrumDrone")
            });
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(FlyAwayTimer);

        public override void ReceiveExtraAI(BinaryReader reader) => FlyAwayTimer = reader.ReadSingle();

        public override void AI()
        {
            NPC.TargetClosest(false);

            Player player = Main.player[NPC.target];

            bool farFromPlayer = NPC.Distance(player.Center) > 960f;

            if (NPC.target < 0 || NPC.target >= Main.maxPlayers || farFromPlayer || player.dead || !player.active)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                farFromPlayer = NPC.Distance(player.Center) > 960f;
                // Fly away if there is no living target, or the closest target is too far away.
                if (player.dead || !player.active || farFromPlayer)
                {
                    if (FlyAwayTimer > 420)
                    {
                        NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.UnitY * -8f, 0.1f);
                        NPC.rotation = NPC.rotation.AngleTowards(0f, MathHelper.ToRadians(15f));
                        NPC.noTileCollide = true;
                    }
                    else
                    {
                        NPC.velocity *= 0.96f;
                        NPC.rotation = NPC.rotation.AngleTowards(0f, MathHelper.ToRadians(15f));
                        FlyAwayTimer++;
                    }
                    return;
                }
            }

            FlyAwayTimer = Utils.Clamp(FlyAwayTimer - 3, 0, 180);

            NPC.noTileCollide = !farFromPlayer;

            if (Supercharged)
            {
                SuperchargeTimer--;
            }

            if (AIState == DroneAIState.Searching)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                if (NPC.direction == 0)
                    NPC.direction = 1;

                float searchVelocity = CalamityWorld.death ? 10f : CalamityWorld.revenge ? 9f : Main.expertMode ? 8f : 6f;
                Vector2 destination = player.Center + new Vector2(300f * NPC.direction, -90f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(destination) * searchVelocity, searchVelocity / 60f);
                if (NPC.Distance(destination) < 40f)
                {
                    Time++;
                    NPC.velocity *= 0.95f;
                    float chargeDelay = CalamityWorld.death ? 10f : CalamityWorld.revenge ? 15f : Main.expertMode ? 20f : 40f;
                    if (Time >= chargeDelay)
                    {
                        AIState = DroneAIState.Charging;
                        NPC.netUpdate = true;
                    }
                }
            }
            else
            {
                // Set damage
                NPC.damage = NPC.defDamage;

                float chargeVelocity = CalamityWorld.death ? 10f : CalamityWorld.revenge ? 9f : Main.expertMode ? 8f : 6f;
                if (HorizontalChargeTime < 25)
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(player.Center) * chargeVelocity, chargeVelocity / 60f);

                if (Supercharged && Main.netMode != NetmodeID.MultiplayerClient && HorizontalChargeTime % 30f == 29f)
                {
                    int damage = Main.masterMode ? 8 : Main.expertMode ? 9 : 12;
                    if (Main.zenithWorld)
                    {
                        int spread = 15;
                        for (int times = 3; times > 0; times--)
                        {
                            Vector2 velocity = NPC.SafeDirectionTo(player.Center, Vector2.UnitY) * 6f;
                            Vector2 perturbedspeed = new Vector2(velocity.X + Main.rand.Next(-2, 3), velocity.Y + Main.rand.Next(-2, 3)).RotatedBy(MathHelper.ToRadians(spread));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.UnitX * 6f * NPC.spriteDirection, perturbedspeed, ProjectileID.SaucerLaser, damage, 0f);
                            spread -= Main.rand.Next(5, 8);
                        }
                    }
                    else
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.UnitX * 6f * NPC.spriteDirection, NPC.SafeDirectionTo(player.Center, Vector2.UnitY) * 6f, ProjectileID.SaucerLaser, damage, 0f);

                    SoundEngine.PlaySound(SoundID.Item12);
                }

                float totalChargeTime = TotalHorizontalChargeTime - (CalamityWorld.death ? 25f : CalamityWorld.revenge ? 20f : Main.expertMode ? 15f : 0f);
                HorizontalChargeTime++;
                if (HorizontalChargeTime > totalChargeTime)
                {
                    AIState = DroneAIState.Searching;
                    HorizontalChargeTime = 0f;
                    NPC.direction = (player.Center.X - NPC.Center.X < 0).ToDirectionInt();
                    NPC.netUpdate = true;
                }
            }

            NPC.spriteDirection = (NPC.velocity.X < 0).ToDirectionInt();
            NPC.rotation = NPC.velocity.X / 25f;

            // Generate idle dust
            if (!Main.dedServ)
            {
                Dust dust = Dust.NewDustPerfect(NPC.Bottom, DustID.Vortex);
                dust.color = Color.Green;
                dust.scale = 0.675f;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            int frame = (int)(NPC.frameCounter / 5) % (Main.npcFrameCount[Type] / 2);
            if (Supercharged)
                frame += Main.npcFrameCount[Type] / 2;

            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || spawner.Player.Calamity().ZoneSulphur || (!spawner.Player.ZoneOverworldHeight && !Main.remixWorld) || (!spawner.Player.ZoneNormalCaverns && spawner.Player.ZoneGlowshroom && Main.remixWorld))
                return 0f;

            return (Main.remixWorld ? SpawnCondition.Cavern.Chance : SpawnCondition.OverworldDaySlime.Chance) * (Main.hardMode ? 0.055f : 0.135f) * (NPC.AnyNPCs(ModContent.NPCType<WulfrumAmplifier>()) ? 5.5f : 1f);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
                }

                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("WulfrumDroneGore1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("WulfrumDroneGore2").Type, 1f);

                    int randomGoreCount = Main.rand.Next(1, 4);
                    for (int i = 0; i < randomGoreCount; i++)
                    {
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("WulfrumEnemyGore" + Main.rand.Next(1, 11).ToString()).Type, 1f);
                    }
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<WulfrumMetalScrap>(), 1, 1, 3);
            npcLoot.Add(ModContent.ItemType<WulfrumBattery>(), new Fraction(7, 100));
            npcLoot.AddIf(info => info.npc.ModNPC<WulfrumDrone>().Supercharged, ModContent.ItemType<EnergyCore>());
        }
    }
}
