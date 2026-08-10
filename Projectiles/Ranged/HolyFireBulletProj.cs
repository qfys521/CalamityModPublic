using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Ammo;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HolyFireBulletProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public Color col = Color.White;
        private float SizeVariance;
        public ref float time => ref Projectile.ai[2];
        public ref float sizeBonus => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 7;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (time == 0)
            {
                Projectile.damage = (int)(Projectile.damage * 1.15f);
                col = Main.rand.NextBool() ? Color.Orange : Color.Goldenrod;
                SizeVariance = Main.rand.NextFloat(0.95f, 1.05f);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;

            sizeBonus = MathHelper.Lerp(1, 2, (float)Math.Pow(Utils.GetLerpValue(50, 15, time, true), 2));
            if (time > 4)
            {
                float sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 15f / MathHelper.Pi);
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<SquashDust>(), -Projectile.velocity.RotatedBy(Main.rand.NextFloat(0.15f, 0.3f) * (Main.rand.NextBool() ? -1 : 1)) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 0.85f);
                    dust.noLightEmittance = true;
                    dust.color = col;
                }

                Player Owner = Main.player[Projectile.owner];
                float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
                if (time > 2 && targetDist < 1400)
                {
                    Particle trail = new CustomSpark(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.5f, "CalamityMod/Particles/DualTrail", false, 4, 0.03f, col * 0.9f, new Vector2(1f, 3), true, true, shrinkSpeed: 0.8f, glowOpacity: 0.6f);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }
            time++;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time <= 0)
                return false;
            Asset<Texture2D> tip = ModContent.Request<Texture2D>("CalamityMod/Particles/SquareRotated");

            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(tip.Value, Projectile.Center - Main.screenPosition, null, Color.Lerp(col, Color.White, i) with { A = 0 } * 0.7f, Projectile.rotation, tip.Size() / 2f, new Vector2(0.2f, 1.4f) * Projectile.scale * (0.28f - 0.1f * i), SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn an on-hit explosion which deals part of the projectile's damage to enemies around the target
            if (Projectile.numHits == 0)
            {
                MakeBlast(0, false);
            }
        }
        public void MakeBlast(int target, bool hitTarget)
        {
            float blastSize = 50 * sizeBonus;
            float minMultiplier = 0.35f;
            int hitsToMinMult = 8;
            int blastDamage = (int)(Projectile.damage * 0.33f);
            int knockback = -10;
            int debuff = ModContent.BuffType<HolyFlames>();
            int debuffTime = 180;
            if (hitTarget)
            {
                Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurstExclusive>(), blastDamage, knockback, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                blast.timeLeft = 3;
                blast.DamageType = Projectile.DamageType;
                blast.localAI[0] = target;
                blast.localAI[1] = debuff;
                blast.localAI[2] = debuffTime;
            }
            else
            {
                Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), blastDamage, knockback, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                blast.timeLeft = 3;
                blast.DamageType = Projectile.DamageType;
                blast.localAI[0] = debuff;
                blast.localAI[1] = debuffTime;
            }

            SoundEngine.PlaySound(HolyFireBullet.Explosion with { Pitch = -0.2f + 0.3f * sizeBonus, Volume = 0.3f + 0.1f * sizeBonus, MaxInstances = 10 }, Projectile.Center);

            float fxScale = MathHelper.Lerp(sizeBonus, 1, 0.25f);
            Vector2 Offset = Main.rand.NextVector2Circular(15, 15);

            Particle blastvfx = new CustomPulse(Projectile.Center + Offset, Vector2.Zero, Color.Lerp(Color.SlateGray, col, 0.8f) * 0.9f, "CalamityMod/Particles/SmokeExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.05f * fxScale, 0.1f * fxScale, Main.rand.Next(8, 10 + 1), true);
            GeneralParticleHandler.SpawnParticle(blastvfx);

            float rot = Main.rand.NextFloat(-5, 5);
            for (int i = -1; i <= 1; i += 2)
            {
                Particle centerShine = new CustomSpark(Projectile.Center + Offset, Vector2.UnitY.RotatedBy(rot + MathHelper.PiOver4 * i) * 0.01f, "CalamityMod/Particles/BloomCircle", false, 5, 0.3f * fxScale, Color.White, new Vector2(1f, 1f), true, false, shrinkSpeed: 2f);
                GeneralParticleHandler.SpawnParticle(centerShine);
            }

            for (int k = 0; k < 7; k++)
            {
                if (Main.rand.NextBool())
                {
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + Offset, ModContent.DustType<DiamondDust>(), new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f) * fxScale);
                    dust2.noGravity = true;
                    dust2.scale = Main.rand.NextFloat(0.4f, 0.6f) * fxScale;
                    dust2.alpha = Main.rand.Next(90, 180 + 1);
                    dust2.color = col;
                    dust2.fadeIn = 10f;
                    dust2.noLight = true;
                    dust2.noLightEmittance = true;
                }
                else
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Offset, ModContent.DustType<LightDust>(), new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f) * fxScale);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 1f) * fxScale;
                    dust.alpha = Main.rand.Next(160, 230 + 1); ;
                    dust.color = Color.White;
                    dust.noLight = true;
                    dust.noLightEmittance = true;
                }
            }
            for (int k = 0; k < 2; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Offset, ModContent.DustType<SquashDust>(), new Vector2(6, 6).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f) * fxScale + new Vector2(0, -5));
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.75f, 0.95f) * fxScale;
                dust.color = Main.rand.NextBool() ? Color.Orange : Color.Goldenrod;
                dust.fadeIn = 1;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300);
            MakeBlast(target.whoAmI, true);
            if (sizeBonus == 2) // If you're at max damage get extra damage
            {
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.3f, Volume = 0.4f, MaxInstances = 10 }, Projectile.Center);
            }
            // This used to be 25% was I fucking insane???
            modifiers.SourceDamage *= MathHelper.Lerp(1, 1.15f, sizeBonus - 1); // Up to 15% damage bonus
        }
    }
}
