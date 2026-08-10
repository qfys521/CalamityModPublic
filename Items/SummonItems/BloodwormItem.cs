using System.Collections.Generic;
using System.Linq;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    public class BloodwormItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";

        public const int SpoofBaitNumber = 4444;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 19; // Celestial Sigil
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = Item.CommonMaxStack;
            // Why spoof this in the tooltip?
            // To fix bugs related to how bait power affects hooking fish.
            // Vanilla gets around this for Truffle Worm with a hundred hardcoded checks but we don't have the full luxury to do that.
            Item.bait = 50;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.makeNPC = (short)ModContent.NPCType<BloodwormNormal>();

            Item.value = Item.sellPrice(gold: 20); // 2x the sell price of Truffle Worms; also sold by Sea King for a custom (2x) price
            Item.rare = ModContent.RarityType<PureGreen>();
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine bait = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "BaitPower");
            bait?.Text = Language.GetTextValue("GameUI.BaitPower", SpoofBaitNumber);
        }
    }
}
