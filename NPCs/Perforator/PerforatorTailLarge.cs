using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.Perforator
{
    [HasPierceResist]
    [LongDistanceNetSync(SyncWith = typeof(PerforatorHeadLarge))]
    public class PerforatorTailLarge : ModNPC
    {
        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/NPCHit/PerfLargeHit", 3);
        public static readonly SoundStyle DeathSound = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");

        public static Asset<Texture2D> GlowTexture;

        public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.PerforatorHeadLarge.DisplayName");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.damage = 16; // 32
            NPC.width = 60;
            NPC.height = 78;
            NPC.defense = 12;

            NPC.LifeMaxNERB(2000, 2600, 80000);
            if (Main.zenithWorld)
                NPC.lifeMax *= 4;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.alpha = 255;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = HitSound;
            NPC.DeathSound = DeathSound;
            NPC.netAlways = true;
            NPC.dontCountMe = true;

            if (CalamityWorld.death || BossRushEvent.BossRushActive)
                NPC.scale *= 1.2f;
            else if (CalamityWorld.revenge)
                NPC.scale *= 1.15f;
            else if (Main.expertMode)
                NPC.scale *= 1.1f;

            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override void AI()
        {
            if (NPC.ai[2] > 0f)
                NPC.realLife = (int)NPC.ai[2];

            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest(true);

            bool shouldDespawn = !NPC.AnyNPCs(ModContent.NPCType<PerforatorHeadLarge>());
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

            if (Main.player[NPC.target].dead)
                NPC.TargetClosest(false);

            Vector2 segmentPosition = NPC.Center;
            float targetX = Main.player[NPC.target].Center.X;
            float targetY = Main.player[NPC.target].Center.Y;
            targetX = (float)((int)(targetX / 16f) * 16);
            targetY = (float)((int)(targetY / 16f) * 16);
            segmentPosition.X = (float)((int)(segmentPosition.X / 16f) * 16);
            segmentPosition.Y = (float)((int)(segmentPosition.Y / 16f) * 16);
            targetX -= segmentPosition.X;
            targetY -= segmentPosition.Y;
            float targetDistance = (float)System.Math.Sqrt((double)(targetX * targetX + targetY * targetY));
            if (NPC.ai[1] > 0f && NPC.ai[1] < (float)Main.npc.Length)
            {
                try
                {
                    segmentPosition = NPC.Center;
                    targetX = Main.npc[(int)NPC.ai[1]].Center.X - segmentPosition.X;
                    targetY = Main.npc[(int)NPC.ai[1]].Center.Y - segmentPosition.Y;
                }
                catch
                {
                }

                NPC.rotation = (float)System.Math.Atan2((double)targetY, (double)targetX) + MathHelper.PiOver2;
                targetDistance = (float)System.Math.Sqrt((double)(targetX * targetX + targetY * targetY));
                int npcWidth = NPC.width;
                targetDistance = (targetDistance - (float)npcWidth) / targetDistance;
                targetX *= targetDistance;
                targetY *= targetDistance;
                NPC.velocity = Vector2.Zero;
                NPC.position.X += targetX;
                NPC.position.Y += targetY;

                if (targetX < 0f)
                    NPC.spriteDirection = 1;
                else if (targetX > 0f)
                    NPC.spriteDirection = -1;
            }

            if (Main.npc[(int)NPC.ai[1]].alpha >= 85)
            {
                if (NPC.alpha > 0 && NPC.life > 0)
                {
                    for (int dustIndex = 0; dustIndex < 2; dustIndex++)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 0f, 0f, 100, default, 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                    }
                }

                if ((NPC.position - NPC.oldPosition).Length() > 2f)
                {
                    NPC.alpha -= 42;
                    if (NPC.alpha < 0)
                        NPC.alpha = 0;
                }
            }
            else if (NPC.alpha > 0)
            {
                NPC.alpha -= 42;
                if (NPC.alpha < 0)
                    NPC.alpha = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture2D15 = TextureAssets.Npc[Type].Value;
            Vector2 halfSizeTexture = new Vector2((float)(TextureAssets.Npc[Type].Value.Width / 2), (float)(TextureAssets.Npc[Type].Value.Height / 2));

            Vector2 drawLocation = NPC.Center - screenPos;
            drawLocation -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height)) * NPC.scale / 2f;
            drawLocation += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture2D15, drawLocation, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

            texture2D15 = GlowTexture.Value;
            Color glowmaskColor = Color.Lerp(Color.White, Color.Yellow, 0.5f);

            spriteBatch.Draw(texture2D15, drawLocation, NPC.frame, glowmaskColor, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

            return false;
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
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LargePerf4").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LargePerf5").Type, NPC.scale);
                }
            }
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<BurningBlood>(), 180);
        }
    }
}
