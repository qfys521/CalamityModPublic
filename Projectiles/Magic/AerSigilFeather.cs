using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;

namespace CalamityMod.Projectiles.Magic
{
    // This projectile is intended to be a simple VFX projectile.
    public class AerSigilFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        private float extraRotation = 0;
        private float intervalModifier = 1f;
        private bool randomized = false;

        public override void SetDefaults()
        {
            Projectile.width = 151;
            Projectile.height = 205;
            Projectile.scale = 0.15f;

            Projectile.friendly = false;
            Projectile.tileCollide = true; 
            Projectile.ignoreWater = true; 
            Projectile.penetrate = 1; 
            Projectile.timeLeft = 170;
        }
        public override void AI()
        {
            // Fade out
            if (Projectile.timeLeft < 50)
            {
                Projectile.alpha = (int)(255 - (255f / 50f) * Projectile.timeLeft);
            }


            // Attempts to replicate the motion of a feather swaying in wind
            float blendProgress = (170f - Projectile.timeLeft) / (170f - 100f);
            blendProgress = Math.Min(blendProgress, 1f);

            if (!randomized)
            {
                extraRotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
                intervalModifier = Main.rand.NextFloat(0.815f, 1.225f);
                randomized = true;
            }

            Vector2 newFeatherVelocity = Vector2.Zero;
            newFeatherVelocity.Y = Projectile.velocity.Y * 0.96f + 0.04f;
            newFeatherVelocity.X = 1.4f * (float)Math.Cos(Projectile.timeLeft * 0.035f * intervalModifier); // The first float is effectively magnitude


            float newFeatherRotation = extraRotation + (float)Math.Sin(Projectile.timeLeft * 0.035f);


            Projectile.velocity = Vector2.Lerp(Projectile.velocity, newFeatherVelocity, blendProgress);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, newFeatherRotation, blendProgress);


        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor = Color.LightGoldenrodYellow * 0.5f;
            return true;
        }
    }
}
