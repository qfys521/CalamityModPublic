using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class AuroraBlazer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 36;
            Item.damage = 69;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 30;
            Item.knockBack = 2f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AuroraFire>();
            Item.shootSpeed = 7.5f;
            Item.useAmmo = AmmoID.Gel;
            Item.consumeAmmoOnFirstShotOnly = true;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item34;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>(Texture + "Glow").Value);

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
    }
}
