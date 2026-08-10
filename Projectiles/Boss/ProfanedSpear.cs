using CalamityMod.Buffs.DamageOverTime;
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
    public class ProfanedSpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0f);

            if (Projectile.timeLeft < 510)
                Projectile.tileCollide = true;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] <= 20f)
                Projectile.velocity *= 0.95f;
            else
                Projectile.velocity *= 1.06f;

            if (Projectile.ai[0] > 40f)
                Projectile.ai[0] = 0f;

            float maxVelocity = 20f;
            if (Projectile.velocity.Length() > maxVelocity)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxVelocity;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return ProvUtils.GetProjectileColor(lightColor);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ProvUtils.StandardAI() ? Terraria.GameContent.TextureAssets.Projectile[Type].Value : ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/ProfanedSpearNight").Value;
            Projectile.DrawBackglow(ProvUtils.GetProjectileColor(lightColor, true), 4f, texture);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, SpriteEffects.None, 0);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Color hiColor = ProvUtils.GetProjectileColor(255);
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
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<HolyFlames>(), 120);
        }
    }
}
