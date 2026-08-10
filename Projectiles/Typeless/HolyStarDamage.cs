using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class HolyStarDamage : ModProjectile, ILocalizedModType
    {
        bool started = false;
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public int time = 0;
        public bool reachedMaxDamage = false;
        public override void SetDefaults()
        {
            Projectile.localAI[1] = Main.rand.NextFloat(30f);
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0f);

            if (!started)
            {
                Color cl = Color.Goldenrod;
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, cl, "CalamityMod/Particles/BlastCone", new Vector2(Main.rand.NextFloat(2f, 4f), 1.5f), Vector2.Zero.AngleTo(Projectile.velocity), 1f * Projectile.scale, 0f, 30));
                started = true;
            }

            if (Projectile.ai[0] < 240f)
            {
                Projectile.ai[0] += 1f;

                if (Projectile.timeLeft < 160)
                    Projectile.timeLeft = 160;
            }

            if (Projectile.ai[1] == 5)
            {
                // Homing stars from burning revalation
                if (Projectile.ai[2] != -1 && time > 20)
                {
                    NPC targeted = Projectile.ai[2] == -1 ? null : Main.npc[(int)Projectile.ai[2]];
                    if (targeted != null && (targeted.life <= 0 || !targeted.CanBeChasedBy(Projectile)))
                        targeted = null;
                    else
                        Projectile.timeLeft++;
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted ?? Projectile.Center.ClosestNPCAt(800f), true, 0.5f, 12, 0.975f);
                }
                if (Projectile.scale < 1.15)
                    Projectile.scale += 0.004f;
                if (Projectile.scale >= 1.15 && !reachedMaxDamage)
                {
                    Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/BloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 0.3f, 8);
                    GeneralParticleHandler.SpawnParticle(orb2);
                    reachedMaxDamage = true;
                }
            }

            if (Projectile.velocity.Length() < 16f)
                Projectile.velocity *= 1.01f;

            Projectile.localAI[1] += (Projectile.velocity.Length() / 20);

            Color col = Projectile.ai[1] == 5 ? (Color.Lerp(Color.Goldenrod, Color.Orchid, Utils.GetLerpValue(0.85f, 1f, Projectile.scale))) : Color.Goldenrod;

            Particle spark = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.8f, false, 5, 0.06f * Projectile.scale, col * 0.7f, new Vector2(1, 0.3f), true, false, 1.5f);
            GeneralParticleHandler.SpawnParticle(spark);

            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 240);
            if (Projectile.ai[1] == 5)
            {
                modifiers.SourceDamage *= Projectile.scale;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float lerpMult = MathHelper.Lerp(0.5f, 1.5f, Math.Abs((float)Math.Sin(Projectile.localAI[1] / 10f)));

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color baseColor = Color.Goldenrod with { A = 150 };
            Color baseColor2 = Color.Khaki with { A = 150 };
            baseColor *= lerpMult;
            baseColor2 *= lerpMult;
            Vector2 origin = texture.Size() / 2f;
            Vector2 scale = new Vector2(0.5f, 1.5f) * lerpMult;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Projectile.rotation += MathHelper.ToRadians(lerpMult * 4f);

            float upRight = MathHelper.PiOver4;
            float up = MathHelper.PiOver2;
            float upLeft = 3f * MathHelper.PiOver4;
            float left = MathHelper.Pi;
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, upLeft + Projectile.rotation, origin, scale * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, upRight - Projectile.rotation, origin, scale * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, upLeft + Projectile.rotation, origin, scale * 0.6f * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, upRight - Projectile.rotation, origin, scale * 0.6f * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, up + Projectile.rotation, origin, scale * 0.6f * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, left - Projectile.rotation, origin, scale * 0.6f * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, up + Projectile.rotation, origin, scale * 0.36f * Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, left - Projectile.rotation, origin, scale * 0.36f * Projectile.scale, spriteEffects, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            int dustType = ModContent.DustType<LightDust>();
            for (int i = 0; i < 7; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, ((new Vector2(7, 7) * Projectile.scale).RotatedByRandom(100) + (reachedMaxDamage ? Vector2.Zero : Projectile.velocity)) * Main.rand.NextFloat(0.2f, 1f) * (reachedMaxDamage ? 2 : 1));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.95f, 1.45f) * Projectile.scale;
                dust.color = Main.rand.NextBool(4) ? (reachedMaxDamage ? Color.Orchid : Color.Khaki) : Color.Goldenrod;
                dust.noLightEmittance = true;
            }
            if (reachedMaxDamage)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.7f }, Projectile.Center);

                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Goldenrod, "CalamityMod/Particles/BloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 1.5f, 10);
                GeneralParticleHandler.SpawnParticle(orb2);

                for (int i = 0; i < 4; i++)
                {
                    Particle spark = new CustomSpark(Projectile.Center, new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(1.15f, 1.3f), Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f)), new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.3f, 0.4f));
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    // Sub projectiles spawning sub explosions... yea it needs armor pen
                    Projectile explo = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BurningHolyBlast>(), (int)(Projectile.damage * 1.2f), Projectile.knockBack, Projectile.owner, 0.75f);
                    explo.ArmorPenetration = 30;
                }
            }
        }
        public override bool? CanDamage() => time > 15 ? null : false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 50, targetHitbox);
    }
}
