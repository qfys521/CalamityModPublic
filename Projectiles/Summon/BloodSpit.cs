using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class BloodSpit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.Calamity().BloodstoneOrbValue = 12; //Bloodstone Orb lifesteal. May want to go back to regular lifesteal.
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.77f, Projectile.Opacity * 0.15f, Projectile.Opacity * 0.08f);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (Projectile.frameCounter++ > 4)
            {
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
                Projectile.frameCounter = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust blood = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, !ChildSafety.Disabled ? DustID.Cloud : DustID.Blood, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
                blood.velocity = Main.rand.NextVector2Circular(1f, 2f);
                blood.noGravity = true;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (ChildSafety.Disabled)
            {
                return base.PreDraw(player, ref lightColor);
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Color drawColor = Color.CornflowerBlue;
            drawColor *= Projectile.Opacity;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
