using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class LeviathanBomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        private bool visible = false;

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 172;
            Projectile.height = 172;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(visible);
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            visible = reader.ReadBoolean();
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Projectile.velocity *= 1.005f;

            Projectile.rotation += 0.1f;

            if (visible && Projectile.alpha > 0)
                Projectile.alpha -= 15;

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] == 12f)
                visible = true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 80f, targetHitbox);

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                int meteorDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 2f);
                Main.dust[meteorDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[meteorDust].scale = 0.5f;
                    Main.dust[meteorDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 15; j++)
            {
                int meteorDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 3f);
                Main.dust[meteorDust2].noGravity = true;
                Main.dust[meteorDust2].velocity *= 5f;
                meteorDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 2f);
                Main.dust[meteorDust2].velocity *= 2f;
            }
        }
    }
}
