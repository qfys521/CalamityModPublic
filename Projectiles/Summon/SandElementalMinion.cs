using System;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SandElementalMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public int dust = 3;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 12;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 98;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 0f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (!modPlayer.sandElemental.HasValue && modPlayer.allElementals is null && !modPlayer.sandElementalVanity && !modPlayer.allElementalsVanity)
            {
                Projectile.active = false;
                return;
            }
            bool isMinion = Projectile.type == ModContent.ProjectileType<SandElementalMinion>();
            if (isMinion)
            {
                if (player.dead)
                {
                    modPlayer.sandEleBuff = false;
                }
                if (modPlayer.sandEleBuff)
                {
                    Projectile.timeLeft = 2;
                }
            }
            dust--;
            if (dust >= 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, DustID.Sand, 0f, 0f, 0, default, 1f);
                    Main.dust[dust].velocity *= 2f;
                    Main.dust[dust].scale *= 1.15f;
                }
            }
            bool passive = modPlayer.sandElementalVanity || modPlayer.allElementalsVanity;
            if (Math.Abs(Projectile.velocity.X) > 0.2f)
            {
                Projectile.spriteDirection = -Projectile.direction;
            }
            float attackDistance = 700f; //700
            if (!passive)
            {
                float lights = (float)Main.rand.Next(90, 111) * 0.01f;
                lights *= Main.essScale;
                Lighting.AddLight(Projectile.Center, 0.7f * lights, 0.6f * lights, 0f * lights);
            }
            Projectile.MinionAntiClump();
            Vector2 projPos = Projectile.position;
            bool canAttack = false;
            if (Projectile.ai[0] != 1f)
            {
                Projectile.tileCollide = false;
            }
            if (Projectile.tileCollide && WorldGen.SolidTile(Framing.GetTileSafely((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16)))
            {
                Projectile.tileCollide = false;
            }
            if (player.HasMinionAttackTargetNPC && !passive)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(Projectile, false))
                {
                    float targetDist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (!canAttack && targetDist < attackDistance)
                    {
                        projPos = npc.Center;
                        canAttack = true;
                    }
                }
            }
            if (!canAttack && !passive)
            {
                foreach (NPC nPC2 in Main.ActiveNPCs)
                {
                    if (nPC2.CanBeChasedBy(Projectile, false))
                    {
                        float targetDist = Vector2.Distance(nPC2.Center, Projectile.Center);
                        if ((!canAttack && targetDist < attackDistance) && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, nPC2.position, nPC2.width, nPC2.height))
                        {
                            attackDistance = targetDist;
                            projPos = nPC2.Center;
                            canAttack = true;
                        }
                    }
                }
            }
            float separationAnxietyDist = 800f;
            if (canAttack && !passive)
            {
                if (Projectile.frame < 6)
                {
                    Projectile.frame = 6;
                }
                Projectile.frameCounter++;
                if (Projectile.frameCounter > 7)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 0;
                }
                if (Projectile.frame > 11)
                {
                    Projectile.frame = 6;
                }
                separationAnxietyDist = 1200f;
            }
            else
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter > 7)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 0;
                }
                if (Projectile.frame > 5)
                {
                    Projectile.frame = 0;
                }
            }
            if (Vector2.Distance(player.Center, Projectile.Center) > separationAnxietyDist)
            {
                Projectile.ai[0] = 1f;
                Projectile.tileCollide = false;
                Projectile.netUpdate = true;
            }
            if (canAttack && Projectile.ai[0] == 0f)
            {
                Vector2 targetDirection = projPos - Projectile.Center;
                float targetDistance = targetDirection.Length();
                targetDirection.Normalize();
                if (targetDistance > 200f)
                {
                    float scaleFactor2 = 8f;
                    targetDirection *= scaleFactor2;
                    Projectile.velocity = (Projectile.velocity * 40f + targetDirection) / 41f;
                }
                else
                {
                    targetDirection *= -4f;
                    Projectile.velocity = (Projectile.velocity * 40f + targetDirection) / 41f;
                }
            }
            else
            {
                bool isReturning = false;
                if (!isReturning)
                {
                    isReturning = Projectile.ai[0] == 1f;
                }
                float returnSpeed = 6f; //6
                if (isReturning)
                {
                    returnSpeed = 15f; //16
                }
                Vector2 center2 = Projectile.Center;
                Vector2 playerDirection = player.Center - center2 + new Vector2(250f, -60f); //-60
                float playerDist = playerDirection.Length();
                if (playerDist > 200f && returnSpeed < 8f) //200 and 8
                {
                    returnSpeed = 8f; //8
                }
                if (playerDist < 200f && isReturning && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                {
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                }
                if (playerDist > 2000f)
                {
                    Projectile.position.X = player.Center.X - (float)(Projectile.width / 2);
                    Projectile.position.Y = player.Center.Y - (float)(Projectile.height / 2);
                    Projectile.netUpdate = true;
                }
                if (playerDist > 70f)
                {
                    playerDirection.Normalize();
                    playerDirection *= returnSpeed;
                    Projectile.velocity = (Projectile.velocity * 40f + playerDirection) / 41f;
                }
                else if (Projectile.velocity.X == 0f && Projectile.velocity.Y == 0f)
                {
                    Projectile.velocity.X = -0.195f;
                    Projectile.velocity.Y = -0.095f;
                }
            }
            if (Projectile.ai[1] > 0f)
            {
                Projectile.ai[1] += (float)Main.rand.Next(1, 4);
            }
            if (Projectile.ai[1] > 220f)
            {
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }

            // Prevent firing immediately
            if (Projectile.localAI[0] < 120f)
                Projectile.localAI[0] += 1f;

            if (Projectile.ai[0] == 0f)
            {
                float scaleFactor3 = 11f;
                int projType = ModContent.ProjectileType<SandBolt>();
                if (canAttack && Projectile.ai[1] == 0f && Projectile.localAI[0] >= 120f)
                {
                    Projectile.ai[1] += 1f;
                    if (Main.myPlayer == Projectile.owner && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, projPos, 0, 0))
                    {
                        Vector2 projVel = projPos - Projectile.Center;
                        projVel.Normalize();
                        projVel *= scaleFactor3;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, projVel, projType, Projectile.damage, 0f, Main.myPlayer);
                        Projectile.netUpdate = true;
                    }
                }
            }
        }
        public override bool? CanDamage() => false;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            SpriteEffects sp = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            var cplayer = Main.player[Projectile.owner].Calamity();

            if ((cplayer.sandElemental.HasValue && !cplayer.sandElemental.Value) || (cplayer.allElementals.HasValue && !cplayer.allElementals.Value))
            {
                Vector3 hsl = Main.rgbToHsl(Color.SandyBrown);
                CalamityUtils.EnterShaderRegion(Main.spriteBatch);
                GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["CalamityMod:BasicTint"].UseSaturation(0.25f);
                GameShaders.Misc["CalamityMod:BasicTint"].UseColor(Main.hslToRgb(1 - hsl.X, hsl.Y, hsl.Z));
                GameShaders.Misc["CalamityMod:BasicTint"].Apply();
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, sp);
                CalamityUtils.ExitShaderRegion(Main.spriteBatch);
            }
            else
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, sp);

            return false;
        }
    }
}
