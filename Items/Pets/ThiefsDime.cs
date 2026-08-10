using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    public class ThiefsDime : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";
        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<GoldiePet>(), ModContent.BuffType<GoldieBuff>());
            Item.UseSound = SoundID.CoinPickup;
            Item.value = Item.buyPrice(platinum: 1); // Sold by Bandit
            Item.rare = ItemRarityID.Pink;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
                player.AddBuff(Item.buffType, 3600);
        }
    }
}
