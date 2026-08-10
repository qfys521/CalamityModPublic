using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.CalClone;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    [LegacyName("BlightedEyeball")]
    public class EyeofDesolation : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 11; // Pirate Map (1 above Mechanical Skull)
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.rare = ItemRarityID.LightPurple;
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
            return !NPC.AnyNPCs(ModContent.NPCType<CalamitasClone>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            CalamityUtils.SpawnBossUsingItem<CalamitasClone>(player, SoundID.Roar);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.HellstoneBar, 10).
                // waffles% stipulation: you can only get 5 essences of havoc from the music box, not 7, and cal clone must be accessible
                AddIngredient<EssenceofHavoc>(5).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
