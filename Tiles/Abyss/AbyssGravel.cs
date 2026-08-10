using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss.AbyssAmbient;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Abyss
{
    public class AbyssGravel : ModTile
    {
        int animationFrameWidth = 234;

        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/AbyssGravelMine", 3);

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithAbyss(Type);

            AddMapEntry(new Color(25, 28, 54));
            MineResist = 5f;
            MinPick = 65;
            HitSound = MineSound;
            DustType = DustID.Water;

            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Stone);
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            World.Abyss.FillTileWithWater(i, j);
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            // Place kelp
            if (WorldGen.genRand.NextBool(8) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<AbyssKelp>();
                up.HasTile = true;
                up.TileFrameY = 0;

                // 16 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(16) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.dedServ)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFramingSystem.GetVariation4x4_012_Low0(i, j);
        }
    }
}
