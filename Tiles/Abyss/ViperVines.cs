using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Abyss
{
    public class ViperVines : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoFail[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
            AddMapEntry(new Color(0, 50, 0));
            HitSound = SoundID.Grass;
            DustType = DustID.Grass;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Main.instance.TilesRenderer.CrawlToTopOfVineAndAddSpecialPoint(j, i);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!WorldGen.isGeneratingOrLoadingWorld)
            {
                Tile tileAbove = Framing.GetTileSafely(i, j - 1);
                if (!tileAbove.HasTile)
                {
                    WorldGen.KillTile(i, j);
                    return true;
                }
            }
            return true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            // GIVE VINE ROPE IF SPECIAL VINE BOOK
            if (WorldGen.genRand.NextBool() && Main.player[(int)Player.FindClosest(new Vector2((float)(i * 16), (float)(j * 16)), 16, 16)].cordage)
                Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16 + 8f, j * 16 + 8f), ItemID.VineRope);

            if (Main.tile[i, j + 1] != null)
            {
                if (Main.tile[i, j + 1].HasTile)
                {
                    if (Main.tile[i, j + 1].TileType == ModContent.TileType<ViperVines>())
                    {
                        WorldGen.KillTile(i, j + 1, false, false, false);
                        if (!Main.tile[i, j + 1].HasTile && Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, (float)i, (float)j + 1, 0f, 0, 0, 0);
                    }
                }
            }
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (WorldGen.genRand.NextBool(5) && !tileBelow.HasTile && tileBelow.LiquidType != LiquidID.Lava)
            {
                bool PlaceVine = false;
                int Test = j;
                while (Test > j - 10)
                {
                    Tile testTile = Framing.GetTileSafely(i, Test);
                    if (testTile.BottomSlope)
                    {
                        break;
                    }
                    else if (!testTile.HasTile || testTile.TileType != ModContent.TileType<PlantyMush>())
                    {
                        Test--;
                        continue;
                    }
                    PlaceVine = true;
                    break;
                }

                if (PlaceVine)
                {
                    tileBelow.TileType = Type;
                    tileBelow.HasTile = true;
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
