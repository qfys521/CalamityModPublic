using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee
{
    public class SwordsplosionGreen : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = ProjAIStyleID.Beam;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            AIType = ProjectileID.EnchantedBeam;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.3f / 255f, (255 - Projectile.alpha) * 0.4f / 255f, (255 - Projectile.alpha) * 1f / 255f);
            if (Projectile.localAI[1] > 7f)
            {
                int greenBoi = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, new Color(53, Main.DiscoG, 255), 1.2f);
                Main.dust[greenBoi].velocity *= 0.1f;
                Main.dust[greenBoi].noGravity = true;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.Atan(90);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(53, Main.DiscoG, 255, Projectile.alpha);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft > 295)
                return false;

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 7; k++)
            {
                int greenBoi = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, 0f, 0f, 150, new Color(53, Main.DiscoG, 255), 1.2f);
                Main.dust[greenBoi].noGravity = true;
            }
        }
    }
}
