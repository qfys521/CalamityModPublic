using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Healing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SanguineFlareProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public bool beingChanneled = true;
        public float dmgMult = 1;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 19;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            if (beingChanneled && player.channel)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 300 * Projectile.MaxUpdates;
                if (Projectile.FinalExtraUpdate())
                {
                    player.itemTime = 15;
                    player.SetItemAnimation(15);
                    var dir = player.Center.DirectionTo(player.ClampedMouseWorld()) * 64;
                    player.direction = dir.X >= 0 ? 1 : -1;
                    Projectile.direction = player.direction;
                    player.itemRotation = (dir * player.direction).ToRotation();
                    Projectile.Center = player.Center + player.Center.DirectionTo(player.ClampedMouseWorld()) * 64;
                    dmgMult *= 1.009f;
                    if (dmgMult >= 5f)
                    {
                        dmgMult = 5f;
                        player.channel = false;
                    }

                }
                    
            } else
            {
                if (beingChanneled)
                {
                    Projectile.damage = (int)(Projectile.damage * dmgMult);
                    Projectile.velocity = player.MountedCenter.DirectionTo(player.ClampedMouseWorld())*4;
                    Projectile.extraUpdates = 19;
                    beingChanneled = false;
                }

                float particleSize = (dmgMult * 5 + 5)/2f;
                Particle beam3 = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.1f, "CalamityMod/Particles/PearlParticleGlow", false, 10, 0.05f * particleSize, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.DarkRed), new Vector2(0.5f, 1), false, false, 0, false, false, 0f);
                GeneralParticleHandler.SpawnParticle(beam3);

                if (Main.rand.NextBool(8))
                {
                    Particle beam4 = new CustomSpark(Projectile.Center, Projectile.velocity * 0.1f, "CalamityMod/Particles/WaterFoam", false, 5, 0.01f * particleSize, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.Red), new Vector2(1f, 1), true, false, Main.rand.NextFloat(-10, 10), false, false, 0f);
                    GeneralParticleHandler.SpawnParticle(beam4);
                }
            }
        }

        public override bool? CanDamage()
        {
            return !beingChanneled;
        }
        public override void OnKill(int timeLeft)
        {
            SoundStyle hitSound = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");
            SoundEngine.PlaySound(hitSound with { Volume = 0.5f }, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int i = 0; i < 3; i++)
            {
                int brimDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (!ChildSafety.Disabled ? DustID.Cloud : (int)CalamityDusts.Brimstone), 0f, 0f, 100, default, 1.2f);
                Main.dust[brimDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[brimDust].scale = 0.5f;
                    Main.dust[brimDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 6; j++)
            {
                int brimDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (!ChildSafety.Disabled ? DustID.Cloud : (int)CalamityDusts.Brimstone), 0f, 0f, 100, default, 1.7f);
                Main.dust[brimDust2].noGravity = true;
                Main.dust[brimDust2].velocity *= 5f;
                brimDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (!ChildSafety.Disabled ? DustID.Cloud : (int)CalamityDusts.Brimstone), 0f, 0f, 100, default, 1f);
                Main.dust[brimDust2].velocity *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 360);
            if (beingChanneled || Projectile.penetrate == -1)
                return;
            var player = Main.player[Projectile.owner];
            float orbCount = MathHelper.Lerp(0, 20, (dmgMult-1) / 4f);
            for (var i = 0; i < orbCount; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Projectile.velocity.RotatedByRandom(1) * Main.rand.NextFloat(0.75f, 1.25f), ModContent.ProjectileType<BloodstoneHealOrb>(), 18, 0f, player.whoAmI);
            }
            if (dmgMult > 1)
            {
                Projectile.position = Projectile.Center;
                Projectile.Size *= dmgMult * 1.5f;
                Projectile.Center = Projectile.position;
                Projectile.penetrate = -1;
                Projectile.extraUpdates = 0;
                Projectile.timeLeft = 2;
                Projectile.velocity *= 0;
                Projectile.damage /= 2;
                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.DarkRed) * 0.75f, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f*dmgMult/5f, 0.87f * dmgMult / 5f, (int)(40 * 0.38f), false);
                GeneralParticleHandler.SpawnParticle(bloodsplosion);
                Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : new Color(255, 32, 32)) * 0.5f, "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f * dmgMult / 5f, 0.155f * dmgMult / 5f, 40);
                GeneralParticleHandler.SpawnParticle(bloodsplosion2);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Laceration>(), 60);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (beingChanneled)
            {

            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SmallGreyscaleCircle").Value;
            for (int i = 0; i < 1; i++)
            {
                //Photoviscerator drawcode, edited slightly
                    Color color = (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.DarkRed) * (dmgMult/5f);
                color.A = 0;
                Vector2 drawPosition = Projectile.oldPos[i]+Projectile.Size /2f + lightTexture.Size() * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(-32.5f, -32.5f); //Last vector is to offset the circle so that it is displayed where the hitbox actually is, instead of a bit down and to the right.
                Color outerColor = color;
                Color innerColor = color * 0.5f;
                float intensity = 0.9f + 0.15f * (float)Math.Cos(dmgMult * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60) //Shrinks to nothing when projectile is nearing death
                {
                    intensity *= Projectile.timeLeft / 60f;
                }
                // Become smaller the futher along the old positions we are.
                Vector2 outerScale = new Vector2(1f) * Projectile.scale * intensity;
                Vector2 innerScale = new Vector2(1f) * Projectile.scale * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, 0f, lightTexture.Size() * 0.5f, outerScale * 1.25f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, 0f, lightTexture.Size() * 0.5f, innerScale * 1.25f, SpriteEffects.None, 0);
                }
            }
            return false;
        }
    }
}
