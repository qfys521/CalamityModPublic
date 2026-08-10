using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ImmolationSpray : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 3;
            Projectile.penetrate = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (time > 5 && targetDist < 1400f)
            {
                Projectile.velocity.Y += 0.18f;
                Projectile.velocity.X *= 0.9835f;

                if (Main.rand.NextBool(3))
                {
                    Vector2 placement = Projectile.Center + Main.rand.NextVector2Circular(8, 8);
                    float speed = Main.rand.NextFloat(0.2f, 0.7f);
                    Particle spark = new GlowOrbParticle(placement, -Projectile.velocity * speed, false, 7, Main.rand.NextFloat(0.4f, 0.7f), Effects.ArsenalEffects.ArsenalPlasmaColor);
                    GeneralParticleHandler.SpawnParticle(spark);

                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalPlasmaDust, -Projectile.velocity);
                    dust.scale = Main.rand.NextFloat(0.4f, 1.1f);
                    dust.velocity = (new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.7f));
                    dust.noGravity = true;
                    dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                }
            }
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SoundEngine.PlaySound(HolofibreImmolator.PlasmaSound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);

            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.8f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (targetDist < 1400)
            {
                Vector2 vel = oldVelocity.SafeNormalize(Vector2.UnitX);
                int dustStyle = Effects.ArsenalEffects.ArsenalPlasmaDust;
                for (int i = 0; i < 6; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + vel * 5, dustStyle, (-vel * Main.rand.NextFloat(5, 8)).RotatedByRandom(0.7f));
                    dust.scale = Main.rand.NextFloat(0.7f, 1.3f);
                    dust.noGravity = false;
                    dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                    dust.fadeIn = 1.2f;
                }
            }
            return true;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (time < 1)
                return false;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark");

            float squash = Utils.GetLerpValue(-3, 10, Projectile.velocity.Length(), true);
            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Effects.ArsenalEffects.ArsenalPlasmaColor with { A = 0 } * 0.6f, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.4f, squash) * 0.045f * (i == 0 ? 0.6f : 1), SpriteEffects.None);
            return false;
        }
    }
}
