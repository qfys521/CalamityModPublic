using System;
using CalamityMod.Events;
using CalamityMod.Items.SummonItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class TerminusHoldout : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Terminus>();
        public SlotId ActivationSoundSlot;

        public Player Owner => Main.player[Projectile.owner];

        public ref float Time => ref Projectile.ai[0];

        public const int Lifetime = 300;

        public override string Texture => "CalamityMod/Items/SummonItems/Terminus";

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 80;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;
            if (Owner.CantUseHoldout())
            {
                // Reset the boss rush timer to what it would normally be if disabling is done prematurely.
                if (BossRushEvent.BossRushActive || BossRushEvent.StartTimer > 0)
                    BossRushEvent.SyncStartTimer(BossRushEvent.StartEffectTotalTime);
                if (SoundEngine.TryGetActiveSound(ActivationSoundSlot, out var t) && t.IsPlaying)
                    t.Stop();

                CreateMysticDeathDust();
                Projectile.Kill();
                return;
            }

            UpdatePlayerFields();

            // Turn off boss rush mode.
            if (BossRushEvent.BossRushActive || BossRushEvent.StartTimer > 0)
            {
                if (Time == 2f)
                    SoundEngine.PlaySound(BossRushEvent.TerminusDeactivationSound, Main.LocalPlayer.Center);

                float lifetime = Utils.GetLerpValue(0f, 30f, Time, true);
                if (Time % 5f == 4f)
                    BossRushEvent.SyncStartTimer((int)MathHelper.Lerp(0f, BossRushEvent.StartEffectTotalTime, 1f - lifetime));

                MoonlordDeathDrama.RequestLight(Utils.GetLerpValue(0f, 15f, Time, true), Main.LocalPlayer.Center);
                if (Time >= 45f)
                {
                    BossRushEvent.End();
                    Projectile.Kill();
                }

                return;
            }

            // Play the activation sound.
            if (Time == 2f)
                ActivationSoundSlot = SoundEngine.PlaySound(BossRushEvent.TerminusActivationSound, Main.LocalPlayer.Center);

            // Update the activation sound.
            if (SoundEngine.TryGetActiveSound(ActivationSoundSlot, out var t2) && t2.IsPlaying)
                t2.Position = Projectile.Center;

            if (Projectile.timeLeft == 1)
            {
                Projectile.Kill();
                CreateEffectsHandler();
                return;
            }

            if (Projectile.timeLeft >= 32)
                CreateIdleMagicDust();

            float currentShakePower = MathHelper.Lerp(0.2f, 8f, Utils.GetLerpValue(Lifetime * 0.725f, Lifetime, Time, true));
            currentShakePower *= 1f - Utils.GetLerpValue(1000f, 3100f, Main.LocalPlayer.Distance(Projectile.Center), true);
            Main.LocalPlayer.SetScreenshake(currentShakePower);
        }

        public void CreateEffectsHandler()
        {
            SoundEngine.PlaySound(BossRushEvent.StartBuildupSound, Main.LocalPlayer.Center);
            Main.LocalPlayer.SetScreenshake(16f);
            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BossRushEffectThing>(), 0, 0f, Projectile.owner);
        }

        public void CreateMysticDeathDust()
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 20; i++)
            {
                Dust paleMagic = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(22f, 28f), DustID.AncientLight);
                paleMagic.velocity = -Vector2.UnitY * Main.rand.NextFloat(1.8f, 3.2f);
                paleMagic.color = Color.White;
                paleMagic.scale = Main.rand.NextFloat(1.1f, 1.35f);
                paleMagic.fadeIn = 1.5f;
                paleMagic.noGravity = true;
            }
        }

        public void CreateIdleMagicDust()
        {
            if (Main.dedServ)
                return;

            int dustCount = (int)Math.Round(MathHelper.SmoothStep(1f, 5f, Time / Lifetime));
            float outwardness = MathHelper.SmoothStep(40f, 150f, Time / Lifetime);
            float dustScale = MathHelper.Lerp(1.15f, 1.725f, Time / Lifetime);
            float colorTime = Time / Lifetime * Main.rand.NextFloat(0.65f, 1f);

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2Unit() * outwardness * Main.rand.NextFloat(0.75f, 1.1f);
                Vector2 dustVelocity = (Projectile.Center - spawnPosition) * 0.085f + Owner.velocity;

                Dust paleMagic = Dust.NewDustPerfect(spawnPosition, DustID.PortalBoltTrail);
                paleMagic.velocity = dustVelocity;
                paleMagic.scale = dustScale * Main.rand.NextFloat(0.75f, 1.15f);
                paleMagic.color = Main.zenithWorld ? Color.Lerp(Color.LightCoral, Color.White, colorTime) : Color.Lerp(Color.Violet, Color.Goldenrod, colorTime);
                paleMagic.noGravity = true;
                paleMagic.noLight = true;
            }
        }

        public void UpdatePlayerFields()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.spriteDirection = Owner.direction;
                Projectile.localAI[0] = 1f;
            }
            Owner.itemRotation = 0f;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.ChangeDir(Projectile.spriteDirection);

            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true) + Vector2.UnitX * Projectile.spriteDirection * 26f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Main.zenithWorld ? ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/Terminus_GFB").Value : Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 baseDrawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Color baseColor = Color.Lerp(Projectile.GetAlpha(lightColor), Color.White, Utils.GetLerpValue(40f, 120f, Time, true));
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (Time >= 150f)
            {
                float outwardness = MathHelper.SmoothStep(0f, 12f, Utils.GetLerpValue(150f, Lifetime - 30f, Time, true));
                Color afterimageColor = Color.Lerp(baseColor, Color.PaleGoldenrod, Utils.GetLerpValue(150f, 195f, Time, true)) * 0.225f;
                afterimageColor.A = 0;

                for (int i = 0; i < 10; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 10f + Main.GlobalTimeWrappedHourly * 4.4f).ToRotationVector2() * outwardness;
                    Main.EntitySpriteDraw(texture, baseDrawPosition + drawOffset, null, afterimageColor, 0f, origin, Projectile.scale, direction, 0);
                }
            }

            Main.EntitySpriteDraw(texture, baseDrawPosition, null, baseColor, 0f, origin, Projectile.scale, direction, 0);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Main.zenithWorld)
                return;

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/TerminusGlow").Value;
            Vector2 baseDrawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (Time >= 75f)
            {
                float outwardness = MathHelper.SmoothStep(0f, 4f, Utils.GetLerpValue(75f, Lifetime - 30f, Time, true));

                for (int i = 0; i < 3; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 4.4f).ToRotationVector2() * outwardness;

                    Main.EntitySpriteDraw(texture, baseDrawPosition + drawOffset, null, new Color(56, 12, 115, 0), 0f, origin, Projectile.scale, direction, 0);
                }
            }

            Main.EntitySpriteDraw(texture, baseDrawPosition, null, Color.White, 0f, origin, Projectile.scale, direction, 0);
        }
    }
}
