using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Tools
{
    public class BallAndChain : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        public static Asset<Texture2D> DisabledSprite;
        public override void Load() => DisabledSprite = ModContent.Request<Texture2D>("CalamityMod/Items/Tools/BallAndChainDisabled");
        
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.rare = ItemRarityID.Blue;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
        }

        #region Toggling
        public bool Enabled = true;

        public override ModItem Clone(Item item)
        {
            var clone = (BallAndChain)base.Clone(item);
            clone.Enabled = Enabled;
            return clone;
        }

        public override void SaveData(TagCompound tag) => tag.Add("blockerEnabled", Enabled);
        public override void LoadData(TagCompound tag) => Enabled = tag.GetBool("blockerEnabled");
        public override void NetSend(BinaryWriter writer) => writer.Write(Enabled);
        public override void NetReceive(BinaryReader reader) => Enabled = reader.ReadBoolean();

        public override bool CanRightClick() => true;
        public override bool ConsumeItem(Player player) => false;
        public override void RightClick(Player player)
        {
            Enabled = !Enabled;
            Item.NetStateChanged();
        }
        #endregion

        public override bool CanUseItem(Player player) => false;
        public override void UpdateInventory(Player player) => player.Calamity().blockAllDashes |= Enabled;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string text = this.GetLocalizedValue(Enabled ? "TooltipEnabled" : "TooltipDisabled");
            tooltips.FindAndReplace("[STATE]", text);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = Enabled ? TextureAssets.Item[Type].Value : DisabledSprite.Value;
            CalamityUtils.DrawInventoryCustomScale(spriteBatch, tex, position, frame, drawColor, itemColor, origin, scale, 0.8f);
            return false;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = Enabled ? TextureAssets.Item[Type].Value : DisabledSprite.Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, item.Bottom - Main.screenPosition - Vector2.UnitY * origin.Y, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddRecipeGroup("IronBar", 10).
                AddIngredient(ItemID.Chain).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
