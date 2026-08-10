using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class AntlionSkewerSandCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Particles/RancorFog";
        public ref float CloudHue => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 256;
            Projectile.friendly = true;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (CloudHue == 0f)
            {
                Projectile.scale = Main.rand.NextFloat(1f, 1.7f);
                Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                CloudHue = Main.rand.NextFloat(0.08f, 0.18f);
            }

            Projectile.rotation += Projectile.velocity.X * 0.004f;
            Projectile.velocity *= 0.985f;
            Projectile.Opacity = Utils.GetLerpValue(300f, 240f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, 90f, Projectile.timeLeft, true);

            foreach (NPC target in Main.ActiveNPCs)
            {
                if (CalamityUtils.CircularHitboxCollision(Projectile.Center, 80f, target.Hitbox))
                    target.Calamity().antlionCloudDebuffTimer = 30;
            }
        }

        public override bool? CanDamage() => false;

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 160;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity != oldVelocity)
                Projectile.velocity = Main.rand.NextFloat(-1.15f, -0.85f) * oldVelocity;
            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color mainColor = Main.hslToRgb(CloudHue, 1f, 0.7f) * Projectile.Opacity * 0.3f;
            Main.EntitySpriteDraw(texture, drawPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Color secondColor = Main.hslToRgb(MathHelper.Lerp(CloudHue, 0.15f, 0.6f), 1f, 0.7f) * Projectile.Opacity * 0.3f;
            Main.EntitySpriteDraw(texture, drawPosition, null, secondColor, Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
