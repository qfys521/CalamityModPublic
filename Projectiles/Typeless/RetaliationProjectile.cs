using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class RetaliationProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        public float radius = 25f;
        public int time = 0;
        public bool homing = false;
        public NPC targeted;
        public Color mainColor = Color.White;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // They only hit once, this is a visual thing
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration = 25;

            Projectile.scale = 0.08f; // Normally you NEVER put scale in here, but since this doesnt use normal sprite drawing it's fine
        }
        public override void AI()
        {
            float fadeOut = Utils.GetLerpValue(0, 50, Projectile.timeLeft, true);

            List<Color> eColors = new List<Color>()
            {
                Color.DarkRed,
                Color.Crimson,
            };
            float rate = (Main.GlobalTimeWrappedHourly * 20);
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            mainColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (time % 2 == 0) // The trail
            {
                Particle spark2 = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.05f, "CalamityMod/Particles/SmallBloom", false, 11, 0.11f, mainColor, new Vector2(Utils.Remap(Projectile.velocity.Length(), 0, 5, 1, 0.6f, true), 1f), true, false, 0, false, false, Utils.Remap(Projectile.velocity.Length(), 0, 5, 0, 0.9f, true));
                GeneralParticleHandler.SpawnParticle(spark2);
            }

            if (Main.rand.NextBool(14))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Projectile.velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.2f, 1f), 0, default, Main.rand.NextFloat(0.8f, 1.15f));
                dust.noGravity = true;
                dust.color = mainColor;
            }

            if ((time > 65 || homing) && Projectile.numHits == 0) // Homing before they hit a target
            {
                homing = true;
                if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active || targeted.life <= 0)
                {
                    targeted = Projectile.Center.ClosestNPCAt(2500);
                }

                if (targeted != null)
                {
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.18f, 25, 0.98f, accelerate: true);
                    Projectile.extraUpdates = 3;
                    Projectile.timeLeft++;
                }
            }
            else // The rotate effect on spawn and on death
            {
                if (Projectile.numHits > 0)
                {
                    Projectile.scale = fadeOut * 0.08f;
                    if (Projectile.timeLeft % 40 == 0)
                        Projectile.ai[1] *= -1;
                }
                Projectile.velocity = Projectile.velocity.RotatedBy((0.03f + (1 - fadeOut) * 0.05f) * Projectile.ai[1]) * (Projectile.numHits == 0 ? 0.98f : 1);
                Projectile.extraUpdates = 2;
            }
            if (targeted == null && homing && Projectile.numHits == 0 && Projectile.velocity.Length() < 7)
                Projectile.velocity *= 1.007f;

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.timeLeft = Main.rand.Next(50, 75 + 1);

            SoundStyle sound = new("CalamityMod/Sounds/NPCHit/PerfHiveHit", 3);
            SoundEngine.PlaySound(sound with { Volume = 0.3f, Pitch = Main.rand.NextFloat(-0.1f, 0.2f), MaxInstances = 15 }, Projectile.Center);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/BigHeart").Value;
            Vector2 squash = new Vector2(Utils.Remap(Projectile.velocity.Length(), 1, 7, 1, 0.5f), Utils.Remap(Projectile.velocity.Length(), 1, 7, 1, 3.2f));

            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8, null, mainColor with { A = 0 }, Projectile.rotation, tex.Size() * 0.5f, squash * Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => homing && Projectile.numHits == 0 ? CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox) : false;
        public override bool? CanCutTiles() => false;
    }
}
