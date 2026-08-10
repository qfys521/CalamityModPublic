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
    public class WaterElementalMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public int dust = 3;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 190;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minion = true;
            Projectile.minionSlots = 0f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            bool isMinion = Projectile.type == ModContent.ProjectileType<WaterElementalMinion>();
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (!modPlayer.waterElemental.HasValue && modPlayer.allElementals is null && !modPlayer.waterElementalVanity && !modPlayer.allElementalsVanity)
            {
                Projectile.active = false;
                return;
            }
            if (isMinion)
            {
                if (player.dead)
                {
                    modPlayer.waterEleBuff = false;
                }
                if (modPlayer.waterEleBuff)
                {
                    Projectile.timeLeft = 2;
                }
            }
            if (dust > 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    int spawnDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, DustID.Water, 0f, 0f, 0, default, 1f);
                    Main.dust[spawnDust].velocity *= 2f;
                    Main.dust[spawnDust].scale *= 1.15f;
                }
                dust--;
            }
            bool passive = modPlayer.waterElementalVanity || modPlayer.allElementalsVanity;
            if (!passive)
                Lighting.AddLight(Projectile.Center, 0f, 0.25f, 1.5f);

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 6)
            {
                Projectile.frame = 0;
            }
            Projectile.Center = player.Center + Vector2.UnitY * (player.gfxOffY - 180f);
            if (player.gravDir == -1f)
            {
                Projectile.position.Y += 360f;
                Projectile.rotation = MathHelper.Pi;
            }
            else
            {
                Projectile.rotation = 0f;
            }
            Projectile.position.X = (int)Projectile.position.X;
            Projectile.position.Y = (int)Projectile.position.Y;
            if (Projectile.owner == Main.myPlayer && !passive)
            {
                // Prevent firing immediately
                if (Projectile.localAI[0] < 120f)
                    Projectile.localAI[0] += 1f;

                if (Projectile.ai[0] != 0f)
                {
                    Projectile.ai[0] -= 1f;
                    return;
                }
                bool canAttack = false;
                float projX = Projectile.Center.X;
                float projY = Projectile.Center.Y;
                float attackRange = 1000f;
                if (player.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    if (npc.CanBeChasedBy(Projectile, false))
                    {
                        float npcX = npc.position.X + (float)(npc.width / 2);
                        float npcY = npc.position.Y + (float)(npc.height / 2);
                        float npcDist = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - npcX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - npcY);
                        if (npcDist < attackRange && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
                        {
                            projX = npcX;
                            projY = npcY;
                            canAttack = true;
                        }
                    }
                }
                if (!canAttack)
                {
                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        if (n.CanBeChasedBy(Projectile, false))
                        {
                            float targetX = n.position.X + (float)(n.width / 2);
                            float targetY = n.position.Y + (float)(n.height / 2);
                            float targetDist = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - targetX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - targetY);
                            if (targetDist < attackRange && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, n.position, n.width, n.height))
                            {
                                attackRange = targetDist;
                                projX = targetX;
                                projY = targetY;
                                canAttack = true;
                            }
                        }
                    }
                }
                if (canAttack && Projectile.localAI[0] >= 120f)
                {
                    float projXStore = projX;
                    float projYStore = projY;
                    projX -= Projectile.Center.X;
                    projY -= Projectile.Center.Y;
                    if (projX < 0f)
                    {
                        Projectile.spriteDirection = 1;
                    }
                    else
                    {
                        Projectile.spriteDirection = -1;
                    }
                    int projectileType = ModContent.ProjectileType<WaterSpearFriendly>();
                    if (Main.rand.NextBool(9))
                    {
                        projectileType = ModContent.ProjectileType<FrostMistFriendly>();
                    }
                    else if (Main.rand.NextBool(9))
                    {
                        projectileType = ModContent.ProjectileType<WaterElementalSong>();
                    }
                    float projVel = Main.rand.Next(12, 20);
                    Vector2 fireDirection = Projectile.Center;
                    float fireXVel = projXStore - fireDirection.X;
                    float fireYVel = projYStore - fireDirection.Y;
                    float fireVelocity = (float)Math.Sqrt((double)(fireXVel * fireXVel + fireYVel * fireYVel));
                    fireVelocity = projVel / fireVelocity;
                    fireXVel *= fireVelocity;
                    fireYVel *= fireVelocity;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X - 4f, Projectile.Center.Y, fireXVel, fireYVel, projectileType, Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
                    Projectile.ai[0] = 12f;
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 200, 200, 200);
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            SpriteEffects sp = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            var cplayer = Main.player[Projectile.owner].Calamity();

            if ((cplayer.waterElemental.HasValue && !cplayer.waterElemental.Value) || (cplayer.allElementals.HasValue && !cplayer.allElementals.Value))
            {
                Vector3 hsl = Main.rgbToHsl(Color.CornflowerBlue);
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
