using System;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HyperiusSplit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private Color currentColor = Color.Black;
        private int rotDirection = 1;
        private float rotIntensity;
        private bool rotPhase2 = false;
        public bool photosen = CalamityClientConfig.Instance.Photosensitivity;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 500;
            Projectile.extraUpdates = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            int trailLifetime = 8;
            if (currentColor == Color.Black)
            {
                Player Owner = Main.player[Projectile.owner];

                Projectile.scale = 0.015f;
                Projectile.alpha = 255;
                rotDirection = Main.rand.NextBool() ? 1 : -1;
                rotIntensity = Main.rand.NextFloat(0.3f, 1.1f);
                Projectile.timeLeft = Main.rand.Next(250, 300 + 1);
                switch (Projectile.ai[2])
                {
                    case 4: // Yellow shot
                        currentColor = Color.Yellow;
                        break;
                    case 3: // Magenta shot
                        currentColor = Color.Magenta;
                        break;
                    case 2: // Red shot
                        currentColor = Color.Red;
                        break;
                    case 1: // Blue shot
                        currentColor = Color.Cyan;
                        break;
                    default: // Green shot
                        currentColor = Color.Lime;
                        break;
                }
            }

            if (Projectile.localAI[0] > 4 && Projectile.localAI[0] % 2 == 0 && !photosen)
            {
                Particle trail = new CustomSpark(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 9, -Projectile.velocity * 0.01f, "CalamityMod/Particles/BloomCircle", false, trailLifetime, 0.15f, currentColor, new Vector2(0.8f, 1.3f), true, true, shrinkSpeed: 0.6f / rotIntensity, glowCenterScale: 0.8f, glowOpacity: 0.7f);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            if (Projectile.timeLeft == 180)
                rotPhase2 = true;

            if (rotPhase2)
            {
                rotIntensity *= 1.003f;
                Projectile.velocity *= 0.995f;
                Projectile.velocity = Projectile.velocity.RotatedBy(-0.04f * rotIntensity * rotDirection);
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(0.02f * rotIntensity * rotDirection);
            }
        }
        public override void OnKill(int timeLeft)
        {
            
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, currentColor with { A = 0 } * (photosen ? 0.4f : 1), Projectile.rotation, texture.Size() * 0.5f, new Vector2(0.9f, 1.5f) * Projectile.scale, SpriteEffects.None);
            return false;
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
        }
    }
}
