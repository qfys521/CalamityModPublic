using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.Items.Placeables.FurnitureCosmilite;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags
{
    public class DevourerofGodsBag : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.TreasureBags";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.BossBag[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan;
            Item.expert = true;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return CalamityUtils.DrawTreasureBagInWorld(item, spriteBatch, ref rotation, ref scale, whoAmI);
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/TreasureBags/DevourerofGodsBagGlow").Value);
        }

        public override bool CanRightClick() => true;

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.4f);

        public override void PostUpdate(WorldItem item)
        {
            CalamityUtils.ForceItemIntoWorld(item);
            item.TreasureBagLightAndDust();
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Money
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<DevourerofGodsHead>()));

            // Materials
            itemLoot.Add(ModContent.ItemType<CosmiliteBar>(), 1, 75, 90);
            itemLoot.Add(ModContent.ItemType<CosmiliteBrick>(), 1, 200, 320);

            // Weapons
            itemLoot.Add(DropHelper.CalamityStyle(DropHelper.BagWeaponDropRateFraction, new int[]
            {
                ModContent.ItemType<MawOfInfinity>(),
                ModContent.ItemType<TheObliterator>(),
                ModContent.ItemType<ThreadOfEradication>(),
                ModContent.ItemType<HyperdeathRiftScepter>(),
                ModContent.ItemType<VoidEaterMarionette>(),
                ModContent.ItemType<DimensionTearingDisk>()
            }));
            itemLoot.Add(ModContent.ItemType<CosmicDischarge>(), 10);

            // Equipment
            itemLoot.Add(ModContent.ItemType<NebulousCore>());
            itemLoot.AddRevBagAccessories();

            // Vanity
            itemLoot.Add(ModContent.ItemType<DevourerofGodsMask>(), 7);
            itemLoot.AddIf((info) => CalamityWorld.death && info.player.difficulty == 2, ModContent.ItemType<CosmicPlushie>());
            itemLoot.Add(ModContent.ItemType<ThankYouPainting>(), ThankYouPainting.DropInt);
        }
    }
}
