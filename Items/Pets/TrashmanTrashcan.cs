using CalamityMod.Buffs.Pets;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    public class TrashmanTrashcan : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";

        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<DannyDevitoPet>(), ModContent.BuffType<DannyDevito>());
            Item.UseSound = SoundID.NPCDeath13;
            Item.value = Item.sellPrice(gold: 1); // "Common drop" pet price (Monster Meat)
            Item.rare = ItemRarityID.Orange;
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
