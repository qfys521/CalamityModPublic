using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SepticSkewerBacteria : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public bool setStats = true;
        public int rotDirection = 1;
        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 45;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 80;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            if (setStats)
            {
                Projectile.timeLeft += Main.rand.Next(0, 25 + 1);
                rotDirection = (Main.rand.NextBool() ? -1 : 1);
                Projectile.rotation = Main.rand.NextFloat(-20, 20);
                Projectile.scale = Main.rand.NextFloat(0.55f, 0.8f);
                setStats = false;
            }
            if (time % 8 == 0 && time > 40)
            {
                Particle spark = new CustomSpark(Projectile.Center - Projectile.velocity + Main.rand.NextVector2Circular(20, 20), -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.4f), "CalamityMod/Projectiles/Boss/OldDukeGore", false, Main.rand.Next(9, 20 + 1), Main.rand.NextFloat(0.4f, 0.6f), (!ChildSafety.Disabled ? Color.LimeGreen : Color.Lerp(Color.White, Color.Chartreuse, 0.5f)) * Main.rand.NextFloat(0.55f, 0.7f) * Utils.GetLerpValue(255, 0, Projectile.alpha), new Vector2(1, 1), false, false, Main.rand.NextFloat(-1f, 1f));
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(13, 13), (int)CalamityDusts.SulphurousSeaAcid);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f) * Utils.GetLerpValue(255, 0, Projectile.alpha);
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.7f);
            }
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), !ChildSafety.Disabled ? (int)CalamityDusts.SulphurousSeaAcid : DustID.Blood, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.8f), 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                dust.noGravity = true;
                dust.alpha = (int)MathHelper.Clamp(Projectile.alpha, 0, 255);
            }
            Projectile.rotation += 0.035f * rotDirection * Utils.GetLerpValue(0, 100, Projectile.timeLeft, true);
            Projectile.velocity *= 0.965f;
            if (Projectile.timeLeft < 60)
                Projectile.alpha += 5;
            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 120);
        }

        public override void OnKill(int timeLeft)
        {
            
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/OldDukeGore").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation;
            Vector2 rotationPoint = texture.Size() * 0.5f;


            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
