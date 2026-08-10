using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Environment
{
    public class BarrelCactusProjectile : ModProjectile, ILocalizedModType
    {   //I tried, someone else figure it out I bet YuH could do it
        public new string LocalizationCategory => "Projectiles.Misc";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.RollingCactus);

            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.damage = 70;
            Projectile.knockBack = 6f;

            Projectile.friendly = false;
            Projectile.hostile = true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = tex.Bounds;
            Vector2 origin = frame.Size() * 0.5f;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                frame,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }
    }
}
