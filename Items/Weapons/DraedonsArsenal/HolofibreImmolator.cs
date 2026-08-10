using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    [LegacyName("MatterModulator")]
    public class HolofibreImmolator : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
        public static readonly SoundStyle PlasmaSound = new("CalamityMod/Sounds/Item/PlasmaSmall");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }
        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 26;
            Item.height = 78;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 30;
            Item.knockBack = 0.8f;
            Item.useTime = Item.useAnimation = 14;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = null;
            Item.noMelee = true;

            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.shoot = ModContent.ProjectileType<HolofibreImmolatorHoldout>();
            Item.shootSpeed = 12f;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;

            player.Calamity().mouseWorldListener = true;
        }
        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/HolofibreImmolatorGlow").Value);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<HolofibreImmolatorHoldout>(), damage, knockback, player.whoAmI, 0, 1);
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 2);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(12).
                AddIngredient<DubiousPlating>(8).
                AddRecipeGroup("AnyMythrilBar", 10).
                AddIngredient(ItemID.SoulofFright, 20).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(2, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
