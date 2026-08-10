using System;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class PerditoSigilShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public Color baseColor = Color.White;
        public int sineDir = 1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 22;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 3;
            Projectile.tileCollide = false;
            Projectile.ArmorPenetration = 25;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            float lifeProgress = 1f - (Projectile.timeLeft / 22f);

            float fadeIn = Utils.GetLerpValue(0f, 0.25f, lifeProgress, true);
            float fadeOut = Utils.GetLerpValue(1f, 0.66f, lifeProgress, true);
            float fadeMult = fadeIn * fadeOut;

            // Store fade multiplier in ai[3]
            Projectile.localAI[0] = fadeMult;

            if (time == 0)
            {
                sineDir = Main.rand.NextBool() ? 1 : -1;
                Projectile.ai[1] = Main.rand.NextFloat(-0.06f, 0.06f); // curvature
            }

            float curvature = Projectile.ai[1];
            if (curvature != 0f)
                Projectile.velocity = Projectile.velocity.RotatedBy(curvature);

            Projectile.scale = MathHelper.Lerp(0.4f, 2.4f, fadeMult);
            baseColor = Color.LightGray * fadeMult;

            if (time > 13)
            {
                float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);
                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 16f;
                float scale = Main.rand.NextFloat(0.8f, 1.1f);
                if (Main.rand.NextBool(3))
                {
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + offset * sineDir, ModContent.DustType<VoidDust>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                    dust2.noGravity = true;
                    dust2.scale = scale;
                    dust2.color = baseColor;
                }
                if (Main.rand.NextBool(3))
                {
                    Dust dust3 = Dust.NewDustPerfect(Projectile.Center - offset * sineDir, ModContent.DustType<VoidDustInverted>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                    dust3.noGravity = true;
                    dust3.scale = scale;
                    dust3.color = baseColor;
                }
            }

            if (time > 13 && time < 34 && Projectile.ai[2] > 0)
            {
                Projectile.Center += Projectile.velocity.RotatedBy((Projectile.ai[2] == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2)) * 0.2f;
            }

            time++;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float fadeMult = Projectile.localAI[0];

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLineBloom");
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLine");

            // Colors dynamically fade out
            Color trailColor1 = (baseColor with { A = 0 }) * (0.35f * fadeMult);
            Color trailColor2 = Color.Black * fadeMult;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], trailColor1, 1, tex.Value);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], trailColor2, 1, tex2.Value, true, true);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, (Main.rand.NextBool() ? ModContent.DustType<VoidDustInverted>() : ModContent.DustType<VoidDust>()), (Projectile.velocity * 3).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.2f, 1f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.15f, 1.45f);
                dust.color = baseColor;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.97f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
    }
}
