using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon.Whips
{
    public class ProfanedCrystalWhipDebuff : ModBuff
    {
        public override string Texture => "CalamityMod/Buffs/Summon/Whips/SentinalLash";

        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Vanilla whip tags are player-owned state in 1.4.5 instead of NPC buffs.
            for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
            {
                Player player = Main.player[playerIndex];
                if (player.active && player.TagEffectState.IsNPCTagged(npc.whoAmI))
                    player.TagEffectState.ResetNPCSlotData(npc.whoAmI);
            }
        }
    }
}
