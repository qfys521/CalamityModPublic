using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class PiggyGold : Piggy
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
            NPCID.Sets.IsGoldCritter[Type] = true;
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.rarity = 3;
            NPC.catchItem = (short)ModContent.ItemType<PiggyGoldItem>();
            Banner = 0;
            BannerItem = 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // All gold critters have the same Bestiary entry.
            string key = Lang.GetNPCName(NPCID.GoldBunny).Key.Replace("NPCName.", "");
            string flavorText = "Bestiary_FlavorText.npc_" + key;
            bestiaryEntry.AddTags(
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(flavorText));
        }

        public override void AI()
        {
            base.AI();
            NPC.ProduceGoldCritterDust();
        }
    }
}
