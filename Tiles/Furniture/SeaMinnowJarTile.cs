using CalamityMod.Items.Placeables.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    public class SeaMinnowJarTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.InteractableByNPCs[Type] = true; // Emulating Fish Bowl. NPCs do not interact with Jelly jars (probably because they're deadly or something)
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.addTile(Type);
            AnimationFrameHeight = 36;
            AddMapEntry(new Color(65, 105, 225), CalamityUtils.GetItemName<SeaMinnowJar>());
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Glass, 0f, 0f, 0, new Color(), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = 2;

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            int frameAmt = 12;
            int timeNeeded = 6;
            frameCounter++;
            if (frameCounter >= timeNeeded)
            {
                frame++;
                frameCounter = 0;
            }
            if (frame >= frameAmt)
            {
                frame = 0;
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameYOffset = this.GetAnimationOffset(i, j, 12, 18, 18, 2, 2, AnimationFrameHeight);
        }
    }
}
