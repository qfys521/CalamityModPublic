using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class ArmorCrunch : ModBuff
    {
        public static int DefenseReduction = 15;
        public static float MultiplicativeDamageReductionPlayer = 0.5f;
        // 15% DR reduction
        public static float MultiplicativeDamageReductionEnemy = 0.85f;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().armorCrunch = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().armorCrunch = true;
        }
    }
}
