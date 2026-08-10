using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace CalamityMod.Projectiles.Enemy
{
    public class HorsPoisonBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Projectiles/Magic/VitriolicViperSpit";
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 15;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < 0.08f)
                Projectile.alpha += 15;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Venom);
                Main.dust[d].noGravity = true;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.position - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            float totalDusts = 18f;
            for (float i = 0f; i < totalDusts; i++)
            {
                Vector2 ringSpeed = new Vector2((float)Math.Cos(i / totalDusts * MathHelper.TwoPi), (float)Math.Sin(i / totalDusts * MathHelper.TwoPi) * 0.5f).RotatedBy(Projectile.rotation) * 4f * Projectile.scale;
                Dust droplets = Dust.NewDustPerfect(Projectile.Center, DustID.Venom, ringSpeed, 100);
                droplets.noGravity = true;
            }
        }
    }
}
