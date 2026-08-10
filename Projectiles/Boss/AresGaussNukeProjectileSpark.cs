using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class AresGaussNukeProjectileSpark : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        private const int timeLeft = 180;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.Opacity = 0f;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
            Projectile.penetrate = -1;
            Projectile.timeLeft = timeLeft;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= 15f)
            {
                Projectile.localAI[0] = 15f;
                Projectile.velocity.Y += 0.1f;
            }

            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            int fadeOutTime = 15;
            int fadeInTime = 3;
            if (Projectile.timeLeft < fadeOutTime)
                Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / (float)fadeOutTime, 0f, 1f);
            else
                Projectile.Opacity = MathHelper.Clamp(1f - ((Projectile.timeLeft - (timeLeft - fadeInTime)) / (float)fadeInTime), 0f, 1f);

            Lighting.AddLight(Projectile.Center, 0.1f * Projectile.Opacity, 0.125f * Projectile.Opacity, 0.025f * Projectile.Opacity);

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) - MathHelper.PiOver2;
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity == 1f;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor.R = (byte)(255 * Projectile.Opacity);
            lightColor.G = (byte)(255 * Projectile.Opacity);
            lightColor.B = (byte)(255 * Projectile.Opacity);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
