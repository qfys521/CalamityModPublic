using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class PwnagehammerProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/Pwnagehammer";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/PwnagehammerSound") { Volume = 0.35f };
        public static readonly SoundStyle UseSoundFunny = new("CalamityMod/Sounds/Item/CalamityBell") { Volume = 1.5f };
        public static int HighBong = 0;
        public int time = 0;
        public ref int EmpoweredHammer => ref Main.player[Projectile.owner].Calamity().Holyhammer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 66;
            Projectile.friendly = true;
            Projectile.timeLeft = 3600;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;
            Projectile.rotation += MathHelper.ToRadians(22.5f) * Projectile.direction;
            if (EmpoweredHammer >= 5)
                EmpoweredHammer = 0;

            int falloffTime = 10;
            if (time > falloffTime)
                Projectile.velocity.X *= 0.9711f;
            if (Projectile.velocity.Y < 15 && time > falloffTime)
                Projectile.velocity.Y += 0.426f;
            if (Projectile.velocity.Y < 5)
                Projectile.velocity.Y *= 0.98f;

            if (Main.rand.NextBool(3))
            {
                Vector2 offset = new Vector2(12, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                Vector2 velOffset = new Vector2(4, 0).RotatedBy(offset.ToRotation());
                Dust dust = Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.Center.Y) + offset, DustID.GoldFlame, new Vector2(Projectile.velocity.X * 0.2f + velOffset.X, Projectile.velocity.Y * 0.2f + velOffset.Y), 100, new Color(255, 245, 198), 1.5f);
                dust.noGravity = true;
            }
            time++;
        }

        public override bool PreKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            float numberOfDusts = 13f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(9f, 0).RotatedBy(rot);
                Vector2 velOffset = new Vector2(6f, 0).RotatedBy(rot);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Sandnado, new Vector2(velOffset.X, velOffset.Y));
                dust.noGravity = true;
                dust.velocity = velOffset;
                dust.scale = 2.5f;
            }

            if (Main.zenithWorld)
                if (HighBong == 1)
                {
                    SoundEngine.PlaySound(UseSoundFunny with { Pitch = 4 * 0.1f - 0.2f }, Projectile.Center);
                    HighBong = 0;
                }
                else
                    SoundEngine.PlaySound(UseSoundFunny with { Pitch = EmpoweredHammer * 0.1f - 0.2f }, Projectile.Center);
            else
                if (HighBong == 1)
            {
                SoundEngine.PlaySound(UseSound with { Pitch = 4 * 0.1f - 0.2f }, Projectile.Center);
                HighBong = 0;
            }
            else
                SoundEngine.PlaySound(UseSound with { Pitch = EmpoweredHammer * 0.1f - 0.2f }, Projectile.Center);

            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0f, ModContent.ProjectileType<PwnagehammerExplosionSmall>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (EmpoweredHammer >= 3)
            {
                Projectile.ai[1] = target.whoAmI;
                int hammer = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Projectile.velocity.SafeNormalize(Vector2.UnitX).X * 5, -15f), ModContent.ProjectileType<PwnagehammerEcho>(), Projectile.damage * 2, Projectile.knockBack * 1.5f, Projectile.owner, 0f, Projectile.ai[1]);
                Main.projectile[hammer].localAI[0] = Math.Sign(Projectile.velocity.X);
                Main.projectile[hammer].netUpdate = true;
                EmpoweredHammer = 0;
                HighBong = 1;
            }
            else
            {
                ++EmpoweredHammer;
            }

        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }
    }
}
