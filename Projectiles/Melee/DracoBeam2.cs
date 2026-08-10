using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class DracoBeam2 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Melee/DracoBeam";

        private int start = 60;
        private int speedTimer = 60;
        private Vector2 setVel = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver4, 0.25f);

            if (setVel == Vector2.Zero)
                setVel = Projectile.velocity * 3;

            start--;
            if (start <= 0)
            {
                speedTimer--;
                if (speedTimer > 30)
                {
                    if (speedTimer == 59)
                    {
                        Projectile.velocity.X = 0f;
                        Projectile.velocity.Y = 2f;
                    }
                    Projectile.velocity *= 1.11f;
                }
                else if (speedTimer <= 30)
                {
                    if (speedTimer == 30)
                    {
                        Projectile.velocity.X = 0f;
                        Projectile.velocity.Y = -2f;
                    }
                    Projectile.velocity *= 1.11f;
                }
                if (speedTimer <= 0)
                {
                    speedTimer = 60;
                    start = 20;
                    Projectile.velocity = setVel;
                }
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0f);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 90);
        }
    }
}
