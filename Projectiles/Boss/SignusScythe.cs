using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class SignusScythe : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        private int counter = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 420;
            Projectile.alpha = 100;
            Projectile.penetrate = -1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(counter);
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            counter = reader.ReadInt32();
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override void AI()
        {
            Projectile.rotation += 0.5f * Projectile.direction;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
            }

            int shadowDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1f);
            Main.dust[shadowDust].noGravity = true;
            Main.dust[shadowDust].velocity *= 0f;

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 150f)
            {
                if (Projectile.ai[1] > 0f && Projectile.ai[0] < 160f)
                {
                    int playerTracker = (int)Projectile.ai[1] - 1;
                    if (playerTracker < Main.maxPlayers)
                    {
                        Vector2 playerDirection = Main.player[playerTracker].Center - Projectile.Center;
                        Projectile.velocity = Vector2.Normalize(playerDirection) * 22f;
                    }
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, Projectile.alpha);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Laceration>(), 180);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                int killDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1f);
                Main.dust[killDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[killDust].scale = 0.5f;
                    Main.dust[killDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 10; j++)
            {
                int killDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1.5f);
                Main.dust[killDust2].noGravity = true;
                Main.dust[killDust2].velocity *= 5f;
                killDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1f);
                Main.dust[killDust2].velocity *= 2f;
            }
        }
    }
}
