using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Healing
{
    public class ShinobiHealOrb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Healing";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.MaxUpdates = 5;
            Projectile.timeLeft = 120 * Projectile.MaxUpdates;
        }

        public override void AI() => Projectile.HealingProjectile((int)Projectile.ai[1], Projectile.owner, 6f, 15f);

        internal Color ColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lerp(Color.Blue, Color.CornflowerBlue, completionRatio);
        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => 10f * (completionRatio < 0.2f ? MathHelper.Clamp(1f - MathF.Pow(5f * completionRatio - 1f, 2f), 0f, 1f) : Utils.GetLerpValue(1f, 0.2f, completionRatio, true));
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/BasicTrail"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(12f, 20f);
                float scale = Main.rand.NextFloat(0.4f, 1.2f);
                Particle sparkle = new CritSpark(Projectile.Center, velocity, Color.White, Color.DarkBlue, scale, 24, 0.1f, scale * 2f, Main.rand.NextFloat(0f, 0.01f));
                GeneralParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}
