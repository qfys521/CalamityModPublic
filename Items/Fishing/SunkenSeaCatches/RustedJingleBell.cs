using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.SunkenSeaCatches
{
    public class RustedJingleBell : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<BabyGhostBell>(), ModContent.BuffType<BabyGhostBellBuff>());
            Item.value = Item.sellPrice(gold: 3); // Equal to Zephyr Fish
            Item.rare = ItemRarityID.Orange;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }
    }
}
