using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class GlaiveOrbital : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Glaive";

        private static int Lifetime = 300;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 136;
            Projectile.height = 136;
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
            Projectile.rotation += 0.2f; Projectile.ai[0]++;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float distance = MathHelper.Min(MathHelper.Lerp(0, 64, Projectile.ai[0] / 15f), 64f);
            if (Projectile.timeLeft < 15)
            {
                distance = MathHelper.Lerp(0, 64, Projectile.timeLeft / 15f);
            }
            int glaiveCount = 3;
            for (var i2 = 0; i2 < 5; i2++)//Afteriamges
            {
                for (var i = 0; i < glaiveCount; i++)
                {
                    float glaiveRot = MathHelper.TwoPi * ((i / (float)glaiveCount));
                    float opacity = i2 / 5f;
                    Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center + new Vector2(distance, 0).RotatedBy(Projectile.rotation + glaiveRot + (1f * i2 / 5f)) - Main.screenPosition, null, lightColor * opacity, Main.GlobalTimeWrappedHourly * -10, TextureAssets.Projectile[Type].Size() * 0.5f, 0.5f * opacity + 0.5f, 0, 1); ;
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
