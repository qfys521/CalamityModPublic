using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class FinalDawnHorizontalSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 9;
        }
        public override void SetDefaults()
        {
            Projectile.width = 600;
            Projectile.height = 156;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player is null || player.dead)
                Projectile.Kill();

            Projectile.Center = player.Center;
            player.heldProj = Projectile.whoAmI;
            Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();

            Projectile.ai[0]++;
            if (Projectile.ai[0] >= 4)
            {
                Projectile.ai[1]++;
                Projectile.ai[0] = 0;
                if (Projectile.ai[1] == 3)
                {
                    Projectile.friendly = true;
                    SoundEngine.PlaySound(TheFinalDawn.UseSound, Projectile.position);

                    if (Projectile.owner == Main.myPlayer)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FinalDawnFlame>(), Projectile.damage / 2, 0f, Projectile.owner);
                    }
                }
            }
            if (Projectile.ai[1] >= 9)
            {
                Projectile.Kill();
                return;
            }

            Projectile.frame = (int)Projectile.ai[1];

            if (Projectile.ai[1] < 2) player.bodyFrame.Y = 1 * player.bodyFrame.Height;
            else player.bodyFrame.Y = 3 * player.bodyFrame.Height;

            if (Projectile.ai[1] == 4 || Projectile.ai[1] == 5) player.ChangeDir(-1 * Projectile.spriteDirection);
            else player.ChangeDir(Projectile.spriteDirection);
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D scytheTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D scytheGlowTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/FinalDawnHorizontalSlash_Glow").Value;
            int height = Terraria.GameContent.TextureAssets.Projectile[Type].Value.Height / Main.projFrames[Type];
            int yStart = height * Projectile.frame;
            Main.spriteBatch.Draw(scytheTexture,
                                  Projectile.Center - Main.screenPosition + Projectile.gfxOffY * Vector2.UnitY,
                                  new Rectangle?(new Rectangle(0, yStart, scytheTexture.Width, height)),
                                  Projectile.GetAlpha(lightColor), Projectile.rotation,
                                  new Vector2((float)scytheTexture.Width / 2f, (float)height / 2f), Projectile.scale,
                                  Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                                  0f);
            Main.spriteBatch.Draw(scytheGlowTexture,
                                  Projectile.Center - Main.screenPosition + Projectile.gfxOffY * Vector2.UnitY,
                                  new Rectangle?(new Rectangle(0, yStart, scytheTexture.Width, height)),
                                  Projectile.GetAlpha(Color.White),
                                  Projectile.rotation,
                                  new Vector2((float)scytheTexture.Width / 2f, (float)height / 2f),
                                  Projectile.scale,
                                  Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                                  0f);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300);
        }
    }
}
