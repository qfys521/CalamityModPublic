using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    // This is p much just the Sulphuric Acid Cannon rocket but without the sticky behaviour
    public class BabyCannonballProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override string Texture => "CalamityMod/NPCs/Abyss/BabyCannonballJellyfish";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = 150 * Projectile.MaxUpdates;;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 11;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            CalamityUtils.HomeInOnNPC(Projectile, true, 720f, 16f, Projectile.MaxUpdates * 20f);
            // Trailing effects
            if (!Main.dedServ && Projectile.FinalExtraUpdate() && Projectile.velocity.Length() > 3f)
            {
                Color color = new Color(136, 211, 113, 127);
                Color fadeColor = new Color(165, 165, 86);
                Vector2 gasSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 gasVelocity = Projectile.velocity * 1.2f + Projectile.velocity.RotatedBy(0.75f) * 0.3f;
                gasVelocity *= Main.rand.NextFloat(0.24f, 0.6f);

                Particle gas = new MediumMistParticle(gasSpawnPosition, gasVelocity, color, fadeColor, Main.rand.NextFloat(0.5f, 1f), 205 - Main.rand.Next(50), 0.02f);
                GeneralParticleHandler.SpawnParticle(gas);
                for (int i = 0; i < 2; i++)
                {
                    Particle spark = new GlowSparkParticle(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10, -Projectile.velocity, false, 8, 0.13f * (i == 0 ? 0.2f : 0.5f), Color.Lerp(Color.SeaGreen, Color.PaleGreen, 0.25f) * 0.5f, new Vector2(0.3f, 1f), false, false, 0.8f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int i = 0; i < 2; i++)
                {
                    Color bubbleColor = Main.rand.NextBool() ? Color.SeaGreen : Color.YellowGreen;
                    Vector2 bubbleSpawnPos = Projectile.Center + Main.rand.NextVector2Circular(50, 50);
                    Vector2 bubbleVelocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f);
                    Particle bubble = new DirectionalPulseRing(bubbleSpawnPos, bubbleVelocity, bubbleColor, new Vector2(0.8f, 1), 0, 0.1f, 0f, 75);
                    GeneralParticleHandler.SpawnParticle(bubble);
                }
            }
            Projectile.Opacity = 1f;

            if (Projectile.frameCounter++ > 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);
            if (Projectile.owner == Main.myPlayer)
            {
                int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SulphuricAcidCannonExplosion>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                Main.projectile[p].DamageType = DamageClass.Default;
            }
            // Circular spread of clouds and bubbles for impact
            for (int i = 0; i < 35; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextVector2Circular(32f, 32f);
                Color smokeColor = Main.rand.NextBool() ? Color.SeaGreen : Color.PaleGreen;
                Particle smoke = new MediumMistParticle(Projectile.Center, smokeVel, smokeColor, Color.Black, Main.rand.NextFloat(1.4f, 4f), (200 - Main.rand.Next(60)), 0.08f);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 bubbleVel = Main.rand.NextVector2Circular(26f, 26f);
                Color bubbleColor = Main.rand.NextBool() ? Color.OliveDrab : Color.SeaGreen;
                DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center, bubbleVel, bubbleColor, new Vector2(0.8f, 1), 0, 0.21f, 0f, 50);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
            //Explosion effect
            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.SeaGreen, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.2f, 20, true, 1f);
            GeneralParticleHandler.SpawnParticle(blastRing);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            return null;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D Texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = Texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(Texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
