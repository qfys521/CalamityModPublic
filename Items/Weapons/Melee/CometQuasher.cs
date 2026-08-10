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
    public class CometQuasher : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 92;
            Item.height = 94;
            Item.damage = 45;

            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = Item.useTime = 38;

            Item.knockBack = 2.75f;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<CometQuasherHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override bool MeleePrefix() => true;
        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/CometQuasherGlow").Value);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.MeteoriteBar, 20).
                AddIngredient<EssenceofSunlight>(4).
                AddIngredient(ItemID.FallenStar, 5).
                AddIngredient<StarblightSoot>(12).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
