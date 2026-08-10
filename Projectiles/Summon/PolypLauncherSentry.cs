using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class PolypLauncherSentry : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 25;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            Projectile.velocity.Y += 0.5f;
            if (Projectile.velocity.Y > 10f)
            {
                Projectile.velocity.Y = 10f;
            }

            if (Projectile.ai[0] > 0f)
            {
                Projectile.ai[0] -= 1f;
                return;
            }
            Projectile.ai[1]++;

            NPC potentialTarget = Projectile.Center.MinionHoming(400f, player, false);

            if (Projectile.owner == Main.myPlayer && potentialTarget != null)
            {
                if (Projectile.ai[1] > 75f)
                {
                    Vector2 spawnPosition = new Vector2(Projectile.oldPosition.X + (Projectile.width / 2), Projectile.oldPosition.Y + (Projectile.height / 2));

                    float shootSpeed = 16f;
                    float gravity = -PolypLauncherProjectile.Gravity;
                    float distance = Vector2.Distance(spawnPosition, potentialTarget.Center);
                    float angle = 0.25f * MathF.Asin(MathHelper.Clamp(gravity * distance * 1.5f / MathF.Pow(shootSpeed, 2f), -1f, 1f));

                    Vector2 velocity = new Vector2(0f, -shootSpeed).RotatedBy(angle).RotatedByRandom(0.05f);
                    velocity.X *= (potentialTarget.Center.X - Projectile.Center.X < 0).ToDirectionInt();

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, ModContent.ProjectileType<PolypLauncherProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Projectile.ai[1] = 0f;
                    Projectile.netUpdate = true;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool? CanDamage() => false;
    }
}
