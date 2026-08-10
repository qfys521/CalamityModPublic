using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ExoTankMissile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public static Asset<Texture2D> Glow;
        public override void Load() => Glow = ModContent.Request<Texture2D>(Texture + "Glow");

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = 90 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            CalamityUtils.HomeInOnNPC(Projectile, false, 1200f, 15f, 20f);
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnKill(int timeLeft)
        {
            Color boomColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.4f) * 0.6f;
            CustomPulse boom = new (Projectile.Center, Vector2.Zero, boomColor, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.08f, 24);
            GeneralParticleHandler.SpawnParticle(boom);

            for (int i = 0; i < 5; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * 0.2f * (i + Main.rand.NextFloat(-0.5f, 0.5f))) * (Main.rand.NextFloat(3f, 10f));
                Color energyColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.4f);
                SquishyLightParticle energy = new (Projectile.Center, velocity, Main.rand.NextFloat(0.1f, 0.5f), energyColor, 15, 1f, 2.5f);
                GeneralParticleHandler.SpawnParticle(energy);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Glow != null)
            {
                Rectangle frame = Glow.Value.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
                Main.EntitySpriteDraw(Glow.Value, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
        }
    }
}
