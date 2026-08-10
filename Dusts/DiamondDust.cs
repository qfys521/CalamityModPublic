using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class DiamondDust : ModDust
    {
        public static Asset<Texture2D> GlowDiamond { get; private set; }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void Load()
        {
            if (!Main.dedServ)
                GlowDiamond = ModContent.Request<Texture2D>("CalamityMod/Particles/SquareRotated");
        }

        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
        }

        public override bool Update(Dust dust)
        {
            dust.rotation = dust.velocity.ToRotation() + MathHelper.PiOver2;

            dust.velocity *= 0.96f;
            if (dust.noGravity)
                dust.scale -= 0.045f;
            else
            {
                dust.scale -= 0.03f;
                dust.velocity.Y += Main.rand.NextFloat(0.1f, 0.35f);
            }

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittance)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            if (dust.scale <= 0)
                dust.active = false;

            dust.position += dust.velocity;
            dust.fadeIn++; // Fade in is used for the time strech instead

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            // dust.fadeIn is used to determine how intense the squash is, 1 is no squash, 0 is normal squash
            float squashLerp = Utils.GetLerpValue(10, 25, dust.fadeIn, true);
            Vector2 squash = new Vector2(MathHelper.Lerp(1, 0.3f, squashLerp), MathHelper.Lerp(1, 7f, squashLerp));

            // Glow Orb
            Main.spriteBatch.Draw(GlowDiamond.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowDiamond.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(GlowDiamond.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.2f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowDiamond.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                Main.spriteBatch.Draw(GlowDiamond.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.4f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha) * (i == 0 ? 1f : 0.7f), dust.rotation, GlowDiamond.Size() * 0.5f, squash * dust.scale * 0.08f * (i == 0 ? 0.7f : 1), SpriteEffects.None, 0);
            }
            return false;
        }
    }

    public class DiamondDustPixelated : DiamondDust
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool PreDraw(Dust dust)
        {
            PixelationManager.AddPixelatedDrawer((_) => DrawPixelated(dust), Enums.GeneralDrawLayer.AfterDusts);
            return false;
        }

        public static void DrawPixelated(Dust dust)
        {
            // dust.fadeIn is used to determine how intense the squash is, 1 is no squash, 0 is normal squash
            float squashLerp = Utils.GetLerpValue(10, 25, dust.fadeIn, true);
            Vector2 squash = new Vector2(MathHelper.Lerp(1, 0.3f, squashLerp), MathHelper.Lerp(1, 7f, squashLerp));

            // Glow Orb
            Main.spriteBatch.Draw(GlowDiamond.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowDiamond.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(GlowDiamond.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.2f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, GlowDiamond.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(GlowDiamond.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.4f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha) * (i == 0 ? 1f : 0.7f), dust.rotation, GlowDiamond.Size() * 0.5f, squash * dust.scale * 0.08f * (i == 0 ? 0.7f : 1), SpriteEffects.None, 0);
            }
        }
    }
}
