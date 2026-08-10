using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    [LegacyName("BearEye")]
    public class BearsEye : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";

        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<Bear>(), ModContent.BuffType<BearBuff>());
            Item.UseSound = SoundID.Meowmere;
            Item.value = Item.buyPrice(platinum: 5); // Sold by Bandit
            Item.rare = ItemRarityID.Pink;
            Item.Calamity().devItem = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 15);
            }
        }
    }
}
