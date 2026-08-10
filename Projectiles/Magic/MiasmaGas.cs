using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class MiasmaGas : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.alpha = Main.rand.Next(35, 75);
                Projectile.localAI[0] = 1f;
            }
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 360f)
            {
                Projectile.ai[1] = 1f;
            }
            if (Projectile.ai[1] == 1f)
            {
                Projectile.velocity *= 0.9865f;
            }
            Projectile.rotation += Projectile.velocity.X * 0.0003f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
            Projectile.ai[1] = 1f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
            Projectile.ai[1] = 1f;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, 48, 30, DustID.Pumpkin, 0f, 0f);
                dust.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(39f));
                dust.alpha = 127;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }
    }
}
