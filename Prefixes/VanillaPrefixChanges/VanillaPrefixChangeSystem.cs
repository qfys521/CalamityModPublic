using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public sealed class VanillaPrefixChangeSystem : ModSystem
    {
        public static bool PrefixReworkEnabled = true;

        public static readonly Dictionary<int, VanillaPrefixChange> PrefixChanges = [];

        public override void Load()
        {
            On_Player.GrantPrefixBenefits += OnGrantBenefits;
            IL_Item.Prefix_int_refBoolean += VanillaPrefixValueOverride;
        }

        public override void Unload()
        {
            PrefixChanges.Clear();
        }

        private void VanillaPrefixValueOverride(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(x => x.MatchCallOrCallvirt<ModPrefix>(nameof(ModPrefix.ModifyValue))))
            {
                CalamityMod.Log.ILFailure("Vanilla Prefix Override", "Unable to locate callvirt (ModPrefix.ModifyValue)");
                return;
            }

            if (!cursor.Prev.MatchLdloca(out var multLocaIdx))
            {
                CalamityMod.Log.ILFailure("Vanilla Prefix Override", "Unable to locate Ldloca (mult)");
                return;
            }

            if (!cursor.TryGotoPrev(x => x.MatchLdsfld<PrefixID>(nameof(PrefixID.Count))))
            {
                CalamityMod.Log.ILFailure("Vanilla Prefix Override", "Unable to locate Ldsfld (PrefixID.Count)");
                return;
            }

            if (!cursor.Prev.MatchLdloc(out var prefixLocaIdx))
            {
                CalamityMod.Log.ILFailure("Vanilla Prefix Override", "Unable to locate Ldloc (prefix)");
                return;
            }

            cursor.GotoPrev(MoveType.AfterLabel); // Emit next to label
            cursor.EmitLdloc(prefixLocaIdx); // push prefixID
            cursor.EmitLdloca(multLocaIdx); // push &valueMult
            cursor.EmitDelegate((int prefixID, ref float value) =>
            {
                if (!PrefixReworkEnabled)
                    return;

                if (PrefixChanges.TryGetValue(prefixID, out var prefixChange))
                {
                    prefixChange.ModifyValue(ref value);
                }
            });
        }

        private void OnGrantBenefits(On_Player.orig_GrantPrefixBenefits orig, Player self, Item item)
        {
            if (!PrefixReworkEnabled)
            {
                orig(self, item);
            }
            else if (PrefixChanges.TryGetValue(item.prefix, out var prefixChange))
            {
                var stats = prefixChange.PopulateStats();
                while (stats.MoveNext())
                {
                    var stat = stats.Current;
                    stat.ApplyEffects(self);
                }
                prefixChange.PostApplyEffects(self);
            }
            else
            {
                orig(self, item);
            }
        }

        public sealed class VanillaPrefixChangeTooltipModify : GlobalItem
        {
            public override bool InstancePerEntity => false;

            public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
            {
                if (!PrefixReworkEnabled)
                    return;

                if (!PrefixChanges.TryGetValue(item.prefix, out var change))
                    return;

                var tooltip = tooltips.FirstOrDefault(x => x.Name.Equals(change.TargetTooltipName));
                if (tooltip == null)
                    return;

                tooltip.Text = string.Empty;

                var stats = change.PopulateStats();
                while (stats.MoveNext())
                {
                    var stat = stats.Current;
                    stat.ModifyTooltip(tooltip);
                }
                change.PostModifyTooltip(tooltip);
                tooltip.Text = tooltip.Text.Trim();
            }
        }
    }
}
