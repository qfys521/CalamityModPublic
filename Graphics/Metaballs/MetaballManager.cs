using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Systems.Graphic;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Metaballs
{
    public class MetaballManager : ModSystem
    {
        internal static readonly List<Metaball> metaballs = new();
        internal static Vector2 CameraOffsetSinceLastFrame { get; private set; }

        private static Vector2 previousScreenPosition;
        private static bool hasPreviousScreenPosition;

        public override void Load()
        {
            // Prepare event subscribers.
            GeneralDrawLayerSystem.OnPrepareDraw += PrepareMetaballTargets;
            GeneralDrawLayerSystem.OnDrawLayer += DrawMetaballs;
        }

        public override void OnModUnload()
        {
            // Clear all unmanaged metaball target resources on the GPU on mod unload.
            Main.QueueMainThreadAction(() =>
            {
                foreach (Metaball metaball in metaballs)
                    metaball?.Dispose();
            });
        }

        public override void OnWorldUnload()
        {
            foreach (Metaball metaball in metaballs)
                metaball.ClearInstances();

            CameraOffsetSinceLastFrame = Vector2.Zero;
            previousScreenPosition = Vector2.Zero;
            hasPreviousScreenPosition = false;
        }

        public override void PostUpdateEverything()
        {

            var activeMetaballs = metaballs.Where(m => m.AnythingToDraw);
            foreach (Metaball metaball in activeMetaballs)
            {
                if (metaball.IgnoreFPS)
                    metaball.Update();
            }
        }

        private void PrepareMetaballTargets()
        {
            Vector2 currentScreenPosition = Main.screenPosition;
            CameraOffsetSinceLastFrame = hasPreviousScreenPosition ? previousScreenPosition - currentScreenPosition : Vector2.Zero;
            previousScreenPosition = currentScreenPosition;
            hasPreviousScreenPosition = true;

            // Get a list of all metaballs currently in use.
            var activeMetaballs = metaballs.Where(m => m.AnythingToDraw);

            // Don't bother wasting resources if metaballs are not in use at the moment.
            if (!activeMetaballs.Any())
                return;

            // Prepare the sprite batch for drawing. Metaballs may restart the sprite batch via PrepareSpriteBatch if their implementation requires it.
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);

            var gd = Main.instance.GraphicsDevice;
            foreach (Metaball metaball in activeMetaballs)
            {
                // Update the metaball collection.
                if (!Main.gamePaused && !metaball.IgnoreFPS)
                    metaball.Update();

                // Prepare the sprite batch in accordance to the needs of the metaball instance. By default this does nothing, 
                metaball.PrepareSpriteBatch(Main.spriteBatch);

                // Draw the raw contents of the metaball to each of its render targets.
                foreach (var target in metaball.LayerTargets)
                {
                    using (target.Scope(clearColor: Color.Transparent))
                    {
                        // -2,-2 is to account for the border appplying at screen edge. We need this to draw off-screen.
                        // We directly adjust screen position so that drawing in the metaball functions works as expected
                        var offset = new Vector2(-2, -2);
                        Main.screenPosition += offset;
                        metaball.DrawInstances();
                        Main.screenPosition -= offset;

                        // Flush metaball contents to its render target and reset the sprite batch for the next iteration.
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);
                    }
                }
            }

            Main.spriteBatch.End();
        }

        /// <summary>
        /// Checks if a metaball of a given layering type is currently in use. This is used to minimize needless <see cref="SpriteBatch"/> restarts when there is nothing to draw.
        /// </summary>
        /// <param name="layerType">The metaball layer type to check against.</param>
        internal static bool AnyActiveMetaballsAtLayer(GeneralDrawLayer layerType) =>
            metaballs.Any(m => m.AnythingToDraw && m.DrawLayer == layerType);

        /// <summary>
        /// Draws all metaballs of a given <see cref="GeneralDrawLayer"/>. Used for layer ordering reasons.
        /// </summary>
        /// <param name="layerType">The layer type to draw with.</param>
        private static void DrawMetaballs(GeneralDrawLayer layerType)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            foreach (Metaball metaball in metaballs.Where(m => m.DrawLayer == layerType && m.AnythingToDraw))
            {
                for (int i = 0; i < metaball.LayerTargets.Count; i++)
                {
                    // -2,-2 is to account for the border appplying at screen edge. We need this to draw off-screen.
                    // We directly adjust screen position so that drawing in the metaball functions works as expected
                    var offset = new Vector2(-2, -2);
                    Main.screenPosition += offset;
                    // Prepare shaders for the given layer target.
                    metaball.PrepareShaderForTarget(i);
                    Main.screenPosition -= offset;

                    // Draw the metaball's raw contents with the shader.
                    // If using Odd Mushroom, clone the drawing to each point.
                    Color metaballDraw = Main.LocalPlayer.Calamity().trippy ? Main.DiscoColor : Color.White;
                    Main.spriteBatch.Draw(metaball.LayerTargets[i].Target, offset, metaballDraw);
                    if (Main.LocalPlayer.Calamity().trippy)
                    {
                        Main.spriteBatch.Draw(metaball.LayerTargets[i].Target, offset, null, metaballDraw, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
                        Main.spriteBatch.Draw(metaball.LayerTargets[i].Target, offset, null, metaballDraw, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
                        Main.spriteBatch.Draw(metaball.LayerTargets[i].Target, offset, null, metaballDraw, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                    }
                }
            }

            Main.spriteBatch.End();
        }
    }
}
