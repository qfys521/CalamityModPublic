using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class StealthNimbusCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Magic/ShadeNimbusCloud";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            if (Projectile.ai[0] == 1f)
                texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/StealthNimbusCloud2").Value;
            int height = texture.Height / Main.projFrames[Type];
            int frameHeight = height * Projectile.frame;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, frameHeight, texture.Width, height)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture.Width / 2f, (float)height / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.02f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                {
                    Projectile.frame = 0;
                    return;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<StealthNimbus>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0], 0f);
            }
        }

        public override bool? CanDamage() => false;
    }
}
