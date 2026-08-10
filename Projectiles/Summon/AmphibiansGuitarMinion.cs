using System;
using System.Collections.Generic;
using CalamityMod.Buffs.Summon;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Summon
{
    public class AmphibiansGuitarMinion : BaseMinionProjectile
    {
        public override int AssociatedProjectileTypeID => ProjectileType<AmphibiansGuitarMinion>();

        public override int AssociatedBuffTypeID => BuffType<AmphibiansGuitarBuff>();

        public override ref bool AssociatedMinionBool => ref ModdedOwner.AmphibiansGuitarBool;

        /// <summary>
        /// A property that states which guitar sprite is using from the spritesheet.<br/>
        /// First guitar goes from 0 and the last to 7.
        /// </summary>
        private int GuitarSprite
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = MathHelper.Clamp(value, 0, 7);
        }
        private ref float ShootTimer => ref Projectile.ai[1];
        private ref float ShootCount => ref Projectile.ai[2];

        // Position is same for targeting and non targeting but is left open to be changed because they may want it changed.
        private Vector2 RotationPosition => Target == null ? Owner.MountedCenter : Owner.MountedCenter;
        private float IntendedRotationAngle => MathHelper.TwoPi / (Owner == null ? 1f : MathHelper.Clamp(Owner.ownedProjectileCounts[Type], 1f, 8f)) * GuitarSprite + Time * 2.4f;
        private static float Time => Main.GameUpdateCount / 60f;

        private bool _hasSpawned;
        private Color _effectsColor = Color.White;

        public override void SetDefaults()
        {
            base.SetDefaults();
            (Projectile.width, Projectile.height) = (92, 92);
        }

        public override void MinionAI()
        {
            if (!_hasSpawned)
            {
                // Pitch matches the song if played with proper timing
                SoundStyle spawnSound = new("CalamityMod/Sounds/Item/AmphibiansGuitarSummon");
                SoundEngine.PlaySound(spawnSound with { Volume = 0.8f, Pitch = (GuitarSprite == 2 || GuitarSprite == 5) ? -0.15f : 0f }, Projectile.Center);

                ShootTimer = GuitarSprite * 12f;

                _hasSpawned = true;
            }

            float oscillation = MathHelper.Clamp(Math.Abs((float)Math.Sin(Time * 5f / MathHelper.Pi)), 0f, 1f);
            Vector2 intendedPosition = RotationPosition - Vector2.UnitY.RotatedBy(IntendedRotationAngle) * (Target == null ? 100f : (Target.Size.Length() / 2f) + (250f - 150f * oscillation));
            Projectile.Center = Vector2.Lerp(Projectile.Center, intendedPosition, Utils.Remap(Projectile.DistanceSQ(intendedPosition), 6400f, 0f, 0.1f, 0.3f));
            Projectile.rotation = IntendedRotationAngle;

            if (Target != null)
            {
                bool bigShot = ShootCount % 3 == 0;
                if (ShootTimer > 96f && Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < (bigShot ? 3 : 1); i++)
                    {
                        float rot = i == 1 ? -0.25f : i == 2 ? 0.25f : 0;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            CalamityUtils.CalculatePredictiveAimToTarget(Projectile.Center, Target, (bigShot ? 36f : 25f) * (1f - Math.Abs(rot))).RotatedBy(rot),
                            ProjectileType<AmphibiansGuitarProjectile>(),
                            Projectile.damage,
                            Projectile.knockBack,
                            Projectile.owner,
                            (Main.rand.NextBool() && Owner.ownedProjectileCounts[Type] == 8).ToInt(),
                            Main.rand.Next(0, 4 + 1),
                            bigShot ? 5 : 0);
                    }

                    if (bigShot)
                    {
                        SoundStyle fire = new("CalamityMod/Sounds/Item/Evernote");
                        SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f), MaxInstances = 10 }, Projectile.Center);

                        Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, _effectsColor, "CalamityMod/Particles/HighResFoggyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-10, 10), 0.01f, 0.09f, 17);
                        GeneralParticleHandler.SpawnParticle(blastRing);
                    }
                    else
                    {
                        SoundStyle fireSmall = new("CalamityMod/Sounds/Item/WulfrumProsthesisShoot");
                        SoundEngine.PlaySound(fireSmall with { Volume = 0.3f, Pitch = Main.rand.NextFloat(0.6f, 0.7f) }, Projectile.Center);
                    }

                    ShootCount++;
                    ShootTimer = 0;
                }

                if (ShootTimer < 10f)
                    Projectile.Center -= Utils.DirectionTo(Projectile.Center, Target.Center) * 10;

                float rate = Time * 2f;
                List<Color> eColors =
                [
                    Color.Red,
                    Color.Cyan,
                    Color.Goldenrod,
                    Color.Magenta,
                    Color.Lime,
                ];

                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                _effectsColor = Color.Lerp(Color.White, Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f), 0.7f);

                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(70, 70), DustType<LightDust>(), (Utils.DirectionTo(Projectile.Center, intendedPosition) * -9).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.75f, 1.25f);
                    dust.color = _effectsColor;
                    dust.noLightEmittance = true;
                }
                else
                {
                    Particle spark = new GlowOrbParticle(Projectile.Center + Main.rand.NextVector2Circular(70, 70), (Utils.DirectionTo(Projectile.Center, intendedPosition) * -9f).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 1f), false, 7, Main.rand.NextFloat(0.5f, 0.8f), _effectsColor, true, false, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                ShootTimer++;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(horizontalFrames: 8, frameX: GuitarSprite);

            Projectile.DrawProjectileWithBackglow(_effectsColor with { A = 0 }, lightColor, Target == null ? 0 : 3, texture, frame);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                Projectile.rotation,
                frame.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None);

            return false;
        }
    }
}
