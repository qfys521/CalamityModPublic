using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonPortal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override string Texture => "CalamityMod/ExtraTextures/GreyscaleVortex";
        public Color color1 = Color.Coral;
        public Color color2 = Color.MediumTurquoise;
        private float Scale01
        {
            get
            {
                if (Projectile.timeLeft > 360)
                    return Utils.GetLerpValue(420f, 360f, Projectile.timeLeft, true);

                if (Projectile.timeLeft >= 60 && Projectile.timeLeft <= 360)
                    return 1f;

                if (Projectile.timeLeft < 60)
                    return Utils.GetLerpValue(0f, 60f, Projectile.timeLeft, true);

                return 0f;
            }
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 100;
            Projectile.timeLeft = 420;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5 * Projectile.MaxUpdates;
            Projectile.extraUpdates = 2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
            => CalamityUtils.CircularHitboxCollision(Projectile.Center, 100f * Scale01, targetHitbox);

        public override void AI()
        {
            Projectile.rotation += MathHelper.ToRadians(1.5f) * Scale01;

            if (Projectile.timeLeft < 300)
            {
                Player Owner = Main.player[Projectile.owner];
                Vector2 moveTotarget = (Owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                if (Projectile.velocity.Length() < 15)
                    Projectile.velocity += moveTotarget * 0.05f;
                else
                    Projectile.velocity *= 0.98f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= 0.08f;

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                int randomBoltAmount = Main.rand.Next(9, 12+1);
                float starterAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < randomBoltAmount; i++)
                {
                    float angle = starterAngle + (MathHelper.TwoPi / randomBoltAmount * i);
                    Vector2 velocity = angle.ToRotationVector2() * 14f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<TauCannonBolt>(),
                        (int)(Projectile.damage * 0.65f),
                        Projectile.knockBack,
                        Projectile.owner, 0, 0, 5);
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 anchorPoint = texture.Size() * 0.5f;
            for (int i = 0; i < 13; i++)
                Main.EntitySpriteDraw(texture, drawPosition, null, Color.Lerp(color1, Color.White, i * 0.075f) with { A = 0 } * Scale01 * 0.45f, Projectile.rotation * 3 - i * 0.15f, anchorPoint, MathHelper.Clamp(Scale01 * 0.375f - i * 0.02f, 0, 5), SpriteEffects.None);

            return false;
        }
    }
}
