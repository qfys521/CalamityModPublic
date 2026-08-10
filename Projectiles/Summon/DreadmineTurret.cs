using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class DreadmineTurret : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public float count = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 80;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }
            Projectile.velocity = new Vector2(0f, (float)Math.Sin((double)(6.28318548f * Projectile.ai[0] / 300f)) * 0.5f);
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 300f)
            {
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
            }
            if (Projectile.ai[0] % 15 == 0)
            {
                int mineAmt = 0;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.owner == Main.myPlayer && p.type == ModContent.ProjectileType<Dreadmine>() && p.ai[0] == Projectile.whoAmI)
                    {
                        mineAmt++;
                    }
                }
                for (float i = 0; i < 5; i++)
                    if (Main.myPlayer == Projectile.owner && mineAmt < 25)
                    {
                        int dreadmineWidth = 58;
                        Vector2 center = Projectile.Center;
                        center += new Vector2(256 * Main.rand.NextFloat() + 64, 0).RotatedByRandom(MathHelper.TwoPi); // This determines the offset of the mine at a random distance between 64 and 320 pixels away. The lower bound is set so mines don't spawn on top of the turret itself, and also helps even out distribution a little.
                        if (!Collision.SolidCollision(center - Projectile.Size / 2f, dreadmineWidth, dreadmineWidth))
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), center, Vector2.Zero, ModContent.ProjectileType<Dreadmine>(), Projectile.damage, Projectile.knockBack, Projectile.owner,Projectile.whoAmI);
                            mineAmt++;
                        }
                        else
                            i -= 0.75f; // This will cause it to run more loops when mines fail, but still eventually get up to the cap of 5 mines per activation even if no spots to spawn can be found.
                    }
                    else
                    {
                        break;
                    }
            }
        }

        public override bool? CanDamage() => false;
    }
}
