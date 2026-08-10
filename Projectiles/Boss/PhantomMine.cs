using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class PhantomMine : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 480;
            Projectile.Opacity = 0f;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Projectile.Opacity = MathHelper.Lerp(0f, 1f, Projectile.velocity.Length() / Projectile.ai[0]);

            if (Projectile.velocity.Length() < Projectile.ai[0])
            {
                Projectile.velocity *= Projectile.ai[1];
                if (Projectile.velocity.Length() > Projectile.ai[0])
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= Projectile.ai[0];
                }

                if (Main.getGoodWorld)
                {
                    if (Projectile.velocity.Length() >= Projectile.ai[0])
                    {
                        if (Projectile.owner == Main.myPlayer)
                        {
                            int totalProjectiles = 8;
                            float radians = MathHelper.TwoPi / totalProjectiles;
                            for (int i = 0; i < totalProjectiles; i++)
                            {
                                Vector2 vector = new Vector2(0f, -8f).RotatedBy(radians * i);
                                int type = Main.rand.NextBool() ? ModContent.ProjectileType<PhantomShot2>() : ModContent.ProjectileType<PhantomShot>();
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, type, Projectile.damage, 0f, Main.myPlayer);
                            }
                        }

                        Projectile.Kill();
                    }
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color((byte)(200 * Projectile.Opacity), (byte)(200 * Projectile.Opacity), (byte)(200 * Projectile.Opacity), Projectile.alpha);
        }

        public override bool CanHitPlayer(Player target) => Projectile.velocity.Length() >= Projectile.ai[0];

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 12f, targetHitbox);

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);
            for (int i = 0; i < 15; i++)
            {
                int phantomDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.2f);
                Main.dust[phantomDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[phantomDust].scale = 0.5f;
                    Main.dust[phantomDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 30; j++)
            {
                int phantomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.7f);
                Main.dust[phantomDust2].noGravity = true;
                Main.dust[phantomDust2].velocity *= 5f;
                phantomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1f);
                Main.dust[phantomDust2].velocity *= 2f;
            }
        }
    }
}
