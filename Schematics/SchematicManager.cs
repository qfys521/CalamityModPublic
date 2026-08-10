using System;
using System.Collections.Generic;
using CalamityMod.TileEntities;
using CalamityMod.Tiles.DraedonStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Schematics
{
    public sealed class SchematicManager : ModSystem
    {
        internal const string ShimmerShrineKey = "Shimmer Shrine Key";
        internal const string ShimmerShrineFilename = "Schematics/Shimmer_Shrine.csch";

        internal const string RustedWorkshopKey = "Rusted Workshop";
        internal const string RustedWorkshopFilename = "Schematics/RustedWorkshop.csch";

        internal const string ResearchOutpostKey = "Research Outpost";
        internal const string ResearchOutpostFilename = "Schematics/ResearchOutpost.csch";

        internal const string SunkenSeaLabKey = "Sunken Sea Laboratory";
        internal const string SunkenSeaLabFilename = "Schematics/Arsenal_Lab_Sunken.csch";

        internal const string PlanetoidLabKey = "Planetoid Laboratory";
        internal const string PlanetoidLabFilename = "Schematics/Arsenal_Lab_Planetoid.csch";

        internal const string PlagueLabKey = "Plague Laboratory";
        internal const string PlagueLabFilename = "Schematics/Arsenal_Lab_Plague.csch";

        internal const string HellLabKey = "Hell Laboratory";
        internal const string HellLabFilename = "Schematics/Arsenal_Lab_Underworld.csch";

        internal const string IceLabKey = "Ice Laboratory";
        internal const string IceLabFilename = "Schematics/Arsenal_Lab_Ice.csch";

        internal const string CavernLabKey = "Cavern Laboratory";
        internal const string CavernLabFilename = "Schematics/Arsenal_Lab_Onyx.csch";

        internal const string CorruptionShrineKey = "Corruption Shrine";
        internal const string CorruptionShrineFilename = "Schematics/Shrine_Corruption.csch";

        internal const string CrimsonShrineKey = "Crimson Shrine";
        internal const string CrimsonShrineFilename = "Schematics/Shrine_Crimson.csch";

        internal const string DesertShrineKey = "Desert Shrine";
        internal const string DesertShrineFilename = "Schematics/Shrine_Desert.csch";

        internal const string GraniteShrineKey = "Granite Shrine";
        internal const string GraniteShrineFilename = "Schematics/Shrine_Granite.csch";

        internal const string IceShrineKey = "Ice Shrine";
        internal const string IceShrineFilename = "Schematics/Shrine_Ice.csch";

        internal const string MarbleShrineKey = "Marble Shrine";
        internal const string MarbleShrineFilename = "Schematics/Shrine_Marble.csch";

        internal const string MushroomShrineKey = "Mushroom Shrine";
        internal const string MushroomShrineFilename = "Schematics/Shrine_Mushroom.csch";

        internal const string SurfaceShrineKey = "Surface Shrine";
        internal const string SurfaceShrineFilename = "Schematics/Shrine_Surface.csch";

        internal const string RoxcaliburShrineKey1 = "Roxcalibur Shrine 1";
        internal const string RoxcaliburShrine1Filename = "Schematics/Shrine_Roxcalibur_ShrineVariant.csch";
        internal const string RoxcaliburShrineKey2 = "Roxcalibur Shrine 2";
        internal const string RoxcaliburShrine2Filename = "Schematics/Shrine_Roxcalibur_TorchVariant.csch";

        internal const string VernalKey = "Vernal Pass";
        internal const string VernalFilename = "Schematics/VernalPass.csch";

        internal const string MechanicShedKey = "Mechanic Key";
        internal const string MechanicShedFilename = "Schematics/MechanicShed.csch";

        internal const string AstralBeaconKey = "Astral Beacon";
        internal const string AstralBeaconFilename = "Schematics/AstralBeacon.csch";

        internal const string CragBridgeKey = "Crags Bridge";
        internal const string CragBridgeFilename = "Schematics/CragBridge.csch";

        internal const string BlueArchiveKey = "Archive Blue";
        internal const string BlueArchiveFilename = "Schematics/DungeonArchiveBlue.csch";

        internal const string GreenArchiveKey = "Archive Green";
        internal const string GreenArchiveFilename = "Schematics/DungeonArchiveGreen.csch";

        internal const string PinkArchiveKey = "Archive Pink";
        internal const string PinkArchiveFilename = "Schematics/DungeonArchivePink.csch";

        internal const string CragRuinKey1 = "Crag Ruin 1";
        internal const string CragRuinKey1Filename = "Schematics/CragRuin1.csch";
        internal const string CragRuinKey2 = "Crag Ruin 21";
        internal const string CragRuinKey2Filename = "Schematics/CragRuin2.csch";
        internal const string CragRuinKey3 = "Crag Ruin 3";
        internal const string CragRuinKey3Filename = "Schematics/CragRuin3.csch";
        internal const string CragRuinKey4 = "Crag Ruin 4";
        internal const string CragRuinKey4Filename = "Schematics/CragRuin4.csch";

        internal static Dictionary<string, SchematicMetaTile[,]> TileMaps;
        internal static Dictionary<string, PilePlacementFunction> PilePlacementMaps;
        public delegate void PilePlacementFunction(int x, int y, Rectangle placeInArea);

        #region Load/Unload
        public override void OnModLoad()
        {
            PilePlacementMaps = new Dictionary<string, PilePlacementFunction>();
            TileMaps = new Dictionary<string, SchematicMetaTile[,]>
            {
                // Shimmer Shrine
                [ShimmerShrineKey] = CalamitySchematicIO.LoadSchematic(ShimmerShrineFilename),

                // Draedon's Arsenal world gen structures
                [RustedWorkshopKey] = CalamitySchematicIO.LoadSchematic(RustedWorkshopFilename),
                [ResearchOutpostKey] = CalamitySchematicIO.LoadSchematic(ResearchOutpostFilename),
                [SunkenSeaLabKey] = CalamitySchematicIO.LoadSchematic(SunkenSeaLabFilename),
                [PlanetoidLabKey] = CalamitySchematicIO.LoadSchematic(PlanetoidLabFilename),
                [PlagueLabKey] = CalamitySchematicIO.LoadSchematic(PlagueLabFilename),
                [HellLabKey] = CalamitySchematicIO.LoadSchematic(HellLabFilename),
                [IceLabKey] = CalamitySchematicIO.LoadSchematic(IceLabFilename),
                [CavernLabKey] = CalamitySchematicIO.LoadSchematic(CavernLabFilename),

                // Shrine world gen structures
                [CorruptionShrineKey] = CalamitySchematicIO.LoadSchematic(CorruptionShrineFilename),
                [CrimsonShrineKey] = CalamitySchematicIO.LoadSchematic(CrimsonShrineFilename),
                [DesertShrineKey] = CalamitySchematicIO.LoadSchematic(DesertShrineFilename),
                [GraniteShrineKey] = CalamitySchematicIO.LoadSchematic(GraniteShrineFilename),
                [IceShrineKey] = CalamitySchematicIO.LoadSchematic(IceShrineFilename),
                [MarbleShrineKey] = CalamitySchematicIO.LoadSchematic(MarbleShrineFilename),
                [MushroomShrineKey] = CalamitySchematicIO.LoadSchematic(MushroomShrineFilename),
                [SurfaceShrineKey] = CalamitySchematicIO.LoadSchematic(SurfaceShrineFilename),
                [RoxcaliburShrineKey1] = CalamitySchematicIO.LoadSchematic(RoxcaliburShrine1Filename),
                [RoxcaliburShrineKey2] = CalamitySchematicIO.LoadSchematic(RoxcaliburShrine2Filename),

                [VernalKey] = CalamitySchematicIO.LoadSchematic(VernalFilename),

                [MechanicShedKey] = CalamitySchematicIO.LoadSchematic(MechanicShedFilename),

                // Astral world gen structures
                [AstralBeaconKey] = CalamitySchematicIO.LoadSchematic(AstralBeaconFilename),

                //crag bridge
                [CragBridgeKey] = CalamitySchematicIO.LoadSchematic(CragBridgeFilename),

                //dungeon archives
                [BlueArchiveKey] = CalamitySchematicIO.LoadSchematic(BlueArchiveFilename),
                [GreenArchiveKey] = CalamitySchematicIO.LoadSchematic(GreenArchiveFilename),
                [PinkArchiveKey] = CalamitySchematicIO.LoadSchematic(PinkArchiveFilename),

                //crags ruins
                [CragRuinKey1] = CalamitySchematicIO.LoadSchematic(CragRuinKey1Filename),
                [CragRuinKey2] = CalamitySchematicIO.LoadSchematic(CragRuinKey2Filename),
                [CragRuinKey3] = CalamitySchematicIO.LoadSchematic(CragRuinKey3Filename),
                [CragRuinKey4] = CalamitySchematicIO.LoadSchematic(CragRuinKey4Filename),

                // Sulphurous Sea scrap world gen structures
                ["Sulphurous Scrap 1"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap1.csch").ShaveOffEdge(),
                ["Sulphurous Scrap 2"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap2.csch").ShaveOffEdge(),
                ["Sulphurous Scrap 3"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap3.csch").ShaveOffEdge(),
                ["Sulphurous Scrap 4"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap4.csch").ShaveOffEdge(),
                ["Sulphurous Scrap 5"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap5.csch").ShaveOffEdge(),
                ["Sulphurous Scrap 6"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap6.csch").ShaveOffEdge(),
                ["Sulphurous Scrap 7"] = CalamitySchematicIO.LoadSchematic("Schematics/SulphurousScrap7.csch").ShaveOffEdge(),
            };
        }

        public override void Unload()
        {
            TileMaps = null;
            PilePlacementMaps = null;
        }
        #endregion

        #region Get Schematic Area
        public static Vector2? GetSchematicArea(string name)
        {
            // If no schematic exists with this name, simply return null.
            if (!TileMaps.TryGetValue(name, out SchematicMetaTile[,] schematic))
                return null;

            return new Vector2(schematic.GetLength(0), schematic.GetLength(1));
        }
        #endregion Get Schematic Area

        #region Place Schematic
        public static void PlaceSchematic<T>(string name, Point pos, SchematicAnchor anchorType, ref bool specialCondition, T chestDelegate = null, bool flipHorizontal = false) where T : Delegate
        {
            // If no schematic exists with this name, cancel with a helpful log message.
            if (!TileMaps.TryGetValue(name, out SchematicMetaTile[,] schematic))
            {
                CalamityMod.Log.Warn($"Tried to place a schematic with name \"{name}\". No matching schematic file found.");
                return;
            }

            // Invalid chest interaction delegates need to throw an error.
            if (chestDelegate != null &&
                !(chestDelegate is Action<Chest>) &&
                !(chestDelegate is Action<Chest, int, bool>))
            {
                throw new ArgumentException("The chest interaction function has invalid parameters.", nameof(chestDelegate));
            }
            PilePlacementMaps.TryGetValue(name, out PilePlacementFunction pilePlacementFunction);

            // Grab the schematic itself from the dictionary of loaded schematics.
            int width = schematic.GetLength(0);
            int height = schematic.GetLength(1);

            // Calculate the appropriate location to start laying down schematic tiles.
            int cornerX = pos.X;
            int cornerY = pos.Y;
            switch (anchorType)
            {
                case SchematicAnchor.TopLeft: // Provided point is top-left corner = No change
                case SchematicAnchor.Default: // This is also default behavior
                default:
                    break;
                case SchematicAnchor.TopCenter: // Provided point is top center = Top-left corner is 1/2 width to the left
                    cornerX -= width / 2;
                    break;
                case SchematicAnchor.TopRight: // Provided point is top-right corner = Top-left corner is 1 width to the left
                    cornerX -= width;
                    break;
                case SchematicAnchor.CenterLeft: // Provided point is left center: Top-left corner is 1/2 height above
                    cornerY -= height / 2;
                    break;
                case SchematicAnchor.Center: // Provided point is center = Top-left corner is 1/2 width and 1/2 height up-left
                    cornerX -= width / 2;
                    cornerY -= height / 2;
                    break;
                case SchematicAnchor.CenterRight: // Provided point is right center: Top-left corner is 1 width and 1/2 height up-left
                    cornerX -= width;
                    cornerY -= height / 2;
                    break;
                case SchematicAnchor.BottomLeft: // Provided point is bottom-left corner = Top-left corner is 1 height above
                    cornerY -= height;
                    break;
                case SchematicAnchor.BottomCenter: // Provided point is bottom center: Top-left corner is 1/2 width and 1 height up-left
                    cornerX -= width / 2;
                    cornerY -= height;
                    break;
                case SchematicAnchor.BottomRight: // Provided point is bottom-right corner = Top-left corner is 1 width to the left and 1 height above
                    cornerX -= width;
                    cornerY -= height;
                    break;
            }

            // Make sure that all four corners of the target area are actually in the world.
            if (!WorldGen.InWorld(cornerX, cornerY) || !WorldGen.InWorld(cornerX + width, cornerY + height))
            {
                CalamityMod.Log.Warn("Schematic failed to place: Part of the target location is outside the game world.");
                return;
            }

            // Make an array for the tiles that used to be where this schematic will be pasted.
            SchematicMetaTile[,] originalTiles = new SchematicMetaTile[width, height];

            // Schematic area pre-processing has three steps.
            // Step 1: Kill all trees and cacti specifically. This prevents ugly tree/cactus pieces from being restored later.
            // Step 2: Fill the original tiles array with everything that was originally in the target rectangle.
            // Step 3: Destroy everything in the target rectangle (except chests -- that'll cause infinite recursion).
            // The third step is necessary so that multi tiles on the edge of the region are properly destroyed (e.g. Life Crystals).

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    Tile t = Main.tile[x + cornerX, y + cornerY];
                    if (t.TileType == TileID.Trees || t.TileType == TileID.PineTree || t.TileType == TileID.Cactus)
                        WorldGen.KillTile(x + cornerX, y + cornerY, noItem: true);
                }

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    Tile t = Main.tile[x + cornerX, y + cornerY];
                    originalTiles[x, y] = new SchematicMetaTile(t);
                }

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                    if (originalTiles[x, y].TileType != TileID.Containers)
                        WorldGen.KillTile(x + cornerX, y + cornerY, noItem: true);

            // Lay down the schematic. If the schematic calls for it, bring back tiles that are stored in the old tiles array.
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    //Uses the tile opposite where its placing when the schematic is flipped horizontally
                    SchematicMetaTile smt = schematic[flipHorizontal ? width - 1 - x : x, y];
                    smt.ApplyTo(x + cornerX, y + cornerY, originalTiles[x, y]);
                    Tile worldTile = Main.tile[x + cornerX, y + cornerY];

                    //Handle any additional fixes that need to be made as a result of this structure being flipped horizontally
                    if (flipHorizontal && !smt.keepTile)
                    {
                        //Turns Left Slopes into Right Slopes and vice versa.
                        if (worldTile.Slope != SlopeType.Solid)
                            worldTile.Slope += (int)worldTile.Slope % 2 == 0 ? -1 : 1;

                        //Correct Multi-Tile TileFrames
                        int style = 0, alt = 0;
                        TileObjectData.GetTileInfo(worldTile, ref style, ref alt);
                        TileObjectData data = TileObjectData.GetTileData(worldTile.TileType, style, alt);
                        if (data != null && !TileID.Sets.Platforms[worldTile.TileType])
                        {
                            int sheetSquare = 16 + data.CoordinatePadding;

                            if (data.Width > 1)
                            {
                                int frameNum = worldTile.TileFrameX / sheetSquare % data.Width;
                                //This equation gets us the amount we need to move TileFrameX to correct any issues MultiTiles that occurred during the flip process
                                worldTile.TileFrameX += (short)((-frameNum + (data.Width - (frameNum + 1))) * (16 + data.CoordinatePadding));
                            }

                            //Flips Tiles that place directionally (Chairs, Beds, ect.)                        
                            if (data.Direction != Terraria.Enums.TileObjectDirection.None)// && !ValidTileEntityTypes.Contains(worldTile.TileType))
                            {
                                int range = 1;
                                if (data.RandomStyleRange > range)
                                    range = data.RandomStyleRange;
                                if (worldTile.TileFrameX / sheetSquare % (data.Width * data.StyleMultiplier * range) < data.Width)
                                    worldTile.TileFrameX += (short)(sheetSquare * data.Width);
                                else
                                    worldTile.TileFrameX -= (short)(sheetSquare * data.Width);
                            }
                        }
                        //Fix Platform TileFrames
                        else if (TileID.Sets.Platforms[worldTile.TileType])
                            switch (worldTile.TileFrameX / 18)
                            {
                                case 1:
                                    worldTile.TileFrameX += 18;
                                    break;
                                case 2:
                                    worldTile.TileFrameX -= 18;
                                    break;
                                case 3:
                                    worldTile.TileFrameX += 18;
                                    break;
                                case 4:
                                    worldTile.TileFrameX -= 18;
                                    break;
                                case 8:
                                    worldTile.TileFrameX += 36;
                                    break;
                                case 10:
                                    worldTile.TileFrameX -= 36;
                                    break;
                                case 12:
                                    worldTile.TileFrameX += 18;
                                    break;
                                case 13:
                                    worldTile.TileFrameX -= 18;
                                    break;
                                case 15:
                                    worldTile.TileFrameX += 18;
                                    break;
                                case 16:
                                    worldTile.TileFrameX -= 18;
                                    break;
                                case 19:
                                    worldTile.TileFrameX += 18;
                                    break;
                                case 20:
                                    worldTile.TileFrameX -= 18;
                                    break;
                                case 25:
                                    worldTile.TileFrameX += 18;
                                    break;
                                case 26:
                                    worldTile.TileFrameX -= 18;
                                    break;
                            }
                        //A handful of tiles do not have any TileObjectData and/or are unqiuely sheeted. Because of this we need to correct their frames manually
                        //Note that this is not a comprehensive list. So far only tiles which appear in existing Calamity Structures are here. There may be other tiles which need to be added in the future.
                        else
                        {
                            switch (worldTile.TileType)
                            {
                                case TileID.Pots: //Vanilla pots do not have any TileObjectData, however Modded pots which do will be caught by the above conditions.
                                    if (worldTile.TileFrameX / 18 == 0)
                                        worldTile.TileFrameX += 18;
                                    else
                                        worldTile.TileFrameX -= 18;
                                    break;
                                case TileID.HolidayLights: //Christmas Lights must be fixed when facing Left or Right
                                    if (worldTile.TileFrameY / 18 == 3)
                                        worldTile.TileFrameY -= 18;
                                    else if (worldTile.TileFrameY / 18 == 2)
                                        worldTile.TileFrameY += 18;
                                    break;
                                case TileID.MinecartTrack: //Minecart Tracks are very similar to Platforms, and must be dealt with accordingly. Strangely, their TileFrameX and Y are handled very differently when compared to most other tiles.
                                    switch (worldTile.TileFrameX)
                                    {
                                        case 2:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 3:
                                            worldTile.TileFrameX--;
                                            break;
                                        case 4:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 5:
                                            worldTile.TileFrameX--;
                                            break;
                                        case 6:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 7:
                                            worldTile.TileFrameX--;
                                            break;
                                        case 8:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 9:
                                            worldTile.TileFrameX--;
                                            break;
                                        case 14:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 15:
                                            worldTile.TileFrameX--;
                                            break;
                                        case 18:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 19:
                                            worldTile.TileFrameX--;
                                            break;
                                        case 24:
                                            worldTile.TileFrameX++;
                                            break;
                                        case 25:
                                            worldTile.TileFrameX--;
                                            break;
                                    }
                                    if (worldTile.TileFrameY == 8)
                                        worldTile.TileFrameY++;
                                    else if (worldTile.TileFrameY == 9)
                                        worldTile.TileFrameY--;
                                    break;
                                case TileID.ExposedGems: //Similarly to Holiday Lights, we need to determine if Exposed Gems are facing to the left or right and then flip them. However unlike holiday lights, Exposed Gems have 3 variants we must also account for. We do this by multiplying the standard size of 18 by three when dividing the TileFrameY
                                    if (worldTile.TileFrameY / 54 == 3)
                                        worldTile.TileFrameY -= 54;
                                    else if (worldTile.TileFrameY / 54 == 2)
                                        worldTile.TileFrameY += 54;
                                    break;
                                case TileID.Trees: //Trees need to be corrected manually as their sheets are pretty much wholely unique.
                                    if (worldTile.TileFrameY / 22 >= 9)
                                    {
                                        if (worldTile.TileFrameX / 22 == 1)
                                            break;
                                        else if (worldTile.TileFrameX / 22 == 2)
                                            worldTile.TileFrameX += 22;
                                        else if (worldTile.TileFrameX / 22 == 3)
                                            worldTile.TileFrameX -= 22;
                                    }
                                    else
                                    {
                                        switch (worldTile.TileFrameX / 22)
                                        {
                                            case 0:
                                                if (worldTile.TileFrameY / 22 > 5)
                                                    worldTile.TileFrameX += 66;
                                                break;
                                            case 1:
                                                worldTile.TileFrameX += 22;
                                                break;
                                            case 2:
                                                worldTile.TileFrameX -= 22;
                                                break;
                                            case 3:
                                                worldTile.TileFrameX += 22;
                                                if (worldTile.TileFrameY / 22 < 3)
                                                    worldTile.TileFrameY += 66;
                                                else if (worldTile.TileFrameY / 18 < 6)
                                                    worldTile.TileFrameY -= 66;
                                                break;
                                            case 4:
                                                worldTile.TileFrameX -= 22;
                                                if (worldTile.TileFrameY / 22 < 3)
                                                    worldTile.TileFrameY += 66;
                                                else if (worldTile.TileFrameY / 22 < 6)
                                                    worldTile.TileFrameY -= 66;
                                                else if (worldTile.TileFrameY / 22 >= 6)
                                                    worldTile.TileFrameX -= 66;
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    // If the determined tile type is a chest and this is its top left corner, define it appropriately.
                    // Skip this step if this schematic position preserves tiles.
                    bool isChest = worldTile.TileType == TileID.Containers || TileID.Sets.BasicChest[worldTile.TileType];
                    if (!smt.keepTile && isChest && worldTile.TileFrameX % 36 == 0 && worldTile.TileFrameY == 0)
                    {
                        // Do not do anything if a chest was already defined at this multitile's top-left corner.
                        Point16 topLeft = TileObjectData.TopLeft(x + cornerX, y + cornerY);
                        int chestIndex = Chest.FindChest(topLeft.X, topLeft.Y);
                        if (chestIndex == -1)
                        {
                            chestIndex = Chest.CreateChest(x + cornerX, y + cornerY, -1);
                            Chest chest = Main.chest[chestIndex];
                            // Use the appropriate chest delegate function to fill the chest.
                            if (chestDelegate is Action<Chest, int, bool>)
                            {
                                (chestDelegate as Action<Chest, int, bool>)?.Invoke(chest, worldTile.TileType, specialCondition);
                                specialCondition = true;
                            }
                            else if (chestDelegate is Action<Chest>)
                                (chestDelegate as Action<Chest>)?.Invoke(chest);
                        }
                    }

                    // Now that the tile data is correctly set, place appropriate tile entities.
                    TryToPlaceTileEntities(x + cornerX, y + cornerY, worldTile);

                    // Activate the pile placement function if defined.
                    Rectangle placeInArea = new Rectangle(x, y, width, height);
                    pilePlacementFunction?.Invoke(x + cornerX, y + cornerY, placeInArea);
                }
        }
        #endregion

        #region Place Schematic Helper Methods
        //This list allows us to check if a tile is valid before we get its data and try to place a Tile Entity
        private static readonly List<int> ValidTileEntityTypes =
        [
            ModContent.TileType<ChargingStation>(),
            ModContent.TileType<DraedonLabTurret>(),
            ModContent.TileType<LabHologramProjector>(),
            ModContent.TileType<HostileFireTurret>(),
            ModContent.TileType<HostileIceTurret>(),
            ModContent.TileType<HostileLaserTurret>(),
            ModContent.TileType<HostileOnyxTurret>(),
            ModContent.TileType<HostilePlagueTurret>(),
            ModContent.TileType<HostileWaterTurret>()
        ];
        private static void TryToPlaceTileEntities(int x, int y, Tile t)
        {
            int tileType = t.TileType;

            if (!ValidTileEntityTypes.Contains(tileType))
                return;

            int index = ValidTileEntityTypes.IndexOf(tileType);

            int FrameX, FrameY;
            int style = 0, alt = 0;
            TileObjectData.GetTileInfo(t, ref style, ref alt);
            TileObjectData data = TileObjectData.GetTileData(t.TileType, style, alt);
            //Using TileObjectData, we're able to check if the given tile is the top left of any style.
            //This resolves issues in generating the Lab Hologram Projector when facing to the left, and should also apply to similar Tile Entities
            if (data != null)
            {
                int sheetSquare = 16 + data.CoordinatePadding;
                FrameX = t.TileFrameX / sheetSquare % data.Width;
                FrameY = t.TileFrameY / sheetSquare % data.Height;
            }
            else
            {
                FrameX = t.TileFrameX;
                FrameY = t.TileFrameY;
            }
            //Instead of just checking if t.TileFrameX == 0, we want to check if its the top left corner of any available style.
            //For example, the old function would not work if the Lab Hologram Projector was facing to the left, due to the top left of that style not being at TileFrameX == 0. Now it will work!
            if (t.HasTile && FrameX == 0 && FrameY == 0)
            {
                switch (index)
                {
                    case 0:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEChargingStation>());
                        break;
                    case 1:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileLabTurret>());
                        break;
                    case 2:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TELabHologramProjector>());
                        break;
                    case 3:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileFireTurret>());
                        break;
                    case 4:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileIceTurret>());
                        break;
                    case 5:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileLaserTurret>());
                        break;
                    case 6:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileOnyxTurret>());
                        break;
                    case 7:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostilePlagueTurret>());
                        break;
                    case 8:
                        TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileWaterTurret>());
                        break;
                }
            }
        }
        #endregion
    }
}
