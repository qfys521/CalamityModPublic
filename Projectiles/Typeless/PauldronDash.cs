using System;
using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    [PierceResistException]
    public class PauldronDash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float ExplosionRadius = 75f;
        public Color effectsColor = Color.White;
        public float fxFade = 0;
        public bool isAlive = true;
        public bool visuals => Owner.Calamity().sPauldronVisual;
        public Vector2 aimVel;

        bool hasHit = false;
        public override void SetDefaults()
        {
            Projectile.width = (int)ExplosionRadius;
            Projectile.height = (int)ExplosionRadius;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }
        public override void AI()
        {
            if (Owner.dashDelay != -1)
                isAlive = false;
            if (isAlive)
                Projectile.timeLeft++;
            Projectile.Center = Owner.Center;

            if (effectsColor == Color.White)
                aimVel = Owner.velocity;
            else
            {
                aimVel = Vector2.Lerp(aimVel, Owner.velocity, 0.2f);
            }

            if (isAlive)
            {
                float goalSize = Utils.GetLerpValue(5, 15, Math.Abs(aimVel.X), true);
                if (goalSize < fxFade)
                    fxFade = MathHelper.Lerp(fxFade, goalSize, 0.03f);
                else
                    fxFade = goalSize;
            }
            else
            {
                fxFade = MathHelper.Lerp(fxFade, 0, 0.08f);
            }


            float rate = Main.GlobalTimeWrappedHourly * 22;
            List<Color> eColors = new List<Color>()
            {
                Color.OrangeRed,
                Color.Orange,
                Color.DarkOrange
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            effectsColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (visuals)
            {
                int dir = MathF.Sign(aimVel.X);
                if (dir == 0)
                    dir = Owner.direction;
                Vector2 safeVel = aimVel.SafeNormalize(Vector2.UnitX * dir);
                float sparkscale2 = 0.35f * Math.Max(fxFade, 0.5f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 sparkVelocity = safeVel.RotatedBy(dir * 4.3f * i) - safeVel * 3;
                    Vector2 sparkPlace = Owner.Center + (safeVel * 15).RotatedBy(2f * dir * i) * 1.5f;
                    Particle spark = new VelChangingSpark(sparkPlace, sparkVelocity, -safeVel * 5, "CalamityMod/Particles/BloomCircle", 10, sparkscale2, effectsColor * fxFade, new Vector2(0.8f, 2f), shrinkSpeed: 0.4f, lerpRate: 0.1f);
                    GeneralParticleHandler.SpawnParticle(spark);

                    if (isAlive)
                    {
                        int dustStyle = ModContent.DustType<SquashDust>();
                        Dust dust2 = Dust.NewDustPerfect(sparkPlace + safeVel * 30, dustStyle, sparkVelocity.RotatedBy(-0.6f * i * dir).RotatedByRandom(0.3f) * Main.rand.NextFloat(3, 5));
                        dust2.scale = Main.rand.NextFloat(0.9f, 1.3f);
                        dust2.color = Main.rand.NextBool() ? Color.Orange : Color.OrangeRed;
                        dust2.noGravity = true;
                    }
                }
            }

            if (Owner.dead || (!isAlive && fxFade < 0.1f) || (Owner.velocity == Vector2.Zero && Owner.dashDelay != -1))
                Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            target.AddBuff(ModContent.BuffType<Buffs.StatDebuffs.ArmorCrunch>(), 300);

            SoundStyle sound = new("CalamityMod/Sounds/Item/HolyColliderProjectileHit");
            SoundEngine.PlaySound(sound with { Volume = 0.4f, Pitch = 0.15f }, Projectile.Center);
            SoundStyle sound2 = new("CalamityMod/Sounds/Item/MagicRockImpact");
            SoundEngine.PlaySound(sound2 with { Volume = 0.6f, Pitch = 0.35f }, Projectile.Center);

            for (int i = 0; i <= 12; i++)
            {
                float variance = Main.rand.NextFloat(-0.7f, 0.7f);
                int dustStyle = ModContent.DustType<SquashDustTileTouch>();
                Dust dust2 = Dust.NewDustPerfect(target.Center, dustStyle, Projectile.velocity);
                dust2.scale = (Main.rand.NextFloat(1.2f, 1.4f) - Math.Abs(variance)) * 2.5f;
                dust2.velocity = (Vector2.UnitY * -25).RotatedBy(variance) * Main.rand.NextFloat(0.7f, 1f) * (1 - Math.Abs(variance) * 1.3f);
                dust2.color = Main.rand.NextBool() ? Color.Orange : Color.OrangeRed;
                dust2.noGravity = false;
                dust2.fadeIn = 0.35f;

                Particle spark = new SparkParticle(Projectile.Center, new Vector2(17, 17).RotatedByRandom(100) * Main.rand.NextFloat(0.4f, 1f), true, 55, 0.85f, Main.rand.NextBool() ? Color.Orange : Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (visuals)
            {
                float baseRot = Main.rand.NextFloat(-9f, 9f);
                for (int i = 0; i < 5; i++)
                {
                    float rot = Main.rand.NextFloat(-0.3f, 0.3f);
                    for (int b = 0; b < 2; b++)
                    {
                        Vector2 pulseVel = new Vector2(0, Main.rand.NextFloat(-7, -9)).RotatedBy(i * (MathHelper.ToRadians(360f) / 5)).RotatedBy(rot + baseRot + 0.9f);
                        Particle orb = new CustomPulse(target.Center, pulseVel, (Main.rand.NextBool() ? Color.Orange : Color.OrangeRed) * 0.9f, "CalamityMod/Projectiles/Summon/RustyBeaconPulse", Vector2.One, pulseVel.ToRotation(), 0.2f, Main.rand.NextFloat(0.55f, 0.85f) * 3f, Main.rand.Next(14, 19 + 1));
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                    Particle orb2 = new CustomSpark(target.Center, new Vector2(0, -7).RotatedBy(i * (MathHelper.ToRadians(360f) / 5)).RotatedBy(rot + baseRot), "CalamityMod/Particles/BloomLineFade", false, 13, 0.095f, Main.rand.NextBool() ? Color.Orange : Color.OrangeRed, new Vector2(2.9f, 0.5f), shrinkSpeed: 0.9f);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }

                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new CustomSpark(target.Center, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 20, 1.2f, Color.OrangeRed, new Vector2(1, 1), true, true, glowCenterScale: 0.7f, glowOpacity: 0.8f);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
            }

            Owner.SetScreenshake(4f);

            if (!hasHit)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<PauldronExplosion>(), Projectile.damage / 5, 0, Projectile.owner);
                hasHit = true;
            }

            Projectile.damage = (int)(Projectile.damage * 0.67f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
        public override bool? CanDamage() => (isAlive ? null : false);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (effectsColor == Color.White || !visuals)
                return false;

            float sine = MathHelper.Lerp(Math.Abs((float)Math.Sin(Main.GlobalTimeWrappedHourly * 50f / MathHelper.Pi)), 0.8f, 0.7f);
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmearLarge").Value;

            for (int i = 0; i < 5; i++)
            {
                float bScale2 = 0.75f;
                Vector2 scale = new Vector2((1 - i * 0.15f) * sine, (1 + i * 0.22f) + fxFade * 0.2f) * (bScale2 - i * 0.08f) * fxFade * 0.23f;
                Main.EntitySpriteDraw(bTexture, Owner.Center - Main.screenPosition + aimVel.SafeNormalize(Vector2.UnitX) * -15, null, Color.Lerp(effectsColor, Color.White, i * 0.15f) with { A = 0 } * fxFade, aimVel.ToRotation() + MathHelper.PiOver2, bTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            return false;
        }
        public override bool? CanCutTiles() => false;
    }
}
