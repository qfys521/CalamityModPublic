using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;



namespace CalamityMod.Tiles.SunkenSea
{
    public class ScarletSeaGrassTile : ModTile
    {
        public enum ExtraState
        {
            Middle = 36,
            OverhangLeft = 18,
            OverhangRight = 54,
            WallEndLeft = 0,
            WallEndRight = 72
        }

        public Asset<Texture2D> GrassTexture;

        private int extraFrameHeight = 36;
        private int extraFrameWidth = 90;
        public override void SetStaticDefaults()
        {
            GrassTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/ScarletSeaGrass");

            TileID.Sets.GeneralPlacementTiles[Type] = false;

            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileBlockLight[Type] = false;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.Hive;
            AddMapEntry(new Color(216, 50, 50));

            Main.tileSand[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Sand"]);
            TileID.Sets.Suffocate[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;
            TileID.Sets.Conversion.Sand[Type] = true;
            TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
            TileID.Sets.Falling[Type] = true;
            TileID.Sets.FallingBlockProjectile[Type] = new TileID.Sets.FallingBlockProjectileInfo(ModContent.ProjectileType<PolypSandBallFalling>(), 15);

            this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
            this.RegisterBlendMergeWith(TileID.Sandstone);
            this.RegisterBlendMergeWith(TileID.Sand);
            this.RegisterBlendMergeWith(TileID.HardenedSand);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight)
        {
            if (Main.tile[i - 1, j - 1].TileType != Type || Main.tile[i, j - 1].TileType != Type || Main.tile[i + 1, j - 1].TileType != Type ||
                Main.tile[i - 1, j - 2].TileType != Type || Main.tile[i, j - 2].TileType != Type || Main.tile[i + 1, j - 2].TileType != Type)
            {
                Main.tile[i, j].Get<TileSpecialDrawData>().HasSpecialPoint = true;
            }
            else
            {
                Main.tile[i, j].Get<TileSpecialDrawData>().HasSpecialPoint = false;
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.tile[i, j].Get<TileSpecialDrawData>().HasSpecialPoint)
            {
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
            Color drawColour = CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, Lighting.GetColor(i, j));
            Texture2D leaves = GrassTexture.Value;

            DrawExtraTop(i, j, leaves, drawOffset, drawColour);
            DrawExtraWallEnds(i, j, leaves, drawOffset, drawColour);
            DrawExtraDrapes(i, j, leaves, drawOffset, drawColour);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0f;
            b = 0.1f;
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            // Place LongScarletSeagrass
            if (WorldGen.genRand.NextBool(1) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<LongScarletSeagrass>();
                up.HasTile = true;
                up.TileFrameY = 0;

                // 16 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(16) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.dedServ)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }

        #region 'Extra Drapes' Drawing
        private void DrawExtraTop(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            /*
                If the tile directly above this tile is not otherworldly stone, or if it is, there is air to both sides of that tile, draw the Extra surface
            */
            if (
                CheckTile(Type, false, 0, 1, i, j) ||
                (CheckTile(Type, true, 0, 1, i, j) && CheckTile(Type, false, 1, 1, i, j) && CheckTile(Type, false, -1, 1, i, j) && CheckTile(Type, true, 1, 0, i, j) && CheckTile(Type, true, -1, 0, i, j))
                )
            {
                var x = GetExtraState(ExtraState.Middle) + GetExtraVariant(i, j);
                var y = GetExtraPattern(i);
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(0f, 16f), new Rectangle(x, y + 18, 18, 18), drawColour);

                DrawExtraOverhang(i, j, extras, drawOffset, drawColour);
            }
        }
        private bool CheckTile(int type, bool equal, int x, int y, int i, int j)
        {
            //Subtract y so that y is vertical for ease of readability
            return Main.tile[i + x, j - y].TileType == type == equal;
        }
        private void DrawExtraWallEnds(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            /*
                Ending the Extra when a wall is reached
            */

            //Left
            if (
                CheckTile(Type, true, 1, 0, i, j) && CheckTile(Type, false, 1, 1, i, j) && CheckTile(Type, true, 0, 1, i, j) &&
                (CheckTile(Type, true, -1, 1, i, j) || CheckTile(Type, false, -1, 0, i, j))
                )
            {
                var x = GetExtraState(ExtraState.WallEndLeft) + GetExtraVariant(i + 1, j);
                var y = GetExtraPattern(i);
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(0f, 16f), new Rectangle(x, y + 18, 18, 18), drawColour);
            }
            //Right
            if (
                CheckTile(Type, true, -1, 0, i, j) && CheckTile(Type, false, -1, 1, i, j) && CheckTile(Type, true, 0, 1, i, j) &&
                (CheckTile(Type, true, 1, 1, i, j) || CheckTile(Type, false, 1, 0, i, j))
                )
            {
                var x = GetExtraState(ExtraState.WallEndRight) + GetExtraVariant(i - 1, j);
                var y = GetExtraPattern(i);
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(0f, 16f), new Rectangle(x, y + 18, 18, 18), drawColour);
            }
        }
        private void DrawExtraOverhang(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            /*
                Called from DrawExtraTop(). Ending the Extra when the edge of the tile is reached
            */

            //Left
            if (
                CheckTile(Type, false, -1, 0, i, j)
                )
            {
                var x = GetExtraState(ExtraState.OverhangLeft) + GetExtraVariant(i, j);
                var y = GetExtraPattern(i - 1);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(-16f, 0f), new Rectangle(x, y, 18, 18), drawColour);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(-16f, 16f), new Rectangle(x, y + 18, 18, 18), drawColour);
            }
            //Right
            if (
                CheckTile(Type, false, 1, 0, i, j)
                )
            {
                var x = GetExtraState(ExtraState.OverhangRight) + GetExtraVariant(i, j);
                var y = GetExtraPattern(i + 1);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(16f, 0f), new Rectangle(x, y, 18, 18), drawColour);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(16f, 16f), new Rectangle(x, y + 18, 18, 18), drawColour);
            }
        }

        private void DrawExtraDrapes(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            /*
                Hanging 'drapes' of the extra element
            */

            //Base
            if (
                (CheckTile(Type, true, 0, 1, i, j) && CheckTile(Type, false, 0, 2, i, j)) ||
                (CheckTile(Type, true, 0, 2, i, j) && CheckTile(Type, false, 1, 2, i, j) && CheckTile(Type, false, -1, 2, i, j) && CheckTile(Type, true, 1, 1, i, j) && CheckTile(Type, true, -1, 1, i, j))
                )
            {
                var x = GetExtraState(ExtraState.Middle) + GetExtraVariant(i, j - 1);
                var y = GetExtraPattern(i) + 18;
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
            }
            //Left Wall
            if (
                CheckTile(Type, true, 1, 1, i, j) && CheckTile(Type, false, 1, 2, i, j) && CheckTile(Type, true, 0, 2, i, j) &&
                (CheckTile(Type, true, -1, 2, i, j) || CheckTile(Type, false, -1, 1, i, j))
                )
            {
                var x = GetExtraState(ExtraState.WallEndLeft) + GetExtraVariant(i + 1, j - 1);
                var y = GetExtraPattern(i) + 18;
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
            }
            //Right Wall
            if (
                CheckTile(Type, true, -1, 1, i, j) && CheckTile(Type, false, -1, 2, i, j) && CheckTile(Type, true, 0, 2, i, j) &&
                (CheckTile(Type, true, 1, 2, i, j) || CheckTile(Type, false, 1, 1, i, j))
                )
            {
                var x = GetExtraState(ExtraState.WallEndRight) + GetExtraVariant(i - 1, j - 1);
                var y = GetExtraPattern(i) + 18;
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
            }
            //Left Overhang
            if (
                CheckTile(Type, true, 1, 1, i, j) && CheckTile(Type, false, 0, 1, i, j) && CheckTile(Type, false, 0, 2, i, j) && CheckTile(Type, false, 1, 2, i, j)
                )
            {
                var x = GetExtraState(ExtraState.OverhangLeft) + GetExtraVariant(i + 1, j - 1);
                var y = GetExtraPattern(i) + 18;
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
            }
            //Right Overhang
            if (
                CheckTile(Type, true, -1, 1, i, j) && CheckTile(Type, false, 0, 1, i, j) && CheckTile(Type, false, 0, 2, i, j) && CheckTile(Type, false, -1, 2, i, j)
                )
            {
                var x = GetExtraState(ExtraState.OverhangRight) + GetExtraVariant(i - 1, j - 1);
                var y = GetExtraPattern(i) + 18;
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle(x, y, 18, 18), drawColour);
            }
        }

        private int GetExtraState(ExtraState type)
        {
            switch (type)
            {
                case ExtraState.Middle:
                case ExtraState.WallEndLeft:
                case ExtraState.WallEndRight:
                case ExtraState.OverhangLeft:
                case ExtraState.OverhangRight:
                    return (int)type; // Funni Trick

                default:
                    Main.NewText(type.ToString() + " is not a valid Extra sheet state");
                    return 0;
            }
        }

        private int GetExtraPattern(int i)
        {
            return i % 3 * extraFrameHeight;
        }

        private int GetExtraVariant(int i, int j)
        {
            return Main.tile[i, j].TileFrameNumber * extraFrameWidth;
        }
    }
}
#endregion
