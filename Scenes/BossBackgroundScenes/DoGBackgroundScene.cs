using CalamityMod.NPCs.DevourerofGods;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class DoGBackgroundScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            bool monolithIsActive = Main.LocalPlayer.Calamity().monolithDevourerBShader > 0 || Main.LocalPlayer.Calamity().monolithDevourerPShader > 0;
            bool dogIsAlive = NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>());

            return dogIsAlive || monolithIsActive;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            Main.SceneState.ManageSpecialBiomeVisuals("CalamityMod:DevourerofGodsHead", isActive);
            if (isActive)
                SkyManager.Instance.Activate("CalamityMod:DevourerofGodsHead");
            else
                SkyManager.Instance.Deactivate("CalamityMod:DevourerofGodsHead");
        }
    }
}
