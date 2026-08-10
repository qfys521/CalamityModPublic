using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class CosmicIceBurst : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.coldDamage = true;
        }

        public override void AI()
        {
            Projectile.ai[1] += 0.01f;
            Projectile.scale = Projectile.ai[1];
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= (3 * Main.projFrames[Type]))
            {
                Projectile.Kill();
                return;
            }
            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Type])
                {
                    Projectile.hide = true;
                }
            }
            Projectile.alpha -= 63;
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
            Lighting.AddLight(Projectile.Center, 0.517f, (float)Main.DiscoG / 300f, 0.85f);
            if (Projectile.ai[0] == 1f)
            {
                Projectile.position = Projectile.Center;
                Projectile.width = Projectile.height = (int)(52f * Projectile.scale);
                Projectile.Center = Projectile.position;
                Projectile.Damage();
                SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
                for (int i = 0; i < 2; i++)
                {
                    int cosmicDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Flare_Blue, 0f, 0f, 100, new Color(150, 255, 255), 1f);
                    Main.dust[cosmicDust].position = Projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * (float)Projectile.width / 2f;
                }
                for (int j = 0; j < 3; j++)
                {
                    int iceDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceRod, 0f, 0f, 200, new Color(150, 255, 255), 1f);
                    Main.dust[iceDust].position = Projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * (float)Projectile.width / 2f;
                    Main.dust[iceDust].noGravity = true;
                    Main.dust[iceDust].velocity *= 3f;
                    iceDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceRod, 0f, 0f, 100, new Color(150, 255, 255), 0.6f);
                    Main.dust[iceDust].position = Projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * (float)Projectile.width / 2f;
                    Main.dust[iceDust].velocity *= 2f;
                    Main.dust[iceDust].noGravity = true;
                    Main.dust[iceDust].fadeIn = 2.5f;
                }
                for (int k = 0; k < 2; k++)
                {
                    int icyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceRod, 0f, 0f, 0, new Color(150, 255, 255), 1f);
                    Main.dust[icyDust].position = Projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy((double)Projectile.velocity.ToRotation(), default) * (float)Projectile.width / 2f;
                    Main.dust[icyDust].noGravity = true;
                    Main.dust[icyDust].velocity *= 3f;
                }
                for (int l = 0; l < 3; l++)
                {
                    int freezeDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Flare_Blue, 0f, 0f, 0, new Color(150, 255, 255), 0.6f);
                    Main.dust[freezeDust].position = Projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy((double)Projectile.velocity.ToRotation(), default) * (float)Projectile.width / 2f;
                    Main.dust[freezeDust].noGravity = true;
                    Main.dust[freezeDust].velocity *= 3f;
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
            Color colorArea = Lighting.GetColor((int)(Projectile.position.X + Projectile.width * 0.5f) / 16, (int)((Projectile.position.Y + Projectile.height * 0.5f) / 16));
            if (Projectile.usesOwnerLight)
            {
                colorArea = Lighting.GetColor((int)mountedCenter.X / 16, (int)(mountedCenter.Y / 16f));
            }
            Texture2D texture2D33 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle rectangl = texture2D33.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            return true;
        }

        // Only inflict Nightwither if not spawned from Icebreaker
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[2] != 1f)
                target.AddBuff(ModContent.BuffType<Nightwither>(), 420);
            Projectile.direction = Main.player[Projectile.owner].direction;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[2] != 1f)
                target.AddBuff(ModContent.BuffType<Nightwither>(), 420);
            Projectile.direction = Main.player[Projectile.owner].direction;
        }
    }
}
