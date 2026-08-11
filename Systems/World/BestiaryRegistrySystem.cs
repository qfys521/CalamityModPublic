using System.Collections.Generic;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.Deconstructors;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems
{
    public class BestiaryRegistrySystem : ModSystem
    {
        public override void PostSetupContent()
        {
            On_BestiaryDatabaseNPCsPopulator.AddEmptyEntries_CrittersAndEnemies_Automated += ForciblyAddEmptyEntriesForCritters;
            On_NPCWasNearPlayerTracker.ScanWorldForFinds += ForciblySetWasSeenByPlayer;

            // Manually register variants post-initiailization
            foreach (var pair in CalamityNPCSets.CountVariantsAsTheSameInBestiary)
                ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[pair.Key] = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[pair.Value];
        }

        private void ForciblyAddEmptyEntriesForCritters(On_BestiaryDatabaseNPCsPopulator.orig_AddEmptyEntries_CrittersAndEnemies_Automated orig, BestiaryDatabaseNPCsPopulator self)
        {
            orig(self);

            // Run through all entries again and remove the empty Enemy entries that are added by tMod itself.
            // Afterwards, mnually add empty Critter entries for all NPCs within the ID set.
            HashSet<int> exclusions = TerrariaInternals.GetBestiaryExclusions();
            foreach (KeyValuePair<int, NPC> pair in ContentSamples.NpcsByNetId)
            {
                if (!exclusions.Contains(pair.Key))
                {
                    if (CalamityNPCSets.ForciblyRegisterAsCritterInBestiary.Contains(pair.Value.type))
                    {
                        BestiaryDatabase database = TerrariaInternals.CurrentBestiaryDatabase;
                        BestiaryEntry enemyEntry = database.FindEntryByNPCID(pair.Value.netID);
                        if (enemyEntry.Info.Count > 0 && enemyEntry.UIInfoProvider is not CritterUICollectionInfoProvider)
                        {
                            database.Entries.Remove(enemyEntry);
                            NPCLoader.SetBestiary(pair.Value, database, TerrariaInternals.RegisterBestiaryEntry(self, BestiaryEntry.Critter(pair.Key)));
                        }
                    }
                }
            }
        }

        private void ForciblySetWasSeenByPlayer(On_NPCWasNearPlayerTracker.orig_ScanWorldForFinds orig, NPCWasNearPlayerTracker self)
        {
            orig(self);

            // Allow NPCs with manully added empty critter entries to be registered by player proximity.
            List<int> seenNearPlayer = TerrariaInternals.SeenNearbyNpcNetIds(self);
            List<Microsoft.Xna.Framework.Rectangle> playerHitboxes = TerrariaInternals.BestiaryPlayerHitboxes(self);
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!CalamityNPCSets.ForciblyRegisterAsCritterInBestiary.Contains(npc.type) || seenNearPlayer.Contains(npc.netID))
                    continue;

                for (int i = 0; i < playerHitboxes.Count; i++)
                {
                    if (npc.Hitbox.Intersects(playerHitboxes[i]))
                    {
                        seenNearPlayer.Add(npc.netID);
                        self.RegisterWasNearby(npc);
                    }
                }
            }
        }
    }
}
