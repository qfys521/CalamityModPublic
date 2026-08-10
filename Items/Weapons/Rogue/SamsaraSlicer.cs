using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    [LegacyName("TerraDisk")]
    public class SamsaraSlicer : RogueWeapon
    {
        public static float Speed = 25f;

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 64;
            Item.damage = 80;
            Item.knockBack = 4f;
            Item.useAnimation = Item.useTime = 30;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.DD2_GoblinBomberThrow;

            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;

            Item.DamageType = RogueDamageClass.Instance;
            Item.shoot = ModContent.ProjectileType<SamsaraSlicerProjectile>();
            Item.shootSpeed = Speed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[type] > 4) return false;

            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (player.Calamity().StealthStrikeAvailable() && proj.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[proj].Calamity().stealthStrike = true;
            }
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] > 4) return false;
            return base.CanUseItem(player);
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/SamsaraSlicerGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Equanimity>().
                AddIngredient<Brimblade>().
                AddIngredient<LivingShard>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
