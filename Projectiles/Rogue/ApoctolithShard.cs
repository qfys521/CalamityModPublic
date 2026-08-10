using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class ApoctolithShard : ModProjectile, ILocalizedModType
    {
        public int TimeBeforeHoming = 30;
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Rogue/AbyssalMirrorProjectile";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            Main.projFrames[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 13;
            Projectile.height = 13;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.scale = Main.rand.NextFloat(0.7f, 1.2f);
            Projectile.timeLeft = 240;
            TimeBeforeHoming = Main.rand.Next(30, 60);
        }
        public override bool? CanDamage()
        {
            if (Projectile.ai[1] < TimeBeforeHoming) return false;
            return base.CanDamage();
        }
        public override void AI()
        {
            Projectile.ai[1]++;
            //Rotation and gravity
            if (Projectile.ai[1] < TimeBeforeHoming)
            {
                Projectile.velocity *= 0.94f;
            }
            else
            {
                Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 10, 0.05f);
                CalamityUtils.HomeInOnNPC(Projectile, false, 400, Projectile.ai[2], 0.2f);
                Projectile.velocity.Y += 0.25f;
            }

        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -Projectile.velocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -Projectile.velocity.Y;
            return false;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Rectangle fr = tex.Frame(1, 3, 0, Projectile.frame, 0, 0);
            CalamityUtils.DrawAfterimagesCentered(Projectile, 2, Color.Lerp(ApoctolithProj.HighBlueColor, Color.Transparent, 0.8f), texture: tex.Value);

            float a = Math.Clamp(MathHelper.Lerp(255f, 0f, Projectile.ai[1] / (float)TimeBeforeHoming), 0, 1);
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, fr, ApoctolithProj.HighBlueColor.MultiplyRGBA(new Color(a, a, a, 0)), Projectile.rotation, new(fr.Width / 2, fr.Height / 2), 1.35f, SpriteEffects.None);
            
            return base.PreDraw(player, ref lightColor);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(AbyssGravel.MineSound, Projectile.position);
            //Dust effect
            int splash = 0;
            while (splash < 4)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceRod, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.10f, 150, default, 0.9f);
                splash += 1;
            }

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, ApoctolithProj.LowBlueColor, "CalamityMod/Particles/LargeBloom", Vector2.One, 0f, 0.3f, 0f, 25));
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", Vector2.One, 0f, 0.15f, 0f, 15));

            for (int i = 0; i < 5; i++) GeneralParticleHandler.SpawnParticle(new BloodParticle2(Projectile.Center, new Vector2(Main.rand.NextFloat(6, 12), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)), 12, Main.rand.NextFloat(0.02f, 0.1f), ApoctolithProj.HighBlueColor));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 120);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 120);
        }
    }
}
