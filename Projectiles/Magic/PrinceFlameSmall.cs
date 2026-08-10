using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class PrinceFlameSmall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float Time => ref Projectile.ai[0];
        public const int AttackDelay = 12;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = 80 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Time++;
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Projectile.Opacity = Utils.GetLerpValue(0f, 15f, Projectile.timeLeft, true);

            if (Main.rand.NextBool(3))
            {
                Particle mist = new MediumMistParticle(Projectile.Center, Projectile.velocity * 0.5f, PrinceFlameLarge.FlameColor * Projectile.Opacity, Color.DarkSlateGray * Projectile.Opacity, Main.rand.NextFloat(0.4f, 0.6f), 140, Main.rand.NextFloat(-0.1f, 0.1f));
                GeneralParticleHandler.SpawnParticle(mist);
            }

            if (Time > AttackDelay)
            {
                CalamityUtils.HomeInOnNPC(Projectile, false, 600f, 14f, 32f);

                Particle fire = new GlowOrbParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), (-Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.4f), false, 9, Main.rand.NextFloat(0.4f, 1f), PrinceFlameLarge.FlameColor);
                GeneralParticleHandler.SpawnParticle(fire);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.8f);
            lightColor.A /= 2;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Time > AttackDelay)
                return null;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<HolyFlames>(), 60);

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(2f, 12f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.5f);
                dust.color = Main.hslToRgb(Main.rand.NextFloat(0.033f, 0.167f), 1f, 0.66f);
                dust.noLightEmittance = true;
            }
        }
    }
}
