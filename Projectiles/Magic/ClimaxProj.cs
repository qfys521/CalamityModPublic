using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class ClimaxProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public bool firedBeam = false;
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
            Projectile.penetrate = -1;
            Projectile.timeLeft = 48;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 48)
                Projectile.frame = Main.rand.Next(0, 4 + 1);

            // Rapidly screech to a halt once spawned.
            Projectile.velocity *= 0.86f;

            // Spin chaotically given a pre-defined spin direction. Choose one initially at random.
            float spinTheta = 0.11f;
            if (Projectile.localAI[0] == 0f)
                Projectile.localAI[0] = Main.rand.NextBool() ? -spinTheta : spinTheta;

            // Animate the lightning orb.
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 4)
                {
                    Projectile.frame = 0;
                }
            }

            // Initial stagger in frames needs to be skipped over before it starts shooting,
            // but once it's past that then it can fire constantly

            NPC target = Projectile.Center.ClosestNPCAt(800);

            --Projectile.ai[0];
            if (Projectile.ai[0] < 0f && firedBeam == false && target != null)
            {
                firedBeam = true;
                CalamityUtils.MagnetSphereHitscan(Projectile, Vector2.Distance(Projectile.Center, target.Center), 8f, 0, 5, ModContent.ProjectileType<ClimaxBeam>(), 1D, false);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.timeLeft < 30)
            {
                float alphaTimer = Projectile.timeLeft / 30f;
                Projectile.alpha = (int)(255f - 255f * alphaTimer);
            }
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture2D13.Width / 2f, framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool? CanDamage() => false;
    }
}
