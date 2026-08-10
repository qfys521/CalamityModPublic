using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class ToxicantTwisterProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/ToxicantTwister";

        public float circleRange = -150;
        public float circleSpeed = 1;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];

            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.Calamity().stealthStrike)
            {
                Projectile.MaxUpdates = 3;
                if (Projectile.ai[1] == 0)
                {
                    Projectile.timeLeft = 600;
                    circleRange *= 5;
                    circleSpeed *= 3;
                }
                if (Projectile.timeLeft % 50 == 0)
                {
                    Projectile dustProjectile = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.1f) * -0.6f, ModContent.ProjectileType<ToxicantTwisterDust>(), (int)(Projectile.damage * 0.4f), 0f, Projectile.owner, 0, 0, Projectile.ai[2]);
                    dustProjectile.timeLeft = 240;
                }
            }
            else if (Projectile.ai[1] == 0)
                Projectile.timeLeft = (int)(Projectile.timeLeft * Main.rand.NextFloat(1.05f, 1.2f));

            if (Main.rand.NextBool(4) && Projectile.ai[1] > 7)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(13, 13), Main.rand.NextBool(3) ? 215 : (int)CalamityDusts.SulphurousSeaAcid);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f) * Utils.GetLerpValue(255, 0, Projectile.alpha);
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.7f);
            }

            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (Projectile.ai[1] % 2 == 0 && Projectile.ai[1] > 7 && targetDist < 1400)
            {
                Particle spark = new GlowSparkParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2, -1), -Projectile.velocity * 0.3f, false, 6, 0.07f, Color.Lerp(Color.Green, Color.Chartreuse, 0.8f) * 0.65f, new Vector2(1, 0.3f), true, false, 1);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // Boomerang rotation
            Projectile.rotation += 0.5f / circleSpeed;

            if (Projectile.ai[1] > 20f)
            {
                Vector2 position = (Owner.ClampedMouseWorld() + ((new Vector2(0, circleRange).RotatedBy(Projectile.rotation * 0.2f)).RotatedBy(MathHelper.ToRadians(90f) * Projectile.ai[2])));
                Vector2 moveToMouse = (position - Projectile.Center).SafeNormalize(Vector2.UnitX);
                if (Projectile.velocity.Length() < 18)
                    Projectile.velocity += moveToMouse * circleSpeed;
                else
                    Projectile.velocity *= 0.9f;
            }
            else
            {
                Projectile.velocity *= 0.985f;
            }
            Projectile.ai[1]++;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 215 : (int)CalamityDusts.SulphurousSeaAcid, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(1.3f, 1.8f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(4, 8);
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center + dustVel, DustID.FireworksRGB, dustVel * 0.7f);
                dust2.scale = Main.rand.NextFloat(0.8f, 0.9f);
                dust2.noGravity = false;
                dust2.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Chartreuse : Color.Green, 0.7f);
            }
            Particle pulse = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Chartreuse, "CalamityMod/Particles/HighResFoggyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.07f, 8);
            GeneralParticleHandler.SpawnParticle(pulse);
            SoundStyle fire = new("CalamityMod/Sounds/NPCHit/RavagerRockPillarHit", 3);
            SoundEngine.PlaySound(fire with { Volume = 0.4f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f), MaxInstances = 4 }, Projectile.Center);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            for (int i = 0; i < 3; i++)
            {
                Color auraColor = Color.Lerp(Color.Chartreuse, Color.Lime, Utils.GetLerpValue(0, 3, i)) * 0.7f;
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 27f).ToRotationVector2();
                rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 15f) * 0.5f + 1f);
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + rotationalDrawOffset, null, auraColor with { A = 0 } * Utils.GetLerpValue(255, 0, Projectile.alpha), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor * 0.3f, 2);
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.Calamity().unbreakableDR)
                modifiers.SourceDamage *= MathHelper.Clamp((float)Math.Pow(0.9f, Projectile.numHits - 1), 0.1f, 1f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 55, targetHitbox);
    }
}
