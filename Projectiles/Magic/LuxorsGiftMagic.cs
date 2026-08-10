using System;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class LuxorsGiftMagic : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }
        public float velLerp => Utils.GetLerpValue(1, 8, Projectile.velocity.Length(), true);
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 360;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 0.35f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.timeLeft == 360)
                Projectile.velocity *= 0.1f;
            if (Projectile.velocity.Length() < 8)
                Projectile.velocity *= 1.01f;

            if (Projectile.timeLeft > 30)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = (MathHelper.TwoPi * i / 2f).ToRotationVector2().RotatedBy(Projectile.timeLeft * 0.1f) * 35 * velLerp;
                    Vector2 dustVel = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.1f) * Main.rand.NextFloat(5.5f, 6.5f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.FireworksRGB, dustVel);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.4f, 0.5f);
                    dust.color = Color.Gold;
                    dust.noLightEmittance = true;
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.5f;
            int hitsToMinMult = 3;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
            for (int g = 0; g < 4; g++)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = (MathHelper.TwoPi * i / 2f).ToRotationVector2().RotatedBy(Projectile.timeLeft * 0.1f) * 35 * velLerp;
                    Vector2 dustVel = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, ModContent.DustType<LightDust>(), dustVel.RotatedByRandom(0.3f) * Main.rand.NextFloat(5.5f, 9.5f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.2f, 1.5f);
                    dust.color = Color.Gold;
                    dust.noLightEmittance = true;
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 10 + 65 * velLerp, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Color drawColor = Color.Gold;
            float fade = Utils.GetLerpValue(0, 90, Projectile.timeLeft, true);

            for (int i = 0; i < 2; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 2f).ToRotationVector2().RotatedBy(Projectile.timeLeft * 0.1f) * 45 * velLerp;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, null, drawColor with { A = 0 } * 0.6f * (float)Math.Pow(fade, 3), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() * 0.5f, new Vector2(1 - 0.3f * velLerp, 1 + 0.8f * velLerp) * Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
