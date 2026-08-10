using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class SwordsmithsPrideAstralBomber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Summon/AureusBomber";

        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Animation and visuals.
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 3)
                    Projectile.frame = 0;
            }
            Lighting.AddLight(Projectile.Center, Color.Lerp(Color.Orange, Color.Cyan, (float)Math.Sin(Main.GlobalTimeWrappedHourly)).ToVector3());
            if (Projectile.timeLeft % 10 == 0)
            {
                Vector2 sparklePos = new Vector2(Projectile.position.X + Main.rand.Next(Projectile.width), Projectile.position.Y + Main.rand.Next(Projectile.height));
                GenericSparkle sparkle = new(sparklePos, Vector2.Zero, Main.rand.NextBool() ? Color.Orange : Color.Cyan, Color.White, 0.9f, 30, 0f);
                GeneralParticleHandler.SpawnParticle(sparkle);
            }

            // Find the closest target.
            Vector2 destination = Projectile.Center;
            bool foundTarget = false;
            float npcDistCompare = 400f;
            int index = -1;

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (!n.CanBeChasedBy(Projectile, false))
                    continue;

                float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                if (currentNPCDist < npcDistCompare)
                {
                    npcDistCompare = currentNPCDist;
                    index = n.whoAmI;
                }
            }
            if (index != -1)
            {
                destination = Main.npc[index].Center;
                foundTarget = true;
            }

            // Homing and facing direction.
            if (foundTarget)
            {
                Vector2 homeDirection = (destination - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * 20f + homeDirection * 15f) / 21f;
                if ((destination - Projectile.Center).X > 0f)
                    Projectile.spriteDirection = Projectile.direction = -1;
                else
                    Projectile.spriteDirection = Projectile.direction = 1;
            }
            else
            {
                if (Projectile.velocity.X > 0f)
                    Projectile.spriteDirection = Projectile.direction = -1;
                else if (Projectile.velocity.X < 0f)
                    Projectile.spriteDirection = Projectile.direction = 1;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 90);

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 9; i++)
            {
                Vector2 veloc = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(3.5f, 6f);
                LineParticle sparks = new(Projectile.Center, veloc, false, 16, 0.8f, Main.rand.NextBool() ? Color.Orange : Color.Cyan);
                GeneralParticleHandler.SpawnParticle(sparks);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D glow = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/AureusBomberGlow").Value;
            Rectangle frame = glow.Frame(1, 3, 0, Projectile.frame);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, frame, Color.White, 0f, frame.Size() / 2f, 1f, SpriteEffects.None);
        }
    }
}
