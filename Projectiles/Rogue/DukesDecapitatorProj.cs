using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class DukesDecapitatorProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        float rotationAmount = 1.5f;
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/DukesDecapitator";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            CalamityPlayer modPlayer = Main.player[Projectile.owner].Calamity();
            if (Projectile.velocity.X != 0 || Projectile.velocity.Y != 0)
            {
                Projectile.velocity *= 0.99f;
            }
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] == 5f)
                Projectile.tileCollide = true;

            if ((Projectile.ai[0] % 15f) == 0f && rotationAmount > 0)
            {
                rotationAmount -= 0.05f;
                if (Projectile.Calamity().stealthStrike && Projectile.owner == Main.myPlayer)
                {
                    float velocityX = Main.rand.NextFloat(-0.8f, 0.8f);
                    float velocityY = Main.rand.NextFloat(-0.8f, -0.8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, velocityX, velocityY, ModContent.ProjectileType<DukesDecapitatorBubble>(), (int)(Projectile.damage * 0.8), Projectile.knockBack, Projectile.owner);
                }
            }
            if (rotationAmount <= 0f)
                Projectile.Kill();

            Projectile.rotation += rotationAmount;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 120);
            Projectile.velocity = Vector2.Zero;
            rotationAmount -= 0.05f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 120);

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Mythril, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, new Color(255, 255, 255), 0.75f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
