using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.BrimstoneElemental;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SeethingDischargeBrimstoneBarrage : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/Boss/BrimstoneBarrage";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            // Play a sound upon spawning
            if (Projectile.ai[0] == 0f)
            {
                SoundEngine.PlaySound(BrimstoneElemental.DartSound, Projectile.Center);
                Projectile.ai[0] = 1f;
            }

            // Accelerate over time
            if ((Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y) < 16f) && Projectile.ai[1] != 1f)
            {
                Projectile.velocity *= 1.01f;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 4)
            {
                Projectile.frame = 0;
            }
            Lighting.AddLight(Projectile.Center, 0.75f, 0f, 0f);

            // Seething Discharge darts have weak homing capabilities
            if (Projectile.ai[1] == 1f && Projectile.timeLeft < 585)
            {
                float npcDistCompare = 480f;
                int index = -1;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (!n.CanBeChasedBy(Projectile, false))
                        continue;

                    float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                    if (currentNPCDist < npcDistCompare)
                    {
                        npcDistCompare = currentNPCDist;
                        index = n.whoAmI;
                    }
                }
                if (index != -1)
                {
                    float homingStrength = Main.rand.NextFloat(0.075f, 0.085f);
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Main.npc[index].Center).ToRotation(), homingStrength).ToRotationVector2() * 12f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(250, 50, 50, Projectile.alpha);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
