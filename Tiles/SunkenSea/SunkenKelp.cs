using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SunkenKelp : ModTile
    {
        private const int MaxChainLength = 18;
        private static readonly ushort[] ValidAnchors = new ushort[] { (ushort)ModContent.TileType<EutrophicSand>(), (ushort)ModContent.TileType<HardenedEutrophicSand>() };

        private const short FrameBottom = 0;
        private const short FrameMid = 18;
        private const short FrameTop = 36;
        private const short FrameWidth = 18;

        public override void SetStaticDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileID.Sets.IsVine[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Grass;
            DustType = DustID.Grass;

            AddMapEntry(new Color(27, 112, 68));
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            if (!IsTopmostSegment(i, j))
                return;

            if (j <= 0 || Main.tile[i, j - 1].HasTile)
                return;

            if (!IsSupportedFromBelow(i, j))
                return;

            if (GetChainLengthUpward(i, j) >= MaxChainLength)
                return;

            bool placed = WorldGen.PlaceTile(i, j - 1, Type, mute: true, forced: true);
            if (placed)
            {
                SetFrame(i, j - 1, FrameTop);
                SetFrame(i, j, FrameMid);
                UpdateBaseSegment(i, j + GetChainLengthDownward(i, j));
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!IsSupportedFromBelow(i, j))
            {
                WorldGen.KillTile(i, j, noItem: true, effectOnly: false);
                return false;
            }

            if (IsTopmostSegment(i, j))
                SetFrame(i, j, FrameTop);
            else if (IsBottomSegment(i, j))
                SetFrame(i, j, FrameBottom);
            else
                SetFrame(i, j, FrameMid);

            return false;
        }

        private static bool IsSupportedFromBelow(int i, int j)
        {
            if (j >= Main.maxTilesY - 1) return false;
            Tile below = Main.tile[i, j + 1];
            if (!below.HasTile) return false;

            ushort t = below.TileType;
            if (t == ModContent.TileType<SunkenKelp>())
                return true;

            foreach (ushort anchor in ValidAnchors)
                if (t == anchor)
                    return true;

            return false;
        }

        private static bool IsTopmostSegment(int i, int j)
        {
            if (j <= 0) return true;
            Tile above = Main.tile[i, j - 1];
            return !(above.HasTile && above.TileType == ModContent.TileType<SunkenKelp>());
        }

        private static bool IsBottomSegment(int i, int j)
        {
            if (j >= Main.maxTilesY - 1) return true;
            Tile below = Main.tile[i, j + 1];
            return !(below.HasTile && below.TileType == ModContent.TileType<SunkenKelp>());
        }

        private static int GetChainLengthUpward(int i, int j)
        {
            int length = 1;
            int y = j - 1;
            int vineType = ModContent.TileType<SunkenKelp>();
            while (y >= 0 && Main.tile[i, y].HasTile && Main.tile[i, y].TileType == vineType)
            {
                length++;
                y--;
                if (length > MaxChainLength) break;
            }
            return length;
        }

        private static int GetChainLengthDownward(int i, int j)
        {
            int length = 0;
            int y = j + 1;
            int vineType = ModContent.TileType<SunkenKelp>();
            while (y < Main.maxTilesY && Main.tile[i, y].HasTile && Main.tile[i, y].TileType == vineType)
            {
                length++;
                y++;
            }
            return length;
        }

        private static void SetFrame(int i, int j, short frameYRow)
        {
            Tile tile = Main.tile[i, j];
            tile.TileFrameX = (short)(WorldGen.genRand.Next(3) * FrameWidth);
            tile.TileFrameY = frameYRow;
        }

        private static void UpdateBaseSegment(int i, int bottomY)
        {
            Tile t = Main.tile[i, bottomY];
            if (t.HasTile && t.TileType == ModContent.TileType<SunkenKelp>())
                SetFrame(i, bottomY, FrameBottom);
        }
    }
}
