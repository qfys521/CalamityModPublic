using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class RefractiveHangingCoral : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoFail[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileNoSunLight[Type] = false;
            TileID.Sets.IsVine[Type] = true;
            TileID.Sets.VineThreads[Type] = true;
            AddMapEntry(new Color(76, 133, 191));
            DustType = DustID.Grass;
            HitSound = SoundID.Grass;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = Framing.GetTileSafely(i, j + 1);
            if (tile.HasTile && tile.TileType == Type)
                WorldGen.KillTile(i, j + 1);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileAbove = Framing.GetTileSafely(i, j - 1);
            int type = -1;
            if (tileAbove.HasTile && !tileAbove.BottomSlope)
                type = tileAbove.TileType;

            if (type == ModContent.TileType<Shellstone>() || type == Type)
                return true;

            WorldGen.KillTile(i, j);
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer && Main.rand.NextBool(300))
            {
                // this comment will exist until The Great Dustpan is merged:
                // vanilla's Firefly dust (304) is completely yellow.
                // therefore, it is completely unable to be turned blue due to how draw colour works!
                // so it will always appear grey when you try to draw blue, and red/green/yellow otherwise.
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 280, 280, DustID.Firefly, 0.2f, 0f, 0, Color.Lerp(new Color(0, 76, 255), new Color(76, 0, 255), Main.rand.NextFloat()), Main.rand.NextFloat(1f, 2f))];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5f;
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Quite possibly some of the laggiest calculations I've ever seen
            float brightness = 0.9f;
            float time = Main.GlobalTimeWrappedHourly * 60.0f;
            brightness *= MathF.Sin(-j / 40f + time * 0.01f + i);
            Vector3 lilac = new(0.4941f, 0.3686f, 0.9882f);
            Vector3 mint = new(0.3764f, 0.9882f, 0.7294f);
            Vector3 value = Vector3.Lerp(lilac, mint, (MathF.Sin(j / 30f + time * 0.017f + -i / 40f) + 1f) * 0.5f);
            Vector3 value1 = Vector3.Lerp(lilac, mint, (MathF.Sin((-j - 100) / 40f + time * 0.014f + i / 20f) + 1f) * 0.5f);
            r = (value.X + value1.X) / 450f;
            g = (value.Y + value1.Y) / 450f;
            b = (value.Z + value1.Z) / 450f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }

        public override void RandomUpdate(int i, int j, bool underground)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (WorldGen.genRand.NextBool(5) && !tileBelow.HasTile && tileBelow.LiquidType != LiquidID.Lava)
            {
                bool PlaceVine = false;
                int Test = j;
                while (Test > j - 10)
                {
                    Tile testTile = Framing.GetTileSafely(i, Test);
                    if (testTile.BottomSlope)
                    {
                        break;
                    }
                    else if (!testTile.HasTile || testTile.TileType != ModContent.TileType<Shellstone>())
                    {
                        Test--;
                        continue;
                    }
                    PlaceVine = true;
                    break;
                }

                if (PlaceVine)
                {
                    tileBelow.TileType = Type;
                    tileBelow.HasTile = true;
                    WorldGen.SquareTileFrame(i, j + 1, true);
                    if (Main.dedServ)
                        NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
                }
            }
        }
    }
}
