using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class EnchantedKnifeStaffProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.width = Projectile.height = 32;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            if (!Main.dedServ && Projectile.timeLeft % 2 == 0)
            {
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), Main.rand.NextBool() ? Main.rand.NextBool() ? 57 : 58 : 15, -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f));
                trailDust.noGravity = true;
                trailDust.scale = Main.rand.NextFloat(0.8f, 1.5f);

                Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.2f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.timeLeft = 3;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Color.Cyan with { A = 0 } * 1.2f;
            float drawRotation = Projectile.rotation + MathHelper.PiOver2;
            Vector2 anchorPoint = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, anchorPoint, Projectile.scale, SpriteEffects.None);

            return false;
        }
    }
}
