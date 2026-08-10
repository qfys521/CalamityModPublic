using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TheStormLightningShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public bool homing = true;
        public bool hasZaged = false;
        public int zagDirection = 1;
        public Vector2 effectVel;
        public NPC closestTarget;
        public float colorValue = 0;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.extraUpdates = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration = 20; // Fast weapon go brr
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            if (time == 0)
            {
                if (Projectile.ai[0] == 5)
                { 
                    Projectile.extraUpdates = 60;
                    Projectile.penetrate = -1;
                }
                colorValue += Main.rand.Next(0, 20);
                zagDirection = Main.rand.NextBool() ? 1 : -1;
                effectVel = Projectile.velocity;
            }
            colorValue = MathHelper.Lerp(colorValue, 50, 0.035f);
            Color usedColor = Color.Lerp(Color.Cyan, Color.Orchid, Utils.GetLerpValue(0, 50, colorValue));
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (targetDist < 1400f)
            {
                if (Projectile.timeLeft % 2 == 0)
                {
                    Particle spark2 = new BoltParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 7, 0.3f, usedColor, new Vector2(1.8f, 0.8f), true, true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                if (Main.rand.NextBool(35))
                {
                    Particle spark2 = new BoltParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.3f, 1.9f), false, 23, Main.rand.NextFloat(0.2f, 0.3f), usedColor, new Vector2(1.8f, 0.8f), true, true, false, 0.3f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
            if (time % 15 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, (effectVel * 10) * Main.rand.NextFloat(-0.4f, -0.7f), 0, default, Main.rand.NextFloat(0.45f, 0.6f));
                dust.noGravity = true;
                dust.color = usedColor;
                effectVel = Projectile.velocity.RotatedBy(0.2f * zagDirection * (hasZaged ? 1 : 0.5f)) * 0.08f;
            }
            if (time % 20 == 0)
            {
                hasZaged = true;
                zagDirection *= -1;
            }

            closestTarget = Projectile.Center.ClosestNPCAt(900);
            if (closestTarget != null && homing && Projectile.ai[0] < 5)
            {
                float moveSpeed = Utils.GetLerpValue(300, 250, Projectile.timeLeft, true);
                CalamityUtils.HomeInOnSelectedNPC(Projectile, closestTarget, true, moveSpeed, 12, 0.97f, accelerate: true);

                colorValue = MathHelper.Lerp(50, 0, Utils.GetLerpValue(500, 0, Vector2.Distance(closestTarget.Center, Projectile.Center), true));
            }
            if (!homing && Projectile.velocity.Length() < 12)
                Projectile.velocity *= 1.01f;

            time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target != closestTarget && Projectile.ai[0] < 5)
            {
                Projectile.numHits--;
                Projectile.penetrate++;
            }
            else
            {
                colorValue = Main.rand.Next(0, 10);
                homing = false;
            }

            target.AddBuff(BuffID.Electrified, 90);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, (Projectile.velocity * 4).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(0.6f, 0.8f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(5) ? Color.Cyan : Color.Orchid;
            }
        }
    }
}
