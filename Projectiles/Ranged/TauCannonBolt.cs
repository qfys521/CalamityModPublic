using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Color turquoiseColor = Color.MediumTurquoise;
        public Color coralColor = Color.Coral;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 32;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Main.dedServ)
                return;
            if (Main.rand.NextBool(10))
            {
                Dust trailDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, Scale: Main.rand.NextFloat(0.5f, 0.8f));
                trailDust.noGravity = true;
                trailDust.color = Main.rand.NextBool(3) ? coralColor : turquoiseColor;
            }

            Particle orb = new CustomSpark(Projectile.Center, Projectile.velocity, "CalamityMod/Projectiles/StarProj",false, 2, 1f, Color.Lerp(turquoiseColor, Color.White, 0.7f), new Vector2(1f, 1f));
            GeneralParticleHandler.SpawnParticle(orb);

            if (Projectile.ai[2] == 5)
                CalamityUtils.HomeInOnNPC(Projectile, true, 500f, 15f, 50f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            int dustAmount = Main.rand.Next(5, 10);
            for (int i = 0; i < dustAmount; i++)
            {
                Dust boomDust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, (MathHelper.TwoPi / dustAmount * i).ToRotationVector2() * Main.rand.NextFloat(5f, 8f), Scale: Main.rand.NextFloat(0.6f, 1f));
                boomDust.noGravity = true;
                boomDust.color = Main.rand.NextBool(3) ? coralColor : turquoiseColor;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 60);
        }
        private float WidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * 32f * CalamityUtils.Convert01To010(completionRatio);

        private Color ColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lerp(turquoiseColor, Color.Transparent, completionRatio) * Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]));
            return false;
        }
    }
}
