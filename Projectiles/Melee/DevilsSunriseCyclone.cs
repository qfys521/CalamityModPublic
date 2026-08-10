using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    [PierceResistException]
    public class DevilsSunriseCyclone : ModProjectile, ILocalizedModType
    {
        public static readonly SoundStyle HitSound = new SoundStyle("CalamityMod/Sounds/Item/MantisSwipe", 2) with { Pitch = 1.25f, PitchVariance = 0.15f };
        public new string LocalizationCategory => "Projectiles.Melee";

        public ref float State => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];
        private int greenAndBlue = 100;
        private const int MaxHits = 10;
        private float ReturnVel = 5f;
        private const float MaxReturnVel = 30f;
        private NPC targetToSlice;
        private Vector2 sliceOffset;

        public override void SetDefaults()
        {
            // These shouldn't matter because it's a circle
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.scale = 1.75f;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 7;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Timer++;
            Projectile.rotation += 0.5f;

            Lighting.AddLight(Projectile.Center, 0.51f, 0.2f, 0.2f);
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, 0f, 0f, 100, new Color(255, greenAndBlue, greenAndBlue));
            Main.dust[dust].velocity *= 0.3f;
            Main.dust[dust].noGravity = true;

            // What is each State?
            // 0: Initial launch. Only AI needed here is just slowing down, as initial velocity is handled when the projectile is spawned.
            // 1: Returning to the player. Effectively functions like your run-of-the-mill boomerang return AI.
            // 2: Lodged in an enemy. This sticks the projectile near the enemy constantly, complete with visual sparks.
            switch (State)
            {
                case 0:
                    Projectile.velocity *= 0.965f;
                    if (Projectile.velocity.Length() < 0.05f)
                        Projectile.velocity = Vector2.Zero;

                    // After 1 second, start returning
                    if (Timer == 60f)
                    {
                        State = 1f;
                        Projectile.velocity = Vector2.Normalize(Owner.Center - Projectile.Center) * ReturnVel;
                    }  
                    break;
                case 1:
                    // Return velocity accelerates over time; the magnitude of acceleration and deceleration are equal
                    ReturnVel *= 1.035f;
                    if (ReturnVel > MaxReturnVel)
                        ReturnVel = MaxReturnVel;
                    Vector2 ownerDist = Owner.Center - Projectile.Center;
                    if (ownerDist.Length() > 3000f)
                        Projectile.Kill();
                    ownerDist = Vector2.Normalize(ownerDist) * ReturnVel;

                    // Return to me, my devilish sword
                    if (Projectile.velocity.X < ownerDist.X)
                        Projectile.velocity.X = ownerDist.X;
                    else if (Projectile.velocity.X > ownerDist.X)
                        Projectile.velocity.X = ownerDist.X;

                    if (Projectile.velocity.Y < ownerDist.Y)
                        Projectile.velocity.Y = ownerDist.Y;
                    else if (Projectile.velocity.Y > ownerDist.Y)
                        Projectile.velocity.Y = ownerDist.Y;

                    // Die once you reach the player
                    if (Main.myPlayer == Projectile.owner)
                    {
                        if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                            Projectile.Kill();
                    }
                    break;
                case 2:
                    Projectile.velocity = Vector2.Zero;
                    Projectile.Center = targetToSlice.Center + sliceOffset + targetToSlice.velocity;

                    if (Timer % 2 == 0)
                    {
                        if (Timer % 4 == 0)
                        {
                            float angle = (Projectile.Center - targetToSlice.Center).ToRotation() + MathHelper.PiOver2;
                            CustomSpark impact = new(targetToSlice.Center, Vector2.Zero, "CalamityMod/Particles/ThinEndedLine", false, 10, Main.rand.NextFloat(0.6f, 0.8f), Color.Red, new Vector2(0.5f, 1f), extraRotation: angle);
                            GeneralParticleHandler.SpawnParticle(impact);
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Vector2 sparkVel = Vector2.Normalize(Projectile.Center - targetToSlice.Center).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(14f, 18f);
                                AltLineParticle spark = new(targetToSlice.Center, sparkVel, false, 12, 0.75f, Color.OrangeRed);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }
                        }
                    }
                    if (!targetToSlice.CanBeChasedBy(Projectile))
                    {
                        State = 1f;
                        Projectile.velocity = Vector2.Normalize(Owner.Center - Projectile.Center) * ReturnVel;
                    }
                    break;
                default:
                    break;
            }
        }

        public override Color? GetAlpha(Color lightColor) => null;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = State == 2f ? 60f : 45f;
            return CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox);
        }
        public override bool? CanHitNPC(NPC target) => Projectile.numHits >= MaxHits ? false : null;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(HitSound, target.Center);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
            // Heals on each hit
            Main.player[Projectile.owner].DoLifestealDirect(target, 1, 0.75f);

            // Start slicing the hit enemy if not doing so already
            if (State != 2f)
            {
                State = 2f;
                targetToSlice = target;
                sliceOffset = Projectile.Center - target.Center;
            }
            // Return if you've reached the max hits
            if (Projectile.numHits >= MaxHits - 1)
            {
                State = 1f;
                Projectile.velocity = Vector2.Normalize(Main.player[Projectile.owner].Center - Projectile.Center) * ReturnVel;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item88, Projectile.position);
            int dustAmt = 24;
            for (int i = 0; i < dustAmt; i++)
            {
                Vector2 dustVel = Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / dustAmt) * Main.rand.NextFloat(8f, 12f);
                Dust cycloneDust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, dustVel, 100, new Color(255, greenAndBlue, greenAndBlue));
                cycloneDust.noGravity = true;
                cycloneDust.noLight = true;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D cyclone = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects sp = SpriteEffects.None;

            Main.EntitySpriteDraw(cyclone, drawPos, null, new Color(255, 50, 50), Projectile.rotation, cyclone.Size() / 2f, Projectile.scale, sp);
            Main.EntitySpriteDraw(cyclone, drawPos, null, new Color(255, 75, 75) * 0.7f, -Projectile.rotation, cyclone.Size() / 2f, Projectile.scale * 1.5f, sp);
            Main.EntitySpriteDraw(cyclone, drawPos, null, new Color(255, greenAndBlue, greenAndBlue) * 0.4f, Projectile.rotation * 0.75f, cyclone.Size() / 2f, Projectile.scale * 2f, sp);

            // Extra slash effect that flickers around everywhere
            Texture2D flashySlash = ModContent.Request<Texture2D>("CalamityMod/Particles/SlashSmear").Value;
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Main.EntitySpriteDraw(flashySlash, drawPos + Main.rand.NextVector2Circular(30f, 30f), null, new Color(255, greenAndBlue, greenAndBlue), Main.rand.NextFloat(MathHelper.TwoPi), flashySlash.Size() / 2f, 0.275f, sp);
            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return false;
        }
    }
}
