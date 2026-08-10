using System;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SilvaCrystal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public int dust = 3;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 52;
            Projectile.ignoreWater = true;
            Projectile.minion = true;
            Projectile.minionSlots = 0f;
            Projectile.timeLeft = 18000;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();

            bool isMinion = Projectile.type == ModContent.ProjectileType<SilvaCrystal>();
            if (!modPlayer.silvaSummon)
            {
                Projectile.active = false;
                return;
            }
            if (isMinion)
            {
                if (player.dead)
                    modPlayer.sCrystal = false;
                if (modPlayer.sCrystal)
                    Projectile.timeLeft = 2;
            }

            Projectile.Center = player.Center + Vector2.UnitY * (player.gfxOffY - 60f);
            if (player.gravDir == -1f)
            {
                Projectile.position.Y += 120f;
                Projectile.rotation = MathHelper.Pi;
            }
            else
            {
                Projectile.rotation = 0f;
            }

            Projectile.velocity = Vector2.Zero;
            Projectile.alpha -= 5;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
            if (Projectile.direction == 0)
                Projectile.direction = Main.player[Projectile.owner].direction;

            if (Projectile.alpha == 0 && Main.rand.NextBool(15))
            {
                Vector2 dustSpawn = Projectile.Center + Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(26f, 30f);
                Dust silvaDust = Dust.NewDustPerfect(dustSpawn, DustID.RainbowMk2, Vector2.UnitY * Main.rand.NextFloat(-2f, 2f), 100, new Color(Main.DiscoR, 203, 103), 0.5f);
                silvaDust.noGravity = true;
                silvaDust.fadeIn = 1f;
            }

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= 60f)
                Projectile.localAI[0] = 0f;
            if (Projectile.ai[0] < 0f)
                Projectile.ai[0] += 1f;
            if (Projectile.ai[0] == 0f)
            {
                int targetID = -1;
                float attackRange = 1500f;
                if (player.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    if (npc.CanBeChasedBy(Projectile, false))
                    {
                        float targetDist = Projectile.Distance(npc.Center);
                        if (targetDist < attackRange && Collision.CanHitLine(Projectile.Center, 0, 0, npc.Center, 0, 0))
                        {
                            attackRange = targetDist;
                            targetID = npc.whoAmI;
                        }
                    }
                }
                if (targetID < 0)
                {
                    foreach (NPC target in Main.ActiveNPCs)
                    {
                        if (target.CanBeChasedBy(Projectile))
                        {
                            float targetDistance = Projectile.Distance(target.Center);
                            if (targetDistance < attackRange && Collision.CanHitLine(Projectile.Center, 0, 0, target.Center, 0, 0))
                            {
                                attackRange = targetDistance;
                                targetID = target.whoAmI;
                            }
                        }
                    }
                }
                if (targetID != -1)
                {
                    Projectile.ai[0] = 1f;
                    Projectile.ai[1] = targetID;
                    Projectile.netUpdate = true;
                    return;
                }
            }
            if (Projectile.ai[0] > 0f)
            {
                int npcTrack = (int)Projectile.ai[1];
                if (!Main.npc[npcTrack].CanBeChasedBy(Projectile))
                {
                    Projectile.ai[0] = 0f;
                    Projectile.ai[1] = 0f;
                    Projectile.netUpdate = true;
                    return;
                }
                Projectile.ai[0] += 1f;
                if (Projectile.ai[0] >= 5f)
                {
                    int projXDirection = (Projectile.SafeDirectionTo(Main.npc[npcTrack].Center, Vector2.UnitY).X > 0f) ? 1 : -1;
                    Projectile.direction = projXDirection;
                    Projectile.ai[0] = -20f;
                    Projectile.netUpdate = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 attackPos = Main.npc[npcTrack].Center - Projectile.Center;
                        for (int j = 0; j < 3; j++)
                        {
                            Vector2 finalAttackPos = Projectile.Center + attackPos.RotatedByRandom(j > 0 ? MathHelper.Pi * 0.125f : 0f) * (j > 0 ? Main.rand.NextFloat(0.9f, 1.1f) : 1f);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), finalAttackPos, Vector2.Zero, ModContent.ProjectileType<SilvaCrystalExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Main.rgbToHsl(new Color(Main.DiscoR, 203, 103)).X, Projectile.whoAmI);
                        }
                        return;
                    }
                }
            }
        }
        public override bool? CanDamage() => false;

        public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 127 - Projectile.alpha / 2);
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 projPos = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Color colorArea = Lighting.GetColor((int)(Projectile.position.X + Projectile.width * 0.5) / 16, (int)((Projectile.position.Y + Projectile.height * 0.5) / 16.0));
            Color colorAlpha = Projectile.GetAlpha(colorArea) * 0.2f;
            Vector2 origin = tex.Size() / 2f;
            float scaleFactor = MathF.Cos(MathHelper.TwoPi * (Projectile.localAI[0] / 60f)) + 6f;

            for (int k = 0; k < 4; k++)
            {
                Main.EntitySpriteDraw(tex, projPos + Vector2.UnitY.RotatedBy(k * MathHelper.PiOver2) * scaleFactor, null, colorAlpha, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
