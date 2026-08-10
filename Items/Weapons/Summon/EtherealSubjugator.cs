using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class EtherealSubjugator : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 70;
            Item.damage = 160;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<Phantom>();
            Item.shoot = ModContent.ProjectileType<PhantomGuy>();
            Item.knockBack = 1f;

            Item.useAnimation = Item.useTime = 24;
            Item.mana = 10;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item82;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame) => player.itemLocation += new Vector2(-13f * player.direction, -15f);

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/EtherealSubjugatorGlow").Value);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI);
            minion.originalDamage = Item.damage;
            return false;
        }
    }
}
