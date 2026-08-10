using CalamityMod.Buffs.Summon;
using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("StatigelHood")]
    public class StatigelHeadSummon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int MinionSlotBoost = 1;
        public static float SummonDamageBoost = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionSlotBoost, SummonDamageBoost.ToPercent());

        // Set Bonus
        public static float SetBonusSummonDamageBoost = 0.15f;
        public static int SlimeDamage = 18;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 4; //20
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<StatigelArmor>() && legs.type == ModContent.ItemType<StatigelGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusSummonDamageBoost.ToPercent(), StatigelArmor.SetBonusJumpSpeedBoost.ToJumpSpeedPercent());
            var modPlayer = player.Calamity();
            modPlayer.slimeGod = true;
            player.GetJumpState<StatigelJump>().Enable();
            Player.jumpHeight += (int)(StatigelArmor.SetBonusJumpHeightPercentBoost * 15);
            player.jumpSpeedBoost += StatigelArmor.SetBonusJumpSpeedBoost;
            player.GetDamage<SummonDamageClass>() += SetBonusSummonDamageBoost;
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                if (player.FindBuffIndex(ModContent.BuffType<BabyPaladinBuff>()) == -1)
                    player.AddBuff(ModContent.BuffType<BabyPaladinBuff>(), 3600);

                int minionID = -1;
                int minionDamage = (int)player.GetTotalDamage<SummonDamageClass>().ApplyTo(SlimeDamage);
                if (player.ownedProjectileCounts[ModContent.ProjectileType<StatigelBlightedSlime>()] < 1)
                    minionID = Projectile.NewProjectile(source, player.Center, -Vector2.UnitY, ModContent.ProjectileType<StatigelBlightedSlime>(), minionDamage, 0f, Main.myPlayer);

                if (Main.projectile.IndexInRange(minionID))
                    Main.projectile[minionID].originalDamage = SlimeDamage;
            }
        }

        public override void UpdateEquip(Player player)
        {
            player.maxMinions += MinionSlotBoost;
            player.GetDamage<SummonDamageClass>() += SummonDamageBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(8).
                AddIngredient<BlightedGel>(8).
                AddTile(TileID.Solidifier).
                SortBeforeFirstRecipesOf(ModContent.ItemType<StatigelHeadRogue>()).
                Register();
        }
    }
}
