using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    [LegacyName("HellcasterFragment")]
    public class YharonSoulFragment : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;

        }

        public override void Update(WorldItem item, ref float gravity, ref float maxFallSpeed)
        {
            float brightness = (float)Main.rand.Next(90, 111) * 0.01f;
            brightness *= Main.essScale;
            Lighting.AddLight((int)((item.position.X + (float)(Item.width / 2)) / 16f), (int)((item.position.Y + (float)(Item.height / 2)) / 16f), 0.5f * brightness, 0.3f * brightness, 0.05f * brightness);
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 14;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }
    }
}
