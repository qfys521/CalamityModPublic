using CalamityMod.Events;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems.Invasion
{
    public class CausticTear : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 4; // Goblin Battle Standard
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 28;
            Item.consumable = true;
            Item.maxStack = Item.CommonMaxStack;
            Item.useAnimation = Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.EventItem;
        }

        public override bool CanUseItem(Player player)
        {
            return !AcidRainEvent.AcidRainEventIsOngoing;
        }

        public override bool? UseItem(Player player)
        {
            // Only Single Player client and Server should call this!
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                AcidRainEvent.TryStartEvent(forceRain: true);
                // TryStartEvent already syncs the world data
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SulphuricScale>().
                AddCondition(Condition.NearWater).
                Register();
        }
    }
}
