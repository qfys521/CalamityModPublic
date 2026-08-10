using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class HardenedEutrophicSand : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            Main.tileShine[Type] = 2500;
            Main.tileShine2[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.Titanium;
            AddMapEntry(new Color(66, 162, 209));

            this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
            this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
            this.RegisterBlendMergeWith(TileID.Sandstone);
            this.RegisterBlendMergeWith(TileID.HardenedSand);
            this.RegisterBlendMergeWith(TileID.Sand);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            if (!up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                //tube corals
                if (WorldGen.genRand.NextBool(8))
                {
                    ushort[] TubeCorals = new ushort[] { (ushort)ModContent.TileType<TubeCoral>(), (ushort)ModContent.TileType<SmallTubeCoral>() };

                    ushort newObject = Main.rand.Next(TubeCorals);

                    WorldGen.PlaceObject(i, j - 1, newObject, true);
                    NetMessage.SendObjectPlacement(-1, i, j - 1, newObject, 0, 0, -1, -1);
                }

                //anemonie
                if (WorldGen.genRand.NextBool(10))
                {
                    WorldGen.PlaceObject(i, j - 1, (ushort)ModContent.TileType<SeaAnemone>(), true);
                    NetMessage.SendObjectPlacement(-1, i, j - 1, (ushort)ModContent.TileType<SeaAnemone>(), 0, 0, -1, -1);
                }
            }

            // Place sunken kelp
            if (WorldGen.genRand.NextBool(2) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<SunkenKelp>();
                up.HasTile = true;

                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.dedServ)
                {
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
                }
            }
        }
    }
}
