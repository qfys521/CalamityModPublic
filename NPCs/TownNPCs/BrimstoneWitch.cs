using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.Items.Tools;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Systems.Collections;
using CalamityMod.UI.CalamitasEnchants;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;
using SCalBoss = CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;

namespace CalamityMod.NPCs.TownNPCs
{
    [AutoloadHead]
    [LegacyName("WITCH")]
    public class BrimstoneWitch : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 27;
            NPCID.Sets.ExtraFramesCount[Type] = 11;
            NPCID.Sets.AttackFrameCount[Type] = 6;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 5;
            NPCID.Sets.ShimmerTownTransform[Type] = false;
            NPC.Happiness
                .SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
                .SetBiomeAffection<BrimstoneCragsBiome>(AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.Clothier, AffectionLevel.Like)
                .SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Dislike);
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.lavaImmune = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.damage = 10;
            NPC.gfxOffY = -2f;

            // You should not be able to kill SCal under any typical circumstances.
            NPC.lifeMax = 960000;

            NPC.defense = 120;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.8f;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.BrimstoneWitch")
            });
        }

        public override bool CanTownNPCSpawn(int numTownNPCs) => DownedBossSystem.downedCalamitas && !NPC.AnyNPCs(NPCType<SCalBoss>());

        public override List<string> SetNPCNameList() => new List<string>() { this.GetLocalizedValue("Name.Calamitas") };

        public override void FindFrame(int frameHeight)
        {
            int extraFrameAmt = (NPC.isLikeATownNPC ? NPCID.Sets.ExtraFramesCount[Type] : 0);
            if (NPC.velocity.Y == 0f)
            {
                if (NPC.direction == 1)
                    NPC.spriteDirection = 1;

                if (NPC.direction == -1)
                    NPC.spriteDirection = -1;

                int nonAttackFrames = Main.npcFrameCount[Type] - NPCID.Sets.AttackFrameCount[Type];
                if (NPC.ai[0] == 23f)
                {
                    NPC.frameCounter += 1D;
                    int currentFrameHeight = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - currentFrameHeight;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && currentFrameHeight != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int num239 = ((!(NPC.frameCounter < 6D)) ? (nonAttackFrames - 4) : (nonAttackFrames - 5));
                    if (NPC.ai[1] < 6f)
                        num239 = nonAttackFrames - 5;

                    NPC.frame.Y = frameHeight * num239;
                }
                else if (NPC.ai[0] >= 20f && NPC.ai[0] <= 22f)
                {
                    int num240 = NPC.frame.Y / frameHeight;
                    switch ((int)NPC.ai[0])
                    {
                        case 20:
                        case 21:
                        case 22:
                            break;
                    }

                    NPC.frame.Y = num240 * frameHeight;
                }
                else if (NPC.ai[0] == 2f)
                {
                    NPC.frameCounter += 1D;
                    if (NPC.frame.Y / frameHeight == nonAttackFrames - 1 && NPC.frameCounter >= 5D)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }
                    else if (NPC.frame.Y / frameHeight == 0 && NPC.frameCounter >= 40D)
                    {
                        NPC.frame.Y = frameHeight * (nonAttackFrames - 1);
                        NPC.frameCounter = 0D;
                    }
                    else if (NPC.frame.Y != 0 && NPC.frame.Y != frameHeight * (nonAttackFrames - 1))
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }
                }
                else if (NPC.ai[0] == 5f) // Sitting
                {
                    NPC.frame.Y = frameHeight * (nonAttackFrames - 3);
                    NPC.frameCounter = 0D;
                }
                else if (NPC.ai[0] == 6f) // Throwing confetti
                {
                    NPC.frameCounter += 1D;
                    int confettiFrameHeight = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - confettiFrameHeight;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && confettiFrameHeight != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int confettiFrame = ((!(NPC.frameCounter < 10D)) ?
                        ((NPC.frameCounter < 16D) ?
                        (nonAttackFrames - 5) : ((NPC.frameCounter < 46D) ?
                        (nonAttackFrames - 4) : ((NPC.frameCounter < 60D) ?
                        (nonAttackFrames - 5) : ((!(NPC.frameCounter < 66D)) ?
                        ((NPC.frameCounter < 72D) ?
                        (nonAttackFrames - 5) : ((NPC.frameCounter < 102D) ?
                        (nonAttackFrames - 4) : ((NPC.frameCounter < 108D) ?
                        (nonAttackFrames - 5) : ((!(NPC.frameCounter < 114D)) ?
                        ((NPC.frameCounter < 120D) ?
                        (nonAttackFrames - 5) : ((NPC.frameCounter < 150D) ?
                        (nonAttackFrames - 4) : ((NPC.frameCounter < 156D) ?
                        (nonAttackFrames - 5) : ((!(NPC.frameCounter < 162D)) ?
                        ((NPC.frameCounter < 168D) ?
                        (nonAttackFrames - 5) : ((NPC.frameCounter < 198D) ?
                        (nonAttackFrames - 4) : ((NPC.frameCounter < 204D) ?
                        (nonAttackFrames - 5) : ((!(NPC.frameCounter < 210D)) ?
                        ((NPC.frameCounter < 216D) ?
                        (nonAttackFrames - 5) : ((NPC.frameCounter < 246D) ?
                        (nonAttackFrames - 4) : ((NPC.frameCounter < 252D) ?
                        (nonAttackFrames - 5) : ((!(NPC.frameCounter < 258D)) ?
                        ((NPC.frameCounter < 264D) ?
                        (nonAttackFrames - 5) : ((NPC.frameCounter < 294D) ?
                        (nonAttackFrames - 4) : ((NPC.frameCounter < 300D) ?
                        (nonAttackFrames - 5) : 0))) : 0)))) : 0)))) : 0)))) : 0)))) : 0)))) : 0);

                    if (confettiFrame == nonAttackFrames - 4 && confettiFrameHeight == nonAttackFrames - 5)
                    {
                        Vector2 vector4 = NPC.Center + new Vector2(10 * NPC.direction, -4f);
                        for (int n = 0; n < 8; n++)
                        {
                            int confettiDust = Main.rand.Next(139, 143);
                            int partyTime = Dust.NewDust(vector4, 0, 0, confettiDust, NPC.velocity.X + (float)NPC.direction, NPC.velocity.Y - 2.5f, 0, default(Color), 1.2f);
                            Main.dust[partyTime].velocity.X += (float)NPC.direction * 1.5f;
                            Dust dust = Main.dust[partyTime];
                            dust.position -= new Vector2(4f);
                            dust = Main.dust[partyTime];
                            dust.velocity *= 2f;
                            Main.dust[partyTime].scale = 0.7f + Main.rand.NextFloat() * 0.3f;
                        }
                    }

                    NPC.frame.Y = frameHeight * confettiFrame;
                    if (NPC.frameCounter >= 300D)
                        NPC.frameCounter = 0D;
                }
                else if (NPC.ai[0] == 7f || NPC.ai[0] == 19f) // Talking to the player
                {
                    NPC.frameCounter += 1D;
                    int playerTalkFrameHeight = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - playerTalkFrameHeight;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && playerTalkFrameHeight != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int playerTalkFrame = 0;
                    if (NPC.frameCounter < 16D)
                        playerTalkFrame = 0;
                    else if (NPC.frameCounter == 16D)
                        EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), 112);
                    else if (NPC.frameCounter < 128D)
                        playerTalkFrame = ((NPC.frameCounter % 16D < 8D) ? (nonAttackFrames - 2) : 0);
                    else if (NPC.frameCounter < 160D)
                        playerTalkFrame = 0;
                    else if (NPC.frameCounter != 160D)
                        playerTalkFrame = ((NPC.frameCounter < 220D) ? ((NPC.frameCounter % 12D < 6D) ? (nonAttackFrames - 2) : 0) : 0);
                    else
                        EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), 60);

                    NPC.frame.Y = frameHeight * playerTalkFrame;
                    if (NPC.frameCounter >= 220D)
                        NPC.frameCounter = 0D;
                }
                else if (NPC.ai[0] == 9f)
                {
                    NPC.frameCounter += 1D;
                    int num251 = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - num251;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && num251 != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int num252 = ((!(NPC.frameCounter < 10D)) ? ((!(NPC.frameCounter < 16D)) ? (nonAttackFrames - 4) : (nonAttackFrames - 5)) : 0);
                    if (NPC.ai[1] < 16f)
                        num252 = nonAttackFrames - 5;

                    if (NPC.ai[1] < 10f)
                        num252 = 0;

                    NPC.frame.Y = frameHeight * num252;
                }
                else if (NPC.ai[0] == 18f)
                {
                    NPC.frameCounter += 1D;
                    int num253 = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - num253;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && num253 != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int num254 = 0;
                    if (NPC.frameCounter < 10D)
                        num254 = 0;
                    else if (NPC.frameCounter < 16D)
                        num254 = nonAttackFrames - 1;
                    else
                        num254 = nonAttackFrames - 2;

                    if (NPC.ai[1] < 16f)
                        num254 = nonAttackFrames - 1;

                    if (NPC.ai[1] < 10f)
                        num254 = 0;

                    num254 = Main.npcFrameCount[Type] - 2;
                    NPC.frame.Y = frameHeight * num254;
                }
                else if (NPC.ai[0] == 10f || NPC.ai[0] == 13f) // Attacking
                {
                    NPC.frameCounter += 1D;
                    int attackFrameHeight = NPC.frame.Y / frameHeight;
                    int currentFrame = attackFrameHeight - nonAttackFrames;
                    if ((uint)currentFrame > 3u && attackFrameHeight != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int attackTimingStart = 10;
                    int attackFrameTiming = 6;
                    int attackFrame = ((!(NPC.frameCounter < (double)attackTimingStart)) ?
                        ((NPC.frameCounter < (double)(attackTimingStart + attackFrameTiming)) ?
                        nonAttackFrames : ((NPC.frameCounter < (double)(attackTimingStart + attackFrameTiming * 2)) ?
                        (nonAttackFrames + 1) : ((NPC.frameCounter < (double)(attackTimingStart + attackFrameTiming * 3)) ?
                        (nonAttackFrames + 2) : ((NPC.frameCounter < (double)(attackTimingStart + attackFrameTiming * 4)) ?
                        (nonAttackFrames + 3) : 0)))) : 0);

                    NPC.frame.Y = frameHeight * attackFrame;
                }
                else if (NPC.ai[0] == 15f)
                {
                    NPC.frameCounter += 1D;
                    int num259 = NPC.frame.Y / frameHeight;
                    int currentFrame = num259 - nonAttackFrames;
                    if ((uint)currentFrame > 3u && num259 != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    float num260 = NPC.ai[1] / (float)NPCID.Sets.AttackTime[Type];
                    int num261 = 0;
                    num261 = ((num260 > 0.65f) ?
                        nonAttackFrames : ((num260 > 0.5f) ?
                        (nonAttackFrames + 1) : ((num260 > 0.35f) ?
                        (nonAttackFrames + 2) : ((num260 > 0f) ?
                        (nonAttackFrames + 3) : 0))));

                    NPC.frame.Y = frameHeight * num261;
                }
                else if (NPC.ai[0] == 25f)
                {
                    NPC.frame.Y = frameHeight;
                }
                else if (NPC.ai[0] == 12f)
                {
                    NPC.frameCounter += 1D;
                    int num262 = NPC.frame.Y / frameHeight;
                    int currentFrame = num262 - nonAttackFrames;
                    if ((uint)currentFrame > 4u && num262 != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int num263 = nonAttackFrames + NPC.GetShootingFrame(NPC.ai[2]);
                    NPC.frame.Y = frameHeight * num263;
                }
                else if (NPC.ai[0] == 14f || NPC.ai[0] == 24f)
                {
                    NPC.frameCounter += 1D;
                    int num264 = NPC.frame.Y / frameHeight;
                    int currentFrame = num264 - nonAttackFrames;
                    if ((uint)currentFrame > 1u && num264 != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    int num265 = 12;
                    int num266 = ((NPC.frameCounter % (double)num265 * 2D < (double)num265) ? nonAttackFrames : (nonAttackFrames + 1));
                    NPC.frame.Y = frameHeight * num266;
                    if (NPC.ai[0] == 24f)
                    {
                        if (NPC.frameCounter == 60D)
                            EmoteBubble.NewBubble(EmoteID.EmoteConfused, new WorldUIAnchor(NPC), 60);

                        if (NPC.frameCounter == 150D)
                            EmoteBubble.NewBubble(EmoteID.EmotionAlert, new WorldUIAnchor(NPC), 90);

                        if (NPC.frameCounter >= 240D)
                            NPC.frame.Y = 0;
                    }
                }
                else if (NPC.ai[0] == 1001f)
                {
                    NPC.frame.Y = frameHeight * (nonAttackFrames - 1);
                    NPC.frameCounter = 0D;
                }
                else if (NPC.CanTalk && (NPC.ai[0] == 3f || NPC.ai[0] == 4f)) // Talking to another NPC
                {
                    NPC.frameCounter += 1D;
                    int npcTalkFrameHeight = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - npcTalkFrameHeight;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && npcTalkFrameHeight != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    bool displayEmote = NPC.ai[0] == 3f;
                    int npcTalkFrame = 0;
                    int npcTalkHandFrame = 0;
                    int emoteDisplayTime = -1;
                    int emoteDisplayTime2 = -1;
                    if (NPC.frameCounter < 10D)
                        npcTalkFrame = 0;
                    else if (NPC.frameCounter < 16D)
                        npcTalkFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 46D)
                        npcTalkFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 60D)
                        npcTalkFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 216D)
                        npcTalkFrame = 0;
                    else if (NPC.frameCounter == 216D && Main.netMode != NetmodeID.MultiplayerClient)
                        emoteDisplayTime = 70;
                    else if (NPC.frameCounter < 286D)
                        npcTalkFrame = ((NPC.frameCounter % 12D < 6D) ? (nonAttackFrames - 2) : 0);
                    else if (NPC.frameCounter < 320D)
                        npcTalkFrame = 0;
                    else if (NPC.frameCounter != 320D || Main.netMode == NetmodeID.MultiplayerClient)
                        npcTalkFrame = ((NPC.frameCounter < 420D) ? ((NPC.frameCounter % 16D < 8D) ? (nonAttackFrames - 2) : 0) : 0);
                    else
                        emoteDisplayTime = 100;

                    if (NPC.frameCounter < 70D)
                    {
                        npcTalkHandFrame = 0;
                    }
                    else if (NPC.frameCounter != 70D || Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        npcTalkHandFrame = ((NPC.frameCounter < 160D) ?
                            ((NPC.frameCounter % 16D < 8D) ?
                            (nonAttackFrames - 2) : 0) : ((NPC.frameCounter < 166D) ?
                            (nonAttackFrames - 5) : ((NPC.frameCounter < 186D) ?
                            (nonAttackFrames - 4) : ((NPC.frameCounter < 200D) ?
                            (nonAttackFrames - 5) : ((!(NPC.frameCounter < 320D)) ?
                            ((NPC.frameCounter < 326D) ?
                            (nonAttackFrames - 1) : 0) : 0)))));
                    }
                    else
                        emoteDisplayTime2 = 90;

                    if (displayEmote)
                    {
                        NPC nPC = Main.npc[(int)NPC.ai[2]];
                        if (emoteDisplayTime != -1)
                            EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), emoteDisplayTime, new WorldUIAnchor(nPC));

                        if (emoteDisplayTime2 != -1 && nPC.CanTalk)
                            EmoteBubble.NewBubbleNPC(new WorldUIAnchor(nPC), emoteDisplayTime2, new WorldUIAnchor(NPC));
                    }

                    NPC.frame.Y = frameHeight * (displayEmote ? npcTalkFrame : npcTalkHandFrame);
                    if (NPC.frameCounter >= 420D)
                        NPC.frameCounter = 0D;
                }
                else if (NPC.CanTalk && (NPC.ai[0] == 16f || NPC.ai[0] == 17f)) // Rock Paper Scissors
                {
                    NPC.frameCounter += 1D;
                    int rpsFrameHeight = NPC.frame.Y / frameHeight;
                    int currentFrame = nonAttackFrames - rpsFrameHeight;
                    if ((uint)(currentFrame - 1) > 1u && (uint)(currentFrame - 4) > 1u && rpsFrameHeight != 0)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0D;
                    }

                    bool controlsRPS = NPC.ai[0] == 16f;
                    int rpsFrame = 0;
                    int emoteDisplayTime = -1;
                    if (NPC.frameCounter < 10D)
                        rpsFrame = 0;
                    else if (NPC.frameCounter < 16D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 22D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 28D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 34D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 40D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter == 40D && Main.netMode != NetmodeID.MultiplayerClient)
                        emoteDisplayTime = 45;
                    else if (NPC.frameCounter < 70D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 76D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 82D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 88D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 94D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 100D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter == 100D && Main.netMode != NetmodeID.MultiplayerClient)
                        emoteDisplayTime = 45;
                    else if (NPC.frameCounter < 130D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 136D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 142D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 148D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter < 154D)
                        rpsFrame = nonAttackFrames - 4;
                    else if (NPC.frameCounter < 160D)
                        rpsFrame = nonAttackFrames - 5;
                    else if (NPC.frameCounter != 160D || Main.netMode == NetmodeID.MultiplayerClient)
                        rpsFrame = ((NPC.frameCounter < 220D) ? (nonAttackFrames - 4) : ((NPC.frameCounter < 226D) ? (nonAttackFrames - 5) : 0));
                    else
                        emoteDisplayTime = 75;

                    if (controlsRPS && emoteDisplayTime != -1)
                    {
                        int npcPick = (int)NPC.localAI[2];
                        int npcWins = (int)NPC.localAI[3];
                        int opponentWins = (int)Main.npc[(int)NPC.ai[2]].localAI[3];
                        int opponentPick = (int)Main.npc[(int)NPC.ai[2]].localAI[2];
                        int rpsGameEnder = 3 - npcPick - npcWins;
                        int numGamesPlayed = 0;
                        if (NPC.frameCounter == 40D)
                            numGamesPlayed = 1;

                        if (NPC.frameCounter == 100D)
                            numGamesPlayed = 2;

                        if (NPC.frameCounter == 160D)
                            numGamesPlayed = 3;

                        int gameCountdown = 3 - numGamesPlayed;
                        int rockPaperScissorsResultType = -1;
                        int gameFrameTimer = 0;
                        while (rockPaperScissorsResultType < 0)
                        {
                            currentFrame = gameFrameTimer + 1;
                            gameFrameTimer = currentFrame;
                            if (currentFrame >= 100)
                                break;

                            rockPaperScissorsResultType = Main.rand.Next(2);
                            if (rockPaperScissorsResultType == 0 && opponentPick >= npcWins)
                                rockPaperScissorsResultType = -1;

                            if (rockPaperScissorsResultType == 1 && opponentWins >= npcPick)
                                rockPaperScissorsResultType = -1;

                            if (rockPaperScissorsResultType == -1 && gameCountdown <= rpsGameEnder)
                                rockPaperScissorsResultType = 2;
                        }

                        if (rockPaperScissorsResultType == 0)
                        {
                            Main.npc[(int)NPC.ai[2]].localAI[3] += 1f;
                            opponentWins++;
                        }

                        if (rockPaperScissorsResultType == 1)
                        {
                            Main.npc[(int)NPC.ai[2]].localAI[2] += 1f;
                            opponentPick++;
                        }

                        int emoteType = Utils.SelectRandom<int>(Main.rand, EmoteID.RPSPaper, EmoteID.RPSRock, EmoteID.RPSScissors);
                        int emoteType2 = emoteType;
                        switch (rockPaperScissorsResultType)
                        {
                            case 0:
                                switch (emoteType)
                                {
                                    case EmoteID.RPSPaper:
                                        emoteType2 = EmoteID.RPSRock;
                                        break;
                                    case EmoteID.RPSRock:
                                        emoteType2 = EmoteID.RPSScissors;
                                        break;
                                    case EmoteID.RPSScissors:
                                        emoteType2 = EmoteID.RPSPaper;
                                        break;
                                }
                                break;
                            case 1:
                                switch (emoteType)
                                {
                                    case EmoteID.RPSPaper:
                                        emoteType2 = EmoteID.RPSScissors;
                                        break;
                                    case EmoteID.RPSRock:
                                        emoteType2 = EmoteID.RPSPaper;
                                        break;
                                    case EmoteID.RPSScissors:
                                        emoteType2 = EmoteID.RPSRock;
                                        break;
                                }
                                break;
                        }

                        if (gameCountdown == 0)
                        {
                            if (opponentWins >= 2)
                                emoteType -= 3;

                            if (opponentPick >= 2)
                                emoteType2 -= 3;
                        }

                        EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(NPC), emoteDisplayTime);
                        EmoteBubble.NewBubble(emoteType2, new WorldUIAnchor(Main.npc[(int)NPC.ai[2]]), emoteDisplayTime);
                    }

                    NPC.frame.Y = frameHeight * (controlsRPS ? rpsFrame : rpsFrame);
                    if (NPC.frameCounter >= 420D)
                        NPC.frameCounter = 0D;
                }
                else if (NPC.velocity.X == 0f)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0D;
                }
                else // Walking
                {
                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 2f;
                    NPC.frameCounter += 1D;

                    int walkFrameHeightLimit = frameHeight * 2;
                    if (NPC.frame.Y < walkFrameHeightLimit)
                        NPC.frame.Y = walkFrameHeightLimit;

                    int walkFrameTimer = 6;
                    if (NPC.frameCounter > (double)walkFrameTimer)
                    {
                        NPC.frame.Y += frameHeight;
                        NPC.frameCounter = 0D;
                    }

                    if (NPC.frame.Y / frameHeight >= Main.npcFrameCount[Type] - extraFrameAmt)
                        NPC.frame.Y = walkFrameHeightLimit;
                }

                return;
            }

            NPC.frameCounter = 0D;
            NPC.frame.Y = frameHeight;
        }

        // The way this works is by having an RNG based on weights.
        // With certain conditions (such as if a blood moon is happening) you can add possibilities
        // to the RNG via dialogue.Add("text", weight);
        // Text can always appear assuming the weight is greater than 0 and there's no if condition deciding whether it can.
        // The higher the weight is, the more likely it is to be selected from all the choices.
        // To give an example of this, assume you have two possibilities:
        // "a" with a weight of 1, and "b" with a weight of 5. The chance of "a" being displayed would be
        // 1/6, while "b" wold have a 5/6 chance of being displayed.
        // If only one possibility exists it will be the only thing that is displayed, regardless of weight.
        public override string GetChat()
        {
            WeightedRandom<string> dialogue = new WeightedRandom<string>();

            // Have a flat chance (1/4444) to simply ignore the below selection and say something humorous instead.
            if (Main.rand.NextBool(4444))
                return this.GetLocalizedValue("Chat.EasterEgg");

            if (NPC.homeless)
                return this.GetLocalizedValue("Chat.Homeless" + Main.rand.Next(1, 2 + 1));

            dialogue.Add(this.GetLocalizedValue("Chat.Normal1"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal2"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal3"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal4"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal5"));

            if (!Main.dayTime)
            {
                if (Main.bloodMoon)
                {
                    dialogue.Add(this.GetLocalizedValue("Chat.BloodMoon1"), 5.15);
                    dialogue.Add(this.GetLocalizedValue("Chat.BloodMoon2"), 5.15);
                }
                else
                {
                    dialogue.Add(this.GetLocalizedValue("Chat.Night1"), 2.8);
                    dialogue.Add(this.GetLocalizedValue("Chat.Night2"), 2.8);
                }
            }

            if (NPC.AnyNPCs(NPCType<SeaKing>()))
                dialogue.Add(this.GetLocalizedValue("Chat.SeaKing"), 1.45);

            if (BirthdayParty.PartyIsUp)
                dialogue.Add(this.GetLocalizedValue("Chat.Party"), 5.5);

            return dialogue;
        }

        public override void RegisterChatButtons(NPCInteractionList interactions)
        {
            NPCInteractionList.Entry enchant = interactions.InsertBefore(new EnchantInteraction(), NPCInteractionDatabase.CloseButton);
            interactions.InsertAfter(new DonorInteraction(), enchant);
        }

        private sealed class EnchantInteraction : NPCInteraction
        {
            public override bool Condition() => true;
            public override string GetText() => Language.GetTextValue("Mods.CalamityMod.NPCs.BrimstoneWitch.EnchantButton");

            public override void Interact()
            {
                Main.playerInventory = true;
                CalamitasEnchantUI.NPCIndex = TalkNPC.whoAmI;
                CalamitasEnchantUI.CurrentlyViewing = true;

                if (!LocalPlayer.Calamity().GivenBrimstoneLocus)
                {
                    Item.NewItem(TalkNPC.GetSource_Loot(), TalkNPC.Hitbox, ItemType<BrimstoneLocus>());
                    LocalPlayer.Calamity().GivenBrimstoneLocus = true;
                }
            }
        }

        private sealed class DonorInteraction : NPCInteraction
        {
            public override bool Condition() => true;
            public override string GetText() => Language.GetTextValue("Mods.CalamityMod.NPCs.BrimstoneWitch.DonorButton");

            public override void Interact()
            {
                if (TalkNPC.ModNPC is BrimstoneWitch brimstoneWitch)
                    Main.npcChatText = brimstoneWitch.GetRandomDonors(25);
            }
        }

        /// <summary>
        /// Returns 25 random donator usernames.
        /// </summary>
        public string GetRandomDonors(int numDonors)
        {
            IList<string> pickingList = [..DonatorsNameList.List];

            string[] pickedDonors = new string[numDonors];
            for (int i = 0; i < numDonors; ++i)
            {
                int idxSelected = Main.rand.Next(pickingList.Count);
                pickedDonors[i] = pickingList[idxSelected];
                pickingList.RemoveAt(idxSelected);
            }

            string text = this.GetLocalization("DonorShoutout").Format(pickedDonors);
            return text;
        }

        // Make this Town NPC teleport to the Queen statue when triggered.
        public override bool CanGoToStatue(bool toKingStatue) => !toKingStatue;

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 300;
            knockback = 10f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 15;
        }

        public override bool PreAI()
        {
            // Disappear if the SCal boss is active. She's supposed to be the boss.
            // However, this doesn't happen in Boss Rush; the SCal there is a silent puppet created by Xeroc, not SCal herself.
            if (NPC.AnyNPCs(NPCType<SCalBoss>()) && !BossRushEvent.BossRushActive)
            {
                NPC.active = false;
                NPC.netUpdate = true;
                return false;
            }
            return true;
        }

        //public override void TownNPCAttackMagic(ref float auraLightMultiplier)

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileType<SeethingDischargeBrimstoneHellblast>();
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 2f;
        }

        // Explode into red dust on death.
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                NPC.position = NPC.Center;
                NPC.width = NPC.height = 50;
                NPC.position.X -= NPC.width / 2;
                NPC.position.Y -= NPC.height / 2;
                for (int i = 0; i < 5; i++)
                {
                    int brimstone = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                    Main.dust[brimstone].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[brimstone].scale = 0.5f;
                        Main.dust[brimstone].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    int fire = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                    Main.dust[fire].noGravity = true;
                    Main.dust[fire].velocity *= 5f;

                    fire = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                    Main.dust[fire].velocity *= 2f;
                }
            }
        }
    }
}
