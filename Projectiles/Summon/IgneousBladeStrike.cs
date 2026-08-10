using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class IgneousBladeStrike : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/Summon/IgneousBlade";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
            Projectile.timeLeft = 360;
            Projectile.alpha = 127;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.PiOver4;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(28, 41); i++)
            {
                Dust.NewDustPerfect(
                    Projectile.Center + Utils.NextVector2Unit(Main.rand) * Main.rand.NextFloat(10f),
                    DustID.Torch,
                    Utils.NextVector2Unit(Main.rand) * Main.rand.NextFloat(1f, 4f));
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {

            var AllWhiteTexture = IgneousExaltation.GetBladeOutlineTex();
            Main.spriteBatch.Draw(AllWhiteTexture, Projectile.Center + new Vector2(0,2) - Main.screenPosition, null, new Color(166, 46, 61), Projectile.rotation, AllWhiteTexture.Size()*0.5f, Projectile.scale * 1f, SpriteEffects.None, 1f);
            Main.spriteBatch.Draw(AllWhiteTexture, Projectile.Center + new Vector2(2, 0) - Main.screenPosition, null, new Color(166, 46, 61), Projectile.rotation, AllWhiteTexture.Size() * 0.5f, Projectile.scale * 1f, SpriteEffects.None, 1f);
            Main.spriteBatch.Draw(AllWhiteTexture, Projectile.Center + new Vector2(0, -2) - Main.screenPosition, null, new Color(166, 46, 61), Projectile.rotation, AllWhiteTexture.Size() * 0.5f, Projectile.scale * 1f, SpriteEffects.None, 1f);
            Main.spriteBatch.Draw(AllWhiteTexture, Projectile.Center + new Vector2(-2, 0) - Main.screenPosition, null, new Color(166, 46, 61), Projectile.rotation, AllWhiteTexture.Size() * 0.5f, Projectile.scale * 1f, SpriteEffects.None, 1f);
            var tex = TextureAssets.Projectile[Type].Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 1f, SpriteEffects.None, 1f);
            return false;
        }
    }
}
