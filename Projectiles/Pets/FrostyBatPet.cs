using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class FrostyBatPet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";
        public Player Owner => Main.player[Projectile.owner];
        public Color effectColor = Color.CornflowerBlue;
        public ref float time => ref Projectile.ai[0];
        public float flapMult = 0;
        public int flapTime = 0;
        public int flapRate = 5;
        public Vector2 goalPosition;
        public Vector2 dashDirection;
        public int dashingTimer = 0;
        public int dashLength = 120;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            Main.projPet[Type] = true;
            ProjectileID.Sets.LightPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (VerifyOwnerIsPresent())
                return;

            HandleFrames();

            int maxFlapTime = flapRate * Main.projFrames[Type];
            if (Projectile.frame == 4)
            {
                if (flapTime == 0)
                    flapTime = maxFlapTime;
                float lerp = 1 - (float)Math.Pow(Utils.GetLerpValue(flapRate - 1, 0, Projectile.frameCounter % flapRate), 1.75f);
                flapMult = CalamityUtils.EaseInOutExp(lerp, 2.5f, 1f);
            }
            else
                flapMult = 1 - (float)Math.Pow(Utils.GetLerpValue(maxFlapTime, 1, flapTime), 3);
            if (flapTime > 0)
                flapTime--;

            float sine = (float)Math.Sin(time * 0.05f / MathHelper.Pi);
            float sine2 = (float)Math.Sin((time * 0.05f / MathHelper.Pi) * 2);

            if (dashingTimer == 0)
                goalPosition = Owner.Center + new Vector2(45 * sine, -60 + 10 * sine2);

            float dashIntensity = 1 - Math.Abs(Utils.GetLerpValue(dashLength / 2, 0, dashingTimer));
            if (Owner.dashDelay == -1 && dashingTimer == 0)
            {
                dashingTimer = dashLength;
                Projectile.extraUpdates = 1;
                dashDirection = Owner.Center.DirectionTo(Owner.Calamity().mouseWorld);
            }
            if (dashingTimer > 0)
            {
                float velPower = (float)Math.Pow(Owner.velocity.Length(), 0.75f) * 35;
                goalPosition = Owner.Center + dashDirection * (250 + velPower) * dashIntensity;

                float opacity = Utils.GetLerpValue(6, 12, Projectile.velocity.Length(), true) * 0.25f + 0.05f;
                Particle iceMist = new CustomSpark(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.3f, 1f) + Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f), "CalamityMod/Particles/BloomCircle", false, 22, Main.rand.NextFloat(0.3f, 0.55f), effectColor * opacity, new Vector2(1f, 1.3f), true, true, glowOpacity: opacity, extraRotation: Main.rand.NextFloat(0, MathHelper.TwoPi), spin: Main.rand.NextFloat(-0.1f, 0.1f));
                GeneralParticleHandler.SpawnParticle(iceMist);
            }
            else if (Projectile.extraUpdates > 0)
                Projectile.extraUpdates = 0;

            float followSpeed = 10;
            Projectile.velocity = (goalPosition - Projectile.Center) / (followSpeed);

            Projectile.rotation = Projectile.velocity.X * 0.035f;

            // Emit light.
            float tileCollideMult = !Collision.SolidCollision(Projectile.Center, 2, 2) ? 1 : (dashingTimer > 0 ? Math.Max(Utils.GetLerpValue(dashLength * 0.8f, dashLength, dashingTimer, true), 0.15f) : 1);
            if (tileCollideMult > 0)
                Lighting.AddLight(Projectile.Center, effectColor.ToVector3() * (0.65f + 0.5f * dashIntensity) * tileCollideMult);

            if (Main.rand.NextBool(15 / (dashingTimer != 0 ? 3 : 1)))
            {
                Vector2 vel = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                Particle iceFlake = new CustomSpark(Projectile.Center + vel * 9, vel * Main.rand.NextFloat(0.3f, 1.5f) + Projectile.velocity * Main.rand.NextFloat(0.1f, 0.4f), "CalamityMod/Particles/GlowFlakeParticle", false, Main.rand.Next(25, 35), Main.rand.NextFloat(0.2f, 0.45f) + (dashingTimer > 0 ? 0.2f : 0), effectColor * 0.7f, new Vector2(1f, 1f), true, true, glowOpacity: 0.8f, extraRotation: Main.rand.NextFloat(0, MathHelper.TwoPi), spin: Main.rand.NextFloat(-0.3f, 0.3f));
                GeneralParticleHandler.SpawnParticle(iceFlake);
            }

            time++;
            if (dashingTimer > 0)
                dashingTimer--;
        }

        public bool VerifyOwnerIsPresent()
        {
            // No logic should be run if the player is no longer active in the game.
            if (!Owner.active)
            {
                Projectile.Kill();
                return true;
            }

            if (Owner.dead)
                Owner.Calamity().frostyBat = false;
            if (Owner.Calamity().frostyBat)
                Projectile.timeLeft = 2;

            return false;
        }

        public void HandleFrames()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / flapRate % Main.projFrames[Type];
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glow = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

            float sine = (float)Math.Sin((time * 0.05f / MathHelper.Pi) * 3);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0, 7) * MathHelper.Lerp(1 , -1, flapMult);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;

            Rectangle frame = texture.Frame(1, 8, 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(glow, drawPosition, null, effectColor with { A = 0 } * (0.3f + 0.65f * i), Projectile.rotation, glow.Size() * 0.5f, new Vector2(1.2f, 1f) * Projectile.scale * (0.5f + sine * 0.05f * (1 - i) - 0.3f * i), Owner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, Owner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

            return false;
        }
    }
}
