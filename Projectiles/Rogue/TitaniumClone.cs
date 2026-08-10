using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class TitaniumClone : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/TitaniumShuriken";

        private static float RotationIncrement = 0.22f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.alpha = 150;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 200;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += RotationIncrement;
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 30f)
            {
                CalamityUtils.HomeInOnNPC(Projectile, true, 320f, 12f, 20f);
            }
        }

        public override bool? CanDamage() => Projectile.ai[0] >= 30f ? null : false;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
