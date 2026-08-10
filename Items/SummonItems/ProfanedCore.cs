using System.Collections.Generic;
using System.Linq;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    [LegacyName("ProfanedCoreUnlimited")]
    public class ProfanedCore : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 19; // Celestial Sigil
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Purple;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<Providence>()) && (player.ZoneHallow || player.ZoneUnderworldHeight) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            int posX = (int)player.position.X;
            int posY = (int)(player.position.Y - 100f);
            int bossToSpawn = ModContent.NPCType<Providence>();
            NPC prov = CalamityUtils.SpawnBossOnPosUsingItem(player, bossToSpawn, posX, posY, Providence.SpawnSound);
            if (Main.getGoodWorld)
            {
                (prov.ModNPC as Providence).hasBeenGivenFullPower = true;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<HolyProfanedCore>(), 0, 0);
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string enrageTooltip = Main.getGoodWorld ? "\n" + this.GetLocalizedValue("EnrageText") : "";
            tooltips.FindAndReplace("[ENRAGE]", enrageTooltip);
        }
    }
}
