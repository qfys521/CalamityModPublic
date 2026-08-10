using System;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.BaseProjectiles
{
    /// <summary>
    /// An abstract class that contains the necessary code for a minion to work.<br/>
    /// Contains useful properties for minions, such as IFrames, Enemy Distance Detection, and sets the Owner and Target.
    /// Also has a <see cref="DoAnimation()"/> hook, with basic animation code that can be overriden.
    /// </summary>
    public abstract class BaseMinionProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        #region Properties

        /// <summary>
        /// Input here the correspondent <see cref="ModContent.ProjectileType{T}"/> of the minion.
        /// </summary>
        public abstract int AssociatedProjectileTypeID { get; }

        /// <summary>
        /// Input here the correspondent <see cref="ModContent.BuffType{T}"/> of the minion.
        /// </summary>
        public abstract int AssociatedBuffTypeID { get; }

        /// <summary>
        /// Input the reference of the minion bool from <see cref="CalamityPlayer" /> with <see cref="ModdedOwner"/>.<br/>
        /// Example for Elemental Axe:
        /// <code>public override ref bool AssociatedMinionBool => ref ModdedOwner.eAxe;</code>
        /// </summary>
        public abstract ref bool AssociatedMinionBool { get; }

        /// <summary>
        /// The amount of minion slots this summon consumes.<br/>
        /// Defaults to 1f.
        /// </summary>
        public virtual float MinionSlots => 1f;

        /// <summary>
        /// The max distance in which the minion can detect an enemy, in pixels.<br/>
        /// <see cref="ProjectileID.Sets.DrawScreenCheckFluff"/> is set to this value.<br/>
        /// Defaults to 1200f (75 tiles).
        /// </summary>
        public virtual float EnemyDistanceDetection => 1200f;

        /// <summary>
        /// The min distance in which the minion can detect an enemy before it goes to its correspondent enemy distance detection.
        /// Defaults to 960f (60 tiles), the radius of a 1080p monitor at max zoom.
        /// </summary>
        public virtual float MinEnemyDistanceDetection => 960f;

        private float AdaptiveEnemyDistanceDetection => Target == null ? MinEnemyDistanceDetection : EnemyDistanceDetection;

        /// <summary>
        /// The amount of local I-Frames this minion has.<br/>
        /// Multiplied by <see cref="Projectile.MaxUpdates"/> so changing the updates won't affect this.<br/>
        /// Defaults to 10.
        /// </summary>
        public int IFrames { get; set; } = 10;

        /// <summary>
        /// If <see langword="true"/>, makes the minion only be able to detect and attack enemies through tiles only when there are any bosses alive.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public virtual bool PreHardmodeMinionTileVision => false;

        /// <summary>
        /// If <see langword="false"/>, makes the minion not target certain NPCs (Like Abyss' or Sunken Sea's enemies) until they're hit.<br/>
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public virtual bool PreventTargettingUntilTargetHit => true;

        /// <summary>
        /// If <see langword="true"/>, the minion will be considered grounded, this will turn on some features that are neccesary for grounded minions.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public virtual bool Grounded => false;

        /// <summary>
        /// The speed at which the minion falls.<br/>
        /// It'll take the absolute value of whatever you input, to ensure that it's on Terraria's coordinate system.<br/>
        /// This is intended to use for grounded minions.<br/>
        /// Defaults to 0.4f.
        /// </summary>
        public float Gravity
        {
            get => _gravity;
            set => _gravity = MathF.Abs(value);
        }
        private float _gravity = 0.4f;

        /// <summary>
        /// The max speed at which the minion falls.<br/>
        /// It'll take the absolute value of whatever you input, to ensure that it's on Terraria's coordinate system.<br/>
        /// This is intended to use for grounded minions.<br/>
        /// Defaults to 20f.
        /// </summary>
        public float MaxGravity
        {
            get => _maxGravity;
            set => _maxGravity = MathF.Abs(value);
        }
        private float _maxGravity = 20f;

        /// <summary>
        /// The amount of animation frames this minion has.<br/>
        /// Defaults to 1 frame.
        /// </summary>
        public virtual int AnimationFrames => 1;

        /// <summary>
        /// The frames that it takes to go to the next frame of animation.<br/>
        /// Defaults to 5.
        /// </summary>
        public int FramesUntilNextAnimationFrame { get; set; } = 5;

        /// <summary>
        /// Set here <see cref="ProjectileID.Sets.TrailingMode"/>.<br/>
        /// Defaults to 2.
        /// </summary>
        public int TrailingMode { get; set; } = 2;

        /// <summary>
        /// Set here <see cref="ProjectileID.Sets.TrailCacheLength"/>.<br/>
        /// Defaults to 5.<br/>
        /// </summary>
        public int TrailCacheLength { get; set; } = 5;

        public Player Owner { get; set; }
        public CalamityPlayer ModdedOwner { get; set; }
        public NPC Target { get; set; }

        #endregion

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = AnimationFrames;
            // IMPORTANT: Setting Main.projPet disables projectile damage! If you are making a charging minion, be sure to override the MinionContactDamage hook
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = TrailingMode;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailCacheLength;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = (int)EnemyDistanceDetection;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = MinionSlots;
            Projectile.penetrate = -1;

            Projectile.friendly = true;
            Projectile.tileCollide = Grounded;
            Projectile.ignoreWater = true;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.netImportant = true;
        }

        public override bool PreAI()
        {
            Projectile.Calamity().overridesMinionDamagePrevention = !PreventTargettingUntilTargetHit;
            return true;
        }

        public override void AI()
        {
            Projectile.localNPCHitCooldown = IFrames * Projectile.MaxUpdates;
            SetOwnerTarget();
            CheckMinionExistence();
            DoAnimation();
            MinionAI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        #region AI Methods

        /// <summary>
        /// Where all of the actual AI of the minion will be placed.
        /// </summary>
        public abstract void MinionAI();

        /// <summary>
        /// The universal way all minions check if they can still exist.
        /// </summary>
        public virtual void CheckMinionExistence()
        {
            Owner.AddBuff(AssociatedBuffTypeID, 2);
            if (Type != AssociatedProjectileTypeID)
                return;

            if (Owner.dead)
                AssociatedMinionBool = false;
            if (AssociatedMinionBool)
                Projectile.timeLeft = 2;
        }

        /// <summary>
        /// Basic animation code that the majority of minions use.<br/>
        /// It will only run if the minion is set to have more than 1 frame of animation.<br/>
        /// If something more complex is needed, override it. Or leave it blank if not needed at all.
        /// </summary>
        public virtual void DoAnimation()
        {
            if (Main.projFrames[Type] <= 1)
                return;

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FramesUntilNextAnimationFrame * Projectile.MaxUpdates)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }
        }

        /// <summary>
        /// Where the null property <see cref="Target"/>, <see cref="Owner"/> and <see cref="ModdedOwner"/> is set to a non-null value.
        /// </summary>
        public virtual void SetOwnerTarget()
        {
            Owner = Main.player[Projectile.owner];
            ModdedOwner = Owner.Calamity();
            Target = Owner.Center.MinionHoming(AdaptiveEnemyDistanceDetection, Owner, !PreHardmodeMinionTileVision || (CalamityPlayer.areThereAnyDamnBosses && !Grounded));

            // When minions are grounded, they are affected by tiles, meaning they have a much limited movement.
            // Pre-Hardmode minions are affected the most since they can't see through walls.
            // In consequence, some targets that may seem valid, aren't detected due to this.
            // If the minion hasn't detected any targets with the center being the player, we give them a second chance with the center being the minion itself.
            // If there are bosses on the world, we don't do this check because if there wasn't any targets with tile X-Ray, this won't do anything.
            if (Grounded && Target is null && PreHardmodeMinionTileVision)
                Target = Projectile.Center.MinionHoming(AdaptiveEnemyDistanceDetection, Owner, false);
        }

        #endregion

        #region Grounded Minion Utilities

        /// <summary>
        /// A quick method to apply gravity to grounded minions.<br/>
        /// You can choose to not use it and make your own if a simple one does not fit your need.
        /// </summary>
        public void DoGravity()
        {
            float speedY = Projectile.velocity.Y;
            if (speedY < _maxGravity)
                speedY = MathF.Min(speedY + _gravity, _maxGravity);
            Projectile.velocity.Y = speedY;
        }

        /// <summary>
        /// Checks whether there's a tile between two vectors vertically.<br/>
        /// This method has many shorthand variants.<br/>
        /// Useful for grounded minion behavior.
        /// </summary>
        /// <param name="startVector">The starter vector. Its X component will be the one used, so put here the vector from where you want to check.</param>
        /// <param name="endVector">The ending vector.</param>
        /// <param name="platformCheckDownwards">Whether you want to check for platforms when <paramref name="endVector"/> is below <paramref name="startVector"/></param>
        /// <returns>Returns <see langword="true"/> if there's a tile between <paramref name="startVector"/> and <paramref name="endVector"/>, the X coordinate being the one from <paramref name="startVector"/>; otherwise <see langword="false"/>.</returns>
        public bool IsTileBetweenTwoVectorsVertically(Vector2 startVector, Vector2 endVector, bool platformCheckDownwards = false)
        {
            Point startPoint = CalamityUtils.ToSafeTileCoordinates(startVector);
            Point endPoint = CalamityUtils.ToSafeTileCoordinates(endVector);

            if (endVector.Y > startVector.Y)
            {
                for (int coordY = endPoint.Y; coordY >= startPoint.Y; coordY--)
                {
                    if (Main.tile[startPoint.X, coordY].IsTileSolidGround())
                        return true;
                }
            }
            else
            {
                for (int coordY = endPoint.Y; coordY <= startPoint.Y; coordY++)
                {
                    if (platformCheckDownwards)
                    {
                        if (Main.tile[startPoint.X, coordY].IsTileSolidGround())
                            return true;
                    }
                    else if (Main.tile[startPoint.X, coordY].IsTileSolid())
                        return true;
                }
            }

            return false;
        }

        public bool IsTileBetweenOwnerAndVectorVertically(Vector2 vector, bool platformCheckDownwards = false) => IsTileBetweenTwoVectorsVertically(Owner.Center, vector, platformCheckDownwards);

        public bool IsTileBetweenTargetAndVectorVertically(Vector2 vector, bool platformCheckDownwards = false) => IsTileBetweenTwoVectorsVertically(Target.Center, vector, platformCheckDownwards);

        public bool IsTileBetweenVectorAndMinionVertically(Vector2 vector, bool platformCheckDownwards = false) => IsTileBetweenTwoVectorsVertically(vector, Projectile.Center, platformCheckDownwards);

        public bool IsTileBetweenOwnerAndMinionVertically(bool platformCheckDownwards = false) => IsTileBetweenTwoVectorsVertically(Owner.Center, Projectile.Center, platformCheckDownwards);

        public bool IsTileBetweenTargetAndMinionVertically(bool platformCheckDownwards = false) => IsTileBetweenTwoVectorsVertically(Target.Center, Projectile.Center, platformCheckDownwards);

        public bool IsMinionFacingTile()
        {
            Rectangle inflatedMinionHitbox = Projectile.getRect();
            inflatedMinionHitbox.Inflate(1, 0);
            bool stuckOnLeftTile = Main.tile[new Vector2(inflatedMinionHitbox.X, inflatedMinionHitbox.Y + inflatedMinionHitbox.Height - 17).ToSafeTileCoordinates()].IsTileSolid();
            bool stuckOnRightTile = Main.tile[new Vector2(inflatedMinionHitbox.X + inflatedMinionHitbox.Width, inflatedMinionHitbox.Y + inflatedMinionHitbox.Height - 17).ToSafeTileCoordinates()].IsTileSolid();
            return stuckOnLeftTile || stuckOnRightTile;
        }

        #endregion
    }
}
