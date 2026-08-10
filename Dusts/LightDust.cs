using System;
using CalamityMod.Enums;
using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class LightDust : ModDust
    {
        public static Asset<Texture2D> SolidCircle { get; private set; }

        public static Asset<Texture2D> BloomCircle { get; private set; }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            SolidCircle = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle");
            BloomCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
        }

        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += MathF.Sign(dust.velocity.X);
            dust.velocity *= 0.98f;
            if (dust.noGravity)
                dust.scale += 0.02f;
            else
                dust.scale -= 0.01f;

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittance)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            return true;
        }

        public override bool PreDraw(Dust dust)
        {
            // Glow Orb
            Main.spriteBatch.Draw(BloomCircle.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomCircle.Size() * 0.5f, dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(BloomCircle.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomCircle.Size() * 0.5f, dust.scale * 0.04f, SpriteEffects.None, 0);
            if (!dust.noLight)
                Main.spriteBatch.Draw(SolidCircle.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, SolidCircle.Size() * 0.5f, dust.scale * 0.075f, SpriteEffects.None, 0);
            return false;
        }
    }

    public class LightDustPixelated : LightDust
    {
        public override string Texture => "CalamityMod/Dusts/LightDust";

        public override bool PreDraw(Dust dust)
        {
            PixelationManager.AddPixelatedDrawer((_) => DrawPixelated(dust), GeneralDrawLayer.AfterDusts);
            return false;
        }

        private static void DrawPixelated(Dust dust)
        {
            // Glow Orb
            Main.spriteBatch.Draw(BloomCircle.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomCircle.Size() * 0.5f, dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.spriteBatch.Draw(BloomCircle.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, BloomCircle.Size() * 0.5f, dust.scale * 0.04f, SpriteEffects.None, 0);
            if (!dust.noLight)
                Main.spriteBatch.Draw(SolidCircle.Value, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, SolidCircle.Size() * 0.5f, dust.scale * 0.075f, SpriteEffects.None, 0);
        }
    }
}
