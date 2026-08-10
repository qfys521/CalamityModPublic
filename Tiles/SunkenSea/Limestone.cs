using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Limestone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = SoundID.Tink;
            DustType = DustID.Pot;
            AddMapEntry(new Color(184, 138, 113));

            //Stone merges
            this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
            this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
            this.RegisterBlendMergeWith(ModContent.TileType<Runestone>());

            //Sand merges
            this.RegisterBlendMergeWith(ModContent.TileType<PolypSand>());
            this.RegisterBlendMergeWith(ModContent.TileType<ScarletSeaGrassTile>());
            this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
            this.RegisterBlendMergeWith(ModContent.TileType<VolcanicSand>());
            this.RegisterBlendMergeWith(TileID.Sandstone);
            this.RegisterBlendMergeWith(TileID.Sand);
            this.RegisterBlendMergeWith(TileID.HardenedSand);

            //Normal merges
            this.RegisterBlendMergeWith(TileID.Stone);
            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Ash);
            this.RegisterBlendMergeWith(TileID.Mud);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile Tile = Framing.GetTileSafely(i, j);
            Tile Below = Framing.GetTileSafely(i, j + 1);
            Tile Above = Framing.GetTileSafely(i, j - 1);

            if (!Below.HasTile && Below.LiquidType <= LiquidID.Water && !Tile.BottomSlope)
            {
                if (Main.rand.NextBool(10))
                {
                    Below.TileType = (ushort)ModContent.TileType<GilHerb>();
                    Below.HasTile = true;
                    WorldGen.SquareTileFrame(i, j + 1, true);
                    if (Main.dedServ)
                    {
                        NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
                    }
                }
            }
        }
    }
}
