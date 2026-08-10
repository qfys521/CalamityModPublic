using System;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class AdamantiteThrowingAxeProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/AdamantiteThrowingAxe";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            float rotation = (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.025f;
            Projectile.rotation += rotation * Projectile.direction;

            if (Projectile.Calamity().stealthStrike && Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Electric, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }

            if (Projectile.timeLeft < 570 && !Projectile.Calamity().stealthStrike)
            {
                Projectile.velocity.X *= 0.96f;
                Projectile.velocity.Y += 0.35f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
            return;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.75f);
            if (Projectile.damage < 1)
            {
                Projectile.damage = 1;
            }
            OnHitEffects();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.75f);
            if (Projectile.damage < 1)
            {
                Projectile.damage = 1;
            }
            OnHitEffects();
        }

        private void OnHitEffects()
        {
            if (Projectile.Calamity().stealthStrike && Main.myPlayer == Projectile.owner)
            {
                // Stolen from Twisting Thunder, the old "lightning" effect was just ???? -CIT
                SoundEngine.PlaySound(CommonCalamitySounds.LightningSound, Projectile.position);
                for (int n = 0; n < 4; n++)
                {
                    Vector2 spawnPoint = new Vector2(Projectile.Center.X + Main.rand.NextFloat(-20f, 20f), Projectile.Center.Y - Main.rand.NextFloat(700f, 800f));
                    float randomVelocity = Main.rand.NextFloat() - 0.5f;
                    Vector2 fireTo = new Vector2(spawnPoint.X + 20f * randomVelocity, spawnPoint.Y + 900);
                    Vector2 ai0 = fireTo - spawnPoint;
                    Vector2 velocity = Vector2.Normalize(ai0.RotatedByRandom(MathHelper.Pi / 10f)) * 9f;

                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnPoint, velocity, ProjectileID.CultistBossLightningOrbArc, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0.ToRotation(), Main.rand.NextFloat(100f), 1f);
                    proj.extraUpdates += 14;
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.tileCollide = false;
                    proj.penetrate = 3;
                    proj.usesLocalNPCImmunity = true;
                    proj.localNPCHitCooldown = -1;
                }
            }
        }
    }
}
