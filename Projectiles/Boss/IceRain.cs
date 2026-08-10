using System;
using System.IO;
using CalamityMod.Events;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class IceRain : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1.2f;
            Projectile.hostile = true;
            Projectile.coldDamage = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
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
            Lighting.AddLight((int)((Projectile.position.X + (Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (Projectile.height / 2)) / 16f), 0f, 0.25f, 0.25f);

            if (Projectile.ai[0] == 0f)
            {
                if (Projectile.velocity.Length() < Projectile.ai[1])
                    Projectile.velocity *= 1.015f;

                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

                for (int i = 0; i < 2; i++)
                {
                    int icyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 0.6f);
                    Main.dust[icyDust].noGravity = true;
                    Dust dust = Main.dust[icyDust];
                    dust.velocity *= 0.3f;
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                if (Projectile.velocity.Length() < 10f)
                    Projectile.velocity *= (CalamityWorld.revenge || BossRushEvent.BossRushActive) ? 1.03f : 1.025f;

                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

                for (int i = 0; i < 2; i++)
                {
                    int icyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 0.6f);
                    Main.dust[icyDust].noGravity = true;
                    Dust dust = Main.dust[icyDust];
                    dust.velocity *= 0.3f;
                }
            }
            else if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity.Y += 0.1f;

                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

                if (Projectile.velocity.Y > 6f)
                    Projectile.velocity.Y = 6f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(1f, 1f, 1f, 1f) * Projectile.Opacity;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Projectile.DrawProjectileWithBackglow(Cryogen.BackglowColor, lightColor, 4f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            //SoundEngine.PlaySound(SoundID.Item27 with { Volume = SoundID.Item27.Volume * 0.25f }, Projectile.Center);
            for (int j = 0; j < 3; j++)
            {
                int snowDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Snow, 0f, 0f, 0, default, 1f);
                Main.dust[snowDust].noGravity = true;
                Main.dust[snowDust].noLight = true;
                Main.dust[snowDust].scale = 0.7f;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(BuffID.Frostburn, 120);
        }
    }
}
