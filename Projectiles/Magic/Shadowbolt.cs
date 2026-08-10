using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ReLogic.Content;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Magic
{
    public class Shadowbolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public float platFade = 0;
        public float platRot = 0;
        public bool spawnPlat = false;
        public bool hasSetPlatSpawn = false;
        public bool hasReboundOffPlat = false;
        public bool reflecting = false;
        public int reflectionTimer = 50;
        public Vector2 platPosCenter;
        public Vector2 platPosWall;
        public NPC chosenTarget;
        public Vector2 targetCenter;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 10;
            Projectile.timeLeft = 2400;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);


            if (!spawnPlat && !hasReboundOffPlat && targetCenter != Vector2.Zero)
                platFade = MathHelper.Lerp(platFade, 1, 0.008f);
            if (hasReboundOffPlat)
            {
                platFade -= 0.0007f;
            }

            BeamMainVisuals(Owner, targetDist);

            if (reflecting)
            {
                if (reflectionTimer > 15)
                    platPosCenter += (targetCenter - platPosCenter).SafeNormalize(Vector2.UnitX) * -2.7f * Utils.GetLerpValue(15, 40, reflectionTimer, true);
                else
                    platPosCenter += (targetCenter - platPosCenter).SafeNormalize(Vector2.UnitX) * 9f * Utils.GetLerpValue(10, 0, reflectionTimer, true);

                Projectile.extraUpdates = 0;
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = platPosWall;
                reflectionTimer--;
                if (reflectionTimer <= 0)
                {
                    SoundStyle bounce = new("CalamityMod/Sounds/Item/ShadowboltReflect");
                    SoundEngine.PlaySound(bounce with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f), MaxInstances = -1 }, platPosWall);
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;
                    Projectile.velocity = (targetCenter - platPosCenter).SafeNormalize(Vector2.UnitX) * 12;
                    reflecting = false;
                    hasReboundOffPlat = true;
                    time = 10;
                    Projectile.extraUpdates = 100;
                }
            }

            if (spawnPlat && hasSetPlatSpawn)
            {
                chosenTarget = Projectile.Center.ClosestNPCAt(2000);

                // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                if (chosenTarget == null)
                    targetCenter = Owner.Calamity().mouseWorld;
                else
                    targetCenter = chosenTarget.Center;

                platPosCenter = Projectile.Center + Projectile.velocity * Main.rand.Next(70, 120 + 1);
                platRot = (Projectile.Center - platPosCenter).SafeNormalize(Vector2.UnitX).ToRotation();
                platPosWall = platPosCenter + new Vector2(0, 8).RotatedBy(platRot);
                spawnPlat = false;
            }
            if (!spawnPlat && reflecting && targetCenter != Vector2.Zero)
            {
                // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                if (chosenTarget == null)
                    targetCenter = Owner.Calamity().mouseWorld;
                else
                    targetCenter = chosenTarget.Center;

                platRot = platRot.AngleLerp((targetCenter - platPosCenter).SafeNormalize(Vector2.UnitX).ToRotation(), 0.1f);
                platPosWall = platPosCenter + new Vector2(0, -30).RotatedBy(platRot + MathHelper.ToRadians(90f));
            }
            if (!spawnPlat && !hasReboundOffPlat && targetCenter != Vector2.Zero)
            {
                float beamDist = Vector2.Distance(platPosWall, Projectile.Center);
                if (beamDist <= 25 && !reflecting)
                {
                    Particle blastRing = new CustomPulse(platPosWall, Vector2.Zero, Color.Purple, "CalamityMod/Particles/SmallBloomRing", Vector2.One, Main.rand.NextFloat(-10, 10), 0.1f, 1.55f, 20, true);
                    GeneralParticleHandler.SpawnParticle(blastRing);

                    SoundStyle wall = new("CalamityMod/Sounds/Item/ShadowboltWallHit");
                    SoundEngine.PlaySound(wall with { Volume = 0.5f, Pitch = 0f, MaxInstances = -1 }, platPosWall);
                    reflecting = true;
                    Projectile.extraUpdates = 0;
                }
            }
            
            if (!hasReboundOffPlat && Projectile.numHits == 0 && !hasSetPlatSpawn && time > 70)
            {
                hasSetPlatSpawn = true;
                spawnPlat = true;
            }
            time++;
        }
        private void BeamMainVisuals(Player Owner, float targetDist)
        {
            if (reflecting)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust chargefull = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2);
                    chargefull.velocity = new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 0.8f);
                    chargefull.scale = Main.rand.NextFloat(0.35f, 0.8f);
                    chargefull.noGravity = true;
                    chargefull.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Indigo : Color.Purple, 0.7f);
                }
            }
            else
            {
                if (time == 14)
                {
                    SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f), MaxInstances = -1 }, Projectile.Center);
                    Projectile.velocity = Projectile.velocity.RotatedByRandom(0.15f);
                    for (int i = 0; i < 2; i++)
                    {
                        Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Indigo, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.3f, 0.75f, 12, true);
                        GeneralParticleHandler.SpawnParticle(blastRing);
                        Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.2f, 0.55f, 12, true);
                        GeneralParticleHandler.SpawnParticle(blastRing2);
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        Dust chargefull = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB);
                        chargefull.velocity = Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.6f, 2f);
                        chargefull.scale = Main.rand.NextFloat(0.65f, 0.9f);
                        chargefull.noGravity = true;
                        chargefull.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Indigo : Color.Purple, 0.7f);
                    }
                }
                if (targetDist < 1400)
                {
                    if (time > 30 && Main.rand.NextBool(15))
                    {
                        Particle spark = new LineParticle(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(-1, 1), false, 50, 1.9f, Color.Lerp(Color.Indigo, Color.Purple, Main.rand.NextFloat(0, 1)));
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    if (time > 22 && time % 3 == 0)
                    {
                        Particle spark = new GlowSparkParticle(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10, -Projectile.velocity * 0.1f, false, 18, 0.05f * (hasReboundOffPlat ? 1.1f : 0.5f), Color.Lerp(Color.Indigo, Color.Orchid, 0.25f), new Vector2(0.7f, 2f), true, false, 0.3f);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    if (time > 30 && Main.rand.NextBool(12))
                    {
                        Dust chargefull = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2);
                        chargefull.velocity = Projectile.velocity * Main.rand.NextFloat(-2, 2);
                        chargefull.scale = Main.rand.NextFloat(0.95f, 1.4f);
                        chargefull.noGravity = true;
                        chargefull.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Indigo : Color.Purple, 0.7f);
                    }
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.25f;
            int hitsToMinMult = 7;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= (hasReboundOffPlat ? 2.5f : 0.8f) * damageMult;

            if (!hasReboundOffPlat && Projectile.numHits == 0 && !hasSetPlatSpawn)
            {
                spawnPlat = true;
                hasSetPlatSpawn = true;
            }
        }
        public override bool? CanDamage() => reflecting ? false : null;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (targetCenter != Vector2.Zero && platFade > 0)
            {
                Texture2D pTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/ShadowPlatform").Value;

                for (int i = 0; i < 5; i++)
                {
                    Color auraColor = Color.Lerp(Color.Indigo, Color.Purple, Utils.GetLerpValue(0, 5, i)) with { A = 0 } * 0.55f * platFade;
                    Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 30f).ToRotationVector2();
                    rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 6f) * 0.5f + 0.5f);
                    Main.EntitySpriteDraw(pTexture, platPosCenter - Main.screenPosition + rotationalDrawOffset, null, auraColor, platRot + MathHelper.ToRadians(90f), pTexture.Size() * 0.5f, new Vector2(1, 0.7f), SpriteEffects.None);
                }

                for (int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(pTexture, platPosCenter - Main.screenPosition, null, Color.Purple with { A = 0 } * platFade, platRot + MathHelper.ToRadians(90f), pTexture.Size() * 0.5f, new Vector2(1, 0.7f), SpriteEffects.None);
                Main.EntitySpriteDraw(pTexture, platPosCenter - Main.screenPosition, null, Color.White with { A = 0 } * platFade * 0.6f, platRot + MathHelper.ToRadians(90f), pTexture.Size() * 0.5f, new Vector2(1, 0.7f) * 0.93f, SpriteEffects.None);
            }

            if (!reflecting && time >= 22 && !hasReboundOffPlat)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark");
                for (int i = 0; i < 5; i++)
                {
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15, null, Color.White with { A = 0 } * 0.7f, Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f), tex.Size() * 0.5f, new Vector2(0.4f, 1f) * (i * 0.3f) * 0.05f * (hasReboundOffPlat ? 1.1f : 0.5f), SpriteEffects.None);
                }
            }

            if (reflecting)
            {
                Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

                float randSize = Main.rand.NextFloat(0.8f, 1.2f);
                Main.EntitySpriteDraw(rechargeTexture, platPosWall - Main.screenPosition, null, Color.Purple with { A = 0 }, Projectile.rotation, rechargeTexture.Size() * 0.5f, 0.65f * Utils.GetLerpValue(-25, 20, reflectionTimer, true) * randSize, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(rechargeTexture, platPosWall - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, rechargeTexture.Size() * 0.5f, 0.45f * Utils.GetLerpValue(-25, 20, reflectionTimer, true) * randSize, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
