using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class AuricLandMineExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private Player Owner => Main.player[Projectile.owner];

        public List<List<Vector2>> lightningTrails = new List<List<Vector2>>();
        public static int lightningCount = 15;
        public static int totalPoints = 10;

        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = true;
            Projectile.trap = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 20)
                Owner.SetScreenshake(200f);
            if (Projectile.timeLeft <= 20)
            {
                if (Projectile.ai[0] % 4 == 0)
                {
                    SoundStyle explode = new("CalamityMod/Sounds/Item/AuricBulletHit");
                    SoundEngine.PlaySound(explode with { Pitch = 0.4f }, Projectile.Center);
                    SoundStyle explode2 = new("CalamityMod/Sounds/Item/TheHiveNuke");
                    SoundEngine.PlaySound(explode2 with { Pitch = -0.4f }, Projectile.Center);
                    lightningTrails.Clear();
                    for (int i = 0; i < lightningCount; i++)
                    {
                        List<Vector2> points = new List<Vector2>();
                        for (int j = 0; j < totalPoints; j++)
                        {
                            float radians = MathHelper.TwoPi / lightningCount;
                            if (j == 0)
                            {
                                points.Add(Projectile.Center + Main.rand.NextVector2Circular(20, 20));
                            }
                            else
                            {
                                Vector2 newPoint = new Vector2();
                                Vector2 jtolookfor = j > 1 ? points[j - 2] : Projectile.Center;
                                float baseDist = j == totalPoints - 1 ? 20 : Main.rand.Next(60, 120) * (1 + (20 - Projectile.timeLeft) / 15);
                                newPoint = points[j - 1] + (jtolookfor.DirectionTo(points[j - 1]) * baseDist).RotatedByRandom(MathHelper.PiOver2);
                                points.Add(newPoint);
                            }
                        }
                        lightningTrails.Add(points);
                    }
                }
                Projectile.ai[0]++;
                Projectile.damage = 40000; // fixed damage 
                Projectile.CritChance = 0;

                for (int l = 0; l < 5; l++)
                {
                    Vector2 rand = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                    Dust extraDust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric);
                    extraDust.velocity = rand * Main.rand.NextFloat(-40, 40f);

                    Particle spark = new GlowSparkParticle(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(15.5f, 35.5f), true, 15, Main.rand.NextFloat(0.02f, 0.06f), Color.Cyan, new Vector2(2.5f, 0.7f), true);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }
        public override bool CanHitPlayer(Player target)
        {
            if (Projectile.timeLeft <= 20)
            {
                var yeetVec = Vector2.Normalize(target.Center - Projectile.Center);
                target.velocity += yeetVec * (target.noKnockback ? 20f : 40f);
                return true;
            }
            else
                return false;
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => MathHelper.Clamp(CalamityUtils.Convert01To010(completionRatio * 2), 0.2f, 1f) * 4f;
        internal Color ColorFunction(float completionRatio, Vector2 vertexPos) => new Color(123, 205, 237); // Auric blue

        internal float BackgroundWidthFunction(float completionRatio, Vector2 vertexPos) => WidthFunction(completionRatio, vertexPos) * 2f;
        internal Color BackgroundColorFunction(float completionRatio, Vector2 vertexPos) => ColorFunction(completionRatio, vertexPos) * 0.5f;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft <= 20)
            {
                if (lightningTrails.Count <= 0)
                    return false;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            GameShaders.Misc["CalamityMod:TeslaTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            foreach (List<Vector2> points in lightningTrails)
            {
                PrimitiveRenderer.RenderTrail(points, new(WidthFunction, ColorFunction, smoothen: false, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]), 60);
                PrimitiveRenderer.RenderTrail(points, new(BackgroundWidthFunction, BackgroundColorFunction, smoothen: false, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]), 60);
            }

                Main.spriteBatch.ExitShaderRegion();
            }
            return false;
        }
        public override bool? CanDamage() => (Projectile.timeLeft <= 20 ? null : false);

        // If anything somehow survives the blast, it is inflicted with Auric Rebuke for 2 seconds.
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<AuricRebuke>(), 120);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<AuricRebuke>(), 120);
    }
}
