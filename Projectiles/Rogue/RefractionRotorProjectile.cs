using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class RefractionRotorProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public const int EnergyShotCount = 6;
        public const int StealthEnergyShotCount = 4;
        private static float RotationIncrement = 0.5f;
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/RefractionRotor";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
        }

        public override void SetDefaults()
        {
            Projectile.width = 142;
            Projectile.height = 126;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            Projectile.MaxUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            float spin = Projectile.direction <= 0 ? -0.8f : 0.8f;
            Projectile.rotation += spin * RotationIncrement;
            Projectile.alpha = Utils.Clamp(Projectile.alpha - 18, 0, 255);
            if (Projectile.timeLeft == 80)
                OnhitGrind(spin);
            Projectile.StickyProjAI(80, true);
            if (Projectile.Calamity().stealthStrike)
            {
                CalamityUtils.HomeInOnNPC(Projectile, true, 450f, 24f, 30f);
            }
        }

        private void OnhitGrind(float spinDir)
        {
            // Spin extra fast to visually shred the enemy.
            Projectile.rotation += spinDir * RotationIncrement * 0.8f;
            Projectile.StickyProjAI(12, true);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft > 80)
                Projectile.timeLeft = 80;
            Projectile.ModifyHitNPCSticky(10);
            Projectile.velocity *= 0.8f;
            if (Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SwiftSlice") { Volume = 0.7f }, Projectile.Center);
                Projectile.soundDelay = 10;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                int slash = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.1f, ModContent.ProjectileType<RefractionRotorSlashCreator>(), Projectile.damage, 0f, Projectile.owner, target.whoAmI, Projectile.velocity.ToRotation());
                if (Main.projectile.IndexInRange(slash))
                    Main.projectile[slash].timeLeft = 20;
            }
        }
        public override void OnKill(int timeLeft)
        {
            // Creates an explosion and fan of blades on death
            if (!Main.dedServ)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust rainbowBurst = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2);
                    rainbowBurst.color = Main.hslToRgb(i / 80f, 0.9f, 0.6f);
                    rainbowBurst.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 5.5f);
                    rainbowBurst.scale = Main.rand.NextFloat(1.4f, 2.4f);
                    rainbowBurst.fadeIn = Main.rand.NextFloat(0.8f, 1.6f);
                    rainbowBurst.noGravity = true;
                }
                Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.2f, 25);
                GeneralParticleHandler.SpawnParticle(bolt2);
                Particle bolt3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.SkyBlue, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.1f, 21);
                GeneralParticleHandler.SpawnParticle(bolt3);
                Particle bolt4 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkGreen, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.15f, 19);
                GeneralParticleHandler.SpawnParticle(bolt4);
            }

            int shootType = ModContent.ProjectileType<PrismShurikenBlade>();
            if (Main.myPlayer != Projectile.owner)
                return;
            if (Main.LocalPlayer.ownedProjectileCounts[shootType] > 24)
                return;

            int energyDamage = (int)(Projectile.damage * 0.5f);
            int shotAmt = Projectile.Calamity().stealthStrike ? StealthEnergyShotCount : EnergyShotCount;
            float baseDirectionRotation = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < shotAmt; i++)
            {
                Vector2 shootVelocity = (MathHelper.TwoPi * i / shotAmt + baseDirectionRotation).ToRotationVector2() * 9f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + shootVelocity, shootVelocity, shootType, energyDamage, Projectile.knockBack, Projectile.owner);
            }
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/RefractionRotorGlowmask").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
            Vector2 origin = glowmask.Size() * 0.5f;
            Main.EntitySpriteDraw(glowmask, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Asset<Texture2D> p = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearSmokey");
            Asset<Texture2D> p2 = ModContent.Request<Texture2D>("CalamityMod/Particles/SemiCircularSmearSwipe");
            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(p2.Value, generalDrawPos, null, (Main.rand.NextBool() ? Color.YellowGreen : Color.Goldenrod) with { A = 0 } * 0.55f, Projectile.rotation * Main.rand.NextFloat(1.6f, 1.7f), p2.Size() * 0.5f, (Main.rand.NextBool() ? 1.6f : 1.4f) * Main.rand.NextFloat(0.8f, 1.15f), SpriteEffects.None);
            Main.EntitySpriteDraw(p.Value, generalDrawPos, null, (Main.rand.NextBool() ? Color.OrangeRed : Color.CornflowerBlue) with { A = 0 } * 0.75f, Projectile.rotation * Main.rand.NextFloat(1.2f, 1.3f), p.Size() * 0.5f, Main.rand.NextBool() ? 1.4f : 1.2f, SpriteEffects.None);
        }
    }
}
