using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    public class CosmicPlushie : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";
        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<ChibiiDoggo>(), ModContent.BuffType<ChibiiDoGBuff>());
            Item.UseSound = SoundID.Meowmere;
            Item.value = Item.sellPrice(gold: 7); // Terry reference
            Item.rare = ModContent.RarityType<CosmicPurple>();
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
