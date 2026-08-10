using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.BaseItems;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity
{
    public class LittleE : TransformationAccessory, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override (EquipType, string, string)[] EquipSlots =>
        [
            (EquipType.Head, "BigE", null),
            (EquipType.Body, "BigE", null),
            (EquipType.Legs, "BigE", null),
        ];

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 14));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[ItemID.AlphabetStatueE] = Type;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.vanity = true;
            Item.rare = ItemRarityID.Red;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.Calamity().devItem = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string text = Language.GetTextValue("Mods.CalamityMod.Items.Accessories.LittleE.Tooltip").FormatWith(Main.LocalPlayer.name);       
            if (Item.social)
                tooltips.Insert(1, new(CalamityMod.Instance, "Tooltip", text));
            else
                tooltips[3].Text = text;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            var frame = Item.GetFrame(whoAmI);
            var position = item.Center - Main.screenPosition + Vector2.UnitY * 4;
            var origin = frame.Size() / 2f;
            spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, lightColor, rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
