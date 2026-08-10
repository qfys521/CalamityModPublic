using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.BaseProjectiles
{
    /// <summary>
    /// Implementation to make ModProjectile worms much easier to make.
    /// Modified version of BaseWormNPC to function on a ModProjectile instead of ModNPC
    /// </summary>
    public abstract class BaseWormProjectile : ModProjectile
    {
        #region Abstracts / Commonly Overridden Fields
        /// <summary>
        /// The amount of segments of this worm. This does not include the head
        /// </summary>
        public abstract int SegmentCount { get; }
        /// <summary>
        /// A list of the offsets to the next segment in the worm from this segment
        /// DOES include the head
        /// </summary>
        public abstract List<float> SegmentTypePositionOffsets { get; }
        /// <summary>
        /// A list of all textures for the segments to draw with
        /// does NOT include the head
        /// </summary>
        public abstract List<string> SegmentTextures { get; }
        /// <summary>
        /// Offsets for drawing each segment
        /// </summary>
        public List<Vector2> SegmentTypeDrawOffsets = new List<Vector2>();

        /// <summary>
        /// A list of all glow textures for the segments to draw with
        /// DOES include the head
        /// </summary>
        public virtual List<string?> GlowTextures { get; }

        /// <summary>
        /// How far through the current animation this worm is
        /// </summary>
        public float AnimationFrame = 0;
        /// <summary>
        /// The active animation for the worm
        /// </summary>
        public WormAnimation ActiveAnimation = null;
        #endregion
        #region Segments
        /// <summary>
        /// The textures for each segment type of this worm. Works like getting a texture from TextureAssets
        /// </summary>
        public List<Asset<Texture2D>> SegmentTextureAssets
        {
            get
            {
                if (internalTexAssets.Count == 0)
                    for (var i = 0; i < SegmentTextures.Count; i++)
                    {
                        internalTexAssets.Add(ModContent.Request<Texture2D>(SegmentTextures[i]));
                        if (SegmentTypeDrawOffsets.Count <= i)
                        {
                            SegmentTypeDrawOffsets.Add(Vector2.Zero);
                        }
                    }
                return internalTexAssets;
            }
        }

        /// <summary>
        /// Internal list that stores the textureassets.
        /// Use SegmentTextureAssets to get the data stored here.
        /// </summary>
        private List<Asset<Texture2D>> internalTexAssets = new List<Asset<Texture2D>>();
        /// <summary>
        /// The textures for each glow type of this worm. Works like getting a texture from TextureAssets
        /// </summary>
        public List<Asset<Texture2D>> GlowTextureAssets
        {
            get
            {
                if (internalGlowAssets.Count == 0)
                    for (var i = 0; i < GlowTextures.Count; i++)
                    {
                        if (GlowTextures[i] is not null)
                            internalGlowAssets.Add(ModContent.Request<Texture2D>(GlowTextures[i]));
                        else internalGlowAssets.Add(ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj"));
                    }
                return internalGlowAssets;
            }
        }

        /// <summary>
        /// Internal list that stores the glow textureassets.
        /// Use SegmentTextureAssets to get the data stored here.
        /// </summary>
        private List<Asset<Texture2D>> internalGlowAssets = new List<Asset<Texture2D>>();

        public List<BaseWormSegment> Segments = new();
       
        public enum SegmentFollowLogic
        {
            Regular = 0, //Traditional worm segment logic
            Exact = 1, //Follows the path the head took exactly
            // In the future, add a segment logic that supports solid tile collisions and forces applied to *any* segment
        }

        /// <summary>
        /// Which type of segment following logic this worm should currently use
        /// </summary>
        public SegmentFollowLogic SegmentFollowType;

        /// <summary>
        /// How rigid the segment should be when using default segment logic
        /// </summary>
        public float SegmentRigidity = 0.2f;

        /// <summary>
        /// The max rotational offset from the direction of the previous segment
        /// </summary>
        public float SegmentMaxRotation = MathHelper.TwoPi;

        /// <summary>
        /// The points used by ExactSegmentLogic to exactly follow the head
        /// </summary>
        private List<Vector2> segmentPoints = new List<Vector2>();

        /// <summary>
        /// Updates the positions of the segments based on the value set in SegmentFollowType
        /// </summary>
        public void UpdateSegments()
        {
            Projectile.position += Projectile.velocity; //Update this segment's movement so that all other segments use this as the base. This is undone at the end of the method.
            if (ActiveAnimation is not null)
            {
                ActiveAnimation.ApplyAnimationFrame(Projectile, AnimationFrame);
                AnimationFrame++;
                if (AnimationFrame > ActiveAnimation.AnimationKeyframes.Keys.Max())
                {
                    AnimationFrame = 0;
                    ActiveAnimation = null;
                }
            }
            else
            {
                if (segmentPoints.Count < 1 || segmentPoints[0].Distance(Projectile.Center) > 8)
                {
                    segmentPoints.Insert(0, Projectile.Center);
                }
                while (segmentPoints.Count > 300)
                {
                    segmentPoints.RemoveAt(segmentPoints.Count - 1);
                }
                switch (SegmentFollowType)
                {
                    case (SegmentFollowLogic.Regular):
                        RegularSegmentLogic();
                        break;
                    case (SegmentFollowLogic.Exact):
                        ExactSegmentLogic();
                        break;
                }
            }
            Projectile.position -= Projectile.velocity;

        }

        private void RegularSegmentLogic()
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                float segmentDistance = SegmentTypePositionOffsets[0];
                var thisSeg = Segments[i];
                var aheadSeg = new BaseWormSegment(this);
                if (i != 0)
                {
                    aheadSeg = Segments[i - 1];
                    segmentDistance = SegmentTypePositionOffsets[Segments[i - 1].segmentType + 1];
                }
                segmentDistance *= Projectile.scale;
                Vector2 nexSegDir = aheadSeg.Center - thisSeg.Center;
                if (aheadSeg.rotation != thisSeg.rotation)
                {
                    nexSegDir = nexSegDir.RotatedBy(MathHelper.WrapAngle(aheadSeg.rotation - thisSeg.rotation) * SegmentRigidity);
                    nexSegDir = nexSegDir.MoveTowards((aheadSeg.rotation - thisSeg.rotation).ToRotationVector2(), 1f);
                }
                thisSeg.rotation = nexSegDir.ToRotation() + MathHelper.PiOver2;
                float angledif = MathHelper.WrapAngle(thisSeg.rotation - aheadSeg.rotation);
                thisSeg.rotation = thisSeg.rotation.AngleLerp(aheadSeg.rotation + MathHelper.Clamp(angledif, -SegmentMaxRotation * 0.5f, SegmentMaxRotation * 0.5f), 0.25f);
                thisSeg.Center = aheadSeg.Center - (thisSeg.rotation - MathHelper.PiOver2).ToRotationVector2() * segmentDistance;

            }
        }

        private void ExactSegmentLogic()
        {
            float dist = 40f;
            int segmentPointInUse = 0;
            for (int i = 0; i < Segments.Count; i++)
            {
                var thisSeg = Segments[i];
                var aheadSeg = new BaseWormSegment(this);
                if (i != 0)
                    aheadSeg = Segments[i - 1];
                bool hasMoved = false;
                while (segmentPointInUse < segmentPoints.Count)
                {
                    if (segmentPointInUse == 0)
                    {
                        if (aheadSeg.Center.Distance(segmentPoints[0]) >= dist)
                        {
                            thisSeg.Center = aheadSeg.Center + aheadSeg.Center.DirectionTo(segmentPoints[0]) * dist;
                            Segments[i].velocity = Segments[i].Center.DirectionTo(aheadSeg.Center);
                            Segments[i].rotation = Segments[i].velocity.ToRotation() + MathHelper.PiOver2;
                            hasMoved = true;
                            break;
                        }
                        else
                        {
                            segmentPointInUse++;
                        }
                    }
                    else
                    {
                        if (aheadSeg.Center.Distance(segmentPoints[segmentPointInUse]) >= dist)
                        {
                            thisSeg.Center = aheadSeg.Center + aheadSeg.Center.DirectionTo(segmentPoints[segmentPointInUse]) * dist;
                            Segments[i].velocity = Segments[i].Center.DirectionTo(aheadSeg.Center);
                            Segments[i].rotation = Segments[i].velocity.ToRotation() + MathHelper.PiOver2;
                            hasMoved = true;
                            break;
                        }
                        else
                        {
                            segmentPointInUse++;
                        }
                    }
                }
                if (!hasMoved && segmentPointInUse >= segmentPoints.Count)
                {
                    thisSeg.Center = segmentPoints[segmentPoints.Count - 1];
                    Segments[i].velocity = Segments[i].Center.DirectionTo(aheadSeg.Center);
                    Segments[i].rotation = Segments[i].velocity.ToRotation() + MathHelper.PiOver2;
                    hasMoved = true;
                    break;
                }
            }
        }
        #endregion

        #region Defaults
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600; //100 tiles offscreen
        }
        public override void SetDefaults()
        {
            for (var i = 0; i < SegmentCount - 1; i++)
            {
                Segments.Add(new BaseWormSegment(this, 0));
            }
            Segments.Add(new BaseWormSegment(this, 1));
        }
        #endregion

        #region Draw
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {

            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                DrawSegment(ref lightColor, Segments[i]);
            }
            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() / 2, Projectile.scale, SpriteEffects.None, 1);

            if (GlowTextures.Count > 0 && GlowTextures[0] is not null)
                Main.spriteBatch.Draw(GlowTextureAssets[0].Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, GlowTextureAssets[0].Size() / 2, Projectile.scale, SpriteEffects.None, 1);
            return false;
        }

        public virtual void DrawSegment(ref Color lightColor, BaseWormSegment segment)
        {
            var color = Lighting.GetColor(segment.Center.ToTileCoordinates());
            if (!SegmentTextureAssets.IndexInRange(segment.segmentType))
            {
                return;
            }
            var tex = SegmentTextureAssets[segment.segmentType].Value;
            Main.spriteBatch.Draw(tex, segment.Center - Main.screenPosition, null, color *segment.Opacity, segment.rotation, tex.Size() / 2 + (SegmentTypeDrawOffsets[segment.segmentType]), Projectile.scale, SpriteEffects.None, 1);
            if (GlowTextures is null || !GlowTextures.IndexInRange(segment.segmentType + 1) || GlowTextures[segment.segmentType + 1] is null)
            {
                return;
            }
            tex = GlowTextureAssets[segment.segmentType + 1].Value;
            Main.spriteBatch.Draw(tex, segment.Center - Main.screenPosition, null, Color.White * segment.Opacity, segment.rotation, tex.Size() / 2 + (SegmentTypeDrawOffsets[segment.segmentType]), Projectile.scale, SpriteEffects.None, 1);
        }
        #endregion

        #region Netcode

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(AnimationFrame);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AnimationFrame = reader.ReadSingle();
        }
        #endregion

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (var i = 0; i < Segments.Count; i++)
            {
                var prevSeg = new BaseWormSegment(this);
                if (i != 0)
                {
                    prevSeg = Segments[i - 1];
                }
                float cpoint = 0;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), prevSeg.Center, Segments[i].Center, 16, ref cpoint))
                {
                    return true;
                }
            }
            return base.Colliding(projHitbox, targetHitbox);
        }
    }
}
