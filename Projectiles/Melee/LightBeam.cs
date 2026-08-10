using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class LightBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public Player Owner => Main.player[Projectile.owner];

        private const float MaxVelocity = DarklightGreatsword.ShootSpeed * 12f;

        private const int FadeOutTime = 30;

        private const int TimeLeft = 150 + FadeOutTime;

        private const int Alpha = 100;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // Actually only hits three times, set to infinite so the fade out works still
            Projectile.timeLeft = TimeLeft;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 24;
            Projectile.alpha = Alpha;
        }

        public override void AI()
        {
            float alphaLightScale = Projectile.alpha / (float)Alpha;
            Lighting.AddLight(Projectile.Center, 1.2f * alphaLightScale, 0f, 0.4f * alphaLightScale);

            if (Projectile.timeLeft > FadeOutTime)
            {
                if (Projectile.velocity.Length() < MaxVelocity)
                    Projectile.velocity *= 1.055f;
            }
            else
                Projectile.velocity *= 0.925f;

            if (Projectile.velocity.X < 0f)
                Projectile.spriteDirection = -1;

            Projectile.rotation += Projectile.direction * 0.05f;
            if (Projectile.timeLeft > FadeOutTime)
                Projectile.rotation += Projectile.direction * 0.5f;
            else
                Projectile.rotation += Projectile.direction * 0.5f * (Projectile.timeLeft / (float)FadeOutTime);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.timeLeft < FadeOutTime)
            {
                byte b2 = (byte)(Projectile.timeLeft * 3);
                byte a2 = (byte)(Alpha * ((float)b2 / byte.MaxValue));
                Projectile.alpha = a2;
                return new Color(b2, b2, b2, Projectile.alpha);
            }
            Projectile.alpha = Alpha;
            return new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, Projectile.alpha);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > FadeOutTime)
            {
                Projectile.velocity = Projectile.oldVelocity;
                Projectile.tileCollide = false;
                Projectile.timeLeft = FadeOutTime;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 120);
            if (Projectile.numHits >= 2) // Fades out after the third hit
                Projectile.timeLeft = FadeOutTime;

            int slashCreatorID = ModContent.ProjectileType<DarklightGreatswordSlashCreator>();
            int damage = (int)(Projectile.damage * DarklightGreatsword.SlashProjectileDamageMultiplier);
            float knockback = Projectile.knockBack * DarklightGreatsword.SlashProjectileDamageMultiplier;
            if (Owner.ownedProjectileCounts[slashCreatorID] < DarklightGreatsword.SlashProjectileLimit)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, slashCreatorID, damage, knockback, Projectile.owner, target.whoAmI, Projectile.rotation, 1f);
                Owner.ownedProjectileCounts[slashCreatorID]++;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Shadowflame>(), 120);

        public override bool? CanDamage() => Projectile.timeLeft < FadeOutTime ? false : null;
    }
}
