using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class SporeGasPlantera : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SporeGas;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Projectile.ai[1] += 1f;
            if (Projectile.ai[1] > (Main.getGoodWorld ? 600f : 900f))
            {
                Projectile.localAI[0] += 10f;
                Projectile.damage = 0;
            }

            if (Projectile.localAI[0] > 255f)
            {
                Projectile.Kill();
                Projectile.localAI[0] = 255f;
            }

            float lightValues = (255 - Projectile.alpha) * 0.6f / 255f;
            Lighting.AddLight(Projectile.Center, 0f, lightValues, 0f);

            Projectile.alpha = (int)(100.0 + Projectile.localAI[0] * 0.7);
            Projectile.rotation += Projectile.velocity.X * 0.02f;
            Projectile.rotation += Projectile.direction * 0.002f;

            if (Projectile.velocity.Length() > (Main.getGoodWorld ? 4f : 2f))
                Projectile.velocity *= 0.985f;
        }

        public override bool CanHitPlayer(Player target) => Projectile.ai[1] <= (Main.getGoodWorld ? 600f : 900f) && Projectile.ai[1] > 120f;

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.ai[1] > (Main.getGoodWorld ? 600f : 900f))
            {
                byte b2 = (byte)((26f - (Projectile.ai[1] - (Main.getGoodWorld ? 600f : 900f))) * 10f);
                byte a2 = (byte)(Projectile.alpha * (b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, Projectile.alpha);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // Changes the texture of the projectile
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            switch ((int)Projectile.ai[0])
            {
                case 0:
                    break;
                case 1:
                    Main.instance.LoadProjectile(ProjectileID.SporeGas2);
                    texture = TextureAssets.Projectile[ProjectileID.SporeGas2].Value;
                    break;
                case 2:
                    Main.instance.LoadProjectile(ProjectileID.SporeGas3);
                    texture = TextureAssets.Projectile[ProjectileID.SporeGas3].Value;
                    break;
                default:
                    break;
            }
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1, texture);
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            if (Projectile.ai[1] <= (Main.getGoodWorld ? 600f : 900f) && Projectile.ai[1] > 120f)
                target.AddBuff(BuffID.Poisoned, 480);
        }
    }
}
