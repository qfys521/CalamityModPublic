using System.Collections.Generic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Melee
{
    public class HeavensMonolith : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Melee/MendedBiomeBlade_HeavensMonolith";
        public Player Owner => Main.player[Projectile.owner];
        public NPC Target => Main.npc[(int)Projectile.ai[2]];
        public float Timer => (100f - Projectile.timeLeft) / 100f;
        public ref float Variant => ref Projectile.ai[0];
        public ref float Scale => ref Projectile.ai[1];
        public const float BaseWidth = 90f;
        public const float BaseHeight = 420f;

        public CurveSegment StaySmall = new CurveSegment(EasingType.Linear, 0f, 0.2f, 0f);
        public CurveSegment GoBig = new CurveSegment(EasingType.SineOut, 0.25f, 0.2f, 1f);
        public CurveSegment GoNormal = new CurveSegment(EasingType.CircIn, 0.4f, 1.2f, -0.2f);
        public CurveSegment StayNormal = new CurveSegment(EasingType.Linear, 0.5f, 1f, 0f);

        internal float Width() => PiecewiseAnimation(Timer, new CurveSegment[] { StaySmall, GoBig, GoNormal, StayNormal }) * BaseWidth;

        public CurveSegment Anticipate = new CurveSegment(EasingType.CircIn, 0f, 0f, 0.15f);
        public CurveSegment Overextend = new CurveSegment(EasingType.SineOut, 0.2f, 0.15f, 1f);
        public CurveSegment Unextend = new CurveSegment(EasingType.CircIn, 0.25f, 1.15f, -0.15f);
        public CurveSegment Hold = new CurveSegment(EasingType.ExpOut, 0.70f, 1f, -0.1f);
        internal float Height() => PiecewiseAnimation(Timer, new CurveSegment[] { Anticipate, Overextend, Unextend, Hold }) * BaseHeight;

        public override void SetDefaults()
        {
            Projectile.drawLayer = Terraria.ID.ProjectileDrawLayerID.BehindNPCsAndTiles;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = Projectile.height = 70;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 102;
            Projectile.scale = Scale;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
            Projectile.hide = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Height()), Width(), ref collisionPoint);
        }


        public override void AI()
        {
            if (Projectile.timeLeft == 100)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.velocity = Vector2.Zero;

                for (int i = 0; i < 20; i++)
                {
                    Particle Sparkle = new CritSpark(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(7.5f, 20f), Color.White, Main.rand.NextBool() ? Color.MediumTurquoise : Color.DarkOrange, 0.1f + Main.rand.NextFloat(0f, 1.5f), 20 + Main.rand.Next(30), 1, 3f);
                    GeneralParticleHandler.SpawnParticle(Sparkle);
                }
            }

            // Constantly follow its target
            // Die if its target is no longer active
            if (!Target.active)
            {
                Projectile.Kill();
                return;
            }
            else
                Projectile.Center = Target.Center;

            if (Projectile.timeLeft == 80)
            {
                Vector2 particleDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);

                for (int i = 0; i < 8; i++)
                {
                    Vector2 hitPositionDisplace = particleDirection.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0f, 10f);
                    Vector2 flyDirection = particleDirection.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)) * Main.rand.NextFloat(5f, 15f);
                    Particle smoke = new SmallSmokeParticle(Projectile.Center + hitPositionDisplace, flyDirection, Color.Lerp(Color.DarkOrange, Color.MediumTurquoise, Main.rand.NextFloat()), new Color(130, 130, 130), Main.rand.NextFloat(2.8f, 3.6f), 165 - Main.rand.Next(30), 0.1f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                for (int i = 0; i < 6; i++)
                {
                    Vector2 hitPositionDisplace = particleDirection.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(10f, 30f);
                    Vector2 flyDirection = particleDirection.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4));

                    Particle Rock = new StoneDebrisParticle(Projectile.Center + hitPositionDisplace * 3, flyDirection * Main.rand.NextFloat(3f, 6f), Color.Lerp(Color.DarkSlateBlue, Color.LightSlateGray, Main.rand.NextFloat()), (1f + Main.rand.NextFloat(0f, 2.4f)), 30 + Main.rand.Next(50), 0.1f);
                    GeneralParticleHandler.SpawnParticle(Rock);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Request<Texture2D>("CalamityMod/Projectiles/Melee/MendedBiomeBlade_HeavensMonolith").Value;

            float drawAngle = Projectile.rotation;
            Rectangle frame = new Rectangle(0 + (int)Variant * 94, 0, 94, 420);

            Vector2 drawScale = new Vector2(Width() / BaseWidth, Height() / BaseHeight) * Scale;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 26f;
            Vector2 drawOrigin = new Vector2(frame.Width / 2f, frame.Height);

            float opacity = MathHelper.Clamp(1f - ((Timer - 0.85f) / 0.15f), 0f, 1f);

            Main.EntitySpriteDraw(tex, drawPosition, frame, lightColor * opacity, drawAngle, drawOrigin, drawScale, 0f, 0);

            return false;
        }
        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Request<Texture2D>("CalamityMod/Projectiles/Melee/MendedBiomeBlade_HeavensMonolith_Glow").Value;

            float drawAngle = Projectile.rotation;
            Rectangle frame = new Rectangle(0 + (int)Variant * 94, 0, 94, 420);

            Vector2 drawScale = new Vector2(Width() / BaseWidth, Height() / BaseHeight) * Scale;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 26f;
            Vector2 drawOrigin = new Vector2(frame.Width / 2f, frame.Height);

            float opacity = MathHelper.Clamp(1f - ((Timer - 0.85f) / 0.15f), 0f, 1f);

            Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * opacity, drawAngle, drawOrigin, drawScale, 0f, 0);
        }
    }
}
