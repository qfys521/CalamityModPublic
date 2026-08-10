using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class ToastyBatPet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";
        public Player Owner => Main.player[Projectile.owner];
        public Color effectColor = Color.OrangeRed;
        public float flapMult = 0;
        public int flapTime = 0;
        public int flapRate = 3;
        public Vector2 goalPosition;
        public ref float time => ref Projectile.ai[0];
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

            float sine = (float)Math.Sin(time * 0.08f / MathHelper.Pi);
            float sine2 = (float)Math.Sin((time * 0.08f / MathHelper.Pi) * 2);

            goalPosition = Owner.Center + new Vector2(35 * sine, -50 + 15 * sine2) + Owner.velocity * 8.5f;

            float followSpeed = 5.5f;
            Projectile.velocity = (goalPosition - Projectile.Center) / (followSpeed);

            Projectile.rotation = Projectile.velocity.X * 0.035f;

            // Emit light.
            Lighting.AddLight(Projectile.Center, Color.Lerp(effectColor, Color.White, 0.6f).ToVector3() * 0.9f);

            float speedMult = Utils.GetLerpValue(4, 8, Owner.velocity.Length(), true);
            if (Main.rand.NextBool((int)(20 - 15 * speedMult)))
            {
                Vector2 vel = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + vel * 4, ModContent.DustType<SquashDust>(), vel * Main.rand.NextFloat(0.1f, 0.5f) + Projectile.velocity * Main.rand.NextFloat(0.3f, 0.75f), 0, default, Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
                dust.color = effectColor;
                dust.noLightEmittance = false;
                dust.fadeIn = -0.95f;
            }
            if (Main.rand.NextBool((int)(10 - 7 * speedMult)) || speedMult >= 1)
            {
                Vector2 vel = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                Particle fireFlake = new CustomSpark(Projectile.Center + vel * 3, vel * Main.rand.NextFloat(0.1f, 0.3f) + Projectile.velocity * Main.rand.NextFloat(0.1f, 0.4f), "CalamityMod/Particles/BloomCircle", false, Main.rand.Next(35, 45), Main.rand.NextFloat(0.3f, 0.45f), effectColor * 0.2f, new Vector2(1f, 1f), true, true, glowOpacity: 0.2f, extraRotation: Main.rand.NextFloat(0, MathHelper.TwoPi), noShrink: true, spin: Main.rand.NextFloat(-0.2f, 0.2f));
                GeneralParticleHandler.SpawnParticle(fireFlake);
            }

            time++;
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
                Owner.Calamity().toastyBat = false;
            if (Owner.Calamity().toastyBat)
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

            float sine = (float)Math.Sin((time * 0.08f / MathHelper.Pi) * 3);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0, 7) * MathHelper.Lerp(1, -1, flapMult);
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
