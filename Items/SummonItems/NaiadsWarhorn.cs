using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Placeables.FurnitureDriftwood;
using CalamityMod.NPCs.Leviathan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    public class NaiadsWarhorn : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public static readonly SoundStyle HornSound = new("CalamityMod/Sounds/Item/LeviathanHornSound") { Volume = 0.55f };
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMiscImportants[Type] = 12; // Truffle Worm
        }

        public override void SetDefaults()
        {
            Item.width = 108;
            Item.height = 68;
            Item.rare = ItemRarityID.Lime;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.consumable = false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(5, 8);

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ZoneBeach && !player.Calamity().ZoneSulphur && !NPC.AnyNPCs(ModContent.NPCType<Anahita>()) && !NPC.AnyNPCs(ModContent.NPCType<Leviathan>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            int posX = (int)player.position.X;
            int posY = (int)(player.position.Y - 700f);
            int bossToSpawn = ModContent.NPCType<Anahita>();
            CalamityUtils.SpawnBossOnPosUsingItem(player, bossToSpawn, posX, posY, HornSound);
            return true;
        }

        // Makes it draw lower in the world so that it doesn't look weird with the cloth at the bottom
        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, item.Center - Main.screenPosition + Vector2.UnitY * 20f, null, lightColor, rotation, tex.Size() / 2f, scale, 0, 0);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(10).
                AddIngredient<AbyssGravel>(15).
                AddIngredient<Lumenyl>(10).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
