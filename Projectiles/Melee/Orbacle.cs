using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee
{
    public class Orbacle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/ExtraTextures/TinyGreyscaleCircle";
        private static int Lifetime = 40;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;

            // Auric orbs never hit the same enemy more than once.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Color color = Color.Goldenrod;

            Main.EntitySpriteDraw(texture, drawPosition, null, Color.Gold * 0.2f, Projectile.rotation, origin, Projectile.scale * 1.6f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPosition, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.spriteBatch.ExitShaderRegion();

            return false;
        }
        public override void AI()
        {
            // Produces golden dust while in flight
            int dustType = Main.rand.NextBool(3) ? 244 : 246;
            float scale = 0.8f + Main.rand.NextFloat(0.6f);
            int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
            Main.dust[idx].noGravity = true;
            Main.dust[idx].velocity = Projectile.velocity / 3f;
            Main.dust[idx].scale = scale;

            if (Projectile.timeLeft < 38)
                Projectile.velocity *= 0.85f;
            if (Projectile.timeLeft == 1)
            {
                for (int i = 0; i < 14; ++i)
                {
                    dustType = Main.rand.NextBool(3) ? 244 : 246;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 0.9f));
                    dust.noGravity = true;
                    dust.scale = 0.6f;
                }
                Particle spark = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Gold, new Vector2(1, 1), 0, 0f, 0.4f, 8);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
