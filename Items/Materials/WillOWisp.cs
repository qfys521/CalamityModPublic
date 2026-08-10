using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class WillOWisp : ModItem, ILocalizedModType
    {
        public int textureVariant = 0;

        public static Asset<Texture2D> altTexture = null;
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 60; // Meteorite
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Material;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            var tex = textureVariant == 0 ? TextureAssets.Item[Type] : CalamityUtils.GetTextureEfficient(ref altTexture, "CalamityMod/Items/Materials/WillOWisp2");
            float rotationAmount = 0.25f;
            float rotationSpeedMult = 0.033f;
            spriteBatch.Draw(tex.Value, item.Center - Main.screenPosition, null, lightColor, rotation + (rotationAmount * (float)Math.Sin(item.whoAmI/* tModPorter Note: Removed. Moved to WorldItem */ + Main.timeForVisualEffects * rotationSpeedMult)), tex.Size() / 2, scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (textureVariant == 0)
                return true;
            else
            {
                var tex = CalamityUtils.GetTextureEfficient(ref altTexture, "CalamityMod/Items/Materials/WillOWisp2");
                spriteBatch.Draw(tex.Value, position, null, drawColor, 0, tex.Size() / 2, scale, SpriteEffects.None, 0);
                return false;
            }
        }
        public override void OnSpawn(WorldItem item, IEntitySource source)
        {
                textureVariant = Main.rand.NextBool().ToInt();
        }
        public override void Update(WorldItem item, ref float gravity, ref float maxFallSpeed)
        {

            if (Collision.SolidCollision(item.position, Item.width, Item.height + CalamityUtils.TilesToPixels(3), true))
                gravity *= 0.2f;
            maxFallSpeed *= 0.1f;
            if (Collision.SolidCollision(item.position, Item.width, Item.height + CalamityUtils.TilesToPixels(2), true))
                gravity *= -1;
            item.velocity.X /= 0.95f;
            item.velocity.X *= 0.975f;
            if (Main.rand.NextBool(10))
            {
                var d = Dust.NewDustDirect(item.position, Item.width, Item.height, DustID.Obsidian, newColor: Color.Red, Scale: 0.9f);
                d.velocity *= 1f;
                d.noGravity = true;
            }
        }
    }
}
