using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class EXPLODINGFROG : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public const float MinExplodeDistance = 96f;
        public const float ExplodeWaitTime = 120f;
        public const float ExplosionAngleVariance = 0.8f;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 26;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[0] = 75;
                Projectile.ai[1] = 1;
            }
            if (Projectile.frameCounter++ > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }
            Player player = Main.player[Projectile.owner];
            bool canExplode = Projectile.Center.ClosestNPCAt(MinExplodeDistance) != null;
            if (player.HasMinionAttackTargetNPC && !canExplode)
            {
                canExplode = Main.npc[player.MinionAttackTargetNPC].Distance(Projectile.Center) < MinExplodeDistance;
            }
            if (Projectile.ai[0] < ExplodeWaitTime)
                Projectile.ai[0]++;
            if (Projectile.ai[0] >= ExplodeWaitTime)
            {
                if (canExplode)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Vector2 rotOffset = -Vector2.UnitY;
                        if (player.HasMinionAttackTargetNPC && Main.npc[player.MinionAttackTargetNPC].Distance(Projectile.Center) < MinExplodeDistance + Math.Max(Main.npc[player.MinionAttackTargetNPC].width * 0.5f, Main.npc[player.MinionAttackTargetNPC].height * 0.5f))
                            rotOffset = (Projectile.DirectionTo(Main.npc[player.MinionAttackTargetNPC].Center));
                        else
                        {
                            var tar = Projectile.Center.ClosestNPCAt(MinExplodeDistance);
                            if (tar != null)
                                rotOffset = (Projectile.DirectionTo(tar.Center));
                        }
                        // Goop projectiles
                        Vector2 direction = Vector2.Lerp(-Vector2.UnitY, rotOffset, 0.25f).SafeNormalize(-Vector2.UnitY);
                            for (int i = 0; i < 3; i++)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                                    (direction * Main.rand.NextFloat(6f, 10f)).RotatedByRandom(ExplosionAngleVariance),
                                    ModContent.ProjectileType<FrogGore1>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                                    (direction * Main.rand.NextFloat(6f, 10f)).RotatedByRandom(ExplosionAngleVariance),
                                    ModContent.ProjectileType<FrogGore2>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                            }
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                            (direction * Main.rand.NextFloat(6f, 10f)).RotatedByRandom(ExplosionAngleVariance),
                            ModContent.ProjectileType<FrogGore3>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                            (direction * Main.rand.NextFloat(6f, 10f)).RotatedByRandom(ExplosionAngleVariance),
                            ModContent.ProjectileType<FrogGore4>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                            (direction * Main.rand.NextFloat(6f, 10f)).RotatedByRandom(ExplosionAngleVariance),
                            ModContent.ProjectileType<FrogGore5>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    // WoF vomit sound.
                    SoundEngine.PlaySound(SoundID.NPCDeath13, Projectile.Center);
                    Projectile.ai[0] = 0;

                    //do a little hop
                    if (Projectile.velocity.Length() < 0.1f)
                        Projectile.velocity.Y -= 5;
                }
            }
            Projectile.velocity.Y += 0.5f;

            if (Projectile.velocity.Y > 10f)
            {
                Projectile.velocity.Y = 10f;
            }
        }

        public override bool? CanDamage() => false;

        // Don't die on tile collision
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0.8f;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
        private static Texture2D GlowOutline = null;
        public static Texture2D GetGlowOutline()
        {
            if (GlowOutline == null)
            {
                var texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/EXPLODINGFROG").Value;
                GlowOutline = new Texture2D(Main.graphics.GraphicsDevice, texture.Width, texture.Height);

                var BaseArray = new Color[GlowOutline.Width * GlowOutline.Height];
                var ColorArray = new Color[GlowOutline.Width * GlowOutline.Height];
                texture.GetData(BaseArray);
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
                }
                GlowOutline.SetData(ColorArray);
            }
            return GlowOutline;
        }

        public void ApplyGlowOutline(Texture2D tex, Rectangle? frame = null, float rotationOffset = 0, float borderPixels = 1, float opacity = 1)
        {
            Color color = Color.GreenYellow;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + new Vector2(2, 0) * borderPixels, frame, color * opacity, Projectile.rotation - rotationOffset, new Vector2(frame.Value.Width,frame.Value.Height) *0.5f, Projectile.scale, (Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, 2) * borderPixels, frame, color * opacity, Projectile.rotation - rotationOffset, new Vector2(frame.Value.Width, frame.Value.Height) * 0.5f, Projectile.scale, (Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + new Vector2(-2, 0) * borderPixels, frame, color * opacity, Projectile.rotation - rotationOffset, new Vector2(frame.Value.Width, frame.Value.Height) * 0.5f, Projectile.scale, (Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, -2) * borderPixels, frame, color * opacity, Projectile.rotation - rotationOffset, new Vector2(frame.Value.Width, frame.Value.Height) * 0.5f, Projectile.scale, (Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            var tex = GetGlowOutline();
            var frame = tex.Frame(1, 5, 0, Projectile.frame);
            ApplyGlowOutline(tex, frame, 0, MathF.Pow(Math.Clamp((Projectile.ai[0]-90)/(ExplodeWaitTime-90), 0, 1), 2),1);

            Lighting.AddLight(Projectile.Center, Color.GreenYellow.ToVector3()*(MathF.Pow(Math.Clamp((Projectile.ai[0] - 90) / (ExplodeWaitTime - 90), 0, 1), 2)));
            tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor * Projectile.Opacity, Projectile.rotation, new Vector2(frame.Width, frame.Height) * 0.5f, Projectile.scale, (Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            return false;
        }
    }
}
