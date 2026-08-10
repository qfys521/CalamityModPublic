using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class AngelicBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // Due to how far/fast this thing moves, it'd require way too many points for a smooth long trail using oldPos
        public List<Vector2> TrailPos = new List<Vector2>();
        public static int Lifetime = 300;
        public static int Fadetime = 30;

        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            // Effectively hitscan
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = Lifetime;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Begin fading once the laser is fully spawned in
            if (Projectile.ai[0] > 0f)
            {
                Projectile.Opacity = Utils.GetLerpValue(0f, Fadetime, Projectile.timeLeft, true);
                return;
            }
            else if (Projectile.timeLeft == 1)
            {
                Projectile.ai[0] = 1f;
                Projectile.MaxUpdates = 1;
                Projectile.timeLeft = Fadetime;
            }

            if (Projectile.FinalExtraUpdate() || Projectile.numUpdates % 30 == 29 || TrailPos == null)
            {
                // Initialize 10 points immediately
                if (TrailPos == null)
                {
                    TrailPos = new List<Vector2>(10);
                    for (int i = 0; i < 10; ++i)
                        TrailPos.Add(Projectile.Center);
                }

                TrailPos.Insert(0, Projectile.Center);

                while (TrailPos.Count > 10)
                    TrailPos.RemoveAt(TrailPos.Count - 1);
            }

            // Trail sparks in the laser's path
            if (Main.rand.NextBool())
            {
                Particle spark = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(10f * Projectile.scale, 10f * Projectile.scale), Projectile.velocity, false, 15, Main.rand.NextFloat(1f, 1.2f) * Projectile.scale, Color.White);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Main.rand.NextBool(4))
            {
                Color fireColor = Main.hslToRgb(Main.rand.NextFloat(0.08f, 0.13f) + 0.05f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f), 1f, 0.7f);
                Particle fire = new GlowOrbParticle(Projectile.Center, (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * (Main.rand.NextFloat(1f, 3f) + 3f * Projectile.scale), false, 9, Main.rand.NextFloat(1f, 1.2f) * Projectile.scale, fireColor);
                GeneralParticleHandler.SpawnParticle(fire);
            }
        }

        // Hitbox size does not normally scale with Projectile.scale for some reason so this is done manually
        public override void ModifyDamageHitbox(ref Rectangle hitbox) => hitbox.Inflate((int)(Projectile.width * (Projectile.scale - 1f)), (int)(Projectile.height * (Projectile.scale - 1f)));

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, ModContent.DustType<LightDust>(), (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(2f, 12f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 2.4f) * Projectile.scale;
                dust.color = Main.hslToRgb(Main.rand.NextFloat(0.033f, 0.167f), 1f, 0.7f);
                dust.noLightEmittance = true;
            }
        }

        public Color LaserColor(float completionRatio, Vector2 vertexPos) => Main.hslToRgb(0.08f + 0.05f * completionRatio + 0.05f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f), 1f, 0.7f) * 0.8f * Projectile.Opacity;
        public float LaserWidth(float completionRatio, Vector2 vertexPos) => 30f * Projectile.Opacity * Projectile.scale;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (TrailPos == null)
                return false;

            GameShaders.Misc["CalamityMod:Flame"].UseImage1("Images/Misc/Perlin");
            GameShaders.Misc["CalamityMod:Flame"].UseSaturation(0.5f);
            PrimitiveRenderer.RenderTrail(TrailPos, new(LaserWidth, LaserColor, shader: GameShaders.Misc["CalamityMod:Flame"]), 10);
            return false;
        }
    }
}
