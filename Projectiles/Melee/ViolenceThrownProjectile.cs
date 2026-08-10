using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityMod.NPCs.ExoMechs.Ares;
using System.Linq;
using Terraria.DataStructures;

namespace CalamityMod.Projectiles.Melee
{
    [PierceResistException]
    public class ViolenceThrownProjectile : ModProjectile
    {
        private int hitstop = 0;
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Violence>();
        internal Player Owner => Main.player[Projectile.owner];
        internal ref float Time => ref Projectile.ai[0];
        public override string Texture => "CalamityMod/Items/Weapons/Melee/Violence";

        public bool isJavelin
        {
            get
            {
                if (Projectile.ai[2] == 0)
                    return true;
                else
                    return false;
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        }

        private Vector2 NormalVelocity = Vector2.Zero;
        private NPC target;
        private Vector2 targetoffset = Vector2.Zero;
        private int[] offsetadjust = new int[]
        {
            ModContent.NPCType<AresBody>(),
            ModContent.NPCType<AresLaserCannon>(),
            ModContent.NPCType<AresTeslaCannon>(),
            ModContent.NPCType<AresGaussNuke>(),
            ModContent.NPCType<AresPlasmaFlamethrower>(),
        };

        public override void OnSpawn(IEntitySource source)
        {
            if (isJavelin)
                JavelinSetDefaults();
            else
                YoyoSetDefaults();
        }

        //These methods adjusts to which AI type is in use
        #region Variable Methods

        public override void SetDefaults()
        {
            Projectile.timeLeft = 60;
            //The rest of this runs in OnSpawn instead due to variable AI stuff
        }
        public override void AI()
        {
            if (isJavelin)
                JavelinAI();
            else
                YoyoAI();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (isJavelin && Projectile.timeLeft > 2)
                return JavelinPreDraw(ref lightColor);
            else
                return YoyoPreDraw(ref lightColor);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (isJavelin)
                JavelinModifyDamageHitbox(ref hitbox);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (isJavelin)
                JavelinOnHitNPC(target, hit, damageDone);
            else
                YoyoOnHitNPC(target, hit, damageDone);
        }
        #endregion

        //Methods for controlling the Javelin throw
        #region Javelin Methods
        public void JavelinAI()
        {

            var player = Main.player[Projectile.owner];
            float adust = MathHelper.ToRadians(45 + 180);
            if (hitstop > 0)
            {
                hitstop--;
                Projectile.timeLeft++;
                Projectile.velocity = NormalVelocity * 0.01f + (offsetadjust.Contains(target.type) ? target.position - targetoffset : Vector2.Zero);
                targetoffset = target.position;

                Vector2 impactPoint = Vector2.Lerp(Projectile.Center, target.Center, 0.5f);//Projectile.Center + new Vector2(0,-100).RotatedBy(Projectile.rotation + MathHelper.Pi/4f);
                Vector2 bloodSpawnPosition = impactPoint + Main.rand.NextVector2Circular(target.width, target.height) * 0.04f;
                Vector2 splatterDirection = Projectile.velocity.SafeNormalize(Vector2.Zero) * -1;

                // Emit blood if the target is organic.
                if (target.Organic())
                {
                    if (Projectile.FinalExtraUpdate())
                        SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.2f }, Projectile.Center);
                    for (int i = 0; i < 1; i++)
                    {
                        int bloodLifetime = Main.rand.Next(22, 36);
                        float bloodScale = Main.rand.NextFloat(0.6f, 0.8f);
                        Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                        bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                        if (Main.rand.NextBool(20))
                            bloodScale *= 2f;

                        Vector2 bloodVelocity = splatterDirection.RotatedByRandom(0.81f) * Main.rand.NextFloat(11f, 23f);
                        bloodVelocity.Y -= 12f;
                        BloodParticle blood = new BloodParticle(bloodSpawnPosition, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        float bloodScale = Main.rand.NextFloat(0.2f, 0.33f);
                        Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.5f, 1f));
                        Vector2 bloodVelocity = splatterDirection.RotatedByRandom(0.9f) * Main.rand.NextFloat(9f, 14.5f);
                        BloodParticle2 blood = new BloodParticle2(bloodSpawnPosition, bloodVelocity, 20, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                }

                // Emit sparks if the target is not organic.
                else
                {

                    if (Projectile.FinalExtraUpdate())
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int sparkLifetime = Main.rand.Next(22, 36);
                            float sparkScale = Main.rand.NextFloat(0.8f, 1f) + 1 * 0.85f;
                            Color sparkColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.7f));
                            sparkColor = Color.Lerp(sparkColor, Color.Orange, Main.rand.NextFloat());

                            if (Main.rand.NextBool(10))
                                sparkScale *= 2f;

                            Vector2 sparkVelocity = splatterDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(12f, 25f);
                            sparkVelocity.Y -= 6f;
                            SparkParticle spark = new SparkParticle(impactPoint, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.2f }, Projectile.Center);
                    }
                }


            }
            else
            {
                Projectile.velocity = NormalVelocity;
            }
            if (Projectile.timeLeft == 1)
            {
                Projectile.timeLeft++;

                ReturnToOwner();
                Projectile.rotation += 0.45f;
                Projectile.extraUpdates = 3;
                Projectile.damage = 0;
                Time = 60;
            }
             Lighting.AddLight(Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * Projectile.height * 0.45f, Color.Red.ToVector3() * 0.4f);
        }
        public bool JavelinPreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            // Not cloning the points causes the below operations to be applied to the original oldPos value by reference
            // and thus causes it to be consistently added over and over, which is not intended behavior.
            Vector2[] drawPoints = (Vector2[])Projectile.oldPos.Clone();
            Vector2 aimAheadDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            PrimitiveRenderer.RenderTrail(Projectile.oldPos.Take(60).ToArray(), new(PrimitiveWidthFunction, PrimitiveColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"], smoothen: true), 25);
            return true;
        }
        public void JavelinSetDefaults()
        {
            Projectile.timeLeft = 60;
            Projectile.width = Projectile.height = 142;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 4;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.Center = Main.player[Projectile.owner].Center;
            NormalVelocity = Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathF.PI / 4;
        }
        public void JavelinModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(45 * Projectile.scale);
            hitbox.Width = (int)(45 * Projectile.scale);
            hitbox.Location = (center + NormalVelocity.SafeNormalize(Vector2.Zero) * 70 - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }
        public void JavelinOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[Projectile.owner];
            this.target = target;
            targetoffset = target.position;
            float x = Utils.GetLerpValue(1000f, 6000f, damageDone, true);
            int x1 = (int)(20 * (x) * (Projectile.extraUpdates + 1));
            if (damageDone > 5)
            {
                hitstop = x1;
                Projectile.damage = (int)(Projectile.damage * 0.9f);
            }
            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 240);


            if (Main.netMode != NetmodeID.Server)
            {
                // Play a splatter and impact sound.
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

                float damageInterpolant = Utils.GetLerpValue(950f, 2000f, hit.Damage, true);
                float impactAngularVelocity = MathHelper.Lerp(0.08f, 0.2f, damageInterpolant);
                float impactParticleScale = MathHelper.Lerp(0.6f, 1f, damageInterpolant);
                impactAngularVelocity *= Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(0.75f, 1.25f);

                Color impactColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.5f));
                Vector2 impactPoint = Vector2.Lerp(Projectile.Center, target.Center, 0.65f);


                // And create an impact point particle.
                ImpactParticle impactParticle = new ImpactParticle(impactPoint, impactAngularVelocity, 20, impactParticleScale, impactColor);
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }

        }
        #endregion

        //These methods are the original Violence "Yoyo" functionality
        #region Yoyo Methods
        public void YoyoSetDefaults()
        {
            Projectile.width = Projectile.height = 142;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90000;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.tileCollide = false;
        }

        public void YoyoAI()
        {
            // Fade in.
            Projectile.Opacity = Utils.GetLerpValue(0f, 15f, Time, true);

            if (Owner.channel)
            {
                HomeTowardsMouse();
                Projectile.rotation += 0.45f / Projectile.MaxUpdates;
            }
            else
            {
                ReturnToOwner();
                float idealAngle = Projectile.AngleTo(Owner.Center) - MathHelper.PiOver4;
                Projectile.rotation = Projectile.rotation.AngleLerp(idealAngle, 0.1f);
                Projectile.rotation = Projectile.rotation.AngleTowards(idealAngle, 0.25f);
            }
            ManipulatePlayerFields();

            // Create some demonic light at the tip of the spear.
            Lighting.AddLight(Projectile.Center + (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * Projectile.height * 0.45f, Color.Red.ToVector3() * 0.4f);
            Time++;
        }

        internal void HomeTowardsMouse()
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 mouse = Owner.ClampedMouseWorld();
            if (Projectile.WithinRange(mouse, Projectile.velocity.Length() * 0.7f))
                Projectile.Center = mouse;
            else
                Projectile.velocity = (Projectile.velocity * 3f + Projectile.DirectionTo(mouse) * 19f) / 4f;
            Projectile.ForceNetUpdate();
        }

        internal void ReturnToOwner()
        {
            Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center, 0.02f);
            Projectile.velocity = Projectile.SafeDirectionTo(Owner.Center) * 22f;
            if (Projectile.Hitbox.Intersects(Owner.Hitbox))
            {
                for (int i = 0; i < 75; i++)
                {
                    Dust fire = Dust.NewDustPerfect(Owner.Center, DustID.Torch);
                    fire.velocity = (MathHelper.TwoPi * i / 75f).ToRotationVector2() * 4f - Vector2.UnitY * 3f;
                    fire.scale = 1.4f;
                    fire.noGravity = true;
                }
                Projectile.Kill();
            }
        }

        internal void ManipulatePlayerFields()
        {
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
        }

        public void YoyoOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Main.dedServ)
            {
                // Play a splatter and impact sound.
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

                float damageInterpolant = Utils.GetLerpValue(950f, 2000f, hit.Damage, true);
                float impactAngularVelocity = MathHelper.Lerp(0.08f, 0.2f, damageInterpolant);
                float impactParticleScale = MathHelper.Lerp(0.6f, 1f, damageInterpolant);
                impactAngularVelocity *= Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(0.75f, 1.25f);

                Color impactColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.5f));
                Vector2 impactPoint = Vector2.Lerp(Projectile.Center, target.Center, 0.65f);
                Vector2 bloodSpawnPosition = target.Center + Main.rand.NextVector2Circular(target.width, target.height) * 0.04f;
                Vector2 splatterDirection = (Projectile.Center - bloodSpawnPosition).SafeNormalize(Vector2.UnitY);

                // Emit blood if the target is organic.
                if (target.Organic())
                {
                    SoundEngine.PlaySound(SoundID.NPCHit18, Projectile.Center);
                    for (int i = 0; i < 6; i++)
                    {
                        int bloodLifetime = Main.rand.Next(22, 36);
                        float bloodScale = Main.rand.NextFloat(0.6f, 0.8f);
                        Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                        bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                        if (Main.rand.NextBool(20))
                            bloodScale *= 2f;

                        Vector2 bloodVelocity = splatterDirection.RotatedByRandom(0.81f) * Main.rand.NextFloat(11f, 23f);
                        bloodVelocity.Y -= 12f;
                        BloodParticle blood = new BloodParticle(bloodSpawnPosition, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        float bloodScale = Main.rand.NextFloat(0.2f, 0.33f);
                        Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.5f, 1f));
                        Vector2 bloodVelocity = splatterDirection.RotatedByRandom(0.9f) * Main.rand.NextFloat(9f, 14.5f);
                        BloodParticle2 blood = new BloodParticle2(bloodSpawnPosition, bloodVelocity, 20, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                }

                // Emit sparks if the target is not organic.
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        int sparkLifetime = Main.rand.Next(22, 36);
                        float sparkScale = Main.rand.NextFloat(0.8f, 1f) + damageInterpolant * 0.85f;
                        Color sparkColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.7f));
                        sparkColor = Color.Lerp(sparkColor, Color.Orange, Main.rand.NextFloat());

                        if (Main.rand.NextBool(10))
                            sparkScale *= 2f;

                        Vector2 sparkVelocity = splatterDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(12f, 25f);
                        sparkVelocity.Y -= 6f;
                        SparkParticle spark = new SparkParticle(impactPoint, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

                // And create an impact point particle.
                ImpactParticle impactParticle = new ImpactParticle(impactPoint, impactAngularVelocity, 20, impactParticleScale, impactColor);
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }
        }
        public bool YoyoPreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            Texture2D spearProjectile = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            // Not cloning the points causes the below operations to be applied to the original oldPos value by reference
            // and thus causes it to be consistently added over and over, which is not intended behavior.
            Vector2[] drawPoints = (Vector2[])Projectile.oldPos.Clone();
            Vector2 aimAheadDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();

            if (Owner.channel || isJavelin)
            {
                drawPoints[0] += aimAheadDirection * -12f;
                drawPoints[1] = drawPoints[0] - (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * Vector2.Distance(drawPoints[0], drawPoints[1]);
            }
            for (int i = 0; i < drawPoints.Length; i++)
                drawPoints[i] -= (Projectile.oldRot[i] + MathHelper.PiOver4).ToRotationVector2() * Projectile.height * 0.5f;

            int numPointsRendered = (int)Math.Min(24, Time);
            PrimitiveRenderer.RenderTrail(drawPoints.Take((int)Math.Min(Time, 36)).ToArray(), new(PrimitiveWidthFunction, PrimitiveColorFunction, (_,_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"], smoothen: true), 24);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 6; i++)
            {
                float rotation = Projectile.oldRot[i] - MathHelper.PiOver2;
                if (Owner.channel)
                    rotation += 0.2f;

                Color afterimageColor = Color.Lerp(lightColor, Color.Transparent, 1f - (float)Math.Pow(Utils.GetLerpValue(0, 6, i), 1.4D)) * Projectile.Opacity;
                Main.EntitySpriteDraw(spearProjectile, drawPosition, null, afterimageColor, rotation, spearProjectile.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
        #endregion

        #region Shared Methods

        internal float PrimitiveWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float tipWidthFactor = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.01f, 0.04f, completionRatio));
            float bodyWidthFactor = (float)Math.Pow(Utils.GetLerpValue(1f, 0.04f, completionRatio), 0.9D);
            return (float)Math.Pow(tipWidthFactor * bodyWidthFactor, 0.1D) * 30f;
        }

        internal Color PrimitiveColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fadeInterpolant = (float)Math.Cos(Main.GlobalTimeWrappedHourly * -9f + completionRatio * 6f + Projectile.identity * 2f) * 0.5f + 0.5f;

            // Adjust the fade interpolant to be on a different scale via a linear interpolation.
            fadeInterpolant = MathHelper.Lerp(0.15f, 0.75f, fadeInterpolant);
            Color frontFade = Color.Lerp(new Color(255, 145, 115), new Color(113, 0, 159), fadeInterpolant);

            // Go halfway to a dark red color.
            frontFade = Color.Lerp(frontFade, Color.DarkRed, 0.5f);
            Color backFade = new Color(255, 145, 115);

            return Color.Lerp(frontFade, backFade, (float)Math.Pow(completionRatio, 1.2D)) * (float)Math.Pow(1f - completionRatio, 1.1D) * Projectile.Opacity;
        }
        #endregion
    }
}
