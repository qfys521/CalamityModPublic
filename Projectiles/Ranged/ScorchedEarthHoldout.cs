using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Ranged.ScorchedEarth;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class ScorchedEarthHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<ScorchedEarth>();
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitY.RotatedBy(Projectile.rotation) * 5f * Projectile.spriteDirection * Owner.gravDir;
        public override float RecoilResolveSpeed => 0.1f;
        public override float MaxOffsetLengthFromArm => 15f;
        public override float OffsetXUpwards => -12f;
        public override float OffsetXDownwards => 2f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 10f;

        public ref float ShootingTimer => ref Projectile.ai[0];
        public ref float TimerBetweenBursts => ref Projectile.ai[1];

        public override void HoldoutAI()
        {

            if (ShootingTimer >= HeldItem.useAnimation)
            {
                // Shooting the burst of rockets.
                // The time is adapted to the speed modifier from the reforge.
                int adaptedTimeBetweenBursts = TimeBetweenBursts * HeldItem.useAnimation / OriginalUseTime;

                if (ShootingTimer % adaptedTimeBetweenBursts == 0f)
                    ShootRocket(HeldItem, false);

                // Resets the timers when everything has been shot.
                if (ShootingTimer >= HeldItem.useAnimation + adaptedTimeBetweenBursts * (ProjectilesPerBurst - 1))
                {
                    ShootingTimer = 0f;
                    TimerBetweenBursts = 0f;
                }
                if (Main.dedServ)
                    return;
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel, Main.rand.NextBool() ? Color.DarkSlateGray : Color.DarkGray, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.3f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);
                //Visual request from the donor that I tried and didn't end up liking. This is commented and not deleted in case the donor really wants this effect 
                //for (int i = 0; i < 2; i++)
                {
                    //Vector2 backsmokeVel = new Vector2(6, -2) * Main.rand.NextFloat(0.1f, 1.1f);
                    //Particle backsmoke = new HeavySmokeParticle(GunTipPosition + (-Projectile.velocity * 110f), -Projectile.velocity.SafeNormalize(Vector2.UnitY) * backsmokeVel, Main.rand.NextBool() ? Color.DarkSlateGray : Color.Black, Main.rand.Next(20, 30 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                    //GeneralParticleHandler.SpawnParticle(backsmoke);
                }
            }

            ShootingTimer++;
        }

        public void ShootRocket(Item item, bool isRMB)
        {
            // We use the velocity of this projectile as its direction vector.
            Vector2 projectileVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero);

            // Every time we shoot we use ammo.
            // With this method we also use the item's stats, like the shoot speed, or the type of ammo it was used.
            Owner.PickAmmo(item, out _, out float projSpeed, out int damage, out float knockback, out int rocketType);

            // Spawns the projectile.
            Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 12;
            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(10f)), ModContent.ProjectileType<ScorchedEarthRocket>(), damage, knockback, Projectile.owner, rocketType);

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;
            SoundEngine.PlaySound(RocketShoot with { Pitch = 0.045f }, Projectile.Center);

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            OffsetLengthFromArm -= 7f;

            Particle shootPulse = new DirectionalPulseRing(GunTipPosition,Vector2.Zero,Color.Gray * 0.7f,new Vector2(0.5f, 1f),Projectile.rotation, 0.1f, 0.4f, 25);
            GeneralParticleHandler.SpawnParticle(shootPulse);
            for (int i = 0; i <= 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(3) ? 303 : 244, (shootVelocity * Main.rand.NextFloat(0.2f, 1.1f)).RotatedByRandom(0.4f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            return false;
        }
    }
}
