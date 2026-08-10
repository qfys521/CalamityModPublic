using Microsoft.Xna.Framework;
using Terraria;
using CalamityMod.Particles;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityMod.Projectiles.Typeless
{
    public class PlaguePulse : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public ref float radius => ref Projectile.ai[1];
        public float maxRadius = 300;
        public bool visible;
        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 96;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            visible = player.Calamity().toxicHeartVisuals;
            if (Projectile.ai[2] == 0)
                Projectile.Center = player.MountedCenter;
            if (Projectile.ai[2] == 1)
            {
                maxRadius = Main.rand.Next(150, 250 + 1);
                Projectile.extraUpdates = 2;
            }

            if (Main.rand.NextBool(5) && visible)
            {
                DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center + Main.rand.NextVector2Circular(radius, radius), Vector2.Zero, (Main.rand.NextBool(3) ? Color.LimeGreen : Color.Green) * 0.6f, new Vector2(1, 1), 0, Main.rand.NextFloat(0.07f, 0.23f), 0f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
            if (Main.rand.NextBool(3) && visible)
            {
                for (int i = 0; i < 2; i++)
                {
                    int DustID = 89;
                    Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(radius, radius);
                    Dust dust2 = Dust.NewDustPerfect(spawnPos, DustID);
                    dust2.scale = Main.rand.NextFloat(0.5f, 0.9f);
                    dust2.velocity = (spawnPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(5, 10);
                    dust2.noGravity = true;
                }
            }

            radius = Utils.Remap(time, 0, 60, 30, maxRadius, true);
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player Owner = Main.player[Projectile.owner];
            bool crit = Main.rand.Next(0, 100 + 1) < Owner.GetTotalCritChance(Owner.GetBestClass());
            if (crit)
                modifiers.SetCrit();

            float minMult = 0.4f;
            int hitsToMinMult = 6;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            target.AddBuff(ModContent.BuffType<Plague>(), 420);

            if (target.life <= 0 && target.realLife == -1 && target.IsAnEnemy(false))
            {
                player.Heal(10);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<PlaguePulse>(), (int)(Projectile.damage * 0.9f), 0f, Projectile.owner, 0, Projectile.ai[1] + 1, 1);
            }
            if (target.CanBeMoved()) // Always push targets away from the player
            {
                Vector2 pushVelocity = Utils.DirectionTo(player.Center, target.Center) * 5;
                target.velocity = pushVelocity;
            }
        }
        public override bool PreDraw(Player renderingPlayer, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Player player = Main.player[Projectile.owner];

            Texture2D rexture = ModContent.Request<Texture2D>("CalamityMod/Particles/SoftRoundExplosion").Value;
            Texture2D fexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Color color = (Color.Green * Utils.Remap(time, 30, 60, 0.35f, 0, true)) with { A = 0 } * (visible ? 1f : 0.25f);
            float scale = (Utils.Remap(time, 0, 60, 1, 6, true) / 21) * (maxRadius == 300 ? 1 : 0.7f);
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(rexture, pos, null, color, Projectile.rotation - Main.GlobalTimeWrappedHourly * 1.5f, rexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(rexture, pos, null, color, Projectile.rotation + Main.GlobalTimeWrappedHourly * 3, rexture.Size() * 0.5f, scale * 0.95f, SpriteEffects.None, 0);
            if (Projectile.ai[2] == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    float pulseScale = (scale * 6 + 0.11f) - i * 0.022f;
                    Main.EntitySpriteDraw(fexture, pos, null, color, Projectile.rotation, fexture.Size() * 0.5f, pulseScale, SpriteEffects.None, 0);
                }
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox);
    }
}
