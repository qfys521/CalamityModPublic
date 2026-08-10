using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class SwirlingFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public ref float AngularTurnSpeed => ref Projectile.ai[0];
        public ref float Time => ref Projectile.ai[1];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
            Projectile.ai[0] = MathHelper.ToRadians(Main.rand.NextFloat(-3, 3));
        }

        public override void AI()
        {
            Color c = ProvUtils.GetProjectileColor(255);

            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, Projectile.velocity / 2f, false, 10, 0.5f * Projectile.ai[2], c));
            GeneralParticleHandler.SpawnParticle(new MediumMistParticle(Projectile.Center, Vector2.Zero, Color.LightSlateGray, Color.DarkSlateGray, 0.5f * Projectile.ai[2], 150, Main.rand.NextFloat(-0.01f, 0.01f)));

            Projectile.ai[2] *= 0.98f;

            Projectile.velocity *= 0.97f;

            if (Projectile.ai[2] < 0.2f)
            {
                Projectile.Kill();
            }

            Projectile.velocity = Projectile.velocity.RotatedBy(AngularTurnSpeed);
            Time++;
        }
    }
}
