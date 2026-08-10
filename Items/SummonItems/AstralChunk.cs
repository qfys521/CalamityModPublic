using CalamityMod.CustomRecipes;
using System;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.AstrumAureus;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace CalamityMod.Items.SummonItems
{
    public class AstralChunk : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Custom/AstrumAureus/AstrumAureusSpawn");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 12; // Truffle Worm
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Lime;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 2);

        public override bool CanUseItem(Player player)
        {
            return player.Calamity().ZoneAstral && !NPC.AnyNPCs(ModContent.NPCType<AstrumAureus>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            int posX = (int)(player.position.X + Main.rand.Next(-250, 251));
            int posY = (int)(player.position.Y - 500f);
            int bossToSpawn = ModContent.NPCType<AstrumAureus>();
            CalamityUtils.SpawnBossOnPosUsingItem(player, bossToSpawn, posX, posY, UseSound);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<DubiousPlating>(8).
                AddIngredient(ItemID.FallenStar, 20).
                AddIngredient<StarblightSoot>(30).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(2, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
