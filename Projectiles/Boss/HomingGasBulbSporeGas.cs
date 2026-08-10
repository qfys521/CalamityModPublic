using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HomingGasBulbSporeGas : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
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
            }

            if (Projectile.localAI[0] > 255f)
            {
                Projectile.Kill();
                Projectile.localAI[0] = 255f;
            }

            float lightValues = (255 - Projectile.alpha) * 0.6f / 255f;
            Lighting.AddLight(Projectile.Center, lightValues, 0f, lightValues);

            Projectile.rotation += Projectile.velocity.X * 0.02f;
            Projectile.rotation += Projectile.direction * 0.002f;

            if (Projectile.velocity.Length() > (Main.getGoodWorld ? 2f : 0.5f))
                Projectile.velocity *= 0.985f;

            if (Projectile.timeLeft <= 40)
            {
                Projectile.Opacity = Utils.GetLerpValue(0, 40, Projectile.timeLeft);
            }
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity > 0.8f;

        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor;
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
                    texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/HomingGasBulbSporeGas2").Value;
                    break;
                case 2:
                    texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/HomingGasBulbSporeGas3").Value;
                    break;
                default:
                    break;
            }
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor * Projectile.Opacity, 1, texture);
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            if (Projectile.ai[1] <= (Main.getGoodWorld ? 600f : 900f) && Projectile.ai[1] > 120f)
                target.AddBuff(BuffID.Poisoned, 240);
        }
    }
}
