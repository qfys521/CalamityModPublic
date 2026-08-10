using System.IO;
using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class SpiritOriginBullseye : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public static readonly int FadeinFrames = 8;
        public static readonly int FadeoutFrames = 15;
        public static readonly float StartingFadeinScale = 1.6f;
        public static readonly float FadeinScaleExponent = 0.95f;
        public static readonly float FadeinOpacityExponent = 0.7f;
        public static readonly float FadeoutFlatScaleBoost = 0.02f;
        public static readonly float FadeoutOpacityLoss = 0.06f;

        public Player Owner => Main.player[Projectile.owner];

        public NPC Target => Main.npc[(int)Projectile.ai[0]];

        private ref float FadeState => ref Projectile.ai[1];
        private ref float VisualScaleDiff => ref Projectile.localAI[0];

        public Vector2 BullseyeOffsetFromCenter;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)(2 * DaawnlightSpiritOrigin.RegularEnemyBullseyeRadius);
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = DaawnlightSpiritOrigin.BullseyeIdleLifetime;
            Projectile.Opacity = 0f;
            Projectile.penetrate = -1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(BullseyeOffsetFromCenter);

        public override void ReceiveExtraAI(BinaryReader reader) => BullseyeOffsetFromCenter = reader.ReadVector2();

        public override void AI()
        {
            if (!Main.npc.IndexInRange((int)Projectile.ai[0]) || !Owner.Calamity().spiritOrigin || !Target.active || Target.life <= 0 || Target.dontTakeDamage)
            {
                Projectile.Kill();
                return;
            }

            // If the projectile has only just started existing, rapidly increase its opacity and shrink it over the first few frames.
            if (Projectile.timeLeft > DaawnlightSpiritOrigin.BullseyeIdleLifetime - FadeinFrames)
            {
                FadeState = 1f;

                // On the very first frame, set its visual scale diff to the starting fade-in scale.
                if (VisualScaleDiff == 0f)
                    VisualScaleDiff = StartingFadeinScale;
                VisualScaleDiff *= FadeinScaleExponent;

                float negOpacity = 1f - Projectile.Opacity;
                negOpacity *= FadeinOpacityExponent;
                Projectile.Opacity = 1f - negOpacity;
            }

            // Otherwise, if the projectile is about to vanish, decrease its opacity accordingly and very slightly inflate it.
            else if (Projectile.timeLeft < FadeoutFrames)
            {
                FadeState = 2f;

                if (VisualScaleDiff < 0f)
                    VisualScaleDiff = 0f;
                VisualScaleDiff += FadeoutFlatScaleBoost;

                Projectile.Opacity -= FadeoutOpacityLoss;
            }

            // Otherwise keep its opacity at maximum and its scale default at all times.
            else
            {
                FadeState = 0f;
                Projectile.Opacity = 1f;
            }

            // If the bullseye is fading out and hits zero opacity for some reason, delete it immediately.
            if (Projectile.Opacity < 0f && FadeState == 2f)
                Projectile.Kill();

            if (BullseyeOffsetFromCenter == Vector2.Zero)
            {
                BullseyeOffsetFromCenter = Main.rand.NextVector2CircularEdge(Target.width, Target.height) * Main.rand.NextFloat(0.925f, 1f) * 0.54f;
                Projectile.netUpdate = true;
            }
            else
                Projectile.Center = Target.Center + BullseyeOffsetFromCenter;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Main.myPlayer != Projectile.owner)
                return false;

            Vector2 drawPosition = Target.Center + BullseyeOffsetFromCenter - Main.screenPosition;

            float scaleToUse = VisualScaleDiff;

            Texture2D bullseyeTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/SpiritOriginRegularBullseye").Value;
            Rectangle frame = bullseyeTexture.Frame();
            if (Target.IsABoss())
            {
                bullseyeTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/SpiritOriginBossBullseye").Value;
                frame = bullseyeTexture.Frame(1, 4, 0, (int)(Main.GlobalTimeWrappedHourly * 7f) % 4);
                drawPosition.Y -= 17;
                drawPosition.X -= 1;
            }

            Main.EntitySpriteDraw(bullseyeTexture, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, scaleToUse, SpriteEffects.None, 0);
            return false;
        }
    }
}
