using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class TranquilityCandle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToTorch(ModContent.TileType<Tiles.Furniture.TranquilityCandle>(), 0, false);
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void HoldItem(Player player)
        {
            player.Calamity().tranquilityCandle = true;

            // Do not make light if wet
            if (Collision.DrownCollision(player.position, player.width, player.height, player.gravDir))
                return;

            if (Main.rand.NextBool(player.itemAnimation > 0 ? 10 : 20))
            {
                Dust.NewDust(new Vector2(player.itemLocation.X + 10f * player.direction, player.itemLocation.Y - 12f * player.gravDir), 4, 4, DustID.PurpleTorch);
            }
            player.itemLocation.Y += 8;
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
            Lighting.AddLight(position, 0.55f, 0.85f, 1f);
        }

        public override void PostUpdate(WorldItem item)
        {
            if (!item.wet)
                Lighting.AddLight((int)((item.position.X + Item.width / 2) / 16f), (int)((item.position.Y + Item.height / 2) / 16f), 1f, 0.55f, 1f);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.PeaceCandle).
                AddIngredient<ZenPotion>().
                AddIngredient<EssenceofEleum>(2).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
