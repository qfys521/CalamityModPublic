using CalamityMod.Dusts;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class UrchinStingerProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/UrchinStinger";
        public ref float Timer => ref Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.StickyProjAI(Projectile.Calamity().stealthStrike ? 10 : 3);
            Timer++;

            if (Projectile.ai[0] != 1f)
            {
                if (Timer <= 30f)
                {
                    Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
                    Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(90) * Projectile.direction;
                }
                else
                {
                    Projectile.velocity.Y += 0.3f;
                    Projectile.velocity.X *= 0.98f;
                    if (Projectile.velocity.Y > 16f)
                        Projectile.velocity.Y = 16f;
                    Projectile.rotation += 0.2f * Projectile.direction;
                }
            }
        }

        public override bool? CanDamage() => Projectile.ai[0] == 1f ? false : base.CanDamage();
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Removes Poisoned immunity
            target.buffImmune[BuffID.Poisoned] = false;
            target.AddBuff(BuffID.Poisoned, Projectile.Calamity().stealthStrike ? 600 : 180);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Poisoned, 180);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.ai[0] = 1f;
            Projectile.ai[1] = target.whoAmI;
            Projectile.velocity = target.Center - Projectile.Center;
            Projectile.netUpdate = true;

            // Count how many projectiles are attached
            int maxStick = 7;
            Point[] stuckProjArray = new Point[maxStick];
            int projCount = 0;
            bool stealthProjStuck = false;

            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.whoAmI != Projectile.whoAmI && Main.myPlayer == p.owner && p.type == Projectile.type && p.ai[0] == 1f && p.ai[1] == Projectile.ai[1])
                {
                    stuckProjArray[projCount++] = new Point(p.whoAmI, p.timeLeft);
                    if (p.Calamity().stealthStrike)
                        stealthProjStuck = true;
                    if (projCount >= stuckProjArray.Length)
                        break;
                }
            }
            // Deleting excess projectiles
            if (projCount >= stuckProjArray.Length)
            {
                // If there's a stealth projectile stuck, delete everything and spawn the irradiated blast
                if (stealthProjStuck)
                {
                    for (int m = 0; m < stuckProjArray.Length; m++)
                        Main.projectile[stuckProjArray[m].X].Kill();

                    SoundEngine.PlaySound(SoundID.Item64, Projectile.Center);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<UrchinIrradiation>(), (int)(Projectile.damage * 1.75f), 0f, Projectile.owner);
                }
                // Otherwise, delete the oldest stuck projectile (found using the timeLeft stored in the Y of the Point)
                else
                {
                    int stuckProjAmt = 0;
                    for (int m = 1; m < stuckProjArray.Length; m++)
                    {
                        if (stuckProjArray[m].Y < stuckProjArray[stuckProjAmt].Y)
                        {
                            stuckProjAmt = m;
                        }
                    }
                    Main.projectile[stuckProjArray[stuckProjAmt].X].Kill();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, (int)CalamityDusts.SulphurousSeaAcid, 0f, 0f, 100);
                dust.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            return null;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.Calamity().stealthStrike)
                return Color.Lerp(Color.White, Color.Green, (float)Math.Abs(Math.Sin(Timer * MathHelper.Pi / 30f)));
            return null;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
