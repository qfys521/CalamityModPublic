using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class DevourerofCodsBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
        }

        //fuck glowmasks btw
        //i second this notion -Dominic
        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/DevourerofCodsGlow").Value;
            float xOffset = (glowmask.Width - Projectile.width) * 0.5f + Projectile.width * 0.5f;
            Vector2 drawPos = Projectile.position - Main.screenPosition;
            drawPos.X += xOffset;
            drawPos.Y += Projectile.height / 2f + Projectile.gfxOffY;
            Rectangle frame = new(0, 0, glowmask.Width, glowmask.Height);
            Vector2 origin = new Vector2(xOffset, Projectile.height / 2f);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (Projectile.ai[0] <= 1f)
            {
                Main.spriteBatch.Draw(glowmask, drawPos, frame, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }
        }

        public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Lighting.AddLight(Projectile.Center, 0.35f, 0f, 0.25f);
            return true;
        }
    }
}
