using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class DarkMasterBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        private int tileCounter = 5; // assure that the beams don't instantly die due to their size

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 2;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (tileCounter > 0)
                tileCounter--;
            if (tileCounter <= 0 && Projectile.ai[1] == 0)
                Projectile.tileCollide = true;

            Projectile.velocity *= 1.03f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame > 5)
                {
                    Projectile.frame = 0;
                }
            }
            Lighting.AddLight(Projectile.Center, TorchID.Red); // til there's a torch id
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * Main.rand.NextFloat(0f, 1f);
                Vector2 angleVec = angle.ToRotationVector2();
                float distance = Main.rand.NextFloat(7f, 18f);
                Vector2 off = angleVec * distance;
                off.Y *= (float)Projectile.height / Projectile.width;
                Vector2 pos = Projectile.Center + off;
                Dust d = Dust.NewDustPerfect(pos, DustID.GemRuby, angleVec * Main.rand.NextFloat(2f, 4f));
                d.customData = true;
            }
        }

        public override Color? GetAlpha(Color drawColor) => Projectile.ai[0] == 1 ? new Color(100, 100, 100, Projectile.alpha) : new Color(255, 255, 255, Projectile.alpha);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
