using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ArcherfishRing : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Particles/HollowCircleHardEdge";

        public static readonly SoundStyle PopSound = new("CalamityMod/Sounds/Custom/BubblyPop") { PitchVariance = 0.5f, Volume = 0.66f };

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
            Projectile.alpha = 15;
            Projectile.timeLeft = 300;
            Projectile.MaxUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);

        public override void AI()
        {
            //The ring expanding slowing while increasing opacity until it disappears at max size 
            if (Projectile.ai[0] == 0f)
            {
                Projectile.scale = 0.5f;
                Projectile.ai[0]++;
            }

            if (Projectile.scale <= 1.5f)
                Projectile.scale *= 1.015f;
            else
                Projectile.scale = 1.5f;

            if (Projectile.velocity.Length() < 0.08f)
                Projectile.alpha += 15;

            if (Projectile.alpha >= 255)
                Projectile.Kill();
            else
                Projectile.velocity *= 0.95f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Audio feedback for hitting the ring
            SoundEngine.PlaySound(PopSound, Projectile.Center);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 ringScale = Projectile.Size / tex.Size() * Projectile.scale;
            Color ringColor = Color.Lerp(Color.DodgerBlue, Color.Blue, Projectile.Opacity) * Projectile.Opacity * 1.2f;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, ringColor, Projectile.rotation, tex.Size() / 2f, ringScale, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            float totalDusts = 18f;
            for (float i = 0f; i < totalDusts; i++)
            {
                Vector2 ringSpeed = new Vector2((float)Math.Cos(i / totalDusts * MathHelper.TwoPi), (float)Math.Sin(i / totalDusts * MathHelper.TwoPi) * 0.5f).RotatedBy(Projectile.rotation) * 4f * Projectile.scale;
                Dust droplets = Dust.NewDustPerfect(Projectile.Center, DustID.Wet, ringSpeed, 100);
                droplets.noGravity = true;
            }
        }
    }
}
