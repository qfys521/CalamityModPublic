using CalamityMod.NPCs.Ravager;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class RavagerMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override int NPCType => ModContent.NPCType<RavagerBody>();
        public override int? MusicModMusic => CalamityMod.Instance.GetMusicFromMusicMod("Ravager");
        public override int VanillaMusic => MusicID.Golem;
        public override int OtherworldMusic => MusicID.OtherworldBoss2;
    }
}
