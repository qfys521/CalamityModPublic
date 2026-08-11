using System;
using System.Threading;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static readonly Rectangle[] Rects9Slice = [
            new Rectangle(x: 0, y: 0, width: 4, height: 4),
            new Rectangle(x: 4, y: 0, width: 8, height: 4),
            new Rectangle(x: 12, y: 0, width: 4, height: 4),

            new Rectangle(x: 0, y: 4, width: 4, height: 8),
            new Rectangle(x: 4, y: 4, width: 8, height: 8),
            new Rectangle(x: 12, y: 4, width: 4, height: 8),

            new Rectangle(x: 0, y: 12, width: 4, height: 4),
            new Rectangle(x: 4, y: 12, width: 8, height: 4),
            new Rectangle(x: 12, y: 12, width: 4, height: 4),
        ];

        private static readonly Rectangle[] Rects4Slice = [
            new Rectangle(x: 0, y: 0, width: 8, height: 8),
            new Rectangle(x: 8, y: 0, width: 8, height: 8),
            new Rectangle(x: 0, y: 8, width: 8, height: 8),
            new Rectangle(x: 8, y: 8, width: 8, height: 8),
        ];

        private static readonly ThreadLocal<Color[]> ColorSliceBuffer = new(() => new Color[9]);

        private void OnDrawTiles(On_Main.orig_DrawTiles orig, Main self, bool solidLayer, bool intoRenderTargets, int waterStyleOverride)
        {
            orig(self, solidLayer, intoRenderTargets, waterStyleOverride);

            if (!solidLayer || CalamityClientConfig.Instance.TileTextureBlendingQuality == TileBlendingQuality.Disable)
                return;

            var screenPosition = Main.Camera.UnscaledPosition;
            var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            var offset = zero + (Main.Camera.UnscaledPosition - Main.Camera.ScaledPosition);
            CalamityUtils.GetScreenDrawArea(screenPosition, offset, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY);

            for (int x = firstTileX; x <= lastTileX; x++)
            {
                for (int y = firstTileY; y <= lastTileY; y++)
                {
                    if (!WorldGen.InWorld(x, y))
                        continue;

                    var tile = Main.tile[x, y];
                    if (!tile.Get<TileSpecialDrawData>().HasBlendMergeData)
                        continue;

                    if (!CalamityTileSets.DrawBlendMergeAfterSolidTile[tile.TileType])
                        continue;

                    if (!TryGetBlendingRefData(x, y, out var blendRefs))
                        continue;

                    DrawOnTile(tile, x, y, in blendRefs);
                }
            }
        }

        public static void DrawOnTile(Tile tile, int tileX, int tileY, in TileBlendingRef[] blendRefs)
        {
            // Generic Drawing Parameter
            var tileType = tile.TileType;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(tileX * 16, tileY * 16) - Main.screenPosition + zero;
            var tileRandomFrame = Math.Clamp(tile.TileFrameNumber, 0, 2);
            var isFullBright = tile.IsTileFullbright;
            Color tileLight = Lighting.GetColor(tileX, tileY);

            // Sliced Rendering
            int sliceLength = 0;
            Rectangle[] sliceRects = null;
            Color[] colorSliceBuffer = null;

            // Is HalfBlock condition is also in vanilla, so we follow that
            var silcedConfigEnabled = CalamityClientConfig.Instance.TileTextureBlendingQuality == TileBlendingQuality.High;
            if (silcedConfigEnabled && Lighting.NotRetro && !tile.IsHalfBlock && !TileID.Sets.DontDrawTileSliced[tileType])
            {
                var tileRenderer = Main.instance.TilesRenderer;
                if (tileLight.IsAnyChannelGreaterThan(TerrariaInternals.HighQualityLightingRequirement(tileRenderer)))
                {
                    sliceLength = 9;
                    sliceRects = Rects9Slice;
                    colorSliceBuffer = ColorSliceBuffer.Value;
                    Lighting.GetColor9Slice(tileX, tileY, ref colorSliceBuffer);
                }
                else if (tileLight.IsAnyChannelGreaterThan(TerrariaInternals.MediumQualityLightingRequirement(tileRenderer)))
                {
                    sliceLength = 4;
                    sliceRects = Rects4Slice;
                    colorSliceBuffer = ColorSliceBuffer.Value;
                    Lighting.GetColor4Slice(tileX, tileY, ref colorSliceBuffer);
                }
            }

            // If tile is Actuated, Set brightness to 40%
            // Otherwise it sets to 100% or 160% (if shine)
            var finalColorMultiplier = tile.IsActuated ? 0.4f : (Main.tileShine2[tileType] ? 1.6f : 1.0f);
            var finalMultColor = new Color(finalColorMultiplier, finalColorMultiplier, finalColorMultiplier);

            foreach (var blendRef in blendRefs)
            {
                var sheetIdx = blendRef.SheetIndex;
                var data = blendRef.BlendData;

                // Break here as standard for TileBlendingData is 0->Count fill, so further fields should be also Invalid
                if (sheetIdx == TileBlendTextureLoader.EmptySlot)
                    break;

                var key = new SheetPositionKey((BlendSideFlags)data, (byte)tileRandomFrame);
                var blendTexture = TileBlendTextureLoader.Registry[sheetIdx];

                blendTexture.RequestBake(tileRandomFrame);
                if (!blendTexture.TryGetDrawingInfo(key, out var texture, out var rect))
                    continue;

                // No Slice Drawing
                if (sliceLength <= 0 || isFullBright)
                {
                    var drawColor = isFullBright ? Color.White : tileLight;
                    var finalColor = CalamityUtils.ApplyPaint(tile.TileColor, drawColor, deepPaintOnly: false).MultiplyRGB(finalMultColor);
                    Main.spriteBatch.Draw(texture, drawPos, rect, finalColor, rotation: 0.0f, origin: default, scale: 1.0f, SpriteEffects.None, layerDepth: 0.0f);
                    continue;
                }

                // Sliced Drawing
                for (int i = 0; i < sliceLength; i++)
                {
                    // Calculate the source rectangle for the specific slice from the blend texture sheet
                    var sourceSliceRect = sliceRects[i];
                    sourceSliceRect.X += rect.X;
                    sourceSliceRect.Y += rect.Y;

                    // Calculate the destination position for the slice on the screen
                    var destinationSlicePos = drawPos + sliceRects[i].Location.ToVector2();
                    var drawColorVec = (tileLight.ToVector3() + colorSliceBuffer[i].ToVector3()) * 0.5f;
                    var finalColor = CalamityUtils.ApplyPaint(tile.TileColor, new Color(drawColorVec), deepPaintOnly: false).MultiplyRGB(finalMultColor);
                    Main.spriteBatch.Draw(texture, destinationSlicePos, sourceSliceRect, finalColor, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
