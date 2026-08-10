using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class SporeKnifeProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/SporeKnife";
        public static int spinTime = 270;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            Projectile.timeLeft = 300;
            AIType = ProjectileID.ThrowingKnife;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            float minScale = 0.5f;
            float maxScale = 0.8f;
            int dust = Dust.NewDust(Projectile.position - new Vector2(10, 10), 30, 30, DustID.JungleSpore, Projectile.velocity.X, Projectile.velocity.Y, 0, default, Main.rand.NextFloat(minScale, maxScale));
            Main.dust[dust].noGravity = true;
            if (Projectile.timeLeft < spinTime)
            {
                Projectile.rotation += 0.4f * Projectile.direction;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Poisoned, 120);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Poisoned, 120);
        public override void OnKill(int timeLeft)
        {
            #region Visuals and Sound
            if (Projectile.Calamity().stealthStrike)
                SoundEngine.PlaySound(SporeKnife.StealthImpactSound, Projectile.Center);
            else
                SoundEngine.PlaySound(SporeKnife.ImpactSound, Projectile.Center);

            for (int i = 0; i < 18; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextVector2Circular(20f, 20f);
                Color smokeColor = Main.rand.NextBool() ? Color.DarkOliveGreen : Color.ForestGreen;
                Particle smoke = new MediumMistParticle(Projectile.Center, smokeVel, smokeColor, Color.Black, Main.rand.NextFloat(0.9f, 1.6f), 200 - Main.rand.Next(60), 0.08f);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int k = 0; k < 11; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.JungleSpore, new Vector2(6, 6).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(4f, 9f), 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                dust.noGravity = false;
                dust.alpha = Main.rand.Next(100, 120 + 1);
            }
            for (int k = 0; k < 11; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, new Vector2(6, 6).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 0.8f), 0, Color.GreenYellow, Main.rand.NextFloat(0.6f, 0.8f));
                dust.noGravity = false;
                dust.alpha = Main.rand.Next(100, 120 + 1);
            }
            //Explosion effect
            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.ForestGreen * 0.8f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.04f, 0.09f, 20, true);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Green * 0.8f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.04f, 0.07f, 20, true);
            GeneralParticleHandler.SpawnParticle(blastRing2);
            #endregion
            //On hit, expand the hitbox and hit again for 30% of the projectile's damage
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.damage = (int)(Projectile.damage * 0.3f);
                Projectile.penetrate = -1;
                Projectile.ExpandHitboxBy(120);
                Projectile.usesLocalNPCImmunity = true;
                Projectile.localNPCHitCooldown = 10;
                Projectile.Damage();
            }
            if (!Projectile.Calamity().stealthStrike)
                return;
            else
            {
                for (int k = 0; k < 3; k++)
                {
                    float baseDirectionRotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 shootVelocity = (MathHelper.TwoPi * k / 15 + baseDirectionRotation).ToRotationVector2() * 9f;
                    int bud = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity, ModContent.ProjectileType<SporeKnifeBud>(), (int)(Projectile.damage * 0.5f), 0f, Projectile.owner, 0f, 0f);
                }
            }
        }
    }
}
