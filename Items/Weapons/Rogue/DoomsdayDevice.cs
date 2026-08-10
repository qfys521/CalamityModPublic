using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    [LegacyName("ShockGrenade")]
    public class DoomsdayDevice : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 30;
            Item.damage = 240;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = 55;
            Item.useAnimation = 55;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<DoomsdayDeviceProjectile>();
            Item.shootSpeed = 12f;
            Item.DamageType = RogueDamageClass.Instance;
            Item.channel = true;
        }
        public override bool CanUseItem(Player player)
        {
            // If a grenade is in the throwing animation, don't throw another one.
            for (int x = 0; x < Main.maxProjectiles; x++)
            {
                Projectile projectile = Main.projectile[x];
                if (projectile.active && projectile.type == Item.shoot && projectile.localAI[1] < 5 && projectile.owner == player.whoAmI)
                {
                    return false;
                }
            }
            return true;
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile fakeBomb = Projectile.NewProjectileDirect(source, position, velocity, type, damage, 0, player.whoAmI, 0, 0);
            return false;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/DoomsdayDeviceGlow").Value);
        }
    }
}
