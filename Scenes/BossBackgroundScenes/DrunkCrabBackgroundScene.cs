using CalamityMod.NPCs.Crabulon;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class DrunkCrabBackgroundScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            if (Main.zenithWorld && NPC.AnyNPCs(ModContent.NPCType<Crabulon>()))
                return true;
            return false;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            Main.SceneState.ManageSpecialBiomeVisuals("CalamityMod:DrunkCrabulon", isActive);
        }
    }
}
