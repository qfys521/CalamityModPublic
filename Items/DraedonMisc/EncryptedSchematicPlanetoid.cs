using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.PlaceableTurrets;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Rarities;
using CalamityMod.UI;
using CalamityMod.UI.DraedonLogs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.DraedonMisc
{
    public class EncryptedSchematicPlanetoid : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.DraedonItems";
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.maxStack = 1;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override void UpdateInventory(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && !RecipeUnlockHandler.HasFoundPlanetoidSchematic)
            {
                RecipeUnlockHandler.HasFoundPlanetoidSchematic = true;
                CalamityNetcode.SyncWorld();
            }
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
            if (RecipeUnlockHandler.HasUnlockedT2ArsenalRecipes)
            {
                if (line != null)
                    line.Text = CalamityUtils.GetTextValue($"{LocalizationCategory}.SchematicUnlocked");
                int insertIndex = list.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
                if (insertIndex != -1)
                {
                    int aureusItem = ModContent.ItemType<AstralChunk>();
                    TooltipLine aureusDisplay = new TooltipLine(this.Mod, "CalamityMod:MeleeDisplay", $"[i:{aureusItem}] {CalamityUtils.GetItemName(aureusItem)}");
                    aureusDisplay.Color = new Color(149, 169, 182);
                    list.Insert(insertIndex + 1, aureusDisplay);

                    int meleeItem = ModContent.ItemType<HydraulicVoltCrasher>();
                    TooltipLine meleeDisplay = new TooltipLine(this.Mod, "CalamityMod:MeleeDisplay", $"[i:{meleeItem}] {CalamityUtils.GetItemName(meleeItem)}");
                    meleeDisplay.Color = new Color(31, 242, 245);
                    list.Insert(insertIndex + 1, meleeDisplay);

                    int rangedItem = ModContent.ItemType<HolofibreImmolator>();
                    TooltipLine rangedDisplay = new TooltipLine(this.Mod, "CalamityMod:RangedDisplay", $"[i:{rangedItem}] {CalamityUtils.GetItemName(rangedItem)}");
                    rangedDisplay.Color = new Color(149, 243, 43);
                    list.Insert(insertIndex + 2, rangedDisplay);

                    int mageItem = ModContent.ItemType<Vulcan>();
                    TooltipLine mageDisplay = new TooltipLine(this.Mod, "CalamityMod:MageDisplay", $"[i:{mageItem}] {CalamityUtils.GetItemName(mageItem)}");
                    mageDisplay.Color = new Color(236, 255, 31);
                    list.Insert(insertIndex + 3, mageDisplay);

                    int summonItem = ModContent.ItemType<MountedScanner>();
                    TooltipLine summonDisplay = new TooltipLine(this.Mod, "CalamityMod:SummonDisplay", $"[i:{summonItem}] {CalamityUtils.GetItemName(summonItem)}");
                    summonDisplay.Color = new Color(255, 64, 31);
                    list.Insert(insertIndex + 4, summonDisplay);

                    int rogueItem = ModContent.ItemType<PulseGrenade>();
                    TooltipLine rogueDisplay = new TooltipLine(this.Mod, "CalamityMod:RogueDisplay", $"[i:{rogueItem}] {CalamityUtils.GetItemName(rogueItem)}");
                    rogueDisplay.Color = new Color(201, 41, 255);
                    list.Insert(insertIndex + 5, rogueDisplay);

                    int turretFireItem = ModContent.ItemType<FireTurret>();
                    TooltipLine turretFireDisplay = new TooltipLine(this.Mod, "CalamityMod:CodeDisplay", $"[i:{turretFireItem}] {CalamityUtils.GetItemName(turretFireItem)}");
                    turretFireDisplay.Color = new Color(165, 118, 104);
                    list.Insert(insertIndex + 6, turretFireDisplay);

                    int turretIceItem = ModContent.ItemType<IceTurret>();
                    TooltipLine turretIceDisplay = new TooltipLine(this.Mod, "CalamityMod:CodeDisplay", $"[i:{turretIceItem}] {CalamityUtils.GetItemName(turretIceItem)}");
                    turretIceDisplay.Color = new Color(165, 118, 104);
                    list.Insert(insertIndex + 7, turretIceDisplay);

                    int turretLaserItem = ModContent.ItemType<LaserTurret>();
                    TooltipLine turretLaserDisplay = new TooltipLine(this.Mod, "CalamityMod:CodeDisplay", $"[i:{turretLaserItem}] {CalamityUtils.GetItemName(turretLaserItem)}");
                    turretLaserDisplay.Color = new Color(165, 118, 104);
                    list.Insert(insertIndex + 8, turretLaserDisplay);

                    int codeItem = ModContent.ItemType<LongRangedSensorArray>();
                    TooltipLine machineDisplay = new TooltipLine(this.Mod, "CalamityMod:CodeDisplay", $"[i:{codeItem}] {CalamityUtils.GetItemName(codeItem)}");
                    machineDisplay.Color = new Color(165, 118, 104);
                    list.Insert(insertIndex + 9, machineDisplay);
                }
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(10).
                AddIngredient<DubiousPlating>(10).
                AddIngredient(ItemID.Glass, 50).
                AddCondition(SchematicRecipe.ConstructRecipeCondition("Planetoid", out Func<bool> condition), condition).
                AddTile(TileID.Anvils).
                Register();
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI && RecipeUnlockHandler.HasUnlockedT2ArsenalRecipes)
            {
                PopupGUIManager.FlipActivityOfGUIWithType(typeof(DraedonSchematicPlanetoidGUI));
            }
            return true;
        }
    }
}
