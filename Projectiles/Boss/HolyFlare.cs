using System;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 840;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            ProvUtils.ApplyGFBDamage(Projectile, 120, 10);

            Lighting.AddLight(Projectile.Center, 0.35f, 0.275f, 0f);

            Player player = Main.player[Projectile.owner];

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 8)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            float velocityYCap = Projectile.position.Y + Projectile.height < player.position.Y ? 1f : 0.25f;
            Projectile.velocity.Y += 0.01f;
            if (Projectile.velocity.Y > velocityYCap)
                Projectile.velocity.Y = velocityYCap;

            float velocityX = !ProvUtils.StandardAI() ? 0.025f : 0.02f;
            if (Projectile.position.X + Projectile.width < player.position.X)
            {
                if (Projectile.velocity.X < 0f)
                    Projectile.velocity.X *= 0.99f;
                Projectile.velocity.X += velocityX;
            }
            else if (Projectile.position.X > player.position.X + player.width)
            {
                if (Projectile.velocity.X > 0f)
                    Projectile.velocity.X *= 0.99f;
                Projectile.velocity.X -= velocityX;
            }

            float velocityXCap = !ProvUtils.StandardAI() ? 10f : 8f;
            if (Projectile.velocity.X > velocityXCap || Projectile.velocity.X < -velocityXCap)
                Projectile.velocity.X *= 0.97f;

            float vel = Math.Clamp(((Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) / 2), 0f, 1f);

            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(15), 0).RotatedByRandom(MathHelper.TwoPi), Projectile.velocity.RotatedBy(Math.PI) * 0.5f, false, 10, Main.rand.NextFloat(0.8f, 1.2f), ProvUtils.GetProjectileColor(255)));

            if (Main.rand.NextBool())
            {
                GeneralParticleHandler.SpawnParticle(new MediumMistParticle(Projectile.Center, Vector2.Zero, Color.LightSlateGray, Color.DarkSlateGray, Main.rand.NextFloat(vel), 150, MathHelper.ToRadians(Main.rand.NextFloat(-1f, 1f))));
            }

            Projectile.rotation = MathHelper.Lerp(0f, MathHelper.WrapAngle(Vector2.Zero.AngleTo(Projectile.velocity) + MathHelper.ToRadians(-90f)), Math.Clamp(vel, 0f, 1f));
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return ProvUtils.GetProjectileColor(lightColor);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ProvUtils.StandardAI() ? Terraria.GameContent.TextureAssets.Projectile[Type].Value : ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/HolyFlareNight").Value;
            int framing = texture.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Projectile.DrawBackglow(ProvUtils.GetProjectileColor(lightColor, true), 4f, texture);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Color hiColor = ProvUtils.GetProjectileColor(255, false);
            Color loColor = ProvUtils.GetProjectileColor(0, true);

            for (int i = 0; i < 25; i++)
            {
                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.8f, 1.2f), hiColor));
            }

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.5f, 0.1f, 4));

            for (float i = 0; i < 1; i += 0.25f)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.02f * i, 0.075f * i, 24));
            }
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.02f, 0.045f, 16));

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact.WithPitchOffset(0.6f), Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item100.WithPitchOffset(0.4f), Projectile.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // If the player is dodging, don't apply debuffs
            if (info.Damage <= 0 || target.creativeGodMode)
                return;

            ProvUtils.ApplyDebuffs(target, 120);
        }
    }
}
