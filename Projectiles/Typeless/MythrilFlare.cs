using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class MythrilFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public ref float Time => ref Projectile.ai[0];
        public const int AttackDelay = 22;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = Projectile.MaxUpdates * 210;
        }

        public override void AI()
        {
            // Create a puff of energy on the first frame.
            if (Projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust energyPuff = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2);
                    energyPuff.velocity = -Vector2.UnitY.RotatedByRandom(0.81f) * Main.rand.NextFloat(1.25f, 6f);
                    energyPuff.color = Color.Lerp(new Color(104, 183, 136), Color.White, Main.rand.NextFloat(0.5f));
                    energyPuff.scale = 1.1f;
                    energyPuff.alpha = 185;
                    energyPuff.noGravity = true;
                }
                Projectile.localAI[0] = 1f;
            }

            NPC potentialTarget = Projectile.Center.ClosestNPCAt(176f);

            // Increment the timer on the last extra update.
            if (Projectile.FinalExtraUpdate())
                Time++;

            // Sit in place for a moment.
            if (Time < AttackDelay)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.Pi / 8f) * 5f;

            // Rapidly move towards the nearest target.
            if (potentialTarget != null)
            {
                float distanceFromTarget = Projectile.Distance(potentialTarget.Center);
                float moveInterpolant = Utils.GetLerpValue(0f, 100f, distanceFromTarget, true) * Utils.GetLerpValue(600f, 400f, distanceFromTarget, true);
                Vector2 targetCenterOffsetVec = potentialTarget.Center - Projectile.Center;
                float movementSpeed = MathHelper.Min(17.5f, targetCenterOffsetVec.Length());
                Vector2 idealVelocity = targetCenterOffsetVec.SafeNormalize(Vector2.Zero) * movementSpeed;

                // Die if anything goes wrong with the velocity.
                if (Projectile.velocity.HasNaNs())
                    Projectile.Kill();

                // Approach the ideal velocity.
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, moveInterpolant * 0.08f);
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 2f);
            }
        }

        public override bool? CanDamage() => Time >= AttackDelay ? null : false;

        public Color TrailColor(float completionRatio, Vector2 vertexPos)
        {
            float trailOpacity = Utils.GetLerpValue(0f, 0.13f, completionRatio, true) * Utils.GetLerpValue(0.7f, 0.58f, completionRatio, true);
            Color startingColor = Color.Lerp(Color.White, Color.LightGreen, 0.47f);
            Color middleColor = Color.LightGreen;
            Color endColor = Color.Transparent;
            return CalamityUtils.MulticolorLerp(completionRatio, startingColor, middleColor, endColor) * trailOpacity;
        }

        public float TrailWidth(float completionRatio, Vector2 vertexPos) => MathHelper.SmoothStep(Projectile.width, 4.25f, completionRatio);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // Prepare the flame trail shader with its map texture.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(TrailWidth, TrailColor, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 74);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust energyPuff = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2);
                energyPuff.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.97f) * Main.rand.NextFloat(1f, 8f);
                energyPuff.color = Color.Lerp(new Color(104, 183, 136), Color.White, Main.rand.NextFloat(0.5f));
                energyPuff.scale = 1.1f;
                energyPuff.alpha = 185;
                energyPuff.noGravity = true;
            }
        }
    }
}
