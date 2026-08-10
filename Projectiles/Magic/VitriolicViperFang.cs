using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class VitriolicViperFang : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.96f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.alpha = (int)(255 * Utils.GetLerpValue(70, 0, Projectile.timeLeft, true));
            if (Main.rand.NextBool(3) && Projectile.timeLeft <= 70)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), Main.rand.NextBool(6) ? 215 : (int)CalamityDusts.SulphurousSeaAcid);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f) * Utils.GetLerpValue(255, 0, Projectile.alpha);
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.7f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            for (int i = 0; i < 3; i++)
            {
                Color auraColor = Color.Lerp(Color.Chartreuse, Color.Lime, Utils.GetLerpValue(0, 3, i)) * 0.5f;
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 27f).ToRotationVector2();
                rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 15f) * 0.5f + 0.5f);
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + rotationalDrawOffset, null, auraColor with { A = 0 } * Utils.GetLerpValue(255, 0, Projectile.alpha), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor * 0.5f, 2);
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 35, targetHitbox);
    }
}
