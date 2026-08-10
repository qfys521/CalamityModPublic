using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss.AbyssAmbient;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Abyss
{
    // Transforms any and all Tenebris in old worlds into planty mush.
    [LegacyName("Tenebris")]
    public class PlantyMush : ModTile
    {
        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/PlantyMushMine", 3);

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Organic"]);

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithAbyss(Type);

            DustType = DustID.Grass;
            AddMapEntry(new Color(84, 102, 39), CalamityUtils.GetItemName<Items.Placeables.Abyss.PlantyMush>());
            HitSound = MineSound;

            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Stone);
            this.RegisterBlendMergeWith(ModContent.TileType<AbyssGravel>());
        }

        int animationFrameWidth = 234;

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            World.Abyss.FillTileWithWater(i, j);
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFramingSystem.GetVariation4x4_01_Low0(i, j);
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            // Place kelp
            if (WorldGen.genRand.NextBool(5) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<AbyssKelp>();
                up.HasTile = true;
                up.TileFrameY = 0;

                //7 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(7) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.dedServ)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }

            Tile down = Main.tile[i, j + 1];
            int vineLength = WorldGen.genRand.Next((int)Main.rockLayer, (int)(Main.rockLayer + (double)Main.maxTilesY * 0.143));
            if (down != null)
            {
                if (!down.HasTile && down.TileType != (ushort)ModContent.TileType<ViperVines>())
                {
                    if (down.LiquidAmount == 255 &&
                        down.WallType == (ushort)ModContent.WallType<SafeAbyssGravelWall>() &&
                        down.LiquidType != LiquidID.Lava)
                    {
                        bool canGrowVine = false;
                        for (int k = vineLength; k > vineLength - 10; k--)
                        {
                            var vineTile = Main.tile[i, k];
                            if (vineTile.BottomSlope)
                            {
                                canGrowVine = false;
                                break;
                            }
                            if (vineTile.HasTile && !vineTile.BottomSlope)
                            {
                                canGrowVine = true;
                                break;
                            }
                        }
                        if (canGrowVine)
                        {
                            int vineX = i;
                            int vineY = j + 1;
                            var newVineTile = Main.tile[vineX, vineY];
                            newVineTile.TileType = (ushort)ModContent.TileType<ViperVines>();
                            newVineTile.Get<TileWallWireStateData>().HasTile = true;
                            WorldGen.SquareTileFrame(vineX, vineY, true);
                            if (Main.dedServ)
                                NetMessage.SendTileSquare(-1, vineX, vineY, 3, TileChangeType.None);
                        }
                    }
                }
            }
        }
    }
}
