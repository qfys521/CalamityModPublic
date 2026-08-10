using System;
using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class NidhoggHoldout : BaseGunHoldoutProjectile
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override int AssociatedItemID => ModContent.ItemType<Nidhogg>();

        public float offsetBase = 30;
        public override float MaxOffsetLengthFromArm => offsetBase;
        public override float RecoilResolveSpeed => (shootingTimer < 0) ? 0.02f : 0.1f;
        public override float OffsetXUpwards => 0f;
        public override float BaseOffsetY => 0f;
        public override float OffsetYDownwards => 0f;
        public override float WeaponTurnSpeed => charging ? 0.75f : 0.4f;
        public SlotId SoundSlot;
        public bool charging => Projectile.ai[2] == 5;
        public ref float shootingTimer => ref Projectile.ai[0];
        public ref float chargeTimer => ref Projectile.ai[1];
        public int fireRate => Owner.HeldItem.useAnimation;
        public int chargeRate => 222; // It's spesifically this to line up with the charge sound
        public int time = 0;
        public float lightSine = 0;
        float charge = 0; // multiplier for vfx based on charge of the weapon

        public override void KillHoldoutLogic()
        {
            if (Owner.dead)
            {
                StopSounds();
                Projectile.Kill();
            }
        }
        public void StopSounds()
        {
            if (SoundEngine.TryGetActiveSound(SoundSlot, out var audio2) && audio2.IsPlaying)
            {
                audio2?.Stop();
            }
        }
        public override void HoldoutAI()
        {
            if (time == 0)
            {
                OffsetLengthFromArm = offsetBase;
                if (!charging)
                    shootingTimer = fireRate;
            }

            if (shootingTimer < 0)
            {
                if (charge > 0)
                    charge = 1 - (float)Math.Pow(Utils.GetLerpValue(-50, -20, shootingTimer, true), 8);
                shootingTimer++;
                if (charge == 0) // Make a sound when the jaws close
                {
                    SoundStyle close = new("CalamityMod/Sounds/NPCHit/ExoHit2");
                    SoundEngine.PlaySound(close with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.3f, 0.4f) }, Projectile.Center);
                    charge -= 0.001f;
                }
                if (shootingTimer == 0)
                {
                    shootingTimer = fireRate;
                }
                
                return;
            }

            if (HeldItem.type != Owner.HeldItem.type)
            {
                StopSounds();
                Projectile.Kill();
                return;
            }
            if (!charging)
            {
                StopSounds();
            }
            if (shootingTimer == 0)
            {
                if (!charging || Owner.Calamity().arsenalCooldown > 0)
                {
                    if (Owner.Calamity().mouseRight)
                    {
                        Projectile.ai[2] = 5;
                        shootingTimer = 1; // This is just so you dont fire when you stop using it
                        return;
                    }
                    Shoot(false);
                }
                else // Charging mode
                {
                    Lighting.AddLight(Projectile.Center, Effects.ArsenalEffects.ArsenalGaussColor.ToVector3() * 2f * MathHelper.Lerp(Math.Abs(lightSine), 0.5f, 0.3f));

                    if (chargeTimer == 0)
                    {
                        SoundStyle sound = new("CalamityMod/Sounds/Item/NidhoggCharge");
                        SoundSlot = SoundEngine.PlaySound(sound with { Volume = 0.9f }, Projectile.Center);
                    }
                    if (chargeTimer == chargeRate) // Reached max charge
                    {
                        Shoot(true);
                    }
                    else
                        chargeTimer++;
                }
            }
            else
            {
                if (!Owner.Calamity().mouseRight && !Main.mouseLeft && shootingTimer > 0)
                {
                    StopSounds();
                    Projectile.Kill();
                }
            }

            if (shootingTimer > 0)
                shootingTimer--;

            time++;

            if (SoundEngine.TryGetActiveSound(SoundSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;
            if (shootingTimer >= 0)
                charge = (float)(Math.Pow(chargeTimer / chargeRate, 3));
        }
        public void Shoot(bool isBig)
        {
            if (isBig)
            {
                Owner.velocity += -Projectile.velocity * 12;
                OffsetLengthFromArm -= 15;
                StopSounds();
                Owner.Calamity().arsenalCooldown = 540;
                Owner.AddCooldown(ArsenalPower.ID, 540);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                if (Main.myPlayer == Projectile.owner)
                {
                    int chargeDamage = Projectile.damage * 33;
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity * 6, ModContent.ProjectileType<NidhoggRailgunBigShot>(), chargeDamage, 0, Projectile.owner);
                }

                SoundStyle sound = new("CalamityMod/Sounds/Item/NidhoggBigShot");
                for (int i = 0; i < 2; i++)
                    SoundEngine.PlaySound(sound with { Volume = 0.9f, MaxInstances = 2, Pitch = i == 0 ? -0.4f : 0 }, Projectile.Center);
                Owner.SetScreenshake(8f);

                for (int i = 0; i < 40; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, Main.rand.NextBool(3) ? ModContent.DustType<SquashDust>() : Effects.ArsenalEffects.ArsenalGaussDust, (shootVelocity * 25).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.3f, 0.9f), 0, default, Main.rand.NextFloat(1.2f, 1.9f));
                    dust.noGravity = true;
                    dust.color = Effects.ArsenalEffects.ArsenalGaussColor;
                }
                for (float i = 0.6f; i <= 1; i += 0.4f)
                {
                    Particle pulse3 = new CustomSpark(Projectile.Center, Projectile.velocity * (20 - 10 * i), "CalamityMod/Particles/GlowSquareFading", false, 19, 0.9f * i, Effects.ArsenalEffects.ArsenalGaussColor, new Vector2(1.2f, 0.7f), shrinkSpeed: -0.1f, extraRotation: MathHelper.Pi);
                    GeneralParticleHandler.SpawnParticle(pulse3);
                }

                shootingTimer = (-50);
                chargeTimer = 0;
                Projectile.ai[2] = 0;
            }
            else
            {
                OffsetLengthFromArm -= 5;
                SoundStyle sound = new("CalamityMod/Sounds/Item/NidhoggFire");
                SoundEngine.PlaySound(sound with { Volume = 0.6f, Pitch = Main.rand.NextFloat(0.5f, 0.65f) }, Projectile.Center);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 6f;
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity, ModContent.ProjectileType<NidhoggRailgunBlast>(), Projectile.damage, 0, Projectile.owner);

                for (int i = 0; i < 9; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition - Projectile.velocity * 15, Main.rand.NextBool(3) ? ModContent.DustType<SquashDust>() : Effects.ArsenalEffects.ArsenalGaussDust, (shootVelocity * 4).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.7f, 1.1f));
                    dust.noGravity = true;
                    dust.color = Effects.ArsenalEffects.ArsenalGaussColor;
                }

                shootingTimer = fireRate;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(SoundSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 2)
                return false;

            Texture2D topJaw = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/NidhoggTop").Value;
            Texture2D bottomJaw = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/NidhoggBottom").Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/NidhoggGlow").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 jawVec = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2 * Projectile.direction);

            float randSize = Main.rand.NextFloat(0.9f, 1f);
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.direction == -1 ? MathHelper.Pi : 0f);
            SpriteEffects flipSprite = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D aimTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLineThick").Value;
            Texture2D ringTex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSquareFading").Value;
            Texture2D pointTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;
            Texture2D centerTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

            if (charging)
            {
                drawPosition += Projectile.velocity * Main.rand.NextFloat(-6 * charge, 6 * charge);

                for (int i = 1; i <= 5; i++) // Sightlines
                {
                    float sine = (float)Math.Sin(((charge * 60f + time * 0.15f) + MathHelper.ToRadians(216) * i) / MathHelper.Pi);
                    float angle = MathHelper.ToRadians(36) * sine * Math.Max(1 - charge * 1.06f, 0);
                    Color lineColor = Effects.ArsenalEffects.ArsenalGaussColor with { A = 0 } * Math.Abs(sine) * Math.Min(chargeTimer / (chargeRate * 0.3f), 1) * 0.2f;
                    Main.EntitySpriteDraw(aimTex, Projectile.Center - Main.screenPosition + Projectile.velocity * 10, null, lineColor, Projectile.rotation + angle - MathHelper.PiOver2, new Vector2(aimTex.Size().X * 0.5f, 0), new Vector2(0.2f - Math.Abs(sine) * 0.12f, 8) * 0.07f, SpriteEffects.FlipVertically, 0);
                }
                for (int i = 0; i < 3; i++)
                    Main.EntitySpriteDraw(centerTex, Projectile.Center - Main.screenPosition + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10, null, Color.Lerp(Effects.ArsenalEffects.ArsenalGaussColor, Color.White, i * 0.3f) with { A = 0 }, drawRotation, centerTex.Size() * 0.5f, new Vector2(2.1f, 0.9f) * ((0.04f + 0.1f * (float)Math.Pow(charge, 3)) - 0.02f * i), flipSprite);
            }
            // Oh boy I love perfectly placing multiple textures together!!!
            Vector2 topJawPlace = drawPosition - jawVec * (5 + 10 * (float)Math.Pow(charge, 3));
            Vector2 bottomJawPlace = drawPosition + jawVec * (4 + 10 * (float)Math.Pow(charge, 3)) + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 6;
            Vector2 glowPlace = topJawPlace + jawVec;
            for (int i = 0; i < 14; i++)
            {
                Color auraColor = Effects.ArsenalEffects.ArsenalGaussColor with { A = 0 } * 0.25f * charge;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 14f).ToRotationVector2() * 3 * charge;
                Main.EntitySpriteDraw(bottomJaw, bottomJawPlace + drawOffset, null, auraColor, drawRotation, bottomJaw.Size() * 0.5f, Projectile.scale, flipSprite);
                Main.EntitySpriteDraw(topJaw, topJawPlace + drawOffset, null, auraColor, drawRotation, topJaw.Size() * 0.5f, Projectile.scale, flipSprite);
            }

            Main.EntitySpriteDraw(bottomJaw, bottomJawPlace, null, drawColor, drawRotation, bottomJaw.Size() * 0.5f, Projectile.scale, flipSprite);
            Main.EntitySpriteDraw(topJaw, topJawPlace, null, drawColor, drawRotation, topJaw.Size() * 0.5f, Projectile.scale, flipSprite);
            
            Main.EntitySpriteDraw(glowTexture, glowPlace, null, Color.White, drawRotation, glowTexture.Size() * 0.5f, Projectile.scale, flipSprite);
            if (charging)
            {
                for (int i = 1; i <= 3; i++) // "Rings"
                {
                    lightSine = (float)Math.Sin(((charge * 100f + time * 0.15f) + MathHelper.ToRadians(360) * i) / MathHelper.Pi);
                    float angle = MathHelper.ToRadians(36) * lightSine * (1 - charge);
                    Vector2 posOffset = Projectile.velocity * 20 * lightSine;
                    Color lineColor = Effects.ArsenalEffects.ArsenalGaussColor with { A = 0 } * Math.Min(chargeTimer / (chargeRate * 0.3f), 1) * 0.7f;
                    Main.EntitySpriteDraw(ringTex, Projectile.Center - Main.screenPosition + posOffset, null, lineColor, Projectile.rotation + MathHelper.PiOver2, ringTex.Size() * 0.5f, new Vector2(1 + 0.5f * (float)Math.Pow(charge, 3), 1) * (0.12f + i * 0.025f), SpriteEffects.FlipVertically, 0);
                }
            }
            return false;
        }
    }
}
