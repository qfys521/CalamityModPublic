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
    public class HolyFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.tileCollide = false;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            ProvUtils.ApplyGFBDamage(Projectile, 120, 10);

            Lighting.AddLight(Projectile.Center, 0.3f, 0.225f, 0f);

            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            }

            Projectile.alpha -= !ProvUtils.StandardAI() ? 10 : 5;
            if (Projectile.alpha <= 0)
                Projectile.Kill();

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;
        }

        public override Color? GetAlpha(Color lightColor) => ProvUtils.GetProjectileColor(lightColor);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Projectile.scale = 1.5f;
            Texture2D texture = ProvUtils.StandardAI() ? Terraria.GameContent.TextureAssets.Projectile[Type].Value : ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/HolyFireNight").Value;
            int framing = texture.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Projectile.DrawBackglow(ProvUtils.GetProjectileColor(lightColor, true), 4f, new Vector2(Projectile.scale));
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Color hiColor = ProvUtils.GetProjectileColor(255, false);
            Color loColor = ProvUtils.GetProjectileColor(0, true);

            for (int i = 0; i < 25; i++)
                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.8f, 1.2f), hiColor));

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.5f, 0.1f, 4));

            for (float i = 0; i < 1; i += 0.25f)
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.02f * i, 0.125f * i, 8));

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.02f, 0.045f, 5));

            if (Main.rand.NextBool())
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 velocity = new Vector2(0.01f, 0f);
                    if (!ProvUtils.StandardAI())
                        velocity *= Main.rand.NextFloat(1f, 2f);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<HolyFire2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1f);

                    if (!ProvUtils.StandardAI())
                        velocity *= Main.rand.NextFloat(1f, 2f);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -velocity, ModContent.ProjectileType<HolyFire2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1f);
                }
            }
            else
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 velocity = new Vector2(0.05f, 0f);
                    if (!ProvUtils.StandardAI())
                        velocity *= Main.rand.NextFloat(1f, 2f);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<HolyFire2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1f);

                    if (!ProvUtils.StandardAI())
                        velocity *= Main.rand.NextFloat(1f, 2f);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -velocity, ModContent.ProjectileType<HolyFire2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1f);
                }
            }

            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
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
