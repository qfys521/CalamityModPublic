using Terraria;
using System.Linq;
using System.Collections.Generic;
using CalamityMod.DataStructures;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;

namespace CalamityMod.Tiles.BaseTiles
{
    public class BranchDrawer
    {
        public class Branch
        {
            public BezierCurve Curve;

            public Vector2 EndOfCurve;

            public float Direction;

            public float CurveLength;

            public float StartingWidth;

            public float EndingWidth;

            public Branch PreviousBranch = null;

            public int Generation
            {
                get
                {
                    int generation = 0;
                    var parent = PreviousBranch;
                    while (parent != null)
                    {
                        parent = parent.PreviousBranch;
                        generation++;
                    }

                    return generation;
                }
            }

            public Branch(BezierCurve curve, Vector2 end, float length, float direction, float startWidth, float endWidth, Branch previousBranch = null)
            {
                Curve = curve;
                EndOfCurve = end;
                CurveLength = length;
                Direction = direction;
                StartingWidth = startWidth;
                EndingWidth = endWidth;
                PreviousBranch = previousBranch;
            }
        }

        internal BasicEffect basicShader = null;

        internal Point PreviousPoint;

        internal VertexPositionColorTexture[] vertexCache = Array.Empty<VertexPositionColorTexture>();

        internal short[] indexCache = Array.Empty<short>();

        internal UnifiedRandom RNG = new(0);

        public BasicEffect BasicShader
        {
            get
            {
                if (!Main.dedServ && basicShader is null)
                {
                    basicShader = new BasicEffect(Main.instance.GraphicsDevice)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = true                        
                    };
                }
                return basicShader;
            }
        }

        public static int GetSeed(int x, int y) => unchecked(x * 55867 + y * 49311329 + (Main.ActiveWorldFileData?.Seed ?? 0));

        public Texture2D BarkTexture;

        // The max amount that branches can travel. Once the distance of the branches reaches or exceeds this threshold, the tree is done growing.
        public float MaxDistanceBeforeCutoff;

        public float DistanceUsedForTrunk;

        public float BranchMaxBendFactor;

        public float BranchTurnAngleVariance;

        // The shortest possible length a branch can have.
        public float MinBranchLength;

        // The width of the trunk. Successive branches have a dynamic, random width, but the trunk should be static.
        public float TrunkWidth;

        // Chance to create new branches instead of extending existing ones.
        public float ChanceToCreateNewBranches;

        public float VerticalStretchFactor;

        public float DownwardBiasFactor;

        public float BranchGrowthWidthDecay;

        public int MaxCutoffBranchesPerBranch;

        public const int ControlPointCountPerBranch = 8;

        public virtual void DrawThingAtEndOfBranch(Branch branch) { }

