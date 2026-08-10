using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class OpalChargedStrike : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 35;
            Projectile.friendly = true;
            Projectile.alpha = 55;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 6;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false; // Custom tile collision since the hitbox is large
        }

        public override void AI()
        {
            if (Collision.SolidCollision(Projectile.Center, 5, 5))
                Projectile.Kill();

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.4f, 0, default, Main.rand.NextFloat(0.75f, 0.95f));
                dust.noGravity = !Main.rand.NextBool(5);
                dust.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                dust.fadeIn = -0.85f;
            }

            Player Owner = Main.player[Projectile.owner];
            float playerDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (Main.rand.NextBool(3) && playerDist < 1400f && Projectile.timeLeft < 290)
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8, 8);
                float trailScale = Main.rand.NextFloat(0.45f, 0.6f);
                Color trailColor = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                Particle Trail = new CustomSpark(trailPos, Projectile.velocity * 0.2f, "CalamityMod/Particles/BloomCircle", false, 40, trailScale, trailColor, new Vector2(0.2f, 1.4f), true, true, shrinkSpeed: 0.13f, glowOpacity: 0.6f);
                GeneralParticleHandler.SpawnParticle(Trail);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.OrangeRed with { A = 0 }, 1);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.75f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(0.8f, 1.8f), 0, default, Main.rand.NextFloat(1.9f, 2.8f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(3) ? Color.Orange : Color.OrangeRed;
                dust.fadeIn = 1.85f;
            }
        }
        public override bool? CanDamage() => base.CanDamage();
    }
}
