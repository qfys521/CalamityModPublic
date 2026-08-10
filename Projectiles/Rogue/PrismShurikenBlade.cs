using System.Collections.Generic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class PrismShurikenBlade : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 260)
                CalamityUtils.HomeInOnNPC(Projectile, false, 850f, 19f, 30f);
            Projectile.velocity += Projectile.velocity.RotatedBy(MathHelper.ToRadians(90f)) * 0.02f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(4))
            {
                List<Color> eColors = new List<Color>()
                    {
                        Color.PaleVioletRed,
                        Color.Turquoise,
                        Color.OrangeRed,
                        Color.GreenYellow
                    };
                float rate = (Main.GlobalTimeWrappedHourly * 8);
                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                Color usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(-0.2f, -0.6f), usedColor, 40, Main.rand.NextFloat(0.45f, 0.6f), 0.8f, Main.rand.NextFloat(-0.2f, 0.2f), true, required: true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft <= 260)
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);
            return false;
        }
    }
}
