using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class CataclysmicFlame : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/FireProj";

        public static int Lifetime => 90;
        public static int Fadetime => 80;
        public ref float Time => ref Projectile.ai[0];
        public int MistType = -1;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Time++;
            if (Time < Fadetime && Main.rand.NextBool(6))
            {
                Vector2 cinderPos = Projectile.Center + Main.rand.NextVector2Circular(60f, 60f) * Utils.Remap(Time, 0f, Lifetime, 0.5f, 1f);
                float cinderSize = Utils.GetLerpValue(6f, 12f, Time, true);
                Dust cinder = Dust.NewDustDirect(cinderPos, 4, 4, ModContent.DustType<BrimstoneFlame>(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
                if (Main.rand.NextBool(3))
                {
                    cinder.scale *= 2f;
                    cinder.velocity *= 2f;
                }
                cinder.noGravity = true;
                cinder.scale *= cinderSize * 1.2f;
                cinder.velocity += Projectile.velocity * Utils.Remap(Time, 0f, Fadetime * 0.75f, 1f, 0.1f) * Utils.Remap(Time, 0f, Fadetime * 0.1f, 0.1f, 1f);
            }

            if (MistType == -1)
                MistType = Main.rand.Next(3);

            Lighting.AddLight(Projectile.Center, 0.75f, 0.15f, 0.15f);
            if (Projectile.timeLeft > Lifetime - 10)
                return;
            float timeRatio = Utils.GetLerpValue(0f, Lifetime, Time);
            float fireSize = Utils.Remap(timeRatio, 0.2f, 0.5f, 0.25f, 1f);
            var p = CataclysmMetaball.SpawnParticle(Projectile.Center, Vector2.Zero, Terraria.GameContent.TextureAssets.Projectile[Type].Width() * fireSize);
            p.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            p.TextureToUse = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            p.SizeScaling = 0.5f;
        }

        // Keeping the flames in place when hitting a block
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = oldVelocity * 0.95f;
            Projectile.position -= Projectile.velocity;
            Time++;
            Projectile.timeLeft--;
            return false;
        }

        // Expanding hitbox
        // A bit more generous than Havoc's Breath fire because enemy projectiles
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int size = (int)Utils.Remap(Time, 0f, Fadetime, 8f, 32f);

            // Shrinks again after fading
            if (Time > Fadetime)
                size = (int)Utils.Remap(Time, Fadetime, Lifetime, 32f, 0f);
            hitbox.Inflate(size, size);
        }

        // Anti-wall hacking checks
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => projHitbox.Intersects(targetHitbox) && Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0) ? null : false;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 240);

            // Cook you up (still scales with player size in case it's manipulated)
            int smokeCount = 4 + (int)MathHelper.Clamp(target.width * 0.1f, 0f, 20f);
            for (int i = 0; i < smokeCount; i++)
            {
                Vector2 smokePos = target.Center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f);
                Vector2 smokeVel = Vector2.UnitY * Main.rand.NextFloat(-2.4f, -0.8f) * MathHelper.Clamp(target.height * 0.1f, 1f, 10f);
                Particle smoke = new MediumMistParticle(smokePos, smokeVel, new Color(255, 50, 50), Color.DimGray, Main.rand.NextFloat(1f, 2f), 245 - Main.rand.Next(50), 0.1f);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => false;
    }
}
