using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class LeviathanTooth : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public ref float time => ref Projectile.ai[0];

        // Vanilla sticky code is jank, So I did my own (for better or worse)
        public NPC chosenTarget;
        public bool stuckInTarget = false;
        public bool stuckInGround = false;
        public bool canDamage = true;
        public bool canStick = true;
        public Vector2 vibrate = Vector2.Zero;
        public int stuckTimer = 180;
        public Vector2 placementCenter;
        float placementDistance;
        Vector2 placementVelocity;
        public Vector2 storedVelocity;
        public bool collideWithTiles = true;
        public bool toothDirection = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.Lerp(Color.Red, Color.White, 0.85f)).ToVector3() * 0.5f * Utils.GetLerpValue(255, 0, Projectile.alpha, true));
            if (time == 0)
            {
                stuckTimer = Main.rand.Next(100, 120 + 1);
                if (Projectile.ai[1] < 4)
                    toothDirection = Main.rand.NextBool();
            }
            if (!stuckInTarget && !stuckInGround)
            {
                if (Projectile.velocity.Length() > 5 && time > 30 && Projectile.numHits == 0)
                    Projectile.velocity *= 0.97f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                if ((time % 2 == 0 || !canStick) && time > 5)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), !ChildSafety.Disabled ? DustID.Cloud : DustID.Blood, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.8f), 100, default, Main.rand.NextFloat(0.8f, 1.4f) * (canStick ? 1 : 1.3f));
                    dust.noGravity = true;
                    Particle blood = new CustomSpark(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(canStick ? 0.3f : 0.1f) * Main.rand.NextFloat(5f, 8f), "CalamityMod/Particles/LargeBloom",
                            false, 8, Main.rand.NextFloat(0.055f, 0.07f), (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.Lerp(Color.DarkRed, Color.Black, Main.rand.NextFloat(0, 0.5f))) * 0.7f, new Vector2(0.7f, 1f), false, shrinkSpeed: 0.8f);
                    GeneralParticleHandler.SpawnParticle(blood);
                }
                if (time > 90 && canStick)
                {
                    Projectile.velocity.Y += 0.03f;
                    if (Projectile.velocity.Y > 0)
                        Projectile.velocity.X *= 0.99f;
                }
            }
            else if (stuckInTarget)
            {
                float power = 5 * Utils.GetLerpValue(120, 0, stuckTimer, true);
                vibrate = Main.rand.NextVector2Circular(power, power);

                Projectile.rotation = (storedVelocity).SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver2;

                Vector2 impaleVel = (storedVelocity * 0.5f) * Utils.GetLerpValue(120, 0, stuckTimer, true);
                placementCenter = chosenTarget.Center + placementVelocity * placementDistance + impaleVel;

                Projectile.Center = placementCenter;

                stuckTimer--;
                if (chosenTarget.life <= 0 || chosenTarget == null)
                    stuckTimer = 0;
                if (stuckTimer == 0)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;

                    canDamage = true;
                    vibrate = Vector2.Zero;
                    canStick = false;
                    collideWithTiles = true;
                    Projectile.velocity = storedVelocity;
                    Projectile.extraUpdates = 4;
                    stuckInTarget = false;

                    for (int i = 0; i <= 7; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, !ChildSafety.Disabled ? DustID.Cloud : DustID.Blood, (storedVelocity * 2.5f).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.9f, 1.8f));
                        dust.noGravity = false;
                    }
                    for (int i = 0; i <= 3; i++)
                    {
                        Particle spark = new AltSparkParticle(Projectile.Center, (storedVelocity * 4.5f).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.8f) + new Vector2(0, -2), true, 20, 0.5f, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.DarkRed) * 0.7f);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    SoundStyle sound = new("CalamityMod/Sounds/NPCHit/PerfSmallHit", 3);
                    SoundEngine.PlaySound(sound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.2f, -0.3f) }, Projectile.Center);
                }
                    
            }
            if (stuckInGround)
            {
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.PiOver2;
            }

            if (collideWithTiles && Collision.SolidCollision(Projectile.Center, 4, 4)) // Vanilla tile collision messes with velocity so we use this instead
            {
                canDamage = false;
                Projectile.timeLeft = 180;
                stuckInGround = true;
                storedVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Zero;
                SoundStyle sound = new("CalamityMod/Sounds/NPCHit/RavagerRockPillarHit1");
                SoundEngine.PlaySound(sound with { Volume = 0.25f, Pitch = Main.rand.NextFloat(-0.3f, -0.6f) }, Projectile.Center);
                collideWithTiles = false;
            }

            Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 0, 60, 255, 0));
            time++;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= target == chosenTarget ? 1 : (canStick ? 0.2f : 0.5f);
        }
        public override bool? CanDamage() => canDamage ? null : false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target == chosenTarget)
                target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
            if (!stuckInTarget && canStick)
            {
                if (Projectile.timeLeft < 400)
                    Projectile.timeLeft = 400;
                collideWithTiles = false;
                canDamage = false;
                placementDistance = -Vector2.Distance(target.Center, Projectile.Center);
                placementVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                placementCenter = placementVelocity * (placementDistance * 0.01f);
                chosenTarget = target;
                stuckInTarget = true;
                storedVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8;
                Projectile.velocity = Vector2.Zero;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            if (Projectile.ai[1] == 2)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/LeviathanTooth2").Value;
            if (Projectile.ai[1] == 3)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/LeviathanTooth3").Value;
            if (Projectile.ai[1] == 4)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GreenWater").Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + vibrate, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, toothDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 18, targetHitbox);
    }
}
