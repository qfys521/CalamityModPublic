using CalamityMod.Items.Placeables.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    [LegacyName("AbyssTorch")]
    public class VoidTorch : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.Torches[Type] = true;
            ItemID.Sets.SingleUseInGamepad[Type] = true;
            ItemID.Sets.WaterTorches[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
        }

        public override void SetDefaults()
        {
            Item.DefaultToTorch(ModContent.TileType<Tiles.Abyss.VoidTorch>(), 0, true);
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(player.itemAnimation > 0 ? 10 : 20))
            {
                Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustID.DungeonSpirit);
            }
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
            Lighting.AddLight(position, 0.5f, 0.5f, 2f);
        }

        public override void PostUpdate(WorldItem item)
        {
            Lighting.AddLight((int)((item.position.X + Item.width / 2) / 16f), (int)((item.position.Y + Item.height / 2) / 16f), 0.5f, 0.5f, 2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe(3).
                AddIngredient(ItemID.Torch, 3).
                AddIngredient<Voidstone>().
                Register();
        }
    }
}
