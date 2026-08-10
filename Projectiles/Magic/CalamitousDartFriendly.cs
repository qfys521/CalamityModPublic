using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class CalamitousDartFriendly : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (Projectile.extraUpdates == 0)
            {
                Projectile.extraUpdates = 1;
                Projectile.velocity *= 0.5f;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 10)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            Lighting.AddLight(Projectile.Center, 0.9f * Projectile.Opacity, 0f, 0f);


            if (Projectile.timeLeft < 51)
                Projectile.Opacity -= 0.02f;

            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(-Projectile.velocity.Y, -Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = 1;
            var dir = Projectile.rotation.ToRotationVector2();
            if (Projectile.spriteDirection == -1)
                dir = dir.RotatedBy(MathHelper.Pi);
            var spot = Vector2.Lerp(Projectile.Center + dir.RotatedBy(MathHelper.PiOver2) * 26, Projectile.Center + dir.RotatedBy(-MathHelper.PiOver2) * 26, Main.rand.NextFloat());
            for (var i = 0; i < 1; i++)
            {
                var p = CalamitasMetaball.SpawnParticle(Projectile.Center + Projectile.velocity * 2, Vector2.Zero, 40);
                p.rotation = Projectile.rotation + MathHelper.PiOver2;
                p.TextureToUse = ModContent.Request<Texture2D>("CalamityMod/Particles/PointParticle").Value;
                p.SizeScaling = 0.65f;
                p = CalamitasMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(3, 3), 24f);
                p.SizeScaling = 0.8f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int dust = 0; dust <= 5; dust++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f);
            }
        }
    }
}
