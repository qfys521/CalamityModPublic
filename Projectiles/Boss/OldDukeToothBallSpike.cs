using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.NPCs.OldDuke;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class OldDukeToothBallSpike : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            Lighting.AddLight((int)((Projectile.position.X + (Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (Projectile.height / 2)) / 16f), 0.45f, 0.35f, 0f);

            float finalVelocity = new Vector2(Projectile.ai[0], Projectile.ai[1]).Length();
            if (Projectile.velocity.Length() < finalVelocity)
                Projectile.velocity *= 1.025f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (int)CalamityDusts.SulphurousSeaAcid, 0, 0, 0, default, 0.6f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // There are two of each draw call because it makes the additive glow a bit brighter
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - (Projectile.velocity * i) - Main.screenPosition, tex.Frame(), OldDuke.GlowColor, Projectile.rotation, tex.Frame().Center(), MathHelper.Lerp(1.5f, 0.2f, (float)i / 5f), SpriteEffects.None);
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - (Projectile.velocity * i) - Main.screenPosition, tex.Frame(), OldDuke.GlowColor, Projectile.rotation, tex.Frame().Center(), MathHelper.Lerp(1.5f, 0.2f, (float)i / 5f), SpriteEffects.None);
            }
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, tex.Frame(), OldDuke.GlowColor, Projectile.rotation, tex.Frame().Center(), 1.5f, SpriteEffects.None);
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, tex.Frame(), OldDuke.GlowColor, Projectile.rotation, tex.Frame().Center(), 1.5f, SpriteEffects.None);

            Projectile.DrawBackglow(OldDuke.GlowColor, 4f);

            return base.PreDraw(player, ref lightColor);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Irradiated>(), 360);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                int idx = Dust.NewDust(Projectile.position, 8, 8, (int)CalamityDusts.SulphurousSeaAcid, 0, 0, 0, default, 0.75f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
                idx = Dust.NewDust(Projectile.position, 8, 8, (int)CalamityDusts.SulphurousSeaAcid, 0, 0, 0, default, 0.75f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
            }
        }
    }
}
