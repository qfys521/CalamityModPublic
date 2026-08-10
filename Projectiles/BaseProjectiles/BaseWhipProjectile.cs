using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.BaseProjectiles
{
    /// <summary>
    /// Base class for a whip that handles drawing, AI, and onHit. To make a simple whip, you only need to specify stats in setWhipStats
    /// This was originally based off the ExampleMod implementation, but with several changes for ease of subclassing
    /// </summary>
    public abstract class BaseWhipProjectile : ModProjectile
    {

        #region Overridable Properties

        
        //Visual and SFX related variables
        
        /// <summary>
        /// Color of the line connecting whip segments
        /// </summary>
        public virtual Color FishingLineColor => Color.White;
        
        /// <summary>
        /// Overrides the color the whips is drawn as. If null, use surrounding tile lighting
        /// </summary>
        public virtual Color? DrawColor => null;
        
        /// <summary>
        /// Color of light the whip emits
        /// </summary>
        public virtual Color LightingColor => Color.Transparent;
        
        /// <summary>
        /// Type of dust (ID) the whip emits, emits no dust if it is null
        /// </summary>
        public virtual int? SwingDust => null;
        
        /// <summary>
        /// How many times the dust spawning loop iterates
        /// </summary>
        public virtual int DustAmount => 1;
        /// <summary>
        /// If true, the tip of the whip kind of fades away when completing it's arc
        /// </summary>
        public virtual bool ShrinkTip => true;
        public virtual bool UseTimeDetermineLifetime => true;
        public virtual SoundStyle? WhipCrackSound => SoundID.Item153;


        
        //Textures are set to InvisibleProj by default, but this should be changed if you want it to be visible.
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public abstract string WhipTipTexture { get;}
        /// <summary>
        /// The order of textures in this list is the order which they will appear when drawn in game.
        /// </summary>
        public abstract List<string> WhipSegmentTexture { get;}

        /// <summary>
        /// UNIMPLEMENTED
        /// The Y offset of each whip segment texture, in order of appearance. 
        /// Length should match WhipSegmentTexture, or be left null.
        /// When null, all offsets are set to 0.
        /// Defaults to null.
        /// </summary>
        public virtual List<string> WhipSegmentYOffset => null;
        public abstract string WhipHandleTexture { get; }
        public virtual string WhipTipGlowTexture => null;
        public virtual List<string> WhipSegmentGlowTexture => null; 
        public virtual string WhipHandleGlowTexture => null;


        //Tag related variables
        public virtual int? TagBuffID => null;
        public virtual int TagDuration => 240;
        
        
        //Gameplay variables
        /// <summary>
        /// Multiply the whip damage by this amount on hitting an enemy
        /// </summary>
        public virtual float? MultihitModifier => .8f;

        #endregion

        internal List<Vector2> whipPoints = new List<Vector2>();
        internal float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public int PointDrawTimer = 0;
        public float lineScale = 1;
        public int lineCount = 1;
        
        internal Vector2? GetTipPosition()
        {
            
            if (whipPoints != null && whipPoints.Count > 2)
                return whipPoints[whipPoints.Count - 1];
            return null;
        }



        #region ModProjectile Functions

        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.>
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            

            SetWhipStats();
        }

        
        public override bool PreAI()
        {
            if (PointDrawTimer % 2 < .001)
            {
                whipPoints.Clear();
                Projectile.FillWhipControlPoints(Projectile, whipPoints);
            }
            return true;
        }
        
        public override void AI()
        {
            WhipAIMotion();
            WhipSFX(LightingColor, SwingDust, DustAmount, WhipCrackSound);
        }

        #endregion


        #region Virtual Functions

                /// <summary>
        /// Function is use to control custom whip stats, called in the parent class's set defaults
        /// </summary>
        public virtual void SetWhipStats()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.WhipSettings.Segments = 30;
            Projectile.WhipSettings.RangeMultiplier = 1f;
        }
        

        // This method draws a line between all points of the whip, in case there's empty space between the sprites.

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return DrawWhip(FishingLineColor);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            WhipOnHit(target);
        }

        /// <summary>
        /// Applies tag buff if there is one, applies multihit penalty, and focuses minions on target. 
        /// Called in OnHitNPC
        /// </summary>
        /// <param name="target"></param>
        public virtual void WhipOnHit(NPC target)
        {
            if (TagBuffID != null)
            {
                target.AddBuff((int)TagBuffID, TagDuration);
            }
            Projectile.damage = (int)(Projectile.damage * MultihitModifier);
            if (Projectile.damage < 1)
            {
                Projectile.damage = 1;
            }
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }

        /// <summary>
        /// Draws whip based on example mod, override if you want custom. 
        /// Called in PreDraw
        /// </summary>
        /// <param name="lineColor"> What color the fishing line is</param>
        /// <returns></returns>
        public virtual bool DrawWhip(Color lineColor)
        {
            //Gets every segment of the whip
            if (whipPoints == null || whipPoints.Count < 1)
                return false;

            for (int i = 0; i < lineCount; i++)
                CalamityUtils.DrawLineBetweenPoints(whipPoints, lineColor, scaleMod: lineScale - (lineScale / lineCount) * i);

            SpriteEffects flip = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Main.instance.LoadProjectile(Type);

            //Load projectiles using file paths
            Texture2D texture = ModContent.Request<Texture2D>(WhipHandleTexture).Value; ;
            Texture2D glowtexture = texture;
            bool drawGlow = false;
            if (WhipHandleGlowTexture != null)
            {
                glowtexture = ModContent.Request<Texture2D>(WhipHandleGlowTexture).Value;
                drawGlow = true;
            }

            //Sets the frame which will be displayed
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = sourceRectangle.Size() / 2f;

            


            Vector2 pos = whipPoints[0];

            //Repeats for each whip point
            for (int i = 0; i < whipPoints.Count; i++)
            {
                float scale = 1;

                //Tip of the whip
                if (i == whipPoints.Count - 1)
                {
                    //Sets image to tip texture
                    texture = ModContent.Request<Texture2D>(WhipTipTexture).Value;

                    //Moves the frame with the animation
                    sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                    origin = sourceRectangle.Size() / 2f;
                    origin.Y += DrawOriginOffsetY * Projectile.spriteDirection;
                    origin.X += DrawOriginOffsetX;

                    drawGlow = false;
                    if (WhipTipGlowTexture != null)
                    {
                        glowtexture = ModContent.Request<Texture2D>(WhipTipGlowTexture).Value;
                        drawGlow = true;
                    }

                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.

                    if (ShrinkTip)
                    {
                        Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                        float t = Timer / timeToFlyOut;
                        scale = MathHelper.Lerp(0.5f, 1.5f,
                            Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true)) * Projectile.scale;
                    }
                }
                else if (i > 0)
                {
                    
                    //Sets image to segment texture
                    texture = ModContent.Request<Texture2D>(WhipSegmentTexture[i%WhipSegmentTexture.Count]).Value;
                    //sets the frame accordingly
                    sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                    origin = sourceRectangle.Size() / 2f;
                    drawGlow = false;
                    if (WhipSegmentGlowTexture != null)
                    {
                        glowtexture = ModContent.Request<Texture2D>(WhipSegmentGlowTexture[i % WhipSegmentTexture.Count]).Value;
                        drawGlow = true;
                    }

                }
                Vector2 element = whipPoints[i];
                Vector2 diff = (i == whipPoints.Count - 1 ? element- whipPoints[i-1] : whipPoints[i + 1] - element);

                float rotation = diff.ToRotation();
                //Rotate the handle
                if (i == 0)
                {
                    //diff.toRotation makes it follow the rotation of the whip anim
                    //MathHelper.Pi can be subtracted / added to adjust where the handle is
                    //Use multiplication as needed to tweak that
                    rotation = diff.ToRotation();
                }
                
                Color color = Lighting.GetColor(element.ToTileCoordinates());
                if (DrawColor != null) {
                
                    color = (Color)DrawColor;
                }

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, flip, 0);
                if (drawGlow)
                {
                    Main.EntitySpriteDraw(glowtexture, pos - Main.screenPosition, sourceRectangle, Color.White, rotation, origin, scale, flip, 0);
                }
                pos += diff;
            }
            return false;
        }

        bool runOnce = true;

        /// <summary>
        /// Runs whip AI similar to example mod, but the center is now on the whip tip. Called in AI
        /// </summary>
        public virtual void WhipAIMotion()
        {
            Player owner = Main.player[Projectile.owner];
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (runOnce)
            {
                Projectile.WhipSettings.Segments = (int)((owner.whipRangeMultiplier + 1) * Projectile.WhipSettings.Segments);
                runOnce = false;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

            Projectile.Center = Vector2.Lerp(Projectile.Center, whipPoints[whipPoints.Count - 1], 1);

            // Vanilla uses Vector2.Dot(Projectile.velocity, Vector2.UnitX) here. Dot Product returns the difference between two vectors, 0 meaning they are perpendicular.
            // However, the use of UnitX basically turns it into a more complicated way of checking if the projectile's velocity is above or equal to zero on the X axis.
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            Timer++;
            PointDrawTimer++;

            if (Timer >= swingTime || (UseTimeDetermineLifetime && owner.itemAnimation <= 0))
            {

                Projectile.Kill();
                return;
            }
        }

        /// <summary>
        /// Plays sound and runs dust, all the parameters should be set in whip stats, though you can override them. 
        /// Called in AI
        /// </summary>
        /// <param name="lightingCol"></param>
        /// <param name="dustID"></param>
        /// <param name="dustNum"></param>
        /// <param name="sound"></param>
        public virtual void WhipSFX(Color lightingCol, int? dustID, int dustNum, SoundStyle? sound)
        {
            Player owner = Main.player[Projectile.owner];
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            //Main.NewText(lightingCol);



            owner.heldProj = Projectile.whoAmI;
            Vector2? tip = GetTipPosition();
            
            if(tip is null)
                return;
            if (Timer == swingTime / 2 && sound != null)
            {
                // Plays a whipcrack sound at the tip of the whip.
                SoundEngine.PlaySound(sound, tip);

            }
            if ((Timer >= swingTime * .45f) && Timer <= swingTime * 0.85f)
            {
                if (dustID != null)
                {
                    for (int i = 0; i < dustNum; i++)
                    {
                        Dust.NewDust((Vector2)tip, 2, 2, (int)dustID, 0, 0, Scale: .5f);
                    }
                }
                if (lightingCol != Color.Transparent)
                {
                    Lighting.AddLight((Vector2)tip, lightingCol.R / 255f, lightingCol.G / 255f, lightingCol.B / 255f);
                }

            }
        }

        #endregion
        
        

        
    }
}
