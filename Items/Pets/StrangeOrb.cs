using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    public class StrangeOrb : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";
        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<OceanSpirit>(), ModContent.BuffType<OceanSpiritBuff>());
            Item.value = Item.sellPrice(gold: 2);
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
