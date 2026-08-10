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
    public class StarmadaHitProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public Color c1 = new Color(164, 47, 160);
        public Color c2 = new Color(227, 97, 72);
        public Color c3 = new Color(193, 255, 146);
        public Color shiftColor;
        public bool move = false;
        public NPC targeted;
        public Vector2 targetPos;
        public Vector2 distFromPos;
        public float squash = 1;
        public bool launched => Projectile.localAI[2] == 5 && move;
        public bool setLaunchStats = true;
        public Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 3;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 800;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
        }
        public void FindTarget()
        {
            if (Projectile.ai[0] != -5)
                targeted = Main.npc[(int)Projectile.ai[0]];
            if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active)
            {
                targeted = Projectile.Center.ClosestNPCAt(800);
                Projectile.ai[0] = -5;
            }
        }

        public override void AI()
        {
            if (time == 0)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                targetPos = Projectile.Center;
            }
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

            float startTime = 120 + Projectile.ai[1];
            float endTime = startTime + 15 + Projectile.ai[1];
            if (launched)
            {
                if (setLaunchStats)
                {
                    Projectile.extraUpdates = 12;
                    Projectile.penetrate = 1;
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;

                    setLaunchStats = false;
                }
                Projectile.rotation += (MathHelper.TwoPi) / (endTime / 3);
                if (time % 2 == 0)
                {
                    Particle trail = new CustomSpark(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 28, Projectile.velocity, "CalamityMod/Particles/BloomCircle", false, 13, 0.11f, shiftColor * 0.6f, new Vector2(launched ? 5.5f : 0.8f, 3f), shrinkSpeed: launched ? 1f : 0.5f);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }
            else
            {
                if (move)
                {
                    squash = MathHelper.Lerp(squash, (time < endTime ? 0.15f : 1f), 0.08f);
                    if (time == endTime)
                        Projectile.localAI[2] = 1;
                    if (time >= endTime)
                    {
                        if (Main.rand.NextBool(15))
                        {
                            int dustStyle = DustType<SquashDust>();
                            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), dustStyle);
                            dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                            dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(5f, 7f);
                            dust.noGravity = true;
                            dust.color = shiftColor;
                            dust.fadeIn = 2f;
                        }

                        Projectile.Center += Projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.2f;
                        if (time < endTime * 2f)
                            Projectile.velocity *= 0.9f + (Projectile.ai[1] * 0.005f);

                        Projectile.rotation += (MathHelper.TwoPi) / (endTime / 3);
                    }
                    else
                    {
                        Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
                        Particle trail = new CustomSpark(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 28, Projectile.velocity, "CalamityMod/Particles/BloomCircle", false, 15, 0.3f, shiftColor, new Vector2(0.8f, 1.3f), shrinkSpeed: 0.5f);
                        GeneralParticleHandler.SpawnParticle(trail);
                    }
                }
                else
                {
                    Projectile.rotation += MathHelper.TwoPi / (startTime / 3);

                    FindTarget();
                    if (targeted != null)
                    {
                        targetPos = targeted.Center;
                    }
                    if (distFromPos != Vector2.Zero)
                    {
                        Projectile.Center = targetPos + distFromPos;
                    }
                    else if (targeted != null)
                        distFromPos = (Projectile.Center - targeted.Center);


                    distFromPos += -Projectile.velocity * 3.5f;
                    Projectile.velocity *= 0.97f + (Projectile.ai[1] * 0.0025f);
                }
            }
            if (time == startTime && setLaunchStats)
            {
                Projectile.timeLeft = (int)(endTime * 3);
                move = true;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15;
            }

            Projectile.scale = (float)Math.Pow(Math.Min(Utils.GetLerpValue(0, 40, time, true), Utils.GetLerpValue(0, 40, Projectile.timeLeft, true)), 3);

            time++;
        }
        public override bool ShouldUpdatePosition() => move;
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
            Texture2D star = Request<Texture2D>("CalamityMod/ExtraTextures/SimpleStar").Value;
            Texture2D tex = Request<Texture2D>("CalamityMod/Particles/CircularSmear").Value;
            Texture2D tex2 = Request<Texture2D>("CalamityMod/Particles/FadeStreak").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = star.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, drawPosition, null, shiftColor with { A = 0 } * Math.Max((float)Math.Pow(squash, 5) - 0.15f, 0), Projectile.rotation * 1.6f, tex.Size() * 0.5f, Projectile.scale * Owner.gravDir * 0.39f, SpriteEffects.None);
            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(star, drawPosition, null, i == 0 ? Color.White with { A = 0 } * 0.3f : (shiftColor with { A = 0 } * 0.6f), Projectile.rotation, rotationPoint, new Vector2(1f * squash, 1f + (1 - squash)) * Projectile.scale * Owner.gravDir * (i == 0 ? 0.12f : 0.2f), SpriteEffects.None);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

            float minMult = 0.1f;
            int hitsToMinMult = 2;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult * (launched ? 2 : 1);
        }
        public override void OnKill(int timeLeft)
        {
            if (launched)
            {
                float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                for (int b = 1; b <= 6; b++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int dustStyle = DustType<SquashDust>();
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, dustStyle);
                        dust.scale = 8 - b;
                        dust.velocity = -Vector2.UnitY.RotatedBy((MathHelper.TwoPi / 5 * i) + rot) * (b * 1.75f + 2);
                        dust.noGravity = true;
                        dust.color = GetRandomColor();
                        dust.fadeIn = 6.5f - b * 0.5f;
                    }
                }
                Particle bloom = new CustomSpark(Projectile.Center, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 25, 1.2f, shiftColor, new Vector2(1, 1), true, true, 0, false, false, glowOpacity: 0.85f);
                GeneralParticleHandler.SpawnParticle(bloom);
            }
        }

    }
}
