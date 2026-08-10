using System;
using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackLightspeedCarveSlashVisual : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        #region params
        private const int VisualLifetime = 80;
        private const int GrowTime = 3;
        private const int FadeInTime = 1;
        private const int FadeOutTime = 5;
        private const int SlashDelay = 2;
        private const int HoldTime = 5;
        private const float FlashIntensity = 1.8f;
        private const float FlashDuration = 0.15f;
        private const float RangeMultiplier = 2.2f;
        private const float MinWidthStandard = 14f;
        private const float MaxWidthStandard = 46f;
        private const float MinWidthPerfect = 28f;
        private const float MaxWidthPerfect = 62f;

        private const float RevealSoftEdge = 0.25f;
        private const float RevealOvershoot = 0.22f;
        private const float RetreatSoftEdge = 0.18f;
        private const float RetreatOvershoot = 0.18f;
        private const int SlashResolution = 12;
        private const float LightningAmplitude = 2.5f;
        private const float LightningAmplitudePerfect = 3.5f;
        private const int LightningSubdivisions = 3;
        private const float LightningDecay = 0.45f;
        private const float LightningDriftSpeed = 4f;
        private const float MaxDepth = 40f;
        private const float DepthWobbleAmount = 8f;
        private const float DepthWobbleSpeed = 6f;
        private const float BreathingAmount = 0.06f;
        private const float BreathingSpeed = 9f;
        private const float CurveStrength = 0.12f;
        private const float CurveStrengthPerfect = 0.18f;
        private const int EchoCount = 1;
        private const float EchoOpacityFalloff = 0.4f;
        private const float EchoWidthFalloff = 0.75f;
        private const float EchoTimeLag = 4.5f;
        private const float GiantSlashLengthMult = 3.4f;
        private const float GiantSlashWidth = 120f;
        private const int GiantSlashGrowTime = 3;
        private const int GiantSlashHoldTime = 5;
        private const int GiantSlashFadeTime = 12;
        private const int GiantSlashDelay = 4;
        private const int GiantSlashStagger = 4;
        private const float GiantSlashScreenShake = 0f;
        private const float GiantSlashImpactFlash = 2.2f;
        private const float GiantSlashImpactFlashDuration = 3f;
        #endregion

        private static readonly BlendState GiantSlashDarkenBlend = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };

        private int GiantSlashCount => 1;
        // 13FEB2026: Ozzatron: previous hash function for allowing multiple visual giant slashes preserved
        /*
        {
            get
            {
                float seed = Projectile.identity * 1.913f;
                return Hash(seed * 3.91f) > 0.5f ? 2 : 1;
            }
        }
        */

        internal bool IsPerfect => Projectile.ai[0] == 1f;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = VisualLifetime;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            if (IsPerfect)
            {
                float age = VisualLifetime - Projectile.timeLeft;
                int count = GiantSlashCount;
                for (int g = 0; g < count; g++)
                {
                    float giantAge = GetGiantAge(age, g);
                    if (giantAge >= 0f && giantAge < 1f)
                    {
                        Player owner = Main.player[Projectile.owner];
                        owner.SetScreenshake(GiantSlashScreenShake);
                    }
                }
            }
        }
        private static float Hash(float x)
        {
            // ty shadertoy :p
            float s = MathF.Sin(x * 127.1f) * 43758.5453f;
            return s - MathF.Floor(s);
        }

        private List<Vector3> BuildSlashPath(
            Vector2 center,
            Vector2 direction,
            float halfLength,
            float curveAmount,
            float slashSeed,
            float growProgress,
            float time)
        {
            var path = new List<Vector3>(SlashResolution);
            Vector2 perp = direction.RotatedBy(MathHelper.PiOver2);
            bool valid = true;

            for (int p = 0; p < SlashResolution; p++)
            {
                float t = (float)p / (SlashResolution - 1);
                float s = t * 2f - 1f;
                Vector2 basePos = center + direction * s * halfLength;

                float curveBulge = (1f - s * s) * curveAmount * halfLength;
                basePos += perp * curveBulge;

                float lightning = 0f;
                float amplitude = IsPerfect ? LightningAmplitudePerfect : LightningAmplitude;
                float freq = 1f;
                for (int oct = 0; oct < LightningSubdivisions; oct++)
                {
                    float noiseInput = slashSeed * 13.7f + s * freq * 1.8f + oct * 5.3f + time * LightningDriftSpeed * (1f + oct * 0.3f);
                    lightning += (Hash(noiseInput) * 2f - 1f) * amplitude;
                    amplitude *= LightningDecay;
                    freq *= 1.8f;
                }

                float envelopeWindow = 1f - s * s;
                lightning *= envelopeWindow * growProgress;

                basePos += perp * lightning;

                float depthWobble = MathF.Sin(time * DepthWobbleSpeed + slashSeed * 4.1f + s * 2f) * DepthWobbleAmount;
                float z = (MathF.Sin(s * MathHelper.Pi + slashSeed * 2.9f) * MaxDepth + depthWobble) * (0.5f + 0.5f * growProgress);

                Vector2 screenPos = basePos - Main.screenPosition;

                if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y) || float.IsNaN(z) ||
                    float.IsInfinity(screenPos.X) || float.IsInfinity(screenPos.Y) || float.IsInfinity(z))
                {
                    valid = false;
                    break;
                }

                path.Add(new Vector3(screenPos, z));
            }

            if (!valid || path.Count < 2)
            {
                path.Clear();
                Vector2 start = center - direction * halfLength - Main.screenPosition;
                Vector2 end = center + direction * halfLength - Main.screenPosition;
                path.Add(new Vector3(start, 0f));
                path.Add(new Vector3(end, 0f));
            }

            return path;
        }

        private static float SlashWidthFunc(float progress, float baseWidth, float cracklePhase, float revealProgress, float retreatProgress)
        {
            float taperInLinear = MathHelper.Clamp(progress / 0.08f, 0f, 1f);
            float taperOutLinear = MathHelper.Clamp((1f - progress) / 0.08f, 0f, 1f);
            float taperIn = 1f - MathF.Pow(1f - taperInLinear, 3f);
            float taperOut = 1f - MathF.Pow(1f - taperOutLinear, 3f);
            float taper = taperIn * taperOut;

            float revealLinear = MathHelper.Clamp((revealProgress - progress) / RevealSoftEdge, 0f, 1f);
            float reveal = 1f - MathF.Pow(1f - revealLinear, 4f);

            float retreatRaw = MathHelper.Clamp((progress - retreatProgress + RetreatSoftEdge) / RetreatSoftEdge, 0f, 1f);
            float retreat = 1f - MathF.Pow(1f - retreatRaw, 4f);

            float crackle = 1f
                + 0.06f * MathF.Sin(progress * 13f + cracklePhase)
                + 0.04f * MathF.Sin(progress * 29f - cracklePhase * 1.7f);

            return baseWidth * taper * crackle * reveal * retreat;
        }

        private static float GiantSlashWidthFunc(float progress, float baseWidth, float revealProgress, float retreatProgress)
        {
            float taperIn = 1f - MathF.Pow(1f - MathHelper.Clamp(progress / 0.06f, 0f, 1f), 4f);
            float taperOut = 1f - MathF.Pow(1f - MathHelper.Clamp((1f - progress) / 0.06f, 0f, 1f), 4f);
            float taper = taperIn * taperOut;

            float centered = (progress - 0.5f) * 2f;
            float starProfile = MathF.Pow(1f - centered * centered, 0.35f);
            float starShape = MathHelper.Lerp(0.3f, 1f, starProfile);

            float revealLinear = MathHelper.Clamp((revealProgress - progress) / 0.2f, 0f, 1f);
            float reveal = 1f - MathF.Pow(1f - revealLinear, 5f);

            float retreatRaw = MathHelper.Clamp((progress - retreatProgress + 0.2f) / 0.2f, 0f, 1f);
            float retreat = 1f - MathF.Pow(1f - retreatRaw, 5f);

            return baseWidth * taper * starShape * reveal * retreat;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            int slashCount = IsPerfect ? 12 : 8;

            float age = VisualLifetime - Projectile.timeLeft;

            Effect shader = CalamityShaders.NanoblackSlashShader.Value;
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
            shader.Parameters["uTime"]?.SetValue(time);
            Color c = IsPerfect ? NanoblackReaper.LightspeedCarveColor1 : NanoblackReaper.NanoblackSlashColor2;
            shader.Parameters["uColor"]?.SetValue(new Vector3(c.R / 255f, c.G / 255f, c.B / 255f));
            shader.Parameters["uBrightness"]?.SetValue(1f);

            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -200f, 200f);

            float baseLength = NanoblackLightspeedCarve.HitboxRadius * 0.9f * RangeMultiplier;
            float seed = Projectile.identity * 1.913f;
            float curveBase = IsPerfect ? CurveStrengthPerfect : CurveStrength;

            var device = Main.instance.GraphicsDevice;

            using var lease = RenderTargetPool.Shared.Rent(
                device,
                Main.screenWidth,
                Main.screenHeight,
                RenderTargetDescriptor.Default
            );

            Main.spriteBatch.End(out var ss);

            using (lease.Scope(clearColor: Color.Transparent))
            {
                using var scope = SanePrimitiveRenderer.BeginShaderScope(shader, Matrix.Identity, view, projection,
                    depthState: DepthStencilState.None, blendState: BlendState.Additive);

                DrawAllSlashes(scope, slashCount, age, time, seed, baseLength, curveBase);

                if (IsPerfect)
                {
                    int giantCount = GiantSlashCount;
                    for (int g = 0; g < giantCount; g++)
                        DrawGiantSlash(scope, age, time, seed, baseLength, g);
                }
            }

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
            Main.spriteBatch.Draw(lease.Target, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            Effect bloomShader = CalamityShaders.GaussianBloomShader.Value;
            bloomShader.Parameters["uSource"]?.SetValue(new Vector4(Main.screenWidth, Main.screenHeight, 0f, 0f));

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
            bloomShader.CurrentTechnique.Passes["BloomShader"].Apply();
            Main.spriteBatch.Draw(lease.Target, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            if (IsPerfect)
            {
                int giantCount = GiantSlashCount;
                for (int g = 0; g < giantCount; g++)
                    DrawGiantSlashBlackCore(age, time, seed, baseLength, g);
            }

            Main.spriteBatch.Begin(ss);

            return false;
        }
        
        private void DrawAllSlashes(
            SanePrimitiveRenderer.ShaderScope scope,
            int slashCount, float age, float time,
            float seed, float baseLength, float curveBase)
        {
            for (int i = 0; i < slashCount; i++)
            {
                float startDelay = i * SlashDelay;
                float growLinear = Utils.GetLerpValue(startDelay, startDelay + GrowTime, age, true);
                if (growLinear <= 0f)
                    continue;
                float growProgress = 1f - MathF.Pow(1f - growLinear, 2.5f);
                float fadeIn = Utils.GetLerpValue(startDelay, startDelay + FadeInTime, age, true);

                float angle = MathHelper.TwoPi * i / slashCount + seed * 0.1f;
                float lengthScale = 0.75f + 0.2f * MathF.Sin(seed + i * 1.7f);
                float offsetScale = 0.25f * MathF.Cos(seed * 1.3f + i * 2.1f);
                float widthInterpolant = 0.5f + 0.5f * MathF.Sin(seed * 0.7f + i * 2.3f);
                float minWidth = IsPerfect ? MinWidthPerfect : MinWidthStandard;
                float maxWidth = IsPerfect ? MaxWidthPerfect : MaxWidthStandard;
                float slashWidth = MathHelper.Lerp(minWidth, maxWidth, widthInterpolant);

                Vector2 direction = angle.ToRotationVector2();
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 offset = perpendicular * baseLength * offsetScale;
                Vector2 center = Projectile.Center + offset;
                float halfLength = baseLength * lengthScale;

                float slashSeed = seed + i * 3.17f;

                float curveDirection = (i % 2 == 0) ? 1f : -1f;
                float curveSigned = curveBase * curveDirection * (0.7f + 0.6f * Hash(slashSeed * 0.43f));

                float retreatStartAge = startDelay + GrowTime + HoldTime;
                float retreatAge = Utils.GetLerpValue(retreatStartAge, retreatStartAge + FadeOutTime, age, true);
                float slashFadeOut = 1f - retreatAge;
                if (slashFadeOut <= 0f)
                    continue;

                float smoothFadeOut = MathF.Pow(slashFadeOut, 0.5f);

                float flashProgress = 1f - slashFadeOut;
                float flash = 1f;
                if (flashProgress > 0f && flashProgress < FlashDuration)
                {
                    float flashT = flashProgress / FlashDuration;
                    flash = 1f + (FlashIntensity - 1f) * (1f - MathF.Pow(1f - MathF.Sin(flashT * MathHelper.Pi), 2f));
                }

                float widthCollapse = 0.7f + 0.3f * MathF.Pow(smoothFadeOut, 3f);

                float retreatT = 1f - smoothFadeOut;
                float retreatProgress = MathF.Pow(retreatT, 0.4f) * (1f + RetreatOvershoot);

                float colorShift = MathF.Pow(1f - smoothFadeOut, 2f);
                Color aliveColor = NanoblackReaper.LightspeedCarveColor1;
                Color dyingColor = NanoblackReaper.LightspeedCarveColor2;
                Color baseColor = Color.Lerp(aliveColor, dyingColor, colorShift);

                float opacityFade = MathF.Pow(MathHelper.Clamp(smoothFadeOut / 0.2f, 0f, 1f), 0.3f);
                float shimmer = 1f + 0.08f * MathF.Sin(time * 14f + slashSeed * 7.7f)
                                    + 0.05f * MathF.Sin(time * 23f - slashSeed * 4.1f);
                float opacity = opacityFade * fadeIn * flash * shimmer;
                float cracklePhase = slashSeed * 11f + time * 12f;

                float breathing = 1f + BreathingAmount * MathF.Sin(time * BreathingSpeed + slashSeed * 3.3f);
                float fadedWidth = slashWidth * widthCollapse * breathing;

                for (int echo = EchoCount; echo >= 0; echo--)
                {
                    float echoAge = age - echo * EchoTimeLag;
                    float echoGrowLinear = Utils.GetLerpValue(startDelay, startDelay + GrowTime, echoAge, true);
                    if (echoGrowLinear <= 0f)
                        continue;
                    float echoGrow = 1f - MathF.Pow(1f - echoGrowLinear, 2.5f);
                    float echoFadeIn = Utils.GetLerpValue(startDelay, startDelay + FadeInTime, echoAge, true);

                    float echoOpacity = opacity * echoFadeIn * MathF.Pow(EchoOpacityFalloff, echo);
                    float echoWidth = fadedWidth * MathF.Pow(EchoWidthFalloff, echo);

                    float echoTime = time - echo * 0.06f;

                    float echoLength = halfLength * (echo == 0 ? 1f : 0.97f - echo * 0.02f);
                    List<Vector3> path = BuildSlashPath(
                        center, direction, echoLength,
                        curveSigned, slashSeed, echoGrow, echoTime);

                    Color echoTint = NanoblackReaper.NanoblackSlashColor2;
                    Color echoColor = echo == 0
                        ? baseColor * echoOpacity
                        : Color.Lerp(echoTint, dyingColor, colorShift) * echoOpacity;

                    float capturedEchoWidth = echoWidth;
                    float capturedCrackle = cracklePhase - echo * 5f;

                    float capturedReveal = echoGrow * (1f + RevealOvershoot);
                    float capturedRetreat = retreatProgress;

                    using var mesh = TriangleStripBuilder.BuildStripPooled(
                        path,
                        progress => SlashWidthFunc(progress, capturedEchoWidth, capturedCrackle, capturedReveal, capturedRetreat),
                        echoColor,
                        PrimitiveMeshCache.Shared,
                        textured: true,
                        smoothingSegments: 2);

                    scope.Draw(mesh.View);
                }
            }
        }

        private void DrawGiantSlash(
            SanePrimitiveRenderer.ShaderScope scope,
            float age, float time, float seed, float baseLength, int giantIndex)
        {
            float giantAge = GetGiantAge(age, giantIndex);
            if (!TryGetGiantTimeline(giantAge, out float growProgress, out float fadeLinear, out float smoothFade))
                return;

            float impactFlash = CalculateImpactFlash(giantAge);
            List<Vector3> path = BuildGiantSlashPath(time, seed, baseLength, giantIndex, growProgress);

            float widthCollapse = 0.6f + 0.4f * MathF.Pow(smoothFade, 3f);
            float impactWidthBoost = 1f + 0.15f * MathF.Max(impactFlash - 1f, 0f);
            float currentWidth = GiantSlashWidth * widthCollapse * impactWidthBoost;

            float opacity = MathF.Pow(MathHelper.Clamp(smoothFade / 0.15f, 0f, 1f), 0.3f) * MathF.Min(impactFlash, 2.5f);
            Color edgeColor = NanoblackReaper.LightspeedCarveColor1 * opacity;

            float retreatProgress = CalculateGiantRetreatProgress(fadeLinear);

            float capturedWidth = currentWidth;
            float capturedReveal = growProgress * 1.22f;
            float capturedRetreat = retreatProgress;

            using var mesh = TriangleStripBuilder.BuildStripPooled(
                path,
                progress => GiantSlashWidthFunc(progress, capturedWidth, capturedReveal, capturedRetreat),
                edgeColor,
                PrimitiveMeshCache.Shared,
                textured: true,
                smoothingSegments: 2);

            scope.Draw(mesh.View);
        }

        private void DrawGiantSlashBlackCore(
            float age, float time, float seed, float baseLength, int giantIndex)
        {
            float giantAge = GetGiantAge(age, giantIndex);
            if (!TryGetGiantTimeline(giantAge, out float growProgress, out float fadeLinear, out float smoothFade))
                return;

            List<Vector3> path = BuildGiantSlashPath(time, seed, baseLength, giantIndex, growProgress);

            float widthCollapse = 0.6f + 0.4f * MathF.Pow(smoothFade, 3f);
            float coreWidth = GiantSlashWidth * 0.65f * widthCollapse;

            float opacity = MathF.Pow(MathHelper.Clamp(smoothFade / 0.15f, 0f, 1f), 0.3f);

            Color coreColor = Color.White * opacity;

            float retreatProgress = CalculateGiantRetreatProgress(fadeLinear);

            float capturedWidth = coreWidth;
            float capturedReveal = growProgress * 1.22f;
            float capturedRetreat = retreatProgress;

            Effect shader = CalamityShaders.NanoblackSlashShader.Value;
            shader.Parameters["uColor"]?.SetValue(new Vector3(1f, 1f, 1f));
            shader.Parameters["uBrightness"]?.SetValue(1.5f);

            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -200f, 200f);

            using var scope = SanePrimitiveRenderer.BeginShaderScope(shader, Matrix.Identity, view, projection,
                depthState: DepthStencilState.None, blendState: GiantSlashDarkenBlend);

            using var mesh = TriangleStripBuilder.BuildStripPooled(
                path,
                progress => GiantSlashWidthFunc(progress, capturedWidth, capturedReveal, capturedRetreat),
                coreColor,
                PrimitiveMeshCache.Shared,
                textured: true,
                smoothingSegments: 2);

            scope.Draw(mesh.View);

            Color c = NanoblackReaper.LightspeedCarveColor1;
            shader.Parameters["uColor"]?.SetValue(new Vector3(c.R / 255f, c.G / 255f, c.B / 255f));
            shader.Parameters["uBrightness"]?.SetValue(1f);
        }

        private static float CalculateGiantRetreatProgress(float fadeLinear)
            => fadeLinear > 0f ? MathF.Pow(fadeLinear, 0.4f) * 1.2f : 0f;

        private float GetGiantAge(float age, int giantIndex)
            => age - GiantSlashDelay - giantIndex * GiantSlashStagger;

        private static bool TryGetGiantTimeline(float giantAge, out float growProgress, out float fadeLinear, out float smoothFade)
        {
            growProgress = 0f;
            fadeLinear = 0f;
            smoothFade = 0f;

            if (giantAge < 0f)
                return false;

            float growLinear = MathHelper.Clamp(giantAge / GiantSlashGrowTime, 0f, 1f);
            growProgress = 1f - MathF.Pow(1f - growLinear, 4f);

            float fadeStartAge = GiantSlashGrowTime + GiantSlashHoldTime;
            fadeLinear = MathHelper.Clamp((giantAge - fadeStartAge) / GiantSlashFadeTime, 0f, 1f);

            float fadeOut = 1f - fadeLinear;
            if (fadeOut <= 0f)
                return false;

            smoothFade = MathF.Pow(fadeOut, 0.5f);
            return true;
        }

        private static float CalculateImpactFlash(float giantAge)
        {
            if (giantAge >= GiantSlashImpactFlashDuration)
                return 1f;

            float flashT = 1f - (giantAge / GiantSlashImpactFlashDuration);
            return 1f + (GiantSlashImpactFlash - 1f) * flashT * flashT;
        }

        private List<Vector3> BuildGiantSlashPath(float time, float seed, float baseLength, int giantIndex, float growProgress)
        {
            float angle = Hash(seed * 2.71f + giantIndex * 7.13f) * MathHelper.TwoPi;
            Vector2 direction = angle.ToRotationVector2();

            float halfLength = baseLength * GiantSlashLengthMult;
            float slashSeed = seed * 5.13f + giantIndex * 11.7f;

            return BuildSlashPath(
                Projectile.Center,
                direction,
                halfLength,
                0f,
                slashSeed,
                growProgress,
                time);
        }
    }
}
