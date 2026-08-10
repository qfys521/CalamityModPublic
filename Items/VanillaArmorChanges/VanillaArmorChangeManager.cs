using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class VanillaArmorChangeManager : ModSystem
    {
        internal static List<VanillaArmorChange> ArmorChanges = [];

        public override void Unload()
        {
            ArmorChanges = null;
        }

        public static void ApplySetBonusTooltipChanges(Item checkItem, ref string setBonusText)
        {
            for (int i = 0; i < ArmorChanges.Count; i++)
            {
                bool isValidHeadPiece = (ArmorChanges[i].HeadPieceID ?? ItemID.None) == checkItem.type ||
                    ArmorChanges[i].AlternativeHeadPieceIDs.Contains(checkItem.type);
                bool isValidBodyPiece = (ArmorChanges[i].BodyPieceID ?? ItemID.None) == checkItem.type ||
                    ArmorChanges[i].AlternativeBodyPieceIDs.Contains(checkItem.type);
                bool isValidLegPiece = (ArmorChanges[i].LegPieceID ?? ItemID.None) == checkItem.type ||
                    ArmorChanges[i].AlternativeLegPieceIDs.Contains(checkItem.type);
                if ((isValidHeadPiece || isValidBodyPiece || isValidLegPiece) && !ArmorChanges[i].NeedsToCreateSetBonusTextManually)
                    ArmorChanges[i].UpdateSetBonusText(ref setBonusText);
            }
        }

        public static void CreateTooltipManuallyAsNecessary(Player player)
        {
            for (int i = 0; i < ArmorChanges.Count; i++)
            {
                if (ArmorChanges[i].IsWearingEntireSet(player) && ArmorChanges[i].NeedsToCreateSetBonusTextManually)
                {
                    ArmorChanges[i].UpdateSetBonusText(ref player.setBonus);
                    return;
                }
            }
        }

        public static string GetSetBonusName(Player player)
        {
            return GetSetBonusName(player.armor[0], player.armor[1], player.armor[2]);
        }

        public static string GetSetBonusName(Item head, Item body, Item legs)
        {
            for (int i = 0; i < ArmorChanges.Count; i++)
            {
                if (ArmorChanges[i].IsArmorSet(head, body, legs))
                    return ArmorChanges[i].ArmorSetName;
            }
            return string.Empty;
        }

        public static void ApplyPotentialEffectsTo(Player player)
        {
            // Look through every armor change, apply individual set piece effects if pieces are being worn, and
            // if the entire set is worn, apply the set bonus.
            for (int i = 0; i < ArmorChanges.Count; i++)
            {
                ArmorChanges[i].ApplyIndividualPieceEffects(player);
                if (ArmorChanges[i].IsWearingEntireSet(player))
                    ArmorChanges[i].ApplyArmorSetBonus(player);
            }
        }
    }
}
