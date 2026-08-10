using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Environment
{
    public class AcidDrop : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 360;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Water drip
            for (int i = 0; i < 4; i++)
            {
                int idx = Dust.NewDust(Projectile.position - Projectile.velocity, 2, 2, DustID.Rain, 0f, 0f, 0, new Color(112, 150, 42, 127), 1f);
                Dust dust = Main.dust[idx];
                dust.position.X -= 2f;
                Main.dust[idx].alpha = 38;
                Main.dust[idx].velocity *= 0.1f;
                Main.dust[idx].velocity -= Projectile.velocity * 0.025f;
                Main.dust[idx].scale = 0.75f;
            }

            return true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            if (Main.rand.NextBool())
            {
                // 1 to 3 seconds of poisoned
                target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(1, 4));
            }
            else if (Main.rand.NextBool(4))
            {
                // 1 to 2 second of Sulphuric Poisoning
                target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 60 * Main.rand.Next(1, 3));
            }
            else
            {
                // 3 to 5 seconds of Irradiated
                target.AddBuff(ModContent.BuffType<Irradiated>(), 60 * Main.rand.Next(3, 6));
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], new Color(255, 255, 255, 127), 2);
            return false;
        }
    }
}
