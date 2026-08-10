using System.Collections.Generic;
using CalamityMod.Items;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes
{
    [LegacyName("PointyWeaponPrefix")]
    public class Pointy : RogueWeaponPrefix
    {
        public override float damageMult => 1.1f;
    }

    [LegacyName("SharpWeaponPrefix")]
    public class Sharp : RogueWeaponPrefix
    {
        public override float damageMult => 1.15f;
    }

    [LegacyName("FeatheredWeaponPrefix")]
    public class Feathered : RogueWeaponPrefix
    {
        public override float useTimeMult => 0.85f;
        public override float shootSpeedMult => 1.1f;
    }

    [LegacyName("SleekWeaponPrefix")]
    public class Sleek : RogueWeaponPrefix
    {
        public override float damageMult => 1f;
        public override float useTimeMult => 0.9f;
        public override float shootSpeedMult => 1.15f;
    }

    [LegacyName("HeftyWeaponPrefix")]
    public class Hefty : RogueWeaponPrefix
    {
        public override float damageMult => 1.1f;
        public override float stealthDmgMult => 1.15f;
    }

    [LegacyName("MightyWeaponPrefix")]
    public class Mighty : RogueWeaponPrefix
    {
        public override float damageMult => 1.15f;
        public override float stealthDmgMult => 1.05f;
    }

    [LegacyName("GloriousWeaponPrefix")]
    public class Glorious : RogueWeaponPrefix
    {
        public override float damageMult => 1.1f;
        public override float useTimeMult => 0.95f;
    }

    [LegacyName("SerratedWeaponPrefix")]
    public class Serrated : RogueWeaponPrefix
    {
        public override float damageMult => 1.1f;
        public override float useTimeMult => 0.9f;
        public override float shootSpeedMult => 1.05f;
    }

    [LegacyName("ViciousWeaponPrefix")]
    public class Vicious : RogueWeaponPrefix
    {
        public override float damageMult => 1.1f;
        public override float useTimeMult => 0.95f;
        public override float shootSpeedMult => 1.15f;
    }

    [LegacyName("LethalWeaponPrefix")]
    public class Lethal : RogueWeaponPrefix
    {
        public override float damageMult => 1.1f;
        public override float useTimeMult => 0.95f;
        public override int critBonus => 2;
        public override float shootSpeedMult => 1.05f;
        public override float stealthDmgMult => 1.05f;
    }

    [LegacyName("FlawlessWeaponPrefix")]
    public class Flawless : RogueWeaponPrefix
    {
        public override float damageMult => 1.15f;
        public override float useTimeMult => 0.9f;
        public override int critBonus => 5;
        public override float shootSpeedMult => 1.1f;
        public override float stealthDmgMult => 1.15f;
    }

    [LegacyName("RadicalWeaponPrefix")]
    public class Radical : RogueWeaponPrefix
    {
        public override float damageMult => 1.05f;
        public override float useTimeMult => 0.95f;
        public override float shootSpeedMult => 1.05f;
        public override float stealthDmgMult => 0.9f;
    }

    [LegacyName("BluntWeaponPrefix")]
    public class Blunt : RogueWeaponPrefix
    {
        public override float damageMult => 0.85f;
    }

    [LegacyName("FlimsyWeaponPrefix")]
    public class Flimsy : RogueWeaponPrefix
    {
        public override float damageMult => 0.9f;
        public override float stealthDmgMult => 0.9f;
    }

    [LegacyName("UnbalancedWeaponPrefix")]
    public class Unbalanced : RogueWeaponPrefix
    {
        public override float useTimeMult => 1.15f;
        public override float shootSpeedMult => 0.95f;
    }

    [LegacyName("AtrociousWeaponPrefix")]
    public class Atrocious : RogueWeaponPrefix
    {
        public override float damageMult => 0.85f;
        public override float shootSpeedMult => 0.9f;
        public override float stealthDmgMult => 0.9f;
    }

    public abstract class RogueWeaponPrefix : ModPrefix, ILocalizedModType
    {
        public new string LocalizationCategory => "Prefixes.Weapon";

        // Stats
        public virtual float damageMult => 1f;
        public virtual float useTimeMult => 1f;
        public virtual int critBonus => 0;
        public virtual float shootSpeedMult => 1f;
        public virtual float stealthDmgMult => 1f;

        // Prefix roll logic -- Can also be rolled by throwing weapons, even if stealth strikes don't exist
        public override PrefixCategory Category => PrefixCategory.AnyWeapon;
        public override bool CanRoll(Item item) => item.CountsAsClass<ThrowingDamageClass>() && (item.maxStack == 1 || item.AllowReforgeForStackableItem) && GetType() != typeof(RogueWeaponPrefix);

        // Applying normal weapon stats
        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = this.damageMult;
            useTimeMult = this.useTimeMult;
            critBonus = this.critBonus;
            shootSpeedMult = this.shootSpeedMult;
        }

        // Applying stealth strike damage
        public override void Apply(Item item)
        {
            if (item.CountsAsClass<RogueDamageClass>() && item.TryGetGlobalItem<RogueGlobalItem>(out var rogueItem))
                rogueItem.StealthStrikePrefixBonus = stealthDmgMult;
        }

        // Changing value based on prefix tier (rarity is set automatically around value multiplier)
        public override void ModifyValue(ref float valueMult)
        {
            float extraStealthDamage = stealthDmgMult - 1f;
            float stealthDamageValueMultiplier = 1f;
            float extraValue = 1f + stealthDamageValueMultiplier * extraStealthDamage;
            valueMult *= extraValue;
        }

        // Extra tooltip for new modifier stats
        internal const string StealthTooltipID = "CalamityMod:PrefixStealthDamage";
        public LocalizedText StealthDamageTooltip => CalamityUtils.GetText($"{LocalizationCategory}.StealthDamageTooltip");
        public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
        {
            // Ignore this if there's no mult
            if (stealthDmgMult == 1f)
                yield break;

            yield return new TooltipLine(Mod, StealthTooltipID, StealthDamageTooltip.Format((stealthDmgMult >= 1f ? "+" : string.Empty) + ((stealthDmgMult * 100) - 100).ToString("N0")))
            {
                Color = stealthDmgMult < 1f ? Colors.PrefixBad : Colors.PrefixGood
            };
        }
    }
}
