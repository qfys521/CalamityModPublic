using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class VeriumBoltProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override string Texture => "CalamityMod/Items/Ammo/VeriumBolt";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, 2, Color.Plum);
            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(4) ? 223 : 252, new Vector2(Projectile.velocity.X * Main.rand.NextFloat(-0.1f, 0.1f), Projectile.velocity.Y * Main.rand.NextFloat(-0.1f, 0.1f)));
            dust.noGravity = true;
            if (dust.type == 223)
                dust.scale = Main.rand.NextFloat(0.4f, 0.66f);
            else
                dust.scale = Main.rand.NextFloat(0.65f, 0.9f);
        }
        // This projectile is always fullbright.
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.LightSkyBlue;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Handles giving the NPC the doom effect
            CalamityGlobalNPC modNPC = target.Calamity();
            if (!modNPC.veriumDoomMarked)
            {
                modNPC.veriumDoomMarked = true;
                modNPC.veriumDoomTimer = CalamityGlobalNPC.veriumDoomTime;
            }
            modNPC.veriumDoomStacks++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 4; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworkFountain_Pink, Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.1f, 0.9f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 0.7f);
            }
        }
    }
}
