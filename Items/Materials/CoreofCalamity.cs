using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class CoreofCalamity : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;

            ItemID.Sets.SortingPriorityMaterials[Type] = 94; // Spectre Bar
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Materials/CoreofCalamityGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<EssenceofSunlight>().
                AddIngredient<EssenceofHavoc>().
                AddIngredient<EssenceofEleum>().
                AddIngredient<AshesofCalamity>().
                AddIngredient(ItemID.Ectoplasm).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
