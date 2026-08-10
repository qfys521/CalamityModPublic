using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using CalamityMod.DataStructures;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.Other
{
    public class VolatileSlime : ModNPC
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.QueenSlimeMinionPurple}";
        public ref float time => ref NPC.ai[0];
        public ref float playerID => ref NPC.ai[1];
        public Player owner => Main.player[(int)playerID];
        public bool released => NPC.ai[3] != 0;
        public bool doOnSpawnEffects = true;
        public Color effectColor = Color.BlueViolet;
        public List<Vector2> oldVelocities = new List<Vector2>();
        public Vector2 randPos = Vector2.Zero;
        public Vector2 goalPositionRand = Vector2.Zero;
        public float travelSpeed = 25;
        public float glowScale = 1;
        public bool drawChains = true;
        public float gravIntensity = 1;
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 4;
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 40;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 75;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0.8f;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
            NPC.Opacity = 0;
            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.Calamity().ProvidesProximityRage = false;
            NPC.Calamity().DoesNotDisappearInBossRush = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(travelSpeed);
            writer.Write(gravIntensity);
            writer.WriteVector2(goalPositionRand);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            travelSpeed = reader.ReadSingle();
            gravIntensity = reader.ReadSingle();
            goalPositionRand = reader.ReadVector2();
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter >= 6.0)
            {
                NPC.frame.Y += 1;
                NPC.frameCounter = 0.0;
            }
            if (NPC.frame.Y >= (released ? Main.npcFrameCount[Type] / 2 : Main.npcFrameCount[Type]))
                NPC.frame.Y = 0;
        }
        public override void AI()
        {
            if (doOnSpawnEffects)
            {
                // In multiplayer, the spawned npc won't spawn with the proper owner, so it just sets it to be the player it spawns on top of
                int player = NPC.FindClosestPlayer();
                NPC.ai[1] = player;

                NPC.Opacity = 1;
                NPC.scale = 0;
                doOnSpawnEffects = false;
                NPC.netUpdate = true;
            }
            float rate = (time * 0.05f);
            List<Color> eColors = new List<Color>()
                {
                    Color.SlateBlue,
                    Color.BlueViolet
                };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            effectColor = Color.Lerp(currentColor, nextColor, rate % 2f >= 1f ? 1f : rate % 1f);
            glowScale = MathHelper.Lerp(glowScale, 1, 0.13f);


            if (owner.wingTime <= 0 && !released)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { pitch = Main.rand.NextFloat(-0.1f, 0.1f), MaxInstances = 10 }, NPC.Center);
                time = 0;
                NPC.dontTakeDamage = false;
                NPC.noTileCollide = false;
                NPC.lavaImmune = false;
                NPC.frame.Y = 0;
                glowScale = 5f;
                gravIntensity = Main.rand.NextFloat(0.8f, 1.3f);

                NPC.ai[3] = 1; // release the slimes
                NPC.netUpdate = true;
                NPC.SyncMotionToServer();
            }

            if (released)
            {
                float sine = (float)Math.Sin(time * 0.6f / MathHelper.Pi);
                if (NPC.velocity.Y < 12 * gravIntensity)
                    NPC.velocity.Y += 0.4f * gravIntensity;
                if (Math.Abs(NPC.velocity.X) < 5 && time < 120)
                    NPC.velocity.X *= 1.03f;
                else
                    NPC.velocity.X *= 0.987f;
            }
            else
            {
                if (time % 120 == 0)
                {
                    goalPositionRand = Main.rand.NextVector2Circular(90, 90);
                    NPC.netUpdate = true;
                }
                randPos = Vector2.Lerp(randPos, goalPositionRand, 0.04f);

                float sineX = (float)Math.Sin(time * 0.4f / MathHelper.Pi);
                float sineY = (float)Math.Sin(time * 0.6f / MathHelper.Pi);

                NPC.velocity += owner.velocity;
                Vector2 destination = owner.Center - Vector2.UnitY * 120 + randPos + new Vector2(10 * sineX, 15 * sineY);

                // Anticlump
                int npcType = ModContent.NPCType<VolatileSlime>();
                for (int x = 0; x < Main.maxNPCs; x++)
                {
                    NPC npc = Main.npc[x];
                    float distFromNPC = Vector2.Distance(NPC.Center, npc.Center);
                    if (npc != NPC && npc.active && npc.type == npcType && npc.ai[1] == owner.whoAmI && npc.ai[3] == 0 && distFromNPC < 150)
                        destination += Utils.DirectionFrom(NPC.Center, npc.Center) * Utils.GetLerpValue(150, 0, distFromNPC) * 25;
                }
                travelSpeed = MathHelper.Lerp(travelSpeed, (owner.controlJump ? 2 : 25), 0.03f);
                
                NPC.velocity = (destination - NPC.Center) / travelSpeed;

                if (owner.controlJump && time % 3 == 0)
                    NPC.netUpdate = true;
            }
            if (NPC.velocity.X > 0 || NPC.velocity.X < 0)
                NPC.spriteDirection = Math.Sign(NPC.velocity.X);

            if (NPC.scale < 1)
                NPC.scale += 0.05f;

            if (NPC.collideX)
            {
                NPC.velocity.X *= -1.2f;
                NPC.netUpdate = true;
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y += -5.5f * gravIntensity - NPC.velocity.Y;
                NPC.netUpdate = true;
            }

            // Kill slime after enough time freed
            if (Main.netMode != NetmodeID.MultiplayerClient && ((time >= 600f && released) || (!owner.volatileGelatin) || (owner.dead && !released)))
            {
                NPC.StrikeInstantKill();
                NPC.netUpdate = true;
            }

            time++;

            oldVelocities.Add(NPC.velocity);
            int maxSaves = 18;
            if (oldVelocities.Count > maxSaves)
                oldVelocities.RemoveAt(0);
        }
        public override void OnKill()
        {
            if (owner.Calamity().volatileGelatinVisuals)
            {
                for (int i = 0; i <= 22; i++)
                {
                    Vector2 vel = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(3f, 8f);

                    Dust dust2 = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<LightDustPixelated>(), vel);
                    dust2.scale = Main.rand.NextFloat(0.8f, 1.7f);
                    bool gravity = !Main.rand.NextBool(5);
                    dust2.noGravity = gravity;
                    if (!gravity)
                        dust2.velocity /= 2;
                    dust2.alpha = Main.rand.Next(70, 150 + 1);
                    dust2.color = Main.rand.NextBool() ? Color.BlueViolet : Color.SlateBlue;
                    dust2.noLight = true;
                    dust2.noLightEmittance = true;
                }
            }
            NPC.netUpdate = true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = released ? Terraria.GameContent.TextureAssets.Npc[NPCID.QueenSlimeMinionBlue].Value : Terraria.GameContent.TextureAssets.Npc[NPCID.QueenSlimeMinionPurple].Value;
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * (released ? 8 : 0));
            Rectangle frame = Utils.Frame(texture, verticalFrames: (released ? Main.npcFrameCount[Type] / 2 : Main.npcFrameCount[Type]), frameY: NPC.frame.Y);
            Vector2 origin = frame.Size() * 0.5f;

            if (drawChains)
            {
                Texture2D chain = ModContent.Request<Texture2D>("CalamityMod/NPCs/Other/VolatileChain").Value;
                Vector2 end = NPC.Center;

                Vector2 drawStart = owner.Center;
                List<Vector2> controlPoints = new List<Vector2>
            {
                drawStart
            };
                for (int i = 0; i < oldVelocities.Count; i++)
                {
                    float swayResponsiveness = Utils.GetLerpValue(0f, 18f, i, true) * Utils.GetLerpValue(oldVelocities.Count, oldVelocities.Count - 18f, i, true) * Utils.GetLerpValue(5, 20, Utils.Distance(drawStart, end), true);
                    Vector2 swayTotalOffset = oldVelocities.ElementAt(i) * swayResponsiveness * -15;
                    controlPoints.Add(Vector2.Lerp(drawStart, end, i / (float)oldVelocities.Count) + swayTotalOffset);
                }
                controlPoints.Add(end);

                int chainPointCount = (int)(Vector2.Distance(controlPoints.First(), controlPoints.Last()) / 11f);
                if (chainPointCount < 14)
                    chainPointCount = 14;


                BezierCurve bezierCurve = new BezierCurve(controlPoints.ToArray());
                List<Vector2> chainPoints = bezierCurve.GetPoints(chainPointCount);

                for (int i = 0; i < chainPoints.Count; i++)
                {
                    float completion = Utils.GetLerpValue(chainPoints.Count * 0.2f, chainPoints.Count, i, true);
                    Vector2 positionAtPoint = chainPoints[i];
                    Color color = Color.Lerp(Lighting.GetColor(positionAtPoint.ToTileCoordinates()), effectColor with { A = 0 }, completion);

                    if (Vector2.Distance(owner.Center, positionAtPoint) > 1400)
                        continue;
                    if (Vector2.Distance(positionAtPoint, NPC.Center) < 10f)
                        continue;
                    float angleAtPoint = i == chainPoints.Count - 1 ? (end - chainPoints[i]).ToRotation() : (chainPoints[i + 1] - chainPoints[i]).ToRotation();
                    angleAtPoint += MathHelper.PiOver2;

                    SpriteEffects sprfx = (i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                    if (i % 4 < 2)
                        sprfx |= SpriteEffects.FlipVertically;

                    if (released)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(2, 2) + NPC.velocity;
                        if (owner.Calamity().volatileGelatinVisuals)
                        {
                            Particle chainfx = new CustomSpark(positionAtPoint, vel, "CalamityMod/NPCs/Other/VolatileChain", Main.rand.NextBool(3), Main.rand.Next(30, 65), 1f, color, Vector2.One, false, false, -vel.ToRotation() + angleAtPoint, false, false, noShrink: false, spin: Main.rand.NextFloat(0, 0.08f) * Math.Sign(vel.X));
                            GeneralParticleHandler.SpawnParticle(chainfx);
                        }
                        
                        drawChains = false;
                    }
                    
                    Main.EntitySpriteDraw(chain, positionAtPoint - Main.screenPosition, null, color * (owner.Calamity().volatileGelatinVisuals ? 1 : 0.05f), angleAtPoint, chain.Size() / 2f, 1.1f - (0.3f * (1 - completion)), sprfx, 0);
                }
            }

            float sine = (float)Math.Sin(time * 0.2f / MathHelper.Pi);
            int draws = 12;
            for (int i = 0; i < draws; i++)
            {
                float rotPoint = MathHelper.TwoPi / draws * i;
                float dist = (2.5f + sine) * glowScale;
                if (owner.Calamity().volatileGelatinVisuals)
                    spriteBatch.Draw(texture, drawPosition + rotPoint.ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 0.2f) * dist, frame, effectColor with { A = 0 } * NPC.Opacity * 0.6f, NPC.rotation, origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
            spriteBatch.Draw(texture, drawPosition, frame, Color.Lerp(drawColor, effectColor with { A = 0 }, Utils.GetLerpValue(1, 3, glowScale)) * NPC.Opacity * (owner.Calamity().volatileGelatinVisuals ? 1f : 0.6f), NPC.rotation, origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
    }
}
