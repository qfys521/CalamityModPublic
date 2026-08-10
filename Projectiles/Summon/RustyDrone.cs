using System;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class RustyDrone : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 12;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            // Determine frames.
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            // Hover in place.
            Projectile.velocity = -Vector2.UnitY * MathF.Sin(MathHelper.TwoPi * Projectile.timeLeft / 96f) * 3f;

            // Look at nearby enemies.
            NPC potentialTarget = Projectile.Center.MinionHoming(1000f, Owner);
            if (potentialTarget is not null)
                Projectile.spriteDirection = (Projectile.Center.X < potentialTarget.Center.X).ToDirectionInt();

            // Release pulses of irradiated energy.
            if (Projectile.timeLeft % RustyBeaconPrototype.PulseReleaseRate == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1.6f }, Projectile.Center);

                if (Main.myPlayer == Projectile.owner)
                {
                    int pulse = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RustyBeaconPulse>(), Projectile.damage, 0f, Projectile.owner);
                    if (Main.projectile.IndexInRange(pulse))
                        Main.projectile[pulse].originalDamage = Projectile.originalDamage;
                }
            }
        }

        // The drone itself does not do damage.
        public override bool? CanDamage() => false;
    }
}
