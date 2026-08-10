using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class MajesticGuard : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.width = 100;
            Item.height = 100;
            Item.damage = 270;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = Item.useTime = 50;
            Item.useTurn = true;
            Item.knockBack = 12f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<MajesticGuardHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override bool MeleePrefix() => true;
        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MajesticGuardGlow").Value);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.GoldBroadsword).
                AddRecipeGroup("AnyMythrilBar", 15).
                AddIngredient<EssenceofSunlight>(3).
                AddIngredient<EssenceofHavoc>(3).
                AddIngredient<EssenceofEleum>(3).
                AddTile(TileID.MythrilAnvil).
                Register();
            CreateRecipe().
                AddIngredient(ItemID.PlatinumBroadsword).
                AddRecipeGroup("AnyMythrilBar", 15).
                AddIngredient<EssenceofSunlight>(3).
                AddIngredient<EssenceofHavoc>(3).
                AddIngredient<EssenceofEleum>(3).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
