using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using CalamityMod.Dusts;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Summon
{
    public class AmphibiansGuitarProjectile : ModProjectile, ILocalizedModType
    {
        public override string LocalizationCategory => "Projectiles.Summon";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public bool IsHatNote => Projectile.ai[0] != 0f;
        public Color noteColor = Color.White;
        public int time = 0;
        public NPC targeted;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 44;
            Projectile.timeLeft = 150;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            if (time == 0)
            {
                if (Projectile.ai[2] == 5)
                {
                    Projectile.penetrate = 3;
                }
            }
            Projectile.scale = 1.6f * Utils.GetLerpValue(-5, 20, time, true);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (time % 10 == 0)
            {
                if (Projectile.ai[1] < 4)
                    Projectile.ai[1]++;
                else
                    Projectile.ai[1] = 0;
            }
            Color chooseColor = Projectile.ai[1] switch
            {
                0 => Color.Red,
                1 => Color.Cyan,
                2 => Color.Goldenrod,
                3 => Color.Magenta,
                _ => Color.Lime,
            };
            if (time == 0)
                noteColor = chooseColor;
            else
                noteColor = Color.Lerp(noteColor, chooseColor, 0.07f);

            Lighting.AddLight(Projectile.Center, noteColor.ToVector3() * 0.5f);
            if (time % 3 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40, 40), ModContent.DustType<LightDust>(), -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.5f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.85f, 1.35f);
                dust.color = noteColor;
                dust.noLightEmittance = true;
            }

            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= IsHatNote ? 1.5f : 1f;
        public override void OnKill(int timeLeft)
        {
            if (IsHatNote)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.8f, Pitch = -0.5f }, Projectile.Center);
            }
            for (int i = 0; i <= (IsHatNote ? 8 : 4); i++)
            {
                Particle spark = new SparkParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(IsHatNote ? 100 : 0.5f) * Main.rand.NextFloat(0.4f, 1.3f), false, 15, 1.2f, noteColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/Evernote");
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/EvernoteWall");
            Asset<Texture2D> glow = ModContent.Request<Texture2D>("CalamityMod/Particles/Light");

            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], noteColor with { A = 0 } * 0.4f, 1, tex.Value, true, true);

            Main.EntitySpriteDraw(tex.Value, generalDrawPos, null, noteColor with { A = 0 }, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.9f * Utils.GetLerpValue(-5, 20, time, true), 1) * Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(tex.Value, generalDrawPos, null, Color.Lerp(Color.White, noteColor, 0.15f) with { A = 0 }, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.9f * Utils.GetLerpValue(-5, 20, time, true), 1) * Projectile.scale * 0.88f, SpriteEffects.None);

            if (IsHatNote)
            {
                for (int i = 0; i < 3; i++)
                    Main.EntitySpriteDraw(tex2.Value, generalDrawPos, null, (noteColor * 0.5f) with { A = 0 }, Main.GlobalTimeWrappedHourly * 7.3f + i * 2, tex2.Size() * 0.5f, Projectile.scale * (0.6f + i * 0.6f), SpriteEffects.None);
            }
            return false;
        }
    }
}
