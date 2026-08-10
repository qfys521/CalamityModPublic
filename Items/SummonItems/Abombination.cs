using CalamityMod.CustomRecipes;
using System;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.PlaguebringerGoliath;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace CalamityMod.Items.SummonItems
{
    [LegacyName("Abomination")]
    public class Abombination : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/PBGSummon");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 17; // Solar Tablet (1 above Lihzahrd Power Cell)
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.rare = ItemRarityID.Yellow;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ZoneJungle && !NPC.AnyNPCs(ModContent.NPCType<PlaguebringerGoliath>()) && !BossRushEvent.BossRushActive;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 3);

        public override bool? UseItem(Player player)
        {
            CalamityUtils.SpawnBossUsingItem<PlaguebringerGoliath>(player, UseSound);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlagueCellCanister>(20).
                AddIngredient<MysteriousCircuitry>(12).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(3, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
