using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Generation.Dungeon;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace CalamityMod.ILEditing
{
    public partial class ILChanges
    {
        public static int DungeonHallXLimit => DungeonHallXLimitOverride ?? (SulphurousSea.BiomeWidth + 25);

        public static int DungeonBaseXLimit => DungeonBaseXLimitOverride ?? (SulphurousSea.BiomeWidth + 167);

        // This exists primarily for Infernum with its larger abyss, but other mods with a reference should be able to theoretically override it.
        // Calamity by itself does not change its value.
        public static int? DungeonHallXLimitOverride
        {
            get;
            set;
        }

        // Same idea as DungeonHallXLimitOverride.
        public static int? DungeonBaseXLimitOverride
        {
            get;
            set;
        }

        #region Fixing of Living Tree/Sulphurous Sea Interactions
        private static void BlockLivingTreesNearOcean(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, int>>(x => Utils.Clamp(x, 560, Main.maxTilesX - 560));
            cursor.Emit(OpCodes.Starg, 0);
        }
        #endregion Fixing of Living Tree/Sulphurous Sea Interactions

        #region Removal of Hardmode Ore Generation from Evil Altars
        private static void PreventSmashAltarCode(On_WorldGen.orig_SmashAltar orig, int i, int j)
        {
            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework)
                return;

            orig(i, j);
        }
        #endregion Removal of Hardmode Ore Generation from Evil Altars

        #region Chlorophyte Spread Improvements
        private static void AdjustChlorophyteSpawnRate(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(300))) // 1 in 300 genRand call used to generate Chlorophyte in mud tiles near jungle grass.
            {
                LogFailure("Chlorophyte Spread Rate", "Could not locate the update chance.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 150); // Increase the chance to 1 in 150.
        }

        private static void AdjustChlorophyteSpawnLimits(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(40))) // Find the 40 Chlorophyte tile limit. This limit is checked within a 71x71-tile square, with the reference tile as the center.
            {
                LogFailure("Chlorophyte Spread Limit", "Could not locate the lower limit.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 60); // Increase the limit to 60.

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(130))) // Find the 130 Chlorophyte tile limit. This limit is checked within a 171x171-tile square, with the reference tile as the center.
            {
                LogFailure("Chlorophyte Spread Limit", "Could not locate the upper limit.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 200); // Increase the limit to 200.
        }
        #endregion Chlorophyte Spread Improvements

        #region World Creation UI Default Size Change
        /// <summary>
        /// Modifies the default world size on the world creation menu to be Large instead of Small.
        /// </summary>
        private static void ChangeDefaultWorldSize(ILContext il)
        {
            // Objective 1: Pop value '0' off the stack and emit value '2'. This changes the enum used for setting the default world size.
            // Objective 2: Invoke UpdatePreviewPlate at the end of the method and set _optionSize to Large.
            var c = new ILCursor(il);

            // OBJECTIVE 1

            // Find and anchor ourselves at roughly the start of the first for loop of this method.
            if (!c.TryGotoNext(x => x.MatchBr(out _)))
            {
                LogFailure("Change Default World Size", "Could not match start of branched for loop.");
                return;
            }

            // Position ourselves directly after where '0' is pushed.
            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(0)))
            {
                LogFailure("Change Default World Size", "Could not match '0' indicating WorldSizeId.Small.");
                return;
            }

            // Pop original value off. Push '2' to the stack.
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_2);

            // OBJECTIVE 2

            // Match right before the method returns.
            if (!c.TryGotoNext(x => x.MatchRet()))
            {
                LogFailure("Change Default World Size", "Could not match end of method.");
                return;
            }

            // Set _optionSize to Large.
            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit(OpCodes.Ldc_I4_2); // '2'
            c.Emit<UIWorldCreation>(OpCodes.Stfld, "_optionSize"); // UIWorldCreation._optionSize

            // Invoke UpdatePreviewPlate with our current instance.
            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit<UIWorldCreation>(OpCodes.Call, "UpdatePreviewPlate"); // UIWorldCreation.UpdatePreviewPlate
        }
        #endregion

        #region Change Small World Description
        /// <summary>
        /// Changes the description of Small worlds to serve as a warning.
        /// </summary>
        private static void SwapSmallDescriptionKey(ILContext il)
        {
            // Objective: Swap the string "UI.WorldDescriptionSizeSmall" with "Mods.CalamityMod.UI.SmallWorldWarning".
            var c = new ILCursor(il);

            // Position ourselves after "UI.WorldDescriptionSizeSmall".
            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.WorldDescriptionSizeSmall")))
            {
                LogFailure("Change Small World Description", "Could not match string \"UI.WorldDescriptionSizeSmall\".");
                return;
            }
            // Pop original value off.
            c.Emit(OpCodes.Pop);

            // Emit our new string "Mods.CalamityMod.UI.SmallWorldWarning".
            c.Emit(OpCodes.Ldstr, "Mods.CalamityMod.UI.SmallWorldWarning");
        }
        #endregion

        #region Prevent Abyss/Dungeon Interactions

        private static void LimitDungeonEntranceXPosition(On_DungeonCrawler.orig_MakeDungeon orig, int x, int y, GenerationProgress progress)
        {
            // Ensure that the base X position stays within its required bounds.
            x = Utils.Clamp(x, DungeonBaseXLimit, Main.maxTilesX - DungeonBaseXLimit);

            // Adjust the Y position of the dungeon to accomodate for the X shift, so that if the clamp shoves the dungeon into the air it has an
            // opportunity to ground itself again.
            //
            // 26JUN2024: Ozzatron: fix bug where this search fails for god-unknown reasons on XL worlds and the dungeon doesn't place
            bool betterPointFound = WorldUtils.Find(new Point(x, y), Searches.Chain(new Searches.Down(9001), new Conditions.IsSolid()), out Point result);
            if (betterPointFound)
                y = result.Y - 10;

            orig(x, y, progress);

            // Vanilla clears DungeonCrawler.dungeonData before its generation pass
            // returns. Preserve the primary dungeon bounds for Calamity's later pass.
            if (GenVars.CurrentDungeon == 0)
                AstralChestGeneration.CaptureDungeonBounds(DungeonCrawler.CurrentDungeonData.dungeonBounds);
        }

        /// <summary>
        /// Ensures that the position of dungeon halls do not exceed a certain horizontal range.
        /// </summary>
        private static void LimitDungeonHallsXPosition(ILContext il)
        {
            var c = new ILCursor(il);

            /* The code being altered is as follows:
             * Vector2D vector2D = default(Vector2D);
             * vector2D.X = (double)i;
             * vector2D.Y = (double)j;
             * 
             * In this context, i and j represent the X and Y position of the dungeon hall, and vector2D represents its position as a vector.
             * The object is to to change the vector2D.X = (double)i; line to actually provide i but clamped.
             */

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdarg(2)))
            {
                LogFailure("Limit Dungeon Hall X Positions", "Could not match the load of argument 0.");
                return;
            }

            // Since the above search specifies that the cursor should be placed after ldarg_0, but before the storage into the X component of the vector, it's
            // possible to simply take in the value as an input for the clamp function and interpret the clamp's output as the true value being stored into the X component.
            // In C#, this represents the following transformation:
            //
            // Original: vector2D.X = (double)i;
            //
            // Altered: vector2D.X = (double)Utils.Clamp(i, DungeonHallXLimit, Main.maxTilesX - DungeonHallXLimit);
            c.EmitDelegate<Func<int, int>>(x => Utils.Clamp(x, DungeonHallXLimit, Main.maxTilesX - DungeonHallXLimit));
        }

        #endregion Prevent Abyss/Dungeon Interactions

        #region Fledgling Wings Loot Changes
        [ThreadStatic]
        private static int? floatingIslandPrimaryLoot;

        /// <summary>
        /// Changes the primary loot of Floating Island chests to include Fledging Wings
        /// </summary>
        private static void MakeFledglingWingsMoreCommon(On_WorldGen.orig_IslandHouse orig, int i, int j, int islandStyle)
        {
            int island = GenVars.skyIslandHouseCount;
            if (island > 4)
                island = WorldGen.genRand.Next(5);

            int? previousLoot = floatingIslandPrimaryLoot;
            floatingIslandPrimaryLoot = island switch
            {
                1 => ItemID.Starfury,
                2 => ItemID.LuckyHorseshoe,
                3 => ItemID.CelestialMagnet,
                4 => ItemID.CreativeWings,
                _ => ItemID.ShinyRedBalloon,
            };

            try
            {
                orig(i, j, islandStyle);
            }
            finally
            {
                floatingIslandPrimaryLoot = previousLoot;
            }
        }

        /// <summary>
        /// Applies Calamity's primary loot choice and removes vanilla's duplicate secondary Fledgling Wings roll.
        /// </summary>
        private static bool ApplyFloatingIslandChestLoot(On_WorldGen.orig_AddBuriedChest_int_int_refPoint_int_bool_int_bool_ushort orig,
            int i, int j, out Point chestLocation, int mainItemInChest, bool notNearOtherChests, int chestStyle, bool trySlope, ushort chestTileType)
        {
            bool floatingIslandChest = floatingIslandPrimaryLoot.HasValue && chestTileType == 0 && (chestStyle == 2 || chestStyle == 13);
            if (floatingIslandChest)
                mainItemInChest = floatingIslandPrimaryLoot.Value;

            bool placed = orig(i, j, out chestLocation, mainItemInChest, notNearOtherChests, chestStyle, trySlope, chestTileType);
            if (!placed || !floatingIslandChest)
                return placed;

            int chestIndex = Chest.FindChest(chestLocation.X, chestLocation.Y);
            if (chestIndex < 0)
                return placed;

            Chest chest = Main.chest[chestIndex];
            for (int slot = 1; slot < chest.item.Length; slot++)
            {
                if (chest.item[slot].type == ItemID.CreativeWings)
                    chest.item[slot].TurnToAir();
            }

            return placed;
        }
        #endregion
    }
}
