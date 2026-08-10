using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class BrimstoneBarrage : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public int time = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 690;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            int target = Player.FindClosest(Projectile.Center, 1, 1);

            float targetDist;
            if (target != -1 && !Main.player[target].dead && Main.player[target].active && Main.player[target] != null)
                targetDist = Vector2.Distance(Main.player[target].Center, Projectile.Center);
            else
                targetDist = 1000;

            if (Projectile.velocity.Length() < Projectile.ai[2])
            {
                Projectile.velocity *= 1.01f;
                if (Projectile.velocity.Length() > Projectile.ai[2])
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= Projectile.ai[2];
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            if (Projectile.timeLeft < 60)
                Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f);

            if (Projectile.ai[0] == 2f)
            {
                if (Projectile.timeLeft > 570)
                {
                    int player = Player.FindClosest(Projectile.Center, 1, 1);
                    Vector2 vector = Main.player[player].Center - Projectile.Center;
                    float scaleFactor = Projectile.velocity.Length();
                    vector.Normalize();
                    vector *= scaleFactor;
                    Projectile.velocity = (Projectile.velocity * 15f + vector) / 16f;
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= scaleFactor;
                }
            }

            // This only runs for SCal projectiles.
            if (Projectile.ai[1] == 2f || (Projectile.ai[1] == 4f && time > 10))
            {
                if (targetDist < 1400f)
                {
                    SparkParticle orb = new SparkParticle(Projectile.Center - Projectile.velocity * 0.5f, -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.6f), false, (int)MathHelper.Clamp(9 * Utils.GetLerpValue(630, 690, Projectile.timeLeft), 2, 9), 1.1f, (Main.rand.NextBool() ? Color.Red : Color.Lerp(Color.Red, Color.Magenta, 0.5f)) * Projectile.Opacity * 0.85f);
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
            // This only runs for SCal seekers and Sepulcher projectiles.
            if (Projectile.ai[1] == 3f)
            {
                if (Projectile.timeLeft > 600)
                    Projectile.velocity *= 1.015f;

                Projectile.scale = 0.85f;

                Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.TheDestroyer);
                trailDust.noGravity = true;
                trailDust.velocity = (-Projectile.velocity * 0.5f) * Main.rand.NextFloat(0.1f, 0.9f);
                trailDust.scale = Main.rand.NextFloat(0.2f, 0.6f);
            }

            Lighting.AddLight(Projectile.Center, 0.75f * Projectile.Opacity, 0f, 0f);
            time++;
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity == 1f;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.Opacity != 1f)
                return;

            if (Projectile.ai[0] == 0f || Main.zenithWorld)
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180);
            else
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            lightColor.R = (byte)(255 * Projectile.Opacity);

            if (CalamityGlobalNPC.SCal != -1 && NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()) == true)
            {
                if (Main.npc[CalamityGlobalNPC.SCal].active)
                {
                    if (Main.npc[CalamityGlobalNPC.SCal].ModNPC<SupremeCalamitas>().permafrost)
                    {
                        lightColor.G = (byte)(255 * Projectile.Opacity);
                        lightColor.B = (byte)(255 * Projectile.Opacity);
                        lightColor.R = 0;
                    }
                }
            }

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 10 * Projectile.scale, targetHitbox);
    }
}
