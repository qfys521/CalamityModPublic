using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    [LegacyName("ChickenEgg", "JungleDragonEgg")]
    public class YharonEgg : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 19; // Celestial Sigil
        }

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 60;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<Yharon>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            CalamityUtils.SpawnBossUsingItem<Yharon>(player, Yharon.FireSound);
            return true;
        }

        public override void UseItemFrame(Player player)
        {
            player.itemLocation = (Vector2)player.HandPosition + new Vector2(10 * -player.direction, 20);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<LifeAlloy>(10).
                AddIngredient<EffulgentFeather>(15).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
