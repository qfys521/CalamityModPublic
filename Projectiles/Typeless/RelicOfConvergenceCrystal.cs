using System;
using System.Collections.Generic;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Items.Tools;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class RelicOfConvergenceCrystal : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<RelicOfConvergence>();
        public int SoundInterval = 25;
        public int TotalCrystalsToDraw = 3;
        public int CrystalsDrawTime = 40;
        public float MaxCrystalOffsetRadius = 80f;
        public float MaxDustOffsetRadius = 70f;

        private Player Owner => Main.player[Projectile.owner];
        public List<bool> healList = new List<bool>(new bool[Main.maxPlayers]);

        public ref float time => ref Projectile.ai[0];
        public float completion = 0;
        public float fade = 0;
        public int killTimer = 0;
        public Vector2 mousePos;
        public bool playSound = true;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 46;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 125;
        }

        public override void AI()
        {
            completion = Utils.GetLerpValue(120, 0, Projectile.timeLeft, true);
            fade = MathHelper.Lerp(fade, 0, 0.04f);
            if (Projectile.timeLeft >= 5)
                mousePos = Owner.ClampedMouseWorld();

            if (Owner.channel)
                killTimer = 18;
            if (killTimer <= 0)
            {
                Projectile.Kill();
                return;
            }
            if (Owner.Calamity().profanedSoulRelicBuff)
                Projectile.extraUpdates = 1;
            killTimer--;

            UpdatePlayerVisuals(Owner);

            // Make a constant "magical" sound.
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.5f * (time >= CrystalsDrawTime ? 1 : 2), Pitch = 0.5f * completion }, Projectile.Center);

                if (time >= CrystalsDrawTime)
                {
                    SoundStyle h = new("CalamityMod/Sounds/Item/NullHit");
                    SoundEngine.PlaySound(h with { Volume = 0.4f, Pitch = -0.3f + 0.7f * completion }, Projectile.Center);

                    float numberOfDusts = 10f;
                    for (int i = 0; i < numberOfDusts; i++)
                    {
                        Particle energy = new VelChangingSpark(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(9f, 18f), Utils.DirectionFrom(mousePos, Projectile.Center) * 35, "CalamityMod/Particles/BloomCircle", 25, Main.rand.NextFloat(0.1f, 0.35f) * completion, Color.Lerp(Color.Orange, Color.Orchid, completion), new Vector2(1f, 1f), lerpRate: 0.04f, shrinkSpeed: 0.15f);
                        GeneralParticleHandler.SpawnParticle(energy);
                    }
                }

                Projectile.soundDelay = (int)(SoundInterval * (time >= CrystalsDrawTime ? 1 - 0.9f * completion : 0.5f));
                fade = 1;
            }

            if (Projectile.timeLeft == 5)
            {
                //if (Projectile.owner != Main.myPlayer)
                    //return;

                for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
                {
                    Player player = Main.player[playerIndex];
                    float targetDist = player.Center.DistanceSQ(mousePos);

                    if (targetDist < 138f * 138f && player.team == Owner.team)
                    {
                        if (healList[playerIndex] == false)
                        {
                            healList[playerIndex] = true;
                            int trueHealValue = (int)(RelicOfConvergence.HealValue * (player.whoAmI == Owner.whoAmI ? 1f : 1.5f) * (Owner.Calamity().profanedSoulRelicBuff ? 1.25f : 1f));
                            player.HealPlayer(trueHealValue, HealTextType.Local);

                            if (playSound)
                            {
                                SoundStyle heal = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianHeal");
                                SoundEngine.PlaySound(heal with { Volume = 1, MaxInstances = -1 }, Projectile.Center);
                                playSound = false;
                            }

                            for (int i = 0; i < 5; i++)
                            {
                                Particle spark = new CustomSpark(player.Center + Main.rand.NextVector2Circular(15, 15), (-Vector2.UnitY * Main.rand.NextFloat(0.2f, 3f)), "CalamityMod/Particles/HealingPlus", false, Main.rand.Next(35, 50 + 1), Main.rand.NextFloat(1.1f, 1.9f), Color.Lerp(Color.Orchid, Color.White, i * 0.1f), Vector2.One, true, true, 0, false, false, 0.1f);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }
                        }
                    }
                }
            }

            if (time >= CrystalsDrawTime)
            {
                GeneratePassiveDust(Owner);

                Lighting.AddLight(Projectile.Center, Color.Lerp(Color.Orange, Color.Orchid, completion).ToVector3() * (2.5f * (completion - 0.375f) + fade));
            }

            time++;
        }

        public void UpdatePlayerVisuals(Player player)
        {
            Vector2 vel = Utils.DirectionTo(player.Center, mousePos);
            float rot = vel.ToRotation() + (player.direction == -1 ? MathHelper.ToRadians(270f) : MathHelper.ToRadians(-90f));

            player.ChangeDir(MathF.Sign(vel.X));

            Projectile.Center = player.Center + vel * 15f;

            // The crystal is a holdout projectile, so change the player's variables to reflect that
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rot);
        }

        public void GeneratePassiveDust(Player player)
        {
            float radius = 45f;
            radius = MathHelper.Lerp(0f, 200f, completion - 0.375f);

            for (float angle = 0f; angle <= MathHelper.TwoPi; angle += MathHelper.ToRadians(Main.rand.NextFloat(6f, 8f)))
            {
                Vector2 drawPos = mousePos + angle.ToRotationVector2() * radius;
                Color useColor = Color.Lerp(Color.Orange, Color.Orchid, completion) * (completion - 0.25f);
                float particleScale = 0.01f + fade * 0.08f + completion * 0.08f;
                Particle aura = new CustomSpark(drawPos, Utils.DirectionTo(mousePos, drawPos), "CalamityMod/Particles/SmallBloom", false, 4, particleScale, useColor, new Vector2(0.5f + completion, (2f - completion) * 7 - completion * 7));
                GeneralParticleHandler.SpawnParticle(aura);

                if (Main.rand.NextBool(70))
                {
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * radius, ModContent.DustType<LightDust>());
                    dust2.position = mousePos + angle.ToRotationVector2() * radius;
                    dust2.scale = Main.rand.NextFloat(1.4f, 1.9f) * completion;
                    dust2.noGravity = false;
                    dust2.velocity = new Vector2(0, Main.rand.NextFloat(1, 5));
                    dust2.color = useColor;
                }

                if (Projectile.timeLeft == 5)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * radius, ModContent.DustType<LightDust>());
                    dust.position = drawPos;
                    dust.scale = Main.rand.NextFloat(1.6f, 1.9f);
                    dust.noGravity = !Main.rand.NextBool(5);
                    dust.velocity = Utils.DirectionTo(mousePos, drawPos) * Main.rand.NextFloat(2f, 4f);
                    dust.color = Color.Orchid;
                    dust.noLightEmittance = true;
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float opacity = time / CrystalsDrawTime;
            Texture2D crystalTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            for (int i = 0; i < TotalCrystalsToDraw; i++)
            {
                float angle = MathHelper.TwoPi / TotalCrystalsToDraw * i + time / 10f;
                float radius = MathHelper.Lerp(MaxCrystalOffsetRadius, 0f, time / CrystalsDrawTime);
                Vector2 drawPositionOffset = angle.ToRotationVector2() * radius;
                Vector2 drawPosition = (time >= CrystalsDrawTime ? Projectile.Center : Projectile.Center + drawPositionOffset + Main.rand.NextVector2Circular(12, 12));

                Projectile.DrawProjectileWithBackglow(Color.Lerp(Color.Orchid, Color.Goldenrod, fade) with { A = 0 } * completion * 0.5f, Color.Lerp(Color.White, Color.White with { A = 0 }, fade * 0.5f) * MathHelper.Clamp(completion * 1.5f, time >= CrystalsDrawTime ? 0.8f : 0f, 1), 4f * completion + (fade * 3), crystalTexture, xPos: drawPosition.X, yPos: drawPosition.Y);
            }
            return false;
        }
    }
}
