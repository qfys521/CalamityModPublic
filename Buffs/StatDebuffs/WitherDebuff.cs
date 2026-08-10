using CalamityMod.Items.Weapons.Melee;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class WitherDebuff : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs(RemsRevenge.WitherDefenseReduction);

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().wither = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().wither = true;
        }
    }
}
