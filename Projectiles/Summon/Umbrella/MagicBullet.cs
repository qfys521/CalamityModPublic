using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon.Umbrella
{
    public class MagicBullet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public VertexStrip TrailDrawer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.extraUpdates = 7;
            Projectile.light = 0.5f;
            Projectile.scale = 1.18f;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.ai[0]++ > 2f)
                Projectile.alpha = 0;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.betsysCurse)
                target.AddBuff(BuffID.BetsysCurse, 180);
            if (!target.ichor)
                target.AddBuff(BuffID.Ichor, 180);
            if (target.Calamity().markedForDeath)
                target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 180);
            if (target.Calamity().armorCrunch)
                target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 180);
            if (target.Calamity().whisperingDeath)
                target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Ichor, 180);
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 180);
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 180);
            target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 180);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.alpha != 0)
                return false;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // Draw the bullet.
            Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            // Draw Afterimages
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);
            return false;
        }
    }
}
