using CalamityMod.NPCs.Leviathan;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class LeviathanBackgroundScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override bool IsSceneEffectActive(Player player) => (Main.zenithWorld ? NPC.AnyNPCs(ModContent.NPCType<Anahita>()) : NPC.AnyNPCs(ModContent.NPCType<Leviathan>())) || Main.LocalPlayer.Calamity().monolithLeviathanShader > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            Main.SceneState.ManageSpecialBiomeVisuals("CalamityMod:Leviathan", isActive);
        }
    }
}
