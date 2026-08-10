using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod.Dusts
{
    public class SquashDustTileTouch : SquashDust
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
        }

        public override bool Update(Dust dust)
        {
            bool touchedTiles = false;
            int size = 2;

            if (Collision.SolidCollision(dust.position, size, size))
            {
                touchedTiles = true;
                dust.velocity = Vector2.Zero;
            }

            float fadeSpeed = (dust.fadeIn + 1); // For this dust, fadeIn is used to increase the power of gravity if it's active, otherwise it makes it's lifetime change
            dust.rotation = dust.velocity.ToRotation() + MathHelper.PiOver2;
            dust.velocity *= 0.96f;
            if (dust.noGravity)
                dust.scale -= 0.045f * (touchedTiles ? 0.4f : 1) * fadeSpeed;
            else
            {
                dust.scale -= 0.03f * (touchedTiles ? 0.4f : 1);
                dust.velocity.Y += Main.rand.NextFloat(0.1f, 0.35f) * fadeSpeed;
            }

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittance)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            if (dust.scale <= 0)
                dust.active = false;

            if (!touchedTiles)
                dust.position += dust.velocity;

            return false;
        }
    }

    public class SquashDustTileTouchPixelated : SquashDustTileTouch
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool PreDraw(Dust dust)
        {
            PixelationManager.AddPixelatedDrawer((_) => DrawPixelated(dust), Enums.GeneralDrawLayer.AfterDusts);
            return false;
        }

        private static void DrawPixelated(Dust dust)
        {
            Vector2 squash = new Vector2(Utils.Remap(dust.velocity.Length(), 2, 7, 1, 0.5f), Utils.Remap(dust.velocity.Length(), 2, 7, 1, 2.5f));

            // Glow Orb
            Main.spriteBatch.Draw(BloomCircle.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomCircle.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(BloomCircle.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomCircle.Size() * 0.5f, squash * dust.scale * 0.04f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                Main.spriteBatch.Draw(SolidCircle.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, SolidCircle.Size() * 0.5f, squash * dust.scale * 0.075f, SpriteEffects.None, 0);
            }
        }
    }
}
