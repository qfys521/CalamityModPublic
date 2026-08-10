using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public class VernalSoil : ModTile
    {
        public Asset<Texture2D> GrassTexture;

        private int extraFrameHeight = 36;
        private int extraFrameWidth = 90;

        public override void SetStaticDefaults()
        {
            GrassTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/VernicFernGrass");

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Organic"]);

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.Mud;
            AddMapEntry(new Color(80, 120, 0));
            HitSound = SoundID.Dig;

            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Stone);
            this.RegisterBlendMergeWith(TileID.Mud);
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

        public override void RandomUpdate(int i, int j, bool underground)
        {
            if (underground)
            {
                int j2 = j - 1;
                if (j2 < 10)
                    j2 = 10;

                if (Main.tile[i, j2].LiquidAmount == 0)
                {
                    if (WorldGen.genRand.NextBool(15))
                    {
                        ushort tileTypeToPlace = (ushort)ModContent.TileType<GiantPlanteraBulb>();
                        int tileTypeToPlaceThickness = 5;
                        bool placeBulb = true;
                        int minDistanceFromOtherBulbs = 10;
                        for (int k = i - minDistanceFromOtherBulbs; k < i + minDistanceFromOtherBulbs; k += 2)
                        {
                            for (int l = j - minDistanceFromOtherBulbs; l < j + minDistanceFromOtherBulbs; l += 2)
                            {
                                if (k > tileTypeToPlaceThickness && k < Main.maxTilesX - tileTypeToPlaceThickness && l > tileTypeToPlaceThickness && l < Main.maxTilesY - tileTypeToPlaceThickness && Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tileTypeToPlace)
                                {
                                    placeBulb = false;
                                    break;
                                }
                            }
                        }

                        if (placeBulb)
                        {
                            if (i < tileTypeToPlaceThickness || i > Main.maxTilesX - tileTypeToPlaceThickness || j2 < tileTypeToPlaceThickness || j2 > Main.maxTilesY - tileTypeToPlaceThickness)
                                return;

                            bool placeTile = true;
                            for (int i2 = i - 2; i2 < i + 3; i2++)
                            {
                                for (int j3 = j2 - 4; j3 < j2 + 1; j3++)
                                {
                                    if (Main.tile[i2, j3] == null)
                                        return;

                                    if (Main.tile[i2, j3].HasTile)
                                        placeTile = false;
                                }

                                if (Main.tile[i2, j2 + 1] == null)
                                    return;

                                if (!WorldGen.SolidTile2(i2, j2 + 1))
                                    placeTile = false;
                            }

                            if (placeTile)
                            {
                                WorldGen.PlaceObject(i, j2, tileTypeToPlace, true);
                                NetMessage.SendObjectPlacement(-1, i, j2, tileTypeToPlace, 0, 0, -1, -1);

                                // Spread of Chlorophyte Partisan clouds if the bulb spawns while a player is near
                                bool isPlayerNear = WorldGen.PlayerLOS(i, j2);
                                if (isPlayerNear)
                                {
                                    float projectileVelocity = 6f;
                                    int projType = ProjectileID.SporeCloud;
                                    int npcType = NPCID.Spore;
                                    Vector2 spawn = new Vector2(i * 16 + 8, j2 * 16 + 8);
                                    Vector2 dustSpawn = new Vector2(i * 16, j2 * 16);
                                    SoundEngine.PlaySound(SoundID.Item73, spawn);
                                    Vector2 destination = new Vector2(i * 16 + 8, (j2 - 2) * 16 + 8) - spawn;
                                    destination.Normalize();
                                    destination *= projectileVelocity;
                                    int numProj = 15;
                                    int numNPCs = 5;
                                    float rotation = MathHelper.ToRadians(100);

                                    for (int projIndex = 0; projIndex < numProj; projIndex++)
                                    {
                                        Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, projIndex / (float)(numProj - 1))) * (Main.rand.NextFloat() + 0.25f);

                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                            Projectile.NewProjectile(new EntitySource_TileUpdate(i, j2), spawn, perturbedSpeed, projType, 0, 0f, Player.FindClosest(new Vector2(i * 16, j2 * 16), 16, 16));

                                        perturbedSpeed *= 2f;
                                        Dust dust = Dust.NewDustDirect(dustSpawn, 16, 16, DustID.JungleSpore, perturbedSpeed.X, perturbedSpeed.Y, 250);
                                        dust.fadeIn = 0.7f;
                                        Dust.NewDustDirect(dustSpawn, 16, 16, (!WorldGen.genRand.NextBool(3) && Main.hardMode) ? DustID.Plantera_Pink : DustID.Plantera_Green, perturbedSpeed.X, perturbedSpeed.Y);
                                    }

                                    if (Main.hardMode)
                                    {
                                        for (int npcIndex = 0; npcIndex < numNPCs; npcIndex++)
                                        {
                                            Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, npcIndex / (float)(numNPCs - 1))) * (Main.rand.NextFloat() + 0.5f) * 0.5f;

                                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                            {
                                                int spore = NPC.NewNPC(new EntitySource_TileUpdate(i, j2), (int)spawn.X, (int)spawn.Y, npcType, 0, -1f);
                                                Main.npc[spore].velocity.X = perturbedSpeed.X;
                                                Main.npc[spore].velocity.Y = perturbedSpeed.Y;
                                                Main.npc[spore].netUpdate = true;
                                            }

                                            perturbedSpeed *= 2f;
                                            Dust dust = Dust.NewDustDirect(dustSpawn, 16, 16, DustID.JungleSpore, perturbedSpeed.X, perturbedSpeed.Y, 250);
                                            dust.fadeIn = 0.7f;
                                            Dust.NewDustDirect(dustSpawn, 16, 16, DustID.Plantera_Pink, perturbedSpeed.X, perturbedSpeed.Y);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.IsTileActuallyInvisible())
                return;

            Tile uptile = Main.tile[i, j - 1];
            if (uptile.HasTile && uptile.IsTileFull())
                return;

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
            Color drawColour = CalamityUtils.ApplyPaint(tile.TileColor, Lighting.GetColor(i, j));
            Texture2D leaves = GrassTexture.Value;

            DrawExtraTop(i, j, leaves, drawOffset, drawColour);
            //DrawExtraWallEnds(i, j, leaves, drawOffset, drawColour);
            DrawExtraDrapes(i, j, leaves, drawOffset, drawColour);
        }

        #region 'Extra Drapes' Drawing
        private void DrawExtraTop(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            // If the tile directly above this tile is not otherworldly stone, or if it is, there is air to both sides of that tile, draw the Extra surface
            if (
                CheckTile(Type, false, 0, 1, i, j) ||
                (CheckTile(Type, true, 0, 1, i, j) && CheckTile(Type, false, 1, 1, i, j) && CheckTile(Type, false, -1, 1, i, j) && CheckTile(Type, true, 1, 0, i, j) && CheckTile(Type, true, -1, 0, i, j))
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("middle") + GetExtraVariant(i, j), GetExtraPattern(i), 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(0f, 16f), new Rectangle?(new Rectangle(GetExtraState("middle") + GetExtraVariant(i, j), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);

                DrawExtraOverhang(i, j, extras, drawOffset, drawColour);
            }
        }

        private void DrawExtraWallEnds(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            // Ending the Extra when a wall is reached

            //Left
            if (
                CheckTile(Type, true, 1, 0, i, j) && CheckTile(Type, false, 1, 1, i, j) && CheckTile(Type, true, 0, 1, i, j) &&
                (CheckTile(Type, true, -1, 1, i, j) || CheckTile(Type, false, -1, 0, i, j))
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("wallEndLeft") + GetExtraVariant(i + 1, j), GetExtraPattern(i), 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(0f, 16f), new Rectangle?(new Rectangle(GetExtraState("wallEndLeft") + GetExtraVariant(i + 1, j), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            //Right
            if (
                CheckTile(Type, true, -1, 0, i, j) && CheckTile(Type, false, -1, 1, i, j) && CheckTile(Type, true, 0, 1, i, j) &&
                (CheckTile(Type, true, 1, 1, i, j) || CheckTile(Type, false, 1, 0, i, j))
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("wallEndRight") + GetExtraVariant(i - 1, j), GetExtraPattern(i), 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(0f, 16f), new Rectangle?(new Rectangle(GetExtraState("wallEndRight") + GetExtraVariant(i - 1, j), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }

        private void DrawExtraOverhang(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            // Called from DrawExtraTop(). Ending the Extra when the edge of the tile is reached

            //Left
            if (
                CheckTile(Type, false, -1, 0, i, j)
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(-16f, 0f), new Rectangle?(new Rectangle(GetExtraState("overhangLeft") + GetExtraVariant(i, j), GetExtraPattern(i - 1), 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(-16f, 16f), new Rectangle?(new Rectangle(GetExtraState("overhangLeft") + GetExtraVariant(i, j), GetExtraPattern(i - 1) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            //Right
            if (
                CheckTile(Type, false, 1, 0, i, j)
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(16f, 0f), new Rectangle?(new Rectangle(GetExtraState("overhangRight") + GetExtraVariant(i, j), GetExtraPattern(i + 1), 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                Main.spriteBatch.Draw(extras, drawOffset + new Vector2(16f, 16f), new Rectangle?(new Rectangle(GetExtraState("overhangRight") + GetExtraVariant(i, j), GetExtraPattern(i + 1) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }

        private void DrawExtraDrapes(int i, int j, Texture2D extras, Vector2 drawOffset, Color drawColour)
        {
            // Hanging 'drapes' of the extra element

            //Base
            if (
                (CheckTile(Type, true, 0, 1, i, j) && CheckTile(Type, false, 0, 2, i, j)) ||
                (CheckTile(Type, true, 0, 2, i, j) && CheckTile(Type, false, 1, 2, i, j) && CheckTile(Type, false, -1, 2, i, j) && CheckTile(Type, true, 1, 1, i, j) && CheckTile(Type, true, -1, 1, i, j))
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("middle") + GetExtraVariant(i, j - 1), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            //Left Wall
            if (
                CheckTile(Type, true, 1, 1, i, j) && CheckTile(Type, false, 1, 2, i, j) && CheckTile(Type, true, 0, 2, i, j) &&
                (CheckTile(Type, true, -1, 2, i, j) || CheckTile(Type, false, -1, 1, i, j))
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("wallEndLeft") + GetExtraVariant(i + 1, j - 1), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            //Right Wall
            if (
                CheckTile(Type, true, -1, 1, i, j) && CheckTile(Type, false, -1, 2, i, j) && CheckTile(Type, true, 0, 2, i, j) &&
                (CheckTile(Type, true, 1, 2, i, j) || CheckTile(Type, false, 1, 1, i, j))
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("wallEndRight") + GetExtraVariant(i - 1, j - 1), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            //Left Overhang
            if (
                CheckTile(Type, true, 1, 1, i, j) && CheckTile(Type, false, 0, 1, i, j) && CheckTile(Type, false, 0, 2, i, j) && CheckTile(Type, false, 1, 2, i, j)
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("overhangLeft") + GetExtraVariant(i + 1, j - 1), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            //Right Overhang
            if (
                CheckTile(Type, true, -1, 1, i, j) && CheckTile(Type, false, 0, 1, i, j) && CheckTile(Type, false, 0, 2, i, j) && CheckTile(Type, false, -1, 2, i, j)
                )
            {
                Main.spriteBatch.Draw(extras, drawOffset, new Rectangle?(new Rectangle(GetExtraState("overhangRight") + GetExtraVariant(i - 1, j - 1), GetExtraPattern(i) + 18, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }
        #endregion

        #region Tile Data
        private bool CheckTile(int type, bool equal, int x, int y, int i, int j)
        {
            //Subtract y so that y is vertical for ease of readability
            return Main.tile[i + x, j - y].TileType == type == equal;
        }

        private int GetExtraState(string type)
        {
            switch (type)
            {
                case "middle":
                    return 36;
                case "overhangLeft":
                    return 18;
                case "overhangRight":
                    return 54;
                case "wallEndLeft":
                    return 0;
                case "wallEndRight":
                    return 72;
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
        #endregion
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
