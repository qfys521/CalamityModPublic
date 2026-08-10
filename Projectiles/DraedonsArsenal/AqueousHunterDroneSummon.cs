using System;
using CalamityMod.Buffs.Summon;
using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class AqueousHunterDroneSummon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        public int time = 0;
        public ref float attackTimer => ref Projectile.ai[0];
        public ref float interactionTimer => ref Projectile.ai[1];
        public int interactionPoint = 600; // The time needed before shimps do their idle animation
        public int cooldownGiven = 450;
        public bool pressedRight => Projectile.ai[2] == 5;
        public bool doingSpecialAttack = false;
        public float moveSpeed = 30; // Lower is actually faster
        public NPC targetedNPC; // The NPC being targeted
        public float tailRot = 0; // Extra rotation on the tail
        public float tailRecoil = 0; // Recoil for the tail after firing
        public int facingDir = 0;
        public Vector2 headPlace;
        public Vector2 tailPlace;
        public float lastFirePositionX; // Used for some of the movement
        public Vector2 chillSpot; // Stored position for intro animation and some movement
        public Vector2 spawnAnimStart; // Second stored position for the spawn animation
        public Vector2 lastTargetCenter;
        public int attackDowntime = 0; // Timer used for post attack effects
        public int surpriseTimer = 0; // Used for the jump animation when surprized
        public float slidingDirection = 0; // Value that lerps from left to right
        public int animationClickTime = 70; // Time before body and tail click together in spawn anim
        public bool doSpin = false; // If the shrimp will spin for it's attack
        public float specialAttackfx = 0; // spinnning effect opacity mult
        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.SummonTagDamageMultiplier[Type] = 0f;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minionSlots = 4f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            int shrimpNum = Owner.ownedProjectileCounts[Projectile.type];
            facingDir = Math.Sign(Projectile.velocity.X);
            GrantBuffs(Owner);
            if ((targetedNPC == null || targetedNPC.life <= 0 || !targetedNPC.CanBeChasedBy() || attackDowntime == 20) && attackDowntime < 30)
                targetedNPC = Projectile.Center.MinionHoming(2000f, Owner);

            // All the stuff for the spawn animation
            if (time < animationClickTime + 20)
            {
                if (time == 0)
                {
                    chillSpot = Projectile.Center;
                    spawnAnimStart = Projectile.Center;
                    int chosenDir = Math.Sign(Projectile.Center.DirectionTo(Owner.Center).X);
                    Projectile.velocity.X = (chosenDir * 16);
                    tailRot = -2f * chosenDir;
                    attackTimer += 8 * Projectile.localAI[2];
                }
                SpawnAnim();
                facingDir = Math.Sign(Projectile.velocity.X);
                time++;
                return;
            }

            slidingDirection = MathHelper.Lerp(slidingDirection, Owner.direction, 0.03f);

            bool isSummonWeapon = Owner.HeldItem.DamageType.CountsAsClass(DamageClass.Summon);
            if (Owner.Calamity().mouseRight && Owner.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse && Owner.Calamity().arsenalCooldown <= 0 && isSummonWeapon)
            {
                Projectile.ai[2] = 5;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type == Projectile.type && p.owner == Projectile.owner)
                    {
                        p.ai[2] = 5;
                    }
                }
            }

            if (doingSpecialAttack)
            {
                SpecialAttack();
            }
            else
            {
                specialAttackfx = MathHelper.Lerp(specialAttackfx, 0, 0.15f);
                if (targetedNPC != null || attackDowntime > 0)
                {
                    lastTargetCenter = (targetedNPC == null ? Owner.Center : targetedNPC.Center);
                    Attacking();
                }
                else
                {
                    Passive();
                    CheckForSpecialAttack();
                }
            }

            if (tailRecoil > 0)
                tailRecoil = MathHelper.Lerp(tailRecoil, 0, 0.2f);
            if (attackDowntime > 0)
                attackDowntime--;
            time++;
            SetVisualPlacement();
        }
        public void CheckForSpecialAttack()
        {
            if (pressedRight)
            {
                Projectile.ai[2] = 0;
                doingSpecialAttack = true;
                attackTimer = 0;
                if (Projectile.velocity.Length() < 5)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 12;
                else
                    Projectile.velocity *= 1.3f;
                Owner.Calamity().arsenalCooldown = cooldownGiven;
                Owner.AddCooldown(ArsenalPower.ID, cooldownGiven);
            }
        }
        public void SpecialAttack()
        {
            interactionTimer = 0;
            int length = 33;
            specialAttackfx = MathHelper.Lerp(specialAttackfx, 1, 0.2f);

            facingDir = Math.Sign(Projectile.Center.DirectionTo(Owner.Center).X);
            Projectile.velocity *= 0.98f;
            Projectile.rotation += ((MathHelper.TwoPi * 3) / length) * facingDir;

            tailRot = MathHelper.Lerp(tailRot, 0.75f * -facingDir, 0.2f);
            for (int i = -1; i <= 1; i += 2)
            {
                bool noFall = !Main.rand.NextBool(5);
                Vector2 vel = Vector2.UnitY.RotatedBy(Projectile.rotation * -5) * i;
                int dustStyle = noFall ? ModContent.DustType<SquashDust>() : Effects.ArsenalEffects.ArsenalPlasmaDust;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + vel * 43 * specialAttackfx, dustStyle);
                dust.scale = Main.rand.NextFloat(0.8f, 1.1f) + (noFall ? 0 : 0.5f);
                dust.velocity = vel.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - MathHelper.PiOver4 * facingDir) * Main.rand.NextFloat(4, 5.5f);
                dust.noGravity = noFall;
                dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                dust.fadeIn = noFall ? 0 : 0.7f;
            }

            if (attackTimer % 3 == 0)
            {
                int usedTimer = (int)(facingDir == -1 ? (length - attackTimer) : attackTimer);
                SoundEngine.PlaySound(AqueousHunterDrone.Fire with { Volume = 0.7f, Pitch = 0.0f, PitchVariance = 0.3f, MaxInstances = 10 }, Projectile.Center);
                Vector2 shootVel = (MathHelper.PiOver2 / 10 * (int)(usedTimer / 3 - 1)).ToRotationVector2().RotatedBy(-MathHelper.PiOver2 - MathHelper.PiOver4);
                Vector2 shootPlace = Projectile.Center + shootVel * 10;

                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), shootPlace, shootVel * 8, ModContent.ProjectileType<ShrimpPlasmaMissile>(), Projectile.damage, 0, Projectile.owner, 0, 0, attackTimer + 1);
            }
            attackTimer++;
            if (attackTimer > length)
            {
                attackTimer = 8 * Projectile.localAI[2];
                doingSpecialAttack = false;
            }
        }
        public void SpawnAnim()
        {
            Vector2 tailDist = new Vector2((30 - Math.Abs(tailRot) * 15) * (-facingDir), 7 - Math.Abs(tailRot) * 3).RotatedBy(Projectile.rotation);
            Vector2 headDist = new Vector2(0, 0);
            headPlace = Projectile.Center + headDist;
            if (time < animationClickTime)
            {
                tailRot += 0.05f * Projectile.direction;
                Projectile.velocity *= 0.98f;
                Projectile.Center -= Projectile.velocity;
                spawnAnimStart = new Vector2(spawnAnimStart.X, Owner.Center.Y - 700);
                
                tailPlace = chillSpot + tailDist + (Vector2.UnitY * ((float)Math.Pow(time, 2.05f) * 0.085f) + Vector2.UnitX * (16 - Math.Abs(Projectile.velocity.X)) * 10 * Projectile.direction);
            }
            else
                tailPlace = Projectile.Center + tailDist;

            if (time > animationClickTime - 40)
            {
                if (time == animationClickTime - 1)
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver4 * Projectile.direction) * 3;
                    SoundStyle click = new("CalamityMod/Sounds/Item/DudFire");
                    SoundEngine.PlaySound(click with { Pitch = 0.4f }, Projectile.Center);
                    SoundEngine.PlaySound((Main.rand.NextBool() ? AqueousHunterDrone.Sound1 : AqueousHunterDrone.Sound2) with { Volume = 0.6f, Pitch = 0.3f, MaxInstances = 10 }, Projectile.Center);

                    for (int i = -9; i <= 9; i++)
                    {
                        int dustStyle = Effects.ArsenalEffects.ArsenalPlasmaDust;
                        Dust dust = Dust.NewDustPerfect(tailPlace + Main.rand.NextVector2Circular(5, 5), dustStyle);
                        dust.scale = Main.rand.NextFloat(0.7f, 1.3f);
                        dust.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi * (Math.Sign(i) == 1 ? 0 : 1)) * Main.rand.NextFloat(1, 5);
                        dust.noGravity = true;
                        dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                        dust.fadeIn = 0.1f;
                        if (i == -1)
                            i++;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        Particle spark = new CustomSpark(tailPlace, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 15, 0.4f, Effects.ArsenalEffects.ArsenalPlasmaColor, new Vector2(1, 1), true, true, 0, false, false);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }
                if (time > animationClickTime - 1)
                {
                    Projectile.velocity *= 0.97f;
                    tailRot = MathHelper.Lerp(tailRot, 0, 0.1f);
                }
                else
                {
                    Projectile.Center = Vector2.Lerp(spawnAnimStart, tailPlace, Utils.GetLerpValue(animationClickTime - 40, animationClickTime, time, true));
                }
            }
        }
        public void Passive()
        {
            float sine = (float)Math.Sin((time + Projectile.localAI[2] * 3) * 0.075f / MathHelper.Pi);
            float sine2 = (float)Math.Sin(((time + Projectile.localAI[2] * 3) * 0.075f / MathHelper.Pi) * 2);

            TailRotationBasedOnMovement();

            attackTimer = 8 * Projectile.localAI[2];

            if (interactionTimer == interactionPoint)
            {
                chillSpot = Owner.Center + Main.rand.NextVector2CircularEdge(190, 190);
            }
            Projectile.rotation = Projectile.rotation.AngleLerp(0, 0.04f);
            Vector2 destination = interactionTimer > interactionPoint ? (chillSpot + new Vector2(220 * sine, -90 + 130 * sine2)) : Owner.Center + new Vector2((-50 + -85 * Projectile.localAI[2]) * slidingDirection, -80 + 30 * sine);
            
            if (surpriseTimer == 0)
                Projectile.velocity = (destination - Projectile.Center) / (moveSpeed + (interactionTimer > interactionPoint ? 20 : 0) - 9 + 9 * sine2);
            else
                surpriseTimer--;
            if (interactionTimer > interactionPoint)
            {
                if (time % 25 == 0)
                {
                    Particle spark2 = new CustomSpark(Projectile.Center + Main.rand.NextVector2Circular(5, 5) - Vector2.UnitY * 20, -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(3.3f, 5.5f), "CalamityMod/Projectiles/Magic/MelterNote1", false, 32, Main.rand.NextFloat(1.2f, 1.3f), Color.Lerp(Color.Green, Color.Chartreuse, Main.rand.NextFloat(0, 0.7f)), new Vector2(1, 1), true, false, 0, false, false);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                if (time % (480 + Projectile.localAI[2] * 120) == 0 && Owner.miscCounter != 0)
                {
                    SoundEngine.PlaySound((Main.rand.NextBool() ? AqueousHunterDrone.Sound1 : AqueousHunterDrone.Sound2) with { Volume = 0.3f, Pitch = 0f + Projectile.localAI[2] * 0.1f * (Projectile.localAI[2] % 2 == 0 ? -1 : 1), MaxInstances = 10 }, Projectile.Center);
                }
            }
            if (Owner.Center.Distance(Projectile.Center) > 450 && interactionTimer > interactionPoint && Owner.velocity.Length() > 5)
            {
                CombatText.NewText(Projectile.Hitbox, Effects.ArsenalEffects.ArsenalPlasmaColor, "!", true);
                SoundEngine.PlaySound(AqueousHunterDrone.Surprise with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
                Projectile.velocity = Projectile.Center.DirectionTo(Owner.Center);
                surpriseTimer = 25;
                interactionTimer = 0;
            }
            if (Projectile.velocity.Length() < 5)
                interactionTimer++;
            else if (interactionTimer > 0 && interactionTimer < interactionPoint)
                interactionTimer -= 2;
        }
        public void Attacking()
        {
            interactionTimer = 0;
            facingDir = Math.Sign(Projectile.Center.DirectionTo(lastTargetCenter).X);
            if (tailRecoil > 1 && doSpin) // Little spin
            {
                Projectile.rotation += 0.35f * facingDir;
            }
            else
                Projectile.rotation = (Projectile.rotation).AngleLerp(tailPlace.DirectionTo(lastTargetCenter).ToRotation() + (facingDir == -1 ? MathHelper.Pi : 0) + (MathHelper.PiOver4 * -facingDir), 0.18f);
            if (Utils.Distance(Projectile.Center, lastTargetCenter) > 250 && Projectile.velocity.Length() < 15)
                Projectile.velocity += tailPlace.DirectionTo(lastTargetCenter) * 0.3f;
            else
                Projectile.velocity *= 0.96f;
            if (lastTargetCenter.Y - 80 < tailPlace.Y)
                Projectile.velocity.Y -= 0.7f;

            Projectile.velocity *= 0.99f;
            if (attackDowntime > 35) // Reposition to somewhere on the other side of the target
            {
                Projectile.velocity.X += 0.8f * Math.Sign(lastTargetCenter.X - lastFirePositionX);
                TailRotationBasedOnMovement();
            }
            else
            {
                CheckForSpecialAttack();
                tailRot = MathHelper.Lerp(tailRot, 0, 0.1f);
            }
            if (attackTimer > 60) // Fire the missiles
            {
                Vector2 shootVel = tailPlace.DirectionTo(lastTargetCenter);
                Vector2 shootPlace = tailPlace + shootVel * 10;

                lastFirePositionX = Projectile.Center.X;
                SoundEngine.PlaySound(AqueousHunterDrone.Fire with { Volume = 0.7f, Pitch = 0.0f, PitchVariance = 0.3f, MaxInstances = 10 }, Projectile.Center);

                for (int i = 0; i < 13; i++)
                {
                    bool noFall = !Main.rand.NextBool(5);
                    float variance = Main.rand.NextFloat(-0.7f, 0.7f);
                    int dustStyle = noFall ? ModContent.DustType<SquashDust>() : Effects.ArsenalEffects.ArsenalPlasmaDust;
                    Dust dust = Dust.NewDustPerfect(tailPlace, dustStyle);
                    dust.scale = Main.rand.NextFloat(1.4f, 1.8f) - Math.Abs(variance);
                    dust.velocity = shootVel.RotatedBy(variance) * Main.rand.NextFloat(9, 9.5f) * (1 - Math.Abs(variance));
                    dust.noGravity = noFall;
                    dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                    dust.fadeIn = noFall ? 0 : 1.1f;
                }

                Particle bolt2 = new CustomPulse(shootPlace, shootVel * 0.2f, Effects.ArsenalEffects.ArsenalPlasmaColor, "CalamityMod/Particles/BloomRing", new Vector2(0.5f, 1f), shootVel.ToRotation(), 0f, 0.6f, 23);
                GeneralParticleHandler.SpawnParticle(bolt2);
                for (int i = 0; i < 2; i++)
                {
                    Particle spark = new CustomSpark(shootPlace, shootVel * 0.2f, "CalamityMod/Particles/BloomCircle", false, 28, 0.6f, Effects.ArsenalEffects.ArsenalPlasmaColor, new Vector2(1f, 0.5f), true, true, 0, false, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                for (int i = -1; i <= 1; i++)
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), shootPlace, shootVel.RotatedBy(0.5f * i) * 8, ModContent.ProjectileType<ShrimpPlasmaMissile>(), Projectile.damage, 0, Projectile.owner, 0, 0, 0);

                doSpin = Main.rand.NextBool(5);
                if (doSpin)
                    SoundEngine.PlaySound((Main.rand.NextBool() ? AqueousHunterDrone.Sound1 : AqueousHunterDrone.Sound2) with { Volume = 0.4f, Pitch = 0.2f, MaxInstances = 10 }, Projectile.Center);
                tailRecoil = 25f;
                Projectile.velocity += shootVel * (doSpin ? -18 : -9);
                attackTimer = 0;
                attackDowntime = 60;
            }
            attackTimer++;
        }
        public void TailRotationBasedOnMovement()
        {
            float sine = (float)Math.Sin(time * 0.1f / MathHelper.Pi);
            tailRot = (sine * 0.2f) + 1.2f * (float)Math.Pow(Utils.GetLerpValue(0, 12 * facingDir, Projectile.velocity.X, true), 1.5f) * facingDir;
        }
        public void SetVisualPlacement()
        {
            float sine = (float)Math.Sin(time * 0.35f / MathHelper.Pi);
            float sine2 = (float)Math.Sin(time * 0.05f / MathHelper.Pi);
            Vector2 shake = new Vector2(3 * sine * (facingDir), 7 * sine2);
            Vector2 visualPlace = Projectile.Center + shake + Vector2.UnitY * (surpriseTimer > 20 ? Utils.GetLerpValue(25, 20, surpriseTimer) :  1 - (float)Math.Pow(Utils.GetLerpValue(20, 15, surpriseTimer, true), 2)) * -25;
            Vector2 tailDist = new Vector2((30 - Math.Abs(tailRot) * 15) * (-facingDir), 7 - Math.Abs(tailRot) * 3).RotatedBy(Projectile.rotation);
            Vector2 headDist = new Vector2(0, 0);
            Vector2 recoilPlace = ((Projectile.rotation - (facingDir == -1 ? MathHelper.Pi : 0) - (MathHelper.PiOver4 * -facingDir)).ToRotationVector2() * -tailRecoil);
            headPlace = visualPlace + headDist;
            tailPlace = visualPlace + tailDist + recoilPlace;
        }
        public void GrantBuffs(Player player)
        {
            bool isCorrectProjectile = Projectile.type == ModContent.ProjectileType<AqueousHunterDroneSummon>();
            player.AddBuff(ModContent.BuffType<AqueousHunterDroneBuff>(), 3600);
            if (isCorrectProjectile)
            {
                if (player.dead)
                {
                    player.Calamity().aqueousHunterDrone = false;
                }
                if (player.Calamity().aqueousHunterDrone)
                {
                    Projectile.timeLeft = 2;
                }
            }
        }
        public override bool MinionContactDamage() => false;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D head = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/ShrimpBody").Value;
            Texture2D glow = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/ShrimpBodyGlow").Value;
            Texture2D tail = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/ShrimpTail").Value;
            Texture2D glow2 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/ShrimpTailGlow").Value;
            Texture2D spin = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearSmokey").Value;
            Color drawColor = Projectile.GetAlpha(lightColor);
            bool left = facingDir == -1;
            float rotation = Projectile.rotation;

            // Tail
            Main.EntitySpriteDraw(tail, tailPlace - Main.screenPosition, null, drawColor, rotation + tailRot, new Vector2(left ? tail.Height : 0, 0), Projectile.scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            Main.EntitySpriteDraw(glow2, tailPlace - Main.screenPosition, null, Color.White, rotation + tailRot, new Vector2(left ? glow2.Height : 0, 0), Projectile.scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            float sine = (float)Math.Sin(time * 0.35f / MathHelper.Pi);
            float headFadeIn = (float)Math.Pow(Utils.GetLerpValue(0, animationClickTime - 25, time, true), 8);
            // Head
            Main.EntitySpriteDraw(head, headPlace - Main.screenPosition, null, drawColor * headFadeIn, rotation + sine * 0.1f, head.Size() * 0.5f, Projectile.scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            Main.EntitySpriteDraw(glow, headPlace - Main.screenPosition, null, Color.White * headFadeIn, rotation + sine * 0.1f, glow.Size() * 0.5f, Projectile.scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            // Spin effect
            Main.EntitySpriteDraw(spin, Vector2.Lerp(headPlace, tailPlace, 0f) - Main.screenPosition, null, Effects.ArsenalEffects.ArsenalPlasmaColor with { A = 0 } * specialAttackfx, rotation * 1.8f, spin.Size() * 0.5f, Projectile.scale * 0.53f * specialAttackfx, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            if (tailRecoil > 0) // Glowing on the gun after firing
            {
                float fade = Utils.GetLerpValue(0, 15, tailRecoil, true);
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * (2 + 2 * fade);
                    Main.EntitySpriteDraw(glow2, tailPlace - Main.screenPosition + vel, null, Color.White with { A = 0 } * fade, rotation + tailRot, new Vector2(left ? glow2.Height : 0, 0), Projectile.scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                }
            }
            return false;
        }
    }
}
