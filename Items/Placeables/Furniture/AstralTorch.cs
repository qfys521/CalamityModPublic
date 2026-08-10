using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class AstralTorch : ModItem, ILocalizedModType
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
            Item.DefaultToTorch(ModContent.TileType<Tiles.Astral.AstralTorch>(), 0, false);
        }

        public override void HoldItem(Player player)
        {
            bool killTorch = Collision.DrownCollision(player.position, player.width, player.height, player.gravDir);
            if (!killTorch && Main.rand.NextBool(player.itemAnimation > 0 ? 10 : 20))
            {
                Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, ModContent.DustType<AstralOrange>());
            }
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

            if (!killTorch)
                Lighting.AddLight(position, 1.6f, 0.6f, 0.3f);
        }

        public override void PostUpdate(WorldItem item)
        {
            if (!item.wet)
                Lighting.AddLight((int)((item.position.X + Item.width / 2) / 16f), (int)((item.position.Y + Item.height / 2) / 16f), 1.6f, 0.6f, 0.3f);
        }

        public override void AddRecipes()
        {
            CreateRecipe(3).
                AddIngredient(ItemID.Torch, 3).
                AddIngredient<StarblightSoot>().
                Register();
        }
    }
}
