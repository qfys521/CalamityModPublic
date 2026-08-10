using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class BlazingStarOrbital : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/BlazingStar";

        private static int Lifetime = 300;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 272;
            Projectile.height = 272;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 1;
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Projectile.Center = Main.player[Projectile.owner].Center;
            Projectile.rotation += 0.2f; Lifetime = 300;
            Projectile.ai[0]++;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float distance = MathHelper.Min(Projectile.ai[0] / 15f, 1f);
            if (Projectile.timeLeft < 15)
            {
                distance = Projectile.timeLeft / 15f;
            }
            int firstRingGlaives = 3;
            int secondRingGlaives = 6;
            for (var i2 = 0; i2 < 5; i2++)//Afteriamges
            {
                for (var i = 0; i < firstRingGlaives; i++)
                {
                    float glaiveRot = MathHelper.TwoPi * ((i / (float)firstRingGlaives));
                    float opacity = i2 / 5f;
                    Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center + new Vector2(distance * 64, 0).RotatedBy(Projectile.rotation + glaiveRot + (1f * i2 / 5f)) - Main.screenPosition, null, lightColor * opacity, Main.GlobalTimeWrappedHourly * -10, TextureAssets.Projectile[Type].Size() * 0.5f, 0.5f * opacity + 0.5f, 0, 1); ;
                }
                for (var i = 0; i < secondRingGlaives; i++)
                {
                    float glaiveRot = MathHelper.TwoPi * ((i / (float)secondRingGlaives));
                    float opacity = 1 - i2 / 5f;
                    Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center + new Vector2(distance * 128, 0).RotatedBy(-Projectile.rotation * 0.5f + glaiveRot + (0.45f * i2 / 5f)) - Main.screenPosition, null, lightColor * opacity, Main.GlobalTimeWrappedHourly * 10, TextureAssets.Projectile[Type].Size() * 0.5f, 0.5f * opacity + 0.5f, 0, 1); ;
                }
            }
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = -target.DirectionTo(Main.player[Projectile.owner].Center).X.DirectionalSign();
        }
    }
}
