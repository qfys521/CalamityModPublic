using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.PlaguebringerGoliath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HiveBombGoliath : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < 18f)
                Projectile.velocity *= 1.01f + (Projectile.ai[0] * 0.0002f);

            if (Projectile.position.Y > Projectile.ai[1])
                Projectile.tileCollide = true;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            if (Math.Abs(Projectile.velocity.X) >= 3f || Math.Abs(Projectile.velocity.Y) >= 3f)
            {
                float randDustXVel = 0f;
                float randDustYVel = 0f;
                if (Main.rand.NextBool(2))
                {
                    randDustXVel = Projectile.velocity.X * 0.5f;
                    randDustYVel = Projectile.velocity.Y * 0.5f;
                }
                int bombDust = Dust.NewDust(new Vector2(Projectile.position.X + 3f + randDustXVel, Projectile.position.Y + 3f + randDustYVel) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Torch, 0f, 0f, 100, default, 0.5f);
                Main.dust[bombDust].scale *= 2f + (float)Main.rand.Next(10) * 0.1f;
                Main.dust[bombDust].velocity *= 0.2f;
                Main.dust[bombDust].noGravity = true;
                bombDust = Dust.NewDust(new Vector2(Projectile.position.X + 3f + randDustXVel, Projectile.position.Y + 3f + randDustYVel) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100, default, 0.25f);
                Main.dust[bombDust].fadeIn = 1f + (float)Main.rand.Next(5) * 0.1f;
                Main.dust[bombDust].velocity *= 0.05f;
            }
            else if (Main.rand.NextBool(4))
            {
                int smoke = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
                Main.dust[smoke].scale = 0.1f + (float)Main.rand.Next(5) * 0.1f;
                Main.dust[smoke].fadeIn = 1.5f + (float)Main.rand.Next(5) * 0.1f;
                Main.dust[smoke].noGravity = true;
                Main.dust[smoke].position = Projectile.Center + new Vector2(0f, (float)(-(float)Projectile.height / 2)).RotatedBy((double)Projectile.rotation, default) * 1.1f;
                Main.rand.Next(2);
                smoke = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                Main.dust[smoke].scale = 1f + (float)Main.rand.Next(5) * 0.1f;
                Main.dust[smoke].noGravity = true;
                Main.dust[smoke].position = Projectile.Center + new Vector2(0f, (float)(-(float)Projectile.height / 2 - 6)).RotatedBy((double)Projectile.rotation, default) * 1.1f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Projectile.DrawBackglow(PlaguebringerGoliath.BackglowColor, 4f);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);

            Texture2D glow = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/HiveBombGoliathGlow").Value;
            Vector2 textureArea = new Vector2(glow.Width / 2, glow.Height / Main.projFrames[Type] / 2);
            Vector2 drawArea = Projectile.Center - Main.screenPosition;
            drawArea -= new Vector2(glow.Width, glow.Height / Main.projFrames[Type]) / 2f;
            drawArea += textureArea + new Vector2(0f, Projectile.gfxOffY);
            Color whiteColor = Color.White;
            int height = glow.Height / Main.projFrames[Type];
            int drawStart = height * Projectile.frame;
            Main.spriteBatch.Draw(glow, drawArea, new Rectangle(0, drawStart, glow.Width, height), whiteColor, Projectile.rotation, textureArea, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 64;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                int plagued = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
                Main.dust[plagued].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[plagued].scale = 0.5f;
                    Main.dust[plagued].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 10; j++)
            {
                int plagued2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald, 0f, 0f, 100, default, 3f);
                Main.dust[plagued2].noGravity = true;
                Main.dust[plagued2].velocity *= 5f;
                plagued2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
                Main.dust[plagued2].velocity *= 2f;
            }

            if (!Main.dedServ)
            {
                Vector2 goreSource = Projectile.Center;
                int goreAmt = 3;
                Vector2 source = new Vector2(goreSource.X - 24f, goreSource.Y - 24f);
                for (int goreIndex = 0; goreIndex < goreAmt; goreIndex++)
                {
                    float velocityMult = 0.33f;
                    if (goreIndex < (goreAmt / 3))
                    {
                        velocityMult = 0.66f;
                    }
                    if (goreIndex >= (2 * goreAmt / 3))
                    {
                        velocityMult = 1f;
                    }
                    Mod mod = ModContent.GetInstance<CalamityMod>();
                    int type = Main.rand.Next(61, 64);
                    int smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                    Gore gore = Main.gore[smoke];
                    gore.velocity *= velocityMult;
                    gore.velocity.X += 1f;
                    gore.velocity.Y += 1f;
                    type = Main.rand.Next(61, 64);
                    smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                    gore = Main.gore[smoke];
                    gore.velocity *= velocityMult;
                    gore.velocity.X -= 1f;
                    gore.velocity.Y += 1f;
                    type = Main.rand.Next(61, 64);
                    smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                    gore = Main.gore[smoke];
                    gore.velocity *= velocityMult;
                    gore.velocity.X += 1f;
                    gore.velocity.Y -= 1f;
                    type = Main.rand.Next(61, 64);
                    smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                    gore = Main.gore[smoke];
                    gore.velocity *= velocityMult;
                    gore.velocity.X -= 1f;
                    gore.velocity.Y -= 1f;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && Main.zenithWorld)
            {
                Vector2 valueBoom = Projectile.Center;
                float spreadBoom = 15f * 0.0174f;
                double startAngleBoom = Math.Atan2(Projectile.velocity.X, Projectile.velocity.Y) - spreadBoom / 2;
                double deltaAngleBoom = spreadBoom / 8f;
                double offsetAngleBoom;
                int iBoom;
                int damageBoom = Projectile.damage / 2;
                for (iBoom = 0; iBoom < 5; iBoom++)
                {
                    if (Main.rand.NextBool(5) && iBoom > 0)
                    {
                        int projectileType = ModContent.ProjectileType<SandPoisonCloud>();
                        offsetAngleBoom = startAngleBoom + deltaAngleBoom * (iBoom + iBoom * iBoom) / 2f + 32f * iBoom;
                        float velocity = 0.5f;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), valueBoom.X, valueBoom.Y, (float)(Math.Sin(offsetAngleBoom) * velocity), (float)(Math.Cos(offsetAngleBoom) * velocity), projectileType, damageBoom, 0f, Main.myPlayer, 0f, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), valueBoom.X, valueBoom.Y, (float)(-Math.Sin(offsetAngleBoom) * velocity), (float)(-Math.Cos(offsetAngleBoom) * velocity), projectileType, damageBoom, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            if (Main.zenithWorld) // it is the plague, you get very sick.
            {
                target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 240);
                target.AddBuff(BuffID.Poisoned, 240);
                target.AddBuff(BuffID.Venom, 240);
            }
            target.AddBuff(ModContent.BuffType<Plague>(), 120);
        }
    }
}
