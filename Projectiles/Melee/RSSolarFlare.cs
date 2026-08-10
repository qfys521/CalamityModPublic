using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class RSSolarFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 150;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            if (Projectile.timeLeft < 60)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.scale -= 0.02f;
                Projectile.alpha += 15;
                if (Projectile.alpha >= 150)
                {
                    Projectile.alpha = 150;
                    Projectile.localAI[0] = 1f;
                }
            }
            else if (Projectile.localAI[0] == 1f)
            {
                Projectile.scale += 0.02f;
                Projectile.alpha -= 15;
                if (Projectile.alpha <= 0)
                {
                    Projectile.alpha = 0;
                    Projectile.localAI[0] = 0f;
                }
            }
            Projectile.velocity.X *= 0.975f;
            Projectile.velocity.Y *= 0.975f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }
            Lighting.AddLight(Projectile.Center, 0.75f, 0.75f, 0f);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4f;
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.PiOver2), ModContent.ProjectileType<SandFire>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(-MathHelper.PiOver2), ModContent.ProjectileType<SandFire>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int j = 0; j < 20; j++)
            {
                int shiny = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Teleporter, 0f, 0f, 100, default, 0.5f);
                Main.dust[shiny].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[shiny].scale = 0.5f;
                    Main.dust[shiny].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int k = 0; k < 35; k++)
            {
                int shiny2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 100, default, 1f);
                Main.dust[shiny2].noGravity = true;
                Main.dust[shiny2].velocity *= 5f;
                shiny2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Teleporter, 0f, 0f, 100, default, 0.5f);
                Main.dust[shiny2].velocity *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 240);
        }
    }
}
