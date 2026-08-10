using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class AndromedaRegislash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 582;
            Projectile.height = 304;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 0f;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.light = 3f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.localAI[0] == 0f)
            {
                if (player.ownedProjectileCounts[Projectile.type] > 1)
                {
                    Projectile.Kill();
                    return;
                }
                SoundEngine.PlaySound(SoundID.DD2_DrakinShot, Projectile.Center);

                // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                Projectile.rotation = Projectile.AngleTo(Main.MouseWorld);
                Projectile.localAI[0] = 1f;
            }
            Projectile.position = player.Center - Projectile.Size / 2f;
            if (Math.Abs(Math.Cos(Projectile.rotation)) > 0.675f)
            {
                Projectile.position.X += Math.Sign(Math.Cos(Projectile.rotation)) * 295f;
            }
            Projectile.position.Y += (float)Math.Sin(Projectile.rotation) * 325f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 7 == 6)
            {
                Projectile.frame++;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.Kill();
            }
            Projectile.direction = ((player.Center.X - Projectile.Center.X) < 0).ToDirectionInt();
            Projectile.spriteDirection = Projectile.direction;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 startPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[Type];
            int frameY = frameHeight * Projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = Projectile.rotation;
            float scale = Projectile.scale;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(texture, startPos, rectangle, Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0);
            return false;
        }
    }
}
