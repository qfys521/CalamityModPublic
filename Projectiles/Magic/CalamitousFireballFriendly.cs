using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class CalamitousFireballFriendly : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 9)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 6)
                Projectile.frame = 0;

            // Fade in
            if (Projectile.alpha > 5)
                Projectile.alpha -= 15;
            if (Projectile.alpha < 5)
                Projectile.alpha = 5;

            // Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) - MathHelper.ToRadians(90f) * Projectile.direction;

            if (Projectile.velocity.Length() < 16f)
                Projectile.velocity *= 1.01f;

            Lighting.AddLight(Projectile.Center, 0.5f, 0f, 0f);

            var brimDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(32, 32), (int)CalamityDusts.Brimstone, null, 170, default, 1.1f);
            brimDust.noGravity = true;
            brimDust.velocity *= 0.5f;
            brimDust.velocity += Projectile.velocity * 0.1f;
            for (var i = 0; i < 1; i++)
            {
                CalamitasMetaball.SpawnParticle(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(2, 2), 64f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(250, 50, 50, Projectile.alpha);
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CalamitousExplosionFriendly>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }
    }
}
