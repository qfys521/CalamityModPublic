using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class BlunderBoosterLightning : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public static int frameWidth = 12;
        public static int frameHeight = 26;
        public int dir = 0;
        public float intensity = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 290;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration = 15;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.ai[1]++;

            if (dir == 0)
            {
                Projectile.timeLeft -= Main.rand.Next(5, 20 + 1);
                dir = Main.rand.NextBool() ? -1 : 1;
                intensity = Main.rand.NextFloat(0.2f, 1.2f);
            }


            if (Projectile.timeLeft < 190)
                CalamityUtils.HomeInOnSelectedNPC(Projectile, Projectile.Center.ClosestNPCAt(1500), true, 0.65f * intensity, 8, 0.98f, 0.99f, true);
            else
                Projectile.velocity = Projectile.velocity.RotatedBy(0.04f * dir * intensity) * Main.rand.NextFloat(0.985f, 1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<VermillionFlux>(), 90);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<VermillionFlux>(), 90);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D sprite;
            if (Projectile.ai[0] == 0f)
                sprite = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/BlunderBoosterLightning").Value;
            else
                sprite = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/BlunderBoosterLightning2").Value;
            Color drawColour = Color.White;

            Vector2 origin = new Vector2(frameWidth / 2, frameHeight / 2);
            Main.EntitySpriteDraw(sprite, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, frameWidth, frameHeight), drawColour, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = SoundID.Item93.Volume * 0.25f }, Projectile.position);

            for (int i = 0; i < 5; i++)
            {
                int dustType = 60;
                int dust = Dust.NewDust(Projectile.Center, 1, 1, dustType, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 0.5f);
                Main.dust[dust].noGravity = true;
            }
        }
    }
}
