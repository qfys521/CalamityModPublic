using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class AerialTrackerProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Items/Weapons/DraedonsArsenal/AerialTracker";
        public NPC extraShotTarget;

        public bool ReturningToPlayer
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value.ToInt();
        }

        public float Time
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public const int LaserFireRate = 19;
        public const int LaserFireRateStealth = 25;
        public const int MaxLaserCountPerShot = 2; // This only applies to stealth strikes.
        public const float MaxTargetSearchDistance = 600f;
        public const float MaxTargetSearchStealth = 800f;
        public const float ReturnAccelerationFactor = 0.0012f;
        public const float ReturnMaxSpeed = 6f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Lerp(Effects.ArsenalEffects.ArsenalLaserColor, Color.White, 0.5f).ToVector3() * 0.5f);

            Player player = Main.player[Projectile.owner];

            Time++;

            if (Time == 5 && !Projectile.Calamity().stealthStrike)
                Projectile.tileCollide = true;
            if (Time < 100 && Time > 10 && Projectile.Calamity().stealthStrike)
                Projectile.velocity = Projectile.velocity.RotatedBy(-0.029f * Projectile.ai[2]);

            if (!ReturningToPlayer)
            {
                if (Time >= 55f)
                {
                    ReturningToPlayer = true;
                    Projectile.tileCollide = false;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                float distanceFromPlayer = Projectile.Distance(player.Center);
                if (distanceFromPlayer > 3000f)
                    Projectile.Kill();

                // This is done instead of a Normalize or DirectionTo call because the variables needed are already present and calculating the square root again would be unnecessary.
                Vector2 idealVelocity = (player.Center - Projectile.Center) / distanceFromPlayer * ReturnMaxSpeed * (Projectile.Calamity().stealthStrike ? Utils.GetLerpValue(90, 300, Time, true) * 1.5f : Utils.GetLerpValue(60, 300, Time, true));

                Projectile.velocity.X += Math.Sign(idealVelocity.X - Projectile.velocity.X) * (ReturnAccelerationFactor * Time);
                Projectile.velocity.Y += Math.Sign(idealVelocity.Y - Projectile.velocity.Y) * (ReturnAccelerationFactor * Time);

                if (Time % (Projectile.Calamity().stealthStrike ? LaserFireRateStealth : LaserFireRate) == 0f)
                    AttemptToFireLasers(Projectile.damage);

                if (Main.myPlayer == Projectile.owner)
                {
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                        Projectile.Kill();
                }
            }

            if (ReturningToPlayer)
            {
                Projectile.rotation += 0.28f * Utils.GetLerpValue(30, 300, Time, true);
            }
            else
                Projectile.rotation += 0.15f;
        }

        public void AttemptToFireLasers(int damage)
        {
            if (Main.myPlayer != Projectile.owner)
                return;
            if (Projectile.Calamity().stealthStrike)
            {
                int targetCount = 0;
                List<NPC> targets = Main.npc.Where(npc =>
                {
                    return npc.active && Projectile.Distance(npc.Center) < MaxTargetSearchStealth && npc.CanBeChasedBy();
                }).ToList();
                foreach (var target in targets)
                {
                    if (targetCount >= MaxLaserCountPerShot)
                        break;
                    Vector2 spawnLocation = Projectile.Center + new Vector2(25, 0).RotatedBy(Projectile.rotation * Utils.GetLerpValue(300, 30, Time, true));
                    Projectile laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<AerialTrackerLaser>(),
                        (int)(damage),
                        Projectile.knockBack,
                        Projectile.owner,
                        1f, Projectile.whoAmI, target.whoAmI);
                    laser.scale *= 1.4f;
                    laser.netUpdate = true;
                    targetCount++;
                    extraShotTarget = target;
                }
                if (targetCount == 1)
                {
                    Vector2 spawnLocation = Projectile.Center + new Vector2(25, 0).RotatedBy(-Projectile.rotation * Utils.GetLerpValue(300, 30, Time, true));
                    Projectile laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<AerialTrackerLaser>(),
                        (int)(damage),
                        Projectile.knockBack,
                        Projectile.owner,
                        -1f, Projectile.whoAmI, extraShotTarget.whoAmI);
                }
            }
            else
            {
                NPC potentialTarget = Projectile.Center.ClosestNPCAt(MaxTargetSearchDistance);
                if (potentialTarget != null)
                {
                    Vector2 spawnLocation = Projectile.Center + new Vector2(25, 0).RotatedBy(Projectile.rotation * 0.4f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AerialTrackerLaser>(), damage, Projectile.knockBack, Projectile.owner, 0, Projectile.whoAmI, potentialTarget.whoAmI);
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ReturningToPlayer = true;
            Projectile.tileCollide = false;
            Projectile.netUpdate = true;
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, ProjectileID.Sets.TrailCacheLength[Projectile.type], null, true, false);

            Main.EntitySpriteDraw(texture.Value, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
