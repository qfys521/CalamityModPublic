using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class InfernadoRevenge : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public const int TornadoHeight = 8800;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 1020;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 360000;
            CooldownSlot = ImmunityCooldownID.BossNoCheese;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                Projectile.active = false;
                Projectile.netUpdate = true;
                return;
            }
        }

        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            return Color.Lerp(Color.Yellow, Color.Yellow, completionRatio);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Bottom,
                Projectile.Bottom - Vector2.UnitY * TornadoHeight,
                72,
                ref _);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["CalamityMod:Bordernado"].UseSaturation(-0.2f);
            GameShaders.Misc["CalamityMod:Bordernado"].UseOpacity(1f);
            GameShaders.Misc["CalamityMod:Bordernado"].SetShaderTexture(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"));
            Vector2[] drawPoints = new Vector2[5];
            Vector2 upwardAscent = Vector2.UnitY * TornadoHeight;
            Vector2 downwardOffset = Vector2.UnitY * Projectile.height / (drawPoints.Length + 1);

            Vector2 bottom = Projectile.Bottom + downwardOffset;
            Vector2 top = bottom - upwardAscent;
            for (int i = 0; i < drawPoints.Length - 1; i++)
                drawPoints[i] = Vector2.Lerp(top, bottom, i / (float)(drawPoints.Length - 1));

            drawPoints[drawPoints.Length - 1] = bottom;
            PrimitiveRenderer.RenderTrail(drawPoints, new((_,_) => Projectile.width * 0.5f + 16f, ColorFunction, shader: GameShaders.Misc["CalamityMod:Bordernado"]), 85);

            Main.spriteBatch.ExitShaderRegion();
            Main.spriteBatch.EnterShaderRegion();
            Texture2D vortexNoise = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Cracks").Value;
            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(1f);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Gold);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.White);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi;
                Color drawColor = Color.White;
                drawColor.A = 0;
                Vector2 drawPosition = Projectile.Bottom - Main.screenPosition + angle.ToRotationVector2() * 3f;
                Main.EntitySpriteDraw(vortexNoise, drawPosition, null, drawColor, angle + MathHelper.PiOver2, vortexNoise.Size() * 0.5f, Projectile.scale * 1.5f, SpriteEffects.None, 0);
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Dragonfire>(), 180);
        }
    }
}
