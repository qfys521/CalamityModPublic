using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class FlashBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public int dir = 0;
        bool setAltPlace = false;
        public Vector2 altPlace;
        public Player Owner => Main.player[Projectile.owner];
        public bool invalidTarget => (Projectile.ai[0] < 0f || Projectile.ai[0] > 199f);
        public bool simplify => Projectile.ai[1] > 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.penetrate = 1; // Survives through its first hit by "cheating" and incrementing its own pierce counter
            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 2;
        }
        public override void AI()
        {
            // If the target is dead, remove them as a target and hit anything
            NPC target = (invalidTarget ? null : Main.npc[(int)Projectile.ai[0]]);
            if (target == null || !target.active || target.life <= 0)
                Projectile.ai[0] = -1;

            // Visibility/Sound toggle on acc visibility
            bool visible = Owner.Calamity().arcFlashRingVisual;
            if (time == 0 && Projectile.ai[0] != -1 && !simplify)
            {
                if (visible)
                {
                    SoundStyle fire = new("CalamityMod/Sounds/Item/ArcFlash");
                    SoundEngine.PlaySound(fire with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Owner.Center);
                }
            }

            time++;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (!setAltPlace)
            {
                NPC target = (invalidTarget ? null : Main.npc[(int)Projectile.ai[0]]);
                if (target == null || !target.active || target.life <= 0)
                    Projectile.ai[0] = -1;
                if (target != null && Projectile.ai[0] != -1)
                {
                    float size = (Math.Min(target.width, target.height) * 0.35f) + 10;
                    altPlace = Projectile.Center + Main.rand.NextVector2Circular(size, size);
                }
                setAltPlace = true;
            }

            if (simplify || !Owner.Calamity().arcFlashRingVisual)
                return false;

            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

            if (dir == 0)
                dir = Main.rand.NextBool() ? -1 : 1;
                
            int distance = (int)(600 + (altPlace.Y - Owner.Center.Y));
            int travelAmount = 10;
            Vector2 currentPos = altPlace;
            Vector2 lastPos = currentPos;
            bool zap = false; // Swapping the direction of the bolt
            Color drawColor = Color.White;
            float lifeLerp = (Projectile.numHits == 0 ? 1 : (float)Math.Pow(Utils.GetLerpValue(-5, 15, Projectile.timeLeft, true), 3));
            int angleSwapTimer = 0;
            int angleSwapMax = 7;
            for (int i = 0; i < distance; i += travelAmount)
            {
                float angleLerp = Math.Min(Utils.GetLerpValue(angleSwapMax, 0, angleSwapTimer, true), Utils.GetLerpValue(0, angleSwapMax, angleSwapTimer, true));
                float endLerp = Utils.GetLerpValue(0, distance, i, true);
                drawColor = Color.Lerp(Color.Cyan, Color.Orchid, endLerp);
                Main.EntitySpriteDraw(tex, currentPos - Main.screenPosition, null, drawColor with { A = 0 }, lastPos.DirectionTo(currentPos).ToRotation() + MathHelper.PiOver2, new Vector2(tex.Width / 2, tex.Height / 2), new Vector2((2.5f - 1.5f * angleLerp) * lifeLerp * Math.Max(endLerp, 0.25f), 1 + 2f * angleLerp) * 0.2f, SpriteEffects.None);
                lastPos = currentPos;
                currentPos -= new Vector2(travelAmount * 1.5f * endLerp * dir * (zap ? -1 : 1) * lifeLerp, travelAmount);
                angleSwapTimer++;
                if (angleSwapTimer >= angleSwapMax) { zap = !zap; angleSwapTimer = 0; }
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 180);
            if (!simplify)
            {
                // Stops the projectile from deleting itself, while maintaining the masquerade 
                Projectile.penetrate++;

                Projectile.timeLeft = 15;
                if (Projectile.numHits == 0)
                {
                    if (!setAltPlace)
                    {
                        if (target != null && Projectile.ai[0] != -1)
                        {
                            float size = (Math.Min(target.width, target.height) * 0.35f) + 10;
                            altPlace = Projectile.Center + Main.rand.NextVector2Circular(size, size);
                        }
                        setAltPlace = true;
                    }
                    Vector2 pos = altPlace;
                    if (Owner.Calamity().arcFlashRingVisual)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<SquashDust>(), new Vector2(18, 18).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), 0, default, Main.rand.NextFloat(1.3f, 1.5f) * 3);
                            dust.noGravity = true;
                            dust.color = Main.rand.NextBool(3) ? Color.Cyan : Color.Orchid;
                            dust.fadeIn = 7.5f;
                        }
                        Particle orb = new CustomPulse(pos, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.68f, 0.5f, 14);
                        GeneralParticleHandler.SpawnParticle(orb);
                        Particle orb2 = new CustomPulse(pos, Vector2.Zero, Color.White * 0.8f, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.225f, 0.2f, 14);
                        GeneralParticleHandler.SpawnParticle(orb2);
                    }
                    Particle pulse2 = new CustomPulse(pos, Vector2.Zero, Color.Cyan * (Owner.Calamity().arcFlashRingVisual ? 1 : 0.6f), "CalamityMod/Particles/BloomRing", new Vector2(1, 1), 0, 0.3f, 0.95f, 10);
                    GeneralParticleHandler.SpawnParticle(pulse2);
                }
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (invalidTarget || Projectile.ai[0] == target.whoAmI)
                return null;
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.numHits > 0 ? false : base.Colliding(projHitbox, targetHitbox);
        public override bool? CanCutTiles() => false;
    }
}
