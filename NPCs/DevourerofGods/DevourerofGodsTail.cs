using System.IO;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.Projectiles.Melee.Yoyos;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.DevourerofGods
{
    [LongDistanceNetSync(SyncWith = typeof(DevourerofGodsHead))]
    public class DevourerofGodsTail : ModNPC
    {
        public static int phase1IconIndex;
        public static int phase2IconIndex;

        public static Asset<Texture2D> Texture_Glow_Purple;
        public static Asset<Texture2D> Texture_Glow_Cyan;
        public static Asset<Texture2D> TextureP2;
        public static Asset<Texture2D> TextureP2_Glow_Purple;
        public static Asset<Texture2D> TextureP2_Glow_Cyan;

        public override void Load()
        {
            string phase1IconPath = "CalamityMod/NPCs/DevourerofGods/DevourerofGodsTail_Head_Boss";
            string phase2IconPath = "CalamityMod/NPCs/DevourerofGods/DevourerofGodsTail_P2_Head_Boss";
            
            phase1IconIndex = CalamityMod.Instance.AddBossHeadTexture(phase1IconPath, -1);
            phase2IconIndex = CalamityMod.Instance.AddBossHeadTexture(phase2IconPath, -1);
        }

        private int invinceTime = 720;
        private bool setOpacity = false;
        private bool phase2Started = false;
        public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.DevourerofGodsHead.DisplayName");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            if (!Main.dedServ)
            {
                Texture_Glow_Purple = ModContent.Request<Texture2D>(Texture + "_Glow_Purple", AssetRequestMode.AsyncLoad);
                Texture_Glow_Cyan = ModContent.Request<Texture2D>(Texture + "_Glow_Cyan", AssetRequestMode.AsyncLoad);
                TextureP2 = ModContent.Request<Texture2D>(Texture + "_P2", AssetRequestMode.AsyncLoad);
                TextureP2_Glow_Purple = ModContent.Request<Texture2D>(Texture + "_P2_Glow_Purple", AssetRequestMode.AsyncLoad);
                TextureP2_Glow_Cyan = ModContent.Request<Texture2D>(Texture + "_P2_Glow_Cyan", AssetRequestMode.AsyncLoad);
            }
        }

        internal void setInvulTime(int time)
        {
            invinceTime = time;
        }

        public override void SetDefaults()
        {
            NPC.damage = 100; // 200
            NPC.npcSlots = 5f;
            NPC.width = 66;
            NPC.height = 66;
            NPC.defense = 50;
            NPC.LifeMaxNERB(760000, 910000, 1500000);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.Opacity = 0f;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.netAlways = true;
            NPC.boss = true;
            NPC.BossBar = Main.BigBossProgressBar.NeverValid;
            NPC.dontCountMe = true;

            if (Main.zenithWorld)
            {
                NPC.scale *= 1.5f;
                NPC.takenDamageMultiplier = 2;
            }
        }

        public override void BossHeadSlot(ref int index)
        {
            NPC head = CalamityGlobalNPC.DoGHead >= 0 ? Main.npc[CalamityGlobalNPC.DoGHead] : null;
            DevourerofGodsHead modNPC = head?.ModNPC<DevourerofGodsHead>() ?? null;

            index = -1;
            if (head is null || NPC.Opacity < 0.1f)
                return;

            if (!modNPC.Phase2Started)
            {
                index = phase1IconIndex;
                return;
            }

            if (!modNPC.AwaitingPhase2Teleport)
                index = phase2IconIndex;
        }

        public override void BossHeadRotation(ref float rotation)
        {
            NPC head = CalamityGlobalNPC.DoGHead >= 0 ? Main.npc[CalamityGlobalNPC.DoGHead] : null;
            DevourerofGodsHead modNPC = head?.ModNPC<DevourerofGodsHead>() ?? null;
            if (head is null || modNPC.AwaitingPhase2Teleport || !modNPC.Phase2Started)
                return;

            rotation = NPC.rotation;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(phase2Started);
            writer.Write(invinceTime);
            writer.Write(setOpacity);
            writer.Write(NPC.dontTakeDamage);
            writer.Write(NPC.Opacity);
            writer.Write(NPC.frame.X);
            writer.Write(NPC.frame.Y);
            writer.Write(NPC.frame.Width);
            writer.Write(NPC.frame.Height);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            phase2Started = reader.ReadBoolean();
            invinceTime = reader.ReadInt32();
            setOpacity = reader.ReadBoolean();
            NPC.dontTakeDamage = reader.ReadBoolean();
            NPC.Opacity = reader.ReadSingle();
            Rectangle frame = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            if (frame.Width > 0 && frame.Height > 0)
                NPC.frame = frame;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

        public override void AI()
        {
            if (NPC.ai[2] > 0f)
                NPC.realLife = (int)NPC.ai[2];

            NPC.life = Main.npc[(int)NPC.ai[2]].life;
            NPC.lifeMax = Main.npc[(int)NPC.ai[2]].lifeMax;

            // Percent life remaining
            float lifeRatio = Main.npc[(int)NPC.ai[2]].life / (float)Main.npc[(int)NPC.ai[2]].lifeMax;

            bool phase2 = lifeRatio < 0.65f;
            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            if (phase2 && !phase2Started && Main.npc[(int)NPC.ai[2]].localAI[2] <= 60)
            {
                phase2Started = true;
                    NPC.position = NPC.Center;
                    NPC.width = (int)(80 * NPC.scale);
                    NPC.height = (int)(80 * NPC.scale);
                    NPC.frame = new Rectangle(0, 0, 86, 148);
                    NPC.position -= NPC.Size * 0.5f;
                    NPC.ForceNetUpdate(false);
            }

            if (invinceTime > 0)
            {
                invinceTime--;
                NPC.dontTakeDamage = true;
            }
            else
                NPC.dontTakeDamage = Main.npc[(int)NPC.ai[2]].dontTakeDamage;

            if (Main.npc[(int)NPC.ai[2]].dontTakeDamage)
                invinceTime = 240;

            // Target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            // Check if other segments are still alive, if not, die
            bool shouldDespawn = !NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>());
            if (!shouldDespawn)
            {
                if (NPC.ai[1] <= 0f)
                    shouldDespawn = true;
                else if (Main.npc[(int)NPC.ai[1]].life <= 0)
                    shouldDespawn = true;
            }
            if (shouldDespawn)
            {
                NPC.life = 0;
                NPC.HitEffect(0, 10.0);
                NPC.checkDead();
                NPC.active = false;
            }

            if (Main.npc[(int)NPC.ai[1]].Opacity >= 0.5f && (!setOpacity || (Main.npc[(int)NPC.ai[2]].localAI[2] <= 60f && Main.npc[(int)NPC.ai[2]].localAI[2] > 0f)))
            {
                NPC.Opacity += 0.165f;
                if (NPC.Opacity >= 1f && invinceTime <= 0)
                {
                    setOpacity = true;
                    NPC.Opacity = 1f;
                }
            }
            else
            {
                if (Main.npc[(int)NPC.ai[2]].ModNPC<DevourerofGodsHead>()?.AttemptingToEnterPortal ?? false)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile portal = Main.projectile[Main.npc[(int)NPC.ai[2]].ModNPC<DevourerofGodsHead>().PortalIndex];
                        float newOpacity = 1f - Utils.GetLerpValue(270f, 100f, NPC.Distance(portal.Center), true);
                        if (newOpacity > 0f && NPC.Opacity > newOpacity)
                        {
                            NPC.Opacity = newOpacity;

                            // Create dust at the portal position.
                            if (Vector2.Dot((NPC.rotation - MathHelper.PiOver2).ToRotationVector2(), Main.npc[(int)NPC.ai[2]].velocity) > 0f)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    Dust cosmicMagic = Dust.NewDustPerfect(portal.Center, Main.rand.NextBool() ? 180 : 173);
                                    cosmicMagic.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 8f);
                                    cosmicMagic.scale *= Main.rand.NextFloat(1f, 1.8f);
                                    cosmicMagic.noGravity = true;
                                }
                            }

                            if (NPC.Opacity < 0.2f)
                                NPC.Opacity = 0f;

                            NPC.ForceNetUpdate(false);
                        }
                    }
                }
                else
                    NPC.Opacity = Main.npc[(int)NPC.ai[2]].Opacity;
            }

            // Copy the damage state of the head
            NPC.damage = Main.npc[(int)NPC.ai[2]].damage == 0 ? 0 : NPC.defDamage;

            Vector2 segmentDirection = NPC.Center;
            float playerXDist = player.position.X + (player.width / 2);
            float playerYDist = player.position.Y + (player.height / 2);
            playerXDist = (int)(playerXDist / 16f) * 16;
            playerYDist = (int)(playerYDist / 16f) * 16;
            segmentDirection.X = (int)(segmentDirection.X / 16f) * 16;
            segmentDirection.Y = (int)(segmentDirection.Y / 16f) * 16;
            playerXDist -= segmentDirection.X;
            playerYDist -= segmentDirection.Y;
            if (NPC.ai[1] > 0f && NPC.ai[1] < Main.npc.Length)
            {
                try
                {
                    segmentDirection = NPC.Center;
                    playerXDist = Main.npc[(int)NPC.ai[1]].position.X + (Main.npc[(int)NPC.ai[1]].width / 2) - segmentDirection.X;
                    playerYDist = Main.npc[(int)NPC.ai[1]].position.Y + (Main.npc[(int)NPC.ai[1]].height / 2) - segmentDirection.Y;
                }
                catch
                {
                }
                NPC.rotation = (float)System.Math.Atan2(playerYDist, playerXDist) + MathHelper.PiOver2;
                float playerDistance = (float)System.Math.Sqrt(playerXDist * playerXDist + playerYDist * playerYDist);
                int segmentWidth = NPC.width;
                playerDistance = (playerDistance - segmentWidth) / playerDistance;
                playerXDist *= playerDistance;
                playerYDist *= playerDistance;
                NPC.velocity = Vector2.Zero;
                NPC.position.X = NPC.position.X + playerXDist;
                NPC.position.Y = NPC.position.Y + playerYDist;

                if (playerXDist < 0f)
                    NPC.spriteDirection = -1;
                else if (playerXDist > 0f)
                    NPC.spriteDirection = 1;
            }

            // Velocity variables
            float segmentVelocity = death ? 17.5f : 16f;
            if (expertMode)
                segmentVelocity += 4f * (1f - lifeRatio);
            if (Main.getGoodWorld)
                segmentVelocity *= 1.1f;
        }

        Vector2 noiseOffset = Vector2.Zero;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.realLife < 0 || NPC.realLife >= Main.maxNPCs || Main.npc[NPC.realLife] is null)
                return true;
            if (Main.npc[NPC.realLife].type != ModContent.NPCType<DevourerofGodsHead>())
                return true;

            bool shouldUseShader = CalamityDrawParameterNPC.DoGDeathAnimationTimer != 0;
            SpriteBatchSnapshot snap = new(spriteBatch);

            if (shouldUseShader)
            {
                if (noiseOffset == Vector2.Zero)
                    noiseOffset = NPC.Center;

                Main.spriteBatch.End(out snap);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                MiscShaderData dissolveShader = GameShaders.Misc["CalamityMod:Dissolve"];
                Texture2D dissolveTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise").Value;

                dissolveShader.Shader.Parameters["noiseScale"].SetValue(1f);
                dissolveShader.Shader.Parameters["dissolveIntensity"].SetValue(CalamityDrawParameterNPC.DoGDeathAnimationTimer / 600f);
                dissolveShader.Shader.Parameters["sampleOffset"].SetValue(noiseOffset * 0.5f);
                dissolveShader.Shader.Parameters["transitionColor"].SetValue(DevourerofGodsHead.SpecialMoveColor.ToVector4());
                dissolveShader.Shader.Parameters["transitionOffset"].SetValue(0.05f);

                Main.instance.GraphicsDevice.Textures[1] = dissolveTexture;
                Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

                dissolveShader.Apply();
            }

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            bool useOtherTextures = phase2Started && Main.npc[(int)NPC.ai[2]].localAI[2] <= 60f;
            Texture2D texture2D15 = useOtherTextures ? TextureP2.Value : TextureAssets.Npc[Type].Value;
            Vector2 halfSizeTexture = new Vector2(texture2D15.Width / 2, texture2D15.Height / 2);

            Vector2 drawPosition = NPC.Center - screenPos;
            drawPosition -= new Vector2(texture2D15.Width, texture2D15.Height) * NPC.scale / 2f;
            drawPosition += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture2D15, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

            if ((!Main.npc[(int)NPC.ai[2]].ModNPC<DevourerofGodsHead>().isInPassiveState || !useOtherTextures) && NPC.Opacity > 0.25f)
            {
                texture2D15 = useOtherTextures ? TextureP2_Glow_Purple.Value : Texture_Glow_Purple.Value;
                Color glowmaskColor = Color.Lerp(Color.White, Color.Fuchsia, 0.5f);

                spriteBatch.Draw(texture2D15, drawPosition, NPC.frame, glowmaskColor, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            }
            if (!Main.npc[(int)NPC.ai[2]].ModNPC<DevourerofGodsHead>().isInAgressiveState && NPC.Opacity > 0.25f)
            {
                texture2D15 = useOtherTextures ? TextureP2_Glow_Cyan.Value : Texture_Glow_Cyan.Value;
                Color glowmaskColor = Color.Lerp(Color.White, Color.Cyan, 0.5f);

                spriteBatch.Draw(texture2D15, drawPosition, NPC.frame, glowmaskColor, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            }

            if (shouldUseShader)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(snap);
            }

            return false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.BossNoCheese;

            Rectangle targetHitbox = target.Hitbox;

            float hitboxTopLeft = Vector2.Distance(NPC.Center, targetHitbox.TopLeft());
            float hitboxTopRight = Vector2.Distance(NPC.Center, targetHitbox.TopRight());
            float hitboxBotLeft = Vector2.Distance(NPC.Center, targetHitbox.BottomLeft());
            float hitboxBotRight = Vector2.Distance(NPC.Center, targetHitbox.BottomRight());

            float minDist = hitboxTopLeft;
            if (hitboxTopRight < minDist)
                minDist = hitboxTopRight;
            if (hitboxBotLeft < minDist)
                minDist = hitboxBotLeft;
            if (hitboxBotRight < minDist)
                minDist = hitboxBotRight;

            return minDist <= (phase2Started ? 70f : 35f) * NPC.scale && NPC.Opacity >= 1f && invinceTime <= 0;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.soundDelay == 0)
            {
                NPC.soundDelay = 8;
                float extrapitch = Main.zenithWorld ? 0.3f : 0f;
                SoundEngine.PlaySound(DevourerofGodsHead.HitSound with { Pitch = DevourerofGodsHead.HitSound.Pitch + extrapitch }, NPC.Center);
            }
            if (NPC.life <= 0)
            {
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("DoGS3").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("DoGS4").Type, NPC.scale);
                }
                NPC.position.X = NPC.position.X + (NPC.width / 2);
                NPC.position.Y = NPC.position.Y + (NPC.height / 2);
                NPC.width = (int)(100 * NPC.scale);
                NPC.height = (int)(100 * NPC.scale);
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                for (int i = 0; i < 10; i++)
                {
                    int cosmiliteDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.PurpleCosmilite, 0f, 0f, 100, default, 2f);
                    Main.dust[cosmiliteDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[cosmiliteDust].scale = 0.5f;
                        Main.dust[cosmiliteDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int j = 0; j < 20; j++)
                {
                    int cosmiliteDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.PurpleCosmilite, 0f, 0f, 100, default, 3f);
                    Main.dust[cosmiliteDust2].noGravity = true;
                    Main.dust[cosmiliteDust2].velocity *= 5f;
                    cosmiliteDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.PurpleCosmilite, 0f, 0f, 100, default, 2f);
                    Main.dust[cosmiliteDust2].velocity *= 2f;
                }
            }
        }

        // This will always put the boss to 1 health before dying, which makes external checks work.
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) => modifiers.SetMaxDamage(NPC.life - 1);

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // viable???, done here since it's conditional
            if (Main.zenithWorld && projectile.type == ModContent.ProjectileType<LaceratorYoyo>())
                modifiers.SourceDamage *= 40f;
        }

        public override bool CheckActive() => false;

        public override bool CheckDead()
        {
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            NPC.ForceNetUpdate(false);

            if (NPC.realLife >= 0)
            {
                NPC Head = Main.npc[NPC.realLife];
                if (Head.type != ModContent.NPCType<DevourerofGodsHead>())
                    return false;

                Head.ModNPC<DevourerofGodsHead>().Dying = true;
                Head.dontTakeDamage = true;
                Head.ForceNetUpdate(false);
            }
            return false;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        }
    }
}