        public Dictionary<Branch, List<Branch>> GenerateBranches(Point p)
        {
            RNG = new(GetSeed(p.X, p.Y));
            float cutoffDistance = MaxDistanceBeforeCutoff;
            float trunkDirection = RNG.NextFloatDirection() * BranchTurnAngleVariance * 0.1f - MathHelper.PiOver2;
            float trunkSize = RNG.NextFloat(-8f, 8f) + DistanceUsedForTrunk;
            float distanceTraversed = trunkSize;
            Vector2 startOfTrunk = Vector2.UnitY * 10f;
            Vector2 endOfTrunk = startOfTrunk + trunkDirection.ToRotationVector2() * trunkSize;
            Branch trunk = GenerateBranchCurve(startOfTrunk, endOfTrunk, TrunkWidth, TrunkWidth);
            Dictionary<Branch, List<Branch>> existingBranches = new()
            {
                [trunk] = new()
            };

            void extendLengthOfBranch(Branch branch, float lengthToAdd)
            {
                branch.CurveLength += lengthToAdd;
                branch.EndOfCurve += branch.Direction.ToRotationVector2() * lengthToAdd;

                // Add the new length to all branches that exist on the current one, ones extented to each of them, and so on.
                foreach (Branch attachedBranch in existingBranches[branch])
                    extendLengthOfBranch(attachedBranch, lengthToAdd);
            }

            int tries = 0;
            while (distanceTraversed < cutoffDistance)
            {
                // Prevent infinite loops if conditions do not permit the distance traveled to reach its maximum.
                tries++;
                if (tries >= 1500)
                    break;

                // Sometimes simply extend an existing branch instead of creating new ones.
                if (RNG.NextFloat() > ChanceToCreateNewBranches)
                {
                    List<Branch> potentialBranchesToExtend = existingBranches.Where(b => b.Key.CurveLength < trunkSize * 0.4f && b.Key.CurveLength > MinBranchLength).Select(b => b.Key).ToList();
                    if (potentialBranchesToExtend.Count <= 0)
                        continue;

                    Branch branchToExtend = RNG.Next(potentialBranchesToExtend);

                    float lengthToAdd = branchToExtend.CurveLength * 0.12f;
                    extendLengthOfBranch(branchToExtend, lengthToAdd);
                    distanceTraversed += lengthToAdd;
                    continue;
                }

                // Pick a random branch to attach to and determine the properties of the potential next one.
                List<Branch> validBranches = existingBranches.Where(b => b.Value.Count < MaxCutoffBranchesPerBranch && b.Key.EndingWidth >= 6f).Select(b => b.Key).ToList();
                if (validBranches.Count <= 0)
                    continue;

                Branch branchToAttachTo = RNG.Next(validBranches);
                float maxBranchAngleVariance = BranchTurnAngleVariance * (branchToAttachTo == trunk ? 2.1f : 1f);
                float directionOfNextBranch = branchToAttachTo.Direction + RNG.NextFloatDirection() * maxBranchAngleVariance;
                float downwardBiasFactor = DownwardBiasFactor;
                float downwardBiasFromGeneration = Utils.Remap(branchToAttachTo.Generation, 0f, 5f, 0f, 0.8f);
                downwardBiasFactor = MathHelper.Clamp(downwardBiasFactor + downwardBiasFromGeneration, 0f, 0.95f);

                if (downwardBiasFactor > 0f && branchToAttachTo != trunk)
                {
                    float randomBias = RNG.NextFloat(0.67f, 1f) * downwardBiasFactor;
                    directionOfNextBranch = Vector2.Lerp(directionOfNextBranch.ToRotationVector2(), Vector2.UnitY, randomBias).ToRotation();
                }

                // Try not to create a branch with a direction very similar to other branches attached to the one that this one will attach to.
                if (existingBranches[branchToAttachTo].Count >= 1 && 
                    existingBranches[branchToAttachTo].Any(b => b.Direction.ToRotationVector2().AngleBetween(directionOfNextBranch.ToRotationVector2()) < 0.12f))
                {
                    continue;
                }

                float lengthOfNextBranch = MathHelper.Max(MinBranchLength, branchToAttachTo.CurveLength * RNG.NextFloat(0.5f, 0.925f));

                Vector2 start = branchToAttachTo.EndOfCurve;
                Vector2 end = start + directionOfNextBranch.ToRotationVector2() * lengthOfNextBranch;

                // Try not to create branches in already crowded areas.
                if (validBranches.Count(b => MathHelper.Distance(b.EndOfCurve.Y, end.Y) < 50f) >= 4f)
                    continue;

                Branch newBranch = GenerateBranchCurve(start, end, branchToAttachTo.EndingWidth, branchToAttachTo.EndingWidth * BranchGrowthWidthDecay, branchToAttachTo);

                // Create the new branch in the dictionary and make the old branch count count as having one extra branch attached.
                existingBranches[branchToAttachTo].Add(newBranch);
                existingBranches[newBranch] = new();

                // Add to traversed distance.
                distanceTraversed += lengthOfNextBranch;
            }

            // Go back and make all end branches have a small end width.
            foreach (Branch branch in existingBranches.Where(b => b.Value.Count <= 0).Select(b => b.Key))
                branch.EndingWidth = MathHelper.Min(3f, branch.EndingWidth);

            return existingBranches;
        }

