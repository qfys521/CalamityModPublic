using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CircletTornado : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/TornadoProj";

        public static float Lifetime = 900f;
        public static float Fadetime = 120f;

        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = -1;
                SoundEngine.PlaySound(SoundID.Item82, Projectile.Center);
            }

            Time++;
            if (Time >= Lifetime)
                Projectile.Kill();
            if (Projectile.numHits >= 30)
            {
                Projectile.damage = 0;
                if (Time < Lifetime - Fadetime)
                {
                    Time = Lifetime - Fadetime + Time % 60f;
                    Projectile.netUpdate = true;
                }
            }

            GetVerticallyExpandedPos(out Vector2 newTop, out Vector2 newBottom, out Vector2 newCenter, out Vector2 newSize);
            Projectile.width = (int)(newSize.X * 0.65f);
            Projectile.height = (int)newSize.Y;
            Projectile.Center = newCenter;
            if (Projectile.owner == Main.myPlayer)
            {
                bool breakFlag = false;
                Vector2 playerCenter = Main.player[Projectile.owner].Center;
                Vector2 playerTop = Main.player[Projectile.owner].Top;
                for (float i = 0f; i < 1f; i += 0.05f)
                {
                    Vector2 collisionPos = Vector2.Lerp(newTop, newBottom, i);
                    if (Collision.CanHitLine(collisionPos, 0, 0, playerCenter, 0, 0) || Collision.CanHitLine(collisionPos, 0, 0, playerTop, 0, 0))
                    {
                        breakFlag = true;
                        break;
                    }
                }
                if (!breakFlag && Time < Lifetime - Fadetime)
                {
                    Time = Lifetime - Fadetime + Time % 60f;
                    Projectile.netUpdate = true;
                }
            }
            if (Time < Lifetime - Fadetime)
            {
                float randFactor = Main.rand.NextFloat();
                Vector2 randomOffset = new Vector2(MathHelper.Lerp(0.1f, 1f, Main.rand.NextFloat()) * MathHelper.Lerp(-2.2f, -0.6f, randFactor), MathHelper.Lerp(-0.5f, 0.9f, randFactor));
                Vector2 fixedOffset = new Vector2(6f, 10f);
                Vector2 dustPos = newCenter + newSize * randomOffset * 0.5f + fixedOffset;
                Dust sand = Dust.NewDustDirect(dustPos, 0, 0, DustID.Sandnado);
                sand.position = dustPos;
                sand.customData = newCenter + fixedOffset;
                sand.fadeIn = 1f;
                sand.scale = 0.3f;
                if (randomOffset.X > -1.2f)
                    sand.velocity.X = 1f + Main.rand.NextFloat();
                sand.velocity.Y = Main.rand.NextFloat() * -0.5f - 1f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            GetVerticallyExpandedPos(out Vector2 newTop, out Vector2 newBottom, out Vector2 newCenter, out Vector2 newSize);

            float TextureFadetime = Lifetime - 60f;
            Color fullColor = new Color(212, 192, 100);
            float colorMult = (Time > TextureFadetime ? MathHelper.Lerp(1f, 0f, (Time - TextureFadetime) / 60f) : MathHelper.Clamp(Time / 30f, 0f, 1f));
            float timedRotation = Time * MathHelper.Pi * -0.02f;

            float incrementStorage = 0f;
            float increment = 5.1f;
            for (float k = newBottom.Y; k > newTop.Y; k -= increment)
            {
                incrementStorage += increment;
                float segmentHeight = incrementStorage / newSize.Y;
                float addedRotation = incrementStorage * MathHelper.Pi * -0.1f;
                float addedScale = segmentHeight - 0.15f;
                Color drawColor =  Color.Lerp(Color.Transparent, fullColor, (segmentHeight > 0.5f ? 2f - segmentHeight * 2f : segmentHeight * 2f));
                drawColor.A = (byte)((float)drawColor.A * 0.5f);

                Vector2 drawPos = new Vector2(newBottom.X, k) - Main.screenPosition;
                Main.spriteBatch.Draw(tex, drawPos, null, drawColor * colorMult, timedRotation + addedRotation, tex.Size() * 0.5f, 1f + addedScale, SpriteEffects.None, 0);
            }
            return false;
        }

        public void GetVerticallyExpandedPos(out Vector2 newTop, out Vector2 newBottom, out Vector2 newCenter, out Vector2 newSize)
        {
            Point center = Projectile.Center.ToTileCoordinates();
            Collision.ExpandVertically(center.X, center.Y, out int topY, out int bottomY, 15, 15);
            newTop = new Vector2(center.X, topY + 1) * 16f + new Vector2(8f);
            newBottom = new Vector2(center.X, bottomY - 1) * 16f + new Vector2(8f);
            newCenter = Vector2.Lerp(newTop, newBottom, 0.5f);
            newSize = new Vector2(0f, newBottom.Y - newTop.Y);
            newSize.X = newSize.Y * 0.2f;
        }
    }
}
