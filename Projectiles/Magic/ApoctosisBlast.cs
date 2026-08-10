using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class ApoctosisBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public bool chargeShot => Projectile.ai[2] == 5;
        public bool onSpawn = true;
        public float sine = 0;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 7;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 40 / MathHelper.Pi);
            if (onSpawn)
            {
                Projectile.penetrate = -1;
                Projectile.extraUpdates = 20;
                Projectile.tileCollide = false;
                onSpawn = false;
            }
            Projectile.velocity *= 0.98f;

            if (Projectile.velocity.Length() > 1 && time > 3)
            {
                Vector2 placement = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 60;
                Particle spark = new GlowSparkParticle(placement, -Projectile.velocity * 0.1f, false, 35, 0.08f, Color.Crimson, new Vector2(0.4f, 1.3f), true, false, 0);
                GeneralParticleHandler.SpawnParticle(spark);

                Vector2 vel = -Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.4f) * Main.rand.NextFloat(3.5f, 14f);
                if (time % 2 == 0)
                {
                    Particle trail = new CustomSpark(placement + Main.rand.NextVector2Circular(9, 9), vel, "CalamityMod/Particles/GlowSquareParticle", false, Main.rand.Next(17, 42), Main.rand.NextFloat(1.4f, 3.5f), Main.rand.NextBool() ? Color.Lerp(Color.Crimson, Color.White, 0.5f) : Color.Crimson, new Vector2(1f, 1f), extraRotation: MathHelper.PiOver4);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }
                

            Lighting.AddLight(Projectile.Center, Color.Crimson.ToVector3() * 0.8f);
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.35f;
            int hitsToMinMult = 10;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;

            for (int i = 0; i < 15; i++)
            {
                Particle sparks = new SparkParticle(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.7f) * Main.rand.NextFloat(7, 25), false, 35, Main.rand.NextFloat(0.5f, 0.9f), Main.rand.NextBool() ? Color.Lerp(Color.Crimson, Color.White, 0.5f) : Color.Crimson);
                GeneralParticleHandler.SpawnParticle(sparks);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player Owner = Main.player[Projectile.owner];
            if (time <= 1)
            {
                float _ = float.NaN;
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Owner.Center, Projectile.width, ref _);
            }
            else
                return base.Colliding(projHitbox, targetHitbox);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time == 0)
                return false;
            Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation - MathHelper.PiOver2;
            float timeleftFade = Utils.GetLerpValue(0, 190, Projectile.timeLeft, true);

            for (int i = 0; i < 10; i++)
            {
                float iMult = (1 - 0.06f * i);
                Vector2 scale = new Vector2((1.2f - 0.8f * iMult) * timeleftFade, 0.2f + 3 * iMult) * iMult * 0.04f;
                Vector2 bodyScale = new Vector2((1.2f - 0.8f * iMult) * timeleftFade, 0.2f + 5 * iMult) * iMult * 0.04f;
                for (int b = -1; b <= 1; b += 2)
                    Main.EntitySpriteDraw(tex2, drawPosition - Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(0.35f * b * (15 - i) * 0.06f * sine) * 65, null, Color.Lerp(Color.Crimson, Color.White, i * 0.05f) with { A = 0 } * iMult * timeleftFade, drawRotation + 0.2f * b * sine, tex2.Size() * 0.5f, scale, SpriteEffects.None);

                Main.EntitySpriteDraw(tex2, drawPosition - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 125, null, Color.Lerp(Color.Crimson, Color.White, i * 0.05f) with { A = 0 } * iMult * timeleftFade, drawRotation, tex2.Size() * 0.5f, bodyScale, SpriteEffects.None);
            }
            
            return false;
        }
    }
}
