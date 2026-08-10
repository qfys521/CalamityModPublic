using System.Collections.Generic;
using System.Linq;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class CosmicOrb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.extraUpdates = 0;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(0.075f, 0.5f, 0.15f));

            Projectile.velocity *= 0.985f;
            Projectile.rotation += Projectile.velocity.X * 0.2f;

            if (Projectile.velocity.X > 0f)
            {
                Projectile.rotation += 0.08f;
            }
            else
            {
                Projectile.rotation -= 0.08f;
            }

            Projectile.ai[1] += 1f;
            if (Projectile.ai[1] > 30f)
            {
                Projectile.alpha += 10;
                if (Projectile.alpha >= 255)
                {
                    Projectile.alpha = 255;
                    Projectile.Kill();
                    return;
                }
            }

            //if (Main.rand.NextBool(2))
            //CalamityUtils.MagnetSphereHitscan(Projectile, 500f, 6f, 8f, 5, ModContent.ProjectileType<CosmicBolt>());
            int chance = 24 * Projectile.MaxUpdates;
            if (Main.rand.NextBool(chance))
            {
                int MaxLaserCountPerShot = 5;
                int targetCount = 0;
                List<NPC> targets = Main.npc.Where(npc =>
                {
                    return npc.active && Projectile.Distance(npc.Center) < 500 && npc.CanBeChasedBy();
                }).ToList();
                foreach (var target in targets)
                {
                    if (targetCount >= MaxLaserCountPerShot)
                        break;
                    Projectile laser = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<DirectStrike>(),
                    (int)(Projectile.damage * 0.8f),
                    Projectile.knockBack,
                    Projectile.owner,
                    target.whoAmI);
                    laser.ArmorPenetration = 100;

                    Vector2 start = Projectile.Center;
                    Vector2 end = target.Center;
                    Color color = Main.rand.NextBool() ? Color.Magenta : Color.HotPink;

                    Vector2 lerpVel = Vector2.Lerp(start, end, 0.5f);
                    float scale = 0.015f;
                    Particle spark = new CustomSpark(lerpVel, Projectile.SafeDirectionTo(target.Center), "CalamityMod/Particles/BloomLineThick", false, 14, scale, color * 0.75f, new Vector2(1, (Utils.Distance(start, end) * 0.034f)), true, false, shrinkSpeed: 0.25f, glowOpacity: 0.65f);
                    GeneralParticleHandler.SpawnParticle(spark);
                    targetCount++;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item54, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                int dustScale = (int)(10f * Projectile.scale);
                int d = Dust.NewDust(Projectile.Center - Vector2.One * (float)dustScale, dustScale * 2, dustScale * 2, DustID.PinkTorch, 0f, 0f, 0, default, 1f);
                Dust dust = Main.dust[d];
                Vector2 offset = Vector2.Normalize(dust.position - Projectile.Center);
                dust.position = Projectile.Center + offset * (float)dustScale * Projectile.scale;
                if (i < 30)
                {
                    dust.velocity = offset * dust.velocity.Length();
                }
                else
                {
                    dust.velocity = offset * Main.rand.NextFloat(4.5f, 9f);
                }
                dust.color = Main.hslToRgb(0.95f, 0.41f + Main.rand.NextFloat() * 0.2f, 0.93f);
                dust.color = Color.Lerp(dust.color, Color.White, 0.3f);
                dust.noGravity = true;
                dust.scale = 0.7f;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Color drawColor = Color.HotPink;
            Projectile.DrawProjectileWithBackglow(drawColor with { A = 0 }, Color.White, 2.5f * Main.rand.NextFloat(0.8f, 1.3f));
            return false;
        }
    }
}
