
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class ScorpiusConstellation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public static int lifetime = 150;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {

            lifetime = 90;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = lifetime;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.scale = 0;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() > 0)
                Projectile.rotation = Projectile.velocity.ToRotation();
            var Owner = Main.player[Projectile.owner];
            var DrawCenter = Projectile.Center + Projectile.velocity;
            var StarScale = 0.2f;
            Projectile.scale = 0.75f*MathF.Min(MathF.Pow((lifetime- Projectile.timeLeft)/ 20f,2f), 1f);
            Projectile.Opacity = MathF.Min(Projectile.timeLeft / 20f, 1f);
            void SpawnStar(Vector2 offset, float intensity, int flashOffset = 0, int flashMod = 60)
            {
                offset += new Vector2(5, 49.25f); //this centers the constellation
                offset.X *= Projectile.spriteDirection;
                var star = new BloomParticle(DrawCenter + offset.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(offset.Length() * 0.001f, 0, 1)), Vector2.Zero, Color.SkyBlue * Projectile.Opacity * ((Owner.miscCounter + flashOffset) % flashMod < 5 ? 0.75f : 1f), StarScale * intensity, StarScale * intensity, 2, false);
                var star2 = new CustomSpark(DrawCenter + offset.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(offset.Length() * 0.001f, 0, 1)), Vector2.UnitX.RotatedBy(MathHelper.Pi * ((Owner.miscCounter + flashOffset) / 300f)) * 0.1f, "CalamityMod/Particles/Sparkle", false, 2, 4 * StarScale * intensity, Color.White * Projectile.Opacity, Vector2.One);
                GeneralParticleHandler.SpawnParticle(star);
                GeneralParticleHandler.SpawnParticle(star2);
            }
            if (Projectile.FinalExtraUpdate())
            {
                SpawnStar(new Vector2(0f, 0f), 0.75f, 25); //Center
                SpawnStar(new Vector2(99,-166), 1.25f, 20); //Antares
                SpawnStar(new Vector2(224,-300), 0.75f, 15); //topHead
                SpawnStar(new Vector2(243,-237), 0.75f, 10); //midHead
                SpawnStar(new Vector2(243,-163), 0.75f, 5); //lowHead
                SpawnStar(new Vector2(246,-97), 0.75f, 0); //bottomHead
                SpawnStar(new Vector2(-255,77), 0.75f, 55); //tail
                SpawnStar(new Vector2(-176,71), 0.75f, 50); //zig
                SpawnStar(new Vector2(-236,142), 0.75f, 45); //zag
                SpawnStar(new Vector2(-188,197), 0.75f, 40); //tail 4
                SpawnStar(new Vector2(-85,193), 0.75f, 35); //taol 5
                SpawnStar(new Vector2(-19,172), 0.75f, 30); //tail 6
            }
            Projectile.position = Projectile.Center;
            Projectile.Size = new Vector2(510,510) * Projectile.scale;
            Projectile.Center = Projectile.position;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Voidfrost>(), 180);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Voidfrost>(), 180);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            var SiriusPos = Projectile.Center;
            var Owner = Main.player[Projectile.owner];
            void ConnectStars(Vector2 point1, Vector2 point2)
            {
                point1 += new Vector2(5, 49.25f); //this centers the constellation
                point2 += new Vector2(5, 49.25f);
                point1.X *= Projectile.spriteDirection;
                point2.X *= Projectile.spriteDirection;
                var color = Color.SkyBlue * 0.75f * ((MathF.Sin(Main.GlobalTimeWrappedHourly) + 1) * 0.25f + 0.5f);
                CalamityUtils.DrawLineBetter(Main.spriteBatch, SiriusPos + point1.RotatedBy(Projectile.rotation) * Projectile.scale, SiriusPos + point2.RotatedBy(Projectile.rotation) * Projectile.scale, color * Projectile.Opacity, 3);
            }
            //Order is back of tail to front. Last four are the head
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                ConnectStars(new Vector2(-255, 77), new Vector2(-176, 71));
                ConnectStars(new Vector2(-176, 71), new Vector2(-236, 142));
                ConnectStars(new Vector2(-236, 142), new Vector2(-188, 197));
                ConnectStars(new Vector2(-188, 197), new Vector2(-85, 193));
                ConnectStars(new Vector2(-85, 193), new Vector2(-19, 172));
                ConnectStars(new Vector2(-19, 172), new Vector2(0f, 0f));
                ConnectStars(new Vector2(0f, 0f), new Vector2(99, -166));
                ConnectStars(new Vector2(99, -166), new Vector2(224, -300));
                ConnectStars(new Vector2(99, -166), new Vector2(243, -237));
                ConnectStars(new Vector2(99, -166), new Vector2(243, -163));
                ConnectStars(new Vector2(99, -166), new Vector2(246, -97));
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
