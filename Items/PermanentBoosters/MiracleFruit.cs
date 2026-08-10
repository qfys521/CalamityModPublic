using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    public class MiracleFruit : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public const int LifeBoost = 25;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LifeBoost);

        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/MiracleFruitConsume");
        public override void SetStaticDefaults()
        {
            // For some reason Life/Mana boosting items are in this set (along with Magic Mirror+)
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 20; // Life Fruit
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = UseSound;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 24);
            Item.rare = ItemRarityID.Yellow;
        }

        public static bool HasConsumedBefore(Player player) => player.Calamity().mFruit;

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
                    string key = "Mods.CalamityMod.Misc.MiracleFruitText";
                    Color messageColor = Color.DeepSkyBlue;
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
                if (modPlayer.mFruit)
                {
                    return null;
                }

                player.UseHealthMaxIncreasingItem(LifeBoost);
                modPlayer.mFruit = true;
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
                AddIngredient<LifeAlloy>(5).
                AddIngredient<LivingShard>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
