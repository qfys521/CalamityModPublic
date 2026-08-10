using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class MagnaShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(4, 4) - Projectile.velocity;
            float trailScale = Main.rand.NextFloat(0.04f, 0.05f);
            Color trailColor = Main.rand.NextBool(3) ? Color.DodgerBlue : Color.RoyalBlue;
            Particle Trail = new CustomSpark(trailPos, Projectile.velocity.RotatedByRandom(0.03f) * Main.rand.NextFloat(0.1f, 0.4f), "CalamityMod/Particles/SquareRotated", false, Main.rand.NextBool(7) ? 35 : 6, trailScale, trailColor, new Vector2(1f, 1f), true, true, glowOpacity: 0.5f, extraRotation: Main.rand.NextFloat(0, MathHelper.TwoPi));
            GeneralParticleHandler.SpawnParticle(Trail);
            
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.DodgerBlue with { A = 0 }, 1);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.8f, 1.8f), 0, default, Main.rand.NextFloat(0.9f, 1.3f));
                dust.noGravity = false;
                dust.color = Main.rand.NextBool(3) ? Color.DodgerBlue : Color.RoyalBlue;
                dust.fadeIn = -0.15f;
            }
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.8f, PitchVariance = 0.3f }, Projectile.Center);
        }
        public override bool? CanDamage() => base.CanDamage();
    }
}
