using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class VenusianFlame : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        private bool initialized = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 120;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            //Rotation
            if (Projectile.ai[1] > 0f)
            {
                Projectile.rotation = -Projectile.velocity.X * 0.05f + MathHelper.PiOver2;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            Projectile.ai[1]--;

            //frames
            if (!initialized)
            {
                initialized = true;
                Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            //movement
            if (Projectile.velocity.X != Projectile.velocity.X)
            {
                Projectile.velocity.X *= -0.1f;
            }
            if (Projectile.velocity.X != Projectile.velocity.X)
            {
                Projectile.velocity.X *= -0.5f;
            }
            if (Projectile.velocity.Y != Projectile.velocity.Y && Projectile.velocity.Y > 1f)
            {
                Projectile.velocity.Y *= -0.5f;
            }
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 5f)
            {
                Projectile.ai[0] = 5f;
                if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f)
                {
                    Projectile.velocity.X *= 0.97f;
                    if (Projectile.velocity.X > -0.01f && Projectile.velocity.X < 0.01f)
                    {
                        Projectile.velocity.X = 0f;
                        Projectile.netUpdate = true;
                    }
                }
                Projectile.velocity.Y += 0.2f;
            }
            if (Projectile.velocity.Y < 0.25f && Projectile.velocity.Y > 0.15f)
            {
                Projectile.velocity.X *= 0.8f;
            }
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }
            if (Projectile.velocity.Y < 0.25f && Projectile.velocity.Y > 0.15f)
            {
                Projectile.velocity.X *= 0.8f;
            }
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }

            //Dust
            if (Main.rand.NextBool(6))
            {
                int venusDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, 0f, 0f, 100, default, 1f);
                Dust dust = Main.dust[venusDust];
                dust.position.X -= 2f;
                dust.position.Y += 2f;
                dust.scale += (float)Main.rand.Next(50) * 0.01f;
                dust.noGravity = true;
                dust.velocity.Y -= 2f;
                dust.velocity += Projectile.velocity * 0.5f;
            }
            if (Main.rand.NextBool(14))
            {
                int venusDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 0, default, Main.rand.NextFloat(0.3f, 0.5f));
                Dust dust2 = Main.dust[venusDust2];
                dust2.position.X -= 2f;
                dust2.position.Y += 2f;
                dust2.noGravity = true;
                dust2.velocity *= 0.1f;
                dust2.velocity += Projectile.velocity * 0.5f;
                dust2.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Orange : Color.Coral, 0.7f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 150);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[1] = 10f;
            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
