using System;
using System.Collections.Generic;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Healing
{
    public class AbsorberAura : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Healing";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private int AbDust = ModContent.DustType<LightDust>();
        public int ShinkGrow = 0;
        public int Framecounter = 0;
        public int CleanseOnce = 1;
        public int PulseOnce = 1;
        public int PulseOnce2 = 1;
        public int PulseOnce3 = 1;
        public static readonly SoundStyle Spawnsound = new("CalamityMod/Sounds/Custom/OrbHeal3") { Volume = 0.5f };
        public List<bool> cleanseList = new List<bool>(new bool[Main.maxPlayers]);
        public ref int CleansingEffect => ref Main.player[Projectile.owner].Calamity().CleansingEffect;

        public override void SetDefaults()
        {
            //These shouldn't matter because its circular
            Projectile.width = Projectile.height = 336;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TheAbsorber.AuraLifetime + 10;
        }

        public override void AI()
        {
            Framecounter++;
            float sine = Math.Abs((float)Math.Sin((Main.GlobalTimeWrappedHourly * 1.1f) * 5f / MathHelper.Pi));
            for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
            {
                Player player = Main.player[playerIndex];
                float targetDist = Vector2.Distance(player.Center, Projectile.Center);

                //Remove the players debuffs and defense damage, but only once per aura
                if (targetDist < 310f)
                {
                    player.AddBuff(ModContent.BuffType<AbsorberRegen>(), 600);
                    if (cleanseList[playerIndex] == false)
                    {
                        cleanseList[playerIndex] = true;
                        CleansingEffect = 1;
                        for (int l = 0; l < Player.MaxBuffs; l++)
                        {
                            int buffID = player.buffType[l];
                            if (player.buffTime[l] > 2 && CalamityBuffSets.IsDebuff[buffID])
                            {
                                player.buffTime[l] *= 0;
                            }
                        }
                        for (int i = 0; i < 55; i++)
                        {
                            int dust = Dust.NewDust(player.Center, player.width + 4, player.height + 4, AbDust, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, default, 5.5f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity *= 1.5f;
                            Main.dust[dust].velocity.Y -= 0.5f;
                            Main.dust[dust].color = Main.rand.NextBool(3) ? Color.PaleGreen : Color.DarkSeaGreen;
                        }
                        SoundEngine.PlaySound(Spawnsound with { Pitch = -0.9f }, Projectile.Center);
                    }
                }
            }

            if (Framecounter >= 10)
            {
                for (int i = 0; i < 3; i++)
                {
                    float areaSize = 305f;
                    Vector2 spawnSpot = Projectile.Center + Main.rand.NextVector2CircularEdge(areaSize, areaSize);
                    Dust dust = Dust.NewDustPerfect(spawnSpot, AbDust, null, 0);
                    dust.scale = Main.rand.NextFloat(1.2f, 2.3f);
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool(3) ? Color.PaleGreen : Color.DarkSeaGreen;
                    dust.velocity = (Utils.DirectionTo(Projectile.Center, spawnSpot) * Main.rand.NextFloat(1.5f, 4.5f) * sine).RotatedByRandom(0.4f);
                }

                for (int i = 0; i < 1; i++)
                {
                    float areaSize = 272.5f + 20 * sine;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(areaSize, areaSize), AbDust, null, 0);
                    dust.scale = Main.rand.NextFloat(0.3f, 0.9f);
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool(3) ? Color.PaleGreen : Color.DarkSeaGreen;
                }
            }
            Projectile.rotation += 0.15f * sine;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/HighResFoggyCircleHardEdge").Value;
            Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/HighResHollowCircleHardEdge").Value;
            Color drawColor1 = Color.DarkSeaGreen;
            Color drawColor2 = Color.PaleGreen;
            float sine = Math.Abs((float)Math.Sin((Main.GlobalTimeWrappedHourly * 1.1f) * 5f / MathHelper.Pi));
            float areaScale = Math.Min(Utils.GetLerpValue(TheAbsorber.AuraLifetime + 10, TheAbsorber.AuraLifetime, Projectile.timeLeft, true), Utils.GetLerpValue(0, 10, Projectile.timeLeft, true));

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, drawColor1 with { A = 0 } * 0.6f, 0, tex.Size() / 2f, (0.305f - 0.006f * sine) * areaScale, SpriteEffects.None, 0);
            for (int i = 1; i <= 8; i++)
            {
                if (i != 0)
                {
                    float rot = (MathHelper.TwoPi * i / 8f);
                    Main.EntitySpriteDraw(tex2, Projectile.Center - Main.screenPosition, null, drawColor2 with { A = 0 } * 0.03f, Projectile.rotation + rot, tex2.Size() / 2f, new Vector2((0.2f * sine) + 0.8f, 1) * 0.29f * areaScale, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(tex2, Projectile.Center - Main.screenPosition, null, drawColor1 with { A = 0 } * 0.3f, -Projectile.rotation, tex2.Size() / 2f, new Vector2(0.97f, 1) * 0.3f * areaScale, SpriteEffects.None, 0);

            return false;
        }
        public override bool? CanDamage() => false;
    }
}
