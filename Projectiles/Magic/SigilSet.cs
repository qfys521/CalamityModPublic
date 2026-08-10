using System;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Content;

namespace CalamityMod.Projectiles.Magic
{
    public class SigilSet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float FadeoutFlag => ref Projectile.ai[2];
        private const float RuneLerpTime = 22f;
        private const float RuneDelayTime = 3f;
        private const float StartRadius = 240f;
        private const float BaseRadius = 150f;
        private const float GhostFlashDuration = 22f;
        public ref float RuneTimer => ref Projectile.localAI[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 550;
            Projectile.height = 550;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Culling Logic
            Player p = Main.player[Projectile.owner];
            if (p == null || !p.active || p.dead || p.HeldItem.type != ModContent.ItemType<UnstableCastersGauntlet>())
            {
                Projectile.Kill();
                return;
            }

            // Visuals
            Projectile.Center = p.MountedCenter + Vector2.UnitY * p.gfxOffY;
            Projectile.rotation += 0.01f;
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);

            if (Projectile.frameCounter++ > 3)
            {
                Projectile.frameCounter = 0;
                if (Projectile.frame++ > 2)
                {
                    Projectile.frame = 0;
                }
            }

            // Fading logic
            if (FadeoutFlag == 1f)
            {
                Projectile.alpha += 13;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.alpha = Utils.Clamp(Projectile.alpha - 25, 0, 255);
            }

            RuneTimer++;


            if (Projectile.ai[1] == 0) // spawn flag 
            {
                int[] sigilTypes = new int[]
                {
                    ModContent.ProjectileType<IgnisSigil>(),
                    ModContent.ProjectileType<AquaSigil>(),
                    ModContent.ProjectileType<TerraSigil>(),
                    ModContent.ProjectileType<AerSigil>(),
                    ModContent.ProjectileType<OrdoSigil>(),
                    ModContent.ProjectileType<PerditoSigil>()
                };

                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, sigilTypes[i], Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.identity, i);
                }

                // Make it not happen again
                Projectile.ai[1] = 1;
            }
            else // Sigils exist 
            {
                int activeSigilCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.ai[0] == Projectile.identity && proj.owner == Projectile.owner)
                    {
                        int projType = proj.type;

                        // Check if it's any sigil that occupies a slot
                        if (projType == ModContent.ProjectileType<IgnisSigil>() ||
                            projType == ModContent.ProjectileType<AquaSigil>() ||
                            projType == ModContent.ProjectileType<TerraSigil>() ||
                            projType == ModContent.ProjectileType<AerSigil>() ||
                            projType == ModContent.ProjectileType<OrdoSigil>() ||
                            projType == ModContent.ProjectileType<PerditoSigil>() ||
                            projType == ModContent.ProjectileType<WarpSigil>())
                        {

                            if (proj.ai[2] <= 0)
                            {
                                activeSigilCount++;
                            }
                        }
                    }
                }

                // If no non-fading sigils remain, start timer
                if (activeSigilCount == 0)
                {
                    Projectile.localAI[0]++;
                    if (Projectile.localAI[0] >= 50)
                    {
                        FadeoutFlag = 1f;
                    }
                }
                else
                {
                    Projectile.localAI[0] = 0;
                }
            }
            Projectile.timeLeft = 4;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D main = TextureAssets.Projectile[Type].Value;
            Texture2D smol = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/ThaumRingSmall").Value;
            Texture2D rune = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/ThaumRune").Value;
            Asset<Texture2D> ghostTextureAsset = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/ThaumRuneGhost");

            // Modify opacity again so it doesn't override changes to alpha in AI
            float drawOpacity = 1f - Projectile.alpha / 255f;
            Main.EntitySpriteDraw(smol, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f * drawOpacity, Projectile.rotation, smol.Size() / 2, Projectile.scale + MathF.Cos(2 * Main.GlobalTimeWrappedHourly + 5) * 0.0027f, SpriteEffects.None);
            Main.EntitySpriteDraw(main, Projectile.Center - Main.screenPosition, main.Frame(1, 4, 0, Projectile.frame), Color.White * 0.5f * drawOpacity, Projectile.rotation, new Vector2(main.Width / 2, main.Height / 8), Projectile.scale, SpriteEffects.None);

            // -- Normal Runes --
            for (int i = 0; i < 14; i++)
            {
                float runeStartTick = i * RuneDelayTime;

                float t = MathHelper.Clamp((RuneTimer - runeStartTick) / RuneLerpTime, 0f, 1f);
                float ease = MathHelper.SmoothStep(0f, 1f, t);

                float baseDist = BaseRadius + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 10;
                float lerpedRadius = MathHelper.Lerp(StartRadius, baseDist, ease);

                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 circleOffset = Vector2.UnitX.RotatedBy(angle) * lerpedRadius;
                circleOffset = circleOffset.RotatedBy(-Projectile.rotation * 0.6f);
                Vector2 animatedPos = Projectile.Center + circleOffset;
                float runeAlpha = ease;

                Main.EntitySpriteDraw(rune, animatedPos - Main.screenPosition, rune.Frame(1, 7, 0, i % 6), Color.White * 0.5f * drawOpacity * runeAlpha, (animatedPos - Projectile.Center).ToRotation() + MathHelper.PiOver2, new Vector2(rune.Width / 2, rune.Height / 14), Projectile.scale, SpriteEffects.None);
            }


            // --- White/Flash layer --
            // Used in spawn anim for the runes
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D ghostTexture = ghostTextureAsset.Value;

            for (int i = 0; i < 14; i++)
            {
                float runeStartTick = i * RuneDelayTime;
                float t = MathHelper.Clamp((RuneTimer - runeStartTick) / RuneLerpTime, 0f, 1f);
                float ease = MathHelper.SmoothStep(0f, 1f, t);

                float peakTime = 0.25f;
                float endTime = 1f;

                float rampUp = Utils.GetLerpValue(0f, peakTime, t, clamped: true);
                float rampDown = Utils.GetLerpValue(endTime, peakTime, t, clamped: true);

                float ghostOpacity = Math.Min(rampUp, rampDown);

                if (ghostOpacity > 0f)
                {
                    // Recalculate position (same as solid rune)
                    float baseDist = BaseRadius + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 10;
                    float lerpedRadius = MathHelper.Lerp(StartRadius, baseDist, ease);

                    float angle = MathHelper.TwoPi * i / 14f;
                    Vector2 circleOffset = Vector2.UnitX.RotatedBy(angle) * lerpedRadius;
                    circleOffset = circleOffset.RotatedBy(-Projectile.rotation * 0.6f);
                    Vector2 animatedPos = Projectile.Center + circleOffset;

                    Rectangle runeFrame = ghostTexture.Frame(1, 7, 0, i % 6);
                    Vector2 runeOrigin = new Vector2(runeFrame.Width / 2, runeFrame.Height / 2);

                    Main.EntitySpriteDraw(ghostTexture, animatedPos - Main.screenPosition, runeFrame, Color.White * drawOpacity * ghostOpacity * 0.6f, (animatedPos - Projectile.Center).ToRotation() + MathHelper.PiOver2, runeOrigin, Projectile.scale * (1f + ghostOpacity * 0.3f), SpriteEffects.None);
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        public override bool? CanDamage() => false;
    }
}
