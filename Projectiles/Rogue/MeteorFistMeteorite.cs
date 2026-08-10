using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class MeteorFistMeteorite : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "Terraria/Images/Backgrounds/Ambience/Meteor";

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 96;
            Projectile.friendly = true;
            Projectile.MaxUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 360;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.scale = 2f;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5 * Projectile.MaxUpdates)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 0;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            for (int i = 0; i < 2; i++)
            {
                Dust meteorDust = Dust.NewDustDirect(Projectile.position - Projectile.velocity * 0.5f, Projectile.width, Projectile.height / 2, DustID.Torch, 0f, 0f, 100, default, 0.5f);
                meteorDust.scale *= 2f + Main.rand.NextFloat();
                meteorDust.velocity *= 0.2f;
                meteorDust.noGravity = true;
            }

            if (Projectile.Center.Y > Main.npc[(int)Projectile.ai[0]].Center.Y)
                Projectile.tileCollide = true;
        }

        public override void OnKill(int timeLeft)
        {
            Main.LocalPlayer.SetScreenshake(3f);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/HellbornImpact"), Projectile.Center);

            Projectile.ExpandHitboxBy(300);
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.Damage();

            for (int k = 0; k < 50; k++)
            {
                int boomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                Main.dust[boomDust2].noGravity = true;
                Main.dust[boomDust2].velocity *= 5f;
                boomDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                Main.dust[boomDust2].velocity *= 2f;
            }
            CustomPulse outerRing = new(Projectile.Center, Vector2.Zero, Color.Red, "CalamityMod/Particles/BloomRing", Vector2.One, 0f, 0f, 2.8f, 15);
            GeneralParticleHandler.SpawnParticle(outerRing);
            CustomPulse innerRing = new(Projectile.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.8f, 30, true, 0.8f);
            GeneralParticleHandler.SpawnParticle(innerRing);
        }

        // Can hit if:
        // 1: Is the same NPC the fist hit. 2: The NPC the fist hit is no longer active. 3: The fist hit a worm segment, and this NPC is another segment of the same worm. 4: Damage from the explosion.
        public override bool? CanHitNPC(NPC target) => target.whoAmI == Projectile.ai[0] || !Main.npc[(int)Projectile.ai[0]].active || (target.realLife != -1 && target.realLife == Main.npc[(int)Projectile.ai[0]].realLife) || Projectile.penetrate == -1 ? null : false;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire, 240);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.OnFire, 240);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, 4, 0, Projectile.frame);
            SpriteEffects effect = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, effect);
            return false;
        }
    }
}
