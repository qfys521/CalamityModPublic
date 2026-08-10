using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("StarfleetMK2")]
    public class Starmada : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static int AmmoSavedPercent = 66;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);

        public override void SetDefaults()
        {
            Item.width = 122;
            Item.height = 50;
            Item.damage = 4900;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 70;
            Item.knockBack = 15f;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<StarmadaStar>();
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.FallenStar;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        }
        // Holdout projectile is spawned when holding the item, so using the item does nothing
        public override bool CanUseItem(Player player) => false;
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            Player Owner = Main.LocalPlayer;
            if (Owner is null)
                return;
            float rate = (Main.GlobalTimeWrappedHourly * 3);
            List<Color> eColors = new List<Color>()
            {
                new Color(164, 47, 160),
                new Color(227, 97, 72),
                new Color(193, 255, 146)
            };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            Color eTooltipColor = Color.Lerp(currentColor, nextColor, rate % 2f >= 1f ? 1f : rate % 1f);

            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip8");
            if (line != null)
                line.Color = Color.Lerp(eTooltipColor, Color.White, 0.2f);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Starfleet>().
                AddIngredient<AuricBar>(5).
                AddIngredient<ExodiumCluster>(25).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
