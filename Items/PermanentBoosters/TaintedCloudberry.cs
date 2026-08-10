using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    [LegacyName("Elderberry")]
    public class TaintedCloudberry : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public const int LifeBoost = 25;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LifeBoost);

        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/ElderberryConsume");
        public override void SetStaticDefaults()
        {
            // For some reason Life/Mana boosting items are in this set (along with Magic Mirror+)
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 20; // Life Fruit
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 34;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = UseSound;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 28);
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public static bool HasConsumedBefore(Player player) => player.Calamity().tCloudberry;

        public override bool CanUseItem(Player player)
        {
            if (player.ConsumedLifeFruit != Player.LifeFruitMax)
            {
                return false;
            }

            if (HasConsumedBefore(player))
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    string key = "Mods.CalamityMod.Misc.TaintedCloudberryText";
                    Color messageColor = Color.Turquoise;
                    Main.NewText(Language.GetTextValue(key), messageColor);
                }
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                if (modPlayer.tCloudberry)
                {
                    return null;
                }

                player.UseHealthMaxIncreasingItem(LifeBoost);
                modPlayer.tCloudberry = true;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (HasConsumedBefore(Main.LocalPlayer))
                list.AddConsumedTooltip();
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LifeFruit).
                AddIngredient<UelibloomBar>(10).
                AddIngredient<DivineGeode>(8).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
