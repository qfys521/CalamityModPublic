using System;
using System.Reflection;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {


        /// <summary>
        /// Sets a <see cref="SpriteBatch"/>'s <see cref="BlendState"/> arbitrarily.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="blendState">The blend state to use.</param>
        public static void SetBlendState(this SpriteBatch spriteBatch, BlendState blendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Determines if a <see cref="SpriteBatch"/> is in a lock due to a <see cref="SpriteBatch.Begin"/> call.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to check.</param>
        public static bool HasBeginBeenCalled(this SpriteBatch spriteBatch)
        {
            return FnaAccessors.IsBegun(spriteBatch);
        }


        /// <summary>
        /// Starts SpriteBatch then Re-Begin batch with old settings when it's all done
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="sortMode"></param>
        /// <param name="settings"></param>
        /// <param name="effect"></param>
        /// <param name="transformMatrix"></param>
        /// <param name="batchCallback"></param>
        [Obsolete("Use SpriteBatch.Begin, SpriteBatchScope, or SpriteBatchSnapshot")]
        public static void SafeBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode,
            BatchSetting settings,
            Effect effect,
            Matrix transformMatrix,
            Action batchCallback
        )
        {
            if (spriteBatch is null)
                return;

            spriteBatch.End(out var ss);
            
            spriteBatch.Begin(sortMode, settings.blendState, settings.samplerState, settings.depthStencilState, settings.rasterizerState ?? Main.Rasterizer, effect, transformMatrix);
            batchCallback?.Invoke();
            spriteBatch.Restart(ss);
        }

        [Obsolete("This is violative of spritebatch's control flow and will eventually be removed")]
        public static bool TryBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode,
            BlendState blendState,
            SamplerState samplerState,
            DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect,
            Matrix transformMatrix)
        {
            if (spriteBatch.HasBeginBeenCalled())
            {
                return false;
            }
            else
            {
                spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect,
                    transformMatrix);
                return true;
            }
        }

        [Obsolete("This is violative of spritebatch's control flow and will eventually be removed")]
        public static bool TryEnd(this SpriteBatch spriteBatch)
        {
            if (!spriteBatch.HasBeginBeenCalled())
            {
                return false;
            }
            else
            {
                spriteBatch.End();
                return true;
            }
        }
    }
}
