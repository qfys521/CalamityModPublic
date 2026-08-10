using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.ProfanedGuardians;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    public class ProfanedShard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianSpawn");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 19; // Celestial Sigil
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Purple;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<ProfanedGuardianCommander>()) && (player.ZoneHallow || player.ZoneUnderworldHeight) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            CalamityUtils.SpawnBossUsingItem<ProfanedGuardianCommander>(player, UseSound);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<UnholyEssence>(25).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
