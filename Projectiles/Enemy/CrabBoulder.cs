using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Enemy
{
    public class CrabBoulder : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.timeLeft = 480;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            if (Projectile.ai[0]++ < 30f)
            {
                Projectile.scale = MathHelper.Lerp(0.004f, 1f, Projectile.ai[0] / 30f);
            }
            else
            {
                if (Projectile.velocity.Y < 12f)
                    Projectile.velocity.Y += 0.18f;
            }
            Projectile.tileCollide = Projectile.ai[0] > 70f;
            Projectile.rotation += Math.Sign(Projectile.velocity.X) * 0.08f;
        }
        public override void OnKill(int timeLeft)
        {
            Utils.PoofOfSmoke(Projectile.Center);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
