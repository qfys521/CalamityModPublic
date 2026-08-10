using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class OldDukeHeadCorpse : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public int GFBTimer = 3000;
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 58;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            NPC target = Projectile.Center.MinionHoming(845f, player, false);
            // No sense in targeting something below this sentry.
            if (target != null)
            {
                if (target.Bottom.Y > Projectile.Top.Y)
                {
                    target = null;
                }
            }

            if (Main.zenithWorld)
            {
                if (GFBTimer > 0)
                {
                    if (Main.rand.NextBool((int)(400 * Utils.GetLerpValue(0, 2000, GFBTimer)) + 1))
                    {
                        Projectile.velocity += (Vector2.One * 0.3f).RotatedByRandom(100);
                        SoundStyle heartbeat = new("CalamityMod/Sounds/Item/Heartbeat");
                        SoundEngine.PlaySound(heartbeat with { Volume = 0.5f, MaxInstances = -1 }, player.Center);
                        player.SetScreenshake(6.5f * Utils.GetLerpValue(600, 0, GFBTimer));
                        Projectile.velocity *= 0.99f;
                    }
                    GFBTimer--;
                    if (GFBTimer == 0)
                    {
                        SoundStyle h = new("CalamityMod/Sounds/Custom/GFB/HeComes");
                        SoundEngine.PlaySound(h with { Volume = 1f, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, player.Center);
                    }
                }
                else
                {
                    bool far = Utils.Distance(player.Center, Projectile.Center) > 2000;
                    if (Utils.Distance(player.Center, Projectile.Center) > 50 && Projectile.timeLeft % (far ? 2 : 10) == 0)
                    {
                        Projectile.scale = 1;
                        Vector2 moveDir = Utils.DirectionTo(Projectile.Center, player.Center);
                        Vector2 moveDir2 = (moveDir * (far ? 40 : Main.rand.NextFloat(20f, 25f))).RotatedByRandom(0.4f);
                        Projectile.Center += moveDir2;
                        Projectile.velocity += far ? Vector2.Zero : moveDir2 * 0.4f;
                        for (int j = 0; j < 8; j++)
                        {
                            Dust c = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(35, 35), ModContent.DustType<LightDust>());
                            c.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(3, 7);
                            c.scale = Main.rand.NextFloat(0.5f, 0.7f);
                            c.noGravity = true;
                            c.color = Color.Chartreuse;
                            c.noLightEmittance = true;
                        }
                        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.85f, MaxInstances = -1 }, Projectile.Center);
                        if (far && Projectile.timeLeft % 30 == 0)
                        {
                            SoundStyle h = new("CalamityMod/Sounds/Custom/GFB/YouAreNotSafe");
                            SoundEngine.PlaySound(h with { Volume = 0.8f, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, player.Center);

                            Main.NewText(CalamityUtils.GetTextValue("Misc.NotSafe"), Color.Chartreuse);
                        }
                    }
                    else
                        Projectile.velocity *= 0.9f;
                    if (Utils.Distance(player.Center, Projectile.Center) < 50)
                    {
                        player.AddBuff(BuffID.Obstructed, 30);
                        player.AddBuff(BuffID.Darkness, 30);
                        player.AddBuff(ModContent.BuffType<MiracleBlight>(), 15);

                        if (player.statLife > 15)
                            player.statLife = (int)(player.statLife * 0.97f);

                        player.velocity *= 0.95f;
                        player.Center += (Vector2.One * 7).RotatedByRandom(100);

                        if (Projectile.timeLeft % 20 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                SoundStyle heartbeat = new("CalamityMod/Sounds/Item/Heartbeat");
                                SoundEngine.PlaySound(heartbeat with { Volume = 0.8f, MaxInstances = -1 }, player.Center);
                            }
                            player.SetScreenshake(4.5f);
                        }
                        Projectile.Center = player.Center;
                        SoundEngine.PlaySound(SoundID.NPCDeath20 with { Volume = 0.35f, MaxInstances = -1 }, Projectile.Center);

                        if (player.dead)
                            Projectile.scale *= 1.02f;
                        else
                            Projectile.scale = 1;
                    }
                }
                
            }
            


            Projectile.frame = (target != null).ToInt();
            if (target != null)
            {
                Projectile.ai[0] += 1f;
                if (Main.myPlayer == Projectile.owner &&
                    Projectile.ai[0] % 8f == 0f)
                {
                    float angle = (float)Math.Atan(Math.Abs(target.Center.X - Projectile.Center.X) / 450f);
                    angle *= Math.Sign(target.Center.X - Projectile.Center.X);
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Top + Vector2.UnitY * 7f,
                        new Vector2(0f, -Main.rand.NextFloat(21f, 30.5f)).RotatedBy(angle),
                        ModContent.ProjectileType<OldDukeSharkVomit>(), Projectile.damage, 5f,
                        Projectile.owner);
                }
            }
            Projectile.velocity.Y += 0.5f;

            if (Projectile.velocity.Y > 10f)
            {
                Projectile.velocity.Y = 10f;
            }
        }

        public override bool? CanDamage() => false;
        // Don't die on tile collision
        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
    }
}
