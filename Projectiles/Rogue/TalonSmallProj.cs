using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class TalonSmallProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            // On-spawn effects
            if (Projectile.ai[0] == 0)
            {
                // Store the X and Y of the spawn velocity so it can be used for trig calculations
                Projectile.localAI[0] = Projectile.velocity.X;
                Projectile.localAI[1] = Projectile.velocity.Y;
            }

            // Doesn't collide with tiles for the first 2 frames
            Projectile.tileCollide = Projectile.ai[0] > 2f;

            // Apply fancy sine movement. Original velocity is reconstructed so that it can be used in the calculation.
            Vector2 originalVelocity = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            ApplySineVelocity(originalVelocity);

            // spawn dust
            if (Main.rand.NextBool(5))
            {
                int dustType = Main.rand.NextBool() ? DustID.Chlorophyte : DustID.Ash;
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, Scale: 0.9f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.ai[0]++;
        }

        private void ApplySineVelocity(Vector2 baseVelocity)
        {
            float radians = Projectile.ai[1] * (float)Math.Sin(-MathHelper.PiOver2 + 0.25f * Projectile.ai[0]) * 0.5f;
            Projectile.velocity = baseVelocity.RotatedBy(radians);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.Chlorophyte : DustID.UnusedBrown;
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Color draw = Color.Lerp(lightColor, Color.White, 0.5f);
            SpriteEffects sp = Projectile.ai[1] == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(draw), Projectile.rotation, tex.Size() / 2f, Projectile.scale, sp, 0);
            return false;
        }
    }
}
