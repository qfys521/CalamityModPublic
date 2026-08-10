using System;
using System.Collections.Generic;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class PrimordialAncientProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float time => ref Projectile.ai[0];
        public int rotDirection = 1;
        public float curve = 0.02f;
        public bool goToCursor = false;
        public Vector2 mousePos;
        public float CenterX;
        public float CenterY;
        public List<bool> buffList = new List<bool>(new bool[Main.maxPlayers]);
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 8;
            Projectile.timeLeft = 430;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (time == 0)
            {
                rotDirection = (Main.rand.NextBool() ? 1 : -1);
                Projectile.rotation = Main.rand.NextFloat(-20, 20);
                Projectile.scale = 0.5f;
            }
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.3f);

            if (!goToCursor)
            {
                curve = MathHelper.Lerp(curve, 0f, 0.035f);
                Projectile.velocity *= Main.rand.NextFloat(0.985f, 0.995f);
                if (Projectile.ai[1] != 1)
                    Projectile.velocity = Projectile.velocity.RotatedBy(curve * Projectile.localAI[0]);
            }

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
                            player.AddBuff(ModContent.BuffType<AeolianEarthBuff>(), 1080);

                            int Dusts = 8;
                            float radians = MathHelper.TwoPi / Dusts;
                            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                            for (int i = 0; i < Dusts; i++)
                            {
                                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i) * 12.5f;
                                GlowSparkParticle spark = new GlowSparkParticle(Projectile.Center, dustVelocity * 0.7f, false, 12, 0.009f, Color.Purple, new Vector2(3.5f, 1.3f), true);
                                GeneralParticleHandler.SpawnParticle(spark);

                                Dust dust = Dust.NewDustPerfect(player.Center, DustID.GemAmethyst, dustVelocity.RotatedBy(MathHelper.ToRadians(22.5f)), 0, default, 0.9f);
                                dust.noGravity = true;
                                Dust dust2 = Dust.NewDustPerfect(player.Center, DustID.GemAmethyst, dustVelocity.RotatedBy(MathHelper.ToRadians(22.5f)) * 0.4f, 0, default, 1.2f);
                                dust2.noGravity = true;
                            }

                            SoundStyle buff = new("CalamityMod/Sounds/Custom/Ravager/RavagerPillarSummon");
                            SoundEngine.PlaySound(buff with { Volume = 0.65f, Pitch = 0.8f }, player.Center);
                        }
                    }
                }
            }

            if (Projectile.timeLeft % 2 == 0)
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
                Projectile.scale *= 1.007f;

            if (time > 5)
            {
                int chance = 5;

                if (Main.rand.NextBool(chance))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 dustPos = Projectile.Center + (i * MathHelper.Pi + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 20f * Projectile.scale;
                        Dust dust = Dust.NewDustPerfect(dustPos, Main.rand.NextBool(5) ? 86 : 287, (i * MathHelper.Pi + Projectile.rotation * Math.Sign(Projectile.velocity.Length())).ToRotationVector2() * (chance > 1 ? 7 : 3));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.75f, 1.2f);
                        dust.alpha = Main.rand.Next(100, 170 + 1);
                        dust.velocity = dust.velocity.RotatedByRandom(0.3f);
                        if (dust.type == 86)
                        {
                            dust.noGravity = true;
                        }
                        if (chance > 1)
                            dust.noGravity = true;
                    }
                }
                if (time < 350)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 dustPos = Projectile.Center + (i * MathHelper.Pi + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 60f * Projectile.scale * Utils.GetLerpValue(350, 250, time, true);
                        Dust dust = Dust.NewDustPerfect(dustPos, DustID.WitherLightning, (i * MathHelper.Pi + Projectile.rotation * Math.Sign(Projectile.velocity.Length())).ToRotationVector2());
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.4f, 0.6f);
                    }
                }
                if (time == 350)
                {
                    Projectile.velocity = Vector2.Zero;

                    Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Purple, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0f, 0.52f, 9);
                    GeneralParticleHandler.SpawnParticle(orb);
                    Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0f, 0.46f, 9);
                    GeneralParticleHandler.SpawnParticle(orb2);

                    SoundStyle explo = new("CalamityMod/Sounds/Item/MagicRockImpact");
                    SoundEngine.PlaySound(explo with { Volume = 0.75f, Pitch = 0.35f }, Projectile.Center);
                    mousePos = Main.player[Projectile.owner].ClampedMouseWorld();
                    goToCursor = true;
                }
                if (goToCursor)
                {
                    mousePos = Main.player[Projectile.owner].ClampedMouseWorld();
                    if (time == 355)
                    {
                        CenterX = Projectile.Center.X;
                        CenterY = Projectile.Center.Y;
                    }
                    if (time > 360 && Projectile.timeLeft % 4 == 0 && Projectile.timeLeft > 15)
                    {
                        Vector2 trailVel = (new Vector2(CenterX, CenterY) - Projectile.Center).SafeNormalize(Vector2.UnitX) * 0.5f;
                        float size = Projectile.width * 0.5f * Projectile.scale;
                        GlowSparkParticle spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(size, size) - trailVel * 4, trailVel, false, 11, 0.005f, Color.Purple, new Vector2(3.5f, 1.3f), true);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    if (time > 360 && Projectile.timeLeft > 15)
                    {
                        float size = Projectile.width * 0.03f * Projectile.scale;
                        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(size, size);
                        Vector2 trailVel = (new Vector2(CenterX, CenterY) - Projectile.Center).SafeNormalize(Vector2.UnitX) * 0.5f;
                        Dust dust = Dust.NewDustPerfect(dustPos, DustID.WitherLightning, trailVel);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.4f, 0.6f);
                    }
                    if (time > 355)
                        Projectile.Center = new Vector2(MathHelper.Lerp(CenterX, mousePos.X, Utils.GetLerpValue(355, 430, time, true)), MathHelper.Lerp(CenterY, mousePos.Y, Utils.GetLerpValue(355, 430, time, true)));
                }

                if (Main.rand.NextBool(6))
                {
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center + new Vector2(25, 25).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f) * Projectile.scale, (-Projectile.velocity * 0.2f).RotatedByRandom(0.2f) + (new Vector2(3, 3).RotatedByRandom(100) * (time > 96 ? 1 : 0)), Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.4f, 1.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                }

                Dust dust2 = Dust.NewDustPerfect(Projectile.Center + new Vector2(25, 25).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f) * Projectile.scale, Main.rand.NextBool(5) ? 86 : 287, -Projectile.velocity * Main.rand.NextFloat(0.1f, 1.3f));
                dust2.noGravity = true;
                dust2.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust2.alpha = 100;
            }

            Projectile.rotation += Main.rand.NextFloat(0.1f * Utils.GetLerpValue(-100, 360, time)) * (float)Projectile.direction * Projectile.localAI[0];

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
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 0.9f, Pitch = -0.4f + i * 0.25f, MaxInstances = 9 }, Projectile.Center);
                for (int i = 0; i < 6; i++)
                {
                    float range = Main.rand.NextFloat(-0.3f, 0.3f);
                    float power = 1 - Math.Abs(range);
                    Vector2 vel = Projectile.Center.DirectionTo(target.Center).RotatedBy(range) * Main.rand.NextFloat(35, 40) * power;
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center, vel, Color.Peru, Color.PeachPuff, Main.rand.NextFloat(1.7f, 2.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                    for (int b = 0; b < 5; b++)
                    {
                        Color clr = Color.Lerp((Main.rand.NextBool() ? Color.Peru : Color.PeachPuff), Color.Black, Main.rand.NextFloat(0.25f, 0.45f));
                        Particle sand = new CustomSpark(Projectile.Center, vel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.4f, 0.9f) * power, "CalamityMod/Particles/SmallSmoke", false, Main.rand.Next(25, 35 + 1), Projectile.scale * Main.rand.NextFloat(0.08f, 0.14f) * 7, clr, new Vector2(1, Main.rand.NextFloat(0.2f, 2f)), false, extraRotation: Main.rand.NextFloat(-4, 4), spin: Main.rand.NextFloat(-0.8f, 0.8f), affectedByLight: true);
                        GeneralParticleHandler.SpawnParticle(sand);
                    }
                }
            }

            modifiers.SourceDamage *= damageMult * 0.03f;
        }
        public override void OnKill(int timeLeft)
        {
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 2);
            if (Projectile.ai[1] == 1)
            {
                Player Owner = Main.player[Projectile.owner];
                Owner.SetScreenshake(8f);

                Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Purple, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.2f * 2f, 0.6f * 2.5f, 17);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.1f * 2f, 0.5f * 2.5f, 17);
                GeneralParticleHandler.SpawnParticle(orb2);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PrimordialAncientExplosion>(), Projectile.damage, Projectile.knockBack * 1.5f, Projectile.owner);
                SoundStyle explo = new("CalamityMod/Sounds/Item/MineralMortarExplode");
                SoundEngine.PlaySound(explo with { Volume = 0.9f }, Projectile.Center);
                SoundStyle explo2 = new("CalamityMod/Sounds/Item/EarthMeteor");
                SoundEngine.PlaySound(explo2 with { Volume = 0.9f, Pitch = -0.3f }, Projectile.Center);

                Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Purple, Color.White, 0.5f) * 0.55f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 2.56f * 2.3f, 18);
                GeneralParticleHandler.SpawnParticle(bolt2);

                Particle bolt3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Peru * 0.55f, "CalamityMod/Particles/SmokeExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.4f, 23);
                GeneralParticleHandler.SpawnParticle(bolt3);

                Particle bolt4 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.PeachPuff * 0.55f, "CalamityMod/Particles/SmokeExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.4f * 1.35f, 18);
                GeneralParticleHandler.SpawnParticle(bolt4);

                for (int i = 0; i < 140; i++)
                {
                    if (Main.rand.NextBool(5))
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(5) ? 86 : 287, new Vector2(21, 21).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                        if (dust.type == 86)
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
                        Particle sand = new CustomSpark(Projectile.Center, (Vector2.One * Main.rand.NextFloat(9, 33)).RotatedByRandom(MathHelper.TwoPi), "CalamityMod/Particles/SmallSmoke", true, Main.rand.Next(25, 55 + 1), Projectile.scale * Main.rand.NextFloat(0.08f, 0.14f) * 23, clr, new Vector2(1, Main.rand.NextFloat(0.2f, 2f)), false, extraRotation: Main.rand.NextFloat(-4, 4), spin: Main.rand.NextFloat(-0.8f, 0.8f), affectedByLight: true);
                        GeneralParticleHandler.SpawnParticle(sand);
                    }
                }
                for (int i = 0; i < 30; i++)
                {
                    Vector2 randVel = new Vector2(30, 30).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.6f);
                    Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, Color.Peru, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.9f, 2.3f), 0.4f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center, randVel * 0.8f, Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.4f, 1.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                }

                float numberOflines = 35;
                float rotFactorlines = 360f / numberOflines;
                for (int e = 0; e < numberOflines; e++)
                {
                    float rot = MathHelper.ToRadians(e * rotFactorlines);
                    Vector2 offset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot + Main.rand.NextFloat(0.1f, 5.1f));
                    Vector2 velOffset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot + Main.rand.NextFloat(0.1f, 5.1f));
                    SparkParticle spark = new SparkParticle(Projectile.Center + offset, velOffset * Main.rand.NextFloat(15.5f, 25.5f), true, 80, Main.rand.NextFloat(0.5f, 1.3f), Color.Lerp(Color.White, Color.Purple, Main.rand.NextFloat(0.3f, 0.7f)));
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.width * 0.5f * Projectile.scale, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor * 0.7f, 1);
            
            Texture2D rTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            float randSize = Main.rand.NextFloat(0.8f, 1.2f);
            Color drawColor2 = Color.Purple;
            Main.EntitySpriteDraw(rTexture, Projectile.Center - Main.screenPosition, null, drawColor2 with { A = 0 }, Projectile.rotation, rTexture.Size() * 0.5f, 0.45f * Utils.GetLerpValue(0, 25, time, true) * randSize, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(rTexture, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, rTexture.Size() * 0.5f, 0.3f * Utils.GetLerpValue(0, 25, time, true) * randSize, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 180);
    }
}
