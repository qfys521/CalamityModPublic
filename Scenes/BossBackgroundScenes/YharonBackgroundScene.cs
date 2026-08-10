using CalamityMod.NPCs.Yharon;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class YharonBackgroundScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(ModContent.NPCType<Yharon>()) || Main.LocalPlayer.Calamity().monolithYharonShader > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            Main.SceneState.ManageSpecialBiomeVisuals("CalamityMod:Yharon", isActive);
        }
    }
}
