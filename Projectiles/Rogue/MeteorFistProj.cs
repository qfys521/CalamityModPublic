using CalamityMod.DataStructures;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class MeteorFistProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";

        public List<VerletSimulatedSegment> Wire;
        private const int Lifetime = 210;
        private const int WireSegments = 15;
        private const float MaxSpeed = 22.5f;
        public ref float Timer => ref Projectile.ai[0];

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.penetrate = 2; // Only hits once, has 2 pierce to create a visual illusion of the connecting wire being left behind after hit
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Timer++;

            // Initialize the connecting wire
            if (Wire == null || Wire.Count < WireSegments)
            {
                Wire = new List<VerletSimulatedSegment>(WireSegments);
                for (int i = 0; i < WireSegments; i++)
                {
                    VerletSimulatedSegment segment = new VerletSimulatedSegment(Owner.Center + Vector2.UnitY * i * 10f);
                    Wire.Add(segment);
                }

                Wire[0].locked = true;
                Wire[Wire.Count - 1].locked = true;
            }

            // Spawn some fire dust, plus smoke if traveling fast enough
            if (Projectile.numHits == 0)
            {
                Dust meteorDust = Dust.NewDustDirect(Projectile.position - Projectile.velocity * 0.5f, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 0.5f);
                meteorDust.scale *= 2f + Main.rand.NextFloat();
                meteorDust.velocity *= 0.2f;
                meteorDust.noGravity = true;

                if (Projectile.velocity.Length() >= 7f)
                {
                    Vector2 smokePos = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * 10f;
                    HeavySmokeParticle smoking = new(smokePos, Vector2.UnitX.RotatedBy(Projectile.rotation + MathHelper.Pi) * 4f, Color.Orange, 5, 0.33f, 0.75f);
                    GeneralParticleHandler.SpawnParticle(smoking);
                }
            }

            if (Projectile.numHits == 0)
            {
                // Homing movement
                Vector2 guidedDirection = (Owner.ClampedMouseWorld() - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float guidedSpeed = Timer < 60f && !Projectile.Calamity().stealthStrike ? 4f : (Timer + (Projectile.Calamity().stealthStrike ? 60f : 0f)) / 7.5f;
                if (guidedSpeed > MaxSpeed)
                    guidedSpeed = MaxSpeed;
                float guidedTurnStrength = MathHelper.Lerp(0.05f, Projectile.Calamity().stealthStrike ? 0.32f : 0.18f, Timer / (float)Lifetime);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Owner.ClampedMouseWorld()).ToRotation(), guidedTurnStrength).ToRotationVector2() * guidedSpeed * Owner.Calamity().rogueVelocity;

                // If you haven't hit anything, explode and leave the wire
                if (Projectile.timeLeft == 2)
                    SetUpLeftoverWireEffect();
            }
            else
            {
                // Used for the sagging motion of the leftover wire
                Projectile.velocity.X *= 0.9f;
                Projectile.velocity.Y += 0.2f;
                if (Projectile.velocity.Y > 10f)
                    Projectile.velocity.Y = 10f;
            }

            // Done to ensure the leftover wire is flush with the ground
            Projectile.rotation = Projectile.numHits > 0 ? 0f : Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Wire[0].oldPosition = Wire[0].position;
            Wire[0].position = Owner.Center;
            Wire[Wire.Count - 1].oldPosition = Wire[0].position;
            Wire[Wire.Count - 1].position = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * (Projectile.numHits > 0 ? 14f : 8f);
            Wire = VerletSimulatedSegment.SimpleSimulation(Wire, 10, loops: 1, gravity: 0.3f);
        }

        // Both of these are done for wire leftover effect
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.numHits == 0)
                SetUpLeftoverWireEffect();
            return false;
        }
        public override bool? CanDamage() => Projectile.numHits == 0 ? null : false;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 120);
            if (Main.myPlayer == Projectile.owner)
            {
                // Stealth strikes drop a whole ass meteorite because hell yeah
                if (Projectile.Calamity().stealthStrike)
                {
                    Vector2 meteorSpawn = target.Center + new Vector2(Main.rand.NextFloat(-100f, 100f), Main.rand.NextFloat(-650f, -750f));
                    Vector2 meteorVel = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(meteorSpawn, target, 15f, 2);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), meteorSpawn, meteorVel, ModContent.ProjectileType<MeteorFistMeteorite>(), Projectile.damage, Projectile.knockBack * 2, Projectile.owner, target.whoAmI);
                }
            }

            // Visual effects
            SetUpLeftoverWireEffect();
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.OnFire, 120);

        private void SetUpLeftoverWireEffect()
        {
            Projectile.numHits++;
            Timer = 0f;
            Projectile.timeLeft = 90;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int k = 0; k < 30; k++)
            {
                int boomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                Main.dust[boomDust2].noGravity = true;
                Main.dust[boomDust2].velocity *= 5f;
                boomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                Main.dust[boomDust2].velocity *= 2f;
            }

            if (!Main.dedServ)
            {
                Vector2 source = new Vector2(Projectile.Center.X - 24f, Projectile.Center.Y - 24f);
                for (int g = 1; g <= 3; g++)
                {
                    float velocityMult = g * 0.33f;
                    for (int spawn = 0; spawn < 2; spawn++)
                    {
                        int type = Main.rand.Next(61, 64);
                        int smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                        Gore gore = Main.gore[smoke];
                        gore.velocity *= velocityMult;
                    }
                }
            }
        }

        public float WidthFunction(float completionRatio, Vector2 vertexPos) => 0.4f;
        public Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fadeThreshold = 60f;
            float opacity = Projectile.numHits > 0 && Timer > fadeThreshold ? MathHelper.Lerp(0.75f, 0f, (Timer - fadeThreshold) / 30f) : 0.75f;
            return Color.Orange * opacity;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // Draw the wire
            List<Vector2> wirePoints = new List<Vector2>();
            if (Wire != null && Wire.Count > 0)
            {
                for (int i = 0; i < Wire.Count; i++)
                {
                    wirePoints.Add(Wire[i].position);
                }
            }
            PrimitiveRenderer.RenderTrail(wirePoints, new(WidthFunction, ColorFunction), 75);

            // Fist itself only draws if it hasn't hit
            // After hitting, the fist stops drawing, creating the illusion of the wire being left behind
            return Projectile.numHits == 0;
        }
    }
}
