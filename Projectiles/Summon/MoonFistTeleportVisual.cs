using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class MoonFistTeleportVisual : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetDefaults()
        {
            Projectile.drawLayer = Terraria.ID.ProjectileDrawLayerID.BehindNPCsAndTiles;
            Projectile.width = Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.timeLeft = 45;
            Projectile.MaxUpdates = 2;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 12f, Projectile.timeLeft);
            Projectile.scale = Utils.GetLerpValue(45f, 5f, Projectile.timeLeft);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D telegraphTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Color telegraphColor = Color.White * Projectile.Opacity * 0.2f;
            telegraphColor.A = 0;

            for (int i = 0; i < 35; i++)
            {
                Vector2 drawPosition = Projectile.Center + (MathHelper.TwoPi * i / 5f + Main.GlobalTimeWrappedHourly * 3f).ToRotationVector2() * 2f;
                drawPosition -= Main.screenPosition;

                Vector2 scale = new Vector2(0.58f, 1f) * Projectile.scale;
                scale *= MathHelper.Lerp(0.015f, 1f, i / 35f);

                Main.spriteBatch.Draw(telegraphTexture, drawPosition, null, telegraphColor, 0f, telegraphTexture.Size() * 0.5f, scale, 0, 0f);
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust magic = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), DustID.RainbowMk2);
                magic.color = Color.SkyBlue;
                magic.scale = 1.1f;
                magic.fadeIn = 0.6f;
                magic.velocity = Main.rand.NextVector2Circular(2f, 2f);
                magic.velocity = Vector2.Lerp(magic.velocity, -Vector2.UnitY * magic.velocity.Length(), Main.rand.NextFloat(0.65f, 1f));
                magic.noGravity = true;
            }
        }

    }
}
