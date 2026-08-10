using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GalaxySmasherEcho : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/GalaxySmasher";
        public static readonly SoundStyle SlamHamSound = new("CalamityMod/Sounds/Item/GalaxySmasherSmash") { Volume = 0.7f };
        public static readonly SoundStyle Kunk = new("CalamityMod/Sounds/Item/TF2PanHit") { Volume = 1.1f };
        public float rotatehammer = 15f;
        public float speed = 0f;
        public NPC targeted;
        public Color usedColor = Color.Aqua;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 72;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            List<Color> eColors = new List<Color>()
            {
                Color.Aqua,
                Color.Magenta,
            };
            float rate = (Main.GlobalTimeWrappedHourly * 43);
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            rotatehammer += 2f;
            Projectile.rotation += MathHelper.ToRadians(rotatehammer) * Projectile.direction;
            if (true)
            {
                if (Projectile.timeLeft % 2 == 0)
                {
                    Particle spark = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.05f, "CalamityMod/Particles/SmallBloom", false, 13, 0.5f, usedColor, new Vector2(0.6f, 1f), true, false, 0, false, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                Projectile.extraUpdates = 7;

                if (Projectile.ai[1] != -5)
                    targeted = Main.npc[(int)Projectile.ai[1]];
                if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active)
                {
                    targeted = Projectile.Center.ClosestNPCAt(2000);
                    Projectile.ai[1] = -5;
                }
                if (targeted != null)
                {
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.6f, 25, 0.98f);
                }
                else
                    Projectile.Kill();
            }

            Vector2 offset = new Vector2(12, 0).RotatedByRandom(MathHelper.ToRadians(360f));
            Vector2 velOffset = new Vector2(4, 0).RotatedBy(offset.ToRotation());
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.FireworksRGB, (-Projectile.velocity + velOffset) * Main.rand.NextFloat(0.3f, 1f), 0, default, Main.rand.NextFloat(0.35f, 0.75f));
            dust.noGravity = true;
            dust.color = Main.rand.NextBool() ? Color.Magenta : Color.Aqua;

            for (int i = 0; i < 2; i++)
            {
                GalaxyMetaball.SpawnParticle(Projectile.Center, -Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1f), 195f * Main.rand.NextFloat(0.9f, 1f));
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (targeted != null && target != targeted)
                modifiers.SourceDamage *= 0.3f;
            else
                modifiers.SetCrit();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 420);
            if (target == targeted)
                Projectile.Kill();
        }

        public override bool PreKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            // This is what we call fucking IMPACT (3).
            Main.player[Projectile.owner].SetScreenshake(15f);
            if (Main.zenithWorld)
                SoundEngine.PlaySound(Kunk, Projectile.Center);
            else
                SoundEngine.PlaySound(SlamHamSound, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                float rot = MathHelper.PiOver4 * i;
                Particle pulse1 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Aqua * 0.4f, "CalamityMod/Particles/HighResHollowCircleHardEdge", new Vector2(1.4f, 0.6f), rot, 0, 1f, 40);
                GeneralParticleHandler.SpawnParticle(pulse1);
            }

            float numberOfDusts = 40f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(15f, 0).RotatedBy(rot);
                Vector2 velOffset = new Vector2(42.5f, 0).RotatedBy(rot) * (i % 2 == 0 ? 0.8f : i % 3 == 0 ? 0.6f : 1f);

                Particle spark = new CustomSpark(Projectile.Center + offset * 6, velOffset, "CalamityMod/Particles/SmallBloom", false, 35, 1f, Color.Magenta * 0.75f, new Vector2(1.2f, 1f), true, false, 0, false, false, 0.3f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            GalaxyMetaball.SpawnParticle(Projectile.Center, Vector2.Zero, 275f);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0f, ModContent.ProjectileType<GalaxySmasherBlast>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0f);

            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GalaxySmasher").Value;
            Asset<Texture2D> p2 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearSmokey");

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor * 0.5f, 3, texture, true, true);

            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;

            Projectile.DrawProjectileWithBackglow(usedColor with { A = 0 }, Color.White, 9.5f, texture, null, Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            Main.EntitySpriteDraw(p2.Value, generalDrawPos, null, usedColor with { A = 0 } * 0.65f, Projectile.rotation * Main.rand.NextFloat(1.4f, 1.45f), p2.Size() * 0.5f, 1.2f * Main.rand.NextFloat(0.9f, 1.1f), SpriteEffects.None);
            return false;
        }
    }
}
