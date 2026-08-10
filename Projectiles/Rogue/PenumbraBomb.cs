using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class PenumbraBomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Penumbra";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 250;
            Projectile.extraUpdates = 2;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            //Hitbox Fixing
            if (Projectile.direction == 1)
            {
                DrawOffsetX = -4;
                DrawOriginOffsetX = -5;
            }
            else
            {
                DrawOffsetX = -11;
                DrawOriginOffsetX = 5;
            }

            //Alpha
            if (Projectile.alpha > 10)
                Projectile.alpha -= 7;
            else
                Projectile.alpha = 10;
            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + (MathHelper.ToRadians(180) * Projectile.direction);

            //Dust
            float dfreq = Projectile.Calamity().stealthStrike ? 8f : 4f;
            if (Projectile.ai[0] == dfreq)
            {
                Vector2 dustspeed = Projectile.velocity * Main.rand.NextFloat(0.5f, 0.8f);
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, dustspeed.X, dustspeed.Y, 0, new Color(38, 30, 43), 1.4f);
                Main.dust[d].velocity = dustspeed;
                if (Projectile.Calamity().stealthStrike)
                {
                    Vector2 dustspeed2 = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, dustspeed2.X, dustspeed2.Y, 0, new Color(38, 30, 43), 1.3f);
                    Main.dust[d2].velocity = dustspeed2;
                }
                Projectile.ai[0] = 0f;
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 180);
            target.AddBuff(ModContent.BuffType<Nightwither>(), 180);
        }
        public override void OnKill(int timeLeft)
        {
            // Dark soul projectiles
            int projAmt = Projectile.Calamity().stealthStrike ? 8 : 6;
            for (int i = 0; i < projAmt; i++)
            {
                Vector2 SoulSpeed = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / projAmt) * 13f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, SoulSpeed, ModContent.ProjectileType<PenumbraSoul>(), (int)(Projectile.damage * 0.15f), 3f, Projectile.owner, 0f, 0f);
            }

            // Dust
            int maxDust = Projectile.Calamity().stealthStrike ? 100 : 70;
            for (int i = 0; i < maxDust; i++)
            {
                Vector2 dustspeed = new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f));
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, dustspeed.X, dustspeed.Y, 0, new Color(38, 30, 43), 1.6f);
            }

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.ExpandHitboxBy(110);
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.damage /= 2;
            Projectile.Damage();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
