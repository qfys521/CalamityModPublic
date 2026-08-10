using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class InkCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // Will get overwritten by PreDraw
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 100;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 15;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (Projectile.timeLeft < 50)
                Projectile.alpha += 5;
            if (Projectile.timeLeft < 75)
                Projectile.velocity *= 0.95f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.friendly)
            {
                target.AddBuff(BuffID.Confused, 60);
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Confused, 300);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            string texPath = "CalamityMod/Projectiles/Rogue/InkCloud" + Projectile.ai[0].ToString();
            Texture2D tex = ModContent.Request<Texture2D>(texPath).Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
