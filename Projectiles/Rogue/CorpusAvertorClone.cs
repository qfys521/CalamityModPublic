using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CorpusAvertorClone : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/CorpusAvertor";

        public const int MaxClones = 3;
        private ref float CloneNum => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.04f;

            if (Projectile.timeLeft < 165)
                CalamityUtils.HomeInOnNPC(Projectile, true, 480f, 14f, 20f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.timeLeft < 85)
                return new Color((int)(150f * (Projectile.timeLeft / 85f)), 0, 0, Projectile.timeLeft / 5 * 3);
            return new Color(150, 0, 0, 50);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override bool? CanDamage() => Projectile.timeLeft > 165 ? false : null;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);

            if (CloneNum < MaxClones && Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -Projectile.velocity.RotatedByRandom(MathHelper.Pi / 8f), ModContent.ProjectileType<CorpusAvertorClone>(), Projectile.damage, Projectile.knockBack, Projectile.owner, CloneNum + 1);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
    }
}
