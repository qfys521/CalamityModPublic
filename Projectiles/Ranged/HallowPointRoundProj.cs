using CalamityMod.Items.Ammo;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HallowPointRoundProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.MaxUpdates = 5;

            // Invisible for the first few frames
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.7f, 0.5f, 0.3f);

            // Projectile becomes visible after a few frames
            if (Projectile.timeLeft == 298)
                Projectile.alpha = 0;

            // Once projectile is visible, spawn trailing sparkles
            if (Projectile.timeLeft <= 298 && Main.rand.NextBool(5))
            {
                int idx = Dust.NewDust(Projectile.Center, 1, 1, DustID.GoldFlame, 0f, 0f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].noLight = true;
                Main.dust[idx].position = Projectile.Center;
                Main.dust[idx].velocity = Vector2.Zero;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, Color.White);
            return false;
        }

        // This bullet increases the base damage of its hits by 6.
        // While this is a "flat" addition of a fixed value, it is NOT SourceDamage.Flat, so it is not actually "flat bonus damage".
        // - The bonus base damage is pre-mitigation
        // - Damage boosts and multipliers apply to the bonus base damage. THIS INCLUDES PLAYER STATS.
        //   -> THIS IS INTENTIONAL because ammo item base damage scales with ranged damage anyway.
        // - The bonus damage transfers to on-hit effects, if any
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage.Base += HallowPointRound.BonusDamageOnHit;
        }

        // On impact, make impact sparkle and play a sound.
        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            GenericSparkle impactParticle = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.Goldenrod, Color.White, Main.rand.NextFloat(0.7f, 1.2f), 12);
            GeneralParticleHandler.SpawnParticle(impactParticle);
        }
    }
}
