using System;
using System.Collections.Generic;
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
    public class BloomStoneFlower : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/Magic/BeamingBolt";

        public ref float HookIndex => ref Projectile.ai[0];
        public ref float FlowerPart => ref Projectile.ai[1];

        public ref float timer => ref Projectile.ai[2];
        public float fadeTime = 0;
        public bool fadeOut = false;
        public int maxTime = 5;
        public override void SetDefaults()
        {
            Projectile.drawLayer = Terraria.ID.ProjectileDrawLayerID.OverPlayers;
            Projectile.width = Projectile.height = 30;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
        }

        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            switch (FlowerPart)
            {
                case 0:
                    Projectile hook = Main.projectile[(int)HookIndex];
                    if (!(hook.active && hook.aiStyle == ProjAIStyleID.Hook && hook.ai[0] == 2f))
                    {
                        fadeOut = true;
                    }
                    float sine = (float)Math.Sin(timer * 0.01f);
                    Projectile.rotation += fadeTime * sine * 0.005f;
                    if (Projectile.scale > 1)
                        Projectile.scale = MathHelper.Lerp(Projectile.scale, 1, 0.15f);

                    Projectile.timeLeft++;
                    if (!fadeOut && Vector2.DistanceSquared(Projectile.Center, Main.player[Projectile.owner].Center) < 4096f)
                    {
                        SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            int dusts = 6;
                            int dir = Main.rand.NextBool() ? -1 : 1;
                            for (int i = 0; i < dusts; i++)
                            {
                                Projectile pollen = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (MathHelper.TwoPi / dusts * i).ToRotationVector2() * 3f, Type, 0, 0f, Projectile.owner, 0f, 1f);
                                pollen.localAI[0] = dir;
                            }
                                
                        }
                        fadeOut = true;
                        Projectile.scale = 2.5f;
                    }
                    if ((fadeOut && Projectile.scale <= 1.05f) || (!fadeOut && fadeTime < maxTime))
                        fadeTime += (fadeOut ? -0.2f : 1);

                    if (fadeTime < 0 && fadeOut)
                        Projectile.Kill();
                    break;
                case 1:
                    if (timer == 0)
                    {
                        Projectile.scale = 1.8f;
                        Projectile.extraUpdates = 1;
                    }
                    if (Projectile.width < 90)
                        Projectile.ExpandHitboxBy(90);
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.04f * Projectile.localAI[0]) * 1.001f;
                    Color mistColor = Color.Lerp(Color.HotPink, Color.Gold, Utils.GetLerpValue(100, 0, Projectile.timeLeft, true));
                    // Visual effect
                    if (Projectile.timeLeft % 4 == 0)
                    {
                        MediumMistParticle pollenCloud = new(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), mistColor, mistColor, 3f * Projectile.scale, 100f);
                        GeneralParticleHandler.SpawnParticle(pollenCloud);
                    }
                    if (Projectile.timeLeft % 4 == 0)
                    {
                        Dust pollenDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<LightDust>(), newColor: mistColor, Scale: 0.6f * Projectile.scale);
                        pollenDust.noLightEmittance = true;
                        pollenDust.noGravity = true;
                    }
                    if (Projectile.scale > 0.7f)
                        Projectile.scale -= 0.025f;

                    // Check for buffing
                    Player owner = Main.player[Projectile.owner];
                    foreach (Player p in Main.ActivePlayers)
                    {
                        if (!(p == owner || (p.team == owner.team && owner.team != 0)))
                            continue;

                        if (Projectile.Hitbox.Intersects(p.Hitbox))
                            p.Calamity().bloomStoneBuffedHealRateTimer = 360;
                    }
                    fadeTime++;
                    break;
            }
            timer++;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (FlowerPart == 0f)
            {
                Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BigHeart").Value;
                Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

                int parts = 5;
                float rot = 0;
                float scaleFade = (float)Math.Pow(Utils.GetLerpValue(0, maxTime, fadeTime, true), 2);
                for (int b = 0; b < 3; b++)
                {
                    Color drawColor = Color.Lerp(Color.HotPink, Color.Plum, b * 0.3f) with { A = 0 };
                    float scaleMult = (1 - 0.15f * b) * scaleFade;
                    for (int i = 0; i < parts; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi * i / parts).ToRotationVector2();
                        Main.EntitySpriteDraw(tex2, Projectile.Center - Main.screenPosition + vel.RotatedBy(Projectile.rotation * (float)Math.Pow(1.5f - scaleMult, 2) + rot) * 35 * scaleMult, null, drawColor * 0.3f, Projectile.rotation * (float)Math.Pow(1.5f - scaleMult, 2) + vel.ToRotation() + MathHelper.PiOver2 + rot, tex2.Size() / 2f, new Vector2(0.9f, 1.5f) * Projectile.scale * 0.15f * scaleMult, SpriteEffects.None, 0);
                    }
                    rot += MathHelper.PiOver4 * 0.5f;
                }
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Gold with { A = 0 }, Projectile.rotation, tex.Size() / 2f, Projectile.scale * 0.45f * scaleFade, SpriteEffects.None, 0);

                return false;
            }
            else return false;
        }
    }
}
