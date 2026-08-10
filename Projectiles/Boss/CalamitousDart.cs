using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class CalamitousDart : ModProjectile, ILocalizedModType
    {

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            if (Projectile.extraUpdates == 0)
            {
                Projectile.extraUpdates = 1;
                Projectile.velocity *= 0.5f;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 10)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            int target = Player.FindClosest(Projectile.Center, 1, 1);

            float targetDist;
            if (target != -1 && !Main.player[target].dead && Main.player[target].active && Main.player[target] != null)
                targetDist = Vector2.Distance(Main.player[target].Center, Projectile.Center);
            else
                targetDist = 1000;

            Lighting.AddLight(Projectile.Center, 0.9f * Projectile.Opacity, 0f, 0f);

            if (targetDist < 1400f && Projectile.ai[1] == 2f)
            {
                // Spawn in a helix-style pattern
                float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 16f;

                SparkParticle orb = new(Projectile.Center + offset, -Projectile.velocity * 0.05f, false, 8, 0.8f, Main.rand.NextBool() ? Color.Red : Color.Lerp(Color.Red, Color.Magenta, 0.5f));
                GeneralParticleHandler.SpawnParticle(orb);

                SparkParticle orb2 = new(Projectile.Center - offset, -Projectile.velocity * 0.05f, false, 8, 0.8f, Main.rand.NextBool() ? Color.Red : Color.Lerp(Color.Red, Color.Magenta, 0.5f));
                GeneralParticleHandler.SpawnParticle(orb2);
            }

            if (Projectile.timeLeft < 51)
                Projectile.Opacity -= 0.02f;


            if (Projectile.velocity.Length() < (Projectile.ai[2] == 2f ? 9.5f : 5.3f)) // Hellblasts in phase 4 have faster accel and higher max velocity
                Projectile.velocity *= Projectile.ai[2] == 2f ? 1.033f : 1.0125f;

            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(-Projectile.velocity.Y, -Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = 1;
            var dir = Projectile.rotation.ToRotationVector2();
            if (Projectile.spriteDirection == -1)
                dir = dir.RotatedBy(MathHelper.Pi);
            var spot = Vector2.Lerp(Projectile.Center + dir.RotatedBy(MathHelper.PiOver2) * 26, Projectile.Center + dir.RotatedBy(-MathHelper.PiOver2) * 26, Main.rand.NextFloat());
            for (var i = 0; i < 1; i++)
            {
                var p = CalamitasMetaball.SpawnParticle(Projectile.Center + Projectile.velocity * 2, Vector2.Zero, 40);
                p.rotation = Projectile.rotation + MathHelper.PiOver2;
                p.TextureToUse = ModContent.Request<Texture2D>("CalamityMod/Particles/PointParticle").Value;
                p.SizeScaling = 0.65f;
                p = CalamitasMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(3, 3), 24f);
                p.SizeScaling = 0.8f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.timeLeft >= 51;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.timeLeft < 51)
                return;

            if (Main.zenithWorld)
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180);
            else
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }

        public override void OnKill(int timeLeft)
        {
            for (int dust = 0; dust <= 5; dust++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f);
            }
        }
    }
}
