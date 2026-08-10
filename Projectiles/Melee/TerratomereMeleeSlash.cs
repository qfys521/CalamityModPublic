﻿using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class TerratomereMeleeSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public Vector2[] ControlPoints;

        public bool Flipped => Projectile.ai[0] == 1f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 144;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Utils.GetLerpValue(0f, 26f, Projectile.timeLeft, true);
            Projectile.velocity *= 0.91f;
            Projectile.scale *= 1.01f;
        }

        public float SlashWidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * 22f;

        public Color SlashColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lime * Utils.GetLerpValue(0.04f, 0.27f, completionRatio, true) * Projectile.Opacity;
        
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            // Draw the slash effect.
            Main.spriteBatch.EnterShaderRegion();
            
            TerratomereHoldoutProj.PrepareSlashShader(Flipped);

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < ControlPoints.Length; i++)
                points.Add(ControlPoints[i] + ControlPoints[i].SafeNormalize(Vector2.Zero) * (Projectile.scale - 1f) * 40f);

            // 17MAY2024: Ozzatron: remove Terratomere rendering its trails multiple times
            PrimitiveRenderer.RenderTrail(points, new(SlashWidthFunction, SlashColorFunction, (_,_) => Projectile.Center, shader: GameShaders.Misc["CalamityMod:ExobladeSlash"]), 65);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);
    }
}
