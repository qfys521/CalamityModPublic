using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class PrismaticWave : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        private int alpha = 50;
        public Color[] colors =
        [
            new Color(255, 0, 0, 50), //Red
            new Color(255, 128, 0, 50), //Orange
            new Color(255, 255, 0, 50), //Yellow
            new Color(128, 255, 0, 50), //Lime
            new Color(0, 255, 0, 50), //Green
            new Color(0, 255, 128, 50), //Turquoise
            new Color(0, 255, 255, 50), //Cyan
            new Color(0, 128, 255, 50), //Light Blue
            new Color(0, 0, 255, 50), //Blue
            new Color(128, 0, 255, 50), //Purple
            new Color(255, 0, 255, 50), //Fuschia
            new Color(255, 0, 128, 50) //Hot Pink
        ];
        public ref float Timer => ref Projectile.ai[0];
        public Particle starEffect;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = MeleeRangedHybridDamageClass.Instance;
            Projectile.alpha = 255;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Timer++;
            Projectile.alpha -= 16;
            if (Projectile.alpha < 64)
                Projectile.alpha = 64;
            Lighting.AddLight(Projectile.Center, Main.DiscoR * 0.5f / 255f, Main.DiscoG * 0.5f / 255f, Main.DiscoB * 0.5f / 255f);

            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool())
            {
                int rainbow = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.RainbowMk2, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, alpha, Main.rand.Next(colors));
                Main.dust[rainbow].noGravity = true;
            }

            // Determine random star rotation
            if (Projectile.localAI[0] == 0f)
                Projectile.localAI[0] = Main.rand.NextFloat(MathHelper.Pi / 60f, MathHelper.Pi / 12f) * Main.rand.NextBool().ToDirectionInt();

            // Draw the star
            if (starEffect == null)
            {
                Color projColor = Color.Lerp(Color.White, colors[(int)Projectile.ai[1]], 0.4f);
                starEffect = new GenericSparkle(Projectile.Center + Projectile.velocity * 1.5f, Vector2.Zero, projColor, colors[(int)Projectile.ai[1]], Projectile.scale * 2.5f, 2, Timer * Projectile.localAI[0]);
                GeneralParticleHandler.SpawnParticle(starEffect);
            }
            else
            {
                starEffect.Time = 0;
                starEffect.Position = Projectile.Center + Projectile.velocity * 1.5f;
            }

            // Home in if it's from Cosmic Rainbow
            if (Projectile.ai[2] == 1f)
                CalamityUtils.HomeInOnNPC(Projectile, true, 400f, 18f, 20f);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => colors[(int)Projectile.ai[1]];

        public override void OnKill(int timeLeft)
        {
            GeneralParticleHandler.RemoveParticle(starEffect);
            for (int k = 0; k < 3; k++)
            {
                int rainbow = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.RainbowMk2, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f, alpha, Main.rand.Next(colors));
                Main.dust[rainbow].noGravity = true;
            }
        }
    }
}
