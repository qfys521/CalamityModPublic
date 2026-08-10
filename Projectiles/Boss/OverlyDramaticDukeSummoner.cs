using System;
using CalamityMod.Events;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class OverlyDramaticDukeSummoner : ModProjectile, ILocalizedModType
    {
        Vector2 cen;

        public SlotId? SoundId;

        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/Boss/OldDukeVortex";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 408;
            Projectile.scale = 0.004f;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1800;
        }

        private static void ExpandVertically(int startX, int startY, out int topY, out int bottomY, int maxExpandUp = 100, int maxExpandDown = 100)
        {
            topY = startY;
            bottomY = startY;
            if (!WorldGen.InWorld(startX, startY, 10))
            {
                return;
            }
            int yUp = 0;
            while (yUp < maxExpandUp && topY > 0 && topY >= 10 && Main.tile[startX, topY] != null)
            {
                topY--;
                yUp++;
            }
            int yDown = 0;
            while (yDown < maxExpandDown && bottomY < Main.maxTilesY - 10 && bottomY <= Main.maxTilesY - 10)
            {
                if (Main.tile[startX, bottomY] == null)
                {
                    return;
                }
                bottomY++;
                yDown++;
            }
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server && !SoundId.HasValue)
            {
                SoundId = SoundEngine.PlaySound(OldDukeVortex.SpawnSound with { IsLooped = true, MaxInstances = 20 }, Projectile.Center, _ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame());
            }
            
            if (Projectile.ai[0] == 0)
                cen = Projectile.Center;

            Projectile.rotation -= 0.15f * (float)(1D - (Projectile.alpha / 255D)) * (Projectile.ai[0] / 660f);
            Projectile.ai[0]++;

            Projectile.ai[1]++;

            Vector2 vec = new Vector2(408, 408) * Projectile.scale;

            Projectile.position = cen - new Vector2((float)Math.Sqrt(vec.X), (float)Math.Sqrt(vec.Y));

            float totalTilesToExpand = 1600f * Projectile.scale / 16;

            Point centerAsTileCoords = Projectile.Center.ToTileCoordinates();
            Vector2 topVector = Projectile.Top;
            Vector2 bottomVector = Projectile.Bottom;
            Vector2 centerVector = Vector2.Lerp(topVector, bottomVector, 0.5f);
            Projectile.width = (int)(208 * Projectile.scale);

            Vector2 ProjectileSpawnPosition = cen;

            if (Projectile.ai[0] < 90f)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Projectile.ai[0] / 90f);
            }
            if (Projectile.ai[0] < 600f)
            {
                Projectile.scale = MathHelper.Lerp(0.004f, 1.6f, Projectile.ai[0] / 660f);

                Vector2 vec2 = Projectile.Center + new Vector2(Main.rand.NextFloat(320, 540) * Projectile.scale, 0).RotatedByRandom(MathHelper.TwoPi);

                GeneralParticleHandler.SpawnParticle(new SparkParticle(vec2, (Projectile.Center - vec2) / 20, false, 10, Main.rand.NextFloat(0.5f, 1f), Color.LimeGreen, true));
            }

            if (Projectile.ai[0] % 10 == 1 && Projectile.ai[0] < 600f)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(cen, Vector2.Zero, new Color(55, 195, 0, 20), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), Projectile.scale * 0.9f, Projectile.scale * 0.4f, 40));
            }

            float maxdist = 1200;

            // Spray gore and acid everywhere
            if (Projectile.ai[0] < 480f && Projectile.ai[0] > 90f)
            {
                if (Projectile.ai[0] % 10f == 9f)
                {
                    Vector2 velocity = new Vector2(0f, -18f).RotatedByRandom(0.7f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), ProjectileSpawnPosition, velocity,
                        ModContent.ProjectileType<OldDukeSummonDrop>(), 65, 2f);
                }
                if (Projectile.ai[0] % 35f == 34f)
                {
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), -7f - Main.rand.NextFloat(4f, 12f)).RotatedByRandom(0.5f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), ProjectileSpawnPosition, velocity,
                        ModContent.ProjectileType<OldDukeGore>(), 65, 2f);
                }
            }

            // Fade out and die
            if (Projectile.ai[0] >= 600f)
            {
                bool canSpawnBoomer = false;
                foreach (Player player in Main.ActivePlayers)
                {
                    if (!player.dead && Projectile.Distance(player.Center) < 12000f)
                    {
                        canSpawnBoomer = true;
                        break;
                    }
                }

                // Summon the boomer duke
                if (Projectile.ai[0] == 600f)
                {
                    if (canSpawnBoomer)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath.WithPitchOffset(0.5f), cen);
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact.WithPitchOffset(-0.5f), cen);

                        for (float i = 0; i <= 5; i++)
                        {
                            if (i == 5)
                            {
                                GeneralParticleHandler.SpawnParticle(new CustomPulse(cen, Vector2.Zero, new Color(55, 255, 0), "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.05f, i * 0.1f, 40));
                            }

                            GeneralParticleHandler.SpawnParticle(new CustomPulse(cen, Vector2.Zero, new Color(55, 255, 0), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.05f, i * 0.1f, 40));
                        }
                        
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int boomer = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)ProjectileSpawnPosition.X, (int)ProjectileSpawnPosition.Y, ModContent.NPCType<OldDuke>());
                            string boomerName = Main.npc[boomer].TypeName;

                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                Main.NewText(Language.GetTextValue("Announcement.HasAwoken", boomerName), new Color(175, 75, 255));
                                return;
                            }

                            if (Main.dedServ)
                            {
                                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", new object[]
                                {
                                    Main.npc[boomer].GetTypeNetName()
                                }), new Color(175, 75, 255));
                                return;
                            }

                            CalamityUtils.BossAwakenMessage(boomer);

                            Main.npc[boomer].velocity = Vector2.UnitY * -12f;
                            Main.npc[boomer].alpha = 255;
                            Main.npc[boomer].Calamity().newAI[3] = 1f; // To signal that Old Duke should not deccelerate as it normally would
                            Main.npc[boomer].netUpdate = true;
                            AcidRainEvent.HasTriedToSummonOldDuke = true;
                            AcidRainEvent.OldDukeHasBeenEncountered = true;
                            AcidRainEvent.UpdateInvasion(false);
                        }
                    }
                    else
                    {
                        AcidRainEvent.AccumulatedKillPoints = 0;
                        AcidRainEvent.HasTriedToSummonOldDuke = false;
                        AcidRainEvent.UpdateInvasion(false);
                    }
                }

                if (Projectile.ai[0] >= 600f)
                {
                    Projectile.alpha = (int)MathHelper.Lerp(0f, 255f, MathHelper.Clamp((Projectile.ai[0] - 600f) / 30, 0f, 1f));
                    Projectile.scale = MathHelper.Lerp(Projectile.scale, 0f, MathHelper.Clamp((Projectile.ai[0] - 600f) / 30, 0f, 1f));
                }
            }
            if (Projectile.ai[0] >= 720f)
            {
                Projectile.Kill();
            }

            if (SoundId.HasValue && SoundEngine.TryGetActiveSound(SoundId.Value, out var Sound) && Sound.IsPlaying)
            {
                Sound.Position = Projectile.Center;
                Sound.Volume = Projectile.scale * 2f;
                Sound.Pitch = MathHelper.Lerp(0f, -1f, (MathHelper.Clamp((Projectile.Distance(Main.LocalPlayer.Center) - 800) / maxdist, 0f, 1f) + (-Projectile.scale + 1)));
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> Tex = ModContent.Request<Texture2D>(Texture);

            float sc = MathHelper.Lerp(1, 0, Projectile.localAI[2]);

            float alphaLerp = MathHelper.Lerp(1f, 0f, (float)Projectile.alpha / 255f);

            Main.EntitySpriteDraw(Tex.Value, cen - Main.screenPosition, Tex.Frame(), new Color(0f, 0f, 0f, 0.4f).MultiplyRGBA(new Color(alphaLerp, alphaLerp, alphaLerp, alphaLerp)), -Projectile.rotation / 2 * (4 + 1), Tex.Frame().Center(), 1.61f * Projectile.scale * sc, SpriteEffects.None);

            for (int i = 2; i >= 0; i--)
            {
                float lerp = (float)i / 3f;

                Main.EntitySpriteDraw(Tex.Value, cen - Main.screenPosition, Tex.Frame(), Color.Lerp(new Color(5, 155, 95, 100), new Color(255, 255, 255, 55), lerp).MultiplyRGBA(new Color(alphaLerp, alphaLerp, alphaLerp, alphaLerp)), -Projectile.rotation / 2 * (i + 1), Tex.Frame().Center(), MathHelper.Lerp(1f, 1.7f, lerp) * Projectile.scale * sc, SpriteEffects.None);
            }
            return false;
        }

        public override bool CanHitPlayer(Player target) => false;
    }
}
