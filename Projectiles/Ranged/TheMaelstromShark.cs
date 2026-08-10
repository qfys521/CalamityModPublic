using System;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TheMaelstromShark : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.Opacity = 0f;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = (Math.Cos(Projectile.rotation) > 0f).ToDirectionInt();
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.Pi;

            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.1f, 0f, 1f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fadeOpacity = Utils.GetLerpValue(0.94f, 0.54f, completionRatio, true) * Projectile.Opacity;
            return Color.Lerp(Color.Cyan, Color.White, 0.4f) * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
            return MathHelper.Lerp(0f, 12f * Projectile.Opacity, expansionCompletion);
        }

        public override void OnKill(int timeLeft)
        {
            // Create death effects for the shark, including a death sound, gore, and some blood.
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            if (!Main.dedServ)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("MaelstromReaperShark1").Type, Projectile.scale);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("MaelstromReaperShark2").Type, Projectile.scale);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("MaelstromReaperShark3").Type, Projectile.scale);
            }
            for (int i = 0; i < 12; i++)
            {
                Dust blood = Dust.NewDustPerfect(Projectile.Center, DustID.Blood);
                blood.velocity = Main.rand.NextVector2Circular(6f, 6f);
                blood.scale *= Main.rand.NextFloat(0.7f, 1.3f);
                blood.noGravity = true;
            }

            if (Main.myPlayer != Projectile.owner)
                return;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TheMaelstromExplosion>(), Projectile.damage, 0f, Projectile.owner);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 60);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            return false;
        }
    }
}
