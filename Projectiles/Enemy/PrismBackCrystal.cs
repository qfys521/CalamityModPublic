using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class PrismBackCrystal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Particles/SeaPrisms";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.timeLeft = 240;
            Projectile.ignoreWater = true;
            Projectile.scale = 0.85f;
        }

        public override void AI()
        {
            Projectile.velocity.X *= 0.995f;
            Projectile.velocity.Y += 0.03f;
            if (Projectile.velocity.Y > 5f)
                Projectile.velocity.Y = 5f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool? CanHitNPC(NPC target) => target.whoAmI == (int)Projectile.ai[1] ? false : null;

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IcyMerman);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = new Rectangle(18 * (int)Projectile.ai[0], 0, 16, 32);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
