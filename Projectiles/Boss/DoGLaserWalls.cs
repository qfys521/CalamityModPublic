using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class DoGLaserWalls : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Boss";
        public float time = 0;
        public ref float attackSpeed => ref Projectile.ai[0]; // How fast the attack goes, higher is faster
        public ref float laserDist => ref Projectile.ai[1]; // The spacing between lasers
        public ref float laserType => ref Projectile.ai[2]; // The style of laser wall, ranging from 0 - 5 (6 for GFB circular variant)
        public bool canDamage => doneAttack && laserFX >= 1f;
        public bool doneAttack = false;
        public int attackTime = 30;
        public int laserCount => (int)(6000 / Math.Max(laserDist, 1));
        public float laserLength => laserDist * laserCount / 2;
        public float laserFX = 0;
        public float storedTime = 0;
        public Color drawColor = Color.Cyan;
        public float sine = 0;
        public Vector2 storedTargetPos;
        public Player targeted;
        public Vector2 targetPos => (targeted == null ? Projectile.Center : targeted.Center);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6000;
        }
        public override void AI()
        {
            if (laserFX > 0)
                laserFX = MathHelper.Lerp(laserFX, 0, time > 15 ? 0.12f : 0.01f);
            sine = (float)Math.Sin(time * 4f / MathHelper.Pi);

            if (time == 0)
            {
                targeted = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                // Some default values for if the projectile spawns without them set
                if (attackSpeed == 0)
                    attackSpeed = 0.5f;
                if (laserDist == 0)
                    laserDist = 250;
                Projectile.velocity = Vector2.Zero;
                laserFX = 1f;

                SoundStyle appear = new("CalamityMod/Sounds/Custom/DoGLaserWallSpawn");
                SoundEngine.PlaySound(appear with { Volume = 1f, Pitch = 0f, MaxInstances = -1 }, targetPos);
                storedTargetPos = targetPos;
                Projectile.ForceNetUpdate();
            }
            if (time >= attackTime && !doneAttack)
            {
                SoundStyle attack = new("CalamityMod/Sounds/Custom/DoGLaserWallLightAttack");
                for (int i = 0; i < 2; i++)
                    SoundEngine.PlaySound(attack with { Volume = 0.9f, Pitch = 0, MaxInstances = -1 }, targetPos);
                laserFX = 3;
                doneAttack = true;
                storedTime = time;
                Projectile.ForceNetUpdate();
            }
            float endTime = storedTime + 10;
            if (time >= endTime && doneAttack)
            {
                Projectile.Kill();
                return;
            }
            else if (doneAttack)
            {
                drawColor = Color.Lerp(Color.Cyan, Color.Magenta, (float)Math.Pow(Utils.GetLerpValue(endTime, storedTime, time, true), 2));
            }
            time += attackSpeed;
        }
        public override bool CanHitPlayer(Player target)
        {
            if (canDamage)
            {
                if (laserType == 6)
                {
                    float distance = Vector2.Distance(Projectile.Center, target.Center);
                    float expand = MathF.Min(target.width, target.height);
                    if (distance < laserDist * 1.5f || distance > laserDist * 10f + 40f)
                        return false;
                    
                    if (distance % laserDist > (laserDist - Utils.Remap(distance, 240f, 1440f, 8f, 56f) - expand) || distance % laserDist < expand)
                        return true;
                }
                else
                    return true;
            }
            return false;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 60);
            base.OnHitPlayer(target, info);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!canDamage)
                return false;
            else if (laserType == 6) // Custom hit check through CanHitPlayer
                return true;
            else
            {
                float _ = float.NaN;
                bool hit = false;
                Vector2 X = Vector2.UnitX.RotatedBy((laserType == 1 || laserType == 3 || laserType == 5) ? MathHelper.PiOver4 : 0);
                Vector2 Y = Vector2.UnitY.RotatedBy((laserType == 1 || laserType == 3 || laserType == 5) ? MathHelper.PiOver4 : 0);
                for (int l = 0; l < 2; l++)
                {
                    bool horizontal = (l != 0);
                    bool crossLasers = ((laserType == 2 && !horizontal) || (laserType == 3 && !horizontal) || (laserType == 4 && horizontal) || (laserType == 5 && horizontal));
                    float length = laserLength * (horizontal ? 1f : 1f);
                    for (int i = 0; i < laserCount; i += (crossLasers ? 2 : 1))
                    {
                        Vector2 startPoint = Projectile.Center - (horizontal ? Y : X) * (length);
                        Vector2 laserPoint = startPoint + (horizontal ? X : Y) * (laserDist * i) - (horizontal ? X : Y) * (laserDist * laserCount / 2);
                        Vector2 rot = laserPoint.DirectionTo(storedTargetPos);

                        Vector2 start = laserPoint;
                        Vector2 end = laserPoint + (crossLasers ? rot : (horizontal ? Y : X)) * laserLength * 2;

                        bool hitCheck = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 10, ref _);
                        if (hitCheck)
                        {
                            hit = true;
                            i = laserCount;
                        }
                    }
                }
                return hit;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (laserFX == 0)
                return false;
            Texture2D beam = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLineThick").Value;
            Texture2D bBeam = ModContent.Request<Texture2D>("CalamityMod/Particles/LineThick").Value;
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            float opacity = (doneAttack ? 0.65f : 0.3f) * (float)Math.Pow(Math.Min(laserFX, 1), 2);
            if (CalamityClientConfig.Instance.Photosensitivity)
                opacity = 0.2f;

            Color beamColor = drawColor with { A = 0 };
            if (laserType == 6)
            {
                Texture2D ring = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRingThinLarge").Value;
                for (float scale = 0.25f * laserDist / 120f; scale < 1.25f; scale += 0.125f * laserDist / 120f)
                    Main.EntitySpriteDraw(ring, Projectile.Center - Main.screenPosition, null, beamColor * opacity, 0f, ring.Size() * 0.5f, scale, SpriteEffects.None);
                return false;
            }

            Vector2 X = Vector2.UnitX.RotatedBy((laserType == 1 || laserType == 3 || laserType == 5) ? MathHelper.PiOver4 : 0);
            Vector2 Y = Vector2.UnitY.RotatedBy((laserType == 1 || laserType == 3 || laserType == 5) ? MathHelper.PiOver4 : 0);
            for (int l = 0; l < 2; l++)
            {
                bool horizontal = (l != 0);
                float length = laserLength;
                bool crossLasers = ((laserType == 2 && !horizontal) || (laserType == 3 && !horizontal) || (laserType == 4 && horizontal) || (laserType == 5 && horizontal));
                for (int i = 0; i < laserCount; i += (crossLasers ? 2 : 1))
                {
                    Vector2 startPoint = Projectile.Center - (horizontal ? Y : X) * (length);
                    Vector2 laserPoint = startPoint + (horizontal ? X : Y) * (laserDist * i) - (horizontal ? X : Y) * (laserDist * laserCount / 2);

                    for (int t = 0; t < (!doneAttack ? 1 : 5); t++)
                    {
                        bool black = (t > 0);
                        Texture2D usedTex = (black ? bBeam : beam);

                        float beamThickness = 0.03f * (black ? (0.8f - 0.15f * t) : 1f) * (laserFX <= 1 ? (float)Math.Pow(Math.Min(laserFX, 1), 2) : laserFX) * Utils.Remap(sine, -1, 1, 0.8f, 1.1f);
                        float rot = (horizontal ? MathHelper.Pi : MathHelper.PiOver2) + ((laserType == 1 || laserType == 3 || laserType == 5) ? MathHelper.PiOver4 : 0);
                        if (crossLasers)
                            rot = laserPoint.DirectionTo(storedTargetPos).ToRotation() + (MathHelper.PiOver2);
                        Main.EntitySpriteDraw(usedTex, laserPoint - Main.screenPosition, null, (black ? Color.Black * opacity : beamColor * opacity) * (black ? (0.2f + 0.15f * t) : 1), rot, new Vector2(beam.Width / 2, beam.Height), new Vector2(beamThickness, length / 975) * Projectile.scale, SpriteEffects.None);
                    }
                }
            }
            return false;
        }
    }
}
