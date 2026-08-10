using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class StarfleetHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<Starfleet>();
        public override Vector2 GunTipPosition => base.GunTipPosition + Projectile.velocity.RotatedBy(MathHelper.PiOver2 * Projectile.direction) * -5;
        public override float RecoilResolveSpeed => 0.1f;
        public override float MaxOffsetLengthFromArm => 25f;
        public override float OffsetXUpwards => -12f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 10f;
        public override float WeaponTurnSpeed => (0.6f);

        public int time = 0;
        public int lastUseTime = 0;
        public static int perfectLeniancy = 3;
        public static int goodLeniancy = perfectLeniancy + 6;
        public static int starburstPerfectTime = 23;
        public ref float shootingCooldown => ref Projectile.ai[0];
        public ref float starburstTimer => ref Projectile.ai[1];
        public int extendedCooldown => (int)(lastUseTime * 1.2f);
        public int naildriverCooldown => (int)(lastUseTime * 1.5f);
        
        public float recoilIntensity = 0;
        public int recoilTimerMax = 62;
        public Vector2 recoilDirection;
        public bool setVel = true;
        public float glowIntensity = 1;
        public Color c1 = new Color(146, 255, 211);
        public Color c2 = new Color(222, 225, 146);
        public Color c3 = new Color(255, 233, 146);
        public Color shiftColor;
        public Vector2 gunBackPosition;
        public ref float starburstCooldown => ref Projectile.ai[2];
        public bool naildriver => ((starburstTimer <= starburstPerfectTime + perfectLeniancy) && (starburstTimer >= starburstPerfectTime - perfectLeniancy)); // if within perfect frame window
        public bool scattershot => !naildriver && ((starburstTimer <= starburstPerfectTime + goodLeniancy) && (starburstTimer >= starburstPerfectTime - goodLeniancy)); // If within early or late frame window
        public override void KillHoldoutLogic() { }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void SendExtraAIHoldout(BinaryWriter writer)
        {
            writer.Write(lastUseTime);
            writer.Write(Projectile.spriteDirection);
        }

        public override void ReceiveExtraAIHoldout(BinaryReader reader)
        {
            lastUseTime = reader.ReadInt32();
            Projectile.spriteDirection = reader.ReadInt32();
        }
        public override void HoldoutAI()
        {
            float rate = (time * 0.05f);
            List<Color> eColors = new List<Color>()
                {
                    c1,
                    c2,
                    c3,
                };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            shiftColor = Color.Lerp(currentColor, nextColor, rate % 2f >= 1f ? 1f : rate % 1f);

            SetUsage = false;
            bool doingNothing = shootingCooldown == 0 && starburstCooldown == 0 && starburstTimer == 0;
            if (lastUseTime == 0 || doingNothing)
                lastUseTime = Owner.HeldItem.useAnimation;
            if (!doingNothing)
                Owner.itemTime = Owner.itemAnimation = 5;

            glowIntensity = MathHelper.Lerp(glowIntensity, (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax, 0, shootingCooldown, true), 5), 0.2f);
            
            if ((Owner.HeldItem.type != ItemType<Starfleet>() && doingNothing) || (doingNothing && (Main.mapFullscreen || Owner.mouseInterface)) || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            bool hasAmmo = Owner.PickAmmo(HeldItem, out _, out _, out _, out _, out _, true);
            bool leftShootChecks = Owner.whoAmI == Main.myPlayer && (Main.mouseLeft && !Main.mapFullscreen && !Owner.mouseInterface && shootingCooldown == 0) && hasAmmo;
            bool rightShootChecks = Owner.whoAmI == Main.myPlayer && (Owner.Calamity().mouseRight && !Main.mapFullscreen && !Owner.mouseInterface && starburstCooldown == 0 && starburstTimer == 0);

            if (Owner.whoAmI == Main.myPlayer && Main.mouseLeft && !hasAmmo && OffsetLengthFromArm >= 24.5f)
            {
                OffsetLengthFromArm -= 8;
                SoundStyle click = new("CalamityMod/Sounds/Item/DudFire");
                SoundEngine.PlaySound(click with { Volume = .6f, Pitch = -.2f }, Projectile.Center);
            }
            if (leftShootChecks)
                FireShotgun();
            if (rightShootChecks)
            {
                Projectile.ForceNetUpdate();
                SoundStyle test = new("CalamityMod/Sounds/Item/StarfleetStarburst");
                SoundEngine.PlaySound(test with { Volume = 1f, Pitch = 0f }, Projectile.Center);
                starburstTimer++;
            }
            if (starburstTimer > 0)
            {
                // Do wind up animation
                if (starburstTimer < starburstPerfectTime / 2)
                    OffsetLengthFromArm = 25 - 12 * (1 - (float)Math.Pow(Utils.GetLerpValue(starburstPerfectTime / 2 - 1, 0, starburstTimer, true), 5));
                else
                    OffsetLengthFromArm = 13 + 20 * ((float)Math.Pow(Utils.GetLerpValue(starburstPerfectTime / 2, starburstPerfectTime - 1, starburstTimer, true), 8));

                if (starburstTimer == starburstPerfectTime)
                    FireStarburst();
                
                starburstTimer++;
                if (starburstTimer > starburstPerfectTime + goodLeniancy + 1)
                    starburstTimer = 0;
            }
            if (shootingCooldown > 0)
                shootingCooldown--;
            if (starburstCooldown > 0)
                starburstCooldown--;
            if (recoilIntensity > 0 && (shootingCooldown > 0 || starburstCooldown > 0))
                ManageRecoil();
            time++;
        }
        public void ManageRecoil()
        {
            float slowdown = (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax / 2, recoilTimerMax, Math.Max(shootingCooldown, starburstCooldown), true), 4);
            Vector2 movement = recoilDirection * (recoilIntensity) * slowdown;
            bool enableRecoil = false;
            if (!enableRecoil || Collision.SolidCollision(Owner.Center + movement, (int)(Owner.width * 1.1f), (int)(Owner.height * 1.1f)) || !Owner.Calamity().mouseRight)
            {
                recoilIntensity = 0;
                return;
            }
            if (slowdown > 0.1f)
            {
                if (setVel)
                {
                    Owner.velocity = movement * 0.25f;
                    setVel = false;
                }
                Projectile.Center += movement;
                Owner.Center += movement;
            }
            else
                setVel = true;
        }
        public void FireShotgun()
        {
            Projectile.ForceNetUpdate();
            // 50% chance to not consume ammo
            Owner.PickAmmo(HeldItem, out _, out _, out _, out _, out _, Main.rand.NextBool());

            SoundStyle shotgunFire = new("CalamityMod/Sounds/Item/StarfleetFire");
            for (int i = 0; i < (naildriver ? 2 : 1); i++)
                SoundEngine.PlaySound(shotgunFire with { Volume = (naildriver && i == 0 ? 0.3f : 0.6f), Pitch = ((naildriver && i == 0) ? 0f : 0.2f), MaxInstances = 2 }, Projectile.Center);
            if (naildriver)
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/HellkiteFullCharge") with { Volume = 0.7f, Pitch = 1.3f, MaxInstances = 2 }, Projectile.Center);
            // Perfects have longer cooldown
            int cooldown = (naildriver ? naildriverCooldown : lastUseTime);
            recoilTimerMax = cooldown;
            shootingCooldown = cooldown;
            recoilDirection = -Projectile.velocity;
            Owner.SetScreenshake(naildriver ? 9 : scattershot ? 7 : 4);
            OffsetLengthFromArm = naildriver ? 0 : scattershot ? 7 : 15;

            if (Main.myPlayer == Projectile.owner)
            {
                int baseShotCount = 6;
                for (int i = 0; i < baseShotCount; i++)
                {
                    float randomVel = Main.rand.NextFloat(0.8f, 1f);
                    float damageMult = ((naildriver || scattershot) ? 1.75f : 1f) / baseShotCount;
                    float spread = (naildriver ? 0.06f : scattershot ? 0.9f : 0.25f);
                    int starExtraUpdates = naildriver ? 9 : scattershot ? 7 : 3;
                    Projectile shotgun = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, randomVel * Projectile.velocity.RotatedByRandom(spread) * 8, ModContent.ProjectileType<StarfleetStar>(), (int)(Projectile.damage * damageMult), Projectile.knockBack, Projectile.owner, 0, starExtraUpdates, Main.rand.Next(0, 300 + 1));
                    shotgun.extraUpdates = starExtraUpdates;
                }
            }

            for (int b = 0; b < 24; b++)
            {
                int parts2 = 4;
                for (int i = 0; i < parts2; i++)
                {
                    float power = Main.rand.NextFloat(0.2f, 1f);
                    Vector2 vel = (MathHelper.TwoPi * i / parts2).ToRotationVector2().RotatedBy(Projectile.rotation) * 12f;
                    float size = (0.8f) * Main.rand.NextFloat(0.9f, 1.1f) * (1.1f - power);
                    int dustStyle = DustType<SquashDust>();
                    Dust dust = Dust.NewDustPerfect(gunBackPosition, dustStyle);
                    dust.scale = size;
                    dust.velocity = vel * power * (0.7f);
                    dust.noGravity = true;
                    dust.color = GetRandomColor();
                    dust.fadeIn = naildriver ? -0.6f : 0f;

                    if (b == 0)
                    {
                        Particle aura = new CustomSpark(gunBackPosition, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, naildriver ? 35 : 20, 0.5f, shiftColor, new Vector2(0.65f, 1f), glowCenter: true, glowOpacity: 0.8f, glowCenterScale: 0.85f, extraRotation: Projectile.rotation + (i % 2 == 0 ? MathHelper.PiOver2 : 0), shrinkSpeed: 0.1f);
                        GeneralParticleHandler.SpawnParticle(aura);
                    }
                }
            }
            for (int i = 0; i < 25; i++)
            {
                float variance = Main.rand.NextFloat(-0.7f, 0.7f);
                int dustStyle = DustType<SquashDust>();
                Dust dust = Dust.NewDustPerfect(GunTipPosition, dustStyle);
                dust.scale = (Main.rand.NextFloat(1.4f, 1.8f) - Math.Abs(variance)) * 3f;
                dust.velocity = Projectile.velocity.RotatedBy(variance) * Main.rand.NextFloat(18f, 19f) * (float)Math.Pow(1 - Math.Abs(variance), 2);
                dust.noGravity = true;
                dust.color = GetRandomColor();
                dust.fadeIn = 4.75f;
            }

            // You can uncomment this to check your timing
            /*if (naildriver)
            Main.NewText("naildriver: " + (starburstPerfectTime - starburstTimer), Color.DarkOrchid);
            if (scattershot)
            Main.NewText("scattershot: " + (starburstPerfectTime - starburstTimer), Color.Lime);*/

            recoilIntensity = (naildriver ? 55f : scattershot ? 20f : 0);
        }
        public void FireStarburst()
        {
            Owner.SetScreenshake(7f);
            recoilDirection = -Projectile.velocity;
            if (recoilIntensity < 15)
                recoilIntensity = 15;
            if (recoilTimerMax < extendedCooldown)
                recoilTimerMax = extendedCooldown;
            if (starburstCooldown < extendedCooldown)
                starburstCooldown = extendedCooldown;

            if (Main.myPlayer == Projectile.owner)
            {
                float blastSize = 140;
                float minMultiplier = 0.1f;
                int hitsToMinMult = 6;
                Projectile blast = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), Projectile.damage * 3, -45, Owner.whoAmI, blastSize, minMultiplier, hitsToMinMult);
                blast.timeLeft = 15;
            }

            for (int i = 0; i < 14; i++)
            {
                float dist = Main.rand.NextFloat(0, 3);
                Particle forwardJet = new CustomSpark(GunTipPosition + Main.rand.NextVector2CircularEdge(dist * 5, dist * 5), Projectile.velocity * Main.rand.NextFloat(4, 5) * (6 - dist * 2), "CalamityMod/Particles/ForwardSmear", false, (int)(Main.rand.Next(9, 15 + 1) + (dist * 3)), Main.rand.NextFloat(0.1f, 0.2f), GetRandomColor(), new Vector2(1f, 1f), shrinkSpeed: 0.3f);
                GeneralParticleHandler.SpawnParticle(forwardJet);
            }
            for (int i = 0; i < 34; i++)
            {
                float rot = Main.rand.NextFloat(0.05f, 0.35f) * (Main.rand.NextBool() ? -1 : 1);
                Vector2 startVel = Projectile.velocity.RotatedBy(rot) * Main.rand.NextFloat(8, 18) * (Main.rand.NextBool(4) ? 2f : 1);
                Particle stars = new VelChangingSpark(GunTipPosition, startVel, startVel.RotatedBy(rot * 5), "CalamityMod/Particles/PulseStar", Main.rand.Next(25, 45 + 1), Main.rand.NextFloat(0.1f, 0.35f), GetRandomColor(), new Vector2(1f, 1f), shrinkSpeed: Main.rand.NextFloat(0.02f, 0.06f), lerpRate: 0.02f, glowCenter: true);
                GeneralParticleHandler.SpawnParticle(stars);
            }
            int parts = 60;
            for (int i = 0; i < parts; i++)
            {
                Vector2 intenededVel = (MathHelper.TwoPi * i / parts).ToRotationVector2() * 4f;
                Vector2 fxVel = new Vector2(intenededVel.X, intenededVel.Y * 2.3f).RotatedBy(Projectile.velocity.ToRotation());
                Vector2 fxVelEnd = new Vector2(intenededVel.X * 0.5f, intenededVel.Y * 6f).RotatedBy(Projectile.velocity.ToRotation());
                Vector2 fxPlace = GunTipPosition + fxVel.RotatedBy(Projectile.velocity.ToRotation());

                float size = Utils.GetLerpValue(0, -4, intenededVel.X, true);
                float width = Utils.GetLerpValue(0, 4 * Math.Sign(fxVel.X), fxVel.X, true);
                Color clr = (size <= 0.5f ? Color.Lerp(c3, c2, size * 2) : Color.Lerp(c2, c1, size * 2 - 1f));

                Particle aura = new CustomSpark(fxPlace, fxVel * 1.2f, "CalamityMod/Particles/BloomCircle", false, (int)(15 + size * 5), 0.35f + size * 0.2f, clr * 0.7f, new Vector2(1f + width * size, 1f), glowCenter: true, glowOpacity: size * 0.85f, glowCenterScale: 0.75f);
                GeneralParticleHandler.SpawnParticle(aura);

            }
        }
        public Color GetRandomColor()
        {
            Color useColor = Main.rand.Next(4) switch
            {
                0 => c1,
                1 => c2,
                _ => c3,
            };
            return useColor;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            gunBackPosition = Projectile.Center - Projectile.velocity * 22f + Projectile.velocity.RotatedBy(MathHelper.PiOver2 * Projectile.direction) * -2;
            
            if (time < 2)
                return false;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D glowTexture = Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/StarfleetGlow").Value;
            Texture2D orb = Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float glowMult = (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax / 2, recoilTimerMax, Math.Max(shootingCooldown, starburstCooldown), true), 4);
            int draws = 18;
            float sine = (float)Math.Sin(time * 0.02f);
            float attackMult = (float)Math.Pow(Utils.GetLerpValue(0, starburstPerfectTime - 1, starburstTimer, true), 2);
            float sine2 = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 55.5f / MathHelper.Pi);
            float fastSine = (float)Math.Sin(time * 0.2f);
            Color glowColor = shiftColor;

            if (starburstTimer > 0 && starburstCooldown == 0)
            {
                for (int i = 0; i < draws; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / draws).ToRotationVector2().RotatedBy(time * 2);
                    Main.EntitySpriteDraw(texture, drawPosition + drawOffset * 6 * attackMult, null, shiftColor with { A = 0 } * 0.7f * attackMult, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
                }
            }
            
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            for (int i = 0; i < draws; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / draws).ToRotationVector2().RotatedBy(time / 5) * (1.25f + (fastSine + 2f) * 0.2f + glowMult * 4);
                Main.EntitySpriteDraw(glowTexture, drawPosition + drawOffset, null, Color.Lerp(Color.Gray * 0.15f, glowColor with { A = 0 }, glowIntensity) * (0.1f + 0.5f * glowMult), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
                Main.EntitySpriteDraw(glowTexture, drawPosition, null, Color.Lerp(Color.Gray * 0.15f, Color.White with { A = 0 }, glowIntensity), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            }
            
            if (starburstTimer > 0 && starburstCooldown == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    // The shining orb at the tip
                    Color orbColor = shiftColor with { A = 0 } * 0.5f;
                    Vector2 scale = new Vector2(Math.Abs(sine2 * 0.5f) + 0.1f, 1) * (0.05f + i * 0.01f) * attackMult * Main.rand.NextFloat(0.9f, 1.1f) * 8.5f;
                    Main.EntitySpriteDraw(orb, GunTipPosition - Main.screenPosition, null, orbColor, Main.rand.NextFloat(-5, 5), orb.Size() * 0.5f, scale, SpriteEffects.None);
                }
            }

            return false;
        }
    }
}
