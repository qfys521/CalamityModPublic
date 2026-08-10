using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Tiles.LivingFire;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.LivingFire
{
    public class LivingBrimstoneFireBlock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<LivingBrimstoneFireBlockTile>());

        public override void PostUpdate(WorldItem item)
        {
            Lighting.AddLight((int)((item.position.X + Item.width / 2) / 16f), (int)((item.position.Y + Item.height / 2) / 16f), 1f, 0f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe(20).
                AddIngredient(ItemID.LivingFireBlock, 20).
                AddIngredient<BrimstoneSlag>().
                AddTile(TileID.CrystalBall).
                Register();
        }
    }
}
