using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class BlazingStarHeal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Particles/Sparkle";

        public static Asset<Texture2D> Bloom;
        public override void Load() => Bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 25;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 200)
                Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.rotation += MathHelper.ToRadians(6f) * Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);
            Projectile.scale = Utils.GetLerpValue(-10f, 30f, Projectile.timeLeft, true); // Gets smaller but not completely invisible while dying out

            if (Projectile.timeLeft % 4 == 0) //only once per 4 frames
                Lighting.AddLight(Projectile.Center, 0f, 0.6f, 0f);
            if (Projectile.timeLeft > 190)
                Projectile.velocity *= 1.1f;
            else if (Projectile.timeLeft <= 190)
                Projectile.velocity *= 0.99f;
            if (Projectile.timeLeft <= 160)
                Projectile.velocity = Vector2.Zero;

            int index = Player.FindClosest(Projectile.position, Projectile.width, Projectile.height);
            Player player = Main.player[index];
            if (Projectile.timeLeft > 190 || player is null || Main.player[Projectile.owner].team != player.team)
                return;

            float playerDist = Vector2.Distance(player.Center, Projectile.Center);
            if (!player.immune && playerDist < 30f * Projectile.scale && Projectile.timeLeft <= 190)
            {
                int healAmt = Utils.Clamp((200 - Projectile.timeLeft) / 10, 1, 10); //min heal is 5, max heal is 10, achievable after 2 seconds
                player.HealPlayer(healAmt, HealTextType.Local);

                NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, index, healAmt);

                Projectile.Kill();
            }
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => (1f - completionRatio) * Projectile.scale * 16f;
        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float hue = 0.35f + 0.1f * completionRatio * CalamityUtils.Convert01To010((Main.GlobalTimeWrappedHourly * 0.25f) % 1f);
            Color trailColor = Main.hslToRgb(hue, 0.6f, 0.5f);
            return trailColor * Projectile.Opacity;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 25);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D sparkleTex = TextureAssets.Projectile[Type].Value;
            Texture2D bloomTex = Bloom.Value;
            float bloomScale = (sparkleTex.Height / (float)bloomTex.Height) * Projectile.scale;
            float sparkleScale = (0.5f + CalamityUtils.Convert01To010((Main.GlobalTimeWrappedHourly % 2f) / 2f) * 0.2f) * Projectile.scale;

            Color color = ColorFunction(0f, Vector2.Zero);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(bloomTex, drawPos, null, color * 0.5f, 0, bloomTex.Size() * 0.5f, 5f * bloomScale, SpriteEffects.None);
            Main.EntitySpriteDraw(sparkleTex, drawPos, null, Color.Lerp(color, Color.White, 0.7f), Projectile.rotation, sparkleTex.Size() * 0.5f, 2.2f * sparkleScale, SpriteEffects.None);
            Main.EntitySpriteDraw(sparkleTex, drawPos, null, color, Projectile.rotation + MathHelper.PiOver4, sparkleTex.Size() * 0.5f, 1.6f * sparkleScale, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
            SoundStyle fireHeal = new("CalamityMod/Sounds/Custom/PlantyMushMine", 3);
            SoundEngine.PlaySound(fireHeal with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);

            Particle pulse = new CustomPulse(Projectile.Center, Vector2.Zero, ColorFunction(0f, Vector2.Zero), "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.04f, 15);
            GeneralParticleHandler.SpawnParticle(pulse);
            Color smokeColor = Color.Lerp(ColorFunction(0f, Vector2.Zero), Color.DarkSlateGray, 0.5f);
            for (int i = 0; i < 7; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(7f), smokeColor, 30, Main.rand.NextFloat(0.4f, 1f), 0.5f, Main.rand.NextFloat(-0.03f, 0.03f), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(1.8f, 10f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.5f);
                dust.color = ColorFunction(0f, Vector2.Zero);
                dust.noLightEmittance = true;
            }
        }

        public override bool? CanDamage() => false;
    }
}
