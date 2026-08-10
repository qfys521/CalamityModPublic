using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static CalamityMod.Events.BossRushEvent;

namespace CalamityMod.Projectiles.Typeless
{
    public class BossRushTierAnimation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public int Tier => (int)Projectile.ai[0];

        public Player Owner => Main.player[Projectile.owner];

        public const int FrameChangeRate = 4;

        public const int TotalFrames = 41;

        public override string Texture => "CalamityMod/Projectiles/Typeless/BossRushTier1Animation";

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = FrameChangeRate * TotalFrames;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.Bottom = Owner.Top - Vector2.UnitY * Projectile.scale * 36f;
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / FrameChangeRate;
            if (Projectile.frame >= TotalFrames)
                Projectile.frame = TotalFrames;

            // Play tier transition sounds on the first frame.
            if (Projectile.localAI[0] == 0f)
            {
                float volume = 2.8f;
                switch (Tier)
                {
                    // Tier 1 doesn't have its own sound yet.
                    case 1:
                        SoundEngine.PlaySound(Tier2TransitionSound with { Volume = volume }, Main.LocalPlayer.Center);
                        break;
                    case 2:
                        SoundEngine.PlaySound(Tier2TransitionSound with { Volume = volume }, Main.LocalPlayer.Center);
                        break;
                    case 3:
                        SoundEngine.PlaySound(Tier3TransitionSound with { Volume = volume }, Main.LocalPlayer.Center);
                        break;
                    case 4:
                        SoundEngine.PlaySound(Tier4TransitionSound with { Volume = volume }, Main.LocalPlayer.Center);
                        break;
                    case 5:
                        SoundEngine.PlaySound(Tier5TransitionSound with { Volume = volume }, Main.LocalPlayer.Center);
                        break;
                }
                Projectile.localAI[0] = 1f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ModContent.Request<Texture2D>($"CalamityMod/Projectiles/Typeless/BossRushTier{Tier}Animation").Value;
            Rectangle frame = texture.Frame(TotalFrames, 1, Projectile.frame % TotalFrames, Projectile.frame / TotalFrames);
            Vector2 origin = frame.Size() * 0.5f;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), 0f, origin, Projectile.scale, 0, 0f);
            return false;
        }
    }
}
