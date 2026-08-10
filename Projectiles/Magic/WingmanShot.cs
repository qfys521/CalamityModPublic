using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class WingmanShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int BounceHits = 0;
        public Color mainColor = Color.White;
        public int time = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 240;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (mainColor == Color.White)
            {
                if (Projectile.ai[1] == -1)
                    mainColor = Color.Turquoise;
                if (Projectile.ai[1] == 0)
                    mainColor = Color.Orchid;
                if (Projectile.ai[1] == 1)
                    mainColor = Color.MediumSlateBlue;
                if (Projectile.ai[1] == 2)
                {
                    mainColor = Color.MediumVioletRed;
                    Projectile.scale = 1.25f;
                    Projectile.extraUpdates = 5;
                    Projectile.penetrate = 3;
                }

            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            //Dust dust = Dust.NewDustPerfect(Projectile.Center, 107); // + Main.rand.NextVector2Circular(-3, 3)
            //dust.noGravity = true;
            //dust.scale = 0.5f;
            if (time < 180)
                Projectile.velocity *= 0.995f;
            if (time % 2 == 0 && Projectile.timeLeft > 15)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY) * 3.5f, Projectile.velocity * 0.01f, false, 5, 1f * Projectile.scale, mainColor * 0.4f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            time++;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time == 0)
                return false;
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation;
            Vector2 rotationPoint = texture.Size() * 0.5f;

            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.Lerp(Color.Cyan, Color.White, 0.7f) * 0.6f, 1);
            Main.EntitySpriteDraw(texture, drawPosition, null, mainColor with { A = 0 }, drawRotation, rotationPoint, new Vector2(0.5f, 1.4f) * 0.025f * Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, drawPosition, null, Color.White with { A = 0 }, drawRotation, rotationPoint, new Vector2(0.5f, 1.4f) * 0.02f * Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.ai[1] != 2)
            {
                if (Projectile.numHits > 0)
                    Projectile.damage = (int)(Projectile.damage * 0.9f);
                if (Projectile.damage < 1)
                    Projectile.damage = 1;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(4) ? 264 : 66, (Projectile.velocity.SafeNormalize(Vector2.UnitY) * 15f).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(1.2f, 1.6f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool() ? Color.Lerp(mainColor, Color.White, 0.5f) : mainColor;
                dust.noLightEmittance = true;
                dust.noLight = true;
            }
        }
    }
}
