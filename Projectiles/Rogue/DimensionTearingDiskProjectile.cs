using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class DimensionTearingDiskProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/DimensionTearingDisk";
        private static float RotationIncrement = 0.15f;
        private static int Lifetime = 350;
        public static int StealthExtraLifetime = 240; // 1 extra update means this is double what you'd expect for 2 seconds
        private static int ReboundTime = 60;

        private float randomLaserCharge = 0f;
        private Vector2 dir = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (dir == Vector2.Zero)
                dir = Projectile.velocity.SafeNormalize(Vector2.Zero);

            // The disc runs its returning AI if it has existed longer than ReboundTime frames.
            if (Projectile.ai[0] != 1f)
            {

                // On the frame the disc begins returning, send a net update.
                if (Projectile.timeLeft == Lifetime - ReboundTime)
                {
                    Projectile.ResetLocalNPCHitImmunity();
                    Projectile.netUpdate = true;
                }
                Projectile.velocity = Vector2.Lerp(dir, Projectile.DirectionTo(Main.player[Projectile.owner].Center).SafeNormalize(Vector2.Zero), (Lifetime - Projectile.timeLeft) / (ReboundTime * 1.75f));
                Projectile.velocity *= Projectile.Calamity().stealthStrike ? 40f : 25f;
            }
            if (Projectile.timeLeft < Lifetime - ReboundTime)
            {
                if (Projectile.Distance(Main.player[Projectile.owner].Center) < 32)
                    Projectile.Kill();
            }
            // Lighting.
            Lighting.AddLight(Projectile.Center, 0.35f, 0f, 0.25f);

            // Rotate the disc as it flies.
            float spin = Projectile.direction <= 0 ? -1f : 1f;
            Projectile.rotation += spin * RotationIncrement;

            // If attached to something (this only occurs for stealth strikes), do the buzzsaw grind and spam lasers everywhere.
            if (Projectile.ai[0] == 1f)
                StealthStrikeGrind(spin);
            else if (Projectile.timeLeft == Lifetime - ReboundTime)
            {
                NPC npc = null;
                float maxDist = 1000;
                foreach (var item in Main.ActiveNPCs)
                {
                    if (!item.CanBeChasedBy())
                        continue;
                    var dist = item.Distance(Projectile.Center);
                    if (dist < maxDist)
                    {
                        maxDist = dist;
                        npc = item;
                    }
                }
                if (npc != null)
                {
                    int laserDamage = (int)(Projectile.damage * 0.7f);
                    Projectile laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(npc.Center), ModContent.ProjectileType<FriendlyLaserWallBeam>(), laserDamage, 0f, Projectile.owner, -1.5f);
                    if (laser.whoAmI.WithinBounds(Main.maxProjectiles))
                    {
                        laser.DamageType = RogueDamageClass.Instance;
                        laser.scale *= 0.5f;
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        int sparkLifetime = Main.rand.Next(14, 21);
                        float sparkScale = Main.rand.NextFloat(0.8f, 1f) + 1f * 0.05f;
                        Color sparkColor = Color.Lerp(Color.Fuchsia, Color.AliceBlue, Main.rand.NextFloat(0.5f));
                        sparkColor = Color.Lerp(sparkColor, Color.Cyan, Main.rand.NextFloat());

                        if (Main.rand.NextBool(5))
                            sparkScale *= 1.4f;

                        Vector2 sparkVelocity = Projectile.DirectionTo(npc.Center).RotatedByRandom(MathHelper.TwoPi) * 5;
                        SparkParticle spark = new SparkParticle(Projectile.Center, sparkVelocity, false, sparkLifetime, sparkScale, sparkColor);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }
            }
        }

        private void StealthStrikeGrind(float spinDir)
        {
            // Spin extra fast to visually shred the enemy.
            Projectile.rotation += spinDir * RotationIncrement * 0.8f;

            // Randomly fire lasers while grinding.
            randomLaserCharge += Main.rand.NextFloat(0.09f, 0.14f);
            if (randomLaserCharge >= 1f)
            {
                randomLaserCharge -= 1f;
                Vector2 velocity = Vector2.UnitX.RotatedBy(Projectile.timeLeft / 120f * MathHelper.TwoPi);

                int laserDamage = (int)(Projectile.damage * 0.2f);
                Projectile laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<FriendlyLaserWallBeam>(), laserDamage, 0f, Projectile.owner, 1.5f,0,2);
                if (laser.whoAmI.WithinBounds(Main.maxProjectiles))
                {
                    laser.DamageType = RogueDamageClass.Instance;
                }
            }

            // Stay stuck to the target.
            Projectile.StickyProjAI(6, true);

            // If still attached to a target, do nothing.
            if (Projectile.ai[0] != 0f)
                return;

            // If the target died, look for a new one.
            const float turnSpeed = 30f;
            const float speedMult = 5f;
            const float homingRange = 600f;
            NPC uDie = Projectile.Center.ClosestNPCAt(homingRange, true, true);
            if (uDie != null)
            {
                Vector2 distNorm = (uDie.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = (Projectile.velocity * (turnSpeed - 1f) + distNorm * speedMult) / turnSpeed;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 180);

            // Spawn sparks; taken from Despair stone then adapted to a projectile
            Vector2 particleSpawnDisplacement;
            Vector2 splatterDirection;

            particleSpawnDisplacement = new Vector2(2f * -Projectile.ai[2], 2f * -Projectile.ai[2]);
            splatterDirection = new Vector2(Projectile.velocity.X, Projectile.velocity.Y);

            Vector2 SparkSpawnPosition = target.Center + particleSpawnDisplacement;

            if (Projectile.ai[1] % 4 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    int sparkLifetime = Main.rand.Next(14, 21);
                    float sparkScale = Main.rand.NextFloat(0.8f, 1f) + 1f * 0.05f;
                    Color sparkColor = Color.Lerp(Color.Fuchsia, Color.AliceBlue, Main.rand.NextFloat(0.5f));
                    sparkColor = Color.Lerp(sparkColor, Color.Cyan, Main.rand.NextFloat());

                    if (Main.rand.NextBool(5))
                        sparkScale *= 1.4f;

                    Vector2 sparkVelocity = splatterDirection.RotatedByRandom(MathHelper.TwoPi);
                    sparkVelocity.Y -= 6f;
                    SparkParticle spark = new SparkParticle(SparkSpawnPosition, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 180);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Non-stealth strikes do not stick on hit.
            if (!Projectile.Calamity().stealthStrike)
                return;

            // On the first frame of impact, slow down massively so it'll effectively stay stuck to an enemy.
            if (Projectile.ai[0] == 0f && Projectile.ai[1] == 0f)
            {
                Projectile.timeLeft = StealthExtraLifetime;
                Projectile.velocity *= 0.1f;

                // Provide a fixed amount of grind time so that DPS can't vary wildly.
                Projectile.timeLeft = 90;
            }

            // Apply sticky AI.
            Projectile.ModifyHitNPCSticky(3);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector2 origin = new Vector2(31f, 29f);
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/DimensionTearingDiskGlow").Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
        }
    }
}
