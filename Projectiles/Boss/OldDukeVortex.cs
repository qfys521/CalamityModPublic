using System;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class OldDukeVortex : ModProjectile, ILocalizedModType
    {
        Vector2 cen = Vector2.Zero;

        public new string LocalizationCategory => "Projectiles.Boss";
        public static SoundStyle SpawnSound = new("CalamityMod/Sounds/Custom/OldDukeVortex");
        public SlotId SoundId;

        public override void SetStaticDefaults()
        {
            SpawnSound.MaxInstances = 50;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            SoundId = SoundEngine.PlaySound(SpawnSound with { IsLooped = true, MaxInstances = 20 }, Projectile.Center, _ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame());
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 408;
            Projectile.height = 408;
            Projectile.scale = 0.004f;
            Projectile.hostile = true;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1800;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                cen = Projectile.Center;
                Projectile.localAI[1] = 1;
            }
            Projectile.position = cen - (new Vector2(Projectile.width / 2) * Projectile.scale);

            if (Main.zenithWorld)
            {
                if (Projectile.scale < 2f)
                {
                    if (Projectile.alpha > 0)
                        Projectile.alpha -= 1;

                    Projectile.scale += 0.004f;
                    if (Projectile.scale > 2f)
                        Projectile.scale = 2f;
                }
                else
                {
                    if (Projectile.timeLeft <= 85)
                    {
                        if (Projectile.alpha < 255)
                            Projectile.alpha += 3;

                        Projectile.scale -= 0.012f;
                    }
                }
            }
            else
            {
                if (Projectile.scale < 1f)
                {
                    if (Projectile.alpha > 0)
                        Projectile.alpha -= 1;

                    Projectile.scale += 0.004f;
                    if (Projectile.scale > 1f)
                        Projectile.scale = 1f;

                    Projectile.width = Projectile.height = (int)(408f * Projectile.scale);
                }
                else
                {
                    if (Projectile.timeLeft <= 85)
                    {
                        if (Projectile.alpha < 255)
                            Projectile.alpha += 3;

                        Projectile.scale -= 0.012f;
                        Projectile.width = Projectile.height = (int)(408f * Projectile.scale);
                    }
                    else
                        Projectile.width = Projectile.height = 408;
                }
            }

            float distanceRequired = 800f * Projectile.scale;
            float succPower = Main.zenithWorld ? 1f : 0.5f;
            foreach (Player player in Main.ActivePlayers)
            {
                float distance = Vector2.Distance(player.Center, cen);
                if (distance < distanceRequired && player.grappling[0] == -1)
                {
                    if (Collision.CanHit(cen, 1, 1, player.Center, 1, 1))
                    {
                        float distanceRatio = distance / distanceRequired;

                        float wingTimeSet = (float)Math.Ceiling((float)player.wingTimeMax * 0.5f * distanceRatio);
                        if (player.wingTime > wingTimeSet)
                            player.wingTime = wingTimeSet;

                        float multiplier = 1f - distanceRatio;
                        if (player.Center.X < cen.X)
                            player.velocity.X += succPower * multiplier;
                        else
                            player.velocity.X -= succPower * multiplier;
                    }
                }
            }

            Projectile.ai[0]++;

            if (Projectile.ai[0] % 10 == 1)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(cen, Vector2.Zero, new Color(55, 195, 0, 20), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), Projectile.scale * 0.9f, Projectile.scale * 0.4f, 40));
            }

            if (Projectile.timeLeft <= 85)
            {
                Projectile.localAI[2] += 1f / 85f;
            }
            Projectile.velocity = Vector2.Zero;

            Projectile.rotation -= 0.1f * (float)(1D - (Projectile.alpha / 255D));

            float lightAmt = 2f * Projectile.scale;
            Lighting.AddLight(cen, lightAmt, lightAmt * 2f, lightAmt);

            float maxdist = 1200;

            if (SoundEngine.TryGetActiveSound(SoundId, out var Sound) && Sound.IsPlaying)
            {
                Sound.Position = cen;
                Sound.Volume = Projectile.scale;
                Sound.Pitch = MathHelper.Lerp(0f, -1f, (MathHelper.Clamp((Projectile.Distance(Main.LocalPlayer.Center) - 800) / maxdist, 0f, 1f) + (-Projectile.scale + 1)));
            }

            if (Projectile.timeLeft > 85)
            {
                Vector2 vec2 = cen + new Vector2(Main.rand.NextFloat(320, 540) * Projectile.scale, 0).RotatedByRandom(MathHelper.TwoPi);

                GeneralParticleHandler.SpawnParticle(new SparkParticle(vec2, (cen - vec2) / 20, false, 10, Main.rand.NextFloat(0.5f, 1f), Color.LimeGreen, true));
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

        public override bool CanHitPlayer(Player target) => Projectile.timeLeft <= 1680 && Projectile.timeLeft > 85;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 210f * Projectile.scale, targetHitbox);

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            if (Projectile.timeLeft <= 1680 && Projectile.timeLeft > 85)
                target.AddBuff(ModContent.BuffType<Irradiated>(), 600);
        }
    }
}
