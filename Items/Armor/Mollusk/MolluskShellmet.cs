using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Mollusk
{
    [AutoloadEquip(EquipType.Head)]
    public class MolluskShellmet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static float DamageBoost = 0.05f;
        public static int CritBoost = 4;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost);

        // Set Bonus
        public static float SetBonusDR = 0.1f;
        public static int ShellfishDamage = 140;

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.defense = 15;
        }

        public override void UpdateEquip(Player player)
        {
            player.ignoreWater = true;
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
            player.Calamity().molluskHelmet = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<MolluskShellplate>() && legs.type == ModContent.ItemType<MolluskShelleggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusDR.ToPercent());
            player.endurance += SetBonusDR;
            player.maxMinions += 4; // These are allocated for the Shellfish minions; there is no net change in minion slots.
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_ItemUse(Item);
                if (player.FindBuffIndex(ModContent.BuffType<ShellfishBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<ShellfishBuff>(), 3600);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Shellfish>()] < 2)
                {
                    Projectile clam = Projectile.NewProjectileDirect(source, player.Center, -Vector2.UnitY, ModContent.ProjectileType<Shellfish>(), ShellfishDamage, 0f, player.whoAmI);
                    clam.originalDamage = ShellfishDamage;
                }
            }
            player.Calamity().wearingRogueArmor = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MolluskHusk>(6).
                AddIngredient<SeaPrism>(15).
                AddTile(TileID.Anvils).
                SortBeforeFirstRecipesOf(ModContent.ItemType<MolluskShelleggings>()).
                Register();
        }
    }
}
