using System;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class VengefulSunBeam : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.MaxUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 0; //has a custom iframe counter
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        }

        public override void AI()
        {
            if (Projectile.ai[0] > 1)
            {
                Projectile.ai[0]--;
                Projectile.timeLeft = 120;
            }
            if (Projectile.ai[0] > 0)
            {
                var target = Projectile.FindTargetWithinRange(640);
                if (target != null)
                {
                    Projectile.Calamity().HomingTarget = target.whoAmI;
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(Projectile.DirectionTo(target.Center).ToRotation(), 0.5f*(1-Projectile.ai[0]/120f)).ToRotationVector2() * Projectile.velocity.Length();
                }
            }
            if (Projectile.damage <= 0)
            {
                Projectile.ai[0] = 0;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 4;
                Projectile.timeLeft = (int)MathHelper.Min(8, Projectile.timeLeft);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 6f;
                float dustScale = Main.rand.NextFloat(3f, 5f);
                Color dustColor = Color.Lerp(Color.OrangeRed, Color.Gold, Main.rand.NextFloat(0.5f, 1f));

                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, dustColor, dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittance = true;
            }
            if (Projectile.ai[0] > 0)
                return;
            for (var i = -1; i <= 1; i+=2)
            {
                var randomVel = Projectile.velocity.RotatedByRandom(MathHelper.TwoPi);
                var p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),Projectile.Center- randomVel * 3, randomVel, Type,Projectile.damage,Projectile.knockBack,Projectile.owner, 120, Projectile.ai[1]);
                p.penetrate = 1;
                p.scale = 0.75f;
            }
            Projectile.damage = 0;
            Projectile.timeLeft = 10;
            Projectile.netUpdate = true;
        }

        public override bool? CanDamage()
        {
            return Projectile.ai[0] <= 90;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => false;

        //Trail based on Fryzahh's work on Faith Incinerator
        public float FireWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = 38f * Projectile.scale;
            float curveRatio = 0.2f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);
            if (completion < curveRatio)
                width = MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);

            float pulseInterpolant = MathF.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return (width + additionalPulseWidth) * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = Color.Lerp(Color.Yellow, Color.OrangeRed, Projectile.ai[1]);
            if (Projectile.ai[1] >= 1)
                mainColor = Color.LightBlue;
            mainColor *= 1.3f;
            Color endColor = Color.Lerp(mainColor, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(mainColor, endColor, completion);
        }

        public float FireCoreWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 24;
            float curveRatio = 0.25f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireCoreColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = Color.Black;
            mainColor = Color.Lerp(mainColor, Color.Gold, 1 - Projectile.ai[1]);
            Color tipColor = Color.Lerp(mainColor, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            Color fullBodyColor = Color.Lerp(mainColor, tipColor, completion);
            return fullBodyColor;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(FireWidthFunction, FireColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length + 32);
            Vector2[] fireCoreLength = Projectile.oldPos.Take(8).ToArray();
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(fireCoreLength, new(FireCoreWidthFunction, FireCoreColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), fireCoreLength.Length + 24);
        }
    }
}
