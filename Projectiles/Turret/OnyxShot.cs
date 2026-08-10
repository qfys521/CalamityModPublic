using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Turret
{
    public class OnyxShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public bool ableToHit = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 60;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool PreAI()
        {
            // If projectile knockback is set to 0 in the tile entity file, projectile hits players instead
            // This is used to check if the projectile came from the hostile version of the tile entity
            if (Projectile.knockBack == 0f)
                Projectile.hostile = true;
            else Projectile.friendly = true;
            return true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] > 0f)
                Projectile.hide = false;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.localAI[0]++;
        }
        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override Color? GetAlpha(Color drawColor) => Color.White;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, lightColor);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.timeLeft = 3;
            ableToHit = false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;
        }
    }
}
