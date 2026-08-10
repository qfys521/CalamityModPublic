using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class PhalanxSurgeLaser : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 7;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, Effects.ArsenalEffects.ArsenalLaserColor.ToVector3() * 0.4f);
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (time > 15 && targetDist < 1400)
            {
                if (Projectile.timeLeft % 3 == 0)
                {
                    Particle spark = new LineParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 8, 1.7f * Projectile.ai[0], Effects.ArsenalEffects.ArsenalLaserColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Projectile.timeLeft % 2 == 0)
                {
                    SparkParticle spark2 = new SparkParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 3, 0.7f * Projectile.ai[0], Color.White);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalLaserDust, (Projectile.velocity * 4).RotatedByRandom(0.1f) * Main.rand.NextFloat(0.3f, 0.8f), 0, default, Main.rand.NextFloat(0.7f, 1.3f));
                dust.noGravity = true;
                dust.color = Effects.ArsenalEffects.ArsenalLaserColor;
                dust.alpha = 100;
                dust.fadeIn = -3;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            
            if (time < 1)
                return false;
            Texture2D pointTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowBlade").Value;
            float fade = Utils.GetLerpValue(0, 15, Projectile.timeLeft, true);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(pointTexture, Projectile.Center - Main.screenPosition, null, Effects.ArsenalEffects.ArsenalLaserColor with { A = 0 } * fade * 0.4f, Projectile.rotation, pointTexture.Size() * 0.5f, new Vector2(0.7f - i * 0.1f, 1 + i * 0.15f) * 0.018f * Projectile.ai[0], SpriteEffects.None);
                Main.EntitySpriteDraw(pointTexture, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * fade * 0.2f, Projectile.rotation, pointTexture.Size() * 0.5f, new Vector2(0.7f - i * 0.1f, 1 + i * 0.15f) * 0.013f * Projectile.ai[0], SpriteEffects.None);
            }
            return false;
        }
    }
}
