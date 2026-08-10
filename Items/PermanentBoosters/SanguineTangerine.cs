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
    [LegacyName("BloodOrange")]
    public class SanguineTangerine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public const int LifeBoost = 25;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LifeBoost);

        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/BloodOrangeConsume");
        public override void SetStaticDefaults()
        {
            // For some reason Life/Mana boosting items are in this set (along with Magic Mirror+)
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 20; // Life Fruit
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 40;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = UseSound;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 16);
            Item.rare = ItemRarityID.LightPurple;
        }

        public static bool HasConsumedBefore(Player player) => player.Calamity().sTangerine;

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
                    string key = "Mods.CalamityMod.Misc.SanguineTangerineText";
                    Color messageColor = Color.Orange;
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
                if (modPlayer.sTangerine)
                {
                    return null;
                }

                player.UseHealthMaxIncreasingItem(LifeBoost);
                modPlayer.sTangerine = true;
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
                AddIngredient<BloodOrb>(10).
                AddIngredient(ItemID.SoulofFright, 5).
                AddIngredient(ItemID.SoulofMight, 5).
                AddIngredient(ItemID.SoulofSight, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
