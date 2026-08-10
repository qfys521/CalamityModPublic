using System;
using System.Linq;
using CalamityMod.DataStructures;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SHPS : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float Timer => ref Projectile.ai[1];

        // Whether or not this soul can be collected by the SHPC or not.
        public bool IsAPickupSoul
        {
            get => Projectile.ai[2] == 1f;
            set => Projectile.ai[2] = value.ToInt();
        }

        private float AIState = 0f; // 0 = Idle, 1 = Homing to enemy, 2 = Getting sucked to player
        private const float HomingRange = 560f;

        private NPC Target;
        private Projectile ToSuckTowards;
        public float RandomAnglingStrength = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
        }

        // Can only deal damage after it starts homing
        public override bool? CanDamage() => Timer >= 25f && !IsAPickupSoul;

        public override void AI()
        {
            Timer++;

            // Determine behavior
            if (Timer >= 25) // Always remains idle for a short delay
            {
                if (!IsAPickupSoul) // Only run homing code if not a pickup
                {
                    float npcDistCheck = HomingRange;
                    int index = -1;
                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        if (!n.CanBeChasedBy(Projectile))
                            continue;

                        float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                        if (currentNPCDist < npcDistCheck)
                        {
                            npcDistCheck = currentNPCDist;
                            index = n.whoAmI;
                        }
                    }

                    if (index == -1)
                        AIState = 0f;
                    else
                    {
                        Target = Main.npc[index];
                        AIState = 1f;
                    }
                }
                else // Only run suction code if a pickup
                {
                    foreach (Projectile p in Main.ActiveProjectiles)
                    {
                        if (p.type == ModContent.ProjectileType<SHPV>() && p.Colliding(p.Hitbox, Projectile.Hitbox))
                        {
                            if (AIState != 2f)
                            {
                                AIState = 2f;
                                Projectile.ExpandHitboxBy(70);
                            }
                            ToSuckTowards = p;
                            break;
                        }
                        else
                            AIState = 0f;
                    }
                }
            }

            // Actual behavior
            switch (AIState)
            {
                case 0f:
                    Projectile.extraUpdates = 0;
                    // Randomly changes the strength of idle turning
                    if (Timer % 30 == 1f)
                        RandomAnglingStrength = Main.rand.NextFloat(-0.16f, 0.16f);
                    Projectile.velocity = Projectile.velocity.RotatedBy(RandomAnglingStrength);
                    if (Projectile.velocity.Length() > 2.75f && IsAPickupSoul)
                        Projectile.velocity *= 0.96f;
                    break;

                case 1f:
                    Projectile.extraUpdates = 1;
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Target.Center).ToRotation(), 0.15f).ToRotationVector2() * speed;
                    break;

                case 2f:
                    // Attempt to smoothly fly towards SHPC's barrel.
                    Vector2 idealVelocity = Projectile.SafeDirectionTo(ToSuckTowards.ModProjectile<SHPV>().TipPosition + Main.player[Projectile.owner].velocity) * 30f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.1f);

                    if (Vector2.Distance(Projectile.Center, ToSuckTowards.ModProjectile<SHPV>().TipPosition) < 70f)
                    {
                        ToSuckTowards.ModProjectile<SHPV>().SoulColors.Add(Projectile.ai[0]);
                        Projectile.Kill();
                    }
                    break;
            }

            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                    Vector2 dustVelocity = Projectile.velocity * -1.2f;
                    float dustScale = Main.rand.NextFloat(0.6f, 0.8f); 
                    Dust dust = Dust.NewDustDirect(dustSpawnPosition, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, SHPB.FindColorForSoul((int)Projectile.ai[0]), dustScale);
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.noLightEmittance = false;
                }
            }

            // Pickup souls give off separate particles to differentiate them from Attack souls.
            if (Main.rand.NextBool(6) && IsAPickupSoul)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 smokeVelocity = Main.rand.NextVector2Circular(1f, 1f) * 0.65f;
                    int smokeLifetime = Main.rand.Next(30, 45);
                    float smokeScale = Main.rand.NextFloat(0.15f, 0.3f);
                    float smokeOpacity = Main.rand.NextFloat(0.75f, 0.9f);

                    HeavySmokeParticle ghastlySmoke = new(Projectile.Center, smokeVelocity, SHPB.FindColorForSoul((int)Projectile.ai[0]), smokeLifetime, smokeScale, smokeOpacity, 0.02f, true);
                    GeneralParticleHandler.SpawnParticle(ghastlySmoke);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn a bunch of dust along the length of the trail a soul is killed.
            BezierCurve curve = new(Projectile.oldPos);
            for (int i = 0; i < 35; i++)
            {
                Vector2 dustSpawnPosition = curve.Evaluate(Main.rand.NextFloat());
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 3f;
                float dustScale = Main.rand.NextFloat(1.2f, 1.8f);
                Dust dust = Dust.NewDustDirect(dustSpawnPosition, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, SHPB.FindColorForSoul((int)Projectile.ai[0]), dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittance = false;
            }

            // Spawn a burst of dust at the center.
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 6f;
                float dustScale = Main.rand.NextFloat(1.8f, 2.4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, SHPB.FindColorForSoul((int)Projectile.ai[0]), dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittance = false;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => false;

        public float SoulWidthFunction(float completion, Vector2 _)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 24f;
            float curveRatio = 0.15f;

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width;
        }

        public Color SoulColorFunction(float completion, Vector2 _)
        {
            Color tipColor = Color.Lerp(SHPB.FindColorForSoul((int)Projectile.ai[0]), Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(SHPB.FindColorForSoul((int)Projectile.ai[0]), tipColor, completion);
        }

        public float SoulCoreWidthFunction(float completion, Vector2 _)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 14f;
            float curveRatio = 0.15f;

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width;
        }

        public Color SoulCoreColorFunction(float completion, Vector2 _)
        {
            Color tipColor = Color.Lerp(Color.White, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(Color.White, tipColor, completion);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            // Render the main trail for the body for the soul.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(SoulWidthFunction, SoulColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length * 2);

            // Render a smaller, pure white trail in the same position to represent the glowing white core of the soul.
            Vector2[] soulCoreLength = Projectile.oldPos.Take(8).ToArray();
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(soulCoreLength, new(SoulCoreWidthFunction, SoulCoreColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), soulCoreLength.Length * 2);
        }
    }
}
