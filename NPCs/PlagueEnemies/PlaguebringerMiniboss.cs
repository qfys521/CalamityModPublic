using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.PlagueEnemies
{
    public class PlaguebringerMiniboss : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Scale = 0.7f,
                PortraitScale = 0.8f,
            };
            value.Position.X += 20f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 70;
            NPC.npcSlots = 8f;
            NPC.width = 66;
            NPC.height = 66;
            NPC.defense = 24;
            NPC.lifeMax = 8750;
            NPC.value = Item.buyPrice(gold: 1, silver: 50);
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            AIType = -1;
            AnimationType = NPCID.QueenBee;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<PlaguebringerBanner>();
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            if (Main.zenithWorld)
            {
                NPC.scale = 2f;
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundJungle,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PlaguebringerMiniboss")
            });
        }

        public override void AI()
        {
            Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.1f, 0.3f, 0f);

            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            // Despawn
            float distanceFromTarget = Vector2.Distance(NPC.Center, Main.player[NPC.target].Center);
            if (NPC.ai[0] != 6f)
            {
                if (NPC.timeLeft < 60)
                    NPC.timeLeft = 60;
                if (distanceFromTarget > 3000f)
                    NPC.ai[0] = 4f;
            }
            if (Main.player[NPC.target].dead || !Main.player[NPC.target].ZoneJungle)
                NPC.ai[0] = 6f;

            if (NPC.ai[0] == 6f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.velocity.Y *= 0.98f;

                if (NPC.velocity.X < 0f)
                    NPC.direction = -1;
                else
                    NPC.direction = 1;

                NPC.spriteDirection = NPC.direction;

                if (NPC.position.X < (Main.maxTilesX * 8))
                {
                    if (NPC.velocity.X > 0f)
                        NPC.velocity.X *= 0.98f;
                    else
                        NPC.localAI[0] = 1f;

                    NPC.velocity.X -= 0.08f;
                }
                else
                {
                    if (NPC.velocity.X < 0f)
                        NPC.velocity.X *= 0.98f;
                    else
                        NPC.localAI[0] = 1f;

                    NPC.velocity.X += 0.08f;
                }

                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;
            }
            else if (NPC.ai[0] == -1f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float currentAttack = NPC.ai[1];
                    int nextAttack;
                    do nextAttack = Main.rand.Next(2);
                    while ((float)nextAttack == currentAttack);
                    NPC.ai[0] = (float)nextAttack;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    return;
                }
            }
            else if (NPC.ai[0] == 0f)
            {
                // Charging distance from player
                int chargeDistanceX = 750 - (CalamityWorld.death ? 100 : CalamityWorld.revenge ? 75 : Main.expertMode ? 50 : 0);

                int chargeDelay = 2;
                if (NPC.ai[1] > (float)(2 * chargeDelay) && NPC.ai[1] % 2f == 0f)
                {
                    NPC.ai[0] = -1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                    return;
                }

                if (NPC.ai[1] % 2f == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.TargetClosest();

                    float chargeDistanceY = 20f;
                    float distanceFromTargetX = Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X);
                    float distanceFromTargetY = Math.Abs(NPC.Center.Y - Main.player[NPC.target].Center.Y);
                    if (distanceFromTargetY < chargeDistanceY && distanceFromTargetX >= chargeDistanceX)
                    {
                        // Set damage
                        NPC.damage = NPC.defDamage;

                        NPC.localAI[0] = 1f;
                        NPC.ai[1] += 1f;
                        NPC.ai[2] = 0f;
                        float chargeSpeed = 16f;
                        Vector2 chargeBeePos = NPC.Center;
                        float chargeTargetX = Main.player[NPC.target].Center.X - chargeBeePos.X;
                        float chargeTargetY = Main.player[NPC.target].Center.Y - chargeBeePos.Y;
                        float chargeTargetDist = (float)Math.Sqrt((double)(chargeTargetX * chargeTargetX + chargeTargetY * chargeTargetY));
                        chargeTargetDist = chargeSpeed / chargeTargetDist;
                        NPC.velocity.X = chargeTargetX * chargeTargetDist;
                        NPC.velocity.Y = chargeTargetY * chargeTargetDist;

                        // Direction
                        float playerLocation = NPC.Center.X - Main.player[NPC.target].Center.X;
                        NPC.direction = playerLocation < 0 ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;

                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                        return;
                    }

                    // Velocity variables
                    NPC.localAI[0] = 0f;
                    float chargeVelocity = 12f;
                    float chargeAcceleration = 0.15f;

                    // Velocity calculations
                    if (NPC.Center.Y < Main.player[NPC.target].Center.Y - chargeDistanceY)
                        NPC.velocity.Y += chargeAcceleration;
                    else if (NPC.Center.Y > Main.player[NPC.target].Center.Y + chargeDistanceY)
                        NPC.velocity.Y -= chargeAcceleration;
                    else
                        NPC.velocity.Y *= 0.7f;

                    if (NPC.velocity.Y < -chargeVelocity)
                        NPC.velocity.Y = -chargeVelocity;
                    if (NPC.velocity.Y > chargeVelocity)
                        NPC.velocity.Y = chargeVelocity;

                    float distanceXMax = 100f;
                    float distanceXMin = 20f;
                    if (distanceFromTargetX > chargeDistanceX + distanceXMax)
                        NPC.velocity.X += chargeAcceleration * NPC.direction;
                    else if (distanceFromTargetX < chargeDistanceX + distanceXMin)
                        NPC.velocity.X -= chargeAcceleration * NPC.direction;
                    else
                        NPC.velocity.X *= 0.7f;

                    // Limit velocity
                    if (NPC.velocity.X < -chargeVelocity)
                        NPC.velocity.X = -chargeVelocity;
                    if (NPC.velocity.X > chargeVelocity)
                        NPC.velocity.X = chargeVelocity;

                    // Direction
                    float playerLocation2 = NPC.Center.X - Main.player[NPC.target].Center.X;
                    NPC.direction = playerLocation2 < 0 ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;
                }
                else
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    if (NPC.velocity.X < 0f)
                        NPC.direction = -1;
                    else
                        NPC.direction = 1;

                    NPC.spriteDirection = NPC.direction;

                    int chargeDirection = 1;
                    if (NPC.Center.X < Main.player[NPC.target].Center.X)
                        chargeDirection = -1;

                    if (NPC.direction == chargeDirection && Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) > chargeDistanceX)
                        NPC.ai[2] = 1f;
                    if (Math.Abs(NPC.Center.Y - Main.player[NPC.target].Center.Y) > chargeDistanceX * 1.5f)
                        NPC.ai[2] = 1f;

                    if (NPC.ai[2] != 1f)
                    {
                        NPC.localAI[0] = 1f;
                        return;
                    }

                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    // Direction
                    float playerLocation = NPC.Center.X - Main.player[NPC.target].Center.X;
                    NPC.direction = playerLocation < 0 ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;

                    NPC.TargetClosest();

                    NPC.localAI[0] = 0f;

                    NPC.velocity *= 0.92f;
                    float chargeDeceleration = 0.08f;
                    if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < chargeDeceleration)
                    {
                        NPC.ai[2] = 0f;
                        NPC.ai[1] += 1f;
                    }
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                // Direction
                float playerLocation = NPC.Center.X - Main.player[NPC.target].Center.X;
                NPC.direction = playerLocation < 0 ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                float stingerAttackSpeed = 12f;
                float stingerAttackAccel = 0.12f;
                Vector2 stingerSpawnPos = new Vector2(NPC.Center.X + (float)(40 * NPC.direction), NPC.Bottom.Y + 30f);
                Vector2 stingerAttackPos = NPC.Center;
                float stingerAttackTargetX = Main.player[NPC.target].Center.X - stingerAttackPos.X;
                float stingerAttackTargetY = Main.player[NPC.target].Center.Y - 300f - stingerAttackPos.Y;
                float stingerAttackTargetDist = (float)Math.Sqrt((double)(stingerAttackTargetX * stingerAttackTargetX + stingerAttackTargetY * stingerAttackTargetY));

                bool canHitTarget = Collision.CanHit(new Vector2(stingerSpawnPos.X, stingerSpawnPos.Y - 30f), 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height);
                Vector2 hoverDestination = Main.player[NPC.target].Center - Vector2.UnitY * (!canHitTarget ? 0f : 300f);
                Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * stingerAttackSpeed;

                NPC.ai[1] += 1f;
                float stingerGateValue = CalamityWorld.death ? 15f : CalamityWorld.revenge ? 20f : Main.expertMode ? 25f : 35f;
                bool canFireStinger = false;
                if (NPC.ai[1] % stingerGateValue == stingerGateValue - 1f)
                    canFireStinger = true;

                if (canFireStinger && NPC.position.Y + (float)NPC.height < Main.player[NPC.target].position.Y && Collision.CanHit(stingerSpawnPos, 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                {
                    SoundEngine.PlaySound(SoundID.Item42, stingerSpawnPos);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float stingerSpeed = 6f;
                        float stingerTargetX = Main.player[NPC.target].Center.X - stingerSpawnPos.X;
                        float stingerTargetY = Main.player[NPC.target].Center.Y - stingerSpawnPos.Y;
                        float stingerTargetDist = (float)Math.Sqrt((double)(stingerTargetX * stingerTargetX + stingerTargetY * stingerTargetY));
                        stingerTargetDist = stingerSpeed / stingerTargetDist;
                        stingerTargetX *= stingerTargetDist;
                        stingerTargetY *= stingerTargetDist;
                        int rocketChance = CalamityWorld.death ? 4 : CalamityWorld.revenge ? 7 : Main.expertMode ? 10 : 15;
                        bool fireRocket = Main.rand.NextBool(rocketChance);
                        int type = fireRocket ? ModContent.ProjectileType<HiveBombGoliath>() : ModContent.ProjectileType<PlagueStingerGoliathV2>();
                        int damage = Main.masterMode ? (fireRocket ? 42 : 29) : Main.expertMode ? (fireRocket ? 50 : 35) : (fireRocket ? 72 : 52);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), stingerSpawnPos.X, stingerSpawnPos.Y, stingerTargetX, stingerTargetY, type, damage, 0f, Main.myPlayer);
                    }
                }

                // Movement calculations
                if (Vector2.Distance(stingerSpawnPos, hoverDestination) > 40f || !canHitTarget)
                    NPC.SimpleFlyMovement(idealVelocity, stingerAttackAccel);

                float stingerPhaseTime = CalamityWorld.death ? 180f : CalamityWorld.revenge ? 240f : Main.expertMode ? 300f : 600f;
                if (NPC.ai[1] > stingerPhaseTime)
                {
                    NPC.ai[0] = -1f;
                    NPC.ai[1] = 1f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.localAI[0] = 1f;
                float despawnVelMult = 14f;

                Vector2 despawnTargetDist = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                despawnTargetDist *= 14f;

                NPC.velocity = (NPC.velocity * despawnVelMult + despawnTargetDist) / (despawnVelMult + 1f);
                if (NPC.velocity.X < 0f)
                    NPC.direction = -1;
                else
                    NPC.direction = 1;

                NPC.spriteDirection = NPC.direction;

                if (distanceFromTarget < 2000f)
                {
                    NPC.ai[0] = -1f;
                    NPC.localAI[0] = 0f;
                }
            }

            if (Main.dedServ)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture2D15 = TextureAssets.Npc[Type].Value;
            Vector2 halfSizeTexture = new Vector2((float)(TextureAssets.Npc[Type].Value.Width / 2), (float)(TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type] / 2));

            if (CalamityClientConfig.Instance.Afterimages)
            {
                if (NPC.ai[0] == 0f && NPC.localAI[0] == 1f)
                {
                    int chargeAfterimageAmount = 6;
                    for (int i = 1; i < chargeAfterimageAmount; i += 2)
                    {
                        Color afterimageColor = drawColor;
                        afterimageColor = Color.Lerp(afterimageColor, Color.White, 0.5f);
                        afterimageColor = NPC.GetAlpha(afterimageColor);
                        afterimageColor *= (float)(chargeAfterimageAmount - i) / 15f;
                        Vector2 afterimagePos = NPC.oldPos[i] + new Vector2((float)NPC.width, (float)NPC.height) / 2f - screenPos;
                        afterimagePos -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[Type])) * NPC.scale / 2f;
                        afterimagePos += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                        spriteBatch.Draw(texture2D15, afterimagePos, NPC.frame, afterimageColor, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
                    }
                }
            }

            Vector2 drawLocation = NPC.Center - screenPos;
            drawLocation -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[Type])) * NPC.scale / 2f;
            drawLocation += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture2D15, drawLocation, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

            return false;
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.noWorms || !NPC.downedGolemBoss || !spawner.Player.ZoneJungle)
                return 0f;

            // Keep this as a separate if check, because it's a loop and we don't want to be checking it constantly.
            if (NPC.AnyNPCs(NPC.type))
                return 0f;

            return SpawnCondition.HardmodeJungle.Chance * 0.02f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Plague, hit.HitDirection, -1f, 0, default, 1f);

            if (NPC.life <= 0)
            {
                NPC.position = NPC.Center;
                NPC.width = NPC.height = 100;
                NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (float)(NPC.height / 2);

                for (int i = 0; i < 40; i++)
                {
                    int plagueDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Plague, 0f, 0f, 100, default, 2f);
                    Main.dust[plagueDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[plagueDust].scale = 0.5f;
                        Main.dust[plagueDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 70; j++)
                {
                    int plagueDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Plague, 0f, 0f, 100, default, 3f);
                    Main.dust[plagueDust2].noGravity = true;
                    Main.dust[plagueDust2].velocity *= 5f;
                    plagueDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Plague, 0f, 0f, 100, default, 2f);
                    Main.dust[plagueDust2].velocity *= 2f;
                }
            }
        }

        public override void OnKill()
        {
            int heartAmt = Main.rand.Next(3) + 3;
            for (int i = 0; i < heartAmt; i++)
                Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<PlagueCellCanister>(), 1, 8, 12);
            npcLoot.Add(ModContent.ItemType<PlaguedFuelPack>(), 4);
            npcLoot.Add(ModContent.ItemType<PlagueCaller>(), 50);
            npcLoot.Add(ItemID.Stinger);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<Plague>(), 240);
        }
    }
}
