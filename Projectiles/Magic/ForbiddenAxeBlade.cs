using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class ForbiddenAxeBlade : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Projectile.alpha -= 3;
            Projectile.rotation += 0.75f;

            Projectile.ai[1] += 1f;
            if (Projectile.ai[1] <= 20f)
            {
                Projectile.velocity *= 0.85f;
            }
            else if (Projectile.ai[1] > 20f && Projectile.ai[1] <= 39f)
            {
                Projectile.velocity *= 1.25f;
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 300f, 10f, 20f);
            }
            else if (Projectile.ai[1] == 40f)
            {
                Projectile.ai[1] = 0f;
            }

            if (Main.rand.NextBool(8))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Teleporter, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Teleporter, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
