using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class DoGRiftCrack : ModProjectile, ILocalizedModType
    {
        public Vector2[] CrackPoints;

        public ref float Timer => ref Projectile.ai[0];

        public ref float MaxWidth => ref Projectile.ai[1];

        public new string LocalizationCategory => "Projectiles.Boss";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
        }

        public override void AI()
        {
            float minDistanceBetweenPoints = 25f;
            float maxDistanceBetweenPoints = 50f;
            if (CrackPoints is null)
            {
                CrackPoints = new Vector2[25];

                Vector2 startingPosition = Projectile.Center;
                for (int i = 0; i < CrackPoints.Length; i++)
                {
                    // First point should always be the starting position.
                    if (i == 0)
                    {
                        CrackPoints[i] = startingPosition;
                    }
                    // Next point pivots away from the starting position in the direction of the velocity.
                    else if (i == 1)
                    {
                        Vector2 nextPointPosition = startingPosition + Projectile.velocity * Main.rand.NextFloat(0.8f, 1.2f) + Main.rand.NextVector2Circular(30f, 30f);
                        CrackPoints[i] = nextPointPosition;
                    }
                    // All other points follow after the last one with the same slight offset to resemble cracks.
                    else
                    {
                        Vector2 previousPoint = CrackPoints[i - 2];
                        float distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints);
                        if (i == CrackPoints.Length - 1)
                            distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints) * 0.5f;

                        Vector2 newPoint = CrackPoints[i - 1] + (previousPoint.DirectionTo(CrackPoints[i - 1]) * distanceFromLastPoint).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f) * i * 0.25f));
                        CrackPoints[i] = newPoint;
                    }

                    if (Main.rand.NextBool(9))
                        CrackPoints[i] += Main.rand.NextVector2Circular(10f, 10f);
                }
            }

            Projectile.scale = MathHelper.Lerp(1f, 0f, Timer / 30f);
            Timer++;
        }

        public float CrackWidthFunction(float completion, Vector2 vertexPos) => Projectile.scale * MathHelper.Lerp(MaxWidth, 0f, completion);

        public Color CrackColorFunction(float completion, Vector2 vertexPos) => Projectile.GetAlpha(Color.White);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (CrackPoints is not null)
            {
                PrimitiveRenderer.RenderTrail(CrackPoints, new(CrackWidthFunction, CrackColorFunction, null, false));
            }
            return false;
        }
    }
}
