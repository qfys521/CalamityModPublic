using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class OpalStrike : ModProjectile, ILocalizedModType
    {
        public bool FirstFrameNoDraw = true;
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.alpha = 55;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false; // Custom tile collision since the hitbox is large
        }

        public override void AI()
        {
            if (Collision.SolidCollision(Projectile.Center, 5, 5))
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.4f, 0, default, Main.rand.NextFloat(0.6f, 0.8f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                dust.fadeIn = -0.5f;
            }
            
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.Orange with { A = 0 }, 1);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 60);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(0.6f, 1.3f), 0, default, Main.rand.NextFloat(1.6f, 2.3f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                dust.fadeIn = 1.5f;
            }
        }
        public override bool? CanDamage() => base.CanDamage();
    }
}
