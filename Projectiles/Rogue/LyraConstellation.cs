
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class LyraConstellation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public static int lifetime = 150;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            lifetime = 300;
            Projectile.timeLeft = lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() > 0)
                Projectile.rotation = Projectile.velocity.ToRotation();
            var Owner = Main.player[Projectile.owner];
            var DrawCenter = Projectile.Center + Projectile.velocity;
            var StarScale = 0.2f;
            Projectile.scale = MathF.Min(Projectile.timeLeft / 20f, MathF.Min((lifetime - Projectile.timeLeft) / 20f, 1f));
            void SpawnStar(Vector2 offset, float intensity, int flashOffset = 0, int flashMod = 100)
            {
                offset += new Vector2(35.666f, -53.166f); //this centers the constellation
                offset.X *= Projectile.spriteDirection;
                var star = new BloomParticle(DrawCenter + offset.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(offset.Length() * 0.001f, 0, 1)), Vector2.Zero, Color.SkyBlue * ((Owner.miscCounter + flashOffset) % flashMod < 5 ? 0.75f : 1f), StarScale * intensity, StarScale * intensity, 2, false);
                var star2 = new CustomSpark(DrawCenter + offset.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(offset.Length() * 0.001f, 0, 1)), Vector2.UnitX.RotatedBy(MathHelper.Pi * ((Owner.miscCounter + flashOffset) / 300f)) * 0.1f, "CalamityMod/Particles/Sparkle", false, 2, 4 * StarScale * intensity, Color.White, Vector2.One);
                GeneralParticleHandler.SpawnParticle(star);
                GeneralParticleHandler.SpawnParticle(star2);
            }
            if (Projectile.FinalExtraUpdate())
            {
                SpawnStar(new Vector2(0f, 0f), 0.75f, 0); //Center
                SpawnStar(new Vector2(75, -60), 1.25f, 40); //Vega
                SpawnStar(new Vector2(3, -102), 0.75f, 120); //Top
                SpawnStar(new Vector2(-52, 207), 0.75f, 5); //Bottom R
                SpawnStar(new Vector2(-144, 239), 0.75f, 10); //Bottom L
                SpawnStar(new Vector2(-96, 35), 0.75f, 75); //Left
            }
            Projectile.position = Projectile.Center;
            Projectile.Size = new Vector2(225, 375) * Projectile.scale;
            Projectile.Center = Projectile.position;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            var SiriusPos = Projectile.Center;
            var Owner = Main.player[Projectile.owner];
            void ConnectStars(Vector2 point1, Vector2 point2)
            {
                point1 += new Vector2(35.666f, -53.166f); //this centers the constellation
                point2 += new Vector2(35.666f, -53.166f);
                point1.X *= Projectile.spriteDirection;
                point2.X *= Projectile.spriteDirection;
                var color = Color.SkyBlue * 0.75f * ((MathF.Sin(Main.GlobalTimeWrappedHourly) + 1) * 0.25f + 0.5f);
                CalamityUtils.DrawLineBetter(Main.spriteBatch, SiriusPos + point1.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(point1.Length() * 0.001f, 0, 1)), SiriusPos + point2.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(point2.Length() * 0.001f, 0, 1)), color, 3);
            }
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                ConnectStars(new Vector2(0f, 0f), new Vector2(75, -60)); //Center - Vega
                ConnectStars(new Vector2(0f, 0f), new Vector2(3, -102)); //Center - Top
                ConnectStars(new Vector2(0f, 0f), new Vector2(-96, 35)); //Center - Left
                ConnectStars(new Vector2(0f, 0f), new Vector2(-52, 207)); //Center - Bottom R
                ConnectStars(new Vector2(75, -60), new Vector2(3, -102)); //Vega - Top
                ConnectStars(new Vector2(-144, 239), new Vector2(-96, 35)); //Bottom L - Left
                ConnectStars(new Vector2(-144, 239), new Vector2(-52, 207)); //Bottom L - Bottom R
                Main.spriteBatch.End();
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                int dustType = Utils.SelectRandom(Main.rand, new int[]
                {
                    109,
                    111,
                    132
                });

                int dust = Dust.NewDust(Projectile.Center, 1, 1, dustType, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
            }
        }
    }
}
