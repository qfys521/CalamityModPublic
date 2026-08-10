using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Basalt : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;
            TileID.Sets.HasSlopeFrames[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileShine2[Type] = false;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            DustType = DustID.Ash;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(77, 75, 86));

            MinPick = 110;

            //Stone merges
            this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
            this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
            this.RegisterBlendMergeWith(ModContent.TileType<Runestone>());
            //Sand merges
            this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
            this.RegisterBlendMergeWith(ModContent.TileType<VolcanicSand>());
            this.RegisterBlendMergeWith(TileID.Sandstone);
            this.RegisterBlendMergeWith(TileID.Sand);
            this.RegisterBlendMergeWith(TileID.HardenedSand);
            //Normal merges
            this.RegisterBlendMergeWith(TileID.Stone);
            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Ash);
            this.RegisterBlendMergeWith(TileID.Mud);
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
        public override void RandomUpdate(int i, int j, bool underground)
        {
            Dust dust;
            dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 16, DustID.Smoke, 0f, -1.9069767f, 195, new Color(255, 255, 255), 1f)];
            dust.noGravity = false;
            dust.fadeIn = 1.4209302f;
        }
    }
}
