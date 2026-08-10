using System.IO;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;

namespace CalamityMod.Projectiles.Ranged
{
    public class GodSlayerSlugProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private const int Lifetime = 600;
        private const int NoDrawFrames = 2;
        // 25 instead of 24 because it is decremented once immediately after turning blue
        private const int BlueNoCollideFrames = 25;
        private const int TurnBlueFrameDelay = 7;
        // Radius of the "circle of inaccuracy" surrounding the mouse. Blue bullets will aim at this circle.
        private const float MouseAimDeviation = 13f;
        private const int TextureHeight = 136;

        private bool BlueMode => Projectile.ai[0] != 0f;
        public override string Texture => "CalamityMod/Projectiles/Ranged/GodSlayerSlugPurple";
        private static Texture2D TextureBlue;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;

            if (!Main.dedServ)
                TextureBlue = Mod.Assets.Request<Texture2D>("Projectiles/Ranged/GodSlayerSlugBlue", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
            Projectile.alpha = 255;
            Projectile.MaxUpdates = 6;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = Lifetime;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.tileCollide);
        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.tileCollide = reader.ReadBoolean();

        public override void AI()
        {
            // Store the original velocity if it has yet to be initialized. This is needed for the warp.
            if (Projectile.localAI[0] == 0f)
                Projectile.localAI[0] = Projectile.velocity.Length();

            // Rapidly fade into visibility.
            if (Projectile.alpha > 0)
                Projectile.alpha -= 17;

            // Add light appropriate to the bullet's current state.
            if (BlueMode)
                Lighting.AddLight(Projectile.Center, 0.06f, 0.24f, 0.29f);
            else
                Lighting.AddLight(Projectile.Center, 0.3f, 0.2f, 0.32f);

            // If the bullet has struck at least one target, increment the counter for turning blue.
            if (Projectile.numHits > 0 && CalamityUtils.FinalExtraUpdate(Projectile) && !BlueMode)
                ++Projectile.ai[1];

            // If the bullet has struck at least one target but hasn't hit anything for several frames, turn blue and warp.
            // Obviously if the bullet is already blue, it can't turn blue again.
            if (!BlueMode && Projectile.ai[1] >= TurnBlueFrameDelay)
                TurnBlue(true);

            // When blue, ignore walls for the first several updates.
            if (BlueMode && Projectile.ai[1] > 0f)
            {
                --Projectile.ai[1];
                if (Projectile.ai[1] == 0f)
                    Projectile.tileCollide = true;
            }
        }

        private void TurnBlue(bool setPosition = false)
        {
            // Switch to blue mode officially
            Projectile.ai[0] = 1f;

            // Provide several frames of passing through walls to prevent frustration
            Projectile.ai[1] = BlueNoCollideFrames;
            Projectile.tileCollide = false;

            // Reduce damage, but remove piercing. Reset local iframes so the bullet, turned blue, may always strike again.
            Projectile.damage = (int)(0.25f * Projectile.damage);
            Projectile.penetrate = 1;
            for (int i = 0; i < Main.maxNPCs; i++)
                Projectile.localNPCImmunity[i] = 0;

            // Reset projectile lifetime so it can fly again for its full possible range
            Projectile.timeLeft = Lifetime - NoDrawFrames * Projectile.MaxUpdates;

            if (!setPosition || Main.myPlayer != Projectile.owner)
                return;

            // The bullet disappears in a puff of dust.
            ProduceWarpCrossDust(Projectile.Center, ModContent.DustType<SquashDust>(), 0.5f, Color.Magenta);

            // The warp must be performed client side because it requires knowledge of the player's mouse position.
            Projectile.netUpdate = true;
            Projectile.tileCollide = false;

            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction

            // Step 1 of the warp: Place the bullet behind the player, opposite the mouse cursor.
            Vector2 playerToMouseVec = CalamityUtils.SafeDirectionTo(Main.LocalPlayer, Main.MouseWorld, -Vector2.UnitY);
            float warpDist = Main.rand.NextFloat(70f, 96f);
            float warpAngle = Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f);
            Vector2 warpOffset = -warpDist * playerToMouseVec.RotatedBy(warpAngle);
            Projectile.position = Main.LocalPlayer.MountedCenter + warpOffset;

