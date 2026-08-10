using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.Perforator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags
{
    public class PerforatorBag : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.TreasureBags";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan;
            Item.expert = true;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
        }

        public override bool CanRightClick() => true;

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.4f);

        public override void PostUpdate(WorldItem item) => item.TreasureBagLightAndDust();

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return CalamityUtils.DrawTreasureBagInWorld(item, spriteBatch, ref rotation, ref scale, whoAmI);
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Money
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<PerforatorHive>()));

            // Materials
            itemLoot.Add(ItemID.CrimtaneBar, 1, 15, 20);
            itemLoot.Add(ItemID.Vertebrae, 1, 15, 20);
            itemLoot.AddIf(() => Main.hardMode, ItemID.Ichor, 1, 25, 30);
            itemLoot.Add(ItemID.CrimsonSeeds, 1, 10, 15);

            // Weapons
            itemLoot.Add(DropHelper.CalamityStyle(DropHelper.BagWeaponDropRateFraction, new WeightedItemStack[]
            {
                ModContent.ItemType<Aorta>(),
                ModContent.ItemType<SausageMaker>(),
                ModContent.ItemType<VeinBurster>(),
                ModContent.ItemType<Eviscerator>(),
                ModContent.ItemType<BloodBath>(),
                ModContent.ItemType<FleshOfInfidelity>(),
                ModContent.ItemType<ToothBall>(),
            }));

            // Equipment
            itemLoot.Add(ModContent.ItemType<BloodstainedGlove>(), DropHelper.BagWeaponDropRateFraction);
            itemLoot.Add(ModContent.ItemType<BloodyWormTooth>());
            itemLoot.AddRevBagAccessories();

            // Vanity
            itemLoot.Add(ModContent.ItemType<PerforatorMask>(), 7);
            itemLoot.Add(ModContent.ItemType<BloodyVein>(), 10);
            itemLoot.Add(ModContent.ItemType<ThankYouPainting>(), ThankYouPainting.DropInt);
        }
    }
}
