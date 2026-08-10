using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Healing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ClaretCannonProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.alpha = 0;
            Projectile.penetrate = 1;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 120 * Projectile.MaxUpdates;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0f / 255f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Projectile.netUpdate = true;
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.penetrate != -1)
                return;
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/NPCKilled/PerfLargeDeath") { Volume = 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/BloodPactCrit") { Volume = 0.5f }, Projectile.Center);
            float particleScale = 8f;
            Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.DarkRed), "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f * particleScale / 5f, 0.87f * particleScale / 5f, (int)(40 * 0.38f), false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);
            Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : new Color(255, 32, 32)), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f * particleScale / 5f, 0.155f * particleScale / 5f, 40);
            GeneralParticleHandler.SpawnParticle(bloodsplosion2);
            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[Projectile.owner];

            int cooldown = 600;
            if (player.HeldItem.ModItem is IClaretCannonInstance cooldownWeapon)
                cooldown = cooldownWeapon.CooldownMax;

            target.AddBuff(ModContent.BuffType<Laceration>(), cooldown / 2);
            target.AddBuff(BuffID.BetsysCurse, cooldown);

            if (Projectile.penetrate == -1)
                return;

            for (var i = 0; i < 20; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(1) * Main.rand.NextFloat(2.75f, 5.25f), ModContent.ProjectileType<BloodstoneHealOrb>(), 20, 0f, player.whoAmI);
            }

            Projectile.position = Projectile.Center;
            Projectile.Size = new Vector2(352);
            Projectile.Center = Projectile.position;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 2;
            Projectile.velocity *= 0;
            Projectile.damage /= 2;
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (ChildSafety.Disabled)
            {
                return base.PreDraw(player, ref lightColor);
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Color drawColor = Color.CornflowerBlue;
            drawColor *= Projectile.Opacity;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
