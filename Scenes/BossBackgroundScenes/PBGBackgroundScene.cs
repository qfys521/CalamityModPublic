using CalamityMod.NPCs.PlaguebringerGoliath;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class PBGBackgroundScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(ModContent.NPCType<PlaguebringerGoliath>()) || Main.LocalPlayer.Calamity().monolithPlagueShader > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            Main.SceneState.ManageSpecialBiomeVisuals("CalamityMod:PlaguebringerGoliath", isActive);
        }
    }
}
