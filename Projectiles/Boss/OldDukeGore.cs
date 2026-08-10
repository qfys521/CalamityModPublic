using CalamityMod.NPCs.OldDuke;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class OldDukeGore : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 420;
            Projectile.alpha = 255;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Lighting.AddLight((int)((Projectile.position.X + (Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (Projectile.height / 2)) / 16f), 0.5f, 0.4f, 0f);

            Projectile.alpha -= 50;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 15f)
                Projectile.velocity.Y += 0.1f;

            if (Projectile.velocity.Y > 12f)
                Projectile.velocity.Y = 12f;

            Projectile.tileCollide = Projectile.timeLeft < 300;

            Projectile.rotation += Projectile.velocity.X * 0.1f;

            MediumMistParticle mist2 = new MediumMistParticle(Projectile.Center, Projectile.velocity, OldDuke.GlowColor, Color.DarkSlateBlue, Main.rand.NextFloat(1f), 200f);
            mist2.AffectedByLight = true;

            GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Projectile.Center, (Projectile.velocity / 2) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)), Color.DarkRed, 20, Main.rand.NextFloat(0.2f, 1f), 0.2f, MathHelper.ToRadians(Main.rand.NextFloat(-2f, 2f)), affectedByLight: true));
            GeneralParticleHandler.SpawnParticle(mist2);

            int blood = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1f);
            Main.dust[blood].noGravity = true;
            Main.dust[blood].velocity *= 0f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 3; i++)
            {
                GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center + (Projectile.velocity * 2f), Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-20, 20))) * Main.rand.NextFloat(3f), true, 8, Main.rand.NextFloat(1f, 2f), Color.DarkRed.MultiplyRGBA(new Color(0.3f, 0.3f, 0.3f, 0.3f)), false, true));
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath12, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), true, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.DarkRed, false, true));
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
