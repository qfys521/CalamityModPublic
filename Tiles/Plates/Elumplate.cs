using CalamityMod.ExtraTextures.GreyscaleGradients;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.Plates
{
    public class Elumplate : GlowMaskTile
    {
        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = CommonCalamitySounds.PlatingMine;
            MineResist = 1f;
            AddMapEntry(new Color(121, 180, 212));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PlatinumCoin, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PlatinumCoin, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            float brightness = GreyscaleGradient.ElumplatePulse.GetRepeat((int)Main.GameUpdateCount);
            brightness = 0.04f + (brightness * 0.156f);
            return Color.White * brightness;
        }
    }
}
