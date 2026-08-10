using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.FurnitureWulfrum;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    [LegacyName("WulfrumShard")]
    public class WulfrumMetalScrap : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";

        public int textureVariant = 0;

        public static Asset<Texture2D> altTexture = null;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 13;
            Item.height = 10;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(copper: 10);
            Item.rare = ItemRarityID.Blue;
            Item.ammo = Item.type;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Material;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (textureVariant == 0)
                return true;
            else
            {
                spriteBatch.Draw(altTexture.Value, item.Center - Main.screenPosition, null, lightColor, rotation, altTexture.Size() / 2, scale, SpriteEffects.None, 0);
                return false;
            }    
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (textureVariant == 0)
                return true;
            else
            {
                spriteBatch.Draw(altTexture.Value, position, null, drawColor, 0, altTexture.Size() / 2, scale, SpriteEffects.None, 0);
                return false;
            }
        }

        public override void OnSpawn(WorldItem item, IEntitySource source)
        {
            if (source is EntitySource_Loot)
            {
                textureVariant = Main.rand.NextBool().ToInt();

                if (Main.rand.NextBool())
                    return;

                bool closePlayer = false;

                foreach (Player player in Main.ActivePlayers)
                {
                    if ((player.Center - item.Center).Length() < 1200 && player.GetModPlayer<WulfrumBatteryPlayer>().battery)
                    {
                        closePlayer = true;
                        break;
                    }
                }

                if (closePlayer)
                {
                    Item.stack++;
                    SoundEngine.PlaySound(WulfrumBattery.ExtraDropSound, item.Center);

                    int numDust = Main.rand.Next(3, 7);
                    for (int i = 0; i < numDust; i++)
                    {
                        Dust.NewDustDirect(item.position, Item.width, Item.height, Main.rand.NextBool() ? 246 : 247, 0, -3f, Scale: Main.rand.NextFloat(0.9f, 1f));
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<WulfrumPlatform>(2).
                DisableDecraft().
                Register();
        }

        public override void Load()
        {
            Terraria.On_Item.CanFillEmptyAmmoSlot += AvoidDefaultingToAmmoSlot;
            altTexture = ModContent.Request<Texture2D>(Texture + "2");
        }

        private bool AvoidDefaultingToAmmoSlot(Terraria.On_Item.orig_CanFillEmptyAmmoSlot orig, Item self)
        {
            if (self.type == Type)
                return false;
            return orig(self);
        }
    }
}
