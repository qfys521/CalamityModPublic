using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class Valaricicle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.coldDamage = true;
            Projectile.ArmorPenetration = 10;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 150 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            if (Projectile.timeLeft < 150)
                CalamityUtils.HomeInOnNPC(Projectile, true, 600f, 9f, 20f);
            else
            {
                Projectile.velocity.X *= 0.9995f;
                Projectile.velocity.Y += 0.01f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.IceRod, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            string texString = Projectile.ai[2] == 1f ? "CalamityMod/Projectiles/Rogue/Valaricicle2" : Texture;
            Texture2D tex = ModContent.Request<Texture2D>(texString).Value;
            SpriteEffects spr = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, spr);
            return false;
        }
    }
}
