using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class CatastrophicCinder : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.penetrate = CalamityWorld.death ? 4 : 3;
            Projectile.timeLeft = 300;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f * Projectile.direction;

            Lighting.AddLight(Projectile.Center, 0.25f, 0f, 0f);

            var p = CatastropheMetaball.SpawnParticle(Projectile.Center + Projectile.velocity, -Projectile.velocity, Terraria.GameContent.TextureAssets.Projectile[Type].Width() * 2);
            p.rotation = Projectile.rotation;
            p.TextureToUse = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            p.SizeScaling = 0.5f;

            Projectile.Opacity = 0;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (var i = 0; i < 10; i++)
            {
                var p = CatastropheMetaball.SpawnParticle(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(4, 4), 16);
                p.SizeScaling = 0.9f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            return Projectile.penetrate-- == 0;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }
    }
}
