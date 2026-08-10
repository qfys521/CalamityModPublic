using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class UnholyEssence : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 7));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ItemNoGravity[Type] = true;
            ItemID.Sets.SortingPriorityMaterials[Type] = 103;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 36;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Purple;
        }
        public override void Update(WorldItem item, ref float gravity, ref float maxFallSpeed)
        {
            float brightness = (float)Main.rand.Next(90, 111) * 0.01f;
            brightness *= Main.essScale;
            Lighting.AddLight((int)((item.position.X + (float)(Item.width / 2)) / 16f), (int)((item.position.Y + (float)(Item.height / 2)) / 16f), 0.45f * brightness, 0.3f * brightness, 0f * brightness);
        }
    }
}
