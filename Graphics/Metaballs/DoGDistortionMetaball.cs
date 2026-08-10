using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.Enums;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Systems.Graphic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Metaballs
{
    public class DoGDistortionMetaball : Metaball
    {
        public class DistortionParticle(Vector2 center, Vector2 velocity, float size, bool square = false, float rotation = 0f)
        {
            public Vector2 Center = center;

            public Vector2 Velocity = velocity;

            public float Size = size;

            public bool Square = square;

            public float Rotation = rotation;

            public void Update()
            {
                Center += Velocity;
                Velocity *= 0.96f;
                Size *= 0.91f;
            }
        }

        public static List<DistortionParticle> Particles { get; private set; } = [];

        public override GeneralDrawLayer DrawLayer => GeneralDrawLayer.AfterProjectiles;

        public override bool AnythingToDraw => Particles.Count > 0 || CalamityUtils.AnyProjectiles(ModContent.ProjectileType<DoGRiftCrack>());

        public override Color EdgeColor => new(136, 26, 186);

        public override IEnumerable<Texture2D> Layers
        {
            get
            {
                yield return ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;
            }
        }

        public static void SpawnParticle(Vector2 position, Vector2 velocity, float size, bool square = false, float rotation = 0f) =>
            Particles.Add(new(position, velocity, size, square, rotation));

        public override void Update()
        {
            for (int i = 0; i < Particles.Count; i++)
                Particles[i].Update();
            Particles.RemoveAll(p => p.Size <= 2f);
        }

        public override void PrepareShaderForTarget(int layerIndex)
        {
            var metaballShader = CalamityShaders.MetaballEdgeShader;
            var gd = Main.instance.GraphicsDevice;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 layerScrollOffset = Main.screenPosition / screenSize + CalculateManualOffsetForLayer(layerIndex);
            if (FixedToScreen)
                layerScrollOffset = Vector2.Zero;

            metaballShader.Value.Parameters["layerSize"]?.SetValue(DoGVisualsManager.DistortionForegroundContentsTarget.Target.Size());
            metaballShader.Value.Parameters["screenSize"]?.SetValue(screenSize);
            metaballShader.Value.Parameters["layerOffset"]?.SetValue(layerScrollOffset);
            metaballShader.Value.Parameters["edgeColor"]?.SetValue(EdgeColor.ToVector4());
            metaballShader.Value.Parameters["singleFrameScreenOffset"]?.SetValue(MetaballManager.CameraOffsetSinceLastFrame / screenSize);

            gd.Textures[1] = DoGVisualsManager.DistortionForegroundContentsTarget.Target;
            gd.SamplerStates[1] = SamplerState.LinearWrap;

            metaballShader.Value.CurrentTechnique.Passes[0].Apply();
        }

        public override void DrawInstances()
        {
            Texture2D metaballTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;
            Texture2D squareTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/Square").Value;

            foreach (DistortionParticle particle in Particles)
            {
                Texture2D textureToUse = particle.Square ? squareTexture : metaballTexture;
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                Vector2 origin = textureToUse.Size() * 0.5f;
                Vector2 scale = Vector2.One * particle.Size / textureToUse.Size();
                Main.spriteBatch.Draw(textureToUse, drawPosition, null, Color.White, particle.Rotation, origin, scale, SpriteEffects.None, 0f);
            }

            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<DoGRiftCrack>())
                {
                    Color lightColor = Color.White;
                    proj.ModProjectile.PreDraw(Main.player[proj.owner], ref lightColor);
                }
            }
        }
    }
}
