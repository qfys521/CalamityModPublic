using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Eternity : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public const int BaseDamage = 840;
        public const int ExplosionDamage = 8400;
        public const int MaxHomers = 40;
        public const int DustID = 16;
        public static readonly Color BlueColor = new Color(34, 34, 160);
        public static readonly Color PinkColor = new Color(169, 30, 184);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 40;
            Item.damage = BaseDamage;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 300;
            Item.useAnimation = Item.useTime = 120;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<EternityBook>();
            Item.shootSpeed = 0f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip1");
            if (line != null)
                line.Color = new Color((int)MathHelper.Lerp(156f, 255f, Main.DiscoR / 256f), 108, 251);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Heresy>().
                AddIngredient<ShadowspecBar>(5).
                AddIngredient<DarkPlasma>(20).
                AddIngredient(ItemID.UnicornHorn, 5).
                AddTile<DraedonsForge>().
                Register();
        }

        public static Color RarityColor()
        {
            List<Color> colorSet = [
                new Color(188, 192, 193), // white
                new Color(157, 100, 183), // purple
                new Color(249, 166, 77), // honey-ish orange
                new Color(255, 105, 234), // pink
                new Color(67, 204, 219), // sky blue
                new Color(249, 245, 99), // bright yellow
                new Color(236, 168, 247), // purplish pink
            ];
            int colorIndex = (int)(Main.GlobalTimeWrappedHourly / 2 % colorSet.Count);
            Color currentColor = colorSet[colorIndex];
            Color nextColor = colorSet[(colorIndex + 1) % colorSet.Count];
            return Color.Lerp(currentColor, nextColor, Main.GlobalTimeWrappedHourly % 2f > 1f ? 1f : Main.GlobalTimeWrappedHourly % 1f);
        }
    }
}
