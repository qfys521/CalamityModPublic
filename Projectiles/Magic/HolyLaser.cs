using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Magic
{
    public class HolyLaser : BaseLaserbeamProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override float Lifetime => isBigLaser ? 60 : 30;

        public override float MaxScale => isBigLaser ? 1.5f : 0.8f;

        public override float MaxLaserLength => 1500f;

        private const string LaserTexturePath = "CalamityMod/ExtraTextures/Lasers/ProvidenceHolyRay";
        public override Texture2D LaserBeginTexture => Request<Texture2D>(LaserTexturePath + "Start").Value;
        public override Texture2D LaserMiddleTexture => Request<Texture2D>(LaserTexturePath + "Mid").Value;
        public override Texture2D LaserEndTexture => Request<Texture2D>(LaserTexturePath + "End").Value;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override Color LaserOverlayColor => Color.White with { A = 0 };
        public override Color LightCastColor => Color.Transparent;
        private Projectile Holdout => Main.projectile[(int)Projectile.ai[1]];

        private bool isBigLaser => Projectile.ai[2] == 1f;
        public Color color1 = Color.Goldenrod;
        public Color color2 = Color.Orange;
        public float extraRot = 0;
        public Vector2 displace = Vector2.Zero;

        private Player Owner { get; set; }

        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.Magic;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), (isBigLaser ? 180 : 90));

            if (Projectile.numHits > 1)
                Projectile.damage = (int)(Projectile.damage * (isBigLaser ? 0.55f : 0.6f));
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, isBigLaser ? 50f : 20f, ref _);
        }

        public override void UpdateLaserMotion()
        {
            float dir = Projectile.ai[0];
            if (time == 0)
            {
                if (!isBigLaser)
                {
                    extraRot = dir;
                    Projectile.ai[0] = 0;
                }
                else
                    extraRot = 1.2f * dir;
            }
            Projectile.velocity = Holdout.velocity.RotatedBy(extraRot);
            if (!isBigLaser)
            {
                Projectile.rotation = Holdout.velocity.ToRotation() - MathHelper.PiOver2 + extraRot;
                extraRot *= 0.85f;
            }
            else
            {
                Projectile.rotation = (Holdout.velocity.ToRotation() - MathHelper.PiOver2) + extraRot;
                extraRot -= 0.05f * dir;
            }
        }

        public override void AttachToSomething()
        {
            if (Owner == null || !Owner.active || Owner.dead || Owner.CCed || Owner.noItems || Owner.ownedProjectileCounts[ProjectileType<PurgeGuzzlerHoldout>()] == 0) return;
            Projectile.Center = Holdout.ModProjectile<PurgeGuzzlerHoldout>().GunTipPosition;
        }

        public override void ExtraBehavior()
        {
            Owner ??= Main.player[Projectile.owner];

            Vector2 effectsPosition = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Main.rand.NextFloat());
            Vector2 randomLineEffectPosition = effectsPosition + Main.rand.NextVector2Circular(7f * (isBigLaser ? 1.2f : 0.8f), 7f * (isBigLaser ? 1.2f : 0.8f));

            if (Main.rand.NextBool() || isBigLaser)
            {
                Dust laserDust = Dust.NewDustPerfect(randomLineEffectPosition, DustID.FireworksRGB, Projectile.velocity * Main.rand.NextFloat(5f, 40f), Scale: Main.rand.NextFloat(0.8f, 1.1f));
                laserDust.noGravity = true;
                laserDust.color = Main.rand.NextBool(3) ? color2 : color1;
            }

            for (int i = 0; i < (isBigLaser ? 4 : 2); i++)
            {
                effectsPosition = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Main.rand.NextFloat());
                randomLineEffectPosition = effectsPosition + Main.rand.NextVector2Circular(7f, 7f);
                if (i % 2 == 0)
                {
                    Dust laserDust2 = Dust.NewDustPerfect(randomLineEffectPosition, ModContent.DustType<LightDust>(), Projectile.velocity * Main.rand.NextFloat(5f, 40f), Scale: Main.rand.NextFloat(0.8f, 1.1f) * (isBigLaser ? 2 : 1));
                    laserDust2.noGravity = true;
                    laserDust2.color = Main.rand.NextBool(3) ? color2 : color1;
                    laserDust2.noLightEmittance = true;
                }
                else
                {
                    Particle spark2 = new CustomSpark(randomLineEffectPosition, Projectile.velocity * Main.rand.NextFloat(1f, 10f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(1.15f, 1.3f), Color.Lerp(color1, color2, Main.rand.NextFloat()), new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.3f, 0.4f));
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }

            time++;
        }
    }
}