        public void GetVertexData(Point p, out List<VertexPositionColorTexture> vertices, out List<short> indices, out IEnumerable<Branch> outwardmostBranches)
        {
            // Initialize vertex and index data.
            vertices = new();
            indices = new();

            // Determine branch data.
            var branchData = GenerateBranches(p);
            var branches = branchData.Select(b => b.Key);
            outwardmostBranches = branchData.Where(b => b.Value.Count <= 0f).Select(b => b.Key);

            // Generate vertex data.
            int batchIndex = 0;
            Vector2 screenOffset = (p.ToWorldCoordinates() - Main.screenPosition).Floor();
            Texture2D barkTexture = BarkTexture;
            foreach (Branch branch in branches.OrderBy(b => b.EndOfCurve.Y))
            {
                int pointCount = 12;
                List<Vector2> smoothenedPoints = branch.Curve.GetPoints(pointCount + 1);
                Vector2? prevBottomLeft = null;
                Vector2? prevBottomRight = null;
                if (branch.PreviousBranch != null)
                {
                    Vector2 previousOrthogonalDirection = (branch.PreviousBranch.Direction + MathHelper.PiOver2).ToRotationVector2();
                    prevBottomLeft = branch.PreviousBranch.EndOfCurve + previousOrthogonalDirection * branch.PreviousBranch.EndingWidth * 0.5f;
                    prevBottomRight = branch.PreviousBranch.EndOfCurve - previousOrthogonalDirection * branch.PreviousBranch.EndingWidth * 0.5f;
                }

                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 top = smoothenedPoints[i];
                    Vector2 bottom = smoothenedPoints[i + 1];
                    float topCompletionRatio = i / (float)pointCount;
                    float bottomCompletionRatio = (i + 1) / (float)pointCount;
                    if (i == pointCount - 1f)
                    {
                        topCompletionRatio = 1f;
                        bottomCompletionRatio = 1f;
                        bottom = branch.EndOfCurve;
                    }

                    // Calculate frame coordinates.
                    // This sucked to make.
                    float topWidth = MathHelper.Lerp(branch.StartingWidth, branch.EndingWidth, topCompletionRatio);
                    float bottomWidth = MathHelper.Lerp(branch.StartingWidth, branch.EndingWidth, bottomCompletionRatio);
                    float topTexCoord = branch.CurveLength * topCompletionRatio / VerticalStretchFactor / barkTexture.Height;
                    float bottomTexCoord = branch.CurveLength * bottomCompletionRatio / VerticalStretchFactor / barkTexture.Height;
                    if (VerticalStretchFactor <= 0f)
                    {
                        topTexCoord = topWidth;
                        bottomTexCoord = bottomWidth;
                    }
                    float stretchedHorizontalCoordTop = topWidth / barkTexture.Width;
                    float stretchedHorizontalCoordBottom = bottomWidth / barkTexture.Width;
                    if (topWidth > barkTexture.Width * 0.5f)
                        stretchedHorizontalCoordTop = 1f;
                    if (bottomWidth > barkTexture.Width * 0.5f)
                        stretchedHorizontalCoordBottom = 1f;

                    // Calculate texture coordinates.
                    Vector2 topLeftTexCoord = new(stretchedHorizontalCoordTop, topTexCoord);
                    Vector2 topRightTexCoord = new(0f, topTexCoord);
                    Vector2 bottomLeftTexCoord = new(stretchedHorizontalCoordBottom, bottomTexCoord);
                    Vector2 bottomRightTexCoord = new(0f, bottomTexCoord);

                    // Calculate draw coordinates.
                    Vector2 orthogonalDirection = (bottom - top).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                    Vector2 topLeft = prevBottomLeft ?? top + orthogonalDirection * topWidth * 0.5f;
                    Vector2 topRight = prevBottomRight ?? top - orthogonalDirection * topWidth * 0.5f;
                    Vector2 bottomLeft = bottom + orthogonalDirection * bottomWidth * 0.5f;
                    Vector2 bottomRight = bottom - orthogonalDirection * bottomWidth * 0.5f;

                    // Calculate lighting colors.
                    Vector2 lightOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(-Main.offScreenRange);
                    Color topLeftColor = Lighting.GetColor((topLeft + p.ToWorldCoordinates() + lightOffset).ToTileCoordinates());
                    Color topRightColor = Lighting.GetColor((topRight + p.ToWorldCoordinates() + lightOffset).ToTileCoordinates());
                    Color bottomLeftColor = Lighting.GetColor((bottomLeft + p.ToWorldCoordinates() + lightOffset).ToTileCoordinates());
                    Color bottomRightColor = Lighting.GetColor((bottomRight + p.ToWorldCoordinates() + lightOffset).ToTileCoordinates());

                    vertices.Add(new VertexPositionColorTexture(new Vector3(topLeft.Floor() + screenOffset, 0f), topLeftColor, topLeftTexCoord));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(topRight.Floor() + screenOffset, 0f), topRightColor, topRightTexCoord));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(bottomRight.Floor() + screenOffset, 0f), bottomRightColor, bottomRightTexCoord));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(bottomLeft.Floor() + screenOffset, 0f), bottomLeftColor, bottomLeftTexCoord));

                    indices.Add((short)(batchIndex * 4));
                    indices.Add((short)(batchIndex * 4 + 1));
                    indices.Add((short)(batchIndex * 4 + 2));
                    indices.Add((short)(batchIndex * 4));
                    indices.Add((short)(batchIndex * 4 + 2));
                    indices.Add((short)(batchIndex * 4 + 3));

                    prevBottomLeft = bottomLeft;
                    prevBottomRight = bottomRight;

                    batchIndex++;
                }
            }
        }

        public void Draw(Point p)
        {
            // Declare the vertex cache.
            GetVertexData(p, out var vertices, out var indices, out IEnumerable<Branch> outwardmostBranches);
            vertexCache = vertices.ToArray();
            indexCache = indices.ToArray();
            PreviousPoint = p;

            // Redefine the perspective matrices of the shader.
            CalamityUtils.CalculatePerspectiveMatricies(out Matrix effectView, out Matrix effectProjection);
            BasicShader.Texture = BarkTexture;
            BasicShader.View = effectView;
            BasicShader.Projection = effectProjection;

            // Draw the tree itself.
            Main.instance.GraphicsDevice.Textures[0] = BarkTexture;
            Main.instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexCache, 0, vertexCache.Length, indexCache, 0, indexCache.Length / 3);

            // Draw things at the end of branches.
            foreach (Branch outwardmostBranch in outwardmostBranches)
                DrawThingAtEndOfBranch(outwardmostBranch);
        }

        public void UseDefaultSize()
        {
            TileObjectData.newTile.Width = (int)Math.Ceiling(TrunkWidth / 16);
            TileObjectData.newTile.Height = (int)Math.Ceiling(DistanceUsedForTrunk / 16);
            TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2, TileObjectData.newTile.Height - 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = 2;
        }

        public Branch GenerateBranchCurve(Vector2 start, Vector2 end, float startWidth, float endWidth, Branch previousBranch = null)
        {
            float distanceBetweenPoints = Vector2.Distance(start, end);
            Vector2[] initialPoints = new Vector2[ControlPointCountPerBranch];
            Vector2 orthogonalDirection = (end - start).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < ControlPointCountPerBranch; i++)
                initialPoints[i] = Vector2.Lerp(start, end, i / (float)(ControlPointCountPerBranch - 1f));

            // Create a bend midway.
            float bendFactor = (float)Math.Pow(RNG.NextFloat(), 0.66) * RNG.NextBool().ToDirectionInt() * BranchMaxBendFactor;
            bendFactor = MathHelper.Lerp(bendFactor, Math.Sign(bendFactor) * BranchMaxBendFactor, Utils.GetLerpValue(DistanceUsedForTrunk * 0.4f, DistanceUsedForTrunk * 0.75f, distanceBetweenPoints, true));

            initialPoints[ControlPointCountPerBranch / 2] += orthogonalDirection * RNG.NextFloatDirection() * distanceBetweenPoints * bendFactor;
            
            return new(new(initialPoints), end, distanceBetweenPoints, (end - start).ToRotation(), startWidth, endWidth, previousBranch);
        }
    }
}
