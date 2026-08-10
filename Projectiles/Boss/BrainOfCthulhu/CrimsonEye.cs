using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss.BrainOfCthulhu;

public class CrimsonEye : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";

    private ref float Time => ref Projectile.ai[0];
    private ref float DistanceRatio => ref Projectile.ai[1];
    private ref float ExplosionTime => ref Projectile.ai[2];

    private static float TimeToExplode => 30f;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.drawLayer = Terraria.ID.ProjectileDrawLayerID.BehindNPCsAndTiles;
        Projectile.width = 100;
        Projectile.height = 36;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.scale = 1;
        Projectile.hostile = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 32; i++)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Crimstone);
    }

    public override void AI()
    {
        if (Time > 45)
        {
            Player closest = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            DistanceRatio = 1 - MathHelper.Clamp((Projectile.Center.Distance(closest.Center) - 128) / 120f, 0f, 1f);

            if (DistanceRatio >= 1 && Projectile.frame == 4)
            {
                if (Projectile.timeLeft < 120)
                    Projectile.timeLeft = 120;

                if (ExplosionTime == TimeToExplode)
                {
                    Projectile.hostile = true;
                    DetailedExplosion explosion = new(Projectile.Center, Vector2.Zero, Color.Orange, Vector2.One, Main.rand.NextFloatDirection(), 0.3f, 0.9f, 24);
                    GeneralParticleHandler.SpawnParticle(explosion);

                    Projectile.Resize(256, 256);
                }
                else if (Projectile.width == 256)
                    Projectile.active = false;

                ExplosionTime++;
            }
            else
            {
                if (ExplosionTime > 0)
                    ExplosionTime--;

                if (Projectile.width == 256)
                    Projectile.active = false;
            }

            if (Time > 65)
            {
                if (Projectile.timeLeft <= 20)
                    Projectile.frame = (Projectile.timeLeft - 5) / 5;
                else
                    Projectile.frame = 4;
            }
            else if (Time % 5 == 0)
                Projectile.frame++;
        }
        Time++;
    }


    public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
    {
        if (ExplosionTime >= TimeToExplode)
            return false;

        if (ExplosionTime > 0)
        {
            float ratio = ExplosionTime / TimeToExplode;

            Vector3 lightLevels = lightColor.ToVector3();
            float lightLevel = (lightLevels.X + lightLevels.Y + lightLevels.Z) / 3f;
            lightColor = Color.Lerp(lightColor, Color.Lerp(Color.Red, Color.Gold, (float)Math.Sin(ExplosionTime / 4f) / 2f + 0.5f) * lightLevel, ratio);
            lightColor.A = 255;
        }

        return true;
    }

    public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
    {
        if (ExplosionTime >= TimeToExplode)
            return;

        float ratio = CalamityUtils.SineOutEasing(ExplosionTime / TimeToExplode, 1);

        if (Projectile.frame == 4)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture + "Pupil").Value;
            Vector2 rangeScale = new Vector2(2.25f, 0.75f);
            Vector2 dir = Projectile.DirectionTo(Main.LocalPlayer.Center);
            if (ExplosionTime > 0)
            {
                if (ratio < 0.25f)
                    dir = Vector2.Lerp(dir, Vector2.Zero, ratio * 4f);
                else
                    dir = Main.rand.NextVector2CircularEdge(1, 1);
            }

            if (Projectile.timeLeft <= 50)
                dir *= (Projectile.timeLeft - 20) / 30f;

            float range = 6f;
            if (ratio > 0.25f)
            {
                range = MathHelper.Lerp(0f, 6f, (ratio - 0.25f) * 0.75f);
            }
            else if (Time < 85)
            {
                float lerp = CalamityUtils.SineOutEasing((Time - 65) / 20f, 1);
                range = MathHelper.Lerp(0f, range, lerp);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center + (dir * rangeScale * range) - Main.screenPosition, null, lightColor, 0, tex.Size() * 0.5f, 1f, 0);
        }

        if (DistanceRatio > 0)
        {
            float opacity = DistanceRatio;
            if (Time <= 65)
                opacity *= (Time - 45) / 20f;

            if (Projectile.timeLeft <= 20)
                opacity *= Projectile.timeLeft / 20f;

            Main.spriteBatch.EnterShaderRegion();
            Texture2D telegraphBase = ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseColor(Color.Lerp(Color.Red, Color.OrangeRed, 0.7f * (float)Math.Pow(0.5 + 0.5 * Math.Sin(Main.GlobalTimeWrappedHourly), 3)));
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSecondaryColor(Color.Lerp(Color.Yellow, Color.White, 0.5f));
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSaturation(ratio * 0.5f + 0.5f);

            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].Apply();

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, lightColor, 0, telegraphBase.Size() / 2f, 248f, 0, 0);
            Main.spriteBatch.ExitShaderRegion();
        }
    }
}
