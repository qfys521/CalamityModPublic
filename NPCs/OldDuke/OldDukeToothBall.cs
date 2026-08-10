using System;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.OldDuke
{
    public class OldDukeToothBall : ModNPC
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
        }

        public static int ToothDamage = 55; // 220
        public static int CloudDamage = 70; // 280

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 120; // 240
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.width = 40;
            NPC.height = 40;
            NPC.defense = 0;
            NPC.lifeMax = 8000;
            if (BossRushEvent.BossRushActive)
            {
                NPC.lifeMax = 16000;
            }
            NPC.knockBackResist = 0.2f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath11;
            NPC.chaseable = false;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void AI()
        {
            Lighting.AddLight((int)((NPC.position.X + (NPC.width / 2)) / 16f), (int)((NPC.position.Y + (NPC.height / 2)) / 16f), 0.65f, 0.55f, 0f);

            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            NPC.rotation += NPC.velocity.X * 0.05f;

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead)
                {
                    if (NPC.timeLeft > 10)
                        NPC.timeLeft = 10;

                    return;
                }
            }
            else if (NPC.timeLeft < 600)
                NPC.timeLeft = 600;

            Vector2 vector = player.Center - NPC.Center;
            float cannonballMovementGateValue = 120f;
            float slowDownGateValue = cannonballMovementGateValue + 300f;
            float dieGateValue = slowDownGateValue + 60f;
            NPC.ai[3] += 1f;
            if (vector.Length() < 40f || NPC.ai[3] >= dieGateValue)
            {
                NPC.life = 0;
                NPC.HitEffect();
                NPC.checkDead();
                return;
            }

            if (NPC.ai[3] < cannonballMovementGateValue)
            {
                Vector2 finalCannonballVelocity = new Vector2(NPC.ai[0], NPC.ai[1]);
                if (NPC.velocity.Length() < finalCannonballVelocity.Length())
                {
                    NPC.velocity *= 1.01f;
                    if (NPC.velocity.Length() > finalCannonballVelocity.Length())
                    {
                        NPC.velocity.Normalize();
                        NPC.velocity *= finalCannonballVelocity.Length();
                    }
                }

                return;
            }

            if (NPC.ai[3] > slowDownGateValue)
            {
                NPC.velocity *= 0.95f;
                return;
            }

            float velocity = death ? 14f : revenge ? 13f : 12f;
            if (expertMode)
            {
                float speedUpMult = 0.005f;
                velocity += Vector2.Distance(player.Center, NPC.Center) * speedUpMult;
            }

            Vector2 toothBallDirection = new Vector2(NPC.Center.X + NPC.direction * 20, NPC.Center.Y + 6f);
            float targetXDist = player.position.X + player.width * 0.5f - toothBallDirection.X;
            float targetYDist = player.Center.Y - toothBallDirection.Y;
            float targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);
            float toothBallSpeed = velocity / targetDistance;
            targetXDist *= toothBallSpeed;
            targetYDist *= toothBallSpeed;

            NPC.ai[2] -= Main.rand.Next(6);
            if (targetDistance < 300f || NPC.ai[2] > 0f)
            {
                if (targetDistance < 300f)
                    NPC.ai[2] = 100f;

                if (NPC.velocity.X < 0f)
                    NPC.direction = -1;
                else
                    NPC.direction = 1;

                return;
            }

            float inertia = 50f;
            NPC.velocity.X = (NPC.velocity.X * inertia + targetXDist) / (inertia + 1f);
            NPC.velocity.Y = (NPC.velocity.Y * inertia + targetYDist) / (inertia + 1f);

            float toothBallAccel = 0.5f;
            foreach (var n in Main.ActiveNPCs)
            {
                if (n.whoAmI != NPC.whoAmI && n.type == NPC.type)
                {
                    if (Vector2.Distance(NPC.Center, n.Center) < 48f)
                    {
                        if (NPC.position.X < n.position.X)
                            NPC.velocity.X -= toothBallAccel;
                        else
                            NPC.velocity.X += toothBallAccel;

                        if (NPC.position.Y < n.position.Y)
                            NPC.velocity.Y -= toothBallAccel;
                        else
                            NPC.velocity.Y += toothBallAccel;
                    }
                }
            }
        }

        public override void OnKill()
        {
            for (int i = 0; i < 15; i++)
            {
                float sc = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(10, 25), 0).RotatedByRandom(MathHelper.TwoPi);

                GeneralParticleHandler.SpawnParticle(new CustomSpark(NPC.Center, vel, "CalamityMod/Projectiles/Boss/OldDukeToothBallSpike", true, 40, sc + 0.5f, OldDuke.GlowColor, Vector2.One));
                GeneralParticleHandler.SpawnParticle(new CustomSpark(NPC.Center, vel, "CalamityMod/Projectiles/Boss/OldDukeToothBallSpike", true, 40, sc, Color.White, Vector2.One, false, affectedByLight: true));
            }

            int closestPlayer = Player.FindClosest(NPC.Center, 1, 1);
            if (Main.rand.NextBool(8) && Main.player[closestPlayer].statLife < Main.player[closestPlayer].statLifeMax2)
                Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int totalProjectiles = CalamityWorld.death ? 5 : CalamityWorld.revenge ? 4 : 3;
                float radians = MathHelper.TwoPi / totalProjectiles;
                int type = ModContent.ProjectileType<OldDukeToothBallSpike>();
                float velocity = 10f;
                double angleA = radians * 0.5;
                double angleB = MathHelper.ToRadians(90f) - angleA;
                float velocityX = (float)(velocity * Math.Sin(angleA) / Math.Sin(angleB));
                Vector2 spinningPoint = Main.rand.NextBool() ? new Vector2(0f, -velocity) : new Vector2(-velocityX, -velocity);
                for (int k = 0; k < totalProjectiles; k++)
                {
                    Vector2 toothSpikeRotation = spinningPoint.RotatedBy(radians * k);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, toothSpikeRotation * 0.1f, type, ToothDamage, 0f, Main.myPlayer, toothSpikeRotation.X, toothSpikeRotation.Y);
                }

                if (Main.expertMode)
                {
                    type = ModContent.ProjectileType<SandPoisonCloudOldDuke>();
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, CloudDamage, 0f, Main.myPlayer);
                }
            }

            if (Main.zenithWorld)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int spawnX = NPC.width / 2;
                    int type = ModContent.ProjectileType<OldDukeGore>();
                    for (int i = 0; i < 2; i++)
                        Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center.X + Main.rand.Next(-spawnX, spawnX), NPC.Center.Y,
                            Main.rand.Next(-1, 2), Main.rand.Next(-6, -3), type, OldDuke.GoreDamage, 0f, Main.myPlayer);
                }
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.BossNoCheese;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float velDist = NPC.velocity.Length();

            Vector2 vel = NPC.velocity;
            vel.Normalize();

            if (velDist > 5)
            {
                float sc = velDist - 5;
                if (Main.rand.NextBool(3))
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(NPC.Center + new Vector2(Main.rand.NextFloat(5, 30), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)), NPC.velocity, false, 20, Main.rand.NextFloat(0.5f, 1.5f), OldDuke.GlowColor, true));
            }

            if (Main.rand.NextBool(3))
                GeneralParticleHandler.SpawnParticle(new MediumMistParticle(NPC.Center, -(vel * 4f).RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)), OldDuke.GlowColor, Color.DarkSlateGray, Main.rand.NextFloat(0.5f, 1.5f), 150f));

            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            for (int i = 0; i < 360; i += 90)
            {
                Main.EntitySpriteDraw(tex.Value, NPC.Center + new Vector2(0, 4) - Main.screenPosition + new Vector2(4, 0).RotatedBy(MathHelper.ToRadians(i)), tex.Frame(), OldDuke.GlowColor, NPC.rotation, tex.Frame().Center(), NPC.scale, SpriteEffects.None);
                Main.EntitySpriteDraw(tex.Value, NPC.Center + new Vector2(0, 4) - Main.screenPosition + new Vector2(8, 0).RotatedBy(MathHelper.ToRadians(i)), tex.Frame(), OldDuke.GlowColor, NPC.rotation, tex.Frame().Center(), NPC.scale, SpriteEffects.None);
            }

            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.SulphurousSeaAcid, hit.HitDirection, -1f, 0, default, 1f);

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.SulphurousSeaAcid, hit.HitDirection, -1f, 0, default, 1f);

                NPC.position.X = NPC.position.X + NPC.width / 2;
                NPC.position.Y = NPC.position.Y + NPC.height / 2;
                NPC.width = NPC.height = 96;
                NPC.position.X = NPC.position.X - NPC.width / 2;
                NPC.position.Y = NPC.position.Y - NPC.height / 2;

                for (int i = 0; i < 15; i++)
                {
                    int bloody = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 0f, 0f, 100, default, 2f);
                    Main.dust[bloody].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[bloody].scale = 0.5f;
                        Main.dust[bloody].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                    Main.dust[bloody].noGravity = true;
                }

                for (int j = 0; j < 30; j++)
                {
                    int toxicDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.SulphurousSeaAcid, 0f, 0f, 100, default, 3f);
                    Main.dust[toxicDust].noGravity = true;
                    Main.dust[toxicDust].velocity *= 5f;
                    toxicDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.SulphurousSeaAcid, 0f, 0f, 100, default, 2f);
                    Main.dust[toxicDust].velocity *= 2f;
                    Main.dust[toxicDust].noGravity = true;
                }

                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("OldDukeToothBallGore").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("OldDukeToothBallGore2").Type, NPC.scale);
                }
            }
        }
    }
}
