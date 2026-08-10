using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GalacticaComet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public int time = 0;
        public int cometType = 0;
        public Color useColor = Color.White;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 102;
            Projectile.height = 102;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (time == 0)
            {
                Projectile.scale = Main.rand.NextFloat(0.6f, 0.8f);
                cometType = Main.rand.Next(1, 3 + 1);
                useColor = cometType switch
                {
                    1 => Color.Cyan,
                    2 => Color.Gold,
                    _ => Color.HotPink,
                };
            }

            if (targetDist < 1400)
            {
                Particle spark = new GlowSparkParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2, -1), -Projectile.velocity * 0.3f, false, 7, 0.1f, useColor * 0.45f, new Vector2(1, 0.3f), true, false, 1f);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            time++;
            Projectile.velocity *= 1.01f;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.9f); // 10% damage nerf for every enemy hit
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits == 0)
            {
                for (int i = 0; i <= 12; i++)
                {
                    if (i < 8)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Vector2.One * 9).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(1.3f, 1.8f));
                        dust.noGravity = true;
                        dust.color = useColor;
                    }
                    else
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, (Vector2.One * 9).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                        dust.noGravity = false;
                        dust.color = Color.Lerp(useColor, Color.White, 0.5f);
                    }
                }
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact with { Volume = 0.7f, PitchVariance = 0.3f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 1f, PitchVariance = 0.3f }, Projectile.Center);
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (cometType == 1)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GalacticaComet").Value;
            if (cometType == 2)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GalacticaComet2").Value;
            if (cometType == 3)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GalacticaComet3").Value;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White, 2, tex);
            return false;
        }
    }
}
