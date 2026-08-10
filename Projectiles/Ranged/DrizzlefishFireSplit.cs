using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class DrizzlefishFireSplit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/DrizzlefishFire";
        public int Time = 0;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.ai[1] == 1f)
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1, ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/DrizzlefishFire2").Value);
            else
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1, ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/DrizzlefishFire").Value);
            //Changes the texture of the projectile
            if (Projectile.ai[1] == 1f)
            {
                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/DrizzlefishFire2").Value;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(new Rectangle(0, 0, 16, 16)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, 20 / 2f), Projectile.scale, SpriteEffects.None, 0);
                return false;
            }
            return true;
        }

        public override void AI()
        {
            Time++;
            Player Owner = Main.player[Projectile.owner];
            if (Main.zenithWorld && Time == 1 && Owner.Calamity().dragoonDrizzlefishGelBoost > 1)
                Projectile.damage = (int)(Projectile.damage * Owner.Calamity().dragoonDrizzlefishGelBoost);
            Projectile.velocity.X *= 0.98f;
            Projectile.velocity.Y += 0.5f;
            int dustType = 235;
            if (Projectile.ai[1] == 1f)
            {
                if (Main.rand.NextBool())
                {
                    dustType = 174;
                }
                else
                {
                    dustType = 162;
                }
            }
            else
            {
                if (Main.rand.NextBool())
                {
                    dustType = 183;
                }
                else
                {
                    dustType = 90;
                }
            }
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4) - Projectile.velocity * 1.5f, dustType, -Projectile.velocity);
                dust.noGravity = true;
                dust.velocity *= 0f;
                dust.scale = Owner.Calamity().dragoonDrizzlefishGelBoost > 1 ? Main.rand.NextFloat(0f + Owner.Calamity().dragoonDrizzlefishGelBoost * 0.5f, 0.3f + Owner.Calamity().dragoonDrizzlefishGelBoost * 0.5f) : Main.rand.NextFloat(0.4f, 0.8f);
            }
            Lighting.AddLight(Projectile.Center, 0.25f, 0f, 0f);
            if (Projectile.timeLeft > 90)
            {
                Projectile.timeLeft = 90;
            }
            Projectile.rotation += 0.3f * (float)Projectile.direction;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            //Doze - Flamethrowers in vanilla are long debuff infliction tools (20 seconds of their debuff).
            //I am applying this as the base for Cal flamethrowers, with shorter times being the exception instead of the rule
            //This is one of the exceptions, as Brimstone Flames is a very strong debuff for it's tier
            if (Projectile.ai[1] == 1f)
            {
                target.AddBuff(BuffID.OnFire3, 60);
            }
            else
            {
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 30);
            }
        }
        public override void OnKill(int timeLeft)
        {
            Player Owner = Main.player[Projectile.owner];
            int dustType = 235;
            for (int i = 0; i <= 9; i++)
            {
                if (Projectile.ai[1] == 1f)
                {
                    if (Main.rand.NextBool())
                    {
                        dustType = 174;
                    }
                    else
                    {
                        dustType = 162;
                    }
                }
                else
                {
                    if (Main.rand.NextBool())
                    {
                        dustType = 183;
                    }
                    else
                    {
                        dustType = 90;
                    }
                }
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, new Vector2(0, -5).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 1.9f));
                dust.noGravity = false;
                dust.scale = Owner.Calamity().dragoonDrizzlefishGelBoost > 1 ? Main.rand.NextFloat(0f + Owner.Calamity().dragoonDrizzlefishGelBoost * 0.5f, 0.6f + Owner.Calamity().dragoonDrizzlefishGelBoost * 0.5f) : Main.rand.NextFloat(0.4f, 1.1f);
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, dustType, new Vector2(0, -3).RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.1f, 1.9f));
                dust2.noGravity = false;
                dust2.scale = Owner.Calamity().dragoonDrizzlefishGelBoost > 1 ? Main.rand.NextFloat(0f + Owner.Calamity().dragoonDrizzlefishGelBoost * 0.5f, 0.6f + Owner.Calamity().dragoonDrizzlefishGelBoost * 0.5f) : Main.rand.NextFloat(0.4f, 1.1f);
            }
        }
    }
}
