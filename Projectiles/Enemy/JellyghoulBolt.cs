using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class JellyghoulBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public float Progress => 1 - Projectile.timeLeft / (float)300;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            Main.projFrames[Type] = 20;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1f * Projectile.Opacity, 0.1f * Projectile.Opacity, 0.1f);
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 4 % Main.projFrames[Type];

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.ai[0]++;
            if (Projectile.ai[0] % 5 == 0)
            {
                Color pulseColor = Main.rand.NextBool() ? (Main.rand.NextBool() ? Color.Red : Color.DarkRed) : (Main.rand.NextBool() ? Color.OrangeRed : Color.IndianRed);
                Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, pulseColor, new Vector2(0.5f, 1f), Projectile.velocity.ToRotation(), 0.1f, 0.2f + 0.2f * (1 - Progress), 10);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonTorch, Scale: 1.5f);
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 60);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                int idx = Dust.NewDust(Projectile.position, 8, 8, (int)CalamityDusts.Brimstone, 0, 0, 0, default, 0.75f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
                idx = Dust.NewDust(Projectile.position, 8, 8, (int)CalamityDusts.Brimstone, 0, 0, 0, default, 0.75f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise"));
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(Color.Red);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(Color.OrangeRed);

            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(SlashWidthFunction, SlashColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), 30);

            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int projFrame = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int y6 = projFrame * Projectile.frame;
            return false;
        }
        public float SlashWidthFunction(float _, Vector2 vertexPos) => 16 * Utils.GetLerpValue(0f, 0.1f, _, true);

        public Color SlashColorFunction(float _, Vector2 vertexPos) => Color.IndianRed * Projectile.Opacity;
    }
}
