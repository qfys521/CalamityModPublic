using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class EmpyreanKnife : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        private int bounce = 3;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 75f)
            {
                Projectile.alpha += 10;
                Projectile.damage = (int)(Projectile.damage * 0.95);
                Projectile.knockBack = Projectile.knockBack * 0.95f;
                if (Projectile.alpha >= 255)
                    Projectile.active = false;
            }
            if (Projectile.ai[0] < 75f)
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
            else
            {
                Projectile.rotation += 0.5f;
            }
            CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 150f, 12f, 20f);
            if (Main.rand.NextBool(6))
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Enchanted_Pink, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounce--;
            if (bounce <= 0)
                Projectile.Kill();
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                int empyreanDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Pink, 0f, 0f, 100, default, 0.8f);
                Main.dust[empyreanDust].noGravity = true;
                Main.dust[empyreanDust].velocity *= 1.2f;
                Main.dust[empyreanDust].velocity -= Projectile.oldVelocity * 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Main.player[Projectile.owner].SpawnLifeStealProjectile(target, Projectile, ProjectileID.VampireHeal, (int)Math.Round(hit.Damage * 0.01), 0.75f);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
