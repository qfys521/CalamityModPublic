using System.IO;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HomingLaserDart : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 900;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override void AI()
        {
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 4)
                Projectile.frame = 0;

            Projectile.alpha -= 40;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            if (Projectile.alpha < 40)
            {
                int laserDust = Dust.NewDust(Projectile.Center - Vector2.One * 5f, 10, 10, DustID.CopperCoin, -Projectile.velocity.X / 3f, -Projectile.velocity.Y / 3f, 150, Color.Transparent, 0.6f);
                Main.dust[laserDust].noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.ai[1] == -1f)
            {
                if (Projectile.velocity.Length() < 18f)
                    Projectile.velocity *= 1.05f;
                else
                    Projectile.tileCollide = true;

                return;
            }

            Vector2 maxVelocity = new Vector2(death ? 13.2f : 12f, death ? 13.2f : 12f);
            float maxAcceleration = death ? 0.44f : 0.4f;
            float timeBeforeHoming = death ? 30f : 45f;
            float distanceAboveTargetBeforeHomingDownward = death ? 400f : 480f;
            if (Projectile.ai[0] == 0f)
            {
                Projectile.localAI[0] += 1f;
                if (Projectile.localAI[0] >= timeBeforeHoming)
                {
                    Projectile.localAI[0] = 0f;
                    Projectile.ai[0] = 1f;
                    Projectile.ai[1] = Player.FindClosest(Projectile.position, Projectile.width, Projectile.height);
                    Projectile.netUpdate = true;
                }

                Projectile.velocity.X = Projectile.velocity.RotatedBy(0D).X;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -maxVelocity.X, maxVelocity.X);

                Projectile.velocity.Y -= maxAcceleration * 0.2f;
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y -= maxAcceleration * 0.5f;
                if (Projectile.velocity.Y < -maxVelocity.Y)
                    Projectile.velocity.Y = -maxVelocity.Y;
            }
            else if (Projectile.ai[0] == 1f)
            {
                if (Main.player[(int)Projectile.ai[1]].Center.Y > Projectile.Center.Y + distanceAboveTargetBeforeHomingDownward)
                {
                    Projectile.ai[0] = 2f;
                    Projectile.netUpdate = true;
                }

                Projectile.velocity.X = Projectile.velocity.RotatedBy(0D).X;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -maxVelocity.X, maxVelocity.X);

                Projectile.velocity.Y -= maxAcceleration * 0.2f;
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y -= maxAcceleration * 0.5f;
                if (Projectile.velocity.Y < -maxVelocity.Y)
                    Projectile.velocity.Y = -maxVelocity.Y;
            }
            else if (Projectile.ai[0] == 2f)
            {
                if (Main.player[(int)Projectile.ai[1]].Center.Y < Projectile.Center.Y)
                    Projectile.tileCollide = true;

                Vector2 playerDistance = Main.player[(int)Projectile.ai[1]].Center - Projectile.Center;
                if (Projectile.Hitbox.Intersects(Main.player[(int)Projectile.ai[1]].Hitbox))
                {
                    Projectile.Kill();
                    return;
                }

                Vector2 projectileVelocity = playerDistance.SafeNormalize(Vector2.UnitY);
                projectileVelocity *= maxVelocity.Length();
                projectileVelocity = Vector2.Lerp(Projectile.velocity, projectileVelocity, 0.6f);
                if (projectileVelocity.Y < maxVelocity.Y)
                    projectileVelocity.Y = maxVelocity.Y;

                if (Projectile.velocity.X < projectileVelocity.X)
                {
                    Projectile.velocity.X += maxAcceleration;
                    if (Projectile.velocity.X < 0f && projectileVelocity.X > 0f)
                        Projectile.velocity.X += maxAcceleration;
                }
                else if (Projectile.velocity.X > projectileVelocity.X)
                {
                    Projectile.velocity.X -= maxAcceleration;
                    if (Projectile.velocity.X > 0f && projectileVelocity.X < 0f)
                        Projectile.velocity.X -= maxAcceleration;
                }

                if (Projectile.velocity.Y < projectileVelocity.Y)
                {
                    Projectile.velocity.Y += maxAcceleration;
                    if (Projectile.velocity.Y < 0f && projectileVelocity.Y > 0f)
                        Projectile.velocity.Y += maxAcceleration;
                }
                else if (Projectile.velocity.Y > projectileVelocity.Y)
                {
                    Projectile.velocity.Y -= maxAcceleration;
                    if (Projectile.velocity.Y > 0f && projectileVelocity.Y < 0f)
                        Projectile.velocity.Y -= maxAcceleration;
                }

                // Fly away from other lasers
                float pushForce = death ? 0.12f : 0.08f;
                float pushDistance = death ? 60f : 40f;
                for (int k = 0; k < Main.maxProjectiles; k++)
                {
                    Projectile otherProj = Main.projectile[k];
                    // Short circuits to make the loop as fast as possible
                    if (!otherProj.active || k == Projectile.whoAmI)
                        continue;

                    // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                    bool sameProjType = otherProj.type == Projectile.type;
                    float taxicabDist = Vector2.Distance(Projectile.Center, otherProj.Center);
                    if (sameProjType && taxicabDist < pushDistance)
                    {
                        if (Projectile.position.X < otherProj.position.X)
                            Projectile.velocity.X -= pushForce;
                        else
                            Projectile.velocity.X += pushForce;

                        if (Projectile.position.Y < otherProj.position.Y)
                            Projectile.velocity.Y -= pushForce;
                        else
                            Projectile.velocity.Y += pushForce;
                    }
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255, 50, 50, Projectile.alpha);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Zombie103, Projectile.Center);
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 96;
            Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);

            for (int i = 0; i < 3; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);

            for (int j = 0; j < 30; j++)
            {
                int killDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 0, default, 2.5f);
                Main.dust[killDust].noGravity = true;
                Main.dust[killDust].velocity *= 3f;
                killDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 1.5f);
                Main.dust[killDust].velocity *= 2f;
                Main.dust[killDust].noGravity = true;
            }
        }
    }
}
