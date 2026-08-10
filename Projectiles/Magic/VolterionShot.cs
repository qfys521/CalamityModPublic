using System;
using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class VolterionShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // Due to how far/fast this thing moves, it'd require way too many points for a smooth long trail using oldPos
        public List<Vector2> TrailPos = new List<Vector2>();
        public const int TrailLength = 50;

        public ref float OrbType => ref Projectile.ai[0];

        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 5000;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.MaxUpdates = 15;
            Projectile.timeLeft = 24 * Projectile.MaxUpdates;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.numHits > 0)
            {
                Projectile.scale -= 0.005f;
                Projectile.Opacity -= 0.005f;
                if (Projectile.Opacity <= 0f)
                    Projectile.Kill();
            }
            else 
            {
                // Update a bit more frequently than the update count for better point distribution
                if (Projectile.FinalExtraUpdate() || Projectile.numUpdates % 3 == 2 || TrailPos == null)
                {
                    // Initialize 15 points immediately
                    if (TrailPos == null)
                    {
                        TrailPos = new List<Vector2>(15);
                        for (int i = 0; i < 15; ++i)
                            TrailPos.Add(Projectile.Center);
                    }

                    // Add some random value for the natural lightning look
                    Vector2 randOffset = (Vector2.UnitY * Main.rand.NextFloat(-24f, 24f)).RotatedBy(Projectile.rotation);
                    TrailPos.Insert(0, Projectile.Center + randOffset);

                    while (TrailPos.Count > TrailLength)
                        TrailPos.RemoveAt(TrailPos.Count - 1);
                }

                // Randomly spawn glowing bolts outwards
                if (Main.rand.NextBool(6))
                {
                    BoltParticle bolt = new BoltParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(36f)), false, 10, 0.3f, TrailColorFunction(0f, Vector2.Zero), Vector2.One, true);
                    GeneralParticleHandler.SpawnParticle(bolt);
                }

                if (Projectile.timeLeft < 50) // Starts exploding and fading by itself if it never hits anything
                    Decay();
            }
        }

        public override bool? CanDamage() => Projectile.numHits == 0 ? null : false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 60);
            if (Projectile.numHits == 0)
                Decay();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.numHits == 0)
                Decay();
            return false;
        }

        public void Decay()
        {
            Projectile.numHits++; // Ensure this can only happen once
            Projectile.timeLeft = 200;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;

            // Final position update
            TrailPos.Insert(0, Projectile.Center);

            while (TrailPos.Count > TrailLength)
                TrailPos.RemoveAt(TrailPos.Count - 1);

            bool secondary = OrbType > 0f;
            if (Projectile.owner == Main.myPlayer && !secondary)
            {
                float rotOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 velocity = Vector2.UnitX.RotatedBy(rotOffset + MathHelper.TwoPi * 0.2f * (i + Main.rand.NextFloat(-0.4f, 0.4f))) * (Main.rand.NextFloat(40f, 45f));
                    Projectile orb = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<VolterionOrb>(), Projectile.damage, Projectile.knockBack, Projectile.owner, i);
                    orb.rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                }
            }

            int boltCount = secondary ? 6 : 10;
            for (int i = 0; i < boltCount; i++)
            {
                Color color = OrbType > 0f ? VolterionOrb.GetColor(OrbType - 1f) : (Main.rand.NextBool() ? Color.Cyan : Color.Orchid);
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * 0.2f * (i + Main.rand.NextFloat(-0.4f, 0.4f))) * (Main.rand.NextFloat(12f, 15f));
                BoltParticle bolt = new BoltParticle(Projectile.Center, velocity, false, 18, Main.rand.NextFloat(0.4f, 0.6f), color, new Vector2(0.6f, 1f), true);
                GeneralParticleHandler.SpawnParticle(bolt);
            }
            for (int k = 0; k < 7; k++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(8f, 14f));
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, velocity);
                spark.noLight = true;
                spark.color = Main.rand.NextBool() ? Color.Cyan : Color.Orchid;
            }
        }

        internal float TrailWidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * 15f * Utils.GetLerpValue(0.5f, 0.4f, MathF.Abs(0.5f - completionRatio), true);
        internal Color TrailColorFunction(float completionRatio, Vector2 vertexPos) => OrbType > 0f ? VolterionOrb.GetColor(OrbType - 1f) : Color.Lerp(new Color(51, 197, 255), new Color(143, 51, 255), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 20f)) * Projectile.Opacity;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (TrailPos == null)
                return false;

            GameShaders.Misc["CalamityMod:HeavenlyGaleLightningArc"].UseImage1("Images/Misc/Perlin");
            GameShaders.Misc["CalamityMod:HeavenlyGaleLightningArc"].Apply();
            PrimitiveRenderer.RenderTrail(TrailPos, new(TrailWidthFunction, TrailColorFunction, smoothen: false, shader: GameShaders.Misc["CalamityMod:HeavenlyGaleLightningArc"]), TrailLength);
            return false;
        }
    }
}
