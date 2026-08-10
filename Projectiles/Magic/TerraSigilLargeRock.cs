using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class TerraSigilLargeRock : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 255;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Time++;
            Projectile.rotation += 0.05f;

            if (Projectile.timeLeft > 247)
                Projectile.scale *= 1.05f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // Normal sprite
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            // All-white variant
            Asset<Texture2D> ghostTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/TerraSigilLargeRockGhost");

            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame), drawColor, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            // Only apply the glow effect during the first 8 frames
            if (Time < 8)
            {
                // Fade from fully white to fully transparent
                float fadeFactor = Utils.GetLerpValue(0, 8, Time, true);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

                Main.EntitySpriteDraw(ghostTexture.Value, Projectile.Center - Main.screenPosition, texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame), Color.White * (1 - fadeFactor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 2; i++)
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 0.8f, Pitch = -0.4f + i * 0.25f, MaxInstances = 2 }, Projectile.Center);

            if (Projectile.owner == Main.myPlayer && Projectile.numHits > 0)
            {
                // Spawns 5 medium rock projectiles outward and evenly spaced
                int numRocks = 5;
                float spreadAngle = MathHelper.TwoPi / numRocks;

                for (int i = 0; i < numRocks; i++)
                {
                    // Calc angle for each rock
                    float rotation = spreadAngle * i;
                    Vector2 launchVelocity = new Vector2(1f, 0f).RotatedBy(rotation) * 20f;

                    // Spawn them.
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, launchVelocity, ModContent.ProjectileType<TerraSigilMediumRock>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }


            // Dust and smoke particles
            for (int i = 0; i < 28; i++)
            {
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? ModContent.DustType<TerraSigilDust>() : 262, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.8f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.85f, 1.25f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.3f;
                    dust.velocity *= 2.2f;
                    dust.alpha = 100;
                }
                else
                {
                    Color clr = Color.Lerp((Main.rand.NextBool() ? Color.Peru : Color.PeachPuff), Color.Black, Main.rand.NextFloat(0.25f, 0.45f));
                    Particle sand = new CustomSpark(Projectile.Center, (Vector2.One * Main.rand.NextFloat(3, 8)).RotatedByRandom(MathHelper.TwoPi), "CalamityMod/Particles/SmallSmoke", true, Main.rand.Next(15, 30 + 1), Projectile.scale * Main.rand.NextFloat(0.05f, 0.1f) * 4, clr, new Vector2(1, Main.rand.NextFloat(0.2f, 1f)), false, extraRotation: Main.rand.NextFloat(-2, 2), spin: Main.rand.NextFloat(-0.5f, 0.5f), affectedByLight: true);
                    GeneralParticleHandler.SpawnParticle(sand);
                }
            }
        }
    }
}
