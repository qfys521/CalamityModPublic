using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CorpusAvertorStealth : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/CorpusAvertor";

        private ref float Timer => ref Projectile.ai[0];
        private ref float Slash => ref Projectile.ai[1]; // If set to 1, this is the slash projectile
        private Vector2 startPos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 4;
            Projectile.timeLeft = 300;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (startPos == Vector2.Zero)
                startPos = Projectile.Center;

            if (Timer < 120f)
                Timer += 1f;

            Projectile.rotation = Projectile.velocity.ToRotation() + (MathHelper.Pi / 3f);
            Projectile.velocity *= 1.01f;

            if (Slash == 1f)
            {
                if (Timer > 4f)
                {
                    GlowSparkParticle glow = new(Projectile.Center, Vector2.Normalize(Projectile.velocity), false, 10, 0.055f, Color.DarkRed, new Vector2(0.66f, 1.5f), true, false);
                    GeneralParticleHandler.SpawnParticle(glow);
                }
            }
            else
            {
                int scale = (int)((Timer - 60f) * 4.25f);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, new Color(scale, 0, 0, 50), 2f);
                Main.dust[dust].velocity *= 0f;
                Main.dust[dust].noGravity = true;
            }

            if (Timer == 60f && Slash == 0f)
            {
                SoundEngine.PlaySound(CommonCalamitySounds.MeatySlashSound with { Volume = 0.4f, PitchVariance = 0.06f }, startPos);
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), startPos, Projectile.velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 1f);
                }
                Projectile.tileCollide = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, Projectile.alpha);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Slash == 1f)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
            if (target.IsAnEnemy(false) && Projectile.numHits == 0 && Slash == 0f)
            {
                Player player = Main.player[Projectile.owner];
                player.SpawnLifeStealProjectile(target, Projectile, ProjectileID.VampireHeal, (int)Math.Round(hit.Damage * 0.025));
                if (Main.LocalPlayer.team == player.team && player.team != 0)
                    Main.LocalPlayer.AddBuff(ModContent.BuffType<AvertorBonus>(), CalamityUtils.SecondsToFrames(20f));
            }

            if (Projectile.numHits > 0 && !(target.life <= 0 && target.realLife == -1))
                Projectile.damage = (int)(Projectile.damage * 0.9f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
    }
}
