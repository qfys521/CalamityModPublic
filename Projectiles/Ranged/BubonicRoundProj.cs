using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class BubonicRoundProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 3;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, 0.25f, 0f);

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 4f)
            {
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SteampunkSteam, -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.01f, 0.15f));
                    dust.scale = Main.rand.NextFloat(0.7f, 0.85f);
                    dust.noGravity = true;
                    if (Main.rand.NextBool(3))
                        dust.color = Color.LimeGreen;
                    else
                        dust.color = Color.Lime;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            //Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            return true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, lightColor);
            return false;
        }

        // This projectile is always fullbright.
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(1f, 1f, 1f, 0f);
        }

        // Ignores 10% of armor
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.ScalingArmorPenetration += 0.1f;
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) => modifiers.ScalingArmorPenetration += 0.1f;

        // Inflicts Plague for 0.75 seconds
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Plague>(), 60);

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 2; k++)
            {
                float pulseScale = Main.rand.NextFloat(0.3f, 0.4f);
                DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center + Main.rand.NextVector2Circular(12, 12), new Vector2(2, 2).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.1f), (Main.rand.NextBool(3) ? Color.LimeGreen : Color.Green) * 0.8f, new Vector2(1, 1), pulseScale - 0.25f, pulseScale, 0f, 15);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
            for (int b = 0; b < 6; b++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Terra, new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 1.1f);
                dust.alpha = 200;
            }
        }
    }
}
