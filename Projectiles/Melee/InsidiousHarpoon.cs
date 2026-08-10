using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    [PierceResistException]
    public class InsidiousHarpoon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<InsidiousImpaler>();
        public int time = 0;
        public float fade = 0;

        public int strongTimeMax = 60;
        public int strongTimer = 0;

        public bool isPowered = false;
        public bool canBePowered = false;
        public Vector2 storedVel = Vector2.Zero;
        public NPC targetedNPC;
        public bool hasHitTarget = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 11;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10 * Projectile.MaxUpdates;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation = storedVel.ToRotation() + MathHelper.ToRadians(45f);

            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (time > 2)
            {
                if (!canBePowered)
                    canBePowered = true;
                Projectile.alpha = 0;
                if (fade < 1)
                    fade += 0.008f;
            }

            if (canBePowered && !isPowered)
            {
                Projectile.velocity *= 0.975f;
                strongTimer++;
                if (strongTimer >= strongTimeMax)
                {
                    SoundStyle fire = new("CalamityMod/Sounds/Item/ImpalerLaunch");
                    SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = Main.rand.NextFloat(0.2f, 0.3f) }, Projectile.Center);

                    targetedNPC = Projectile.Center.ClosestNPCAt(1200);
                    if (targetedNPC != null)
                        storedVel = (Projectile.Center - targetedNPC.Center).SafeNormalize(Vector2.UnitX) * -30;
                    Projectile.velocity = storedVel * 1.15f;
                    Particle pulse2 = new CustomPulse(Projectile.Center - storedVel * 3, -storedVel * 0.1f, Color.Chartreuse * 0.7f, "CalamityMod/Particles/DustyCircleHardEdge", new Vector2(0.4f, 1f), storedVel.ToRotation(), 0f, 0.13f, 25);
                    GeneralParticleHandler.SpawnParticle(pulse2);
                    Particle pulse = new CustomPulse(Projectile.Center - storedVel * 3, -storedVel * 0.2f, Color.Chartreuse * 0.7f, "CalamityMod/Particles/FlameExplosion", new Vector2(0.4f, 1f), storedVel.ToRotation(), 0f, 0.25f, 25);
                    GeneralParticleHandler.SpawnParticle(pulse);
                    Projectile.extraUpdates = 3;
                    strongTimer = 0;
                    isPowered = true;
                }
            }
            if (isPowered)
            {
                if (targetDist < 1400)
                {
                    if (Main.rand.NextBool(7))
                    {
                        Particle trail = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(30, 30), -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f), false, 60, Main.rand.NextFloat(0.8f, 1.3f), Color.Chartreuse);
                        GeneralParticleHandler.SpawnParticle(trail);
                    }
                    if (time % 10 == 0)
                    {
                        Particle pulse2 = new CustomPulse(Projectile.Center - Projectile.velocity, Projectile.velocity * 0.1f, Color.Chartreuse, "CalamityMod/Particles/DustyCircleHardEdge", new Vector2(0.4f, 1.1f), Projectile.velocity.ToRotation(), 0f, 0.075f, 14);
                        GeneralParticleHandler.SpawnParticle(pulse2);
                    }

                    Particle spark = new GlowSparkParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2, -1), -Projectile.velocity * 0.3f, false, 6, 0.07f, Color.Lerp(Color.Green, Color.Chartreuse, 0.8f) * 0.65f, new Vector2(1, 0.3f), true, false, 1);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                targetedNPC = hasHitTarget ? null : Projectile.Center.ClosestNPCAt(600);
                CalamityUtils.HomeInOnSelectedNPC(Projectile, targetedNPC, true, 0.6f, 23, 0.97f, accelerate: true);
            }
            else if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(13, 13), Main.rand.NextBool(7) ? 28 : 215);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f);
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.7f);
            }
            if (strongTimer == 0)
            {
                storedVel = Projectile.velocity;
            }
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(13, 13), (int)CalamityDusts.SulphurousSeaAcid);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f);
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.7f);
            }
            time++;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits < 1)
            {
                canBePowered = true;
                strongTimer = 1;
                Particle pulse3 = new GlowSparkParticle(Projectile.Center, storedVel * 1.5f, false, 7, 0.057f, Color.Chartreuse, new Vector2(1.7f, 0.8f), true);
                GeneralParticleHandler.SpawnParticle(pulse3);
                for (int i = 0; i <= 15; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.SulphurousSeaAcid, storedVel.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(1.3f, 1.8f));
                    dust.noGravity = true;
                }
                if (!isPowered)
                {
                    Projectile.velocity *= -1f;
                    SoundStyle fire = new("CalamityMod/Sounds/NPCHit/NuclearTerrorHit");
                    SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = 0.7f }, Projectile.Center);
                    SoundStyle fir2e = new("CalamityMod/Sounds/NPCHit/PerfMediumHit2");
                    SoundEngine.PlaySound(fir2e with { Volume = 0.65f, Pitch = -0.6f }, Projectile.Center);
                }
            }
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);

            modifiers.SourceDamage *= isPowered ? 1.2f : 1f;
            if (targetedNPC != null && target == targetedNPC)
                hasHitTarget = true;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 7)
                return false;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            if (isPowered || canBePowered)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color auraColor = Color.Lerp(Color.Chartreuse, Color.Lime, Utils.GetLerpValue(0, 3, i)) * 0.7f * fade;
                    Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 27f).ToRotationVector2();
                    rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 15f) * 0.5f + (isPowered ? 1.5f : 4.5f * Utils.GetLerpValue(0, strongTimeMax, strongTimer)));
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + rotationalDrawOffset, null, auraColor with { A = 0 }, storedVel.ToRotation() + MathHelper.ToRadians(45f), tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }

            if (!isPowered)
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return true;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 40, targetHitbox);
    }
}
