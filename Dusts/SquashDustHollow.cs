using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class SquashDustHollow : ModDust
    {
        public static Asset<Texture2D> BloomRing { get; private set; }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void Load()
        {
            if (!Main.dedServ)
                BloomRing = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing");
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

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            // dust.fadeIn is used to determine how intense the squash is, 1 is no squash, 0 is normal squash
            Vector2 squash = Vector2.Lerp(new Vector2(Utils.Remap(dust.velocity.Length(), 2, 7, 1, 0.5f), Utils.Remap(dust.velocity.Length(), 2, 7, 1, 2.5f)), Vector2.One, dust.fadeIn);

            // Glow Orb
            Main.EntitySpriteDraw(BloomRing.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomRing.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.EntitySpriteDraw(BloomRing.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.2f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomRing.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(BloomRing.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.4f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomRing.Size() * 0.5f, squash * dust.scale * 0.075f, SpriteEffects.None, 0);
            }
            return false;
        }
    }

    public class SquashDustHollowPixelated : SquashDustHollow
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool PreDraw(Dust dust)
        {
            PixelationManager.AddPixelatedDrawer((_) => DrawPixelated(dust), Enums.GeneralDrawLayer.AfterDusts);
            return false;
        }

        private static void DrawPixelated(Dust dust)
        {
            // dust.fadeIn is used to determine how intense the squash is, 1 is no squash, 0 is normal squash
            Vector2 squash = Vector2.Lerp(new Vector2(Utils.Remap(dust.velocity.Length(), 2, 7, 1, 0.5f), Utils.Remap(dust.velocity.Length(), 2, 7, 1, 2.5f)), Vector2.One, dust.fadeIn);

            // Glow Orb
            Main.EntitySpriteDraw(BloomRing.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomRing.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.EntitySpriteDraw(BloomRing.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.2f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomRing.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(BloomRing.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.4f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomRing.Size() * 0.5f, squash * dust.scale * 0.075f, SpriteEffects.None, 0);
            }
        }
    }
}
