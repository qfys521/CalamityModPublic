using System;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class LuxorsGiftRanged : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 5;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.35f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.timeLeft == 300)
                Projectile.velocity *= 0.5f;
            if (Projectile.timeLeft > 30)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * (i == 0 ? -3 : 3);
                    Vector2 dustVel = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(2.5f, 8.5f);
                    Dust dust = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(2, 2), Main.rand.NextBool() ? 278 : ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.5f, 1.2f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.3f, 0.4f);
                    dust.color = Color.Cyan;
                    dust.noLightEmittance = true;
                }
            }
            if (!Collision.SolidCollision(Projectile.Center, 35, 35) || Projectile.timeLeft < 200)
                Projectile.tileCollide = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.5f;
            int hitsToMinMult = 3;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(2.5f, 8.5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(11, 11), Main.rand.NextBool() ? 278 : ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.3f, 1.7f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.1f);
                dust.color = Color.Cyan;
                dust.noLightEmittance = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 20, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Color drawColor = Color.Cyan;
            float fade = Utils.GetLerpValue(0, 90, Projectile.timeLeft, true);

            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 2f;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, null, drawColor with { A = 0 } * 0.4f * (float)Math.Pow(fade, 3), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
