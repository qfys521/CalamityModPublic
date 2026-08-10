using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class PlasmaGrenadeSmallExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int frameX = 0;
        public int frameY = 0;
        private const int horizontalFrames = 2;
        private const int verticalFrames = 7;
        private const int frameLength = 5;
        private const float radius = 139.5f;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 279;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter % frameLength == frameLength - 1)
            {
                frameY++;
                if (frameY >= verticalFrames)
                {
                    frameX++;
                    frameY = 0;
                }
                if (frameX >= horizontalFrames)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.CursedInferno, 180);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int length = texture.Width / horizontalFrames;
            int height = texture.Height / verticalFrames;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle frame = new Rectangle(frameX * length, frameY * height, length, height);
            Vector2 origin = new Vector2(length / 2f, height / 2f);
            Main.EntitySpriteDraw(texture, drawPos, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
