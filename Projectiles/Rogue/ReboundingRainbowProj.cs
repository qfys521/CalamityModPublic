using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class ReboundingRainbowProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/ReboundingRainbow";
        private int Lifetime = 400;
        private int ReboundTime = 30;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 56;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            SpawnProjectilesNearEnemies();
            BoomerangAI();
            LightingAndDust();
        }

        private void BoomerangAI()
        {
            // Boomerang rotation
            Projectile.rotation += 0.4f * Projectile.direction;

            // Boomerang sound
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 8;
                SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
            }

            // Returns after some number of frames in the air
            int timeMult = Projectile.Calamity().stealthStrike ? ReboundingRainbow.stealthTimeMult : 1;
            if (Projectile.timeLeft < Lifetime * timeMult - ReboundTime * timeMult)
                Projectile.ai[0] = 1f;

            if (Projectile.ai[0] == 1f)
            {
                Player player = Main.player[Projectile.owner];
                float returnSpeed = 14f;
                float acceleration = Projectile.Calamity().stealthStrike ? 0.45f : 0.6f;
                Vector2 playerVec = player.Center - Projectile.Center;
                float dist = playerVec.Length();

                // Delete the projectile if it's excessively far away.
                if (dist > 3000f)
                    Projectile.Kill();

                playerVec.Normalize();
                playerVec *= returnSpeed;

                // Home back in on the player.
                if (Projectile.velocity.X < playerVec.X)
                {
                    Projectile.velocity.X += acceleration;
                    if (Projectile.velocity.X < 0f && playerVec.X > 0f)
                        Projectile.velocity.X += acceleration;
                }
                else if (Projectile.velocity.X > playerVec.X)
                {
                    Projectile.velocity.X -= acceleration;
                    if (Projectile.velocity.X > 0f && playerVec.X < 0f)
                        Projectile.velocity.X -= acceleration;
                }
                if (Projectile.velocity.Y < playerVec.Y)
                {
                    Projectile.velocity.Y += acceleration;
                    if (Projectile.velocity.Y < 0f && playerVec.Y > 0f)
                        Projectile.velocity.Y += acceleration;
                }
                else if (Projectile.velocity.Y > playerVec.Y)
                {
                    Projectile.velocity.Y -= acceleration;
                    if (Projectile.velocity.Y > 0f && playerVec.Y < 0f)
                        Projectile.velocity.Y -= acceleration;
                }

                // Delete the projectile if it touches its owner.
                if (Main.myPlayer == Projectile.owner)
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                        Projectile.Kill();
            }
        }

        private void SpawnProjectilesNearEnemies()
        {
            if (!Projectile.friendly)
                return;

            float maxDistance = 300f;
            bool homeIn = false;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(Projectile, false))
                {
                    float extraDistance = npc.width / 2 + npc.height / 2;

                    bool canHit = true;
                    if (extraDistance < maxDistance)
                        canHit = Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1);

                    if (Vector2.Distance(npc.Center, Projectile.Center) < maxDistance + extraDistance && canHit)
                    {
                        homeIn = true;
                        break;
                    }
                }
            }

            if (homeIn)
            {
                int counter = Projectile.Calamity().stealthStrike ? 40 : 60;
                if (Main.player[Projectile.owner].miscCounter % counter == 0)
                {
                    int splitProj = ModContent.ProjectileType<ReboundingRainbowSplit>();
                    if (Projectile.owner == Main.myPlayer && Main.player[Projectile.owner].ownedProjectileCounts[splitProj] < 16)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 velocity = ((MathHelper.TwoPi * i / 8f) - (MathHelper.ToRadians(67.5f) - Projectile.velocity.ToRotation())).ToRotationVector2() * 4f;
                            Projectile split = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, splitProj, Projectile.damage, Projectile.knockBack, Projectile.owner);
                            if (Projectile.Calamity().stealthStrike)
                            {
                                split.idStaticNPCHitCooldown = 8;
                                split.usesIDStaticNPCImmunity = true;
                                split.timeLeft = 60;
                            }
                        }
                    }
                }
            }
        }

        private void LightingAndDust()
        {
            if (Main.rand.NextBool(3))
            {
                int rainbow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, Projectile.direction * 2, 0f, 150, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), 1.3f);
                Main.dust[rainbow].noGravity = true;
                Main.dust[rainbow].velocity *= 0f;
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 1f, 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 60);

            if (!Projectile.Calamity().stealthStrike)
                Projectile.ai[0] = 1f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (!Projectile.Calamity().stealthStrike)
                Projectile.ai[0] = 1f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }
    }
}
