using System.Collections.Generic;
using CalamityMod.Items.BaseItems;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class Hellkite : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public static readonly SoundStyle SwingSound = new("CalamityMod/Sounds/Item/HellkiteSwing", 2);
        public static readonly SoundStyle SwingSoundBig = new("CalamityMod/Sounds/Item/HellkiteHeavySwing");
        public static readonly SoundStyle HitSoundSmall = new("CalamityMod/Sounds/Item/HellkiteSmallHit", 3);
        public static readonly SoundStyle HitSoundBig = new("CalamityMod/Sounds/Item/HellkiteBigHit", 2);
        public static readonly SoundStyle ChargeSound = new("CalamityMod/Sounds/Item/HellkiteCharge");
        public static readonly SoundStyle FullChargeSound = new("CalamityMod/Sounds/Item/HellkiteFullCharge");
        public override void SetStaticDefaults() => ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        public override void SetDefaults()
        {
            Item.width = 124;
            Item.height = 124;
            Item.damage = 570;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = Item.useTime = 71;
            Item.useTurn = true;
            Item.knockBack = 13f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<HellkiteHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool MeleePrefix() => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().mouseRight)
            {
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 0, 5);
            }
            else
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 0, 0);
            return false;
        }
        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/HellkiteGlow").Value);
        }
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
        }
    }
}
