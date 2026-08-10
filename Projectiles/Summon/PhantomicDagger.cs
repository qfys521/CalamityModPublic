using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class PhantomicDagger : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        private bool homing = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 38;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.alpha = 200;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (homing)
                return null; //cannot hit until it is beginning to home.
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(homing);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            homing = reader.ReadBoolean();
        }

        // Reduce damage of projectiles if more than the cap are active
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Avoid touching things that you probably aren't meant to damage
            if (target.defense > 999 || target.Calamity().DR >= 0.95f || target.Calamity().unbreakableDR)
                return;

            // Bypass a portion of the target's DR
            float maxDRPenetration = 1.05f; // 5% extra damage
            modifiers.FinalDamage *= MathHelper.Clamp(1f / (1f - target.Calamity().DR), 1f, maxDRPenetration);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int d = 0; d < 4; d++)
            {
                int shadow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 2f);
                Main.dust[shadow].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[shadow].scale = 0.5f;
                    Main.dust[shadow].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int d = 0; d < 12; d++)
            {
                int shadow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 3f);
                Main.dust[shadow].noGravity = true;
                Main.dust[shadow].velocity *= 5f;
                shadow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 2f);
                Main.dust[shadow].velocity *= 2f;
            }

        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (CalamityClientConfig.Instance.Afterimages)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            }
            return true;
        }

        public override void AI()
        {
            if (Main.dust.Length < Main.maxDust - 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 3f); //new Color(99, 54, 84)
                    Main.dust[dust].noGravity = true;
                }
            }
            if (Projectile.alpha != 0)
            {
                Projectile.rotation -= 6f * (MathF.Pow(Projectile.Opacity,2f));
                Projectile.velocity *= MathHelper.Lerp(1, 0.9f, Projectile.Opacity);
                if (Projectile.alpha < 3)
                {
                    Projectile.alpha = 0;
                    homing = true;
                }
                else
                    Projectile.alpha -= 2;
            }
            else
            {
                NPC target = CalamityUtils.MinionHoming(Projectile.Center, 1500f, Main.player[Projectile.owner]);
                if (target != null)
                {
                    float projVel = 40f;
                    Vector2 projDirection = Projectile.Center;
                    float targetXDist = target.Center.X - projDirection.X;
                    float targetYDist = target.Center.Y - projDirection.Y;
                    float targetDist = (float)Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));
                    if (targetDist < 100f)
                    {
                        projVel = 28f; //14
                    }
                    targetDist = projVel / targetDist;
                    targetXDist *= targetDist;
                    targetYDist *= targetDist;
                    Projectile.velocity.X = (Projectile.velocity.X * 2f + targetXDist) / 3f;
                    Projectile.velocity.Y = (Projectile.velocity.Y * 2f + targetYDist) / 3f;
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.Atan(90);
            }
        }
    }
}
