using CalamityMod.ExtraTextures.GreyscaleGradients;
using CalamityMod.Sounds;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss.AbyssAmbient;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Abyss
{
    public class Voidstone : GlowMaskTile
    {
        public override string GlowMaskAsset => $"{Texture}_Glowmask";

        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithAbyss(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = CommonCalamitySounds.VoidstoneMine;
            MineResist = 15f;
            MinPick = 180;
            AddMapEntry(new Color(15, 15, 15));

            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Stone);
            this.RegisterBlendMergeWith(ModContent.TileType<PyreMantle>());
        }

        int animationFrameWidth = 234;

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(128, 128, 128), 1f);
            return false;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            World.Abyss.FillTileWithWater(i, j);
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            // Place Tenebris
            if (WorldGen.genRand.NextBool(12) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<TenebrisRemnant>();
                up.HasTile = true;
                up.TileFrameY = 0;

                // 6 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(6) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.dedServ)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFramingSystem.GetVariation4x4_012_Low0(i, j);
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            int time = (int)(Main.timeForVisualEffects * 0.11);
            float brightness = 1.0f - GreyscaleGradient.BlobbyNoise.GetRepeat((i * 100) + time, (j * 100) + time);
            brightness -= 0.55f;
            return new Color(brightness, brightness, brightness);
        }
    }
}
