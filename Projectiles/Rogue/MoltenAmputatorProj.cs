using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class MoltenAmputatorProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/MoltenAmputator";

        public int time = 0;
        public Vector2 squash = new Vector2(1, 1);
        public float fakeRot = 0;
        public int returnTime = 180;
        public bool pulled = false;
        public bool returning = false;
        public int direction = 0;
        public int pulledTimer = 0;
        public bool AMPUTATE = false;

        public float effectScale = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 74;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 35 * Projectile.MaxUpdates;
            Projectile.timeLeft = 900;
            Projectile.extraUpdates = 4;
            Projectile.DamageType = RogueDamageClass.Instance;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            if (Projectile.ai[2] == 5 && Projectile.ai[2] < 10 && !Projectile.Calamity().stealthStrike)
            {
                if (time >= returnTime * 0.07f && time < returnTime * 1.4f)
                {
                    if (!pulled)
                    {
                        Projectile.localNPCHitCooldown = -1;
                        for (int i = 0; i < Main.maxNPCs; i++)
                            Projectile.localNPCImmunity[i] = 0;
                        Projectile.ai[2] = 10;
                        Projectile.numHits = 0;
                        Projectile.velocity = Utils.DirectionTo(Projectile.Center, Owner.Center) * 3;
                        Projectile.extraUpdates += 4;
                        pulled = true;
                    }
                }
                else
                    Projectile.ai[2] = 0;
            }
            if (time >= returnTime)
                returning = true;

            fakeRot += 0.13f * direction;
            Projectile.rotation = (Projectile.velocity.ToRotation()) + MathHelper.PiOver2;

            if (Projectile.velocity.Length() < 5)
                effectScale = MathHelper.Lerp(effectScale, 1, 0.05f);
            else if (returning)
                effectScale = MathHelper.Lerp(effectScale, 0, 0.05f);

            float x = MathHelper.Clamp(Utils.GetLerpValue(16, 2, Projectile.velocity.Length(), true), 0.3f, 1);
            float y = 1;
            squash = new Vector2(x, y);

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 7)
            {
                Projectile.frame = 0;
            }

            if (pulled)
            {
                if (time < returnTime * 1.5f)
                    time = (int)(returnTime * 1.5f);
                if (pulledTimer > 20)
                {
                    if (time % 2 == 0)
                    {
                        Particle spark2 = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 19, 0.05f, Color.Orchid * 0.3f, new Vector2(0.7f, 1.2f), true, false, 0.35f);
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                    else
                    {
                        Particle spark2 = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 11, 0.07f, Color.Goldenrod * 0.45f, new Vector2(0.7f, 1.2f), true, false, 0.55f);
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                }
                
                pulledTimer++;
            }

            // Frame 1, pick a direction for the scythe. This direction isn't changed from that point on
            if (direction == 0f)
            {
                direction = (Utils.DirectionTo(Projectile.Center, Owner.Calamity().mouseWorld).X > 0 ? 1 : -1);
            }

            // Boomerang glows orange
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 1.5f);

            // Boomerang noises
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 7 * Projectile.MaxUpdates;
                SoundStyle sound = new("CalamityMod/Sounds/Item/SwooshMid");
                SoundEngine.PlaySound(sound with { MaxInstances = -1, Volume = Projectile.Calamity().stealthStrike ? 0.15f : 0.3f }, Projectile.Center);
            }

            // Main boomerang logic. projectile.ai[0] is a frame counter.
            Projectile.ai[0] += 1f;

            // On the first returning frame, send a net update.
            if (time == returnTime)
                Projectile.netUpdate = true;

            // Once returning, use boomerang return AI.
            if (returning)
            {
                Vector2 moveToTrackingPos = (Owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                if (Projectile.velocity.Length() < 3 + (8 * Utils.GetLerpValue(returnTime * 1.2f, returnTime * 1.5f, time, true)))
                    Projectile.velocity += moveToTrackingPos * (0.02f + (4 * Utils.GetLerpValue(returnTime * 1.2f, returnTime * 2.5f, time, true)));
                else
                    Projectile.velocity *= 0.95f;

                // Destroy the boomerang when it returns to the player.
                if (Main.myPlayer == Projectile.owner)
                    if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                        Projectile.Kill();
            }
            else
            {
                if (Projectile.Calamity().stealthStrike && time > returnTime * 0.2f)
                {
                    Vector2 moveToTrackingPos = (Owner.ClampedMouseWorld() - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    if (Projectile.velocity.Length() < 16 * Utils.GetLerpValue(returnTime * 0.7f, 0, time, true))
                        Projectile.velocity += moveToTrackingPos * (0.5f * Utils.GetLerpValue(returnTime * 0.7f, 0, time, true));
                    else
                        Projectile.velocity *= 0.95f;
                }
                Projectile.velocity *= (time > returnTime * 0.4f ? 0.97f : 0.982f);
            }

            if (Main.rand.NextBool(5 * (Projectile.Calamity().stealthStrike ? 2 : 1)))
            {
                int numParts = 2;
                for (int i = 0; i < numParts; i++)
                {
                    float fade = (Utils.GetLerpValue(5, 2, Projectile.velocity.Length(), true) * 3 + 1) * squash.X;

                    float rot = fakeRot + (MathHelper.TwoPi * i / numParts);
                    Vector2 vel = (Utils.MoveTowards(-Projectile.velocity, new Vector2(0, -130).RotatedBy(rot).RotatedBy(-1.3f * direction), (Utils.GetLerpValue(5, 2, Projectile.velocity.Length(), true))));
                    
                    if (Main.rand.NextBool(6))
                    {
                        Particle spark2 = new CustomSpark(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), vel.RotatedByRandom(0.4f) * fade, "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(1.15f, 1.3f), Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f)), new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.3f, 0.4f));
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                    else
                    {
                        Particle spark = new CustomSpark(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), vel.RotatedByRandom(0.4f) * fade, "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(0.75f, 0.82f), Main.rand.NextBool(4) ? Color.Khaki : Color.Orange, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.3f, 0.4f));
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    if (Main.rand.NextBool(6))
                    {
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), Main.rand.NextBool(4) ? 278 : ModContent.DustType<LightDust>());
                        dust2.noGravity = (dust2.type == 278 ? false : true);
                        dust2.scale = dust2.type == 278 ? 0.95f : 1.2f;
                        dust2.color = Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f));
                        dust2.velocity = (vel * 2).RotatedByRandom(0.4f) * fade;
                    }
                    else
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), Main.rand.NextBool(4) ? 278 : ModContent.DustType<LightDust>());
                        dust.noGravity = (dust.type == 278 ? false : true);
                        dust.scale = dust.type == 278 ? 0.75f : 0.9f;
                        dust.color = Main.rand.NextBool(4) ? Color.Khaki : Color.Goldenrod;
                        dust.velocity = (vel * 2).RotatedByRandom(0.4f) * fade;
                    }
                }
            }
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 120);

            Player Owner = Main.player[Projectile.owner];
            if (Projectile.Calamity().stealthStrike)
            {
                if (Projectile.numHits == 0)
                {
                    SoundStyle fire = new("CalamityMod/Sounds/Item/FinalDawnSlash");
                    SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.4f, 0.85f), MaxInstances = -1 }, Projectile.Center);
                }

                if (Projectile.numHits < 8)
                {
                    int numParts = 2;
                    for (int i = 0; i < numParts; i++)
                    {
                        float fade = 4;

                        float rot = fakeRot + (MathHelper.TwoPi * i / numParts) * 0.4f + Projectile.numHits * 5;
                        Vector2 vel = (new Vector2(0, -130).RotatedBy(rot).RotatedBy(-1.3f * direction)) * squash.X;

                        Particle spark2 = new CustomSpark(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), (vel * 0.02f).RotatedByRandom(1.4f) * fade, "CalamityMod/Particles/GlowSpark", false, 18, Main.rand.NextFloat(0.015f, 0.025f), Color.Goldenrod, new Vector2(2f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.6f, 0.7f));
                        GeneralParticleHandler.SpawnParticle(spark2);

                        BloodParticle blood = new BloodParticle(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), (vel * 0.04f).RotatedByRandom(1.4f) * fade, Main.rand.Next(13, 24 + 1), Main.rand.NextFloat(0.85f, 1.2f), Main.rand.NextBool() ? Color.Goldenrod : Color.Orange);
                        GeneralParticleHandler.SpawnParticle(blood);

                        if (Main.rand.NextBool(6))
                        {
                            Dust dust2 = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), Main.rand.NextBool(4) ? 278 : ModContent.DustType<LightDust>());
                            dust2.noGravity = true;
                            dust2.scale = dust2.type == 278 ? 0.95f : 1.2f;
                            dust2.color = Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f));
                            dust2.velocity = (vel * 0.04f).RotatedByRandom(1.4f) * fade;
                        }
                        else
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), Main.rand.NextBool(4) ? 278 : ModContent.DustType<LightDust>());
                            dust.noGravity = true;
                            dust.scale = dust.type == 278 ? 0.75f : 0.9f;
                            dust.color = Main.rand.NextBool(4) ? Color.Khaki : Color.Goldenrod;
                            dust.velocity = (vel * 0.04f).RotatedByRandom(1.4f) * fade;
                        }
                    }
                }
            }
            if (pulled && (pulledTimer < 10 || Owner.Calamity().focusFlurryAttackCount > 0) && !AMPUTATE && !Projectile.Calamity().stealthStrike)
            {
                // Amputate them
                Projectile strike = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), Projectile.damage * 7, 0f, Owner.whoAmI, target.whoAmI, 1f);
                Particle blastRingBase = new CustomPulse(target.Center, Vector2.Zero, Color.Orange, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 1.1f, 1f, 18, true);
                GeneralParticleHandler.SpawnParticle(blastRingBase);
                for (int i = 0; i < 3; i++)
                {
                    Particle blastRing = new CustomPulse(target.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.7f * (i + 1), 1f, 18, true);
                    GeneralParticleHandler.SpawnParticle(blastRing);
                    Particle blastRing2 = new CustomPulse(target.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.35f * (i + 1), 0.5f, 18, true);
                    GeneralParticleHandler.SpawnParticle(blastRing2);
                }

                for (int i = 0; i < 6; i++)
                {
                    Particle spark = new GlowSparkParticle(target.Center, (Projectile.velocity).SafeNormalize(Vector2.UnitY) * -5 * (i % 2 == 0 ? -1 : 1), false, 15, 0.08f - i * 0.01f, Color.Goldenrod, new Vector2(5, 0.8f), true, false, 1.2f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                for (int i = 0; i < 20; i++)
                {
                    int dir = (i < 10 ? 1 : -1);
                    LineParticle spark2 = new LineParticle(Projectile.Center + Main.rand.NextVector2Circular(13, 13), (Projectile.velocity).SafeNormalize(Vector2.UnitY) * -25 * Main.rand.NextFloat(0.4f, 2f) * dir, false, 12, 1.1f, Main.rand.NextBool(5) ? Color.Khaki : Color.Goldenrod);
                    GeneralParticleHandler.SpawnParticle(spark2);

                    BloodParticle blood = new BloodParticle(Projectile.Center, (((Projectile.velocity).SafeNormalize(Vector2.UnitY) * -25).RotatedByRandom(0.6f) + new Vector2(0, -1.5f)) * Main.rand.NextFloat(0.4f, 2f) * dir, Main.rand.Next(13, 24 + 1), Main.rand.NextFloat(0.85f, 1.2f), Main.rand.NextBool() ? Color.Goldenrod : Color.Orange);
                    GeneralParticleHandler.SpawnParticle(blood);
                }

                SoundStyle slice = new("CalamityMod/Sounds/Item/HellkiteFullCharge");
                SoundEngine.PlaySound(slice with { Volume = 1f, Pitch = Main.rand.NextFloat(0.5f, 0.6f) }, target.Center);
                SoundStyle cut = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");
                SoundEngine.PlaySound(cut with { Volume = 1f, Pitch = Main.rand.NextFloat(0.5f, 0.6f) }, target.Center);

                AMPUTATE = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 80, targetHitbox);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation;

            Asset<Texture2D> p = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire2");
            Asset<Texture2D> p2 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire3");
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(p2.Value, drawPosition, null, Color.Orchid with { A = 0 } * 0.25f * effectScale, fakeRot * (Main.rand.NextFloat(1.5f, 1.55f) * (i * 0.5f + 0.2f)), p2.Size() * 0.5f, 1.1f * Main.rand.NextFloat(0.8f, 1.15f) * effectScale, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                Main.EntitySpriteDraw(p.Value, drawPosition, null, Color.Orange with { A = 0 } * 0.35f * effectScale, fakeRot * (Main.rand.NextFloat(1.1f, 1.15f) * (i * 0.5f + 0.2f)), p.Size() * 0.5f, 0.9f * effectScale, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }

            Asset<Texture2D> tex3 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/MoltenAmputatorAnimated");
            Rectangle frame = tex3.Frame(1, 8, 0, Projectile.frame);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(tex3.Value, drawPosition, frame, lightColor, drawRotation, rotationPoint, squash * Projectile.scale, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
