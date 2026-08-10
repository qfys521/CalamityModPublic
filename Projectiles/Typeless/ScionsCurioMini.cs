using System;
using System.Collections.Generic;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ScionsCurioMini : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public Color usedColor = Color.Chartreuse;
        public ref float time => ref Projectile.ai[0];
        public ref float attackTimer => ref Projectile.ai[1];
        public ref float idleTimer => ref Projectile.localAI[0];

        public float fxFade = 0; // The glow visuals multiplier
        public float followSpeed = 12; // The speed it follows you, lower is faster
        public float lerpDir = 0;
        public float facing = 0;
        public Vector2 savedPos;
        public bool sharingSwineSecrets => idleTimer >= idleMax;
        public int swineSecretTimer = 0;
        public int swineText = 1;
        public float actionSpeed = 1;
        public int chosenSecret = 0;
        public int idleMax = 10800; // 3 minutes
        Vector2 goalPosition;
        public List<int> listNumbers = new List<int>();
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 38;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Main.zenithWorld) idleMax = 301;
            float rate = Main.GlobalTimeWrappedHourly * 5;
            List<Color> eColors = new List<Color>()
            {
                Color.Chartreuse,
                Color.LimeGreen
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            usedColor = Color.Lerp(Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f), Color.White, 0.7f);

            float sine = (float)Math.Sin(time * 0.1f * actionSpeed / MathHelper.Pi);
            float sine2 = (float)Math.Sin(time * 0.15f * actionSpeed / MathHelper.Pi);

            Vector2 baseDestination = Owner.Center - Vector2.UnitY * (20 + 5 * sine2) - Vector2.UnitX * 30 * lerpDir;

            if (Owner.Calamity().scionsCurioGotHit)
            {
                if (attackTimer == 0)
                    savedPos = baseDestination;
                if (attackTimer == 30)
                {
                    float blastScale = 1.6f;
                    for (int g = 0; g < 17; g++)
                    {
                        int DustID = ModContent.DustType<SquashDust>();
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID);
                        dust.scale = Main.rand.NextFloat(1.1f, 1.35f) * blastScale;
                        dust.velocity = new Vector2(9, 9).RotatedByRandom(100) * blastScale * Main.rand.NextFloat(0.4f, 0.9f) + Vector2.UnitY * -10;
                        dust.noGravity = false;
                        dust.color = Main.rand.NextBool() ? Color.Green : Color.Chartreuse;
                        dust.fadeIn = Main.rand.NextFloat(0.2f, 2f);
                    }
                    Particle blastvfx = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Chartreuse * 0.9f, "CalamityMod/Particles/ShineExplosion1", Vector2.One, Main.rand.NextFloat(-10, 10), 0.05f * blastScale, 0.15f * blastScale, 10, true);
                    GeneralParticleHandler.SpawnParticle(blastvfx);
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f * blastScale, Pitch = Main.rand.NextFloat(0.5f, 0.7f), MaxInstances = 6 }, Projectile.Center);

                    // Create Blast
                    float blastSize = 115 * blastScale;
                    float minMultiplier = 0.5f;
                    int hitsToMinMult = 5;
                    int debuff = ModContent.BuffType<Irradiated>();
                    int debuffTime = 300;
                    Projectile blast = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), ScionsCurio.postHitDamage, 7, (Owner != null ? Owner.whoAmI : -1), blastSize, minMultiplier, hitsToMinMult);
                    blast.localAI[0] = debuff;
                    blast.localAI[1] = debuffTime;
                    blast.timeLeft = 15;
                    blast.DamageType = DamageClass.Ranged;
                    attackTimer = -1;
                    Owner.Calamity().scionsCurioGotHit = false;
                }
                baseDestination = savedPos + new Vector2(4, 4).RotatedBy(sine2 * 0.02f);
                attackTimer++;
            }

            facing = Math.Sign(Projectile.Center.X == Owner.Center.X ? Owner.direction : Projectile.Center.DirectionTo(Owner.Center).X);
            
            Vector2 speakingDestination = Owner.Center - Vector2.UnitY * (10 + 3 * sine2) + Vector2.UnitX * 45 * lerpDir;
            if (time == 0)
                goalPosition = baseDestination;
            goalPosition = Vector2.Lerp(goalPosition, swineSecretTimer > 0 ? speakingDestination : baseDestination, 0.08f);
            lerpDir = MathHelper.Lerp(Owner.direction, facing, 0.04f);

            Projectile.velocity = (goalPosition - Projectile.Center) / (followSpeed * (swineSecretTimer > 0 ? 0.5f : 1));

            Projectile.rotation = 0.2f * sine;

            if (Owner.Calamity().scionsCurioVisuals)
                Lighting.AddLight(Projectile.Center, usedColor.ToVector3() * 0.3f);

            // Swine Secrets
            int textSpeed = 4;
            if (swineSecretTimer > 0)
            {
                if (swineSecretTimer % textSpeed == 0 && swineSecretTimer > 30)
                {
                    int originalLength = CalamityUtils.GetTextValue(("Misc.SwineSecret" + chosenSecret.ToString())).Length + 1;
                    string text = CalamityUtils.GetTextValue(("Misc.SwineSecret" + chosenSecret.ToString()));
                    int charstoRemove = (text.Length - (int)swineText);
                    if (charstoRemove > -1)
                    { 
                        text = text.Remove(swineText, charstoRemove);
                        if (swineSecretTimer >= 2)
                            text = text.Remove(0, swineText - 1);
                    }
                    if (swineText <= originalLength)
                        swineText++;
                    float textSpacing = Utils.Remap(originalLength, 5, 190, 20, 9, true);
                    Vector2 position = Owner.Center - (Vector2.UnitX * originalLength * (textSpacing / 2)) + (Vector2.UnitX * originalLength * textSpacing / originalLength * swineText);
                    int letter = CombatText.NewText(new Rectangle((int)position.X, (int)position.Y, 1, 1), usedColor, text, false, false);
                    Main.combatText[letter].lifeTime = 100;
                }
                if (swineSecretTimer % 6 == 0 && swineSecretTimer > 30)
                {
                    SoundStyle snort = new("CalamityMod/Sounds/Item/Swine", 2);
                    SoundEngine.PlaySound(snort with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.35f, 0.55f) }, Projectile.Center);
                    actionSpeed = Main.rand.NextFloat(0.5f, 1.5f);
                }
                swineSecretTimer--;
                fxFade = MathHelper.Lerp(fxFade, 1, 0.08f);
                if (swineSecretTimer == 0)
                    time = 120;
            }
            else
            {
                swineText = 1;
                fxFade = MathHelper.Lerp(fxFade, 0, 0.08f);
            }

            if (idleTimer % 90 == 0 && swineSecretTimer == 0 && idleTimer > idleMax - 300)
            {
                CombatText.NewText(Projectile.Hitbox, usedColor, CalamityUtils.GetTextValue("Misc.SwineSecret0"), false, true);
                actionSpeed = 1.1f;
                SoundStyle snort = new("CalamityMod/Sounds/Item/Swine", 2);
                SoundEngine.PlaySound(snort with { Volume = 0.6f, Pitch = Main.rand.NextFloat(0.35f, 0.55f) }, Projectile.Center);
            }
            

            actionSpeed = MathHelper.Lerp(actionSpeed, 1, 0.08f);
            if (Owner.Calamity().scionsCurio)
                Projectile.timeLeft++;
            else
                Projectile.Kill();
            if (Owner.dead)
                Projectile.Kill();

            time++;
            if (sharingSwineSecrets && swineSecretTimer == 0)
            {
                int number = 0;
                for (int i = 0; i < 1; i++)
                {
                    int attemptNumber = Main.rand.Next(1, 40 + 1);
                    if (listNumbers.Contains(attemptNumber))
                    {
                        if (listNumbers.Count >= 40) // Once all options have been exhausted, reset
                        {
                            listNumbers.Clear();
                            listNumbers.Add(attemptNumber);
                            number = attemptNumber;
                        }
                        else
                            i--;
                    }
                    else
                    {
                        listNumbers.Add(attemptNumber);
                        number = attemptNumber;
                    }
                }
                chosenSecret = number;
                int originalLength = CalamityUtils.GetTextValue(("Misc.SwineSecret" + chosenSecret.ToString())).Length;
                swineSecretTimer = originalLength * textSpeed + 30;
                idleTimer = idleMax - 300 - swineSecretTimer;
            }
            if (Owner.velocity.Length() < 2 && Owner.Calamity().scionsCurioVisuals)
                idleTimer++;
            else
            {
                idleTimer = 0;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Texture2D cTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Color drawColor = usedColor * Utils.GetLerpValue(0, 100, time, true);
            Color bodyColor = lightColor;
            float drawMult = 1;
            float attackFade = (float)Math.Pow(1 + (float)Math.Pow(Utils.GetLerpValue(0, 30, attackTimer, true), 4), 3);

            Vector2 shake = Main.rand.NextVector2Circular((attackFade - 1) * 5, (attackFade - 1) * 5);

            for (int i = 0; i < 18; i++) // Backglow
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 18f).ToRotationVector2() * 2 * drawMult * attackFade;
                if (attackFade > 1 || Owner.Calamity().scionsCurioVisuals)
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset + shake, null, Color.Lerp(drawColor, Color.Chartreuse, attackFade - 1) with { A = 0 } * 0.2f * drawMult, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, facing == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            // Main body
            if (Owner.Calamity().scionsCurioVisuals)
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + shake, null, Color.Lerp(bodyColor, Color.Chartreuse with { A = 0 }, attackFade - 1), Projectile.rotation, tex.Size() * 0.5f, new Vector2(1f, 1f) * Projectile.scale, facing == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            // Eye Shine
            for (int j = -1; j <= 1; j += 2)
            {
                for (int i = -4; i <= 4; i++)
                {
                    Vector2 pos = (Vector2.UnitX * 3 * facing + Vector2.UnitY * -2).RotatedBy(Projectile.rotation);
                    float scale = 1;
                    if (i == 0) i++;
                    if (i > 0)
                    {
                        scale = 0.9f;
                        pos = (Vector2.UnitX * 10 * facing + Vector2.UnitY * -2).RotatedBy(Projectile.rotation);
                    }
                    Main.EntitySpriteDraw(cTexture, Projectile.Center - Main.screenPosition + pos + shake, null, Color.Lerp(Color.Red, Color.White, Math.Abs(i) * 0.05f) with { A = 0 } * 0.7f * fxFade, Projectile.rotation + (j == -1 ? MathHelper.PiOver2 : 0), cTexture.Size() * 0.5f, new Vector2(2f - 0.1f * Math.Abs(i), 1f + 0.4f * Math.Abs(i)) * Projectile.scale * 0.35f * scale * Main.rand.NextFloat(0.65f, 1.25f), SpriteEffects.None);
                }
            }
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
