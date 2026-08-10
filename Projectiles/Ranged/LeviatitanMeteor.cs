using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using Terraria.DataStructures;

namespace CalamityMod.Projectiles.Ranged
{
    public class LeviatitanMeteor : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public static int Lifetime = 600;
        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            if (Projectile.velocity.Y < 25f)
                Projectile.velocity.Y += 0.12f;
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            if (Projectile.timeLeft <= Lifetime - 6)
            {
                if (Main.rand.NextBool(3))
                {
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center + new Vector2(25, 25).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.3f) * Projectile.scale, (-Projectile.velocity * 0.2f).RotatedByRandom(0.2f) + (new Vector2(3, 3).RotatedByRandom(100)), Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.4f, 1.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                }
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedBrown, 0f, 0f, 100, default, Main.rand.NextFloat(0.75f, 1.2f));
                Main.dust[dust].velocity *= 0f;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;
            int smokeAmount = Main.rand.Next(8, 12 + 1);
            for (int i = 0; i < smokeAmount; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.2f, 2f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.3f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                SoundStyle explo = new("CalamityMod/Sounds/Item/MineralMortarExplode");
                SoundEngine.PlaySound(explo with { Volume = 0.9f }, Projectile.Center);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<LeviatitanExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 randVel = new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 0.7f);
                    Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, Color.Peru, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.9f, 2.3f), 0.4f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center, randVel * 0.8f, Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.4f, 1.3f), 120f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                }
                int debriAmount = Main.rand.Next(10, 15 + 1);
                for (int debriIndex = 0; debriIndex < debriAmount; debriIndex++)
                {
                    float angle = MathHelper.TwoPi / debriAmount * debriIndex;
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 8f);
                    Particle debri = new StoneDebrisParticle(Projectile.Center, velocity, Color.Lerp(Color.White, Color.LightGray, Main.rand.NextFloat()), Main.rand.NextFloat(0.4f, 0.6f), Main.rand.Next(30, 45 + 1), Main.rand.NextFloat(MathHelper.Pi));
                    GeneralParticleHandler.SpawnParticle(debri);
                }
                int mistAmount = Main.rand.Next(5, 8 + 1);
                for (int mistIndex = 0; mistIndex < mistAmount; mistIndex++)
                {
                    float angle = MathHelper.TwoPi / mistAmount * mistIndex;
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 15f);
                    Particle boomMist = new MediumMistParticle(Projectile.Center, velocity, Color.SaddleBrown * 0.8f, Color.Transparent, Main.rand.NextFloat(.6f, 1.4f), Main.rand.NextFloat(200f, 400f));
                    GeneralParticleHandler.SpawnParticle(boomMist);
                }
                Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Red, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), Projectile.width / 5000f, Projectile.width / 200f, 29, true, 0.7f);
                GeneralParticleHandler.SpawnParticle(blastRing2);
                Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.SaddleBrown * 0.8f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), Projectile.width / 5000f, Projectile.width / 300f, 29);
                GeneralParticleHandler.SpawnParticle(blastRing);
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
