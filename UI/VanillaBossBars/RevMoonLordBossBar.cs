using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.UI.VanillaBossBars
{
    internal class RevMoonLordBossBar : ModBossBar
    {
        private List<int> MoonLordPartIDs = [NPCID.MoonLordCore, NPCID.MoonLordHead, NPCID.MoonLordHand];
        private NPC _dummy;

        public override void SetStaticDefaults()
        {
            _dummy = new NPC();
        }
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.MoonLordHead]];

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC target = Main.npc[info.npcIndexToAimAt];
            if ((!target.active || InBadAI(target)) && !TryFindingAnotherMoonLordPiece(ref info))
                return false;

            life = 0f;
            lifeMax = 0f;
            shield = 0f;
            shieldMax = 0f;

            NPCSpawnParams dummy = target.GetMatchingSpawnParams();
            _dummy.SetDefaults(NPCID.MoonLordCore, dummy);
            lifeMax += _dummy.lifeMax;
            _dummy.SetDefaults(NPCID.MoonLordHead, dummy);
            lifeMax += _dummy.lifeMax;
            _dummy.SetDefaults(NPCID.MoonLordHand, dummy);
            lifeMax += _dummy.lifeMax * 2;

            // Find all the Moon Lord parts, and add their totals. Do not add them if in a "bad AI" state.
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (MoonLordPartIDs.Contains(n.type) && !InBadAI(n))
                {
                    life += n.life;
                }
            }

            return true;
        }

        private static bool InBadAI(NPC n)
        {
            // If in death animation, or in initial startup state
            if (n.type == NPCID.MoonLordCore && (n.ai[0] == 2f || n.ai[0] == -1f || n.localAI[3] == 0f))
                return true;
            // If in death animation, or this eye has been "killed"
            if (n.ai[0] == -2f || n.ai[0] == -3f || n.Calamity().newAI[0] == 1f)
                return true;
            return false;
        }

        private bool TryFindingAnotherMoonLordPiece(ref BigProgressBarInfo info)
        {
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (MoonLordPartIDs.Contains(n.type) && !InBadAI(n))
                {
                    info.npcIndexToAimAt = n.whoAmI;
                    return true;
                }
            }
            return false;
        }
    }
}
