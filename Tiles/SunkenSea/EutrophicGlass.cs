using System;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{

    public class EutrophicGlass : ModTile
    {
        private static int sheetWidth = 216;
        private static int sheetHeight = 72;

        public static int TypeCache;

        public Asset<Texture2D> TileTexture;
        public Asset<Texture2D> GlintTexture;

        public Vector2 GlintDir;

        public override void SetStaticDefaults()
        {
            TypeCache = Type;

            TileTexture = ModContent.Request<Texture2D>(Texture + "_Tile");
            GlintTexture = ModContent.Request<Texture2D>(Texture + "_Glint");

            GlintDir = new Vector2(1f, 1f);
            GlintDir.Normalize();

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeSmoothTiles(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            Main.tileLighted[Type] = true;
            Main.tileShine2[Type] = false;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.TruncatesWalls[Type] = true;
            DustType = DustID.RainCloud;
            AddMapEntry(new Color(197, 220, 220));
            HitSound = SoundID.Shatter;
            MinPick = 55;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].IsTileActuallyInvisible())
                return;

            float transparency = 0.4f;

            // Must be set here 
            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileNoSunLight[Type] = false;

            Tile tile = Main.tile[i, j];
            int xPos = i % 10;
            int yPos = j % 10;
            int frameXOffset = xPos * sheetWidth;
            int frameYOffset = yPos * sheetHeight;
            Rectangle frame = new Rectangle(tile.TileFrameX + frameXOffset, tile.TileFrameY + frameYOffset, 16, 16);

            Color color = Lighting.GetColor(i, j) * transparency;
            TileFramingSystem.SlopedGlowmask(in tile, i, j, TileTexture.Value, frame, CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, color, false), default);

            //IF this glint effect below runs poorly on lower end PC's we should keep it as a setting for those with good PC's

            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + offScreen;

            Vector2 screenPos = position;

            float projection = Vector2.Dot(screenPos, GlintDir);

            // this sets the length between the glints diagonally 
            float screenDiagonalLength = Vector2.Dot(new Vector2(Main.screenWidth, Main.screenHeight), GlintDir);

            float stripeWidth = 100f;
            Color lightColor = Lighting.GetColor(i, j) * 2;

            DrawGlint(screenDiagonalLength * 0.53f);
            DrawGlint(screenDiagonalLength * 0.63f);
            DrawGlint(screenDiagonalLength * 0.73f);

            void DrawGlint(float beamCenter)
            {
                float dist = Math.Abs(projection - beamCenter);
                float strength = MathHelper.Clamp(1f - dist / stripeWidth, 0f, 1f) * 0.4f;

                if (strength > 0f)
                {
                    spriteBatch.Draw(GlintTexture.Value, position, frame, lightColor * strength, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFramingSystem.CompactFraming(i, j, resetFrame);
            return false;
        }
    }
}
