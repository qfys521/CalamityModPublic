using System.IO;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon.SmallAresArms
{
    public class ExoskeletonLaserCannon : ExoskeletonCannon
    {
        public ref float ShootCounter => ref Projectile.localAI[0];

        // This is how many small, regular lasers will happen before the laser will charge up and prepare a laserbeam.
        public const int NormalLasersBeforeBeam = 6;

        public override int ShootRate => ShootCounter % NormalLasersBeforeBeam == 0f ? 150 : AresExoskeleton.LaserCannonNormalShootRate;

        public override float ShootSpeed => 19f;

        public override Vector2 OwnerRestingOffset => HoverOffsetTable[HoverOffsetIndex];

        public override void ClampFirstLimbRotation(ref double limbRotation) => limbRotation = RotationalClampTable[HoverOffsetIndex];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ShootCounter);
            base.SendExtraAI(writer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ShootCounter = reader.ReadSingle();
            base.ReceiveExtraAI(reader);
        }

        public override void ShootAtTarget(NPC target, Vector2 shootDirection)
        {
            // Play the laser cannon fire sound.
            SoundEngine.PlaySound(CommonCalamitySounds.LaserCannonSound with { Volume = 0.2f }, Projectile.Center);

            // Create a burst of dust.
            for (int i = 0; i < 24; i++)
            {
                Dust laserEnergy = Dust.NewDustPerfect(Projectile.Center + shootDirection * Projectile.width * Projectile.scale * 0.45f, DustID.TheDestroyer);
                laserEnergy.velocity = (MathHelper.TwoPi * i / 24f).ToRotationVector2() * 4f;
                laserEnergy.scale = 1.1f;
                laserEnergy.fadeIn = 0.4f;
                laserEnergy.noGravity = true;
            }

            // Shoot the laser. This only happens for the owner client.
            if (Main.myPlayer != Projectile.owner)
                return;

            int laserID = ModContent.ProjectileType<MinionLaserBurst>();
            bool fireLaser = ShootCounter % NormalLasersBeforeBeam == NormalLasersBeforeBeam - 1f;
            if (fireLaser)
                laserID = ModContent.ProjectileType<CannonLaserbeam>();

            Vector2 laserVelocity = shootDirection * ShootSpeed;
            int laser = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, laserVelocity, laserID, (int)(Projectile.damage * AresExoskeleton.LaserDamageFactor), 0f, Projectile.owner);
            if (Main.projectile.IndexInRange(laser))
            {
                if (fireLaser)
                    Main.projectile[laser].ai[1] = Projectile.identity;
            }

            // Increment the shoot counter. This decides if the cannon fires a simple laser or a beam.
            ShootCounter++;
            Projectile.netUpdate = true;
        }

        // Create some charge dust before firing a laserbeam.
        public override void PostAI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];
            if (ShootCounter % NormalLasersBeforeBeam != NormalLasersBeforeBeam - 1f || !TargetingSomething)
                return;

            Vector2 aimDirection = Projectile.rotation.ToRotationVector2();
            Dust laserEnergy = Dust.NewDustPerfect(Projectile.Center + aimDirection * Projectile.width * Projectile.scale * 0.51f, DustID.TheDestroyer);
            laserEnergy.velocity = aimDirection.RotatedByRandom(0.48f) * Main.rand.NextFloat(3f);
            laserEnergy.scale = 1.1f;
            laserEnergy.fadeIn = 0.4f;
            laserEnergy.noGravity = true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            DefaultDrawCannon(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/SmallAresArms/ExoskeletonLaserCannonGlowmask").Value);
            return false;
        }
    }
}
