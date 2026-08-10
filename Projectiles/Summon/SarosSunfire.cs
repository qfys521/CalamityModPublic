using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class SarosSunfire : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.MaxUpdates = 3;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 24;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.timeLeft = 120 * Projectile.MaxUpdates;
        }

        public override void OnSpawn(IEntitySource source)
        {
        }

        NPC target = null;
        public override void AI()
        {
            if (Projectile.ai[0] > 1)
            {
                Projectile.ai[0]--;
                Projectile.timeLeft = 120 * Projectile.MaxUpdates;

                
            }

            if (target != null && Projectile.localNPCImmunity[target.whoAmI] <= 0 && target.active && !target.dontTakeDamage)
            {
                Projectile.Calamity().HomingTarget = target.whoAmI;
                Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(Projectile.DirectionTo(target.Center).ToRotation(), 0.5f * (1 - Projectile.ai[0] / 120f)).ToRotationVector2() * Projectile.velocity.Length();
            }
            else
            {
                target = GetTargetInRange(2000);
            }

            if (Projectile.damage <= 0)
            {
                Projectile.ai[0] = 0;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 4;
                Projectile.timeLeft = (int)MathHelper.Min(8, Projectile.timeLeft);
            }
            }
        NPC GetTargetInRange(float range)
        {
            var player = Main.player[Projectile.owner];
            if (player.HasMinionAttackTargetNPC && Main.npc[player.MinionAttackTargetNPC].CanBeChasedBy() && Projectile.localNPCImmunity[player.MinionAttackTargetNPC] <= 0 && Projectile.IsInRangeOfMeOrMyOwner(Main.npc[player.MinionAttackTargetNPC], range, out var _, out var _, out var _))
            {
                return Main.npc[player.MinionAttackTargetNPC];
            }
            else
            {
                NPC gotTarget = null;
                float currentDistance = range;
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (Projectile.localNPCImmunity[npc.whoAmI] > 0)
                        continue;
                    var myDistance = npc.Distance(Projectile.Center);
                    
                    if (npc.CanBeChasedBy() && myDistance < currentDistance)
                    {
                        currentDistance = myDistance;
                        gotTarget = npc;
                    }
                }
                return gotTarget;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target != this.target)
                return false;
            return null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.75f);
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 6f;
                float dustScale = Main.rand.NextFloat(3f, 5f);
                Color dustColor = Color.Lerp(Color.OrangeRed, Color.Gold, Main.rand.NextFloat(0.5f, 1f));

                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, dustColor, dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittance = true;
            }
        }

        public override bool? CanDamage()
        {
            return Projectile.ai[0] <= 90;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => false;

        //Prim trail based off Fryzahh's work on Faith Incinerator
        public float FireWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = 48f * Projectile.scale;
            float curveRatio = 0.2f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);
            if (completion < curveRatio)
                width = MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);

            // Pulse inwards and outwards over time.
            float pulseInterpolant = MathF.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return (width + additionalPulseWidth) * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = new Color(238, 226, 153);
            return Color.Lerp(mainColor, Color.Transparent, completion);
        }

        public float FireCoreWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 32;
            float curveRatio = 0.25f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireCoreColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = new Color(255, 191, 73);
            return mainColor;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            //Flame body
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(FireWidthFunction, FireColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length + 32);
            //Flame core
            Vector2[] fireCoreLength = Projectile.oldPos.Take(8).ToArray();
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(fireCoreLength, new(FireCoreWidthFunction, FireCoreColorFunction, (_,_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), fireCoreLength.Length + 24);
        }
    }
}
