using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class InfernalKrisProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/InfernalKris";
        private bool hasSpawnedCinders = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.timeLeft < 280)
            {
                Projectile.rotation += 0.4f * Projectile.direction;

                float minScale = 1.9f;
                float maxScale = 2.5f;
                int dust = Dust.NewDust(Projectile.position - new Vector2(10, 10), 30, 30, DustID.Torch, Projectile.velocity.X, Projectile.velocity.Y, 0, default, Main.rand.NextFloat(minScale, maxScale));
                Main.dust[dust].noGravity = true;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }

            Projectile.velocity.Y += 0.01f;
            if (Projectile.velocity.Y > 10f)
            {
                Projectile.velocity.Y = 10f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int debuffTime = 60 * (Projectile.Calamity().stealthStrike ? Main.rand.Next(4, 8) : Main.rand.Next(3, 6));
            target.AddBuff(BuffID.OnFire, debuffTime);

            if (Projectile.Calamity().stealthStrike)
                StealthEffect();
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            int debuffTime = 60 * (Projectile.Calamity().stealthStrike ? Main.rand.Next(4, 8) : Main.rand.Next(3, 6));
            target.AddBuff(BuffID.OnFire, debuffTime);

            if (Projectile.Calamity().stealthStrike)
                StealthEffect();
        }
        private void StealthEffect()
        {
            hasSpawnedCinders = true;
            int sparkCount = Main.rand.Next(4, 5 + 1);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVelocity = Main.rand.NextVector2Circular(1f, 3f);
                sparkVelocity = sparkVelocity.SafeNormalize(Vector2.UnitY) * 3f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, sparkVelocity, ModContent.ProjectileType<InfernalKrisCinder>(), (int)(Projectile.damage * 0.5f), 0, Projectile.owner, 0, 0);
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<InfernalKrisExplosion>(), (int)(Projectile.damage * 0.5f), 0, Projectile.owner, 0, 0);
            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
        }


        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.Calamity().stealthStrike)
            {
                // If this is a stealth strike, make the blade glow orange
                Color glowColour = new Color(255, 215, 100, 100);
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], glowColour, 1);

                float minScale = 1.9f;
                float maxScale = 2.5f;
                int dust = Dust.NewDust(Projectile.position, 10, 10, DustID.Torch, Projectile.velocity.X, Projectile.velocity.Y, 0, default, Main.rand.NextFloat(minScale, maxScale));
                Main.dust[dust].noGravity = true;
            }
            else
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            }
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);

            Projectile.ai[0] = 0;
            Projectile.ai[1] = 0;

            if (Projectile.velocity.X != oldVelocity.X)
            {
                if (oldVelocity.X < 0)
                {
                    Projectile.ai[0] = 1;
                }
                if (oldVelocity.X > 0)
                {
                    Projectile.ai[0] = -1;
                }
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                if (oldVelocity.Y < 0)
                {
                    Projectile.ai[1] = 1;
                }
                if (oldVelocity.Y > 0)
                {
                    Projectile.ai[1] = -1;
                }
            }

            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
            Projectile.Kill();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.Calamity().stealthStrike && !hasSpawnedCinders)
            {
                int sparkCount = Main.rand.Next(4, 5+1);
                for (int i = 0; i < sparkCount; i++)
                {
                    Vector2 sparkVelocity = Main.rand.NextVector2Circular(1f, 3f);
                    sparkVelocity = sparkVelocity.SafeNormalize(Vector2.UnitY) * 3f;

                    if (Projectile.ai[0] != 0)
                    {
                        sparkVelocity.X *= -1;
                    }
                    if (Projectile.ai[1] != 0)
                    {
                        sparkVelocity.Y *= -1;
                    }

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, sparkVelocity, ModContent.ProjectileType<InfernalKrisCinder>(), (int)(Projectile.damage * 0.5f), 0, Projectile.owner, 0, 0);
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<InfernalKrisExplosion>(), (int)(Projectile.damage * 0.5f), 0, Projectile.owner, 0, 0);
                SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
            }
        }
    }
}
