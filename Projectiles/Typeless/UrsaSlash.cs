using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class UrsaSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float radius = 85f;
        public bool visuals = true;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)radius;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (visuals)
            {
                bool visible = Owner.Calamity().ursaSergeantVisual;
                Vector2 slashDir = new Vector2(13, 13).RotatedByRandom(100);
                Vector2 slashPos1 = Projectile.Center + slashDir.RotatedBy(MathHelper.ToRadians(90f) * 1.25f);
                Vector2 slashPos2 = Projectile.Center + slashDir.RotatedBy(MathHelper.ToRadians(-90f) * 1.25f);

                Owner.SetScreenshake(4f);
                for (int i = 0; i < 3; i++)
                {
                    Particle bigSpark = new GlowSparkParticle(Projectile.Center - slashDir * 6, slashDir * 0.65f, false, 19, 0.085f * (1 - i * 0.25f), Color.Coral * (visible ? 1 : 0.3f), new Vector2(1.9f, 1), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(bigSpark);
                    Particle spark1 = new GlowSparkParticle(slashPos1 - slashDir * 6, slashDir.RotatedBy(0.06f) * 0.65f, false, 19, 0.067f * (1 - i * 0.25f), Color.DarkTurquoise * (visible ? 1 : 0.3f), new Vector2(1.9f, 1), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark1);
                    Particle spark2 = new GlowSparkParticle(slashPos2 - slashDir * 6, slashDir.RotatedBy(-0.06f) * 0.65f, false, 19, 0.067f * (1 - i * 0.25f), Color.DarkTurquoise * (visible ? 1 : 0.3f), new Vector2(1.9f, 1), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                for (int i = 0; i <= 9; i++)
                {
                    int dustStyle = ModContent.DustType<SquashDust>();
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - slashDir * 6, dustStyle);
                    dust.scale = Main.rand.NextFloat(0.8f, 1.7f) * (visible ? 1 : 0.3f) * (Main.rand.NextBool(5) ? 1.5f : 1);
                    dust.velocity = slashDir.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 2);
                    dust.noGravity = true;
                    dust.color = Color.Coral;
                    dust.fadeIn = 0.5f;
                    if (!visible)
                    {
                        dust.noLight = true;
                        dust.noLightEmittance = true;
                    }
                    Dust dust2 = Dust.NewDustPerfect(slashPos1 - slashDir * 6, dustStyle);
                    dust2.scale = Main.rand.NextFloat(0.8f, 1.7f) * (visible ? 1 : 0.3f) * (Main.rand.NextBool(5) ? 1.5f : 1);
                    dust2.velocity = slashDir.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 2);
                    dust2.noGravity = true;
                    dust2.color = Color.DarkTurquoise;
                    dust2.fadeIn = 0.5f;
                    if (!visible)
                    {
                        dust2.noLight = true;
                        dust2.noLightEmittance = true;
                    }
                    Dust dust3 = Dust.NewDustPerfect(slashPos2 - slashDir * 6, dustStyle);
                    dust3.scale = Main.rand.NextFloat(0.8f, 1.7f) * (visible ? 1 : 0.3f) * (Main.rand.NextBool(5) ? 1.5f : 1);
                    dust3.velocity = slashDir.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 2);
                    dust3.noGravity = true;
                    dust3.color = Color.DarkTurquoise;
                    dust3.fadeIn = 0.5f;
                    if (!visible)
                    {
                        dust3.noLight = true;
                        dust3.noLightEmittance = true;
                    }
                }

                if (visible)
                {
                    SoundStyle sound = new("CalamityMod/Sounds/Item/AstralSlash", 3);
                    SoundEngine.PlaySound(sound with { Volume = 0.65f }, Projectile.Center);
                    SoundStyle sound2 = new("CalamityMod/Sounds/NPCHit/PerfLargeHit", 3);
                    SoundEngine.PlaySound(sound2 with { Volume = 0.85f, Pitch = Main.rand.NextFloat(0.4f, 0.5f) }, Projectile.Center);
                }
            }
            visuals = false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Buffs.DamageOverTime.AstralInfectionDebuff>(), 120);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.7f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox);
        public override bool? CanDamage() => base.CanDamage();
        public override bool? CanCutTiles() => false;
    }
}
