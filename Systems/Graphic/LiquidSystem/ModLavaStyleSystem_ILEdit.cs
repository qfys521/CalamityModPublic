using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityMod.ILEditing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class ModLavaStyleSystem : ModSystem
    {
        private static readonly MethodInfo Tex2DAssetGetter = typeof(Asset<Texture2D>).GetProperty(nameof(Asset<>.Value)).GetMethod;

        private static void ApplyEdits(ManipulatorContext ctx)
        {
            if (Tex2DAssetGetter == null)
            {
                CalamityMod.Log.ILFailure("ModLavaStyle", "Cannot find Getter for Asset<Texture2D>::Value");
                return;
            }

            // Graphic
            IL_LiquidRenderer.DrawNormalLiquids += DrawNormalLiquidPatch;
            IL_TileDrawing.DrawPartialLiquid += DrawPartialLiquidPatch;
            IL_Main.oldDrawWater += DrawOldWaterPatch;
            On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += DrawWaterfall;
            On_WaterfallManager.AddLight += WaterfallAddLight;
            On_WaterfallManager.StylizeColor += WaterfallGlowmaskEditor;

            // Mechanics
            IL_LiquidRenderer.InternalPrepareDraw += LavaBubbleReplacer;
            IL_TileDrawing.EmitLiquidDrops += LavaDropletReplacer;
            IL_NPC.Collision_WaterCollision += SplashEntityLava;
            IL_Projectile.Update += SplashEntityLava;
            IL_WorldItem.MoveInWorld += SplashEntityLava;
            ctx.PlayerUpdate.Add(SplashEntityLava);
            ctx.PlayerUpdate.Add(PlayerDebuffEdit);
        }

        #region Drawing IL Edits

        private static void DrawNormalLiquidPatch(ILContext il)
        {
            const string PatchName = "ModLavaStyle::DrawNormalLiquid";

            var cursor = new ILCursor(il);

            var patched = PatchNextGetTexture2DLdLoc(cursor, (textureIdxLocIdx, textureArrayField) =>
            {
                cursor.EmitLdloc(textureIdxLocIdx);
                cursor.EmitDelegate((Texture2D origTex, int textureIdx) =>
                {
                    return (textureIdx == LiquidID.Lava) ? LavaRT : origTex;
                });
            });

            if (!patched)
            {
                CalamityMod.Log.ILFailure(PatchName, "Unable to Locate Asset<Texture2D>::Value call");
            }
        }

        private static void DrawPartialLiquidPatch(ILContext il)
        {
            const string PatchName = "ModLavaStyle::DrawPartialLiquids";
            const int DesiredPatchCount = 5;

            var cursor = new ILCursor(il);

            int patchedCount = 0;
            foreach (var context in FindAllGetTexture2DLdArg(cursor))
            {
                var c = context.c;
                var textureArgIdx = context.textureArgIdx;
                var textureArrayField = context.textureArrayField;
                var textureArrayFieldName = textureArrayField.Name;
                if (textureArrayField.Name == nameof(TextureAssets.Liquid))
                {
                    c.EmitLdarg(textureArgIdx);
                    c.EmitDelegate((Texture2D origTex, int textureIdx) =>
                    {
                        return (textureIdx == LiquidID.Lava) ? LavaBlockRT : origTex;
                    });
                    patchedCount++;
                }
                else if (textureArrayField.Name == nameof(TextureAssets.LiquidSlope))
                {
                    c.EmitLdarg(textureArgIdx);
                    c.EmitDelegate((Texture2D origTex, int textureIdx) =>
                    {
                        return (textureIdx == LiquidID.Lava) ? LavaSlopeRT : origTex;
                    });
                    patchedCount++;
                }
                else
                {
                    CalamityMod.Log.ILFailure(PatchName, $"Texture Array We referencing is [{textureArrayField.Name}] Which is not intended. Skipping");
                }
            }

            // Finalizing Patch
            if (patchedCount <= 0)
            {
                CalamityMod.Log.ILFailure(PatchName, "Unable to patch any of the texture reference");
                return;
            }
            else
            {
                if (patchedCount != DesiredPatchCount)
                {
                    CalamityMod.Log.Warn($"We did patched into {patchedCount} entry which is unmatching with desired count ({DesiredPatchCount}) when designed this iledit. Please check if anything is broken, If not update the count accordingly to suppress this message!");
                    CalamityMod.Log.Warn($"Location: ModLavaStyleSystem_ILEdit.cs :: DrawPartialLiquidPatch");
                }
            }
        }

        private static void DrawOldWaterPatch(ILContext il)
        {
            const string PatchName = "ModLavaStyle::DrawOldWater";
            const int DesiredPatchCount = 10;

            var cursor = new ILCursor(il);

            int patchedCount = 0;
            foreach (var context in FindAllGetTexture2DLdLoc(cursor))
            {
                var c = context.c;
                var textureLocIdx = context.textureLocIdx;
                var textureArrayField = context.textureArrayField;
                var textureArrayFieldName = textureArrayField.Name;
                if (textureArrayField.Name == nameof(TextureAssets.Liquid))
                {
                    c.EmitLdloc(textureLocIdx);
                    c.EmitDelegate((Texture2D origTex, int textureIdx) =>
                    {
                        return (textureIdx == LiquidID.Lava) ? LavaBlockRT : origTex;
                    });
                    patchedCount++;
                }
                else if (textureArrayField.Name == nameof(TextureAssets.LiquidSlope))
                {
                    c.EmitLdloc(textureLocIdx);
                    c.EmitDelegate((Texture2D origTex, int textureIdx) =>
                    {
                        return (textureIdx == LiquidID.Lava) ? LavaSlopeRT : origTex;
                    });
                    patchedCount++;
                }
                else
                {
                    CalamityMod.Log.ILFailure(PatchName, $"Texture Array We referencing is [{textureArrayField.Name}] Which is not intended. Skipping");
                }
            }

            // Finalizing Patch
            if (patchedCount <= 0)
            {
                CalamityMod.Log.ILFailure(PatchName, "Unable to patch any of the texture reference");
                return;
            }
            else
            {
                if (patchedCount != DesiredPatchCount)
                {
                    CalamityMod.Log.Warn($"We did patched into {patchedCount} entry which is unmatching with desired count ({DesiredPatchCount}) when designed this iledit. Please check if anything is broken, If not update the count accordingly to suppress this message!");
                    CalamityMod.Log.Warn($"Patch Name: {PatchName}");
                }
            }
        }

        private static void DrawWaterfall(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects)
        {
            if (waterfallType == LiquidID.Lava && CurrentLavaStyle != null)
            {
                var lightColor = Lighting.GetColor(x, y);
                Main.spriteBatch.Draw(LavaWaterfallRT, position, sourceRect, lightColor * opacity, 0f, Vector2.Zero, 1f, effects, 0f);
            }
            else
            {
                orig(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
            }
        }

        private static void WaterfallAddLight(On_WaterfallManager.orig_AddLight orig, int waterfallType, int x, int y)
        {
            if (waterfallType == LiquidID.Lava && CurrentLavaStyle is ModLavaStyle lavaStyle)
            {
                float r = 0.55f;
                float g = 0.33f;
                float b = 0.11f;
                lavaStyle.ModifyLight(x, y, ref r, ref g, ref b);
                Lighting.AddLight(x, y, r, g, b);
                return;
            }
            orig.Invoke(waterfallType, x, y);
        }

        private static Color WaterfallGlowmaskEditor(On_WaterfallManager.orig_StylizeColor orig, float alpha, int maxSteps, int waterfallType, int y, int s, Tile tileCache, Color aColor)
        {
            if (waterfallType == LiquidID.Lava && !(CurrentLavaStyle?.LavafallGlowmask() ?? false))
            {
                return aColor;
            }
            else
            {
                return orig.Invoke(alpha, maxSteps, waterfallType, y, s, tileCache, aColor);
            }
        }

        #endregion

        #region Mechanics IL Edits
        private static void LavaBubbleReplacer(ILContext il)
        {
            const string PatchName = "Ambient lava bubble replacer";

            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(16),
                i => i.MatchLdcI4(16),
                i => i.MatchLdcI4(DustID.Lava)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the bubble newdust parameters");
                return;
            }

            // int dustID
            cursor.EmitDelegate<Func<int, int>>(GetSplashDustID);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(16),
                i => i.MatchLdcI4(8),
                i => i.MatchLdcI4(DustID.Lava)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the surface bubble newdust parameters");
                return;
            }

            // int dustID
            cursor.EmitDelegate<Func<int, int>>(GetSplashDustID);
        }

        private static void LavaDropletReplacer(ILContext il)
        {
            const string PatchName = "Ambient lava droplet replacer";

            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(out _),
                i => i.MatchLdcI4(374),
                i => i.MatchBneUn(out _),
                i => i.MatchLdcI4(GoreID.LavaDrip)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the lava droplet newgore parameters");
                return;
            }

            // int goreID
            cursor.EmitDelegate<Func<int, int>>(GetDropletGoreID);
        }

        private static void SplashEntityLava(ILContext il)
        {
            const string PatchName = "Entity Lava Splashing (Item, Projectile, NPC, Player)";

            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<Entity>("width"),
                i => i.MatchLdcI4(12),
                i => i.MatchAdd(),
                i => i.MatchLdcI4(24),
                i => i.MatchLdcI4(DustID.Lava)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the first lava bubble splashing");
                return;
            }

            // int dustID
            cursor.EmitDelegate<Func<int, int>>(GetSplashDustID);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<Entity>("width"),
                i => i.MatchLdcI4(12),
                i => i.MatchAdd(),
                i => i.MatchLdcI4(24),
                i => i.MatchLdcI4(DustID.Lava)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the second lava bubble splashing");
                return;
            }

            // int dustID
            cursor.EmitDelegate<Func<int, int>>(GetSplashDustID);
        }

        private static void PlayerDebuffEdit(ILContext il)
        {
            const string PatchName = "Player Update Lava Debuff";

            //Injects code directly at the position where the OnFire debuff is handled
            ILCursor cursor = new ILCursor(il);

            int onFireTimeLocalIdx = 0;
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg0(),
                i => i.MatchLdcI4(BuffID.OnFire),
                i => i.MatchLdloc(out onFireTimeLocalIdx), //
                i => i.MatchLdcI4(1), // quiet
                i => i.MatchLdcI4(0), // foodHack
                i => i.MatchCall<Player>(nameof(Player.AddBuff))))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the infliction of the On Fire! debuff inside the Player Update code");
                return;
            }
            cursor.EmitLdarg0(); // Player player
            cursor.EmitLdloc(onFireTimeLocalIdx); // int OnFireTime
            cursor.EmitDelegate(InflictDebuff);
        }
        #endregion

        #region Patch Utils

        private static IEnumerable<(ILCursor c, int textureArgIdx, FieldReference textureArrayField)> FindAllGetTexture2DLdArg(ILCursor cursor)
        {
            int textureArgIdx = 0;
            FieldReference textureArrayField = null;
            foreach (var c in FindAll(cursor, MoveType.After,
                x => x.MatchLdsfld(out textureArrayField) || x.MatchLdfld(out textureArrayField),
                x => x.MatchLdarg(out textureArgIdx),
                x => x.MatchLdelemRef(),
                x => x.MatchCallOrCallvirt(Tex2DAssetGetter)))
            {
                yield return (c, textureArgIdx, textureArrayField);
            }
        }

        private static IEnumerable<(ILCursor c, int textureLocIdx, FieldReference textureArrayField)> FindAllGetTexture2DLdLoc(ILCursor cursor)
        {
            int textureLocIdx = 0;
            FieldReference textureArrayField = null;
            foreach (var c in FindAll(cursor, MoveType.After,
                x => x.MatchLdsfld(out textureArrayField) || x.MatchLdfld(out textureArrayField),
                x => x.MatchLdloc(out textureLocIdx),
                x => x.MatchLdelemRef(),
                x => x.MatchCallOrCallvirt(Tex2DAssetGetter)))
            {
                yield return (c, textureLocIdx, textureArrayField);
            }
        }

        private static IEnumerable<ILCursor> FindAll(ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates)
        {
            var c = cursor.Clone();
            while (c.TryGotoNext(moveType, predicates))
            {
                yield return c.Clone();
            }
        }

        private static bool PatchNextGetTexture2DLdArg(ILCursor cursor, Action<int, FieldReference> patcher)
        {
            int textureArgIdx = 0;
            FieldReference textureArrayField = null;
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(out textureArrayField) || x.MatchLdfld(out textureArrayField),
                x => x.MatchLdarg(out textureArgIdx),
                x => x.MatchLdelemRef(),
                x => x.MatchCallOrCallvirt(Tex2DAssetGetter)))
            {
                patcher.Invoke(textureArgIdx, textureArrayField);
                return true;
            }

            return false;
        }

        private static bool PatchNextGetTexture2DLdLoc(ILCursor cursor, Action<int, FieldReference> patcher)
        {
            int textureLocalIdx = 0;
            FieldReference textureArrayField = null;
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(out textureArrayField) || x.MatchLdfld(out textureArrayField),
                x => x.MatchLdloc(out textureLocalIdx),
                x => x.MatchLdelemRef(),
                x => x.MatchCallOrCallvirt(Tex2DAssetGetter)))
            {
                patcher.Invoke(textureLocalIdx, textureArrayField);
                return true;
            }

            return false;
        }

        #endregion

    }
}
