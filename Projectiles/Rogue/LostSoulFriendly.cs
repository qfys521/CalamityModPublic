using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class LostSoulFriendly : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 150;
        }

        public override bool? CanHitNPC(NPC target) => (Projectile.timeLeft < 210 || Projectile.ai[0] == 2f) && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 8)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = 0;

            Lighting.AddLight(Projectile.Center, 0.25f, 0.25f, 0.25f);
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);

            if (Projectile.ai[0] != 1f && (Projectile.timeLeft < 210 || Projectile.ai[0] == 2f))
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 600f, 12f, 20f);
        }

        // Reduce damage of projectiles if more than the cap are active
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.DamageType == DamageClass.Generic)
            {
                int cap = 10;
                float capDamageFactor = 0.05f;
                int excessCount = Main.player[Projectile.owner].ownedProjectileCounts[Type] - cap;
                modifiers.SourceDamage *= MathHelper.Clamp(1f - (capDamageFactor * excessCount), 0f, 1f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int j = 0; j <= 10; j++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpectreStaff, 0f, 0f, 100, default, 1f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(150, 150, 150, 150);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
