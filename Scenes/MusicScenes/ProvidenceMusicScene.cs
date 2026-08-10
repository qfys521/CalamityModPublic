using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class ProvidenceMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int NPCType => ModContent.NPCType<Providence>();
        public static int? ProvidenceTrack => CalamityMod.Instance.GetMusicFromMusicMod("Providence");
        public static int SilenceTrack => MusicLoader.GetMusicSlot(CalamityMod.Instance, "Sounds/Music/Silence");
        public override int? MusicModMusic => ProvidenceTrack is not null && ProvidenceSpawnState() < 180f ? SilenceTrack : ProvidenceTrack;
        public override int VanillaMusic => MusicID.MoonLord;
        public override int OtherworldMusic => MusicID.OtherworldMoonLord;
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (ProvidenceSpawnState() == 180f && ProvidenceTrack is int track)
                Main.musicFade[track] = 1f;
        }

        public static float ProvidenceSpawnState()
        {
            int provIndex = CalamityGlobalNPC.holyBoss;
            if (!Main.npc.IndexInRange(provIndex))
                return -1f;

            var prov = Main.npc[provIndex];
            return prov.Calamity().newAI[3];
        }
    }
}
