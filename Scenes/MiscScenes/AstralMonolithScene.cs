using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class AstralMonolithScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override bool IsSceneEffectActive(Player player) => Main.LocalPlayer.Calamity().monolithAstralShader > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            Main.SceneState.ManageSpecialBiomeVisuals("CalamityMod:Astral", isActive);
        }
    }
}
