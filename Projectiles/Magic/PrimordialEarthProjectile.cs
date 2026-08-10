using System;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
namespace CalamityMod.Projectiles.Magic
{
    public class PrimordialEarthProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/Magic/DeathValleyDusterProjectile";
        public ref float time => ref Projectile.ai[0];
        public int rotDirection = 1;
        public float curve = 0f;
        public List<bool> buffList = new List<bool>(new bool[Main.maxPlayers]);
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 9;
            Projectile.timeLeft = 132; // NOTE: Do not change this. Specific timing is needed for the weapon to function properly.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (time == 0)
            {
                rotDirection = (Projectile.ai[1] == 1 ? 1 : -1);
                Projectile.rotation = Main.rand.NextFloat(-20, 20);
            }
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3());

            if (time < 200)
            {
                curve = MathHelper.Lerp(curve, 0.04f, 0.004f);
            }
            else
            {
                curve = MathHelper.Lerp(curve, -0.05f, 0.004f);
            }
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1] == 1 ? curve : -curve);

            if (Projectile.ai[2] == 1)
            {
                for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
                {
                    Player player = Main.player[playerIndex];
                    float targetDist = Vector2.Distance(player.Center, Projectile.Center);
                    if (targetDist < Projectile.width * 0.5f * Projectile.scale)
                    {
                        if (buffList[playerIndex] == false)
                        {
                            buffList[playerIndex] = true;
                            player.AddBuff(ModContent.BuffType<SandsWindBuff>(), 840);

                            int Dusts = 12;
                            float radians = MathHelper.TwoPi / Dusts;
                            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                            for (int i = 0; i < Dusts; i++)
                            {
                                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i) * 12.5f;
                                Dust dust = Dust.NewDustPerfect(player.Center, DustID.AmberBolt, dustVelocity, 0, default, 0.9f);
                                dust.noGravity = true;

                                Dust dust2 = Dust.NewDustPerfect(player.Center, DustID.AmberBolt, dustVelocity * 0.6f, 0, default, 1.2f);
                                dust2.noGravity = true;
                            }
                            SoundStyle buff = new("CalamityMod/Sounds/Custom/Ravager/RavagerPillarSummon");
                            SoundEngine.PlaySound(buff with { Volume = 0.65f, Pitch = 0.8f }, player.Center);
                        }
                    }
                }
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            if (time < 80)
                Projectile.scale *= 1.004f;

            if (time > 5)
            {
                int chance = Main.rand.NextBool(3) ? 2 : 1;

                if (Main.rand.NextBool(chance))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 dustPos = Projectile.Center + (i * MathHelper.Pi + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 20f * Projectile.scale;
                        Dust dust = Dust.NewDustPerfect(dustPos, Main.rand.NextBool(6) ? 262 : 287, (i * MathHelper.Pi + Projectile.rotation * Math.Sign(Projectile.velocity.Length())).ToRotationVector2() * (chance > 1 ? 7 : 3));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.75f, 1.2f);
                        dust.alpha = Main.rand.Next(100, 170 + 1);
                        dust.velocity = dust.velocity.RotatedByRandom(0.3f);
                        if (dust.type == 262)
                        {
                            dust.noGravity = true;
                        }
                        if (chance > 1)
                            dust.noGravity = true;
                    }
                }

                if (Main.rand.NextBool(4))
                {
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center + new Vector2(25, 25).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f) * Projectile.scale, (-Projectile.velocity * 0.2f).RotatedByRandom(0.2f) + (new Vector2(3, 3).RotatedByRandom(100) * (time > 96 ? 1 : 0)), Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.4f, 1.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                }

                Dust dust2 = Dust.NewDustPerfect(Projectile.Center + new Vector2(25, 25).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f) * Projectile.scale, Main.rand.NextBool(8) ? 262 : 287, -Projectile.velocity * Main.rand.NextFloat(0.1f, 1.3f));
                dust2.noGravity = true;
                dust2.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust2.alpha = 100;
            }

            Projectile.rotation += Main.rand.NextFloat(0.01f, 0.18f) * (float)Projectile.direction * rotDirection;

            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.25f;
            int hitsToMinMult = 6;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            if (Projectile.numHits == 0)
            {
                damageMult += 0.5f;
                for (int i = 0; i < 3; i++)
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 0.9f, Pitch = -0.4f + i * 0.25f, MaxInstances = 6 }, Projectile.Center);
                for (int i = 0; i < 9; i++)
                {
                    float range = Main.rand.NextFloat(-0.3f, 0.3f);
                    float power = 1 - Math.Abs(range);
                    Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(range) * Main.rand.NextFloat(35, 40) * power;
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center, vel, Color.Peru, Color.PeachPuff, Main.rand.NextFloat(1.7f, 2.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                    for (int b = 0; b < 6; b++)
                    {
                        Color clr = Color.Lerp((Main.rand.NextBool() ? Color.Peru : Color.PeachPuff), Color.Black, Main.rand.NextFloat(0.25f, 0.45f));
                        Particle sand = new CustomSpark(Projectile.Center, vel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.4f, 0.9f) * power, "CalamityMod/Particles/SmallSmoke", false, Main.rand.Next(25, 35 + 1), Projectile.scale * Main.rand.NextFloat(0.08f, 0.14f) * 7, clr, new Vector2(1, Main.rand.NextFloat(0.2f, 2f)), false, extraRotation: Main.rand.NextFloat(-4, 4), spin: Main.rand.NextFloat(-0.8f, 0.8f), affectedByLight: true);
                        GeneralParticleHandler.SpawnParticle(sand);
                    }
                }
            }
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[1] == 1)
            {
                Player Owner = Main.player[Projectile.owner];
                Owner.SetScreenshake(5f);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PrimordialEarthExplosion>(), (int)(Projectile.damage * 2.5f), 0, Projectile.owner);
                SoundStyle explo = new("CalamityMod/Sounds/Item/MagicRockImpact");
                SoundEngine.PlaySound(explo with { Volume = 0.75f }, Projectile.Center);

                Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Gold * 0.55f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 2.56f, 15);
                GeneralParticleHandler.SpawnParticle(bolt2);

                Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Gold, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.1f, 0.4f * 1.5f, 17);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.05f, 0.3f * 1.5f, 17);
                GeneralParticleHandler.SpawnParticle(orb2);
            }
            for (int i = 0; i < 65; i++)
            {
                if (Main.rand.NextBool(4))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? 262 : 287, new Vector2(7, 7).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                    if (dust.type == 262)
                    {
                        dust.noGravity = true;
                        dust.fadeIn = 0.5f;
                        dust.velocity *= 2;
                    }
                    dust.alpha = 100;
                }
                else
                {
                    Color clr = Color.Lerp((Main.rand.NextBool() ? Color.Peru : Color.PeachPuff), Color.Black, Main.rand.NextFloat(0.25f, 0.45f));
                    Particle sand = new CustomSpark(Projectile.Center, (Vector2.One * Main.rand.NextFloat(6, 15)).RotatedByRandom(MathHelper.TwoPi), "CalamityMod/Particles/SmallSmoke", true, Main.rand.Next(20, 45 + 1), Projectile.scale * Main.rand.NextFloat(0.08f, 0.14f) * 8, clr, new Vector2(1, Main.rand.NextFloat(0.2f, 2f)), false, extraRotation: Main.rand.NextFloat(-4, 4), spin: Main.rand.NextFloat(-0.8f, 0.8f), affectedByLight: true);
                    GeneralParticleHandler.SpawnParticle(sand);
                }
            }
            for (int i = 0; i < 9; i++)
            {
                Vector2 randVel = new Vector2(10, 10).RotatedByRandom(100) * Main.rand.NextFloat(0.8f, 1.6f);
                Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, Color.Peru, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.9f, 2.3f), 0.4f);
                GeneralParticleHandler.SpawnParticle(smoke);
                MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center, randVel * 0.8f, Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.4f, 1.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                GeneralParticleHandler.SpawnParticle(SandCloud);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.width * 0.5f * Projectile.scale, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft > 50)
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor * 0.4f, 1);
            return true;
        }
    }
}
