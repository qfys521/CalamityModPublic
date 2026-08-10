using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    [PierceResistException(onlyForSingleHitbox: true)]
    public class AtlantisSpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        private static int TotalSegments = 16;
        private float damageMultiplier = 1f;
        private int time = 0;
        private float fade = 1;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 52;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 8;
            Projectile.appliesImmunityTimeOnSingleHits = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (Projectile.ai[1] == 0f)
            {
                Projectile.alpha -= 100;
                if (Projectile.alpha <= 0)
                {
                    Projectile.alpha = 0;
                    Projectile.ai[1] = 1f;

                    // This projectile normally does not move by itself, so this will manually move it one time only
                    // This is only for the first segment and the on-kill segments
                    if (Projectile.ai[0] == 0f || Projectile.ai[0] > TotalSegments)
                    {
                        Projectile.ai[0]++;
                        Projectile.position += Projectile.velocity;
                    }

                    // Spawn the next segment
                    if (Main.myPlayer == Projectile.owner && Projectile.ai[0] < TotalSegments)
                    {
                        int nextSegment = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Projectile.velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0] + 1f, 0f, 5f);
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, nextSegment);
                    }
                }
            }
            else // Begin fading out
            {
                int AlphaPerFrame = 12;
                Projectile.alpha += AlphaPerFrame;

                if (Projectile.alpha >= 255)
                    Projectile.Kill();
            }
            if (Main.rand.NextBool(7))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity * 0.5f, DustID.FireworksRGB, Projectile.velocity.RotatedByRandom(0.8) * Main.rand.NextFloat(0.03f, 0.18f));
                dust.scale = Main.rand.NextFloat(0.3f, 0.5f);
                dust.color = Main.rand.NextBool() ? Color.CornflowerBlue : Color.LightBlue;
                dust.noGravity = true;
            }
            time++;

            fade = Utils.GetLerpValue(25, 12, time, true);
        }

        // This is essential for Vilethorn-type projectiles, as velocity is a stored parameter and isn't supposed to actually move the projectile
        public override bool ShouldUpdatePosition() => false;

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, Projectile.alpha);

        // Damage falloff is handled in ModifyHitNPC instead of OnHitNPC so that the split spears don't inherit the lower damage
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= damageMultiplier;
            damageMultiplier *= 0.8f;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[2] == 5)
            {
                for (int k = 0; k < 2; k++)
                {
                    Particle spark = new SparkParticle(Projectile.Center + Projectile.velocity * 0.5f, Projectile.velocity.RotatedByRandom(0.8) * Main.rand.NextFloat(0.1f, 0.6f), false, 23, Main.rand.NextFloat(0.7f, 1.1f), Color.LightBlue * 0.6f);
                    GeneralParticleHandler.SpawnParticle(spark);
                    Particle spark2 = new PointParticle(Projectile.Center + Projectile.velocity * 0.5f, Projectile.velocity.RotatedByRandom(0.8) * Main.rand.NextFloat(0.1f, 0.6f), false, 23, Main.rand.NextFloat(0.7f, 1.1f), Color.LightBlue * 0.6f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.3f, Pitch = (Projectile.ai[0] * -0.02f) - 0.1f }, Projectile.Center);
            }

            // Prevent recursion: the segments that are being spawned here will deliberately be set higher than total segments
            if (Projectile.ai[0] > TotalSegments || Main.myPlayer != Projectile.owner)
                return;

            // Spawn two ungrowing segments to either side on death
            int numProj = 2;
            float rotation = MathHelper.ToRadians(20);
            for (int i = 0; i < numProj; i++)
            {
                Vector2 perturbedSpeed = Projectile.velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProj - 1)));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, TotalSegments + 1f, 0, i);
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            
            if (time > 0)
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * fade, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
