using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Environment
{
    public class LavaChunk : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5)
            {
                Projectile.frame = 0;
            }
            if (Projectile.localAI[1] < 1f)
            {
                Projectile.localAI[1] += 0.002f;
                Projectile.scale -= 0.002f;
                Projectile.width = (int)(18f * Projectile.scale);
                Projectile.height = (int)(18f * Projectile.scale);
            }
            else
            {
                Projectile.Kill();
            }
            if (Projectile.scale > 0.25f)
            {
                for (int i = 0; i < 2; i++)
                {
                    float dustYOffset = 0f;
                    if (i == 1)
                    {
                        dustYOffset = Projectile.velocity.Y * 0.5f;
                    }
                    int lavaDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 3f + dustYOffset) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Torch, 0f, 0f, 100, default, Projectile.scale);
                    Main.dust[lavaDust].scale *= 2f + (float)Main.rand.Next(10) * 0.1f;
                    Main.dust[lavaDust].velocity *= 0.2f;
                    Main.dust[lavaDust].noGravity = true;
                    lavaDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 3f + dustYOffset) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100, default, Projectile.scale * 0.5f);
                    Main.dust[lavaDust].fadeIn = 1f + (float)Main.rand.Next(5) * 0.1f;
                    Main.dust[lavaDust].velocity *= 0.05f;
                }
            }
            else
            {
                Projectile.damage = 0;
            }
            if (Projectile.velocity.Y < 6f)
            {
                Projectile.velocity.Y = Projectile.velocity.Y + 0.05f;
            }
            if (Projectile.wet)
            {
                if (Projectile.velocity.Y < 0f)
                {
                    Projectile.velocity.Y = Projectile.velocity.Y * 0.98f;
                }
                if (Projectile.velocity.Y < 0.5f)
                {
                    Projectile.velocity.Y = Projectile.velocity.Y + 0.01f;
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
