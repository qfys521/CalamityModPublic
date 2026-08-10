using System;
using System.Collections.Generic;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Wings;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace CalamityMod.Rarities
{
    public class HotPink : ModRarity
    {
        // Hot Pink is used for developer items. It Has a system built in for custom rarity effects.
        // It is a unique rarity and does not have its items rarity change on reforge.
        public override Color RarityColor => TextColor;
        public static Color TextColor => new Color(255, 0, 255);

        public static Dictionary<int, Func<string, TextSnippet>> CustomRarities = new()
        {
            { ModContent.ItemType<TiredTail>(), text => new TiredTailTextEffects(text) }
        };
        public static Dictionary<int, Func<Color>> CustomColors = new()
        {
            { ModContent.ItemType<AngelicAlliance>(), AngelicAlliance.RarityColor },
            { ModContent.ItemType<Contagion>(), Contagion.RarityColor },
            { ModContent.ItemType<CrystylCrusher>(), CrystylCrusher.RarityColor },
            { ModContent.ItemType<TheDanceofLight>(), TheDanceofLight.GetSyncedLightColor },
            { ModContent.ItemType<DemonshadeHelm>(), DemonshadeHelm.DemonshadeRarityColor },
            { ModContent.ItemType<DemonshadeBreastplate>(), DemonshadeHelm.DemonshadeRarityColor },
            { ModContent.ItemType<DemonshadeGreaves>(), DemonshadeHelm.DemonshadeRarityColor },
            { ModContent.ItemType<DraconicDestruction>(), DraconicDestruction.RarityColor },
            { ModContent.ItemType<Earth>(), Earth.RarityColor },
            { ModContent.ItemType<Endogenesis>(), Endogenesis.RarityColor },
            { ModContent.ItemType<Eternity>(), Eternity.RarityColor },
            { ModContent.ItemType<FlamsteedRing>(), FlamsteedRing.RarityColor },
            { ModContent.ItemType<IllustriousKnives>(), IllustriousKnives.RarityColor },
            { ModContent.ItemType<NanoblackReaper>(), NanoblackReaper.RarityColor },
            { ModContent.ItemType<Ozzathoth>(), ShatteredCommunity.GetRarityColor }, // Yes, this reuses Shattered Community's color
            { ModContent.ItemType<ProfanedSoulCrystal>(), ProfanedSoulCrystal.RarityColor },
            { ModContent.ItemType<RedSun>(), RedSun.RarityColor },
            { ModContent.ItemType<ScarletDevil>(), ScarletDevil.RarityColor },
            { ModContent.ItemType<ShatteredCommunity>(), ShatteredCommunity.GetRarityColor },
            { ModContent.ItemType<SomaPrime>(), SomaPrime.RarityColor },
            { ModContent.ItemType<StaffofBlushie>(), StaffofBlushie.RarityColor },
            { ModContent.ItemType<Svantechnical>(), Svantechnical.RarityColor },
            { ModContent.ItemType<Sylvestaff>(), Sylvestaff.RarityColor },
            { ModContent.ItemType<TemporalUmbrella>(), TemporalUmbrella.RarityColor },
            { ModContent.ItemType<TriactisTruePaladinianMageHammerofMight>(), TriactisTruePaladinianMageHammerofMight.RarityColor }
        };

        public static void Draw(Item Item, SpriteBatch spriteBatch, string text, int X, int Y, Color textColor, Color lightColor, float rotation,
        Vector2 origin, Vector2 baseScale, float time, bool renderTextSparkles, DynamicSpriteFont font)
        {
            if (CustomColors.TryGetValue(Item.type, out var color)) // For items which use a custom item color, give them that custom color.
            {
                textColor = color.Invoke();
            }
            List<TextSnippet> snippets = ChatManager.ParseMessage(text, textColor);

            if (CustomRarities.ContainsKey(Item.type)) // For items in the custom rarity table, give them custom rarity effects.
            {
                for (int i = 0; i < snippets.Count; i++)
                {
                    TextSnippet textSnippet = snippets[i];
                    if (snippets[i].GetType() == typeof(TextSnippet))
                    {
                        snippets[i] = CustomRarities[Item.type].Invoke(textSnippet.Text);
                        continue;
                    }
                }
            }
            else
                ChatManager.ConvertNormalSnippets(snippets);

            ChatManager.DrawColorCodedString(spriteBatch, font, snippets, new(X, Y), textColor, 0, Vector2.Zero, baseScale, out _, -1, true);
        }

        public static void Draw(Item Item, string text, int X, int Y, float rotation, Vector2 origin, Vector2 baseScale, Color? textColor = null, Color? lightColor = null, bool? renderTextSparkles = null)
        {
            Draw(Item, Main.spriteBatch, text, X, Y, Colors.AlphaDarken(textColor ?? TextColor), lightColor ?? Color.White, rotation, origin, baseScale, Main.GlobalTimeWrappedHourly,
                renderTextSparkles ?? CalamityClientConfig.Instance.TextEffects, FontAssets.MouseText.Value);
        }

        public static void Draw(Item Item, DrawableTooltipLine line)
        {
            Draw(Item, line.Text, line.X, line.Y, line.Rotation, line.Origin, line.BaseScale);
        }

    }
}
