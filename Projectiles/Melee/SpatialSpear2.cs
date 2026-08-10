using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class SpatialSpear2 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        private const int TimeLeft = 100;
        private int TotalSplits = Main.zenithWorld ? 4 : 3;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = TimeLeft;
            Projectile.tileCollide = !Main.zenithWorld;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.05f, 0.05f, 1f);

            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver4;

            if (Projectile.timeLeft < (TimeLeft / 2))
            {
                Projectile.velocity.Y += 0.16f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;

                Projectile.velocity.X *= 0.995f;
            }

            if (Projectile.localAI[1] == 0f)
            {
                Projectile.scale -= 0.01f;
                Projectile.alpha += 15;
                if (Projectile.alpha >= 125)
                {
                    Projectile.alpha = 130;
                    Projectile.localAI[1] = 1f;
                }
            }
            else if (Projectile.localAI[1] == 1f)
            {
                Projectile.scale += 0.01f;
                Projectile.alpha -= 15;
                if (Projectile.alpha <= 0)
                {
                    Projectile.alpha = 0;
                    Projectile.localAI[1] = 0f;
                }
            }

            int dust = Dust.NewDust(Projectile.oldPosition + Projectile.oldVelocity, Projectile.width, Projectile.height, DustID.BlueFairy, 0f, 0f, 100, default, 1.25f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0f;
            Main.dust[dust].noLightEmittance = true;

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= TimeLeft / TotalSplits)
            {
                Projectile.localAI[0] = 0f;
                int numProj = 2;
                float rotation = MathHelper.ToRadians(10);
                if (Projectile.owner == Main.myPlayer)
                {
                    for (int i = 0; i < numProj; i++)
                    {
                        Vector2 perturbedSpeed = Projectile.velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<SpatialSpear3>(), (int)(Projectile.damage * 0.8), Projectile.knockBack * 0.5f, Projectile.owner);
                    }
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(128, 128, byte.MaxValue);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft > TimeLeft - 5)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 30);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            for (int i = 4; i < 31; i++)
            {
                float projOldX = Projectile.oldVelocity.X * (30f / i);
                float projOldY = Projectile.oldVelocity.Y * (30f / i);
                int dust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - projOldX, Projectile.oldPosition.Y - projOldY), 8, 8, DustID.BlueFairy, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.8f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLightEmittance = true;

                dust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - projOldX, Projectile.oldPosition.Y - projOldY), 8, 8, DustID.BlueFairy, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.1f;
                Main.dust[dust].noLightEmittance = true;
            }
        }
    }
}
