using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.BaseItems
{
    /// <summary>
    /// A item that only exists as a held item. Useful if for example, it is used as a mean to temporarily replace the players attack ability.
    /// </summary>
    public abstract class HeldOnlyItem : ModItem
    {
        public virtual bool VisibleInUI => false;

        public override void PostUpdate(WorldItem item)
        {
            //Die if in the world
            Item.type = ItemID.None;
            Item.stack = 0;
        }

        public override bool CanPickup(WorldItem item, Player player) => false;

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => false;

        #region Hooks
        private sealed class HeldOnlyItemHooks : ModSystem
        {
            public override void Load()
            {
                Terraria.On_Player.dropItemCheck += DontDropCoolStuff;
                Terraria.UI.On_ItemSlot.LeftClick += LockMouseToSpecialItem;
                Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
            }

            private static void DrawSpecial(Terraria.UI.On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb, Item[] inv, int context, int slot, Vector2 position, Color color)
            {
                if (inv[slot].ModItem is HeldOnlyItem heldOnlyItem && !heldOnlyItem.VisibleInUI)
                    return;

                else
                    orig(sb, inv, context, slot, position, color);
            }

            private static void LockMouseToSpecialItem(Terraria.UI.On_ItemSlot.orig_LeftClick orig, Item[] inv, int context, int slot)
            {
                if (Main.mouseItem.ModItem is not HeldOnlyItem)
                    orig(inv, context, slot);
            }

            //https://media.discordapp.net/attachments/458432092301295618/993675527539916850/unknown.png
            private static void DontDropCoolStuff(Terraria.On_Player.orig_dropItemCheck orig, Terraria.Player self)
            {
                if (Main.mouseItem.ModItem is not HeldOnlyItem)
                    orig(self);
            }
        }
        #endregion
    }
}
