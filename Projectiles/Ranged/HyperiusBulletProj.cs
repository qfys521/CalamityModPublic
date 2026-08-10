using System;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HyperiusBulletProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private Color currentColor = Color.Black;
        public float dustAngle = 0f;
        public bool growing = false;
        public bool dustWave = false;
        public float variance = 0.8f;
        public Vector2 lastPos;
        public int slowdownTime = 7;
        public bool tileTouched = false;
        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;
            Projectile.extraUpdates = 25;
            Projectile.tileCollide = false;
            AIType = ProjectileID.Bullet;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            float timeleftFade = (float)Math.Pow(Utils.GetLerpValue(0, slowdownTime * Projectile.extraUpdates, Projectile.timeLeft, true), 1);
            if (currentColor == Color.Black)
            {
                slowdownTime = Main.rand.Next(6, 9 + 1);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20;
                variance = Main.rand.NextFloat(0.7f, 1f);
                dustAngle = Main.rand.NextFloat(-0.43f, 0.43f);
                dustWave = Math.Sign(dustAngle) == 1;
                Projectile.scale = 1.5f;
                Projectile.velocity *= 0.3f;
                lastPos = Projectile.velocity;
                switch (Main.rand.Next(0, 4 +1))
                {
                    case 4: // Yellow shot
                        currentColor = Color.Yellow * 0.65f;
                        break;
                    case 3: // Magenta shot
                        currentColor = Color.Magenta * 0.65f;
                        break;
                    case 2: // Red shot
                        currentColor = Color.Red * 0.65f;
                        break;
                    case 1: // Blue shot
                        currentColor = Color.Cyan * 0.65f;
                        break;
                    default: // Green shot
                        currentColor = Color.Lime * 0.65f;
                        break;
                }
            }
            if (dustAngle <= -0.5f)
            {
                growing = true;
            }
            if (dustAngle >= 0.5f)
            {
                growing = false;
            }
            dustAngle += (growing ? 0.07f * variance : -0.07f * variance);

            if (Collision.SolidCollision(Projectile.Center, 4, 4) && !tileTouched && Projectile.numHits == 0)
            {
                tileTouched = true;
                OnHitEffects(null);
            }
            if (Projectile.numHits > 0 || tileTouched)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5 * Math.Max(timeleftFade, 0.01f);
                if (Projectile.timeLeft > slowdownTime * Projectile.extraUpdates)
                    Projectile.timeLeft = slowdownTime * Projectile.extraUpdates;
            }

            Projectile.ai[2]++;
            Vector2 orbPos = Projectile.Center + (Projectile.velocity.RotatedBy((dustWave ? 1 : -1) * dustAngle) * 4.5f - Projectile.velocity * 5);
            if (Projectile.ai[2] > 15 && targetDist < 1200f)
            {
                CustomSpark orb = new CustomSpark(orbPos - Utils.DirectionTo(lastPos, orbPos) * timeleftFade, Utils.DirectionTo(lastPos, orbPos) * 0.1f, "CalamityMod/Particles/BloomCircle", false, 5, (0.55f + MathF.Abs(dustAngle * 0.65f)) * 0.15f * timeleftFade, currentColor, new Vector2(1 - MathF.Abs(dustAngle * 0.2f), 2 - (1 - MathF.Abs(dustAngle))), true, true, 0, false, false, 0.8f - MathF.Abs(dustAngle * 0.6f), 0.7f, 0.8f);
                GeneralParticleHandler.SpawnParticle(orb);

                if (Main.rand.NextBool(8))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDust>(), -Projectile.velocity.RotatedByRandom(0.15f) * Main.rand.NextFloat(0.9f, 1.8f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.2f, 1.9f) * timeleftFade;
                    dust.color = currentColor;
                    dust.noLightEmittance = true;
                    dust.fadeIn = 1.4f;
                }
            }
            lastPos = orbPos;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return (Projectile.numHits > 0 ? false : null);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            CalamityGlobalNPC modNPC = target.Calamity();

            if (!modNPC.hyperiusMarked)
                modNPC.hyperiusMarked = true;
                
            Player Owner = Main.player[Projectile.owner];
            // Hits can crit and the collapse damage will take that into account
            bool crit = Main.rand.Next(0, 100 + 1) < Owner.GetTotalCritChance(Projectile.DamageType);
            modNPC.hyperiusDamage += Math.Max(Projectile.damage * (crit ? 2 : 1) - 1, 1);
            
            modifiers.DisableCrit();
            modifiers.SourceDamage *= 0;
            modifiers.FinalDamage.Flat = 0.1f;
            modifiers.HideCombatText();

            OnHitEffects(target);
        }
        private void OnHitEffects(NPC target)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ShadowboltWallHit") with { Volume = 0.25f, Pitch = Main.rand.NextFloat(0.6f, 1f), MaxInstances = -1 }, Projectile.Center);
                for (int b = 0; b < 2; b++)
                {
                    Vector2 velocity = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 7 * (tileTouched ? -1 : 1)).RotatedByRandom(0.5f);
                    Projectile split = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity * 0.7f, ModContent.ProjectileType<HyperiusSplit>(), (int)Math.Max(Projectile.originalDamage * 0.05f, 1), 0, Projectile.owner, 0f, 0f, Main.rand.Next(0, 4 + 1));
                    split.DamageType = Projectile.DamageType;
                }
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> orb = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark");
            int startTime = 9;
            int endTime = 35;
            float timeleftFade = (float)Math.Pow(Utils.GetLerpValue(0, slowdownTime * Projectile.extraUpdates, Projectile.timeLeft, true), 1);

            for (int i = 0; i < 4; i++)
            {
                Vector2 squash = new Vector2(Utils.Remap(Projectile.ai[2], startTime, endTime, 0.2f, 0.6f + i * 0.2f), Utils.Remap(Projectile.ai[2], startTime, endTime, 1f, 3f - i * 0.4f));
                Color orbColor = Color.Lerp(currentColor, Color.White, i * 0.2f) with { A = 0 } * 0.9f;
                Vector2 scale = Projectile.scale * timeleftFade * squash * (0.05f - i * 0.008f) * 0.3f;
                Main.EntitySpriteDraw(orb.Value, Projectile.Center - Main.screenPosition + Projectile.velocity * i * 1.3f, null, orbColor, Projectile.rotation, orb.Size() * 0.5f, scale, SpriteEffects.None);
            }
            return false;
        }
    }
}
