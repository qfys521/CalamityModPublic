using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EquanimityLightShard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "Terraria/Images/Item_528";
        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;

        public int TimeBeforeHoming = 30;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.DamageType = RogueDamageClass.Instance;
            TimeBeforeHoming = Main.rand.Next(30, 60);
        }
        public override bool? CanDamage()
        {
            if (Projectile.ai[1] < TimeBeforeHoming) return false;
            return base.CanDamage();
        }
        public override void AI()
        {
            Projectile.ai[1]++;
            //Rotation and gravity
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.03f;
            if (Projectile.ai[1] < TimeBeforeHoming)
            {
                Projectile.velocity *= 0.94f;
            }
            else
            {
                Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 10, 0.05f);
                CalamityUtils.HomeInOnNPC(Projectile, false, Projectile.ai[0] == 1f ? 800 : 400, Projectile.ai[2], 0.2f);
                Projectile.velocity.Y += 0.25f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center - Projectile.velocity / 2f, 0, 0, DustID.GemDiamond, 0f, 0f, 100, default, 1f);
                Main.dust[dust].velocity *= 2f;
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Projectile.Kill();
            return true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D Texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (Projectile.ai[0] == 1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Main.EntitySpriteDraw(Texture, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, Texture.Size() * 0.5f, Projectile.scale * 1.25f, SpriteEffects.None);
                }
            }
            Rectangle frame = Texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(Texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
