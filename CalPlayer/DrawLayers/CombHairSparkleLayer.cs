using CalamityMod.Items.Accessories.Vanity;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.CalPlayer.DrawLayers
{
    public class CombHairSparkleLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.LeinforsHairShampoo);

        public override bool IsHeadLayer => true;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            CalamityPlayer modPlayer = drawPlayer.Calamity();

            // Specific hair drawing requirements: Hair must be drawing with 20% lightness, and you're not bald
            bool canReceiveHairSparkles = (drawInfo.fullHair || drawInfo.hatHair || drawInfo.drawsBackHairWithoutHeadgear || drawPlayer.head == -1 || drawPlayer.head == 0) && Main.rgbToHsl(drawInfo.colorHead).Z > 0.2f;
            bool baldHairStyles = drawPlayer.hair == 15 || drawPlayer.hair == 76;
            return drawInfo.shadow == 0f && !drawPlayer.dead && !drawInfo.headOnlyRender && modPlayer.combHair && canReceiveHairSparkles && !baldHairStyles;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            // 5% chance per frame any of this draws
            if (!Main.rand.NextBool(20))
                return;

            // Search through the equipped index for the comb. We need this to get the right dye
            int dyeIndex = -1;
            for (int i = 0; i < 20; i++)
            {
                if (drawPlayer.armor[i].type == ModContent.ItemType<TheComb>())
                {
                    dyeIndex = i;
                    break;
                }
            }
            var shader = GameShaders.Armor.GetSecondaryShader(dyeIndex == -1 ? 0 : drawPlayer.dye[dyeIndex % 10].dye, drawPlayer);

            // Now detect if there are chests nearby
            int LeftRange = Utils.Clamp((int)drawPlayer.MountedCenter.X / 16 - 50, 2, Main.maxTilesX - 2);
            int RightRange = Utils.Clamp((int)drawPlayer.MountedCenter.X / 16 + 50, 2, Main.maxTilesX - 2);
            int TopRange = Utils.Clamp((int)drawPlayer.MountedCenter.Y / 16 - 50, 2, Main.maxTilesY - 2);
            int BottomRange = Utils.Clamp((int)drawPlayer.MountedCenter.Y / 16 + 50, 2, Main.maxTilesY - 2);

            float range = 50000f;
            Vector2 ChestPosition = Vector2.Zero;
            for (int i = LeftRange; i <= RightRange; i++)
            {
                for (int j = TopRange; j <= BottomRange; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (!tile.HasTile || !TileID.Sets.IsAContainer[tile.TileType] || TileID.Sets.BasicDresser[tile.TileType])
                        continue;

                    Point16 topLeft = TileObjectData.TopLeft(i, j);
                    int PotentialChest = Chest.FindChest(topLeft.X, topLeft.Y);
                    if (PotentialChest != -1)
                    {
                        // Adjust position to the center of the chest
                        Chest chest = Main.chest[PotentialChest];
                        Vector2 chestCenter = new Vector2(chest.x + 1, chest.y + 1) * 16f;
                        float distance = Vector2.Distance(chestCenter, drawPlayer.MountedCenter);

                        if (distance < range)
                        {
                            range = distance;
                            ChestPosition = chestCenter;
                        }
                    }
                }
            }

            if (!drawInfo.hatHair || Main.rand.NextBool())
            {
                Rectangle area = drawInfo.hatHair ? Utils.CenteredRectangle(drawInfo.Position + drawPlayer.Size * 0.5f + new Vector2(drawPlayer.direction * -10, drawPlayer.gravDir * -10f), new Vector2(5f, 5f))
                : Utils.CenteredRectangle(drawInfo.Position + drawPlayer.Size * 0.5f + new Vector2(0f, drawPlayer.gravDir * -20f), new Vector2(20f, 14f));

                Dust sparkle = Dust.NewDustDirect(area.TopLeft(), area.Width, area.Height, DustID.GoldCoin, Alpha: 150, Scale: 0.3f);
                sparkle.fadeIn = 1f;
                sparkle.velocity = ChestPosition == Vector2.Zero ? (sparkle.velocity * 0.1f) : (ChestPosition - sparkle.position).SafeNormalize(Vector2.Zero) * 0.2f;
                sparkle.noLight = true;
                sparkle.shader = shader;
                drawInfo.DustCache.Add(sparkle.dustIndex);
            }

            // Back hair
            if (drawPlayer.velocity.X != 0f && drawInfo.backHairDraw)
            {
                Rectangle areaB = Utils.CenteredRectangle(drawInfo.Position + drawPlayer.Size * 0.5f + new Vector2(drawPlayer.direction * -14, 0f), new Vector2(4f, 30f));
                Dust sparkleB = Dust.NewDustDirect(areaB.TopLeft(), areaB.Width, areaB.Height, DustID.GoldCoin, Alpha: 150, Scale: 0.3f);
                sparkleB.fadeIn = 1f;
                sparkleB.velocity = ChestPosition == Vector2.Zero ? (sparkleB.velocity * 0.1f) : (ChestPosition - sparkleB.position).SafeNormalize(Vector2.Zero) * 0.2f;
                sparkleB.noLight = true;
                sparkleB.shader = shader;
                drawInfo.DustCache.Add(sparkleB.dustIndex);
            }
        }
    }
}
