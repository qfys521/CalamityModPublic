using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class AntlionSkewerProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/AntlionSkewer";

        public ref float Time => ref Projectile.ai[0];

        public static float TimeToSpit => 15f;
        public static float TimeToAccelerate => 30f;
        public static int StealthExtraSpit = 4;
        public static float SandBlastDamage = 0.5f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.localNPCHitCooldown = 20 * Projectile.MaxUpdates;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Time++;
            if (Time == TimeToSpit && Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(Projectile.Calamity().stealthStrike ? SoundID.NPCDeath13 : SoundID.Item17, Projectile.Center);

                var source = Projectile.GetSource_FromThis();
                if (Projectile.Calamity().stealthStrike)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        float offset = MathHelper.ToRadians(MathHelper.Lerp(-30f, 30f, i / 8f));
                        Vector2 spreadVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(offset) * (i % 2 == 0 ? 6f : 4f);
                        Projectile.NewProjectile(source, Projectile.Center, spreadVel, ModContent.ProjectileType<AntlionSkewerSandCloud>(), 0, 0f, Projectile.owner);
                    }

                    for (int i = 0; i < StealthExtraSpit; i++)
                    {
                        Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(24f));
                        Projectile.NewProjectile(source, Projectile.Center, velocity, ModContent.ProjectileType<AntlionSkewerSandBlast>(), (int)(Projectile.damage * SandBlastDamage), Projectile.knockBack * SandBlastDamage, Projectile.owner);
                    }
                }

                Projectile.NewProjectile(source, Projectile.Center, Projectile.velocity, ModContent.ProjectileType<AntlionSkewerSandBlast>(), (int)(Projectile.damage * SandBlastDamage), Projectile.knockBack * SandBlastDamage, Projectile.owner);
            }

            if (Time > TimeToSpit && Time <= TimeToSpit + TimeToAccelerate)
                Projectile.velocity *= 1.015f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }
    }
}
