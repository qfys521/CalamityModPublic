using System;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class NovaShot : ModProjectile, ILocalizedModType
    {
        public bool FirstFrameNoDraw = true;
        public new string LocalizationCategory => "Projectiles.Ranged";
        public int sineDir = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 20;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (sineDir == 0)
                sineDir = Main.rand.NextBool() ? -1 : 1;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            //Dust dust = Dust.NewDustPerfect(Projectile.Center, 107); // + Main.rand.NextVector2Circular(-3, 3)
            //dust.noGravity = true;
            //dust.scale = 0.5f;
            if (Projectile.timeLeft % 2 == 0 && Projectile.timeLeft < 290)
            {
                Particle Trail = new CustomSpark(Projectile.Center - Projectile.velocity * 0.5f, Projectile.velocity * 0.01f, "CalamityMod/Particles/BloomCircle", false, 7, 0.33f, ArcNovaDiffuser.mainColor * 0.8f, new Vector2(0.2f, 1.4f), true, true, shrinkSpeed: 0.4f, glowOpacity: 0.8f);
                GeneralParticleHandler.SpawnParticle(Trail);
            }
            if (Main.rand.NextBool(3))
            {
                float sine = (float)Math.Sin(Projectile.timeLeft * 0.45f / MathHelper.Pi);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2 * sineDir) * sine * 8, ModContent.DustType<SquashDust>(), Projectile.velocity * Main.rand.NextFloat(0.85f, 0.9f), 0, default, Main.rand.NextFloat(0.4f, 0.45f) * 3);
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(3) ? Color.Lime : ArcNovaDiffuser.mainColor;
                dust.fadeIn = Main.rand.NextBool(8) ? -0.4f : 1.75f;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return ArcNovaDiffuser.mainColor with { A = 0 };
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits == 0)
            {
                for (int i = 0; i <= 4; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.4f) * Main.rand.NextFloat(7f, 9f), 0, default, Main.rand.NextFloat(0.8f, 1.8f));
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool(3) ? Color.Lime : ArcNovaDiffuser.mainColor;
                    dust.fadeIn = 1.5f;
                    if (Main.rand.NextBool(3))
                    {
                        dust.scale = Main.rand.NextFloat(0.4f, 0.5f);
                        dust.fadeIn = -0.85f;
                        dust.velocity /= 2;
                    }
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.88f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.Terra, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(1.2f, 1.6f));
                dust.noGravity = false;
            }
        }
        public override bool? CanDamage() => base.CanDamage();
    }
}
