using System;
using System.Collections.Generic;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Systems
{
    public class WorldSelectionDifficultySystem : ModSystem
    {
        public override void PostSetupContent()
        {
            // Load all of the World Difficulties to the World Difficulty list
            // The difficulty the world uses will be the latest applicable one, so these are added in order from easiest to hardest
            // These colors are all picked from the wiki's colors for each Difficulty combo
            // Calamity's own difficulties aren't added if the Luminance mod is on, as it adds its own world system
            if (ExternalMods.luminance == null)
            {
                WorldDifficulties.Add(new WorldDifficulty(CalamityUtils.GetTextValue("UI.Revengeance"), GetRevengeance, new(211, 42, 42)));
                WorldDifficulties.Add(new WorldDifficulty(CalamityUtils.GetTextValue("UI.Death"), GetDeath, new(192, 64, 219)));
                WorldDifficulties.Add(new WorldDifficulty(CalamityUtils.GetTextValue("UI.Malice"), GetMalice, new(240, 128, 128)));
            }
        }

        public override void SaveWorldHeader(TagCompound tag)
        {
            // Since CalamityWorld is static, and therefore invalid for TryGetHeaderData, make copies for Rev and Death
            tag["RevengeanceMode"] = CalamityWorld.revenge;
            tag["DeathMode"] = CalamityWorld.death;
        }

        public record WorldDifficulty(string name, Func<AWorldListItem, bool> function, Color color);

        public static List<WorldDifficulty> WorldDifficulties = new List<WorldDifficulty>();
        public static bool GetRevengeance(AWorldListItem item)
        {
            if (item.Data.TryGetHeaderData<WorldSelectionDifficultySystem>(out TagCompound tag))
            {
                if (tag.ContainsKey("RevengeanceMode") && tag.GetBool("RevengeanceMode"))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetDeath(AWorldListItem item)
        {
            // Grab data from the listed world
            WorldFileData worldData = item.Data;

            if (item.Data.TryGetHeaderData<WorldSelectionDifficultySystem>(out TagCompound tag))
            {
                if (tag.ContainsKey("DeathMode") && tag.GetBool("DeathMode"))
                {
                    return true;
                }
                else if (tag.ContainsKey("RevengeanceMode") && tag.GetBool("RevengeanceMode"))
                {
                    return worldData.ForTheWorthy;
                }
            }
            return false;
        }

        public static bool GetMalice(AWorldListItem item)
        {
            // Grab data from the listed world
            WorldFileData worldData = item.Data;

            int trueGameMode = worldData.GameMode;
            if (worldData.ForTheWorthy)
            {
                trueGameMode++;
            }

            if (item.Data.TryGetHeaderData<WorldSelectionDifficultySystem>(out TagCompound tag))
            {
                if (tag.ContainsKey("DeathMode") && tag.GetBool("DeathMode") && trueGameMode == 3)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
