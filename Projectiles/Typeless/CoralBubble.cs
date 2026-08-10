using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class CoralBubble : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 360;
            Projectile.penetrate = 1;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] > 2f)
            {
                Projectile.alpha -= 5;
                if (Projectile.alpha < 100)
                    Projectile.alpha = 100;
            }
            else
                Projectile.localAI[0] += 1f;

            if (Projectile.ai[1] > 30f)
            {
                if (Projectile.velocity.Y > -1.5f)
                    Projectile.velocity.Y = Projectile.velocity.Y - 0.05f;
            }
            else
                Projectile.ai[1] += 1f;

            if (Projectile.wet)
            {
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y = Projectile.velocity.Y * 0.98f;
                if (Projectile.velocity.Y > -1f)
                    Projectile.velocity.Y = Projectile.velocity.Y - 0.2f;
            }

            int closestPlayer = Player.FindClosest(Projectile.Center, 1, 1);
            if (Projectile.Distance(Main.player[closestPlayer].Center) < 14f)
            {
                SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);
                Main.player[closestPlayer].AddBuff(BuffID.Gills, 90);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector3 lightCol = new(85f / 255f, 151f / 255f, 196f / 255f);
            float brightness = 0.7f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.01f;
            brightness *= (float)MathF.Sin(8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
            brightness += 0.4f;
            brightness = MathHelper.Clamp(brightness, 0.1f, 0.5f);
            lightCol *= brightness;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            Lighting.AddLight(Projectile.position, lightCol);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                int size = 12;
                int dustIndex = Dust.NewDust(Projectile.Center - Vector2.One * size, size * 2, size * 2, DustID.BubbleBurst_White);
                Dust dust = Main.dust[dustIndex];
                Vector2 value14 = Vector2.Normalize(dust.position - Projectile.Center);
                dust.position = Projectile.Center + value14 * size;
                dust.velocity = value14 * dust.velocity.Length();
                dust.color = Main.hslToRgb((float)(0.4000000059604645 + Main.rand.NextDouble() * 0.20000000298023224), 1f, 0.7f);
                dust.color = Color.Lerp(dust.color, Color.White, 0.3f);
                dust.noGravity = true;
                dust.scale = 0.7f;
            }
        }
    }
}
