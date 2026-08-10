using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class ScourgeoftheCosmosProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        private int bounce = 3;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 36;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.alpha <= 200)
            {
                for (int i = 0; i < 2; i++)
                {
                    int dustType = Main.rand.NextBool(3) ? 56 : 242;
                    Dust scourgeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 0, default, 1f);
                    scourgeDust.position = Projectile.Center - Projectile.velocity * i * 0.25f;
                    scourgeDust.velocity *= 0f;
                    scourgeDust.scale = 0.7f;
                }
            }
            Projectile.alpha -= 50;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 180f)
            {
                Projectile.velocity.Y = Projectile.velocity.Y + 0.4f;
                Projectile.velocity.X = Projectile.velocity.X * 0.97f;
            }
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounce--;
            if (bounce <= 0)
                Projectile.Kill();
            else
            {
                SoundEngine.PlaySound(SoundID.NPCHit4, Projectile.position);
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;
                if (Projectile.owner == Main.myPlayer)
                {
                    int minisAmt = Main.rand.Next(1, 2 + 1);
                    for (int j = 0; j < minisAmt; j++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), ModContent.ProjectileType<ScourgeoftheCosmosMini>(), (int)(Projectile.damage * 0.75), Projectile.knockBack * 0.35f, Projectile.owner);
                    }
                }
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit4, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.NextBool(3) ? 56 : 242;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 0, default, 1f);
                dust.scale *= 1.1f;
                dust.noGravity = true;
            }
            for (int j = 0; j < 15; j++)
            {
                int dustType = Main.rand.NextBool(3) ? 56 : 242;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 0, default, 1f);
                dust.velocity *= 2.5f;
                dust.scale *= 0.8f;
                dust.noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                int minisAmt = Main.rand.Next(3, 4 + 1);
                for (int j = 0; j < minisAmt; j++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), ModContent.ProjectileType<ScourgeoftheCosmosMini>(), (int)(Projectile.damage * 0.75), Projectile.knockBack * 0.35f, Projectile.owner);
                }
            }
        }
    }
}
