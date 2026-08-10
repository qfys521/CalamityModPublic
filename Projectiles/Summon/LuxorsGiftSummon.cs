using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class LuxorsGiftSummon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public NPC targeted;
        public int attackTime = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.penetrate = -1; // 3
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Lime.ToVector3() * 0.35f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (targeted == null || !targeted.active || targeted.life <= 0)
                targeted = Projectile.Center.ClosestNPCAt(500);
            if (targeted != null && attackTime == 0)
            {
                CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.8f, 12, 0.93f, 0.99f);
            }
            if (attackTime > 0)
            {
                attackTime--;
                Projectile.velocity = Projectile.velocity.RotatedBy(0.09f * (Projectile.numHits % 2 == 0 ? -1 : 1));
                if (Projectile.velocity.Length() < 12)
                    Projectile.velocity *= 1.025f;
            }

            if (Projectile.timeLeft % 2 == 0)
            {
                bool sparkly = Main.rand.NextBool(3);
                Vector2 dustVel = -(Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(0.8f, 1.3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), sparkly ? DustID.FireworksRGB : ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.5f, 1.2f));
                dust.noGravity = !sparkly;
                dust.scale = Main.rand.NextFloat(0.45f, 0.6f) * (sparkly ? 1.6f : 1);
                dust.color = Color.Lime;
                dust.noLightEmittance = true;
                dust.velocity *= (sparkly ? 1 : 8);
            }

            Particle spark = new CustomSpark(Projectile.Center - Projectile.velocity * 0.5f, -Projectile.velocity * 0.01f, "CalamityMod/Particles/BloomCircle", false, 9, 0.18f, Color.Lime * 0.7f, new Vector2(0.9f, 1.2f), true, false, 0.4f);
            GeneralParticleHandler.SpawnParticle(spark);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.5f;
            int hitsToMinMult = 3;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;

            attackTime = 45;

            if (Projectile.numHits >= 2)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 dustVel = -(Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.8f) * Main.rand.NextFloat(6f, 14f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<LightDust>(), dustVel);
                    dust.noGravity = Main.rand.NextBool();
                    dust.scale = Main.rand.NextFloat(0.65f, 1.2f);
                    dust.color = Color.Lime;
                    dust.noLightEmittance = true;
                }
                Projectile.Kill();
            }
        }
        public override bool? CanHitNPC(NPC target) => (targeted != null && target == targeted && attackTime == 0) ? null : false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 8, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Color drawColor = Color.Lime;
            float fade = Utils.GetLerpValue(0, 90, Projectile.timeLeft, true);


            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 1f;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, null, drawColor with { A = 0 } * 0.4f * (float)Math.Pow(fade, 3), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
