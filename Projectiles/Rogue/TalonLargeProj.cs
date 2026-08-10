using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class TalonLargeProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        private NPC hitTarget = null;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.MaxUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.UnusedBrown, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
            if (Main.rand.NextBool(10))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Clentaminator_Cyan, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }

            // Fade out after hitting
            // After completely fading, spawn an extra slash if you haven't hit the limit
            if (Projectile.numHits > 0)
            {
                Projectile.alpha += 12;
                if (Projectile.alpha >= 255)
                {
                    // Exit early if we've hit the limit of 5 slashes
                    if (Projectile.ai[1] >= 4f)
                    {
                        Projectile.Kill();
                        return;
                    }

                    // Target index is originally the index of the NPC that was hit, but if this becomes invalid, find a new NPC to target
                    int index = hitTarget.whoAmI;
                    if (!hitTarget.CanBeChasedBy(Projectile))
                    {
                        float checkDist = 1000f;
                        index = -1;
                        foreach (NPC n in Main.ActiveNPCs)
                        {
                            if (!n.CanBeChasedBy(Projectile, false))
                                continue;

                            float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                            if (currentNPCDist < checkDist)
                            {
                                checkDist = currentNPCDist;
                                index = n.whoAmI;
                            }
                        }
                    }
                    // Spawn the extra slash
                    if (index != -1)
                    {
                        Vector2 spawnOffset = Main.npc[index].Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 8f).RotatedBy(Main.rand.NextBool() ? MathHelper.Pi : 0) * Main.rand.NextFloat(140f, 180f);
                        Vector2 spawnVel = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(spawnOffset, Main.npc[index], 12f, 3);

                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile extraSlash = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnOffset, spawnVel, ModContent.ProjectileType<TalonLargeProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, Projectile.ai[1] + 1);
                            extraSlash.Calamity().stealthStrike = true;
                            extraSlash.tileCollide = false;
                            (extraSlash.ModProjectile as TalonLargeProj).hitTarget = hitTarget;
                        }

                        PointParticle point = new(spawnOffset, Vector2.Normalize(spawnVel), false, 6, 2.25f, Color.Cyan);
                        GeneralParticleHandler.SpawnParticle(point);
                    }
                    Projectile.Kill();
                }
            }

            Projectile.ai[0]++;
            Projectile.tileCollide = Projectile.ai[1] == 0f && Projectile.ai[0] > 2f;
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
        }

        public override bool? CanDamage()
        {
            if (Projectile.numHits > 0)
                return false;
            if (hitTarget == null || Projectile.getRect().Intersects(hitTarget.getRect()))
                return null;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitTarget = target;
            Projectile.tileCollide = false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.White, 1);
            return false;
        }
    }
}
