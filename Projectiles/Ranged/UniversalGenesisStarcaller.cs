using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class UniversalGenesisStarcaller : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.23f, 0.19f, 0.25f);

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 4f)
            {
                if (Main.rand.NextBool())
                {
                    int idx = Dust.NewDust(Projectile.position, 1, 1, DustID.ShadowbeamStaff, 0f, 0f, 0, default, 0.5f);
                    Main.dust[idx].alpha = Projectile.alpha;
                    Main.dust[idx].velocity *= 0f;
                    Main.dust[idx].noGravity = true;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item11 with { PitchVariance = 0.05f }, Projectile.Center);
            return true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, lightColor);
            return false;
        }

        // This projectile is always fullbright.
        public override Color? GetAlpha(Color lightColor) => new Color(1f, 1f, 1f, 0f);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // The patron said 10 blocks, but 10 blocks is like nothing...  I'm sure they won't mind
            int maxDistance = 480; // 30 blocks
            bool bossFound = false;
            int life = 0;
            int index = -1;
            Vector2 targetVec = Projectile.Center;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (bossFound && !npc.IsABoss())
                    continue;
                if (npc.CanBeChasedBy(Projectile, false))
                {
                    float extraDist = (npc.width / 2) + (npc.height / 2);
                    // Calculate distance between target and the projectile to know if it's too far or not
                    float targetDist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (targetDist < (maxDistance + extraDist) && (npc.IsABoss() || npc.life > life))
                    {
                        if (npc.IsABoss())
                            bossFound = true;
                        life = npc.life;
                        targetVec = npc.Center;
                        index = npc.whoAmI;
                    }
                }
            }

            Vector2 spawnPos = targetVec - new Vector2(Main.rand.NextFloat(-300f, 300f), Main.rand.NextFloat(500f, 800f));
            Vector2 velocity = index == -1 ? Utils.DirectionTo(spawnPos, Projectile.Center) * 29f : CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(spawnPos, Main.npc[index], 29f, 2);
            velocity.X += Main.rand.NextFloat(-1f, 1f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity, ModContent.ProjectileType<UniversalGenesisStar>(), (int)(Projectile.damage * 0.65f), Projectile.knockBack, Projectile.owner);
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            if (!Main.dedServ)
            {
                for (int g = 0; g < 3; g++)
                {
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18), 1f);
                }
            }
        }
    }
}
