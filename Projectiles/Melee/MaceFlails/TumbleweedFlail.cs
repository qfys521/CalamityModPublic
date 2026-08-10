using CalamityMod.DataStructures;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Typeless;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.MaceFlails
{
    public class TumbleweedFlail : BaseMaceFlailProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Tumbleweed>();
        public override int SpinIFrames => 10;
        public override float SpinHitboxRadius => MathF.Max(64f, 224f * AuraScale);
        public override float SpinVerticalFactor => 1f; // The aura is fully circular, rather than elliptical
        public override float SpinVisualRadius => 45f;
        public override float LaunchSpeed => 24f;
        public override int LaunchLifespan => 20;
        public override float MaxDropRange => 640f;
        public override float MaxRetractSpeed => 28f;
        public override float RetractAcceleration => 4f;

        public static float MaxAuraTime = 60f;
        private bool hasDetached = false;
        private int detachDelay = 0;

        public ref float AuraScale => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 42;
            base.SetDefaults();
        }

        public override bool ExtraBehavior()
        {
            // Randomly play grass sounds idly if it is moving/spinning
            if ((CurrentFlailState == FlailState.Spinning || Projectile.velocity.Length() > 2f) && Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Grass with { PitchVariance = 1.2f }, Projectile.Center);
                Projectile.soundDelay = Main.rand.Next(12, 15);
            }

            if (CurrentFlailState != FlailState.Spinning || Main.rand.NextBool())
            {
                Dust sand = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Alpha: 100, Scale: Main.rand.NextFloat(0.6f, 1.2f));
                sand.noGravity = CurrentFlailState == FlailState.Dropping;
                sand.velocity = CurrentFlailState == FlailState.Spinning ? Main.rand.NextVector2Unit() * 1.5f : Projectile.velocity * 0.25f;
            }

            // Force the flail to stay in launch state for a few frames after detaching to prevent spam-click abuse at point-blank
            if (detachDelay > 0)
            {
                detachDelay--;
                CurrentFlailState = detachDelay == 0 ? FlailState.ForcedRetracting : FlailState.LaunchingForward;
            }

            if (CurrentFlailState == FlailState.Spinning)
            {
                Projectile.ownerHitCheck = false; // Dust aura can hit through walls
                AuraScale = MathHelper.Clamp(AuraScale + 1f / MaxAuraTime, 0f, 1f);

                if (AuraScale < 0.15f)
                    return true;

                // The dust machine
                for (int i = 0; i < (int)(10 * AuraScale); i++)
                {
                    Circle dustCircle = new Circle(Owner.MountedCenter, 240f * AuraScale);
                    Vector2 dustPos = dustCircle.RandomPointInCircle();
                    if ((dustPos - Owner.MountedCenter).Length() > 60f * AuraScale)
                    {
                        Dust sand = Dust.NewDustPerfect(dustPos, DustID.Sand);
                        sand.noGravity = true;
                        sand.fadeIn = Main.rand.NextFloat(0.4f, 1f);
                        sand.velocity = (dustCircle.Center - dustPos).SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver4) * Vector2.Distance(dustCircle.Center, dustPos) * 0.04f;
                    }
                }
            }
            else
                AuraScale = MathHelper.Clamp(AuraScale - 3f / MaxAuraTime, 0f, 1f);

            return true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (AuraScale > 0f)
            {
                Texture2D Aura = TextureAssets.Projectile[ModContent.ProjectileType<SandCloakVeil>()].Value;
                Vector2 position = Owner.MountedCenter - Main.screenPosition;
                Color drawColor = Projectile.GetAlpha(lightColor) * 0.05f * AuraScale;
                for (int i = 0; i < 20; i++)
                {
                    Main.EntitySpriteDraw(Aura, position, null, drawColor, Main.GlobalTimeWrappedHourly * 0.5f + (i * i * 0.03f), Aura.Size() * 0.5f, AuraScale - (i * 0.03f), SpriteEffects.None, 0);
                }
            }

            if (hasDetached)
            {
                DrawChain();
                return false;
            }
            else
                return base.PreDraw(player, ref lightColor);
        }

        public override bool? CanDamage()
        {
            if (hasDetached)
                return false;
            return base.CanDamage();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentFlailState == FlailState.Spinning)
                return;

            SoundEngine.PlaySound(SoundID.NPCDeath15, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust tumbleDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Alpha: 100, Scale: 1.2f);
                tumbleDust.velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    tumbleDust.scale = 0.5f;
                    tumbleDust.fadeIn = Main.rand.NextFloat(1f, 1.1f);
                }

                tumbleDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedBrown, Alpha: 100, Scale: 1.7f);
                tumbleDust.noGravity = true;
                tumbleDust.velocity *= 5f;

                tumbleDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedBrown, Alpha: 100, Scale: 1f);
                tumbleDust.velocity *= 2f;
            }
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<TumbleweedRolling>(), (int)(Projectile.damage * LaunchDamage), Projectile.knockBack, Projectile.owner, CurrentFlailState == FlailState.LaunchingForward ? 0f : 1f);

            hasDetached = true;
            if ((CurrentFlailState == FlailState.LaunchingForward || CurrentFlailState == FlailState.Ricochet) && StateTimer < 6)
                detachDelay = 6;
            else
                CurrentFlailState = FlailState.ForcedRetracting;
        }
    }
}
