using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class RavagerNuke : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public static readonly SoundStyle ExplosionSound = new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion");
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;

            if (Projectile.timeLeft < 180)
                Projectile.tileCollide = true;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 18)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 4)
                Projectile.frame = 0;

            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

            float inertia = revenge ? 90f : 110f;
            float scaleFactor12 = revenge ? 16f : 12f;

            if (Projectile.alpha > 0)
                Projectile.alpha -= 10;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            Lighting.AddLight(Projectile.Center, 1f, 0.7f, 0f);

            int playerTracker = (int)Projectile.ai[0];
            if (playerTracker >= 0 && Main.player[playerTracker].active && !Main.player[playerTracker].dead)
            {
                if (Projectile.Distance(Main.player[playerTracker].Center) > 320f)
                {
                    Vector2 moveDirection = Projectile.SafeDirectionTo(Main.player[playerTracker].Center, Vector2.UnitY);
                    Projectile.velocity = (Projectile.velocity * (inertia - 1f) + moveDirection * scaleFactor12) / inertia;
                }
            }
            else
            {
                if (Projectile.timeLeft > 30)
                    Projectile.timeLeft = 30;

                if (Projectile.ai[0] != -1f)
                {
                    Projectile.ai[0] = -1f;
                    Projectile.netUpdate = true;
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                if (DownedBossSystem.downedProvidence)
                {
                    target.AddBuff(ModContent.BuffType<Laceration>(), 180);
                }
                else
                {
                    target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(ExplosionSound, Projectile.Center);
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 160;
            Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);
            Projectile.Damage();
            for (int i = 0; i < 30; i++)
            {
                int nukeDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 2f);
                Main.dust[nukeDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[nukeDust].scale = 0.5f;
                    Main.dust[nukeDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 40; j++)
            {
                int nukeDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 3f);
                Main.dust[nukeDust2].noGravity = true;
                Main.dust[nukeDust2].velocity *= 5f;
                nukeDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 2f);
                Main.dust[nukeDust2].velocity *= 2f;
            }

            if (!Main.dedServ)
            {
                Vector2 source = new Vector2(Projectile.Center.X - 24f, Projectile.Center.Y - 24f);
                for (int g = 1; g <= 3; g++)
                {
                    float velocityMult = g * 0.33f;
                    for (int spawn = 0; spawn < 4; spawn++)
                    {
                        int type = Main.rand.Next(61, 64);
                        int smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                        Gore gore = Main.gore[smoke];
                        gore.velocity *= velocityMult;
                        gore.velocity.X += 1f;
                        gore.velocity.Y += 1f;
                    }
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            int timeToStartWarning = 180;

            Color initialColor = lightColor;
            Color finalColor = Color.Lerp(Color.White, Color.Red, (float)Math.Abs(Math.Sin((timeToStartWarning - Projectile.timeLeft) * (MathHelper.Pi * 7f / 180f))));
            Color warningColor;
            finalColor.A = (byte)(255 - Projectile.alpha);
            float colorTransitionRatio = MathHelper.Clamp((timeToStartWarning - Projectile.timeLeft) / (float)timeToStartWarning, 0f, 1f);

            if (Projectile.timeLeft <= timeToStartWarning)
                warningColor = Color.Lerp(initialColor, finalColor, colorTransitionRatio);
            else
                warningColor = initialColor;

            float strength = Utils.GetLerpValue(0, timeToStartWarning / 1.5f, timeToStartWarning - Projectile.timeLeft, true);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, 5, 0, Projectile.frame);
            SpriteEffects sp = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Projectile.DrawBackglow(new Color(64, 255, 255) * strength, 4.5f, null, frame, sp);

            Main.spriteBatch.EnterShaderRegion(BlendState.AlphaBlend);
            GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(colorTransitionRatio * 0.45f);
            GameShaders.Misc["CalamityMod:BasicTint"].UseColor(warningColor);
            GameShaders.Misc["CalamityMod:BasicTint"].Apply();
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, sp);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
