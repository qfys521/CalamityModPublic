using CalamityMod.Buffs.StatBuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class Affliction : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int RegenBoost = 1;
        public static int MaxLifeBoostPercent = 10;
        public static float DamageReductionBoost = 0.07f;
        public static int DefenseBoost = 9;
        public static float DamageBoost = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), MaxLifeBoostPercent, DamageReductionBoost.ToPercent(), DefenseBoost, DamageBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 44;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.expert = true;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/Affliction").Value);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.affliction = true;
            if (player.whoAmI != Main.myPlayer && player.miscCounter % 10 == 0)
            {
                if (Main.LocalPlayer.team == player.team && player.team != 0)
                {
                    Main.LocalPlayer.AddBuff(ModContent.BuffType<Afflicted>(), 20);
                }
            }
        }
    }
}
