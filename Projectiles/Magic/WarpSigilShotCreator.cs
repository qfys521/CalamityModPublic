using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class WarpSigilShotCreator : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float FiringTimer => ref Projectile.ai[0];
        public ref float ParentIndex => ref Projectile.ai[1];

        private const int DelayBetweenShots = 5;
        private const float ShotSpeed = 20f;
        private const float SpawnDistance = 250f;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile parent = Main.projectile[(int)ParentIndex];
            bool parentActive = parent != null && parent.active && parent.type == ModContent.ProjectileType<WarpSigil>();

            if (!parentActive)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = parent.Center;
            Projectile.timeLeft = parent.timeLeft;
            Vector2 targetCenter = Main.MouseWorld;

            FiringTimer++;
            if (FiringTimer % DelayBetweenShots == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/UnstableCastersGauntlet/VisNeedleFire") { Volume = 0.35f, Pitch = -0.3f, PitchVariance = 0.1f }, Projectile.Center);

                // Shoot blasts from random angles, then pass a given point to target
                Vector2 fixedTargetOffset = Main.rand.NextVector2Circular(36f, 36f);
                float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawnOffset = startAngle.ToRotationVector2() * SpawnDistance;
                Vector2 spawnPosition = targetCenter + spawnOffset;

                Vector2 initialTargetLocation = targetCenter + fixedTargetOffset;
                Vector2 velocity = (initialTargetLocation - spawnPosition).SafeNormalize(Vector2.UnitX) * ShotSpeed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<WarpSigilShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: fixedTargetOffset.X, ai1: fixedTargetOffset.Y);
            }

        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => false;
    }
}
