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
    public class DarksunFragment : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public int frameCounter = 0;
        public int frame = 0;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 8));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ItemNoGravity[Type] = true;
            ItemID.Sets.SortingPriorityMaterials[Type] = 117;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            spriteBatch.Draw(texture, item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 6, 8), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Materials/DarksunFragmentGlow").Value;
            spriteBatch.Draw(texture, item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 6, 8, false), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        public override void Update(WorldItem item, ref float gravity, ref float maxFallSpeed)
        {
            float brightness = Main.essScale * Main.rand.NextFloat(0.9f, 1.1f);
            Lighting.AddLight(item.Center, 0.5f * brightness, 0.5f * brightness, 0.5f * brightness);
        }
    }
}
