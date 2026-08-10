using System;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class HallowedStarSummon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        private int noTileHitCounter = 120;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 50;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.extraUpdates = 4;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void AI()
        {
            int randomToSubtract = Main.rand.Next(1, 3);
            noTileHitCounter -= randomToSubtract;
            if (noTileHitCounter == 0)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                if (Main.rand.NextBool(5))
                    SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }
            Projectile.alpha -= 15;
            int alphaMin = 150;
            if (Projectile.Center.Y >= Projectile.ai[1])
            {
                alphaMin = 0;
            }
            if (Projectile.alpha < alphaMin)
            {
                Projectile.alpha = alphaMin;
            }
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
            if (Main.rand.NextBool(48) && !Main.dedServ)
            {
                int idx = Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.2f, 16, 1f);
                Main.gore[idx].velocity *= 0.66f;
                Main.gore[idx].velocity += Projectile.velocity * 0.3f;
            }
            if (Projectile.ai[1] == 1f)
            {
                Projectile.light = 0.9f;
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1.2f);
                }
                if (Main.rand.NextBool(20) && !Main.dedServ)
                {
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), Main.rand.Next(16, 18), 1f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 45, 250, Projectile.alpha);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Projectile.DrawStarTrail(Color.Purple, Color.White);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.position += Projectile.Size;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position -= Projectile.Size;
            for (int i = 0; i < 5; i++)
            {
                int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud, 0f, 0f, 100, default, 1.2f);
                Main.dust[idx].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[idx].scale = 0.5f;
                    Main.dust[idx].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity * 0.05f, Main.rand.Next(16, 18), 1f);
                }
            }
        }
    }
}
