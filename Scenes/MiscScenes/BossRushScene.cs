using System.Collections.Generic;
using CalamityMod.Events;
using CalamityMod.Skies;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class BossRushScene : ModSceneEffect
    {
        public override int Music => BossRushEvent.MusicToPlay;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player) => BossRushSky.DetermineDrawEligibility();

        public override void SpecialVisuals(Player player, bool isActive)
        {
            // Clear all other skies, including the vanilla ones.
            if (isActive)
            {
                Dictionary<string, CustomSky> skies = TerrariaInternals.SkyEffects(SkyManager.Instance);
                bool updateRequired = false;
                foreach (string skyName in skies.Keys)
                {
                    if (skies[skyName].IsActive() && skyName != "CalamityMod:BossRush")
                    {
                        skies[skyName].Opacity = 0f;
                        skies[skyName].Deactivate();
                        updateRequired = true;
                    }
                }

                if (updateRequired)
                    SkyManager.Instance.Update(new GameTime());
            }

            if (SkyManager.Instance["CalamityMod:BossRush"] != null && isActive != SkyManager.Instance["CalamityMod:BossRush"].IsActive())
            {
                if (isActive)
                    SkyManager.Instance.Activate("CalamityMod:BossRush");
                else
                    SkyManager.Instance.Deactivate("CalamityMod:BossRush");
            }
        }

        public override float GetWeight(Player player) => 1f;
    }
}
