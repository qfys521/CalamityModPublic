using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CorpusAvertorProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/CorpusAvertor";

        private ref float Timer => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.03f;

            if (Timer < 120f)
                Timer += 1f;

            if (Timer == 20f)
            {
                Vector2 cloneVel = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(1f, 1.75f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, cloneVel, ModContent.ProjectileType<CorpusAvertorClone>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, 1f);
            }

            if (Timer > 60f)
            {
                Projectile.velocity *= 1.01f;

                int scale = (int)((Timer - 60f) * 3f);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, new Color(scale, 0, 0, 50), 2f);
                Main.dust[dust].velocity *= 0f;
                Main.dust[dust].noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color returnColor;
            if (Timer >= 61f)
            {
                returnColor = Color.Lerp(Color.White, Color.Red, Math.Min((Timer - 60f) / 120f * 3f, 1f));
                returnColor.A = 50;
                return returnColor;
            }
            returnColor = Color.White;
            returnColor.A = 50;
            return returnColor;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
            Main.player[Projectile.owner].SpawnLifeStealProjectile(target, Projectile, ProjectileID.VampireHeal, (int)Math.Round(hit.Damage * 0.05));
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
    }
}
