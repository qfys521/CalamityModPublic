using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class CosmicSpiritBomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 150;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
        {
            float scaleModd = (float)Main.mouseTextColor / 200f - 0.35f;
            scaleModd *= 0.2f;
            Projectile.scale = scaleModd + 0.95f;

            float projDistance = (Projectile.Center - Main.player[Projectile.owner].Center).Length() / 100f;
            if (projDistance <= 2f)
                projDistance = 1f;
            else
                projDistance *= 1.33f;

            Projectile.velocity = Vector2.Normalize(Main.player[Projectile.owner].Center - Projectile.Center) * projDistance;
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, Projectile.alpha);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int buffType = -1;
            switch (Projectile.ai[0])
            {
                case 0:
                    buffType = BuffID.Frostburn2;
                    break;
                case 1:
                    buffType = BuffID.OnFire3;
                    break;
                case 2:
                    buffType = BuffID.Ichor;
                    break;
            }
            target.AddBuff(buffType, 120);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.ExpandHitboxBy(200);

            int dustType = -1;
            switch (Projectile.ai[0])
            {
                case 0:
                    dustType = DustID.MagicMirror;
                    break;
                case 1:
                    dustType = DustID.PinkFairy;
                    break;
                case 2:
                    dustType = DustID.CopperCoin;
                    break;
            }
            for (int k = 0; k < 10; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, dustType, Projectile.oldVelocity.X * 2.5f, Projectile.oldVelocity.Y * 2.5f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            string textureName = "";
            switch (Projectile.ai[0])
            {
                case 0:
                    textureName = Texture;
                    break;
                case 1:
                    textureName = "CalamityMod/Projectiles/Melee/CosmicSpiritBomb2";
                    break;
                case 2:
                    textureName = "CalamityMod/Projectiles/Melee/CosmicSpiritBomb3";
                    break;
            }
            Texture2D realTexture = ModContent.Request<Texture2D>(textureName).Value;
            Main.EntitySpriteDraw(realTexture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, realTexture.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
