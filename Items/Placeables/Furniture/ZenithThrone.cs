using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture
{
    public class ZenithThrone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ZenithThrone>());
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.Calamity().donorItem = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Zenith).
                AddRecipeGroup(RecipeGroups.IronBar, 10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
