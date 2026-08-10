using System;
using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class PlasmaRifleShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // Due to how far/fast this thing moves, it'd require way too many points for a smooth long trail using oldPos
        public List<Vector2> TrailPos = new List<Vector2>();

        // Meanwhile, this is for the shorter trail at the head
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 5000;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.MaxUpdates = 10;
            Projectile.timeLeft = 30 * Projectile.MaxUpdates;
            Projectile.penetrate = -1; // Effectively no pierce -- this is just to prevent sudden trail cutting
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.numHits > 0)
            {
                Projectile.scale -= 0.02f;
                Projectile.Opacity -= 0.02f;
                if (Projectile.Opacity <= 0f)
                    Projectile.Kill();
            }
            else 
            {
                if (Projectile.FinalExtraUpdate() || TrailPos.Count < 15)
                    TrailPos.Add(Projectile.Center);

                if (Projectile.timeLeft < 50) // Starts exploding and fading by itself if it never hits anything
                    Explode();
            }
        }

        public override bool? CanDamage() => Projectile.numHits == 0 ? null : false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits == 0)
                Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.numHits == 0)
                Explode();
            return false;
        }

        public void Explode()
        {
            Projectile.numHits++; // Ensure this can only happen once
            Projectile.timeLeft = 50;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
            TrailPos.Add(Projectile.Center); // Final position update

            // Right click can not explode
            if (Projectile.ai[0] > 0f)
                return;

            SoundEngine.PlaySound(AnomalysNanogunMPFBBoom.MPFBExplosion, Projectile.Center);
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PlasmaRifleExplosion>(), (int)(Projectile.damage * 0.7f), Projectile.knockBack, Projectile.owner, ai1: 160f);

            for (int k = 0; k < 30; k++)
            {
                float intensity = Main.rand.NextFloat(0.1f, 1f);
                Color BaseCol = Color.Lerp(Color.Lime, Color.Yellow, intensity);
                Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(24f, 25f) - 20f * intensity);
                MediumMistParticle cloud = new MediumMistParticle(Projectile.Center, velocity, BaseCol, Color.Chartreuse, Main.rand.NextFloat(1f, 1.6f) + 2f * intensity, 210f, Main.rand.NextFloat(0.03f, -0.03f));
                GeneralParticleHandler.SpawnParticle(cloud);
            }
        }

        internal float HeadWidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * (Projectile.ai[0] > 0f ? 2f : 4f) * Utils.GetLerpValue(0.5f, 0.25f, MathF.Abs(0.5f - completionRatio), true);
        internal Color HeadColorFunction(float completionRatio, Vector2 vertexPos) => Color.LightSlateGray * Projectile.Opacity;
        internal float TrailWidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * (Projectile.ai[0] > 0f ? 2f : 4f);
        internal Color TrailColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lerp(Color.Chartreuse, Color.SlateGray, Utils.Remap(Projectile.Opacity, 0.5f, 1f, 0f, 0.8f)) * Projectile.Opacity * 0.3f;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (TrailPos == null)
                return false;

            PrimitiveRenderer.RenderTrail(TrailPos, new(TrailWidthFunction, TrailColorFunction), 30);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(HeadWidthFunction, HeadColorFunction, (_,_) => Projectile.Size * 0.5f), 12);
            return false;
        }
    }
}
