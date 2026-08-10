using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class StellarContemptEcho : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public static readonly SoundStyle SlamHamSound = new("CalamityMod/Sounds/Item/StellarContemptImpact") { Volume = 0.6f };
        public static readonly SoundStyle Kunk = new("CalamityMod/Sounds/Item/TF2PanHit") { Volume = 1.1f };
        public float rotatehammer = 35f;
        public int ColorAlpha = 225;
        public float speed = 0f;
        public NPC targeted;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 76;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            rotatehammer--;
            Projectile.rotation += MathHelper.ToRadians(rotatehammer) * Projectile.direction;

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] < 42f)
            {
                Projectile.velocity *= 0.95f;
                Projectile.rotation += MathHelper.ToRadians(Projectile.ai[0] * 0.5f) * Projectile.localAI[0];
            }
            else if (Projectile.ai[0] >= 42f)
            {
                Projectile.extraUpdates = 5;
                if (Projectile.ai[1] != -5)
                    targeted = Main.npc[(int)Projectile.ai[1]];
                if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active)
                {
                    Projectile.ai[1] = -5;
                    targeted = Projectile.Center.ClosestNPCAt(2000);
                }
                if (targeted != null)
                {
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.65f, 25, 0.98f);
                }
                else
                    Projectile.Kill();

                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = new Vector2(7, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                    Vector2 velOffset = new Vector2(3, 0).RotatedBy(offset.ToRotation());
                    Dust dust = Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.Center.Y) + offset, ModContent.DustType<LightDust>(), new Vector2(Projectile.velocity.X * 0.5f + velOffset.X, Projectile.velocity.Y * 0.5f + velOffset.Y));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
                    dust.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
                }
            }
            if (Projectile.ai[0] == 42f && targeted != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, ((targeted.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 25).RotatedByRandom(0.6f) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.9f);
                    dust.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = new Vector2(0, -30).RotatedBy(-MathHelper.PiOver4 * 0.5f).RotatedBy(Projectile.rotation);
                Vector2 velOffset = new Vector2(0, -5).RotatedBy(-MathHelper.PiOver4).RotatedBy(Projectile.rotation) * Main.rand.NextFloat(0.5f, 1f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset + Main.rand.NextVector2Circular(6, 6), Main.rand.NextBool(3) ? 278 : ModContent.DustType<LightDust>(), velOffset);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f);
                dust.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (targeted != null && target != targeted)
                modifiers.SourceDamage *= 0.3f;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] <= 42f)
                return false;

            return null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Nightwither>(), 180);
            if (target == targeted)
                Projectile.Kill();
        }

        public override bool PreKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];

            // This is what we call fucking IMPACT (2).
            Main.player[Projectile.owner].SetScreenshake(7f);

            if (Main.zenithWorld)
                SoundEngine.PlaySound(Kunk, Projectile.Center);
            else
                SoundEngine.PlaySound(SlamHamSound, Projectile.Center);

            float numberOfDusts = 156f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                float intensity = Main.rand.NextFloat(0.2f, 1f);
                Vector2 offset = new Vector2(30f, 5.8f).RotatedBy(rot);
                Vector2 velOffset = new Vector2(40.8f, 10.5f).RotatedBy(rot);
                if (i % 2 == 0)
                {
                    Particle orb = new CustomSpark(Projectile.Center + offset, velOffset * intensity * 0.7f, "CalamityMod/Particles/Sparkle", false, (int)(40 * intensity), intensity, Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise, new Vector2(1f, 2f), true, true);
                    GeneralParticleHandler.SpawnParticle(orb);
                }
                else
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, ModContent.DustType<LightDust>(), velOffset);
                    dust.noGravity = true;
                    dust.velocity = velOffset * intensity;
                    dust.scale = Main.rand.NextFloat(2.1f, 2.8f) * intensity;
                    dust.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
                }
            }

            for (int i = 0; i < 40; ++i)
            {
                int dustID = ModContent.DustType<LightDust>();
                // Choose a random speed and angle for the dust.
                float dustSpeed = 45;
                float intensity = Main.rand.NextFloat(0.5f, 1.5f);
                Vector2 dustVel = new Vector2(dustSpeed, 0.0f).RotatedBy(Projectile.velocity.ToRotation());
                dustVel = dustVel.RotatedByRandom((1.5f - intensity) * 0.4f) * intensity;


                // Pick a size for the dust particle.
                float scale = Main.rand.NextFloat(2.8f, 3.5f) - intensity;

                // Actually spawn the dust.
                Dust idx = Dust.NewDustPerfect(Projectile.Center, dustID, dustVel, 0, default, scale);
                idx.noGravity = true;
                idx.position = Projectile.Center;
                idx.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
            }

            Particle pulse3 = new GlowSparkParticle(Projectile.Center + Projectile.velocity, Projectile.velocity * 2, false, 12, 0.17f, Color.Turquoise, new Vector2(2.5f, 0.7f), true, true, 0.85f);
            GeneralParticleHandler.SpawnParticle(pulse3);

            Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Turquoise, "CalamityMod/Particles/BloomRing", new Vector2(0.7f, 1), Projectile.velocity.ToRotation(), 0f, 3f, 25);
            GeneralParticleHandler.SpawnParticle(bolt2);
            Particle bolt3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.PaleTurquoise, "CalamityMod/Particles/BloomRing", new Vector2(0.4f, 1), Projectile.velocity.ToRotation(), 0f, 4f, 25);
            GeneralParticleHandler.SpawnParticle(bolt3);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0f, ModContent.ProjectileType<StellarContemptBlast>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0f);

            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.Turquoise with { A = 0 } * 0.5f, 1, texture, true, true);

            Projectile.DrawProjectileWithBackglow(Color.Turquoise with { A = 0 }, Color.White, 12f * Utils.GetLerpValue(0, 42, Projectile.ai[0], true) , texture);
            return true;
        }
    }
}
