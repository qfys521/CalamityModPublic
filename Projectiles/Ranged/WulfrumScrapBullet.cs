using System;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class WulfrumScrapBullet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 100;
            Projectile.extraUpdates = 4;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            Lighting.AddLight(Projectile.Center, (Color.PaleGoldenrod * 0.8f).ToVector3() * 0.5f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            return base.OnTileCollide(oldVelocity);
        }

        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fadeOpacity = (float)Math.Sqrt(Projectile.timeLeft / 100f);
            return Color.PaleGoldenrod * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos)
        {
            return (1 - completionRatio) * 6.4f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(Request<Texture2D>("CalamityMod/ExtraTextures/Trails/BasicTrail"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);
            return false;
        }
    }
}
