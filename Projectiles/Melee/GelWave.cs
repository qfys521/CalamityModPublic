using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GelWave : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 84;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3());
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(10))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.BlueFairy, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.PinkFairy, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
            if (Projectile.timeLeft <= 60)
            {
                Projectile.alpha = (int)Utils.Remap(Projectile.timeLeft, 0, 60, 255, 0);
                Projectile.velocity *= 0.94f;
            }
            else if (Projectile.scale < 2f)
            {
                Projectile.velocity *= 0.99f;
                Projectile.scale += 0.02f;
            }
            if (Projectile.timeLeft > 60)
            {
                Dust disgustingtrail = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity, DustID.Ice_Pink, -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.4f));
                disgustingtrail.noGravity = true;
                disgustingtrail.scale = 1.2f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);

        public override bool? CanDamage() => (Projectile.alpha == 0 ? null : false);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slimed, 300);
            Projectile.velocity *= 1.1f;

            if (Projectile.numHits >= 3 && Projectile.timeLeft > 60)
                Projectile.timeLeft = 60;
        }

    }
}
