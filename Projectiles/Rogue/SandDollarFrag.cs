using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class SandDollarFrag : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // Gets overridden in PreDraw anyways
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.11f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            Projectile.rotation += Projectile.velocity.X * 0.025f;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Projectile.oldVelocity.X / 4, Projectile.oldVelocity.Y / 4, 0, new Color(234, 183, 100), 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/SandDollarFrag" + Projectile.ai[0].ToString()).Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None);
            return false;
        }
    }
}
