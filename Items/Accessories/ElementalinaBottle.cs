using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("WifeinaBottle")]
    public class ElementalinaBottle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public const int ElementalDamage = 45;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override bool CanEquipAccessory(Player player, int slot, bool modded) => !player.Calamity().allElementals.HasValue;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.sandElemental = !hideVisual;
            SpawnElemental(player);
        }

        public override void UpdateVanity(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.sandElementalVanity = true;
            SpawnElemental(player);
        }

        public void SpawnElemental(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<SandElemental>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<SandElemental>(), 3600);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<SandElementalMinion>()] < 1)
                {
                    int damage = (int)player.GetTotalDamage<SummonDamageClass>().ApplyTo(ElementalDamage);

                    var p = Projectile.NewProjectileDirect(player.GetSource_Accessory(Item), player.Center, -Vector2.UnitY, ModContent.ProjectileType<SandElementalMinion>(), damage, 2f, Main.myPlayer);
                    p.originalDamage = ElementalDamage;
                }
            }
        }
    }
}
