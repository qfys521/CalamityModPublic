using System;
using System.IO;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolySpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        Vector2 velocity = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.WriteVector2(velocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            velocity = reader.ReadVector2();
        }

        public override void AI()
        {
            ProvUtils.ApplyGFBDamage(Projectile, 120, 10);

            Lighting.AddLight(Projectile.Center, 0.45f * Projectile.Opacity, 0.35f * Projectile.Opacity, 0f);

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;

                if (Projectile.ai[0] == 1f)
                    velocity = Projectile.velocity;
            }

            bool commanderSpear = Projectile.ai[0] == -1f;
            bool enragedCommanderSpear = Projectile.ai[0] == -2f;
            float timeGateValue = !ProvUtils.StandardAI() ? 420f : ((commanderSpear || enragedCommanderSpear) ? 360f : 540f);
            if (Projectile.ai[0] <= 0f)
            {
                Projectile.ai[1] += 1f;

                float slowGateValue = !ProvUtils.StandardAI() ? 60f : ((commanderSpear || enragedCommanderSpear) ? 30f : 90f);
                float fastGateValue = 30f;
                float minVelocity = !ProvUtils.StandardAI() ? 4f : (enragedCommanderSpear ? 6f : commanderSpear ? 4.5f : 3f);
                float maxVelocity = minVelocity * 4f;
                float extremeVelocity = maxVelocity * 2f;
                float deceleration = 0.95f;
                float acceleration = 1.2f;

                if (Projectile.localAI[1] >= timeGateValue)
                {
                    if (Projectile.velocity.Length() < extremeVelocity)
                        Projectile.velocity *= acceleration;
                }
                else
                {
                    if (Projectile.ai[1] <= slowGateValue)
                    {
                        if (Projectile.velocity.Length() > minVelocity)
                            Projectile.velocity *= deceleration;
                    }
                    else if (Projectile.ai[1] < slowGateValue + fastGateValue)
                    {
                        if (Projectile.velocity.Length() < maxVelocity)
                            Projectile.velocity *= acceleration;
                    }
                    else
                        Projectile.ai[1] = 0f;
                }
            }
            else
            {
                float frequency = !ProvUtils.StandardAI() ? 0.2f : 0.1f;
                float amplitude = !ProvUtils.StandardAI() ? 4f : 2f;

                Projectile.ai[1] += frequency;

                float wavyVelocity = (float)Math.Sin(Projectile.ai[1]);

                Projectile.velocity = velocity + new Vector2(wavyVelocity, wavyVelocity).RotatedBy(MathHelper.ToRadians(velocity.ToRotation())) * amplitude;
            }

            if (Projectile.localAI[1] < timeGateValue)
            {
                Projectile.localAI[1] += 1f;

                if (Projectile.timeLeft < 160)
                    Projectile.timeLeft = 160;
            }

            Projectile.Opacity = MathHelper.Lerp(240f, 220f, Projectile.timeLeft);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            bool aimedSpear = Projectile.ai[0] > 0f;

            Color baseColor = new Color(255, aimedSpear ? 255 : 125, aimedSpear ? 25 : 125); // Default to standard yellow

            if (Main.zenithWorld)
            {
                if (Main.GlobalTimeWrappedHourly % 6f >= 5f) // Violet
                    baseColor = new Color(aimedSpear ? 125 : 255, aimedSpear ? 0 : 255, 125);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 4f) // Blue
                    baseColor = new Color(aimedSpear ? 100 : 175, aimedSpear ? 255 : 175, 255);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 3f) // Green
                    baseColor = new Color(0, 255, aimedSpear ? 0 : 175);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 2f) // Yellow
                    baseColor = new Color(255, aimedSpear ? 255 : 125, aimedSpear ? 25 : 125);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 1f) // Orange
                    baseColor = new Color(255, 125, aimedSpear ? 0 : 175);
                else // Red
                    baseColor = new Color(255, aimedSpear ? 0 : 125, aimedSpear ? 0 : 255);
            }
            else if (!ProvUtils.StandardAI())
                baseColor = new Color(aimedSpear ? 100 : 175, aimedSpear ? 255 : 175, 255);

            GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), Projectile.velocity, false, 15, Main.rand.NextFloat(0.5f, 1.5f) * MathHelper.Clamp(Projectile.velocity.Length() / 15, 0f, 2f), aimedSpear ? baseColor : ProvUtils.GetProjectileColor(255, false)));
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D drawTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            bool aimedSpear = Projectile.ai[0] > 0f;

            Color baseColor = new Color(255, aimedSpear ? 255 : 125, aimedSpear ? 25 : 125); // Default to standard yellow
            Color baseColor2 = new Color(15, 35, 50, 0);

            if (Main.zenithWorld)
            {
                if (Main.GlobalTimeWrappedHourly % 6f >= 5f) // Violet
                    baseColor = new Color(aimedSpear ? 125 : 255, aimedSpear ? 0 : 255, 125);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 4f) // Blue
                    baseColor = new Color(aimedSpear ? 100 : 175, aimedSpear ? 255 : 175, 255);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 3f) // Green
                    baseColor = new Color(0, 255, aimedSpear ? 0 : 175);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 2f) // Yellow
                    baseColor = new Color(255, aimedSpear ? 255 : 125, aimedSpear ? 25 : 125);
                else if (Main.GlobalTimeWrappedHourly % 6f >= 1f) // Orange
                    baseColor = new Color(255, 125, aimedSpear ? 0 : 175);
                else // Red
                    baseColor = new Color(255, aimedSpear ? 0 : 125, aimedSpear ? 0 : 255);
            }
            else if (!ProvUtils.StandardAI())
                baseColor = new Color(aimedSpear ? 100 : 175, aimedSpear ? 255 : 175, 255);

            if (!aimedSpear)
            {
                baseColor = ProvUtils.GetProjectileColor(0);
                baseColor2 = ProvUtils.GetProjectileColor(0, true);
            }

            Vector2 projDirection = Main.screenPosition - new Vector2(0f, Projectile.gfxOffY);
            Vector2 halfTextureSize = drawTexture.Size() / 2f;
            Color halfOfHalfBaseColor = baseColor2 * 0.5f;

            float squish = MathHelper.Clamp(Projectile.velocity.Length() / 20, -0.3f, 0.3f);

            Vector2 sc = new Vector2(1 - squish, 1 + squish);

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(drawTexture, Projectile.oldPos[i] - projDirection + (Projectile.Size / 2), null, Color.Lerp(baseColor, baseColor2, MathHelper.Lerp((float)i / (float)Projectile.oldPos.Length, 1f, 0.5f)), Projectile.rotation, halfTextureSize, sc * MathHelper.Lerp(1, 0, (float)((float)i / Projectile.oldPos.Length)), spriteEffects, 0);
                }
            }

            for (float i = 0; i < 360; i += 90)
            {
                Main.EntitySpriteDraw(drawTexture, Projectile.Center + new Vector2(4, 0).RotatedBy(MathHelper.ToRadians(i)) - projDirection, null, Color.Lerp(baseColor, baseColor2, 0.75f), Projectile.rotation, halfTextureSize, sc, spriteEffects, 0);
            }

            Main.EntitySpriteDraw(drawTexture, Projectile.Center - projDirection, null, baseColor, Projectile.rotation, halfTextureSize, sc, spriteEffects, 0);

            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // If the player is dodging, don't apply debuffs
            if (info.Damage <= 0 || target.creativeGodMode)
                return;

            ProvUtils.ApplyDebuffs(target, 120);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return ProvUtils.GetProjectileColor(0, false);
        }
    }
}
