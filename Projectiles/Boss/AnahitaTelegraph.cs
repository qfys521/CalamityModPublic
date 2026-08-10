using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class AnahitaTelegraph : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int Lifetime = 30;
        public ref float TeleType => ref Projectile.ai[1]; // 0: Water spear. 1: Frost mist. 2: Treble clef.
        public static Asset<Texture2D> WaterSpearTex;
        public static Asset<Texture2D> FrostMistTex;
        public static Asset<Texture2D> TrebleClefTex;
        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                WaterSpearTex = ModContent.Request<Texture2D>("CalamityMod/Particles/PointParticle");
                FrostMistTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
                TrebleClefTex = ModContent.Request<Texture2D>("CalamityMod/Particles/Sparkle");
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 15;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 255;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.rotation);
        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.rotation = reader.ReadSingle();

        private Color DetermineColor()
        {
            Color returnColor = Color.Black;
            switch (TeleType)
            {
                case 0:
                    returnColor = new Color(55, 70, 240);
                    break;
                case 1:
                    returnColor = Color.White * 0.9f;
                    break;
                case 2:
                    returnColor = new Color(199, 90, 67);
                    break;
            }
            return returnColor;
        }

        public override void AI()
        {
            Player target = Main.player[(int)Projectile.ai[0]];

            // Setting water spear and treble clef rotation on spawn
            if (Projectile.timeLeft == Lifetime)
            {
                switch (TeleType)
                {
                    case 0:
                        Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                        break;
                    case 2:
                        Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                        break;
                    default:
                        break;
                }
            }

            // Fade in and fade out
            if (Projectile.timeLeft > Math.Ceiling(Lifetime * 0.66f))
                Projectile.alpha -= 25;
            if (Projectile.timeLeft <= Math.Ceiling(Lifetime * 0.33f))
                Projectile.alpha += 25;

            // Constantly move with the targeted player
            Projectile.Center += target.velocity;

            // Dust
            if (Projectile.timeLeft % 3 == 0)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.TintableDustLighted, newColor: DetermineColor(), Scale: 0.75f);

            // Specific behavior for each telegraph
            switch (TeleType)
            {
                // Water spears: Move inward at constant speed
                case 0:
                    Projectile.Center += Vector2.Normalize(target.Center - Projectile.Center) * 8.5f;
                    break;
                // Frost mists: Move inward with accelerating speed
                case 1:
                    int accFactor = Lifetime - Projectile.timeLeft;
                    Projectile.Center += Vector2.Normalize(target.Center - Projectile.Center) * accFactor * 0.73f;
                    break;
                // Treble clefs: Stay in place and rotate
                case 2:
                    Projectile.rotation += 0.165f;
                    break;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex;
            Color telegraphColor = DetermineColor();

            switch (TeleType)
            {
                case 0: // Draws a pointing particle
                    tex = WaterSpearTex;
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, telegraphColor * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver2, tex.Value.Size() / 2f, new Vector2(0.75f, 1.75f), SpriteEffects.None);
                    break;
                case 1: // Draws a bloom circle
                    tex = FrostMistTex;
                    Main.spriteBatch.SetBlendState(BlendState.Additive);
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, telegraphColor * Projectile.Opacity * 0.75f, 0f, tex.Value.Size() / 2f, Projectile.scale * 0.7f, SpriteEffects.None);
                    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
                    break;
                case 2: // Draws two rotating stars
                    tex = TrebleClefTex;
                    Main.spriteBatch.SetBlendState(BlendState.Additive);
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, telegraphColor * Projectile.Opacity * 0.9f, Projectile.rotation, tex.Value.Size() / 2f, Projectile.scale * 2f, SpriteEffects.None);
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, telegraphColor * Projectile.Opacity * 0.9f, -Projectile.rotation, tex.Value.Size() / 2f, Projectile.scale * 2f, SpriteEffects.None);
                    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
                    break;
            }

            return false;
        }
    }
}
