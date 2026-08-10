using System.Collections.Generic;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    public class RedLightningContainer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override LocalizedText Tooltip => CalamityUtils.GetText($"{LocalizationCategory}.RageBoosterTooltip").WithFormatArgs(BalancingConstants.RageDurationPerBooster.FramesToSeconds());

        public int frameCounter = 0;
        public int frame = 0;
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 40;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = SoundID.Item122;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Purple;
            Item.SetRevExclusive();
        }

        public static bool HasConsumedBefore(Player player) => player.Calamity().rageBoostThree;

        public override bool CanUseItem(Player player)
        {
            if (HasConsumedBefore(player))
            {
                // Refuse Text can be added on here
                return false;
            }

            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/PermanentBoosters/RedLightningContainer_Animated").Value;
            spriteBatch.Draw(texture, position, Item.GetCurrentFrame(ref frame, ref frameCounter, 5, 6), Color.White, 0f, origin, scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/PermanentBoosters/RedLightningContainer_Animated").Value;
            spriteBatch.Draw(texture, item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 5, 6), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/PermanentBoosters/RedLightningContainerGlow").Value;
            spriteBatch.Draw(texture, item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 5, 6, false), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                CalamityPlayer modPlayer = player.Calamity();
                modPlayer.rageBoostThree = true;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (HasConsumedBefore(Main.LocalPlayer))
                list.AddConsumedTooltip("Tooltip0");
        }
    }
}
