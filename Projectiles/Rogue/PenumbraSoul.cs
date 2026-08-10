using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class PenumbraSoul : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.height = 18;
            Projectile.width = 18;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.alpha = 80;
            Projectile.extraUpdates = 1;
            DrawOffsetX = 1;
            DrawOriginOffsetY = 4;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Continuously trail dust
            int trailDust = 1;
            for (int i = 0; i < trailDust; ++i)
            {
                int idx = Dust.NewDust(Projectile.position - Projectile.velocity, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 0, new Color(38, 30, 43));
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity += Projectile.velocity * 0.8f;
            }

            // If tentacle is currently on cooldown, reduce the cooldown.
            if (Projectile.ai[0] > 0f)
                Projectile.ai[0] -= 1f;

            // Home in on nearby enemies
            CalamityUtils.HomeInOnNPC(Projectile, true, 400f, 12f, 35f);
        }

        public override void OnKill(int timeLeft)
        {
            // Create a burst of dust
            int dustAmt = Main.rand.Next(30, 41);
            for (int i = 0; i < dustAmt; i++)
            {
                int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 0, new Color(38, 30, 43), Main.rand.NextFloat(1f, 1.8f));
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= Main.rand.NextFloat(2f, 3.1f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
