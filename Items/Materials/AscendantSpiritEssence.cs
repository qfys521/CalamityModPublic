using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class AscendantSpiritEssence : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public int frameCounter = 0;
        public int frame = 0;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ItemNoGravity[Type] = true;
            ItemID.Sets.SortingPriorityMaterials[Type] = 118;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 54;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }
        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            spriteBatch.Draw(texture, item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 6, 6), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Materials/AscendantSpiritEssenceGlow").Value;
            spriteBatch.Draw(texture, item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 6, 6, false), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        public override void Update(WorldItem item, ref float gravity, ref float maxFallSpeed)
        {
            float brightness = Main.essScale * Main.rand.NextFloat(0.9f, 1.1f);
            Lighting.AddLight(item.Center, 1.2f * brightness, 0.4f * brightness, 0.8f);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Necroplasm>(2).
                AddIngredient<NightmareFuel>(5).
                AddIngredient<EndothermicEnergy>(5).
                AddIngredient<DarksunFragment>(2).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
