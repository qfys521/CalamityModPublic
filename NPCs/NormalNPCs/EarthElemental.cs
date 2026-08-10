using System;
using System.IO;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.NormalNPCs
{
    [LegacyName("Horse")]
    public class EarthElemental : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Scale = 0.4f,
                PortraitScale = 0.6f,
                PortraitPositionYOverride = -20f
            };
            value.Position.X += 28f;
            value.Position.Y -= 56f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.npcSlots = 3f;
            NPC.damage = 50;
            NPC.width = 230;
            NPC.height = 230;
            NPC.defense = 20;
            NPC.lifeMax = 3800;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0.05f;
            NPC.value = Item.buyPrice(gold: 1, silver: 50);
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.rarity = 2;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<EarthElementalBanner>();
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.EarthElemental")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.dontTakeDamage);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.dontTakeDamage = reader.ReadBoolean();
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || !Main.hardMode || spawner.Player.Calamity().ZoneAbyss || spawner.Player.Calamity().ZoneSunkenSea || !spawner.Player.ZoneRockLayerHeight)
                return 0f;

            // Keep this as a separate if check, because it's a loop and we don't want to be checking it constantly.
            if (NPC.AnyNPCs(NPC.type))
                return 0f;

            return SpawnCondition.Cavern.Chance * 0.005f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.ai[0] == 0f && !NPC.IsABestiaryIconDummy)
                return;

            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[Type] * frameHeight);
                NPC.frameCounter = 0;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            var weapons = new int[]
            {
                ModContent.ItemType<EarthenPike>(),
                ModContent.ItemType<SlagMagnum>(),
            };
            npcLoot.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, weapons));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                NPC.position.X = NPC.position.X + (NPC.width / 2);
                NPC.position.Y = NPC.position.Y + (NPC.height / 2);
                NPC.width = 160;
                NPC.height = 160;
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                for (int i = 0; i < 40; i++)
                {
                    int earthDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[earthDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[earthDust].scale = 0.5f;
                        Main.dust[earthDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int j = 0; j < 70; j++)
                {
                    int earthDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                    Main.dust[earthDust2].noGravity = true;
                    Main.dust[earthDust2].velocity *= 5f;
                    earthDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                    Main.dust[earthDust2].velocity *= 2f;
                }

                if (!Main.dedServ)
                {
                    Vector2 goreSource = NPC.Center;
                    int goreAmt = 3;
                    Vector2 source = new Vector2(goreSource.X - 24f, goreSource.Y - 24f);
                    for (int goreIndex = 0; goreIndex < goreAmt; goreIndex++)
                    {
                        float velocityMult = 0.33f;
                        if (goreIndex < (goreAmt / 3))
                        {
                            velocityMult = 0.66f;
                        }
                        if (goreIndex >= (2 * goreAmt / 3))
                        {
                            velocityMult = 1f;
                        }
                        Mod mod = ModContent.GetInstance<CalamityMod>();
                        int type = Main.rand.Next(61, 64);
                        int smoke = Gore.NewGore(NPC.GetSource_Death(), source, default, type, 1f);
                        Gore gore = Main.gore[smoke];
                        gore.velocity *= velocityMult;
                        gore.velocity.X += 1f;
                        gore.velocity.Y += 1f;
                        type = Main.rand.Next(61, 64);
                        smoke = Gore.NewGore(NPC.GetSource_Death(), source, default, type, 1f);
                        gore = Main.gore[smoke];
                        gore.velocity *= velocityMult;
                        gore.velocity.X -= 1f;
                        gore.velocity.Y += 1f;
                        type = Main.rand.Next(61, 64);
                        smoke = Gore.NewGore(NPC.GetSource_Death(), source, default, type, 1f);
                        gore = Main.gore[smoke];
                        gore.velocity *= velocityMult;
                        gore.velocity.X += 1f;
                        gore.velocity.Y -= 1f;
                        type = Main.rand.Next(61, 64);
                        smoke = Gore.NewGore(NPC.GetSource_Death(), source, default, type, 1f);
                        gore = Main.gore[smoke];
                        gore.velocity *= velocityMult;
                        gore.velocity.X -= 1f;
                        gore.velocity.Y -= 1f;
                    }
                }
            }
        }

        public override bool PreAI()
        {
            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            if (Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) < 480f)
            {
                if (NPC.ai[0] == 0f)
                {
                    if (Main.zenithWorld)
                    {
                        SoundEngine.PlaySound(SoundID.ScaryScream, Main.player[NPC.target].Center);
                    }
                    NPC.ai[0] = 1f;
                    NPC.dontTakeDamage = false;
                }
            }
            else
                NPC.TargetClosest();

            if (NPC.ai[0] == 0f)
                return false;

            if (Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                if (NPC.velocity.Y < -2f)
                    NPC.velocity.Y = -2f;
                NPC.velocity.Y += 0.1f;
                if (NPC.velocity.Y > 12f)
                    NPC.velocity.Y = 12f;

                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
            }

            NPC.localAI[0] += 1f;
            if (NPC.localAI[0] >= 300f)
            {
                NPC.localAI[0] = 0f;
                SoundEngine.PlaySound(SoundID.NPCHit43, NPC.Center);
                NPC.TargetClosest();
                if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                {
                    float rockSpeed = 4f;
                    int damage = Main.masterMode ? 18 : Main.expertMode ? 22 : 30;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 projPosition = NPC.Center;
                        float targetXDist = Main.player[NPC.target].position.X + Main.player[NPC.target].width * 0.5f - projPosition.X;
                        float absoluteTargetX = Math.Abs(targetXDist) * 0.1f;
                        float targetYDist = Main.player[NPC.target].position.Y + Main.player[NPC.target].height * 0.5f - projPosition.Y - absoluteTargetX;
                        float targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);
                        targetDistance = rockSpeed / targetDistance;
                        targetXDist *= targetDistance;
                        targetYDist *= targetDistance;
                        int rockType = ModContent.ProjectileType<EarthRockSmall>();
                        projPosition.X += targetXDist;
                        projPosition.Y += targetYDist;
                        for (int k = 0; k < 4; k++)
                        {
                            rockType = Main.rand.NextBool(4) ? ModContent.ProjectileType<EarthRockBig>() : ModContent.ProjectileType<EarthRockSmall>();
                            targetXDist = Main.player[NPC.target].position.X + Main.player[NPC.target].width * 0.5f - projPosition.X;
                            targetYDist = Main.player[NPC.target].position.Y + Main.player[NPC.target].height * 0.5f - projPosition.Y;
                            targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);
                            targetDistance = rockSpeed / targetDistance;
                            targetXDist += Main.rand.Next(-40, 41);
                            targetYDist += Main.rand.Next(-40, 41);
                            targetXDist *= targetDistance;
                            targetYDist *= targetDistance;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), projPosition.X, projPosition.Y, targetXDist, targetYDist, rockType, damage, 0f, Main.myPlayer, 0f, 0f);
                        }
                    }
                }
            }

            float playerLocation = NPC.Center.X - Main.player[NPC.target].Center.X;
            NPC.direction = playerLocation < 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            Vector2 direction = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            float chargeGateValue = Main.expertMode ? 300f : 600f;
            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= chargeGateValue)
            {
                // Set damage
                NPC.damage = NPC.defDamage;

                float chargeVelocity = Main.expertMode ? 9f : 6f;
                direction *= chargeVelocity;
                NPC.velocity = direction;
                NPC.ai[1] = -30f;
            }

            if (Math.Sqrt((NPC.velocity.X * NPC.velocity.X) + (NPC.velocity.Y * NPC.velocity.Y)) > 1D && NPC.ai[1] >= 0f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.velocity *= 0.97f;
            }

            if (Math.Sqrt((NPC.velocity.X * NPC.velocity.X) + (NPC.velocity.Y * NPC.velocity.Y)) <= 1.15)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.velocity = direction;
            }

            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 300);
        }
    }
}
