using System;
using System.IO;
using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    //Based off Virid Vanguard projectile code for circling and drawing logic
    public class IgneousBlade : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public enum AIState
        {
            CircleOwner,
            TransitionToLaunch,
            LaunchAtPos
        }

        public int BladeIndex;

        public VertexStrip TrailDrawer;

        public Vector2 ChargeStartingPosition;

        public float BladeHoverOffsetAngle
        {
            get
            {
                float projectileCounts = Owner.ownedProjectileCounts[Type];
                if (projectileCounts <= 1f)
                    projectileCounts = 1f;

                return MathHelper.WrapAngle(MathHelper.TwoPi * BladeIndex / projectileCounts + MathHelper.TwoPi * (Owner.miscCounter % 60 / 60f)) * (Owner.Calamity().InvertExaltationLineRotationDirections ? -1 : 1);
            }
        }

        public AIState CurrentState
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];

        public ref float AITimer => ref Projectile.ai[1];

        public ref float DistanceTimer => ref Projectile.ai[2];

        public ref float BladeGleamInterpolant => ref Projectile.localAI[0];

        public Vector2 ChargeTargetPos = Vector2.Zero;
        public Vector2 ChargeStartPos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 45;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;

            Projectile.width = 84;
            Projectile.height = 84;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90000;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 7;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(BladeIndex);
            writer.WriteVector2(ChargeStartingPosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            BladeIndex = reader.ReadInt32();
            ChargeStartingPosition = reader.ReadVector2();
        }


        public override void AI()
        {
            // Decide whether the minion should still exist.
            HandleMinionBools();

            // Reset extra updates.
            Projectile.MaxUpdates = 1;

            // Have the gleam interpolant dissipate.
            BladeGleamInterpolant = MathHelper.Lerp(BladeGleamInterpolant, 0f, 0.1f);
            if (BladeGleamInterpolant <= 0.02f)
                BladeGleamInterpolant = 0f;

            switch (CurrentState)
            {
                case AIState.CircleOwner:
                    CircleOwner();
                    break;
                case AIState.TransitionToLaunch:
                    Owner.Calamity().mouseWorldListener = true;
                    ChargeTargetPos = Owner.Calamity().mouseWorldDeltaFromPlayer;
                    ChargeStartPos = Owner.Center;
                    CurrentState = AIState.LaunchAtPos;
                    AITimer = 0;
                    Projectile.penetrate = 15;
                    LaunchAtTargetPos();
                    break;
                case AIState.LaunchAtPos:
                    LaunchAtTargetPos();
                    break;
            }
            if (CurrentState == AIState.CircleOwner)
            {
                if (Owner.HeldItem.type == ModContent.ItemType<IgneousExaltation>())
                    AITimer++;
                else
                    AITimer--;
                AITimer = MathHelper.Clamp(AITimer, -IgneousExaltation.ChargeCooldown, 0);
            }
            else
                AITimer++;
            DistanceTimer++;
        }

        public void CircleOwner()
        {
            Vector2 hoverDestination = Owner.Center + BladeHoverOffsetAngle.ToRotationVector2() * (DistanceTimer < 0 ? MathHelper.Lerp(200, 100, 1 - (-DistanceTimer) / IgneousExaltation.ChargeCooldown) : 100f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, hoverDestination, 0.04f).MoveTowards(hoverDestination, 24f);
            Projectile.velocity *= 0.8f;

            // Teleport to the owner if sufficiently far away.
            if (!Projectile.WithinRange(Owner.Center, 3200f))
            {
                Projectile.Center = hoverDestination;
                Projectile.netUpdate = true;
            }

            // Aim away from the target.
            Projectile.rotation = Projectile.AngleFrom(Owner.Center) + MathHelper.PiOver2;
        }

        public void LaunchAtTargetPos()
        {
            int chargeTime = IgneousExaltation.ChargeDuration * 2;
            int chargeLength = 2000;
            float circleWidth = Math.Min(75, Owner.ownedProjectileCounts[Type] * 4);
            var CurrentPlace = Vector2.Lerp(ChargeStartPos, ChargeStartPos + ChargeTargetPos.SafeNormalize(Vector2.UnitX) * chargeLength, MathF.Pow(AITimer / chargeTime, 3));

            Projectile.Center = CurrentPlace + BladeHoverOffsetAngle.ToRotationVector2() * Math.Max(MathHelper.Lerp(100f, 25f, AITimer / 10f), circleWidth);
            Projectile.rotation = Projectile.AngleFrom(CurrentPlace) + MathHelper.PiOver2;
            if (AITimer >= chargeTime)
            {
                Projectile.damage = Projectile.originalDamage;
                AITimer = -IgneousExaltation.ChargeCooldown;
                DistanceTimer = -IgneousExaltation.ChargeCooldown;
                CurrentState = AIState.CircleOwner;
                Projectile.penetrate = -1;
            }
        }

        public void HandleMinionBools()
        {
            Owner.AddBuff(ModContent.BuffType<IgneousExaltationBuff>(), 3600);
            if (Projectile.type == ModContent.ProjectileType<IgneousBlade>())
            {
                if (Owner.dead)
                    Owner.Calamity().igneousExaltation = false;

                if (Owner.Calamity().igneousExaltation)
                    Projectile.timeLeft = 2;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.penetrate >= 10) // First 5 hits deal 2x dmg
                modifiers.SourceDamage *= 2;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.penetrate >= 10 && CurrentState != AIState.CircleOwner) //The first 5 hits per launch can spawn blades
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        Vector2 spawnPosition = Projectile.Center - new Vector2(0f, 550f).RotatedByRandom(MathHelper.TwoPi);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Vector2.Normalize(Projectile.Center - spawnPosition) * 24f, ModContent.ProjectileType<IgneousBladeStrike>(),
                            Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    for (int i = 0; i < Main.rand.Next(28, 41); i++)
                    {
                        Dust.NewDustPerfect(
                            Projectile.Center + Utils.NextVector2Unit(Main.rand) * Main.rand.NextFloat(10f),
                            DustID.Torch,
                            Utils.NextVector2Unit(Main.rand) * Main.rand.NextFloat(1f, 4f));
                    }
                    Projectile.netUpdate = true;
                }
            base.OnHitNPC(target, hit, damageDone);
        }
        public Color TrailColorFunction(float completionRatio)
        {
            float opacity = (float)Math.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4D) * Projectile.Opacity * 0.48f;
            var redColor = new Color(166, 46, 61);
            return Color.Lerp(redColor, new(64, 51, 66), MathHelper.Clamp(completionRatio * 1.4f, 0f, 1f)) * opacity;
        }

        public float TrailWidthFunction(float completionRatio) => Projectile.height * (1f - completionRatio) * 0.3f;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ^ Owner.Calamity().InvertExaltationLineRotationDirections ? SpriteEffects.FlipHorizontally : SpriteEffects.None;


            // Draw the afterimage trail.
            TrailDrawer ??= new();
            GameShaders.Misc["EmpressBlade"].UseImage0("Images/Extra_201");
            GameShaders.Misc["EmpressBlade"].UseImage1("Images/Extra_193");
            GameShaders.Misc["EmpressBlade"].UseShaderSpecificData(new Vector4(1f, 0f, 0f, 0.6f));
            GameShaders.Misc["EmpressBlade"].Apply(null);
            TrailDrawer.PrepareStrip(Projectile.oldPos, Projectile.oldRot, TrailColorFunction, TrailWidthFunction, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailDrawer.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // Draw the blade.
            float outlineOpacity = 1;
            float outlineWidth = 1;
            if ((CurrentState == AIState.CircleOwner && AITimer < 0))
            {
                outlineWidth = Math.Clamp(MathF.Pow(1 - (AITimer) / -IgneousExaltation.ChargeCooldown, 3), 0, 100);
                outlineOpacity = 0.75f;
            }
            var outlineTex = IgneousExaltation.GetBladeOutlineTex();
            float rotation = Projectile.rotation - MathHelper.PiOver4 * (Owner.Calamity().InvertExaltationLineRotationDirections ? -1 : 1);
            Main.EntitySpriteDraw(outlineTex, drawPosition + new Vector2(2, 0) * outlineWidth, frame, new Color(166, 46, 61) * outlineOpacity, rotation, origin, Projectile.scale, direction, 0);
            Main.EntitySpriteDraw(outlineTex, drawPosition + new Vector2(0, 2) * outlineWidth, frame, new Color(166, 46, 61) * outlineOpacity, rotation, origin, Projectile.scale, direction, 0);
            Main.EntitySpriteDraw(outlineTex, drawPosition + new Vector2(-2, 0) * outlineWidth, frame, new Color(166, 46, 61) * outlineOpacity, rotation, origin, Projectile.scale, direction, 0);
            Main.EntitySpriteDraw(outlineTex, drawPosition + new Vector2(0, -2) * outlineWidth, frame, new Color(166, 46, 61) * outlineOpacity, rotation, origin, Projectile.scale, direction, 0);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0);

            // Draw the gleam at the tip of the blade.
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
            shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

            Vector2 lensFlareWorldPosition = Projectile.Center + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Projectile.width * Projectile.scale * 0.88f;
            Color lensFlareColor = Color.Lerp(Color.LimeGreen, Color.Yellow, 0.23f) with { A = 0 } * BladeGleamInterpolant;
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);

            // Reset textures for shaders, since they're only defined once at load-time in vanilla.
            GameShaders.Misc["EmpressBlade"].UseImage0("Images/Extra_209");
            GameShaders.Misc["EmpressBlade"].UseImage1("Images/Extra_210");
            return false;
        }
    }
}
