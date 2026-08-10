using System;
using CalamityMod.Enums;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class VisNeedle : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
        }

        public override void AI()
        {
            // Face the right direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Leak a particle
            if (Main.rand.NextBool(6))
            {
                Vector2 velocity = Vector2.Normalize(Projectile.velocity).RotatedBy(Main.rand.NextFloat(-0.07f, 0.07f)) * 0.8f;
                float scale = Projectile.scale * 0.33f;

                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, velocity, false, 20, scale, Color.Magenta));
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, Color.White, Projectile.rotation, sourceRect.Size() * 0.5f, 1f, 0);

            return false;
        }

        // Handle the projectile's trail from here
        private float WidthFunction(float completionRatio, Vector2 vertexPos) => MathHelper.Lerp(0f, MathHelper.Lerp(10f, 0f, completionRatio), MathF.Pow(completionRatio, 1f / 2.5f));
        private Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float offsetTime = Main.GlobalTimeWrappedHourly;
            float fadeOpacity = Utils.GetLerpValue(0.5f, 0f, completionRatio, true) * (Projectile.Opacity * 0.75f);
            Color endColor = Color.Lerp(Color.Magenta, Color.Violet, (float)Math.Sin(completionRatio * MathHelper.Pi * 2f - offsetTime * 4f) * 0.5f + 0.5f);
            return Color.Lerp(endColor, Color.White, completionRatio) * fadeOpacity;
        }
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, (_, _) => Projectile.Size* 0.5f, pixelate: true, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);
        }
    }
}
