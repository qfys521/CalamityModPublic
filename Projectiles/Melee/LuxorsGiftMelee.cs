using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class LuxorsGiftMelee : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // Works like 2
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.35f);
            Projectile.scale = (Projectile.ai[0] == 5 ? 0.5f : 0.75f);
            Projectile.velocity *= 0.973f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.ai[0] == 5)
            {
                if (Projectile.timeLeft % 9 == 0 && Projectile.timeLeft > 30)
                {
                    Vector2 dustVel = -(Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(0.8f, 1.3f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.5f, 1.2f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.45f, 0.6f);
                    dust.color = Color.Red;
                    dust.alpha = 120;
                    dust.noLightEmittance = true;
                }
            }
            else if (Projectile.timeLeft % 4 == 0 && Projectile.timeLeft > 30)
            {
                Vector2 dustVel = -(Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(0.8f, 1.3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.5f, 1.2f));
                dust.noGravity = !Main.rand.NextBool(3);
                dust.scale = Main.rand.NextFloat(0.45f, 0.6f);
                dust.color = Color.Red;
                dust.noLightEmittance = true;
            }
            if (Projectile.timeLeft > 90 && Projectile.timeLeft < 177 && Projectile.timeLeft % 2 == 0 && Projectile.ai[0] == 0)
            {
                Particle spark = new GlowSparkParticle(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2, -Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2, false, 14, 0.02f, Color.Red * 0.45f, new Vector2(1.4f, 1f), true, false, 0.6f);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Collision.SolidCollision(Projectile.Center, 9, 9) && Projectile.timeLeft <= 150)
                Projectile.velocity *= 0.91f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.5f;
            int hitsToMinMult = 3;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
            if (Projectile.numHits >= 1 && Projectile.timeLeft > 70)
                Projectile.timeLeft = 70;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => (Projectile.ai[0] == 5 || Projectile.numHits >= 2) ? false : CalamityUtils.CircularHitboxCollision(Projectile.Center, 20, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Color drawColor = Color.Red;
            float fade = Utils.GetLerpValue(0, 90, Projectile.timeLeft, true);


            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 2f;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, null, drawColor with { A = 0 } * 0.4f * (float)Math.Pow(fade, 3), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }
    }
}
