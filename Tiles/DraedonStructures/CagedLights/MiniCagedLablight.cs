using CalamityMod.Items.Placeables.DraedonStructures.CagedLights;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using CalamityMod.Sounds;

namespace CalamityMod.Tiles.DraedonStructures.CagedLights
{
    public class MiniCagedLablight : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = false;

            HitSound = CommonCalamitySounds.PlatingMine;
            DustType = DustID.KryptonMoss;

            TileID.Sets.RoomNeeds.CountsAsTorch[Type] = true;
            AddMapEntry(new Color(48, 201, 214), CalamityUtils.GetItemName<MiniCagedLablightItem>());


            // Attach to ground
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 10; // total 10 frames, all should be same "itemStyle"
            TileObjectData.newTile.StyleWrapLimit = 2; // only 1 placement alternative per row
            TileObjectData.newTile.Origin = new Point16(0, 0);

            // Attach to side (right)
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.addAlternate(2);

            // Attach to ceiling
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.addAlternate(4);

            // Attach to side (left)
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.addAlternate(6);

            // Attach to wall 
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.addAlternate(8);
            TileObjectData.addTile(Type);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void HitWire(int i, int j)
        {
            FurnitureCommon.LightHitWire(Type, i, j, 1, 1);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (Main.tile[i, j].TileFrameX < 18)
            {
                r = 31f / 255f;
                g = 251f / 255f;
                b = 255f / 255f;
            }
            else
            {
                r = 0f;
                g = 0f;
                b = 0f;
            }
        }
    }
}
