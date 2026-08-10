using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class MagneticOrb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        private const int Lifetime = 120;
        private const float FramesPerBeam = 12f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 50;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            // Drift to a stop after being launched
            if (Projectile.timeLeft < 40)
            Projectile.velocity *= 0.882f;

            // On frame 1, pick a random offset to use for the firing pattern.
            if (Projectile.timeLeft == Lifetime)
                Projectile.localAI[1] = Main.rand.NextFloat(0f, FramesPerBeam);

            // What in the hell does this code do
            if (Projectile.velocity.X > 0f)
            {
                Projectile.rotation += (Math.Abs(Projectile.velocity.Y) + Math.Abs(Projectile.velocity.X)) * 0.001f;
            }
            else
            {
                Projectile.rotation -= (Math.Abs(Projectile.velocity.Y) + Math.Abs(Projectile.velocity.X)) * 0.001f;
            }

            // Update animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 4)
                {
                    Projectile.frame = 0;
                }
            }

            NPC target = Projectile.Center.ClosestNPCAt(550);

            if ((Projectile.timeLeft == 30 || Projectile.timeLeft == 10) && target != null)
                CalamityUtils.MagnetSphereHitscan(Projectile, Vector2.Distance(Projectile.Center, target.Center), 8f, 0, 1, ModContent.ProjectileType<MagneticBeam>(), 1D, true);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.timeLeft < 30)
            {
                float timeAlpha = (float)Projectile.timeLeft / 30f;
                Projectile.alpha = (int)(255f - 255f * timeAlpha);
            }
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
