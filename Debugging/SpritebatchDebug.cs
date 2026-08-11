using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.ModLoader;

namespace CalamityMod.Debugging
{
    public static class SpritebatchDebug
    {
        public static string Trace { get; set; }
    }

    [Autoload(false)]
    public class SpritebatchDebugInitializer : ILoadable
    {
        private static Hook _beginHook;
        private static Hook _endHook;
        
        public void Load(Mod mod)
        {
            MethodInfo begin = typeof(SpriteBatch).GetMethod(
                nameof(SpriteBatch.Begin),
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[]
                {
                    typeof(SpriteSortMode),
                    typeof(BlendState),
                    typeof(SamplerState),
                    typeof(DepthStencilState),
                    typeof(RasterizerState),
                    typeof(Effect),
                    typeof(Matrix)
                },
                modifiers: null);
            MethodInfo end = typeof(SpriteBatch).GetMethod(nameof(SpriteBatch.End), BindingFlags.Public | BindingFlags.Instance);
            
            Debug.Assert(begin != null);
            Debug.Assert(end != null);
            
            _beginHook = new Hook(begin, Begin_Impl);
            _endHook = new Hook(end, End_Impl);
        }

        private static void Begin_Impl(Action<SpriteBatch, SpriteSortMode, BlendState,
            SamplerState,
            DepthStencilState,
            RasterizerState,
            Effect,
            Matrix> orig, SpriteBatch self, SpriteSortMode sortMode,
            BlendState blendState,
            SamplerState samplerState,
            DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect,
            Matrix transformMatrix)
        {
            if (FnaAccessors.IsBegun(self)) ModLoader.GetMod(nameof(CalamityMod)).Logger.Debug("[SPRITEBATCH DEBUG] Begin was last called here: " + SpritebatchDebug.Trace);
            SpritebatchDebug.Trace = Environment.StackTrace;
            
            orig(self, sortMode,
                blendState,
                samplerState,
                depthStencilState,
                rasterizerState,
                effect,
                transformMatrix);
        }

        private static void End_Impl(Action<SpriteBatch> orig, SpriteBatch self)
        {
            if (!FnaAccessors.IsBegun(self)) ModLoader.GetMod(nameof(CalamityMod)).Logger.Debug("[SPRITEBATCH DEBUG] Doesn't seem like Begin was called. Here's the last end call: " + SpritebatchDebug.Trace);
            SpritebatchDebug.Trace = Environment.StackTrace;
            
            orig(self);
        }
        
        public void Unload()
        {
            _beginHook.Dispose();
            _endHook.Dispose();
        }
    }
}
