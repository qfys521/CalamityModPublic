using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Melee
{
    public class BitingEmbraceMist : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Particles/MediumMist";
        public Player Owner => Main.player[Projectile.owner];
        public Color mistColor;
        public int variant = 0;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = Projectile.height = 34;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 size = Projectile.Size * Projectile.scale;
            return Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - size / 2f, size);
        }

        public override void OnSpawn(IEntitySource source)
        {
            mistColor = Main.hslToRgb(Main.rand.NextFloat(0.5f, 0.8f), 1f, 0.8f);
            variant = Main.rand.Next(3);
        }

        public override void AI()
        {
            if (Main.rand.NextBool(15) && Projectile.alpha <= 140) //only try to spawn your particles if you're not close to dying
            {
                Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * Projectile.scale * 0.5f, Projectile.height * Projectile.scale * 0.5f);
                if (Main.rand.NextBool())
                {
                    Particle snowflake = new SnowflakeSparkle(particlePosition, Vector2.Zero, Color.White, new Color(75, 177, 250), Main.rand.NextFloat(0.3f, 1.5f), 40, 0.5f);
                    GeneralParticleHandler.SpawnParticle(snowflake);
                }
                else
                {
                    float scale = Main.rand.NextFloat(0.5f, 1.8f);
                    Particle star = new CritSpark(particlePosition, Vector2.Zero, Color.White, Color.Indigo, scale, 30, 0.5f, scale * 2f);
                    GeneralParticleHandler.SpawnParticle(star);
                }
            }

            Projectile.velocity *= 0.85f;
            Projectile.position += Projectile.velocity;
            Projectile.rotation += 0.02f * Projectile.timeLeft / 300f * ((Projectile.velocity.X > 0) ? 1f : -1f);

            if (Projectile.alpha < 165)
            {
                Projectile.scale += 0.05f;
                Projectile.alpha += 2;
            }
            else
            {
                Projectile.scale *= 0.975f;
                Projectile.alpha += 1;
            }
            if (Projectile.alpha >= 170)
                Projectile.Kill();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            var tex = Request<Texture2D>("CalamityMod/Particles/MediumMist").Value;
            Rectangle frame = tex.Frame(1, 3, 0, variant);
            Main.EntitySpriteDraw(tex, Projectile.position - Main.screenPosition, frame, mistColor * 0.5f * ((255f - Projectile.alpha) / 255f), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

    }
}
