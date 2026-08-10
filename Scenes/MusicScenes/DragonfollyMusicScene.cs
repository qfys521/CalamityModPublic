using CalamityMod.NPCs.Bumblebirb;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class DragonfollyMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

        public override int NPCType => ModContent.NPCType<Dragonfolly>();
        public override int? MusicModMusic => CalamityMod.Instance.GetMusicFromMusicMod("Dragonfolly");
        public override int VanillaMusic => MusicID.Golem;
        public override int OtherworldMusic => MusicID.OtherworldBoss2;
    }
}
