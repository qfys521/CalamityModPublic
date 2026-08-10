using System;
using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class OntologicalDespoilerShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/OntologicalDespoilerShot";
        public ref float time => ref Projectile.ai[0];
        public Color baseColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
        public int sineDir = 1;

        public Player Owner => Main.player[Projectile.owner];
        public Color color1;
        public Color color2;
        public Color color3;
        public Color color4;
        public bool Positive => Projectile.ai[2] < 5;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 25;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 750;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 4;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            color1 = Owner.shirtColor;
            color2 = Color.Lerp(Owner.shirtColor, Color.Black, 0.2f);
            color3 = Color.Lerp(Owner.shirtColor, Color.White, 0.1f);
            color4 = Color.Lerp(Owner.shirtColor, Color.White, 0.2f);

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 10)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5)
            {
                Projectile.frame = 0;
            }

            if (time == 0)
            {
                Projectile.scale = 1f;
                sineDir = Main.rand.NextBool() ? 1 : -1;
                Projectile.frame = Main.rand.Next(0, 6 + 1);
                if (!Positive)
                    Projectile.penetrate = 1;
            }
            if (Owner.shirtColor != Color.White)
            {
                float rate = (Main.GlobalTimeWrappedHourly * 15);
                List<Color> eColors = new List<Color>()
                {
                    color1,
                    color2,
                    color3,
                    color4
                };
                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                baseColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);
            }
            else
                baseColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

            if (!Positive)
            {
                baseColor = Color.White;
                Projectile.extraUpdates = 3;
            }
            else
                Projectile.extraUpdates = 5;

            if (time > 20)
            {
                if (!Positive)
                {

                    float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                    Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 13f;
                    float scale = Main.rand.NextFloat(0.45f, 0.55f);
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + offset * sineDir, (time % 2 == 0 ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                    dust2.noGravity = true;
                    dust2.scale = scale;
                    dust2.color = baseColor;
                }
                else
                {
                    if (Main.rand.NextBool(21))
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), ModContent.DustType<LightDust>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 1.8f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.8f, 1.15f);
                        dust.color = baseColor;
                    }
                    if ((time < 15 || time % 25 <= 15) && Projectile.ai[2] != 1)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy(0.5f / 15f * (Projectile.ai[2] == 2 ? 1 : -1));
                        if (time % 25 == 0 || time == 15)
                            Projectile.ai[2] = (Projectile.ai[2] == 2 ? 0 : 2);
                    }
                }
            }

            if (!Positive)
            {
                NPC targetedNPC = Projectile.Center.ClosestNPCAt(700);
                if (targetedNPC != null && time > 30 && Projectile.numHits < 1 && Vector2.Distance(targetedNPC.Center, Projectile.Center) < 700)
                {
                    float moveSpeed = (0.42f + Utils.GetLerpValue(650, 450, Projectile.timeLeft, true));
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targetedNPC, true, moveSpeed, 8, 0.95f, accelerate: true);
                }
            }
            else
            {
                if (Projectile.timeLeft < 100)
                {
                    Projectile.velocity *= 0.96f;
                    Projectile.scale *= 0.98f;
                }
                Projectile.timeLeft--;
                if (Projectile.timeLeft <= 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Particle orb2 = new LineParticle(Projectile.Center, (Projectile.velocity * 5).RotatedByRandom(0.05f) * Main.rand.NextFloat(0.1f, 1f), false, Main.rand.Next(20, 28 + 1), Main.rand.NextFloat(0.6f, 1.3f), baseColor);
                        GeneralParticleHandler.SpawnParticle(orb2);
                    }
                }
            }

            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Positive)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, !Positive ? (Main.rand.NextBool() ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()) : ModContent.DustType<LightDust>(), (Projectile.velocity * 3).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.15f, 1.45f);
                    dust.color = baseColor;
                }
                Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.15f, 0.65f, 8);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloomRingLayered", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.2f, 0.75f, 8, false);
                GeneralParticleHandler.SpawnParticle(orb2);
                Particle orb3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.4f, 0.05f, 9, false);
                GeneralParticleHandler.SpawnParticle(orb3);
            }
            else
            {
                for (int i = 0; i < MathHelper.Clamp((int)(3 - Projectile.numHits * 0.3f), 1, 3); i++)
                {
                    Particle orb2 = new LineParticle(Projectile.Center, (Projectile.velocity * 4).RotatedByRandom(0.15f) * Main.rand.NextFloat(0.1f, 1f), false, Main.rand.Next(20, 28 + 1), Main.rand.NextFloat(0.6f, 1.3f), baseColor);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }
            }
            SoundStyle fire = Positive ? new SoundStyle("CalamityMod/Sounds/Item/OntologicalDespoilerSmallImpact") : OntologicalDespoiler.SmallImpact; // Yes these are two different sounds...
            SoundEngine.PlaySound(fire with { Volume = (!Positive ? 1 : 0.3f), Pitch = Main.rand.NextFloat(0.05f, 0.15f) * (!Positive ? 3 : 5), MaxInstances = !Positive ? 1 : 6 }, Projectile.Center);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.3f;
            int hitsToMinMult = 3;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 2)
                return false;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLineBloom2");
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLine2");
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], baseColor with { A = 0 } * 0.8f, 1, tex.Value);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Positive ? Color.Lerp(Color.White, baseColor, MathHelper.Clamp(Utils.GetLerpValue(50, 175, time, true), 0, 0.5f)) with { A = 0 } : Color.Black, 1, tex2.Value, true, true);

            Asset<Texture2D> tex3 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerShot");
            Asset<Texture2D> tex4 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/OntologicalDespoilerShot2");

            Texture2D rTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

            Rectangle frame = tex3.Frame(1, 6, 0, Projectile.frame);
            Vector2 rotationPoint = frame.Size() * 0.5f;

            if (Positive)
            {
                for (int i = 0; i < 5; i++)
                {
                    Main.EntitySpriteDraw(Positive ? tex4.Value : tex3.Value, drawPosition, frame, Positive ? baseColor with { A = 0 } : baseColor, drawRotation, rotationPoint, new Vector2(1 + i * 0.45f, 1 - i * 0.25f) * Projectile.scale * 0.9f, SpriteEffects.None);
                    Main.EntitySpriteDraw(tex4.Value, drawPosition, frame, Color.Lerp(baseColor, Color.White, 0.7f) with { A = 0 }, drawRotation, rotationPoint, new Vector2(1 + i * 0.45f, 1 - i * 0.25f) * Projectile.scale * 0.8f, SpriteEffects.None);
                }
            }
            else
            {
                Main.EntitySpriteDraw(Positive ? tex4.Value : tex3.Value, drawPosition, frame, Positive ? baseColor with { A = 0 } : baseColor, drawRotation, rotationPoint, Projectile.scale, SpriteEffects.None);
            }

            return false;
        }
    }
}
