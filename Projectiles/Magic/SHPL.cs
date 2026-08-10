using System.Linq;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class SHPL : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Magic";

        public bool ShouldStopAndDie
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value.ToInt();
        }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public GeneralDrawLayer LayerToRenderTo => GeneralDrawLayer.BeforeProjectiles;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.scale = 1.85f;
            Projectile.friendly = true;
            Projectile.extraUpdates = 3;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override bool? CanDamage() => !ShouldStopAndDie;

        public override void AI()
        {
            // 12MAY2025: fryzahh:
            // This is made so that when hitting targets or tiles, the oldPos array can properly update so the laser appears to 
            // naturally dissipate entirely before despawning. Of course, it's also coded so that the laser stops dealing damage 
            // entirely once this is done to avoid hitting targets multiple times, so functionally the projectile remains the same
            // and this change should have little to no effect on gameplay. If it does, ping me about it.
            // 2FEB2026: sunny:
            // Due to Calclone's new border, lasers no longer have tile collision, so this part of the code is now only used for hitting targets.
            if (ShouldStopAndDie)
            {
                Projectile.velocity *= 0f;
                if (Projectile.oldPos.Last() == Projectile.position)
                {
                    Projectile.scale -= 0.15f;
                    if (Projectile.scale <= 0f)
                    {
                        Projectile.Kill();
                        return;
                    }
                }

                // Spawn a bunch of dusts at the dissipation point.
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 6f;
                    float dustScale = Main.rand.NextFloat(1.8f, 2.4f);
                    Color dustColor = Color.Lerp(SHPB.FindColorForSoul((int)Projectile.ai[0]), Color.White, Main.rand.NextFloat());

                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, dustColor, dustScale);
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.noLightEmittance = false;
                }
            }

            Projectile.rotation += MathHelper.TwoPi / 60f;
            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 0.5f, 0.2f, 0.5f);
            float timerIncr = 3f;
            Projectile.localAI[0] += timerIncr;
            if (Projectile.localAI[0] > 100f)
                Projectile.localAI[0] = 100f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ShouldStopAndDie = true;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ShouldStopAndDie = true;
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            AltLineParticle line = new(Projectile.Center, -Projectile.velocity.RotatedByRandom(MathHelper.Pi / 6f), false, 20, 1f, SHPB.FindColorForSoul((int)Projectile.ai[0]));
            GeneralParticleHandler.SpawnParticle(line);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => false;

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (!ShouldStopAndDie)
                return;

            // Draw a star over the collision point while dissipting.
            Texture2D starTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Main.EntitySpriteDraw(starTexture, drawPosition, null, Color.White, Projectile.rotation, starTexture.Size() * 0.5f, Projectile.scale, 0);
            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
        }

        public float LaserWidthFunction(float completion, Vector2 vertexPos)
        {
            float maxBodyWidth = Projectile.scale * Projectile.width;
            float scaleInterpolant = Utils.GetLerpValue(0.05f, 0.2f, completion, true) * Utils.GetLerpValue(0.95f, 0.8f, completion, true);
            return maxBodyWidth * scaleInterpolant * Projectile.scale;
        }

        public float LaserCoreEnergyFunction(float completion, Vector2 vertexPos)
        {
            float maxBodyWidth = (Projectile.scale * Projectile.width) * 0.25f;
            float scaleInterpolant = Utils.GetLerpValue(0.05f, 0.2f, completion, true) * Utils.GetLerpValue(0.95f, 0.8f, completion, true);
            return maxBodyWidth * scaleInterpolant * Projectile.scale;
        }

        public Color LaserColorFunction(float completion, Vector2 vertexPos)
        {
            Color startColor = Color.Lerp(Color.White, SHPB.FindColorForSoul((int)Projectile.ai[0]), Utils.GetLerpValue(0f, 0.125f, completion, true));
            Color bodyColor = Color.Lerp(startColor, SHPB.FindColorForSoul((int)Projectile.ai[0]), Utils.GetLerpValue(0.125f, 1f, completion, true));           
            return Color.Lerp(startColor, bodyColor, completion);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            // Render the main trail for the body for the laser.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(LaserWidthFunction, LaserColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length * 2);

            // Render a smaller, pure white trail in the same position to represent condensed energy in the core of the laser.
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(LaserCoreEnergyFunction, (_,_) => Color.White, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length * 2);
        }
    }
}
