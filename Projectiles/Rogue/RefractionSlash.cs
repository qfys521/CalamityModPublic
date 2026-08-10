using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class RefractionSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 512;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.Opacity = 1f;
            Projectile.timeLeft = 35;
            Projectile.MaxUpdates = 2;
            Projectile.scale = 0.75f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 12;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Projectile.timeLeft / 35f;
            if (Projectile.timeLeft == 34)
            {
                Particle spark2 = new GlowSparkParticle(Projectile.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 10, Main.rand.NextFloat(0.05f, 0.09f), (Main.rand.NextBool() ? Color.Cyan : Main.rand.NextBool() ? Color.LimeGreen : Color.PaleVioletRed) * 0.7f, new Vector2(2f, 0.5f), true);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);

        public override bool ShouldUpdatePosition() => true;

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.Cyan, Color.Lime, Projectile.identity / 7f % 1f) * Projectile.Opacity;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }
    }
}
