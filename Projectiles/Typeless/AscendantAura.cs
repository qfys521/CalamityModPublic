using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class AscendantAura : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public float beamWidth = 1.04f;
        public bool beamsize = false;
        public CalamityPlayer moddedOwner => Owner.Calamity();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
        }
        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.width = Projectile.height = 78;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (beamWidth <= 0.96f)
            {
                beamsize = true;
            }
            if (beamWidth >= 1.04f)
            {
                beamsize = false;
            }
            beamWidth += (beamsize ? 0.015f : -0.015f);

            Projectile.scale = beamWidth;

            if (Projectile.timeLeft >= 240)
            {
                int dustAmount = 200;
                for (int d = 0; d < dustAmount; d++)
                {
                    float angle = MathHelper.TwoPi / dustAmount * d;
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 40f);

                    Dust spawnDust = Dust.NewDustPerfect(Projectile.Center, DustID.TreasureSparkle, velocity);
                    spawnDust.noGravity = true;
                    spawnDust.scale = velocity.Length() * 0.05f;
                    spawnDust.velocity *= 0.4f;
                }
            }
            // Stay on the player's head
            Projectile.Center = (Owner.MountedCenter + new Vector2(0, -45));

            // Emit some light
            Vector3 Light = new Vector3(0.251f, 0.255f, 0.219f);
            Lighting.AddLight(Projectile.Center, Light * 5);
        }
        public override void OnKill(int timeLeft)
        {
            float numberOfDusts = 40f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(8f, 0).RotatedBy(rot);
                Vector2 velOffset = new Vector2(10.5f, 0).RotatedBy(rot);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.FireworksRGB, new Vector2(velOffset.X, velOffset.Y));
                dust.noGravity = true;
                dust.velocity = velOffset * Main.rand.NextFloat(0.2f, 1.1f);
                dust.scale = Main.rand.NextFloat(0.3f, 0.8f);
                dust.color = Main.rand.NextBool(3) ? Color.LightGreen : Color.Khaki;
            }

            SoundStyle s = new("CalamityMod/Sounds/Item/AscendantActivate");
            SoundEngine.PlaySound(s with { Volume = 0.3f, Pitch = -0.9f }, Projectile.Center);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D rTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/AscendantAura").Value;
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;
            Color drawColor = Color.Lerp(Color.Khaki, Color.LightGreen, Utils.GetLerpValue(240, -100, Projectile.timeLeft, true));

            Vector2 bScale = new Vector2(Main.rand.NextFloat(0.15f, 0.2f), 0.5f) * Projectile.scale * 0.04f * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true) * Main.rand.NextFloat(0.7f, 1.3f);
            
            Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition + new Vector2(-30 * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), 0) * Main.rand.NextFloat(0.6f, 1.3f), null, drawColor with { A = 0 } * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), MathHelper.ToRadians(90f), bTexture.Size() * 0.5f, bScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition + new Vector2(30 * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), 0) * Main.rand.NextFloat(0.6f, 1.3f), null, drawColor with { A = 0 } * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), MathHelper.ToRadians(-90f), bTexture.Size() * 0.5f, bScale, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition + new Vector2(0, -30 * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true)) * Main.rand.NextFloat(0.6f, 1.3f), null, drawColor with { A = 0 } * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), 0, bTexture.Size() * 0.5f, bScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition + new Vector2(0, 30 * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true)) * Main.rand.NextFloat(0.6f, 1.3f), null, drawColor with { A = 0 } * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), MathHelper.ToRadians(180f), bTexture.Size() * 0.5f, bScale, SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 bScale2 = new Vector2(0.1f, 0.8f) * Projectile.scale * 0.04f * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true) * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 bVel = new Vector2(20 * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), 0).RotatedBy(MathHelper.ToRadians(45f + (i * 90))) * Main.rand.NextFloat(0.6f, 1.3f);
                Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition + bVel, null, drawColor with { A = 0 } * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), bVel.ToRotation() + MathHelper.ToRadians(90f), bTexture.Size() * 0.5f, bScale2, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(rTexture, Projectile.Center - Main.screenPosition, null, drawColor with { A = 0 } * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), Projectile.rotation, rTexture.Size() * 0.5f, Projectile.scale * 0.4f * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), SpriteEffects.None, 0);
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
