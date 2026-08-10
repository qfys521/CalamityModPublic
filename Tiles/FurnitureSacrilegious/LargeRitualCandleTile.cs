using CalamityMod.Items.Placeables.FurnitureSacrilegious;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FurnitureSacrilegious
{
    public class LargeRitualCandleTile : ModTile
    {
        public Asset<Texture2D> FlameTexture;

        public override void SetStaticDefaults()
        {
            // Due to how Ritual Candles are implemented (right click to swap styles), item drop for the alternate style will never register normally.
            RegisterItemDrop(ModContent.ItemType<LargeRitualCandle>());

            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 6;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16,
                16,
                16,
                16,
                16,
                16
            };
            TileObjectData.newTile.Origin = new Point16(0, 4);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.addTile(Type);

            TileID.Sets.RoomNeeds.CountsAsTorch[Type] = true;
            AddMapEntry(new Color(43, 19, 42), CalamityUtils.GetItemName<LargeRitualCandle>());

            TileID.Sets.DisableSmartCursor[Type] = true;
            AdjTiles = new int[] { TileID.Lamps };
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Iron, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (Main.tile[i, j].TileFrameX < 36)
            {
                r = 3f;
                g = 0.6f;
                b = 0.6f;
            }
            else
            {
                r = 0f;
                g = 0f;
                b = 0f;
            }
        }

        public override void HitWire(int i, int j)
        {
            FurnitureCommon.LightHitWire(Type, i, j, 2, 6);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            FlameTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/FurnitureSacrilegious/LargeRitualCandleTileFlame");
            CalamityUtils.DrawFlameEffect(FlameTexture.Value, i, j);
        }
    }
}
