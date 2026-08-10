using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class DoGLaserWallsBigBeam : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Boss";
        public float time = 0;
        public ref float attackSpeed => ref Projectile.ai[0];
        public ref float laserType => ref Projectile.ai[2];
        public bool canDamage => doneAttack && laserFX >= 1f;
        public bool doneAttack = false;
        public int attackTime = 30;
        public float laserLength => 3000;
        public float laserFX = 0;
        public float storedTime = 0;
        public Color drawColor = Color.Magenta;
        public float sine = 0;
        public float laserRot = 0;
        Vector2 beamStart = Vector2.Zero;
        Vector2 directionToTarget = Vector2.Zero;
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
            Projectile.scale = 2;
        }
        public override void AI()
        {
            if (laserFX > 0)
                laserFX = MathHelper.Lerp(laserFX, 0, time > 15 ? 0.07f : 0.01f);
            sine = (float)Math.Sin(time * 4f / MathHelper.Pi);
            if (time == 0)
            {
                targeted = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (CalamityWorld.LegendaryMode)
                    Projectile.scale = 6;
                laserRot = Main.rand.NextFloat(-0.4f, 0.4f);

                beamStart = targetPos + ((laserType == 0 || laserType == 4 || laserType == 5) ? Vector2.UnitX : Vector2.UnitY).RotatedBy(laserRot) * laserLength;
                directionToTarget = beamStart.DirectionTo(targetPos);

                Projectile.Center += Main.rand.NextVector2CircularEdge(400, 400);
                // Some default values for if the projectile spawns without them set
                if (attackSpeed == 0)
                    attackSpeed = 0.5f;
                Projectile.velocity = Vector2.Zero;
                laserFX = 1f;
                Projectile.ForceNetUpdate();
            }
            if (time >= attackTime && !doneAttack)
            {
                Main.LocalPlayer.SetScreenshake(7f);
                SoundStyle attack = new("CalamityMod/Sounds/Custom/DoGLaserWallBigAttack2");
                for (int i = 0; i < 2; i++)
                    SoundEngine.PlaySound(attack with { Volume = 0.8f, Pitch = 0, MaxInstances = -1 }, targetPos);
                laserFX = 2.5f;
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
                drawColor = Color.Lerp(Color.Magenta, Color.Cyan, (float)Math.Pow(Utils.GetLerpValue(endTime, storedTime, time, true), 2));
            }
            time += attackSpeed;
        }
        public override bool CanHitPlayer(Player target)
        {
            if (canDamage)
                return true;
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
            else
            {
                float _ = float.NaN;
                Vector2 start = beamStart;
                Vector2 end = beamStart + directionToTarget * laserLength * 2;
                bool hitCheck = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30 * Projectile.scale, ref _);

                return hitCheck;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (laserFX == 0)
                return false;
            Texture2D beam = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLineThick").Value;
            Texture2D bBeam = ModContent.Request<Texture2D>("CalamityMod/Particles/LineThick").Value;
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            float opacity = (doneAttack ? 0.65f : 0.35f) * (float)Math.Pow(Math.Min(laserFX, 1), 2);
            Color beamColor = drawColor with { A = 0 };

            if (CalamityClientConfig.Instance.Photosensitivity)
                opacity = 0.2f;
            for (int t = 0; t < (!doneAttack ? 1 : 4 * Projectile.scale); t ++)
            {
                bool black = (t > 0 + (Projectile.scale - 1));
                if (black)
                    beam = bBeam;
                
                float beamThickness = 0.09f * (black ? (0.8f - 0.15f * t / Projectile.scale) : 1f) * (laserFX <= 1 ? (float)Math.Pow(Math.Min(laserFX, 1), 2) : laserFX) * Utils.Remap(sine, -1, 1, 0.8f, 1.1f);
                Main.EntitySpriteDraw(beam, beamStart - Main.screenPosition, null, (black ? Color.Black * opacity : beamColor * (1 - t * 0.3f) * opacity) * (black ? (0.2f + 0.15f * t / Projectile.scale) : 1), directionToTarget.ToRotation() + MathHelper.PiOver2, new Vector2(beam.Width / 2, beam.Height), new Vector2(beamThickness, laserLength / 975) * Projectile.scale, SpriteEffects.None);
            }
            //Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null, Color.Red with { A = 0 }, 0, bloom.Size() / 2, 0.3f, SpriteEffects.None);
            return false;
        }
    }
}
