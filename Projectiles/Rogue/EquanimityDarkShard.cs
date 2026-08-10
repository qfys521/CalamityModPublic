using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EquanimityDarkShard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "Terraria/Images/Item_527";
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.penetrate = 4;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 45;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.03f;
            Projectile.velocity *= 0.94f;
            if (Projectile.ai[0] == 1f)
            {
                Projectile.penetrate = -1;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Projectile.Kill();
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] == 1f)
            {

            }
            for (int i = 0; i < 16; i++)
            {
                int dust = Dust.NewDust(Projectile.Center - Projectile.velocity / 2f, 0, 0, DustID.Asphalt, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 0.6f;
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D Texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (Projectile.ai[0] == 1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Main.EntitySpriteDraw(Texture, Projectile.Center - Main.screenPosition, null, Color.Indigo with { A = 0 }, Projectile.rotation, Texture.Size() * 0.5f, Projectile.scale * 1.5f, SpriteEffects.None);
                }
            }
            Rectangle frame = Texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(Texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
