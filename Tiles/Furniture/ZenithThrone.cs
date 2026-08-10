using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    public class ZenithThrone : ModTile
    {
        public override void SetStaticDefaults()
        {
            RegisterItemDrop(ModContent.ItemType<Items.Placeables.Furniture.ZenithThrone>());

            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            TileID.Sets.CanBeSatOnForPlayers[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Height = 11;
            TileObjectData.newTile.Width = 8;
            TileObjectData.newTile.Origin = new Point16(4, 9);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
            TileObjectData.addTile(Type);

            // All sofas count as chairs.
            TileID.Sets.RoomNeeds.CountsAsChair[Type] = true;
            AddMapEntry(new Color(43, 199, 217), Language.GetText("ItemName.Throne"));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.IceTorch);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Player player = Main.LocalPlayer;

            int tileNum = tile.TileFrameX / 18;

            info.DirectionOffset = 0;
            float offset = 0f;
            // left 2 tiles
            if (tileNum <= 1)
                offset = 18 * (2 - tileNum);
            // right 2 tiles
            else if (tileNum >= 6)
                offset = 18 * -(tileNum - 5);

            if (player.direction == -1)
                offset *= -1;

            info.VisualOffset = new Vector2(offset, -8);
            info.TargetDirection = player.direction;

            info.AnchorTilePosition.X = i;
            info.AnchorTilePosition.Y = j;
        }

        public override bool RightClick(int i, int j) => FurnitureCommon.ChairRightClick(i, j);

        public override void MouseOver(int i, int j) => FurnitureCommon.BenchMouseOver(i, j, ModContent.ItemType<Items.Placeables.Furniture.ZenithThrone>());

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);
        }
    }
}
