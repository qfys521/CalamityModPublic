using System;
using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class SquareDust : ModDust
    {
        public static Asset<Texture2D> GlowSquare { get; private set; }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void Load()
        {
            if (!Main.dedServ)
                GlowSquare = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSquareParticleThick");
        }

        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
            dust.rotation = Main.rand.NextFloat(-5, 5);
        }

        public override bool Update(Dust dust)
        {
            float fadeSpeed = (dust.fadeIn + 1);
            float rotDir = Math.Sign(dust.rotation);
            dust.rotation += 0.04f * dust.scale * rotDir;
            dust.velocity *= 0.96f * fadeSpeed;
            if (dust.noGravity)
                dust.scale -= 0.045f * fadeSpeed;
            else
            {
                dust.scale -= 0.03f * fadeSpeed;
                dust.velocity.Y += Main.rand.NextFloat(0.1f, 0.35f) * fadeSpeed;
            }

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittance)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            if (dust.scale <= 0)
                dust.active = false;

            dust.position += dust.velocity;

            return false;
        }
        public override bool PreDraw(Dust dust)
        {
            Vector2 squash = Vector2.One;

            // Glow Orb
            Main.spriteBatch.Draw(GlowSquare.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowSquare.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0f);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(GlowSquare.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowSquare.Size() * 0.5f, squash * dust.scale * 0.095f, SpriteEffects.None, 0f);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(GlowSquare.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowSquare.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class SquareDustPixelated : SquareDust
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool PreDraw(Dust dust)
        {
            PixelationManager.AddPixelatedDrawer((_) => DrawPixelated(dust), Enums.GeneralDrawLayer.AfterDusts);
            return false;
        }

        private static void DrawPixelated(Dust dust)
        {
            Vector2 squash = Vector2.One;

            // Glow Orb
            Main.spriteBatch.Draw(GlowSquare.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowSquare.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0f);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(GlowSquare.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowSquare.Size() * 0.5f, squash * dust.scale * 0.095f, SpriteEffects.None, 0f);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(GlowSquare.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowSquare.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0f);
            }
        }
    }
}
