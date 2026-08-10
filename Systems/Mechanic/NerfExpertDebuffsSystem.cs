using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class NerfExpertDebuffsSystem : ModSystem
    {
        public override void Load()
        {
            On_Player.AddBuff_DetermineBuffTimeToAdd += DetermineBuffTimeToAdd;
        }

        private static int DetermineBuffTimeToAdd(On_Player.orig_AddBuff_DetermineBuffTimeToAdd orig, Player self, int type, int time)
        {
            if (!CalamityServerConfig.Instance.NerfExpertDebuffs || !Main.expertMode || !BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty[type])
                return orig(self, type, time);

            if (self.deadCellsPotionStation && BuffID.Sets.BuffTimeIsExtendedByDeadCellsPotionStationBuff[type])
                time = (int)(time * 1.2f);

            return time;
        }
    }
}
