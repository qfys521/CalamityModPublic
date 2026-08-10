using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class HellionSpike : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = ProjAIStyleID.Beam;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 180;
            AIType = ProjectileID.SporeCloud;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, 0.25f, 0f);
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.JungleSpore, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.JungleSpore, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft > 175)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHitEffects(target.Center, hit.Crit);
            target.AddBuff(BuffID.Venom, 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitEffects(target.Center, true);
            target.AddBuff(BuffID.Venom, 180);
        }

        private void OnHitEffects(Vector2 targetPos, bool crit)
        {
            if (crit)
            {
                var source = Projectile.GetSource_FromThis();
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile petal = CalamityUtils.ProjectileBarrage(source, Projectile.Center, targetPos, Main.rand.NextBool(), 800f, 800f, 0f, 800f, 24f, ProjectileID.FlowerPetal, (int)(Projectile.damage * 0.5), Projectile.knockBack * 0.5f, Projectile.owner, false, 0f);
                    if (petal.whoAmI.WithinBounds(Main.maxProjectiles))
                    {
                        petal.DamageType = DamageClass.Melee;
                        petal.localNPCHitCooldown = -1;
                    }
                }
            }
        }
    }
}
