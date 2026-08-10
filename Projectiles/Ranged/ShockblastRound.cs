using System;
using CalamityMod.Projectiles.Healing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ShockblastRound : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 3;
            AIType = ProjectileID.Bullet;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Shockblast>(), Projectile.damage, 0f, Projectile.owner, 0f, Projectile.ai[1]);
                Main.projectile[proj].scale = (Projectile.ai[1] * 0.5f) + 1f;
            }
            return true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override bool PreAI()
        {
            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(90) * Projectile.direction;

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 4f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dspeed = -Projectile.velocity * Main.rand.NextFloat(0.5f, 0.7f);
                    float x2 = Projectile.Center.X - Projectile.velocity.X / 10f * i;
                    float y2 = Projectile.Center.Y - Projectile.velocity.Y / 10f * i;
                    int dust = Dust.NewDust(new Vector2(x2, y2), 1, 1, DustID.FrostHydra, 0f, 0f, 0, default, 1f);
                    Main.dust[dust].alpha = Projectile.alpha;
                    Main.dust[dust].position.X = x2;
                    Main.dust[dust].position.Y = y2;
                    Main.dust[dust].velocity = dspeed;
                    Main.dust[dust].noGravity = true;
                }
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Shockblast>(), Projectile.damage, 0f, Projectile.owner, 0f, Projectile.ai[1]);
                Main.projectile[proj].scale = (Projectile.ai[1] * 0.5f) + 1f;
            }

            Main.player[Projectile.owner].SpawnLifeStealProjectile(target, Projectile, ModContent.ProjectileType<TransfusionTrail>(), (int)Math.Round(hit.Damage * 0.05));
        }
    }
}