            // Step 2 of the warp: Angle the bullet so that it is pointing at the mouse cursor.
            // This intentionally has a slight inaccuracy.
            Vector2 mouseTargetVec = Main.MouseWorld + Main.rand.NextVector2Circular(MouseAimDeviation, MouseAimDeviation);
            Vector2 bulletToMouseVec = CalamityUtils.SafeDirectionTo(Projectile, mouseTargetVec, -Vector2.UnitY);
            Projectile.velocity = bulletToMouseVec * Projectile.localAI[0];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Set all old positions to the bullet's warp position so that there aren't weird afterimages.
            // If an old position is uninitialized (0,0 aka never used), then don't change it.
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; ++i)
            {
                Vector2 oldPosElem = Projectile.oldPos[i];
                if (!(oldPosElem == Vector2.Zero))
                    Projectile.oldPos[i] = Projectile.position;
            }

            // Now that the bullet has warped, produce a tiny puff of dust at its back for effect.
            Vector2 warpInDustPos = Projectile.Center - bulletToMouseVec * TextureHeight;
            ProduceWarpCrossDust(warpInDustPos, ModContent.DustType<SquashDust>(), 1f, Color.Cyan);
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.05f) * Main.rand.NextFloat(9, 15);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<VoidDust>());
                d.position = warpInDustPos;
                d.velocity = dustVel;
                d.noGravity = true;
                d.scale *= Main.rand.NextFloat(0.6f, 1f);
                d.color = Color.Cyan;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 140);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft >= Lifetime - NoDrawFrames * Projectile.MaxUpdates)
                return false;
            // Use the blue bullet texture if the bullet has turned blue.
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, lightColor, BlueMode ? TextureBlue : null);
            return false;
        }

        // God Slayer Slugs explode on death, even if they never visually turned blue.
        public override void OnKill(int timeLeft)
        {
            // Turn blue to set stats correctly, if not already done.
            if (!BlueMode)
                TurnBlue(false);
            Projectile.ExpandHitboxBy(48);
            Projectile.Damage();

            // Create a fancy triangle of dust.
            int dustID = ModContent.DustType<SquashDust>();
            int numDust = 9;
            float triangleAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < numDust; ++i)
            {
                float lerp = i / (float)(numDust - 1);
                float speed = MathHelper.Lerp(0.2f, 3.6f, lerp);
                Vector2 dustVel = Vector2.UnitX.RotatedBy(triangleAngle) * speed * 2;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustID);
                d.position = Projectile.Center;
                d.velocity = dustVel;
                d.noGravity = true;
                d.fadeIn = 1.5f;
                d.scale *= Main.rand.NextFloat(1.4f, 1.9f) - lerp * 0.5f;
                d.color = Color.Lerp(Color.Cyan, Color.Magenta, lerp);
                Dust.BetterCloneDust(d).velocity = dustVel.RotatedBy(MathHelper.Pi * 2f / 3f);
                Dust.BetterCloneDust(d).velocity = dustVel.RotatedBy(MathHelper.Pi * 4f / 3f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // If the projectile has hit something but hasn't turned blue, turn it blue and warp it behind the player.
            if (Projectile.numHits > 0 && !BlueMode)
            {
                TurnBlue(true);
                return false;
            }

            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            return true;
        }

        private void ProduceWarpCrossDust(Vector2 dustPos, int dustID, float speedMultiplier, Color color)
        {
            if (speedMultiplier > 0.8f) //dark
            {
                for (int i = 0; i < 2; ++i)
                {
                    Particle pulse = new CustomPulse(dustPos, Vector2.Zero, Color.Black, "CalamityMod/ExtraTextures/BasicCircle", Vector2.One, 0, 0.4f * speedMultiplier, 0.05f * speedMultiplier, 12, false);
                    GeneralParticleHandler.SpawnParticle(pulse);
                    Particle pulse2 = new CustomPulse(dustPos, Vector2.Zero, color, "CalamityMod/Particles/BloomRing", Vector2.One, 0, 0.15f * (1 + i * 0.2f) * speedMultiplier, 0.025f * (1 + i * 0.2f) * speedMultiplier, 12, true);
                    GeneralParticleHandler.SpawnParticle(pulse2);
                }
            }
            else //light
            {
                Particle pulse = new CustomSpark(dustPos, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 10, 0.3f * speedMultiplier, color, new Vector2(1f, 1f), true, true, glowOpacity: 1f);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
            
            
            for (int i = 0; i < 5; ++i)
            {
                float speed = Main.rand.NextFloat(3f, 6f);
                Vector2 dustVel = Vector2.UnitX * speed * speedMultiplier * 1.5f;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustID);
                d.position = dustPos;
                d.velocity = dustVel;
                d.noGravity = true;
                d.scale *= Main.rand.NextFloat(1.8f, 2.2f) * (1 - speed / 7);
                d.color = color;
                d.fadeIn = 1;
                Dust.BetterCloneDust(d).velocity = dustVel.RotatedBy(MathHelper.PiOver2);
                Dust.BetterCloneDust(d).velocity = dustVel.RotatedBy(MathHelper.Pi);
                Dust.BetterCloneDust(d).velocity = dustVel.RotatedBy(-MathHelper.PiOver2);
            }
        }
    }
}
