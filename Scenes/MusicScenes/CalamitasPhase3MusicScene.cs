using CalamityMod.NPCs;
using CalamityMod.NPCs.SupremeCalamitas;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class CalamitasPhase3MusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int NPCType => ModContent.NPCType<SupremeCalamitas>();
        public override int? MusicModMusic => CalamityMod.Instance.GetMusicFromMusicMod("CalamitasPhase3");
        public override int VanillaMusic => MusicID.MoonLord;
        public override int OtherworldMusic => MusicID.OtherworldMoonLord;

        public override bool AdditionalCheck() => CalamityGlobalNPC.SCalEpiphany != -1;
    }
}
