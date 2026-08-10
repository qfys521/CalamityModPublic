using System;
using System.IO;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class BirbAura : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        float timer = 135f;
        float timeBeforeVanish = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1200;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.Write(timer);
            writer.Write(timeBeforeVanish);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            timer = reader.ReadSingle();
            timeBeforeVanish = reader.ReadSingle();
        }

        public override void AI()
        {
            Vector2? vector78 = null;

            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
                Projectile.velocity = -Vector2.UnitY;

            Vector2 fireFrom = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Projectile.position = fireFrom - new Vector2(Projectile.width, Projectile.height) / 2f;

            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
                Projectile.velocity = -Vector2.UnitY;

            if (timer > 0f)
                timer -= 1f;

            float projScale = 1f;
            if (timeBeforeVanish == 0f)
                timeBeforeVanish = Projectile.timeLeft <= 900 ? 900f : 1200f;

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= timeBeforeVanish)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.localAI[0] % 4 == 0)
                Projectile.frameCounter++;

            Projectile.scale = (float)Math.Sin(Projectile.localAI[0] * Math.PI / timeBeforeVanish) * 10f * projScale;
            if (Projectile.scale > projScale)
                Projectile.scale = projScale;

            float projVelRotation = Projectile.velocity.ToRotation();
            Projectile.rotation = projVelRotation - MathHelper.PiOver2;
            Projectile.velocity = projVelRotation.ToRotationVector2();

            float projWidth = Projectile.width;

            Vector2 samplingPoint = Projectile.Center;
            if (vector78.HasValue)
                samplingPoint = vector78.Value;

            float laserLength = Projectile.ai[1] - 160f;
            float[] array3 = new float[3];
            Collision.LaserScan(samplingPoint, Projectile.velocity, projWidth * Projectile.scale, laserLength, array3);
            float auraLength = 0f;
            for (int j = 0; j < array3.Length; j++)
            {
                auraLength += array3[j];
            }
            auraLength /= 3f;

            auraLength = MathHelper.Clamp(auraLength, 3600f, 4800f);

            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], auraLength, 0.5f);

            DelegateMethods.v3_1 = new Vector3(0.9f, 0.3f, 0.3f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.localAI[1], Projectile.width * Projectile.scale, DelegateMethods.CastLight);
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (timer <= 0f && (Projectile.localAI[0] >= 120f || Projectile.timeLeft <= 900))
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    timer = 15f;
                    Vector2 fireFrom = new Vector2(target.Center.X, target.Center.Y - 900f);
                    SoundEngine.PlaySound(CommonCalamitySounds.LightningSound, target.Center - Vector2.UnitY * 300f);
                    Vector2 ai0 = target.Center - fireFrom;
                    float ai = Main.rand.Next(100);
                    Vector2 velocity = Vector2.Normalize(ai0.RotatedByRandom(MathHelper.PiOver4)) * 7f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), fireFrom.X, fireFrom.Y, velocity.X, velocity.Y, ModContent.ProjectileType<RedLightning>(), Projectile.damage, 0f, Projectile.owner, ai0.ToRotation(), ai);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.velocity == Vector2.Zero)
                return false;

            Texture2D middleTex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D endpointTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/BirbAuraEndpoints", AssetRequestMode.ImmediateLoad).Value;

            // Used to track the animation of the segments
            int middleFrame = Projectile.frameCounter % 12;
            int endpointFrame = Projectile.frameCounter % 3;
            float auraDrawLength = Projectile.localAI[1]; // Length of the laser
            Color grayColor = new Color(128, 128, 128, 0);

            // Draw the bottom endpoint segment
            Rectangle? sourceRectangle2 = endpointTex.Frame(1, 3, 0, endpointFrame);
            Vector2 endPosition = Projectile.Center - Main.screenPosition - new Vector2(0f, 22f); // Of course it's 22, this stupid fucking accursed number
            Main.EntitySpriteDraw(endpointTex, endPosition, sourceRectangle2, grayColor, Projectile.rotation, endpointTex.Frame(1, 1, 0, 0).Top(), Projectile.scale, SpriteEffects.None, 0);

            // Draw the middle segments
            auraDrawLength -= (endpointTex.Height / 2 + endpointTex.Height) * Projectile.scale;
            Vector2 projCenter = Projectile.Center;
            projCenter += Projectile.velocity * Projectile.scale * endpointTex.Height / 2f;
            if (auraDrawLength > 0f) // Only draw a middle segment if there's enough height for one
            {
                float auraSegment = 0f;
                Rectangle drawRectangle = middleTex.Frame(12, 1, middleFrame, 0);
                while (auraSegment + 1f < auraDrawLength)
                {
                    if (auraDrawLength - auraSegment < drawRectangle.Height)
                    {
                        drawRectangle.Height = (int)(auraDrawLength - auraSegment);
                    }
                    Main.EntitySpriteDraw(middleTex, projCenter - Main.screenPosition, new Rectangle?(drawRectangle), grayColor, Projectile.rotation, new Vector2(drawRectangle.Width / 2, 0f), Projectile.scale, SpriteEffects.None, 0);
                    auraSegment += drawRectangle.Height * Projectile.scale;
                    projCenter += Projectile.velocity * drawRectangle.Height * Projectile.scale;
                    drawRectangle.Y += middleTex.Height;
                    if (drawRectangle.Y + drawRectangle.Height > middleTex.Height)
                    {
                        drawRectangle.Y = 0;
                    }
                }
            }
            // There is intentionally no top endpoint segment drawn
            return false;
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 unit = Projectile.velocity;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * Projectile.localAI[1], Projectile.width * Projectile.scale, DelegateMethods.CutTiles);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (timer > 15f)
            {
                return false;
            }
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }
            float useless = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.localAI[1], 80f * Projectile.scale, ref useless))
            {
                return true;
            }
            return false;
        }
    }
}
