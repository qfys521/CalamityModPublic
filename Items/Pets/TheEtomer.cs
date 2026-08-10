using CalamityMod.Buffs.Pets;
using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Pets
{
    [LegacyName("RotomRemote")]
    public class TheEtomer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Pets";
        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ModContent.ProjectileType<RotomPet>(), ModContent.BuffType<ElectricTroublemaker>());
            Item.UseSound = SoundID.Item113;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.Calamity().devItem = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Pets/TheEtomerGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddRecipeGroup("AnyGoldBar", 8).
                AddIngredient<PrismShard>(5).
                AddIngredient<ScorchedBone>(5).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
