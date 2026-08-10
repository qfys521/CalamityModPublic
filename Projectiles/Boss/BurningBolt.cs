using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class BurningBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public int time = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 690;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            int target = Player.FindClosest(Projectile.Center, 1, 1);

            float targetDist;
            if (target != -1 && !Main.player[target].dead && Main.player[target].active && Main.player[target] != null)
                targetDist = Vector2.Distance(Main.player[target].Center, Projectile.Center);
            else
                targetDist = 1000;

            if (Projectile.velocity.Length() < Projectile.ai[2])
            {
                Projectile.velocity *= 1.01f;
                if (Projectile.velocity.Length() > Projectile.ai[2])
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= Projectile.ai[2];
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            if (Projectile.timeLeft < 60)
                Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f);

            if (Projectile.ai[0] == 2f)
            {
                if (Projectile.timeLeft > 570)
                {
                    int player = Player.FindClosest(Projectile.Center, 1, 1);
                    Vector2 vector = Main.player[player].Center - Projectile.Center;
                    float scaleFactor = Projectile.velocity.Length();
                    vector.Normalize();
                    vector *= scaleFactor;
                    Projectile.velocity = (Projectile.velocity * 15f + vector) / 16f;
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= scaleFactor;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.75f * Projectile.Opacity, 0f, 0f);
            time++;

            var p = SeekersMetaball.SpawnParticle(Projectile.Center + Projectile.velocity * Projectile.MaxUpdates, Vector2.Zero, Terraria.GameContent.TextureAssets.Projectile[Type].Width() * Projectile.scale);
            p.rotation = Projectile.rotation;
            p.CurrentFrame = Projectile.frame;
            p.MaxFrames = Main.projFrames[Type];
            p.TextureToUse = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            p.SizeScaling = 0f;
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity == 1f;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.Opacity != 1f)
                return;
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 10 * Projectile.scale, targetHitbox);
    }
}
