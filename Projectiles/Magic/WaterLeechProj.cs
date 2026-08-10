using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    [PierceResistException]
    public class WaterLeechProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 540;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            if (Projectile.ai[0] == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            }
            //Sticky Behaviour
            Projectile.StickyProjAI(5);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => Projectile.ModifyHitNPCSticky(3);
        public override bool? CanDamage() => Projectile.ai[0] == 1f ? false : base.CanDamage();

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            Projectile.Kill();
            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int height = texture.Height / Main.projFrames[Type];
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Size / 2;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Color color = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, drawStart, texture.Width, height)), color, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int height = texture.Height / Main.projFrames[Type];
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Size / 2;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Color color = Projectile.GetAlpha(Color.White);
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/WaterLeechGlow").Value, Projectile.Center - Main.screenPosition, new Rectangle(0, drawStart, texture.Width, height), Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = SoundID.NPCDeath1.Volume * 0.5f }, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, (int)CalamityDusts.SulphurousSeaAcid, 0, 0, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
            }
        }
    }
}
