using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class EmesisGore : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public bool setStats = true;
        public int rotDirection = 1;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 4;
            Projectile.timeLeft = 800;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (setStats)
            {
                rotDirection = (Main.rand.NextBool() ? -1 : 1);
                Projectile.rotation = Main.rand.NextFloat(-20, 20);
                setStats = false;
            }
            Projectile.rotation += 0.01f * rotDirection * Utils.GetLerpValue(0, 800, Projectile.timeLeft);
            Projectile.velocity *= 0.9975f;
            if (Projectile.ai[0] < 20 && Projectile.timeLeft > 695 && Projectile.timeLeft < 785)
                Projectile.velocity = Projectile.velocity.RotatedBy(0.0017f * Projectile.ai[0] * Projectile.ai[2]);
            Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 70, 0, 0, 255, true));
            if (targetDist < 1400f && Projectile.timeLeft > 70 && Projectile.timeLeft < 790)
            {
                if (Projectile.timeLeft % 5 == 0)
                {
                    //Particle spark = new LineParticle(Projectile.Center - Projectile.velocity + Main.rand.NextVector2Circular(20, 20), -Projectile.velocity * Main.rand.NextFloat(0.2f, 1.8f), false, Main.rand.Next(9, 20 + 1), Main.rand.NextFloat(0.8f, 1.2f), Color.Chartreuse * Main.rand.NextFloat(0.15f, 0.5f));
                    //GeneralParticleHandler.SpawnParticle(spark);

                    Particle spark = new GlowSparkParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2, -1), -Projectile.velocity * 0.3f, false, 6, 0.07f, Color.Lerp(Color.Green, Color.Chartreuse, 0.8f) * 0.5f, new Vector2(1, 0.3f), true, false, 1);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), Main.rand.NextBool(4) ? 215 : (int)CalamityDusts.SulphurousSeaAcid, -Projectile.velocity.RotatedByRandom(0.1) * Main.rand.NextFloat(0.1f, 0.3f), 0, default, Main.rand.NextFloat(0.5f, 1.2f));
                    dust.noGravity = true;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 420);
            for (int k = 0; k < 7; k++)
            {
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(5) ? 28 : 215, new Vector2(11, 11).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.8f));
                dust2.scale = Main.rand.NextFloat(0.75f, 1.25f);
                dust2.noGravity = false;
                Particle spark = new LineParticle(Projectile.Center, new Vector2(11, 11).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), true, Main.rand.Next(23, 35 + 1), Main.rand.NextFloat(0.8f, 1.2f), Color.Chartreuse * Main.rand.NextFloat(0.15f, 0.5f));
                GeneralParticleHandler.SpawnParticle(spark);
            }
            SoundStyle splode = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");
            SoundEngine.PlaySound(splode with { Volume = 0.6f }, Projectile.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 420);
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
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 20, targetHitbox);
    }
}
