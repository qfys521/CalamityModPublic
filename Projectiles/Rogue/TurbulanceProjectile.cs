using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class TurbulanceProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Turbulance";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.tileCollide = Projectile.ai[0] >= 2f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);

            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Flare_Blue, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 100, new Color(53, Main.DiscoG, 255));
            }
            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Cloud, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }

            if (Projectile.Calamity().stealthStrike) //Stealth strike
            {
                if (Projectile.timeLeft % 14 == 0)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TurbulanceWindSlash>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack / 2, Projectile.owner, 1f, 1f);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 10; i++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Flare_Blue, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f, 50, new Color(53, Main.DiscoG, 255));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHitEffects(hit.Crit || Main.rand.NextBool(3));
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitEffects(false);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnHitEffects(false);
            return true;
        }

        private void OnHitEffects(bool homeIn)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int w = 0; w < 3; w++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<TurbulanceWindSlash>(), Projectile.damage / 3, Projectile.knockBack / 3, Main.myPlayer, 0f, homeIn ? 1f : 0f);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
