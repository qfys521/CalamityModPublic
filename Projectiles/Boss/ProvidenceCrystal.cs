using System;
using System.IO;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class ProvidenceCrystal : ModProjectile, ILocalizedModType
    {
        public bool introAnimationDone = false;
        public float introAnimationProgress = 0f;
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = CalamityWorld.death ? 2100 : 3600;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        void CrystalExplosion()
        {
            Player proviTarget = Main.player[Main.npc[CalamityGlobalNPC.holyBoss].target];
            for (int i = 0; i < 7; i++)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, proviTarget.velocity, Color.Violet, "CalamityMod/Particles/BlastCone", new Vector2(Main.rand.NextFloat(2.5f, 6f), 3f), Main.rand.NextFloat(MathHelper.TwoPi), 1f, 0.5f, 20));
            }
            for (int i = 0; i < 4; i++)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, proviTarget.velocity, Color.Violet, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 3f, 0f, 35));
            }
        }

        public override void AI()
        {
            if (introAnimationProgress == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item101.WithPitchOffset(-1f), Projectile.Center);
            }

            introAnimationProgress += 0.025f;
            introAnimationProgress = MathHelper.Clamp(introAnimationProgress, 0f, 1f);

            if (introAnimationProgress == 1f)
            {
                if (!introAnimationDone)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact.WithPitchOffset(-0.5f), Projectile.Center);
                    SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);
                    CrystalExplosion();
                }
                introAnimationDone = true;
            }

            if (CalamityGlobalNPC.holyBoss < 0 || !Main.npc[CalamityGlobalNPC.holyBoss].active)
            {
                Projectile.active = false;
                Projectile.netUpdate = true;
                return;
            }

            if (Projectile.ai[2] > 0f)
            {
                if (Projectile.timeLeft > Projectile.ai[2])
                    Projectile.timeLeft = (int)Projectile.ai[2];
            }

            Player proviTarget = Main.player[Main.npc[CalamityGlobalNPC.holyBoss].target];

            Projectile.position.X = proviTarget.Center.X - (Projectile.width / 2);
            Projectile.position.Y = proviTarget.Center.Y - (Projectile.height / 2) + proviTarget.gfxOffY - 360f;
            if (proviTarget.gravDir == -1f)
            {
                Projectile.position.Y += 400f;
                Projectile.rotation = MathHelper.Pi;
            }
            else
                Projectile.rotation = 0f;

            Projectile.position.X = (int)Projectile.position.X;
            Projectile.position.Y = (int)Projectile.position.Y;
            Projectile.velocity = Vector2.Zero;

            Projectile.alpha -= 20;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            if (Projectile.alpha == 0 && Main.rand.NextBool(15))
            {
                Color BaseColor = ProvUtils.GetProjectileColor(0);
                float Brightness = 0.8f;
                Color DustColor = Color.Lerp(BaseColor, Color.White, Brightness);
                Dust crystalDust = Main.dust[Dust.NewDust(Projectile.Top, 0, 0, DustID.RainbowMk2, 0f, 0f, 100, DustColor, 1f)];
                crystalDust.velocity.X = 0f;
                crystalDust.noGravity = true;
                crystalDust.fadeIn = 1f;
                crystalDust.position = Projectile.Center + Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * (4f * Main.rand.NextFloat() + 26f);
                crystalDust.scale = 0.5f;
            }

            float lifeRatio = Projectile.ai[0];

            // Increment timer
            Projectile.localAI[0] += 1f;

            // Normally, fires every 300 frames but lasts 1500-3600 frames.
            // When enraged, fires every 30 frames but lasts 210 frames.
            // In GFB, fires at enraged rate for the first 210 frames, then at standard rate for the next 1500.
            bool standardAI = ProvUtils.StandardAI() || (Main.zenithWorld && Projectile.timeLeft < 1500);

            if (Projectile.localAI[0] >= (standardAI ? 300f : 30f))
            {
                // Spawn shards every 30 frames while enraged or at 300 frames normally
                if (Projectile.localAI[0] % 30f == 0f || standardAI)
                {
                    SoundEngine.PlaySound(SoundID.Item109, Projectile.Center);
                    Projectile.netUpdate = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        int totalProjectiles = standardAI ? 15 : (Projectile.localAI[0] % 60f == 0f ? 15 : 10);
                        float speedX = standardAI ? -21f : -15f;
                        float speedAdjustment = Math.Abs(speedX * 2f / (totalProjectiles - 1));
                        float speedY = -3f;
                        for (int i = 0; i < totalProjectiles; i++)
                        {
                            float x4 = standardAI ? Main.rgbToHsl(new Color(255, 200, Main.DiscoB)).X : Main.rgbToHsl(new Color(Main.DiscoR, 200, 255)).X;
                            float randomSpread = standardAI ? 0f : Main.rand.Next(-150, 151) * 0.01f * (1f - lifeRatio);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, speedX + speedAdjustment * i + randomSpread, speedY, ModContent.ProjectileType<ProvidenceCrystalShard>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, x4, Projectile.whoAmI);
                        }
                        CrystalExplosion();
                    }

                    // Reset timer
                    if (Projectile.localAI[0] >= 60f)
                        Projectile.localAI[0] = 0f;
                }
            }
        }

        public override bool CanHitPlayer(Player target) => false;

        public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (introAnimationDone)
            {
                Color colorArea = Lighting.GetColor((int)(Projectile.position.X + Projectile.width * 0.5) / 16, (int)((Projectile.position.Y + Projectile.height * 0.5) / 16.0));
                Vector2 drawArea = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                Texture2D texture2D34 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
                Rectangle textureRect = texture2D34.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
                Color colorAlpha = Projectile.GetAlpha(colorArea);
                Vector2 halfRect = textureRect.Size() / 2f;
                float scaleFactor = (float)Math.Cos(MathHelper.TwoPi * (Projectile.localAI[0] / 60f)) + 3f + 3f;
                for (float i = 0f; i < 4f; i += 0.5f)
                {
                    double angle = i * MathHelper.PiOver2;
                    Vector2 center = default;
                    Main.spriteBatch.Draw(texture2D34, drawArea + Vector2.UnitY.RotatedBy(angle, center) * scaleFactor, new Microsoft.Xna.Framework.Rectangle?(textureRect), Color.Violet.MultiplyRGBA(new Color(1f, 1f, 1f, 0f)), Projectile.rotation, halfRect, Projectile.scale, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(texture2D34, Projectile.Center - Main.screenPosition, texture2D34.Frame(), Color.White, 0f, texture2D34.Frame().Center(), 1f, SpriteEffects.None);
            }
            else
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/ProvidenceCrystal_Halves");
                for (int i = 0; i < 2; i++)
                {
                    float dir = i == 0 ? -1 : 1;
                    float ii = MathHelper.Lerp(60, 0, CalamityUtils.CircInEasing(introAnimationProgress, 1) - CalamityUtils.SineBumpEasing(introAnimationProgress, 1));
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center + (new Vector2(dir * ii).RotatedBy(MathHelper.ToRadians(MathHelper.Lerp(0, -90, CalamityUtils.CircInEasing(introAnimationProgress, 1))))) - Main.screenPosition, new Rectangle(i * 50, 0, 50, tex.Height()), Projectile.GetAlpha(lightColor).MultiplyRGBA(new Color(1f, 1f, 1f, 1f)), MathHelper.ToRadians(MathHelper.Lerp(70, 0, introAnimationProgress)), new Vector2(25, tex.Height() / 2), 1f, SpriteEffects.None);
                }
            }
            return false;
        }
    }
}
