using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    [LegacyName("FrequencyManipulator")]
    public class PulseGrenade : RogueWeapon, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 30;
            Item.height = 40;
            Item.damage = 123;
            Item.DamageType = RogueDamageClass.Instance;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = 75;
            Item.useAnimation = 75;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.channel = true;

            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.shootSpeed = 16f;
            Item.shoot = ModContent.ProjectileType<PulseGrenadeProjectile>();
        }

        public override float StealthDamageMultiplier => 0.85f;
        public override bool CanUseItem(Player player)
        {
            return (player.ownedProjectileCounts[Item.shoot] <= 0 && player.ownedProjectileCounts[ModContent.ProjectileType<PulseGrenadeOrb>()] <= 0);
        }
        public override void HoldItem(Player player)
        {
            bool notUsingItem = (player.ownedProjectileCounts[Item.shoot] <= 0 && player.ownedProjectileCounts[ModContent.ProjectileType<PulseGrenadeOrb>()] <= 0);
            if (!notUsingItem)
            {
                player.Calamity().rogueStealth = 0;
            }

            player.Calamity().mouseWorldListener = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 2);
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile grenade = Projectile.NewProjectileDirect(source, position, velocity, type, damage, 0, player.whoAmI, 0, 0);
            return false;
        }
        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/PulseGrenadeGlow").Value);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(8).
                AddIngredient<DubiousPlating>(12).
                AddRecipeGroup("AnyMythrilBar", 10).
                AddIngredient(ItemID.SoulofSight, 20).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(2, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
