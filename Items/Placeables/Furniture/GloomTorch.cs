using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class GloomTorch : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.Torches[Type] = true;
            ItemID.Sets.SingleUseInGamepad[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
        }

        public override void SetDefaults()
        {
            Item.DefaultToTorch(ModContent.TileType<Tiles.Crags.GloomTorch>(), 0, false);
        }

        public override void HoldItem(Player player)
        {
            bool killTorch = Collision.DrownCollision(player.position, player.width, player.height, player.gravDir);
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

            if (!killTorch)
                Lighting.AddLight(position, 0.5f, 0.75f, 1.2f);
        }

        public override void PostUpdate(WorldItem item)
        {
            if (!item.wet)
                Lighting.AddLight((int)((item.position.X + Item.width / 2) / 16f), (int)((item.position.Y + Item.height / 2) / 16f), 0.5f, 0.75f, 1.2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe(3).
            AddIngredient(ItemID.Torch, 3).
            AddIngredient<Items.Placeables.Crags.ScorchedBone>().
            Register();
        }
    }
}
