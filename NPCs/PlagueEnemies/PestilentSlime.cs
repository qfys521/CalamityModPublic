using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.PlagueEnemies
{
    public class PestilentSlime : ModNPC
    {
        public float spikeTimer = 60f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Slime;
            NPC.damage = 55;
            NPC.width = 40;
            NPC.height = 30;
            NPC.defense = 35;
            NPC.lifeMax = 600;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.CorruptSlime;
            NPC.value = Item.buyPrice(silver: 10);
            NPC.alpha = 60;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<PestilentSlimeBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundJungle,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PestilentSlime")
            });
        }

        public override void AI()
        {
            if (spikeTimer > 0f)
            {
                spikeTimer -= 1f;
            }
            if (!NPC.wet && !Main.player[NPC.target].npcTypeNoAggro[Type])
            {
                Vector2 slimePosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                float targetXDist = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - slimePosition.X;
                float targetYDist = Main.player[NPC.target].position.Y - slimePosition.Y;
                float targetDistance = (float)Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));
                if (Main.expertMode && targetDistance < 120f && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && NPC.velocity.Y == 0f)
                {
                    NPC.ai[0] = -40f;
                    if (NPC.velocity.Y == 0f)
                    {
                        NPC.velocity.X = NPC.velocity.X * 0.9f;
                    }
                    if (spikeTimer == 0f)
                    {
                        int damage = Main.masterMode ? 16 : Main.expertMode ? 19 : 25;
                        SoundEngine.PlaySound(SoundID.Item42, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int n = 0; n < 5; n++)
                            {
                                Vector2 spikeVelocity = new Vector2((float)(n - 2), -4f);
                                spikeVelocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.005f;
                                spikeVelocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.005f;
                                spikeVelocity.Normalize();
                                spikeVelocity *= 4f + (float)Main.rand.Next(-50, 51) * 0.01f;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), slimePosition.X, slimePosition.Y, spikeVelocity.X, spikeVelocity.Y, ModContent.ProjectileType<PlagueStingerGoliathV2>(), damage, 0f, Main.myPlayer, 0f, 0f);
                                spikeTimer = 30f;
                            }
                        }
                    }
                }
                else if (targetDistance < 360f && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && NPC.velocity.Y == 0f)
                {
                    NPC.ai[0] = -40f;
                    if (NPC.velocity.Y == 0f)
                    {
                        NPC.velocity.X = NPC.velocity.X * 0.9f;
                    }
                    if (spikeTimer == 0f)
                    {
                        int damage = Main.masterMode ? 14 : Main.expertMode ? 17 : 22;
                        SoundEngine.PlaySound(SoundID.Item42, NPC.Center);
                        targetYDist = Main.player[NPC.target].position.Y - slimePosition.Y - (float)Main.rand.Next(0, 200);
                        targetDistance = (float)Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));
                        targetDistance = 6.5f / targetDistance;
                        targetXDist *= targetDistance;
                        targetYDist *= targetDistance;
                        spikeTimer = 50f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), slimePosition.X, slimePosition.Y, targetXDist, targetYDist, ModContent.ProjectileType<PlagueStingerGoliathV2>(), damage, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
            }
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || !NPC.downedGolemBoss || spawner.Player.Calamity().ZoneSunkenSea)
            {
                return 0f;
            }
            return SpawnCondition.HardmodeJungle.Chance * 0.09f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Plague, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                SoundEngine.PlaySound(CommonCalamitySounds.PlagueBoomSound, NPC.Center);
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Plague, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<PlagueCellCanister>(), 1, 1, 2);
            npcLoot.Add(ItemDropRule.NormalvsExpert(ItemID.Stinger, 4, 2));
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<Plague>(), 180);
        }
    }
}
