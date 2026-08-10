using System;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class WarpSigilShot : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float FixedOffsetX => ref Projectile.ai[0];
        public ref float FixedOffsetY => ref Projectile.ai[1];
        public ref float DamageStatus => ref Projectile.ai[2];


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = false; // Only deals damage after it has passed over its targeted point
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 28;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            // Called here so that the projectile dissapears instantly after the frame it is able to damage something
            if (DamageStatus == 1 && Projectile.timeLeft <= 6)
                Projectile.Kill();

            Vector2 fixedOffset = new Vector2(FixedOffsetX, FixedOffsetY);
            Vector2 targetPosition = Main.MouseWorld + fixedOffset;

            Vector2 direction = targetPosition - Projectile.Center;
            float distanceToTarget = direction.Length();
            direction.Normalize();

            float homingStrength = 0f;
            if (Projectile.timeLeft <= 22)
                homingStrength = 0.475f;

            float maxSpeed = 60f;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * maxSpeed, homingStrength * 0.4f);

            Projectile.velocity *= 1.05f;

            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Vector2 vectorToTarget = targetPosition - Projectile.Center;

            // Checking for if the projectile is over it's targeted point
            if (Vector2.Dot(Projectile.velocity, vectorToTarget) < 0f)
            {
                DamageStatus = 1;
            }
            Projectile.friendly = DamageStatus == 1 ? true : false;

            float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);
            Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 16f;
            if (Main.rand.NextBool(5))
            {
                Dust dust3 = Dust.NewDustPerfect(Projectile.Center - offset, ModContent.DustType<LightDust>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                dust3.noGravity = true;
                dust3.scale = Main.rand.NextFloat(0.4f, 0.85f);
                dust3.color = Color.Violet;
            }

        }
        public override void OnKill(int timeLeft)
        {
            SoundStyle w = new("CalamityMod/Sounds/Custom/PlagueSounds/PlagueBoom4");
            SoundEngine.PlaySound(w with { Volume = 0.25f, Pitch = 0f, MaxInstances = 4 }, Projectile.Center);

            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkMagenta * 2f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.04f, 14, true, 1f);
            GeneralParticleHandler.SpawnParticle(blastRing);

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.ShadowbeamStaff, Main.rand.NextVector2Circular(2f, 2f));
                dust.noGravity = true;
                dust.scale = 0.5f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, Color.White, Projectile.rotation, sourceRect.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            return false;
        }

        // Handle the projectile's trail from here
        private float WidthFunction(float completionRatio, Vector2 vertexPos) => MathHelper.Lerp(0f, MathHelper.Lerp(32f, 0f, completionRatio), MathF.Pow(completionRatio, 1f / 2.5f));
        private Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float offsetTime = Main.GlobalTimeWrappedHourly;
            float fadeOpacity = Utils.GetLerpValue(0.5f, 0f, completionRatio, true) * Projectile.Opacity;
            Color endColor = Color.Lerp(Color.Magenta, Color.DarkMagenta, (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - offsetTime * 4f) * 0.5f + 0.5f);
            return Color.Lerp(endColor, Color.White, completionRatio) * fadeOpacity;
        }
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, pixelate: true, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 32);

            GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(Projectile.Center, Projectile.velocity, Projectile.scale * 0.4f, Color.Magenta, 2, 1f, 0.5f, 1f), pixelate: true);
        }
    }
}
