using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class MushBombGround : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/Boss/MushBomb";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            if (Main.rand.NextBool())
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy, 0f, 0f, 100, default, 0.8f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0f;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            Lighting.AddLight(Projectile.Center, 0f, 0.15f, 0.3f);

            float xVelocityMultiplier = 1.025f;
            float xVelocityLimit = 12f;
            if (Math.Abs(Projectile.velocity.X) < xVelocityLimit)
                Projectile.velocity.X *= xVelocityMultiplier;
        }

        public override Color? GetAlpha(Color drawColor) => Main.zenithWorld ? new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, Projectile.alpha) : new Color(255, 255, 255, Projectile.alpha);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int height = texture.Height / Main.projFrames[Type];
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Size / 2;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, drawStart, texture.Width, height), Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int height = texture.Height / Main.projFrames[Type];
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Size / 2;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/MushBombGlow").Value, Projectile.Center - Main.screenPosition, new Rectangle(0, drawStart, texture.Width, height), Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            
            for (int i = 0; i < 3; i++)
            {
                int shroomDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy, 0f, 0f, 100, default, 1f);
                Main.dust[shroomDust].velocity *= 1.5f;
                if (Main.rand.NextBool())
                {
                    Main.dust[shroomDust].scale = 0.5f;
                    Main.dust[shroomDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }

            for (int j = 0; j < 9; j++)
            {
                int shroomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy, 0f, 0f, 100, default, 1.5f);
                Main.dust[shroomDust2].noGravity = true;
                Main.dust[shroomDust2].velocity *= 2f;
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy, 0f, 0f, 100, default, 1.5f);
            }
        }
    }
}
