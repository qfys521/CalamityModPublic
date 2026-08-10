using System;
using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class StarfleetStar : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public Color c1 = new Color(146, 255, 211);
        public Color c2 = new Color(222, 225, 146);
        public Color c3 = new Color(255, 233, 146);
        public Color shiftColor;
        public Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (time == 0 && Projectile.ai[1] > 0)
                Projectile.extraUpdates = (int)Projectile.ai[1];

            float rate = (Projectile.ai[2] * 0.05f);
            List<Color> eColors = new List<Color>()
                {
                    c1,
                    c2,
                    c3,
                };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            shiftColor = Color.Lerp(currentColor, nextColor, rate % 2f >= 1f ? 1f : rate % 1f);
            Projectile.ai[2]++;

            if (time > 5 && time % 2 == 0)
            {
                Particle trail = new CustomSpark(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 28, Projectile.velocity, "CalamityMod/Particles/DualTrail", false, 15, 0.11f, shiftColor * 0.6f, new Vector2(0.9f, 1.1f), shrinkSpeed: 0.5f);
                GeneralParticleHandler.SpawnParticle(trail);

                if (time > 18)
                {
                    int dustStyle = DustType<SquashDust>();
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), dustStyle);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                    dust.velocity = -Projectile.velocity * Main.rand.NextFloat(3f, 5f);
                    dust.noGravity = true;
                    dust.color = shiftColor;
                    dust.fadeIn = 5f;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            time++;
        }
        public Color GetRandomColor()
        {
            Color useColor = Main.rand.Next(4) switch
            {
                0 => c1,
                1 => c2,
                _ => c3,
            };
            return useColor;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D glowTexture = Request<Texture2D>("CalamityMod/Projectiles/Rogue/LeonidStar").Value;
            Texture2D tex2 = Request<Texture2D>("CalamityMod/Particles/FadeStreak").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = glowTexture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 5).ToRotationVector2().RotatedBy(Projectile.rotation + MathHelper.Pi);
                for (int t = 0; t < 2; t++)
                    Main.EntitySpriteDraw(tex2, drawPosition, null, shiftColor with { A = 0 } * 0.4f, offset.ToRotation(), new Vector2(tex2.Width * 0.5f, 0), new Vector2((2.3f + t * 0.03f) * (i == 2 ? 1.8f : i == 3 ? 1.8f : 1), (1.1f + t * 0.03f) * (i == 2 ? 0.75f : i == 3 ? 0.75f : 1)) * Projectile.scale * Owner.gravDir * 0.3f, SpriteEffects.FlipVertically);
                Main.EntitySpriteDraw(tex2, drawPosition, null, Color.White with { A = 0 } * 0.4f, offset.ToRotation(), new Vector2(tex2.Width * 0.5f, 0), new Vector2(1.5f, 0.7f) * Projectile.scale * Owner.gravDir * 0.3f, SpriteEffects.FlipVertically);
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.1f;
            int hitsToMinMult = 2;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                float variance = Main.rand.NextFloat(-0.5f, 0.5f);
                int dustStyle = DustType<SquashDust>();
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustStyle);
                dust.scale = (Main.rand.NextFloat(1.4f, 1.8f) - Math.Abs(variance)) * 1.5f;
                dust.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(variance) * Main.rand.NextFloat(18f, 19f) * (float)Math.Pow(1 - Math.Abs(variance), 2);
                dust.noGravity = true;
                dust.color = GetRandomColor();
                dust.fadeIn = 2.5f;
            }
        }

    }
}
