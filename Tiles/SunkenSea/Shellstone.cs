using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Shellstone : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = SoundID.Tink;
            DustType = DustID.CorruptPlants;
            AddMapEntry(new Color(123, 127, 170));

            //Sand merges
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

            if (!Below.HasTile && Below.LiquidType == LiquidID.Water && !Tile.BottomSlope)
            {
                if (Main.rand.NextBool(10))
                {
                    Below.TileType = (ushort)ModContent.TileType<RefractiveHangingCoral>();
                    Below.HasTile = true;
                    WorldGen.SquareTileFrame(i, j + 1, true);
                    if (Main.dedServ)
                        NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
                }
            }
        }
    }
}
