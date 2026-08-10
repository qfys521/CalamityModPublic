using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class ScorchedEarthClusterBomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public Vector2 vibrate = Vector2.Zero;
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.95f;
            if (Projectile.velocity.Length() < 0.5f && Projectile.timeLeft > 10)
                Projectile.timeLeft = 10;
            if (Projectile.timeLeft <= 75)
            {
                float power = 2.5f;
                vibrate = Main.rand.NextVector2Circular(power, power);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.8f , MaxInstances = 3}, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                int size = 16;
                Vector2 position = Projectile.Center;
                Vector2 velocity = Main.rand.NextVector2Circular(size, size);
                SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.2f, 0.3f), Color.DarkOrange * 0.5f, Main.rand.Next(6, 9), 1, 1.5f);
                GeneralParticleHandler.SpawnParticle(energy);
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, velocity, 0, default, Main.rand.NextFloat(1f, 2f));
                dust.noGravity = true;
            }
            int projAmt = Main.rand.Next(2, 4);
            if (Projectile.owner == Main.myPlayer)
            {
                for (int k = 0; k < projAmt; k++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<RocketFire>(), Projectile.damage, 0f, Projectile.owner);
                }
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D Texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = Texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 drawPosition;
            Vector2 origin = frame.Size() * 0.5f;
            drawPosition = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(Texture, drawPosition + vibrate, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
