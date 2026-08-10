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
    public class IgnisSigilEmbers : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 9;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = false; // This is VFX
            Projectile.ignoreWater = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.damage = 0; // This is VFX
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.22f / 255f, (255 - Projectile.alpha) * 0.05f / 255f, (255 - Projectile.alpha) * 0.05f / 255f);
            Projectile.scale -= 0.01f;

            if (Projectile.scale <= 0f)
            {
                Projectile.Kill();
            }

            if (Projectile.ai[0] <= 3f)
            {
                Projectile.ai[0] += 1f;
                return;
            }

            // Gravity!
            Projectile.velocity.Y = Projectile.velocity.Y + 0.35f;
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
        private float WidthFunction(float completionRatio, Vector2 vertexPos) => MathHelper.Lerp(0f, MathHelper.Lerp(Projectile.scale * 48f, 0f, completionRatio), MathF.Pow(completionRatio, 1f / 2.5f));
        private Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float offsetTime = Main.GlobalTimeWrappedHourly;
            float fadeOpacity = Utils.GetLerpValue(0.5f, 0f, completionRatio, true) * Projectile.Opacity;
            Color endColor = Color.DarkOrange;
            return Color.Lerp(endColor, Color.DarkSalmon, completionRatio) * fadeOpacity;
        }
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, (_, _) => Projectile.Size * 0.5f, pixelate: true, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 32);
            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, Projectile.velocity, false, 2, Projectile.scale * 1.45f, Color.Salmon), pixelate: true);
        }
    }
}
