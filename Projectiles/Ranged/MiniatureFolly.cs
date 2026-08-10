using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class MiniatureFolly : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public bool SpawnedByFatFuck => Projectile.ai[2] == 1f;

        // Randomized trail offsets required
        public List<Vector2> TrailPos = new List<Vector2>();
        public const int TrailLength = 12;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 180 * Projectile.MaxUpdates;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit51, Projectile.Center);

            for (int i = 0; i < 4; i++)
            {
                Color color = Color.Lerp(Color.Red, Color.Magenta, Main.rand.NextFloat(0f, 0.6f));
                Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.ToRadians(15f) * (i + Main.rand.NextFloat(-0.5f, 0.5f))) * (Main.rand.NextFloat(8f, 12f));
                BoltParticle bolt = new BoltParticle(Projectile.Center, velocity, false, 18, Main.rand.NextFloat(0.4f, 0.6f), color, new Vector2(0.6f, 1f), true);
                GeneralParticleHandler.SpawnParticle(bolt);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<VermillionFlux>(), 90);

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            if (Main.rand.NextBool(5))
            {
                Dust dustTrail = Dust.NewDustPerfect(Projectile.Center, DustID.CopperCoin, Main.rand.NextVector2Circular(0.2f, 0.2f));
                dustTrail.noLight = true;
            }

            if (Projectile.FinalExtraUpdate() || TrailPos == null)
            {
                // Ideally move it closer to the front of the birb
                Vector2 posOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f;

                // Initialize all points immediately
                if (TrailPos == null)
                {
                    TrailPos = new List<Vector2>(TrailLength);
                    for (int i = 0; i < TrailLength; ++i)
                        TrailPos.Add(Projectile.Center + posOffset);
                }

                // Add some random value for the natural lightning look
                Vector2 randOffset = (Vector2.UnitY * Main.rand.NextFloat(-12f, 12f)).RotatedBy(Projectile.rotation);
                TrailPos.Insert(0, Projectile.Center + randOffset + posOffset);

                while (TrailPos.Count > TrailLength)
                    TrailPos.RemoveAt(TrailPos.Count - 1);
            }

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? MathHelper.Pi : 0f);

            CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, SpawnedByFatFuck ? 960f : 300f, 10f, 20f);
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => (1f - completionRatio) * Projectile.scale * 10f;
        internal Color ColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lerp(Color.Red, Color.Magenta, 0.7f * completionRatio + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 20f)) * Projectile.Opacity;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (TrailPos == null)
                return false;

            GameShaders.Misc["CalamityMod:HeavenlyGaleLightningArc"].UseImage1("Images/Misc/Perlin");
            GameShaders.Misc["CalamityMod:HeavenlyGaleLightningArc"].Apply();
            PrimitiveRenderer.RenderTrail(TrailPos, new(WidthFunction, ColorFunction, smoothen: false, shader: GameShaders.Misc["CalamityMod:HeavenlyGaleLightningArc"]), TrailLength);

            // Draw the swarmer at full brightness so it's less awkward with the trail
            Texture2D glow = TextureAssets.Projectile[Type].Value;
            Rectangle frame = glow.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
