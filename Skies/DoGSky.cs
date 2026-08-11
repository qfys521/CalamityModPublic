using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.Events;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Systems.Graphic;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.Skies
{
    public class DoGSky : CustomSky
    {
        public class RealityCrack
        {
            public Vector2 Position;

            public float Depth;

            public float Scale;

            public float Rotation;
        }

        public class DistortionRiftArm
        {
            public Vector2 ArmStartingPosition;

            public List<Vector2> ArmPoints;

            public float Depth;

            public float MaxWidth;

            public float MinDistanceBetweenPoints;

            public float MaxDistanceBetweenPoints;

            public float MinPointAngularVariance;

            public float MaxPointAngularVariance;

            public int TotalPoints;
        }

        public static readonly Color DoGTwlight = new(147, 24, 204);

        public static readonly Color DoGLightBlue = new(0, 221, 250);

        public bool Initialized;

        public int DoGIndex;

        public List<DistortionRiftArm> MainRiftCracks;

        public List<DistortionRiftArm> OuterRiftCracks;

        public List<RealityCrack> RealityCracks;

        public List<DistortionRiftArm> DistortionRiftArms;

        public static bool CanSkyBeActive { get; private set; }

        public static float SkyIntensity { get; private set; }

        public static Color DoGSkyColor { get; private set; }

        public override void Update(GameTime gameTime)
        {
            // Initialize the rift arms and background cracks.
            GenerateRift();

            DoGIndex = NPC.FindFirstNPC(ModContent.NPCType<DevourerofGodsHead>());
            GetCorrectDoGColor();

            if (CanSkyBeActive)
                SkyIntensity = MathHelper.Clamp(SkyIntensity + 0.05f, 0f, 1f);
            else
                SkyIntensity = MathHelper.Clamp(SkyIntensity - 0.05f, 0f, 1f);
        }

        private void GenerateRift()
        {
            Vector2 riftSpawnLocation = Main.LocalPlayer.Center - Vector2.UnitY * 2000f + Main.rand.NextVector2Circular(2000f, 2000f);

            if (!Initialized)
            {
                MainRiftCracks = [];
                OuterRiftCracks = [];
                RealityCracks = [];
                DistortionRiftArms = [];

                int crackCount = 3;
                for (int i = 0; i < crackCount; i++)
                {
                    RealityCrack realityCrack = new RealityCrack()
                    {
                        Position = riftSpawnLocation,
                        Depth = 30f,
                        Scale = 1.55f * Main.rand.NextFloat(0.9f, 1.1f),
                        Rotation = i * MathHelper.TwoPi / crackCount
                    };
                    RealityCracks.Add(realityCrack);
                }

                // Generate up to 30 arms for the rift.
                // The first 14 are larger cracks to form the main hole at the impact point.
                // The rest are longer, skinner cracks protruding outwards from it.
                for (int i = 0; i < 14; i++)
                {
                    DistortionRiftArm distortionRiftArm = new DistortionRiftArm()
                    {
                        ArmStartingPosition = riftSpawnLocation,
                        Depth = 30f,
                        TotalPoints = 8,

                        MaxWidth = Main.rand.NextFloat(100f, 120f),
                        MinDistanceBetweenPoints = 100f,
                        MaxDistanceBetweenPoints = 140f,
                        MinPointAngularVariance = -6,
                        MaxPointAngularVariance = 6,

                        ArmPoints = [],
                    };
                    MainRiftCracks.Add(distortionRiftArm);
                }

                int outerCracksAmount = Main.rand.Next(12, 17);
                for (int i = 0; i < outerCracksAmount; i++)
                {
                    DistortionRiftArm distortionRiftArm = new DistortionRiftArm()
                    {
                        ArmStartingPosition = riftSpawnLocation,
                        Depth = 30f,
                        TotalPoints = Main.rand.Next(6, 10),

                        MaxWidth = Main.rand.NextFloat(6f, 9f),
                        MinDistanceBetweenPoints = 400f,
                        MaxDistanceBetweenPoints = 500f,
                        MinPointAngularVariance = Main.rand.Next(-10, -5),
                        MaxPointAngularVariance = Main.rand.Next(5, 10),

                        ArmPoints = [],
                    };
                    OuterRiftCracks.Add(distortionRiftArm);
                }

                // Generate the points for each arm and add them all to the global arm list.
                // Main cracks.
                for (int i = 0; i < MainRiftCracks.Count; i++)
                {
                    float minDistance = MainRiftCracks[i].MinDistanceBetweenPoints * MainRiftCracks[i].Depth * 0.1f;
                    float maxDistance = MainRiftCracks[i].MaxDistanceBetweenPoints * MainRiftCracks[i].Depth * 0.1f;
                    float minAngularVariance = MainRiftCracks[i].MinPointAngularVariance;
                    float maxAngularVariance = MainRiftCracks[i].MaxPointAngularVariance;
                    float placementAngle = i * MathHelper.TwoPi / MainRiftCracks.Count;

                    if (MainRiftCracks[i].ArmPoints.Count <= 0)
                        MainRiftCracks[i].ArmPoints = GenerateDistortionRiftArmPoints(MainRiftCracks[i].ArmStartingPosition, MainRiftCracks[i].TotalPoints, minDistance, maxDistance, minAngularVariance, maxAngularVariance, placementAngle);
                    DistortionRiftArms.Add(MainRiftCracks[i]);
                }

                // Outer cracks.
                for (int i = 0; i < OuterRiftCracks.Count; i++)
                {
                    float minDistance = OuterRiftCracks[i].MinDistanceBetweenPoints * OuterRiftCracks[i].Depth * 0.1f;
                    float maxDistance = OuterRiftCracks[i].MaxDistanceBetweenPoints * OuterRiftCracks[i].Depth * 0.1f;
                    float minAngularVariance = OuterRiftCracks[i].MinPointAngularVariance;
                    float maxAngularVariance = OuterRiftCracks[i].MaxPointAngularVariance;
                    float placementAngle = i * MathHelper.TwoPi / OuterRiftCracks.Count;

                    if (OuterRiftCracks[i].ArmPoints.Count <= 0)
                        OuterRiftCracks[i].ArmPoints = GenerateDistortionRiftArmPoints(OuterRiftCracks[i].ArmStartingPosition, OuterRiftCracks[i].TotalPoints, minDistance, maxDistance, minAngularVariance, maxAngularVariance, placementAngle);
                    DistortionRiftArms.Add(OuterRiftCracks[i]);
                }

                Initialized = true;
            }
        }

        private static List<Vector2> GenerateDistortionRiftArmPoints(Vector2 startingPosition, int totalPoints, float minDistanceBetweenPoints, float maxDistanceBetweenPoints, float minAngularVariance, float maxAngularVariance, float? optionalPointPlacementAngle = null)
        {
            // (Original code by Xyk; See TriactisHammerExplosion)
            // -fryzahh

            List<Vector2> points = [];
            for (int j = 0; j < totalPoints; j++)
            {
                // First point should always be the starting position.
                if (j == 0)
                {
                    points.Add(startingPosition);
                }
                // Next point should be a random location around the starting position.
                // If the placement angle has a value then rotate towards that with a very slight offset.
                else if (j == 1)
                {
                    float distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints);
                    Vector2 nextPointPosition = startingPosition + Main.rand.NextVector2Unit() * distanceFromLastPoint;
                    if (optionalPointPlacementAngle != null)
                        nextPointPosition = startingPosition + Vector2.UnitX.RotatedBy(optionalPointPlacementAngle.Value) * distanceFromLastPoint;
                    points.Add(nextPointPosition);
                }
                // All other points follow after the last one with the same slight offset to resemble cracks.
                else
                {
                    Vector2 previousPoint = points[j - 2];
                    float distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints);
                    if (j == totalPoints - 1)
                        distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints) * 0.5f;

                    Vector2 newPoint = points[j - 1] + (previousPoint.DirectionTo(points[j - 1]) * distanceFromLastPoint).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(minAngularVariance, maxAngularVariance) * j * 0.5f));
                    points.Add(newPoint);
                }
            }

            return points;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (Main.gameMenu || !CalamityClientConfig.Instance.FancyBackgroundVisuals || BossRushEvent.BossRushActive)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the wind effect.
            if (minDepth >= float.MinValue && maxDepth <= 5f)
                DrawDistortionWinds(spriteBatch);

            // Draw the rolling fog effect.
            if (minDepth >= 3f && maxDepth <= 10f)
                DrawRollingBackgroundFog(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the rift aura.
            if (minDepth >= 6f && maxDepth <= float.MaxValue)
                DrawRiftAura(spriteBatch);

            // Draw the cracked glass textures.
            if (minDepth >= 6f && maxDepth <= float.MaxValue)
                DrawRealityCracks(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the rift to the Distortion over the cracked glass.
            if (minDepth >= 6f && maxDepth <= float.MaxValue)
                DrawDistortionRift(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
        }

        public void DrawRiftToRenderTarget()
        {
            if (DistortionRiftArms == null || DistortionRiftArms.Count == 0)
                return;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);

            for (int i = 0; i < DistortionRiftArms.Count; i++)
            {
                Vector2 depthFactor = new(1f / DistortionRiftArms[i].Depth, 0.9f / DistortionRiftArms[i].Depth);
                List<Vector2> parallaxedPoints = [];
                for (int j = 0; j < DistortionRiftArms[i].ArmPoints.Count; j++)
                    parallaxedPoints.Add((DistortionRiftArms[i].ArmPoints[j] - screenCenter) * depthFactor + screenCenter);

                PrimitiveRenderer.RenderTrail(parallaxedPoints, new((completion, _) => MathHelper.Lerp(0f, DistortionRiftArms[i].MaxWidth, 1f - completion) * SkyIntensity, (_, _) => Color.White * 1f, null, false, useUnscaledMatrices: true), DistortionRiftArms[i].TotalPoints + 16);
            }
        }

        private void DrawDistortionWinds(SpriteBatch spriteBatch)
        {
            Effect shader = CalamityShaders.DoGDistortionWindsShader.Value;
            Texture2D windsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2").Value;
            Texture2D highlightsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/SharpNoise").Value;
            Texture2D distortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/MeltyNoise").Value;
            Texture2D erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Pebbles").Value;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);

            shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["overallOpacity"].SetValue(SkyIntensity);
            shader.Parameters["distortionStrength"].SetValue(0.3f);
            shader.Parameters["mainNoiseTextureScale"].SetValue(0.8f);
            shader.Parameters["distortionTextureScale"].SetValue(0.6f);
            shader.Parameters["erosionTextureScale"].SetValue(2f);
            shader.Parameters["erosionMin"].SetValue(0.38f * SkyIntensity);
            shader.Parameters["gradientPrecision"].SetValue(20f);

            shader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            shader.Parameters["worldOffset"].SetValue(Main.screenPosition / windsTexture.Size() * 0.025f);

            shader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.82f).ToVector3());
            shader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.Black, DoGSkyColor, 0.32f).ToVector3());
            shader.Parameters["highlightsColor"].SetValue(DoGSkyColor.ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = highlightsTexture;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

            Main.instance.GraphicsDevice.Textures[2] = distortionTexture;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[3] = erosionTexture;
            Main.instance.GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;

            shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(windsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private void DrawRollingBackgroundFog(SpriteBatch spriteBatch)
        {
            Effect shader = CalamityShaders.DoGBackgroundFogShader.Value;
            Texture2D cloudsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/RealisticClouds").Value;
            Texture2D erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise").Value;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);

            shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 3f);
            shader.Parameters["overallOpacity"].SetValue(SkyIntensity * 0.25f);
            shader.Parameters["distortionStrength"].SetValue(0.12f);
            shader.Parameters["mainNoiseTextureScale"].SetValue(0.6f);
            shader.Parameters["distortionTextureScale"].SetValue(0.8f);
            shader.Parameters["erosionTextureScale"].SetValue(0.26f);
            shader.Parameters["erosionMin"].SetValue(0.06f * SkyIntensity);
            shader.Parameters["gradientPrecision"].SetValue(20f);

            shader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            shader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.005f);

            shader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.Black, Color.DarkGray, 0.75f).ToVector3());
            shader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.DarkGray, DoGSkyColor, 0.8f).ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = cloudsTexture;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[2] = erosionTexture;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(cloudsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private void DrawRiftAura(SpriteBatch spriteBatch)
        {
            if (RealityCracks == null || RealityCracks.Count == 0)
                return;

            Effect shader = CalamityShaders.DoGRiftAuraShader.Value;
            Texture2D riftAuraTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Smudges").Value;
            Texture2D distortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Swirls").Value;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);

            shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["pixelSize"].SetValue(screenSize * 0.5f);
            shader.Parameters["distortionStrength"].SetValue(0.78f);

            shader.Parameters["opacityCutoffValue"].SetValue(0.675f * SkyIntensity);
            shader.Parameters["fadeoutPower"].SetValue(1.25f);
            shader.Parameters["overallOpacity"].SetValue(0.6f);

            shader.Parameters["minBrightnessValue"].SetValue(0f);
            shader.Parameters["brighterPixelColor"].SetValue(DoGSkyColor.ToVector3());
            shader.Parameters["gradientPrecision"].SetValue(8f);

            Main.instance.GraphicsDevice.Textures[1] = distortionTexture;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

            shader.CurrentTechnique.Passes[0].Apply();

            Vector2 depthFactor = new(1f / RealityCracks[0].Depth, 0.9f / RealityCracks[0].Depth);
            Vector2 drawPosition = (RealityCracks[0].Position - screenCenter) * depthFactor + screenCenter - Main.screenPosition;
            spriteBatch.Draw(riftAuraTexture, drawPosition, null, Color.White, Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 270f, riftAuraTexture.Size() * 0.5f, 1f, 0, 0f);
        }

        private void DrawRealityCracks(SpriteBatch spriteBatch)
        {
            if (RealityCracks == null || RealityCracks.Count == 0)
                return;

            Effect shader = CalamityShaders.DoGRealityCrackShader.Value;
            Texture2D cracksTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/CrackedGlass_Glowing").Value;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);

            shader.Parameters["opacityCutoffValue"].SetValue(0.8f * SkyIntensity);
            shader.Parameters["fadeoutPower"].SetValue(1f);
            shader.Parameters["overallOpacity"].SetValue(0.1015f);

            shader.Parameters["minBrightnessValue"].SetValue(0.6f);
            shader.Parameters["darkerPixelColor"].SetValue(DoGSkyColor.ToVector3());
            shader.Parameters["brighterPixelColor"].SetValue(Color.White.ToVector3());

            shader.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < RealityCracks.Count; i++)
            {
                Vector2 depthFactor = new(1f / RealityCracks[i].Depth, 0.9f / RealityCracks[i].Depth);
                Vector2 drawPosition = (RealityCracks[i].Position - screenCenter) * depthFactor + screenCenter - Main.screenPosition;
                spriteBatch.Draw(cracksTexture, drawPosition, null, Color.White, RealityCracks[i].Rotation, cracksTexture.Size() * 0.5f, RealityCracks[i].Scale, 0, 0f);
            }
        }

        private void DrawDistortionRift(SpriteBatch spriteBatch)
        {
            using (spriteBatch.Scope())
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, CalamityShaders.MetaballEdgeShader.Value, Main.BackgroundViewMatrix.TransformationMatrix);
                // Impose the distortion rift contents onto the primitives via shader.
                var metaballShader = CalamityShaders.MetaballEdgeShader;
                Texture2D distortionRiftContents = DoGVisualsManager.DistortionRiftBackgroundContentsTarget.Target;
                Texture2D distortionRift = DoGVisualsManager.DistortionRiftPrimitivesTarget.Target;

                Vector2 screenSize = new(Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height);

                metaballShader.Value.Parameters["layerSize"]?.SetValue(distortionRiftContents.Size());
                metaballShader.Value.Parameters["screenSize"]?.SetValue(screenSize);
                metaballShader.Value.Parameters["layerOffset"]?.SetValue(Vector2.Zero);
                metaballShader.Value.Parameters["singleFrameScreenOffset"]?.SetValue(Vector2.Zero);
                metaballShader.Value.Parameters["edgeColor"]?.SetValue(Color.Lerp(Color.Lerp(DoGSkyColor, Color.White, 0.6f), Color.Black, 0.15f).ToVector4());
                metaballShader.Value.Parameters["layerColor"]?.SetValue(DoGSkyColor.ToVector4());

                Main.instance.GraphicsDevice.Textures[1] = distortionRiftContents;
                Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

                metaballShader.Value.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(distortionRift, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.White);
                Main.spriteBatch.End();
            }
        }

        private void GetCorrectDoGColor()
        {
            if (DoGIndex != -1)
            {
                var DoG = Main.npc[DoGIndex].ModNPC<DevourerofGodsHead>();
                Color goalSkyColor = Color.Black;
                if (DoG.isInAgressiveState)
                    goalSkyColor = Color.Fuchsia;
                if (DoG.isInPassiveState)
                    goalSkyColor = DoGLightBlue;
                if (DoG.isInLaserWallState || DoG.isInPostWallState || DoG.postTeleportTimer > 0 || DoG.teleportTimer > 0 || DoG.NPC.localAI[2] < 180 && DoG.NPC.localAI[2] > 60)
                    goalSkyColor = DoGTwlight;

                DoGSkyColor = Color.Lerp(DoGSkyColor, goalSkyColor, 0.1f);
            }
            else
            {
                var goalSkyColor = Color.Black;
                if (Main.LocalPlayer.Calamity().monolithDevourerPShader > 0)
                    goalSkyColor = Color.Fuchsia;
                if (Main.LocalPlayer.Calamity().monolithDevourerBShader > 0)
                    goalSkyColor = DoGLightBlue;

                DoGSkyColor = Color.Lerp(DoGSkyColor, goalSkyColor, 0.1f);
            }
        }

        public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, DoGSkyColor, SkyIntensity);

        public override float GetCloudAlpha() => 1f;

        public override void Activate(Vector2 position, params object[] args) => CanSkyBeActive = true;

        public override bool IsActive() => CanSkyBeActive || SkyIntensity > 0f;

        public override void Deactivate(params object[] args)
        {
            CanSkyBeActive = false;
            if (SkyIntensity <= 0.1f)
            {
                Initialized = false;
                DistortionRiftArms?.Clear();
                RealityCracks?.Clear();
            }
        }

        public override void Reset()
        {
            CanSkyBeActive = false;
            Initialized = false;
            DistortionRiftArms?.Clear();
            RealityCracks?.Clear();
        }
    }
}
