using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class FlameLickedBarrage : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/Boss/BrimstoneBarrage";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 1f && Projectile.timeLeft > 200)
                Projectile.timeLeft = 200;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            if (Projectile.timeLeft < 60)
                Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f);


            Lighting.AddLight(Projectile.Center, 0.75f * Projectile.Opacity, 0f, 0f);
        }

        public override bool? CanDamage() => Projectile.Opacity == 1f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Doze - I gave all parry accessories long debuff infliction times due to the lack of weapons that inflict debuffs for a decent time, and the scarcity of using the parry
            //Most common vanilla debuffs have a way to inflict them for 15, 20, or even 30 seconds
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), CalamityUtils.SecondsToFrames(15));
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (target.creativeGodMode)
                return;
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 90);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor.R = (byte)(255 * Projectile.Opacity);

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
