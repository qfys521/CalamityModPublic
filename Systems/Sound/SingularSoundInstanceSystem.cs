using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class SingularSoundInstanceSystem : ModSystem
    {
        public static SlotId SoundSlot;

        internal static int maxPlaytime = 0;
        internal static int remainingPlaytime = 0;
        internal static int timeUntilReset = 0;
        internal static Entity AttachedEntity;

        public static bool currentlyPlaying = false;

        public override void UpdateUI(GameTime gameTime)
        {
            // Decrement timers
            if (remainingPlaytime > 0)
                --remainingPlaytime;
            if (timeUntilReset > 0)
                --timeUntilReset;

            if (currentlyPlaying)
            {
                // If the reset timer has run out, stop the active sound instance entirely.
                // The next time the sound is played, it will start from the beginning.
                if (timeUntilReset <= 0)
                {
                    currentlyPlaying = false;
                    maxPlaytime = 0;
                    AttachedEntity = null;
                    if (SoundEngine.TryGetActiveSound(SoundSlot, out var activeSound))
                        activeSound.Stop();
                    SoundSlot = SlotId.Invalid;
                }

                // Otherwise, set the volume of the active sound instance appropriately.
                else
                {
                    bool foundSound = SoundEngine.TryGetActiveSound(SoundSlot, out var activeSound);
                    // If an attached entity stops existing, so does the sound.
                    if (!foundSound || AttachedEntity.IsNullOrInactive())
                    {
                        currentlyPlaying = false;
                        maxPlaytime = 0;
                        AttachedEntity = null;
                        activeSound?.Stop();
                        SoundSlot = SlotId.Invalid;
                        return;
                    }
                    // Otherwise, attach to the entity.
                    else
                        activeSound.Position = AttachedEntity.Center;

                    float newVolume = MathHelper.Clamp(remainingPlaytime / (float)maxPlaytime, 0f, 1f);
                    activeSound.Volume = newVolume;
                    activeSound.Update();
                }
            }
        }

        public static void PlaySingleInstance(SoundStyle Sound, int Playtime, int ResetTime, Entity Attachment = null)
        {
            if (!currentlyPlaying)
                SoundSlot = SoundEngine.PlaySound(Sound);

            currentlyPlaying = true;
            remainingPlaytime = Playtime;
            maxPlaytime = Playtime;
            timeUntilReset = ResetTime;

            if (Attachment != null)
                AttachedEntity = Attachment;
        }
    }
}
