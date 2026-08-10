using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.BaseProjectiles
{
	// Adapted from ExampleMod's ExampleAdvancedFlailProjectile (which in turn also adapted from Vanilla's AI Style 15)
	public abstract class BaseMaceFlailProjectile : ModProjectile
	{
		public abstract int AssociatedItemID { get; }

		// The following can also be used in each individual projectile class, without the need to re-define:
		#region Utilities
		public enum FlailState
		{
			Spinning = 0,
			LaunchingForward = 1,
			Retracting = 2,
			ForcedRetracting = 4,
			Ricochet = 5,
			Dropping = 6
		}

		public FlailState CurrentFlailState
		{
			get => (FlailState)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}
		public ref float StateTimer => ref Projectile.ai[1];
		public ref float CollisionCounter => ref Projectile.localAI[0];
		// ai[2] and localAI[1] are completely open for additional gimmicks.

		public Player Owner => Main.player[Projectile.owner];
		#endregion

		// Core properties of the mace's function.
		#region Properties
		/// <summary>
		/// Local immunity frames granted while dropped or retracted<br/>
		/// Defaults to 10.
		/// </summary>
		public virtual int BaseIFrames => 10;

		/// <summary>
		/// Local immunity frames granted during the spin.<br/>
		/// Defaults to 20.
		/// </summary>
		public virtual int SpinIFrames => 20;

		/// <summary>
		/// Local immunity frames granted while launched or ricochet.<br/>
		/// Defaults to 10.
		/// </summary>
		public virtual int LaunchIFrames => 10;

		/// <summary>
		/// The rate of which the mace visually spins around the player. Does NOT influence any other aspects.<br/>
		/// Defaults to 10f.
		/// </summary>
		public virtual float SpinSpeed => 10f;

		/// <summary>
		/// Horizontal radius for the mace's hitbox while spinning.<br/>
		/// Defaults to 55f. (3.4375 tiles)
		/// </summary>
		public virtual float SpinHitboxRadius => 55f;

		/// </summary>
		/// The distance between the projectile and the player's centers while spinning.<br/>
		/// Defaults to 30f. (1.875 tiles)
		/// </summary>
		public virtual float SpinVisualRadius => 30f;

		/// <summary>
		/// Effect which runs as soon as a spinning mace launches. Does nothing by default.
		/// </summary>
		public virtual Action<Projectile> EffectBeforeLaunch => null;

		/// <summary>
		/// Max amount of afterimages, which is only drawn while the mace is launched.<br/>
		/// Defaults to 6.
		/// </summary>
		public virtual int AfterimageLength => 6;

		/// <summary>
		/// Constant speed when launched, affected by attack speed.<br/>
		/// Defaults to 16f.
		/// </summary>
		public virtual float LaunchSpeed { get; set; } = 16f;

		/// <summary>
		/// How long the mace launches before retracting. Multiply by its speed to determine its base range.<br/>
		/// Defaults to 15. (0.25 seconds)
		/// </summary>
		public virtual int LaunchLifespan => 15;

		/// <summary>
		/// Max launch range before forcibly being set to retract.<br/>
		/// Typically not useful as it should retract naturally through lifespan before doing so by force.<br/>
		/// Defaults to 960f. (60 tiles)
		/// </summary>
		public virtual float MaxLaunchRange => 960f;

		/// <summary>
		/// Max drop range before forcibly being set to retract.<br/>
		/// Defaults to 400f. (25 tiles)
		/// </summary>
		public virtual float MaxDropRange => 400f;

		/// <summary>
		/// Effect which runs as soon as a launched mace retracts. Does nothing by default.
		/// </summary>
		public virtual Action<Projectile> EffectBeforePullback => null;

		/// <summary>
		/// When hitting a tile while launched, how long the ricochet lasts before dropping.
		/// Defaults to the launch's lifespan added by 5. (20 / 0.33 seconds)
		/// </summary>
		public virtual int RicochetLifespan => LaunchLifespan + 5;

		/// <summary>
		/// Max speed when retracted, affected by attack speed.<br/>
		/// Defaults to 15f.
		/// </summary>
		public virtual float MaxRetractSpeed { get; set; } = 15f;
		
		/// <summary>
		/// Acceleration when retracted, affected by attack speed.<br/>
		/// Defaults to 2.5f.
		/// </summary>
		public virtual float RetractAcceleration { get; set; } = 2.5f;
		#endregion

		// Most of these should be kept default for consistency with vanilla maces.
		// However, they can be overridden anyway.
		#region Overridable Values
		/// <summary>
		/// File path for the mace's chain. These should just be the name of the projectile with "Chain" appended at the end.<br/>
		/// Custom maces may also elect to not draw chains. Simply override this value with string.Empty.
		/// </summary>
		public virtual string ChainTexturePath => Texture + "Chain";

		/// <summary>
		/// Damage multiplier while spinning. Defaults to 1.2f.
		/// </summary>
		public virtual float SpinDamage => 1.2f;

		/// <summary>
		/// Knockback multiplier while spinning. Defaults to 0.25f.
		/// </summary>
		public virtual float SpinKnockback => 0.25f;

		/// <summary>
		/// The vertical multiplier which makes the spin motion elliptical. Defaults to 0.8f.
		/// </summary>
		public virtual float SpinVerticalFactor => 0.8f;

		/// <summary>
		/// Damage multiplier while launching or non-forced retracting. Defaults to 2f.
		/// </summary>
		public virtual float LaunchDamage => 2f;

		/// <summary>
		/// Knockback multiplier while dropping. Defaults to 0.5f.
		/// </summary>
		public virtual float DropKnockback => 0.5f;
		#endregion

		// Methods for most individual behaviour steps which can be useful to override.
		#region Overridable Methods
		/// <summary>
		/// Scales every value that should be affected by melee speed.<br/>
		/// Can be overridden to leave out certain variables.
		/// </summary>
		public virtual void ScaleWithMeleeSpeed(ref float launchSpeed, ref float maxSpeed, ref float acceleration)
		{
			float meleeSpeedMultiplier = Owner.GetTotalAttackSpeed(DamageClass.Melee);
			launchSpeed *= meleeSpeedMultiplier;
			maxSpeed *= meleeSpeedMultiplier;
			acceleration *= meleeSpeedMultiplier;
		}

		/// <summary>
		/// Calculates the damage and knockback multiplier based on the current behaviour.<br/>
		/// Can be overridden to include any extra stat adjustments.
		/// </summary>
		public virtual void UpdateDamageKB(out float damageMult, out float kbMult)
		{
			damageMult = 1f;
			kbMult = 1f;
			if (CurrentFlailState == FlailState.Spinning)
			{
				damageMult = SpinDamage;
				kbMult = SpinKnockback;
			}
			else if (CurrentFlailState == FlailState.LaunchingForward || CurrentFlailState == FlailState.Retracting)
				damageMult = LaunchDamage;
			else if (CurrentFlailState == FlailState.Dropping)
				kbMult = DropKnockback;
		}

		public virtual void SpinAI(float launchSpeed)
		{
			Projectile.ownerHitCheck = true;
			if (Projectile.owner == Main.myPlayer)
			{
				// 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
				Vector2 toMouse = Owner.MountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * Owner.direction);
				Owner.ChangeDir((toMouse.X > 0f).ToDirectionInt());
				if (!Owner.channel)
				{
					CurrentFlailState = FlailState.LaunchingForward;
					StateTimer = 0f;
					Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);
					Projectile.velocity = toMouse * launchSpeed;
					Projectile.netUpdate = true;
					Projectile.ResetLocalNPCHitImmunity();
					Projectile.localNPCHitCooldown = LaunchIFrames * Projectile.MaxUpdates;
					Projectile.ownerHitCheck = false;
					EffectBeforeLaunch?.Invoke(Projectile);
					return;
				}
			}
			StateTimer++;
			Vector2 spinOffset = new Vector2(Owner.direction).RotatedBy(MathHelper.Pi * SpinSpeed * (StateTimer / 60f) * Owner.direction);

			spinOffset.Y *= SpinVerticalFactor;
			if (spinOffset.Y * Owner.gravDir > 0f)
				spinOffset.Y *= 0.5f;

			Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter + spinOffset * SpinVisualRadius);
			Projectile.velocity = Vector2.Zero;
			Projectile.localNPCHitCooldown = SpinIFrames * Projectile.MaxUpdates;
		}

		public virtual void LaunchAI()
		{
			bool shouldRetract = StateTimer++ >= LaunchLifespan;
			shouldRetract |= Projectile.Distance(Owner.MountedCenter) >= MaxLaunchRange;
			if (Owner.controlUseItem) // If the player clicks, transition to the Dropping state
			{
				CurrentFlailState = FlailState.Dropping;
				StateTimer = 0f;
				Projectile.netUpdate = true;
				Projectile.velocity *= 0.2f;
				return;
			}
			if (shouldRetract)
			{
				CurrentFlailState = FlailState.Retracting;
				StateTimer = 0f;
				Projectile.netUpdate = true;
				Projectile.velocity *= 0.3f;
				EffectBeforePullback?.Invoke(Projectile);
			}
			Owner.ChangeDir((Owner.Center.X < Projectile.Center.X).ToDirectionInt());
			Projectile.localNPCHitCooldown = LaunchIFrames * Projectile.MaxUpdates;
		}

		public virtual void RetractAI(bool forced, float maxSpeed, float acceleration)
		{
			if (forced)
			{
				Projectile.tileCollide = false;
				Projectile.ignoreWater = true;
			}

			float forceMult = forced ? 2f : 1f;
			Vector2 toPlayer = Projectile.SafeDirectionTo(Owner.MountedCenter);
			Vector2 value = Owner.MountedCenter.DirectionFrom(Projectile.Center + Projectile.velocity).SafeNormalize(Vector2.Zero);
			if (Projectile.Distance(Owner.MountedCenter) <= maxSpeed * forceMult || (forced && Vector2.Dot(toPlayer, value) < 0f))
			{
				Projectile.Kill();
				return;
			}
			if (Owner.controlUseItem && !forced) // If the player clicks, transition to the Dropping state
			{
				CurrentFlailState = FlailState.Dropping;
				StateTimer = 0f;
				Projectile.netUpdate = true;
				Projectile.velocity *= 0.2f;
				return;
			}

			Projectile.velocity *= 0.98f;
			Projectile.velocity = Projectile.velocity.MoveTowards(toPlayer * maxSpeed * forceMult, acceleration * forceMult);
			Owner.ChangeDir((Owner.Center.X < Projectile.Center.X).ToDirectionInt());
		}

		public virtual void RicochetAI()
		{
			if (StateTimer++ >= RicochetLifespan)
			{
				CurrentFlailState = FlailState.Dropping;
				StateTimer = 0f;
				Projectile.netUpdate = true;
				return;
			}

			Projectile.localNPCHitCooldown = LaunchIFrames * Projectile.MaxUpdates;
			Projectile.velocity.Y += 0.6f;
			Projectile.velocity.X *= 0.95f;
			Owner.ChangeDir((Owner.Center.X < Projectile.Center.X).ToDirectionInt());
		}

		public virtual void DropAI()
		{
			if (!Owner.controlUseItem || Projectile.Distance(Owner.MountedCenter) > MaxDropRange)
			{
				CurrentFlailState = FlailState.ForcedRetracting;
				StateTimer = 0f;
				Projectile.netUpdate = true;
				EffectBeforePullback?.Invoke(Projectile);
				return;
			}

			Projectile.localNPCHitCooldown = BaseIFrames * Projectile.MaxUpdates;
			Projectile.velocity.Y += 0.8f;
			Projectile.velocity.X *= 0.95f;
			Owner.ChangeDir((Owner.Center.X < Projectile.Center.X).ToDirectionInt());
		}

		/// <summary>
		/// Reserved for extra AI that is always ran regardless of state. Does nothing by default.<br/>
		/// Example cases include shooting projectiles in a similar manner to Flower Pow.<br/>
		/// Return false to entirely override base flail behaviour.
		/// </summary>
		public virtual bool ExtraBehavior() => true;

		/// <summary>
		/// Handles simple chain drawing. Can be overridden for cases such as chains with multiple frames/variants.
		/// Example cases include Drippler Cripper's chain variants.
		/// </summary>
		public virtual void DrawChain()
		{
			Texture2D Chain = ModContent.Request<Texture2D>(ChainTexturePath).Value;
			Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile, Owner);

			// This fixes a vanilla GetPlayerArmPosition bug causing the chain to draw incorrectly when stepping up slopes. The flail itself still draws incorrectly due to another similar bug.
			// This should be removed once the vanilla bug is fixed.
			playerArmPosition.Y -= Owner.gfxOffY;

			Vector2 chainPos = Projectile.Center;
			Vector2 toArms = playerArmPosition.MoveTowards(chainPos, 4f) - chainPos;
			float chainSegmentLength = MathF.Max(1f, Chain.Height);
			float rotation = toArms.ToRotation() + MathHelper.PiOver2;

			float chainsLeft = toArms.Length() + chainSegmentLength * 0.5f;
			while (chainsLeft > 0f)
			{
				Color chainDrawColor = Lighting.GetColor((int)(chainPos.X / 16f), (int)(chainPos.Y / 16f));
				Main.spriteBatch.Draw(Chain, chainPos - Main.screenPosition, null, chainDrawColor, rotation, Chain.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

				chainPos += toArms.SafeNormalize(Vector2.Zero) * chainSegmentLength;
				chainsLeft -= chainSegmentLength;
			}
		}
		#endregion

		#region Hook Overrides
		public override LocalizedText DisplayName => CalamityUtils.GetItemName(AssociatedItemID);

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = AfterimageLength;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = BaseIFrames * Projectile.MaxUpdates;
		}

		public override void AI()
		{
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed || Vector2.Distance(Projectile.Center, Owner.Center) > MaxLaunchRange + 160f)
			{
				Projectile.Kill();
				return;
			}
			if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
			{
				Projectile.Kill();
				return;
			}

			float launchSpeed = LaunchSpeed;
			float maxRetractSpeed = MaxRetractSpeed;
			float retractAcceleration = RetractAcceleration;
			ScaleWithMeleeSpeed(ref launchSpeed, ref maxRetractSpeed, ref retractAcceleration);

			switch (CurrentFlailState)
			{
				case FlailState.Spinning:
					SpinAI(launchSpeed);
					break;
				case FlailState.LaunchingForward:
					LaunchAI();
					break;
				case FlailState.Retracting:
					RetractAI(false, maxRetractSpeed, retractAcceleration);
					break;
				case FlailState.ForcedRetracting:
					RetractAI(true, maxRetractSpeed, retractAcceleration);
					break;
				case FlailState.Ricochet:
					RicochetAI();
					break;
				case FlailState.Dropping:
					DropAI();
					break;
			}

			if (ExtraBehavior())
			{
				Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();

				// Non-symmetric rotation (symmetric is just in this if condition here)
				if (CurrentFlailState == FlailState.Ricochet || CurrentFlailState == FlailState.Dropping)
				{
					if (Projectile.velocity.Length() > 1f)
						Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi + Projectile.velocity.X * 0.1f;
					else
						Projectile.rotation += Projectile.velocity.X * 0.1f;
				}
				else
				{
					Vector2 vectorTowardsPlayer = Projectile.SafeDirectionTo(Owner.MountedCenter);
					Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.ToRadians(270f);
				}

				Projectile.timeLeft = 2;
				Owner.heldProj = Projectile.whoAmI;
				Owner.SetDummyItemTime(2);
				Owner.itemRotation = Projectile.DirectionFrom(Owner.MountedCenter).ToRotation();
				if (Projectile.Center.X < Owner.MountedCenter.X)
					Owner.itemRotation += MathHelper.Pi;
				Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			int impactIntensity = 0;
			Vector2 velocity = Projectile.velocity;
			float bounceFactor = 0.2f;
			if (CurrentFlailState == FlailState.LaunchingForward || CurrentFlailState == FlailState.Ricochet)
				bounceFactor = 0.4f;
			if (CurrentFlailState == FlailState.Dropping)
				bounceFactor = 0f;

			if (oldVelocity.X != Projectile.velocity.X)
			{
				if (Math.Abs(oldVelocity.X) > 4f)
					impactIntensity = 1;

				Projectile.velocity.X = oldVelocity.X * -bounceFactor;
				CollisionCounter++;
			}

			if (oldVelocity.Y != Projectile.velocity.Y)
			{
				if (Math.Abs(oldVelocity.Y) > 4f)
					impactIntensity = 1;

				Projectile.velocity.Y = oldVelocity.Y * -bounceFactor;
				CollisionCounter++;
			}

			if (CurrentFlailState == FlailState.LaunchingForward)
			{
				CurrentFlailState = FlailState.Ricochet;
				Projectile.localNPCHitCooldown = BaseIFrames * Projectile.MaxUpdates;
				Projectile.netUpdate = true;
				Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
				Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
				impactIntensity = 2;
				Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out bool causedShockwaves);
				Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
				Projectile.position -= velocity;
			}

			if (impactIntensity > 0)
			{
				Projectile.netUpdate = true;
				for (int i = 0; i < impactIntensity; i++)
					Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);

				SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			}

			// Force retraction if stuck on tiles while retracting
			if (CurrentFlailState != FlailState.Spinning && CurrentFlailState != FlailState.Ricochet && CurrentFlailState != FlailState.Dropping && CollisionCounter >= 10f)
			{
				CurrentFlailState = FlailState.ForcedRetracting;
				Projectile.netUpdate = true;
			}

			return false;
		}

		public override bool? CanDamage()
		{
			if (CurrentFlailState == FlailState.Spinning && StateTimer <= 12f)
				return false;

			return base.CanDamage();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (CurrentFlailState == FlailState.Spinning)
			{
				Vector2 distance = targetHitbox.ClosestPointInRect(Owner.MountedCenter) - Owner.MountedCenter;
				distance.Y /= SpinVerticalFactor;
				return distance.Length() <= SpinHitboxRadius;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			UpdateDamageKB(out float damageMult, out float kbMult);
			modifiers.SourceDamage *= damageMult;
			modifiers.Knockback *= kbMult;

			modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();
		}

		public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
		{
			if (!String.IsNullOrEmpty(ChainTexturePath))
				DrawChain();

			// By default, maces have afterimages when launched.
			// This can still be disabled via config or set individually by the projectile.
			// (Does not use utils due to set trail length conditions)
			if (CurrentFlailState == FlailState.LaunchingForward && CalamityClientConfig.Instance.Afterimages && AfterimageLength > 0)
			{
				Texture2D maceTex = TextureAssets.Projectile[Type].Value;
				Vector2 drawOrigin = new Vector2(maceTex.Width * 0.5f, maceTex.Height * 0.5f);
				SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				for (int k = 0; k < Projectile.oldPos.Length && k < StateTimer; k++)
				{
					Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin * Projectile.scale + new Vector2(0f, Projectile.gfxOffY);
					Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
					Main.spriteBatch.Draw(maceTex, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
				}
			}
			return true;
		}
		#endregion
	}
}
