using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Walls
{
    public class LargeBasaltWall : MultiVariantModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(65, 64, 68));
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            DrainWater(i - 1, j - 1);
            DrainWater(i - 1, j);
            DrainWater(i - 1, j + 1);
            DrainWater(i, j - 1);
            DrainWater(i, j);
            DrainWater(i, j + 1);
            DrainWater(i + 1, j - 1);
            DrainWater(i + 1, j);
            DrainWater(i + 1, j + 1);

            static void DrainWater(int x, int y)
            {
                var tile = Main.tile[x, y];
                ref var liquidData = ref tile.Get<LiquidData>();

                if (liquidData.LiquidType != LiquidID.Water || liquidData.Amount == 0)
                    return;

                liquidData.Amount = 0;
                WorldGen.SquareTileFrame(x, y);

                if (Main.dedServ)
                    NetMessage.sendWater(x, y);
            }

            Dust dust;
            dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 16, DustID.Smoke, 0f, -1.9069767f, 195, new Color(255, 255, 255), 1f)];
            dust.noGravity = false;
            dust.fadeIn = 1.4209302f;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Tile t = Main.tile[i, j];
            if (t.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                t.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Ash, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = (i % 4) * 468;
            frameYOffset = (j % 4) * 180;
        }
    }
}
