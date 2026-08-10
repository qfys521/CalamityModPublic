using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class InkPoisonCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 45;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 3600;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Projectile.ai[0] < 180f)
            {
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            else
            {
                Projectile.damage = 0;
            }

            // Kill the Projectile if reached end of frame
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.Kill();
            }

            Projectile.velocity *= 0.98f;

            if (Math.Abs(Projectile.velocity.X) > 0f)
            {
                Projectile.spriteDirection = -Projectile.direction;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            var effects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 drawOrigin = new(tex.Width * 0.5f, Projectile.height * 0.5f);
            Vector2 vector = new Vector2(Projectile.Center.X, Projectile.Center.Y) - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
            Rectangle rectangle = new(0, tex.Height / Main.projFrames[Type] * Projectile.frame, tex.Width, tex.Height / Main.projFrames[Type]);
            Main.EntitySpriteDraw(tex, vector, rectangle, Color.White * 0.75f, Projectile.rotation, drawOrigin, Projectile.scale * 1.2f, effects, 0);

            return true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 20f, targetHitbox);

        public override bool CanHitPlayer(Player target) => Projectile.ai[0] < 180f;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(BuffID.Darkness, 300);
        }
    }
}
