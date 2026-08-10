using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.BrimstoneElemental;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    public class CharredIdol : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Custom/BrimstoneElemental/BrimstoneSpawn");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 10; // Mechanical Skull
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.rare = ItemRarityID.Pink;
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
            CalamityPlayer modPlayer = player.Calamity();
            return modPlayer.ZoneCalamity && !NPC.AnyNPCs(ModContent.NPCType<BrimstoneElemental>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            CalamityUtils.SpawnBossUsingItem<BrimstoneElemental>(player, UseSound);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UnholyCore>(5).
                AddIngredient<EssenceofHavoc>(7).
                AddIngredient(ItemID.SoulofNight, 5).
                AddTile(TileID.Hellforge).
                Register();
        }
    }
}
