using CalamityMod.NPCs.Crabulon;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class CrabulonMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override int NPCType => ModContent.NPCType<Crabulon>();
        public override int? MusicModMusic => CalamityMod.Instance.GetMusicFromMusicMod("Crabulon");
        public override int VanillaMusic => MusicID.Golem;
        public override int OtherworldMusic => MusicID.OtherworldBoss1;
    }
}
