using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class BeamingBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.timeLeft = 120;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            if (Projectile.ai[0] == 1f)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/BeamingThornBlossom").Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void AI()
        {
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;
            Projectile.velocity *= 0.98f;
            for (int dust = 0; dust < 2; dust++)
            {
                int randomDust = Utils.SelectRandom(Main.rand, new int[]
                {
                    164,
                    58,
                    204
                });
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, randomDust, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 6; k++)
            {
                int randomDust = Utils.SelectRandom(Main.rand, new int[]
                {
                    164,
                    58,
                    204
                });
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, randomDust, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 velocity = ((MathHelper.TwoPi * i / 8f) - (MathHelper.ToRadians(67.5f) - Projectile.velocity.ToRotation())).ToRotationVector2() * 4f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BeamingBolt2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0]);
                }
            }
            SoundEngine.PlaySound(SoundID.Item105, Projectile.Center);
        }
    }
}
