using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class M1GarandEmptyClip : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        internal static readonly SoundStyle BlingSound = new("CalamityMod/Sounds/Custom/Ultrabling") { PitchVariance = 0.5f };
        internal static readonly SoundStyle BlingHitSound = new("CalamityMod/Sounds/Custom/UltrablingHit") { PitchVariance = 0.5f };
        private static Asset<Texture2D> sheenAsset;
        private static Asset<Texture2D> bloomAsset;

        public static float UsedCoinGrabRangeMultiplier = 5f;
        public static float ClipTossForce = 7.33f;
        public float midAirRot = 0f;

        public static float MaxIntraClipRicoshotDistance = 1000f;

        public static readonly float RicoshotSearchDistance = 2000f;

        public static float SuperpredictionRatio = 0.1f;

        public static float ClipBonus = 2.5f;
        public static float ClipMulticlipBonus = 0.9f; // lmar

        internal static readonly int UpdateCount = 4;
        internal static readonly int ClipLifetime = UpdateCount * CalamityUtils.SecondsToFrames(7);

        // Clips fade out for the last 30 frames of their existence.
        private static readonly int FadeoutTime = UpdateCount * 30;
        private static readonly float ForceFadeDistance = 2000f;
        public static int CritDelayFrames = 22;
        internal static int CritDelayTime => UpdateCount * CritDelayFrames;
        internal ref float ShotFreezeTimer => ref Projectile.ai[1];
        internal static int RicochetPause = UpdateCount * 22;
        public bool HasBeenShot => Projectile.localAI[0] > 0f;

        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 60;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            Main.projFrames[Type] = 1;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = UpdateCount;
            Projectile.timeLeft = ClipLifetime;
            // Draws very small otherwise
            Projectile.scale = 1.5f;
        }

        public override bool? CanDamage() => false;

        public override bool ShouldUpdatePosition() => ShotFreezeTimer <= 0;

        public override void AI()
        {
            if (ShotFreezeTimer > 0f)
            {
                --ShotFreezeTimer;
                if (ShotFreezeTimer <= 0)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Lighting.AddLight(Projectile.Center, Color.LightGray.ToVector3() * 0.2f); 

            if (Main.rand.NextBool(10))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                d.noGravity = true;
            }


            midAirRot += 0.05f;
            Projectile.rotation = Projectile.velocity.X * 0.5f + midAirRot;

            if (Projectile.FinalExtraUpdate())
            {
                float clipGravity = Player.defaultGravity / Projectile.MaxUpdates;
                Projectile.velocity.Y += clipGravity / 1.75f;
            }

            if (Projectile.timeLeft > FadeoutTime && Projectile.Center.Distance(Owner.MountedCenter) > ForceFadeDistance)
                Projectile.timeLeft = FadeoutTime;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            return base.OnTileCollide(oldVelocity);
        }
        // Pretty much all from ricoshot coins from here.
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, 1);
            float fadingOpacity = Math.Clamp(Projectile.timeLeft / (float)FadeoutTime, 0f, 1f);
            Color alphaColor = Projectile.GetAlpha(lightColor) * fadingOpacity;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, alphaColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            int numUpdatesPassed = ClipLifetime - Projectile.timeLeft;
            float x = Math.Clamp((float)numUpdatesPassed / CritDelayTime, 0f, 2f);
            float sheenFunction = Math.Min(MathF.Pow(x + 0.1f, 10f), MathF.Pow(x - 2.1f, 10f));
            float sheenOpacity = Math.Clamp(sheenFunction, 0f, 2f);

            if (sheenOpacity > 0f)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                sheenAsset ??= ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar");
                Texture2D shineTex = sheenAsset.Value;

                bloomAsset ??= ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
                Texture2D bloomTex = bloomAsset.Value;

                Vector2 shineScale = new Vector2(1f, 3f - sheenOpacity * 2f);
                Color shineColor = Color.LightGray;

                Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, null, shineColor * sheenOpacity * 0.3f, MathHelper.PiOver2, bloomTex.Size() / 2f, shineScale * Projectile.scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(shineTex, Projectile.Center - Main.screenPosition, null, shineColor * sheenOpacity, MathHelper.PiOver2, shineTex.Size() / 2f, shineScale * Projectile.scale, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
