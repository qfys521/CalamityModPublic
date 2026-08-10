using System.Collections.Generic;
using System.IO;
using CalamityMod.Items.Weapons.Melee;
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
    public class SwordsmithsPrideMonolith : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Melee/TrueBiomeBlade_SwordsmithsPrideMonolith";
        public Player Owner => Main.player[Projectile.owner];
        public float Timer => (100f - Projectile.timeLeft) / 100f;
        public ref float Variant => ref Projectile.ai[0];
        public ref float Size => ref Projectile.ai[1];
        public float Scale; // The scale multiplier based on the target's size
        public Vector2 OriginDirection; // The direction of the original strike
        public float Facing; // The direction of the original strike
        public NPC Target; // The NPC to stick on top of
        public const float BaseWidth = 90f;
        public const float BaseHeight = 420f;

        public CurveSegment StaySmall = new CurveSegment(EasingType.Linear, 0f, 0.2f, 0f);
        public CurveSegment GoBig = new CurveSegment(EasingType.SineOut, 0.25f, 0.2f, 1f);
        public CurveSegment GoNormal = new CurveSegment(EasingType.CircIn, 0.4f, 1.2f, -0.2f);
        public CurveSegment StayNormal = new CurveSegment(EasingType.Linear, 0.5f, 1f, 0f);

        internal float Width() => PiecewiseAnimation(Timer, new CurveSegment[] { StaySmall, GoBig, GoNormal, StayNormal }) * BaseWidth * Size;

        public CurveSegment Anticipate = new CurveSegment(EasingType.CircIn, 0f, 0f, 0.15f);
        public CurveSegment Overextend = new CurveSegment(EasingType.SineOut, 0.2f, 0.15f, 1f);
        public CurveSegment Unextend = new CurveSegment(EasingType.CircIn, 0.25f, 1.15f, -0.15f);
        public CurveSegment Hold = new CurveSegment(EasingType.ExpOut, 0.70f, 1f, -0.1f);
        internal float Height() => PiecewiseAnimation(Timer, new CurveSegment[] { Anticipate, Overextend, Unextend, Hold }) * BaseHeight * Size;

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
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.hide = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Height()), Width(), ref collisionPoint);
        }


        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.velocity = Vector2.Zero;
            }

            if (!Target.active)
            {
                Projectile.Kill();
                return;
            }
            else
                Projectile.Center = Target.Center;

            if (Projectile.timeLeft == 100)
            {
                if (Size >= 1) //Big monoliths create sparkles even before sprouting up
                {

                    for (int i = 0; i < 20; i++)
                    {
                        Particle Sparkle = new CritSpark(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(7.5f, 20f), Color.White, Main.rand.NextBool() ? Color.MediumTurquoise : Color.DarkOrange, 0.1f + Main.rand.NextFloat(0f, 1.5f), 20 + Main.rand.Next(30), 1, 3f);
                        GeneralParticleHandler.SpawnParticle(Sparkle);
                    }
                }

                if (Projectile.ai[2] > 1f)
                {
                    if (Facing == 0)
                    {
                        SideSprouts(-1f, Size * 0.8f);
                        SideSprouts(1f, Size * 0.8f);
                    }
                    else
                    {
                        SideSprouts(Facing, Size * 0.8f);
                    }
                }
                
            }

            if (Projectile.timeLeft == 80)
            {
                Vector2 particleDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);

                for (int i = 0; i < 8; i++)
                {
                    Vector2 hitPositionDisplace = particleDirection.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0f, 10f);
                    Vector2 flyDirection = particleDirection.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)) * Main.rand.NextFloat(5f, 15f);
                    Particle smoke = new SmallSmokeParticle(Projectile.Center + hitPositionDisplace, flyDirection, Color.Lerp(Color.DarkOrange, Color.MediumTurquoise, Main.rand.NextFloat()), new Color(130, 130, 130), Main.rand.NextFloat(2.8f, 3.6f) * Size, 165 - Main.rand.Next(30), 0.1f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                for (int i = 0; i < 6; i++)
                {
                    Vector2 hitPositionDisplace = particleDirection.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(10f, 30f);
                    Vector2 flyDirection = particleDirection.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4));

                    Particle Rock = new StoneDebrisParticle(Projectile.Center + hitPositionDisplace * 3, flyDirection * Main.rand.NextFloat(3f, 6f), Color.Lerp(Color.DarkSlateBlue, Color.LightSlateGray, Main.rand.NextFloat()), (1f + Main.rand.NextFloat(0f, 2.4f)) * Size, 30 + Main.rand.Next(50), 0.1f);
                    GeneralParticleHandler.SpawnParticle(Rock);
                }
            }
        }

        public void SideSprouts(float facing, float projSize)
        {
            Vector2 monolithRotation = OriginDirection.RotatedBy(MathHelper.Pi / 9.5f * facing);
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, monolithRotation, ProjectileType<SwordsmithsPrideMonolith>(), Projectile.damage, 10f, Owner.whoAmI, Main.rand.Next(4), projSize, Projectile.ai[2] - 1f);
            if (proj.ModProjectile is SwordsmithsPrideMonolith monolith)
            {
                monolith.Scale = Scale;
                monolith.OriginDirection = monolithRotation;
                monolith.Facing = facing;
                monolith.Target = Target;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Owner.HeldItem.ModItem is OmegaBiomeBlade sword && Main.rand.NextFloat() <= OmegaBiomeBlade.WhirlwindAttunement_MonolithProc)
                sword.OnHitProc = true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Request<Texture2D>("CalamityMod/Projectiles/Melee/TrueBiomeBlade_SwordsmithsPrideMonolith").Value;

            Vector2 Shake = Projectile.timeLeft < 70 ? Vector2.Zero : Vector2.One.RotatedByRandom(MathHelper.TwoPi) * (70 - Projectile.timeLeft / 30f) * 0.05f;

            float drawAngle = Projectile.rotation;
            Rectangle frame = new Rectangle(0 + (int)Variant * 94, 0, 94, 420);

            Vector2 drawScale = new Vector2(Width() / BaseWidth, Height() / BaseHeight) * Scale;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 26f * Scale;
            Vector2 drawOrigin = new Vector2(frame.Width / 2f, frame.Height);

            float opacity = MathHelper.Clamp(1f - ((Timer - 0.85f) / 0.15f), 0f, 1f);

            Main.EntitySpriteDraw(tex, drawPosition + Shake, frame, lightColor * opacity, drawAngle, drawOrigin, drawScale, 0f, 0);

            return false;
        }
        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Request<Texture2D>("CalamityMod/Projectiles/Melee/MendedBiomeBlade_HeavensMonolith_Glow").Value;

            float drawAngle = Projectile.rotation;
            Rectangle frame = new Rectangle(0 + (int)Variant * 94, 0, 94, 420);

            Vector2 drawScale = new Vector2(Width() / BaseWidth, Height() / BaseHeight) * Scale;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 26f * Scale;
            Vector2 drawOrigin = new Vector2(frame.Width / 2f, frame.Height);

            float opacity = MathHelper.Clamp(1f - ((Timer - 0.85f) / 0.15f), 0f, 1f);

            Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * opacity, drawAngle, drawOrigin, drawScale, 0f, 0);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Facing);
            writer.WriteVector2(OriginDirection);

        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Facing = reader.ReadSingle();
            OriginDirection = reader.ReadVector2();
        }
    }
}
