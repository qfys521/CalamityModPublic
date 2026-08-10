using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class EntropicFlechette : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public float sizeVariance = 1;
        public int time = 60;
        public int spinDir = 100;
        public int waveOften = 40;
        NPC target;

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.penetrate = 5;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration = 15;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (spinDir == 100)
            {
                spinDir = Main.rand.NextBool() ? 1 : -1;
                waveOften = Main.rand.Next(10, 40 + 1);
                Projectile.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            if (Projectile.numHits > 0)
                Projectile.extraUpdates = 2;
            else
                Projectile.extraUpdates = 1;

            sizeVariance = Utils.GetLerpValue(-5, 60, Projectile.timeLeft, true);

            if (time > 65)
            {
                if (Main.myPlayer == Projectile.owner)
                { 
                    Particle spark2 = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.4f, "CalamityMod/Particles/GlowSpark2", false, (int)MathHelper.Clamp(9 * sizeVariance, 3, 9), MathHelper.Clamp(0.03f * sizeVariance, 0.01f, 0.03f), Color.Black * 0.6f, new Vector2(1.2f, 0.5f), false, shrinkSpeed: 1.1f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }

                if (Main.rand.NextBool(8))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<VoidDustInverted>());
                    dust.scale = Main.rand.NextFloat(0.6f, 1.1f);
                    dust.velocity = -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.7f);
                    dust.noGravity = true;
                    dust.color = Color.LightGreen;
                    dust.noLightEmittance = true;
                }
            }
            if (time >= 35)
            {
                if (Projectile.numHits < 1)
                {
                    target = Projectile.Center.ClosestNPCAt(320);

                    if (target == null)
                    {
                        Vector2 moveToMouse = (Owner.ClampedMouseWorld() - Projectile.Center).SafeNormalize(Vector2.UnitX);
                        if (Projectile.velocity.Length() < 14)
                            Projectile.velocity += moveToMouse * 0.15f;
                        else
                            Projectile.velocity *= 0.95f;

                        if (time % waveOften == 0)
                            spinDir *= -1;

                        //Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(0.05f, 0.15f) * spinDir * Utils.GetLerpValue(60, 180, time, true));
                    }
                    else
                    {
                        CalamityUtils.HomeInOnSelectedNPC(Projectile, target, true, 0.5f, 13, 0.98f);
                    }
                }
                else
                {
                    if (time % waveOften == 0)
                        spinDir *= -1;

                    Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(0.03f, 0.07f) * spinDir * Utils.GetLerpValue(60, 180, time, true));
                }
            }

            time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits == 0)
            {
                Projectile.netUpdate = true;
                SoundStyle sound = new("CalamityMod/Sounds/Item/MeldBurn");
                SoundEngine.PlaySound(sound with { Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 9; i++)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustStyle, Projectile.velocity);
                dust.scale = Main.rand.NextFloat(0.5f, 1.2f);
                dust.velocity = Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 2.1f);
                dust.noGravity = true;
                dust.color = Color.LightGreen;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/WaterFlavored");
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;
            float minSpeed = 6;
            float maxSpeed = 14;
            float totalScale = Projectile.scale * sizeVariance;
            for (int i = 0; i < 8; i++)
            {
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * Utils.Remap(Projectile.velocity.Length(), minSpeed, maxSpeed, 30, 7, true) * totalScale;
                Vector2 aimDir = Utils.DirectionTo(Projectile.Center + rotationalDrawOffset, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 92);
                Main.EntitySpriteDraw(tex.Value, generalDrawPos + rotationalDrawOffset, null, Color.Black * 0.8f, aimDir.ToRotation() + MathHelper.ToRadians(-90), tex.Size() * 0.5f, new Vector2(0.3f, 1) * totalScale, SpriteEffects.None);
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 3f).ToRotationVector2() * 3;
                Main.EntitySpriteDraw(tex2.Value, generalDrawPos + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 30, null, Color.LightGreen with { A = 0 }, Projectile.rotation, tex2.Size() * 0.5f, new Vector2(Utils.Remap(Projectile.velocity.Length(), minSpeed, maxSpeed, 0.7f, 0.25f, true), Utils.Remap(Projectile.velocity.Length(), minSpeed, maxSpeed, 0.7f, 1.7f, true)) * totalScale * 0.2f, SpriteEffects.None);
            }
            return false;
        }
    }
}
