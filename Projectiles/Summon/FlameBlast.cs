using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class FlameBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public float count = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 240;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            NPC potentialTarget = Projectile.Center.MinionHoming(900f, Main.player[Projectile.owner]);
            float velPower = Utils.GetLerpValue(2, 5, Projectile.velocity.Length(), true);
            if (count == 0f)
            {
                
                count += 1f;
            }
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 4f && Projectile.localAI[0] % 2 == 0f)
            {
                Particle beam = new CustomSpark(Projectile.Center, Projectile.velocity * 0.1f, "CalamityMod/Particles/SmallBloom", false, 8, 0.085f, Color.Lerp(Color.OrangeRed, Color.Goldenrod, Utils.GetLerpValue(270, 230, Projectile.timeLeft)), new Vector2(1f, 1 + 0.8f * velPower), true, false, 0, false, false, 0.5f * velPower);
                GeneralParticleHandler.SpawnParticle(beam);
            }
            
            if (potentialTarget != null && (Projectile.localAI[0] % 100 <= 55 || Projectile.localAI[0] < 20))
            {
                Projectile.timeLeft++; // Extend liftime so it doesnt fizzle out so fast while tracking something
                Projectile.velocity = (Projectile.velocity * 20f + Projectile.SafeDirectionTo(potentialTarget.Center) * 8f) / 21f;
            }
            else
                Projectile.velocity *= Main.rand.NextFloat(0.96f, 0.97f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);

        public override void OnKill(int timeLeft)
        {
            for (int j = 0; j < 9; j++)
            {
                Dust c = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>());
                c.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(7, 15);
                c.scale = Main.rand.NextFloat(0.9f, 1.1f);
                c.noGravity = true;
                c.color = Main.rand.NextBool() ? Color.Orange : Color.Goldenrod;
                c.noLightEmittance = true;
            }
        }
    }
}
