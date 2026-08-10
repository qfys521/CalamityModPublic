using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Typeless
{
    public class CursorProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
                Projectile.alpha -= 15;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft > 285)
                return;

            float aiTrack = 5f;
            float scaleFactor = 6f;
            int dustType = Utils.SelectRandom(Main.rand, new int[]
            {
                246,
                242,
                229,
                226,
                247
            });
            int crystalDustType = 255;
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                Projectile.localAI[0] = (float)-(float)Main.rand.Next(48);
            }
            else if (Projectile.ai[1] == 1f && Projectile.owner == Main.myPlayer)
            {
                int targetID = -1;
                float hitDistance = 300f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.CanBeChasedBy(Projectile, false))
                    {
                        Vector2 targetCenter = n.Center;
                        float targetDist = Vector2.Distance(targetCenter, Projectile.Center);
                        if (targetDist < hitDistance && targetID == -1 && Collision.CanHitLine(Projectile.Center, 1, 1, targetCenter, 1, 1))
                        {
                            hitDistance = targetDist;
                            targetID = n.whoAmI;
                        }
                    }
                }
                if (hitDistance < 4f)
                {
                    Projectile.Kill();
                    return;
                }
                if (targetID != -1)
                {
                    Projectile.ai[1] = aiTrack + 1f;
                    Projectile.ai[0] = (float)targetID;
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[1] > aiTrack)
            {
                Projectile.ai[1] += 1f;
                int npcTrack = (int)Projectile.ai[0];
                if (!Main.npc[npcTrack].active || !Main.npc[npcTrack].CanBeChasedBy(Projectile, false))
                {
                    Projectile.ai[1] = 1f;
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Projectile.velocity.ToRotation();
                    Vector2 npcDirection = Main.npc[npcTrack].Center - Projectile.Center;
                    if (npcDirection.Length() < 10f)
                    {
                        Projectile.Kill();
                        return;
                    }
                    if (npcDirection != Vector2.Zero)
                    {
                        npcDirection.Normalize();
                        npcDirection *= scaleFactor;
                    }
                    Projectile.velocity = (Projectile.velocity * 29f + npcDirection) / 30f;
                }
            }
            if (Projectile.ai[1] >= 1f && Projectile.ai[1] < aiTrack)
            {
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] == aiTrack)
                {
                    Projectile.ai[1] = 1f;
                }
            }
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] == 48f)
            {
                Projectile.localAI[0] = 0f;
            }
            if (Main.rand.NextBool(12))
            {
                Vector2 rotateFirstDust = -Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 16f).RotatedBy(Projectile.velocity.ToRotation());
                Dust crystalDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, crystalDustType, 0f, 0f, 160, default, 1f);
                crystalDust.velocity *= 0.1f;
                crystalDust.position = Projectile.Center + rotateFirstDust * Projectile.width / 2f + Projectile.velocity * 2f;
                crystalDust.fadeIn = 0.9f;
            }
            if (Main.rand.NextBool(18))
            {
                Vector2 rotateSecondDust = -Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 8f).RotatedBy(Projectile.velocity.ToRotation());
                Dust greenDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.TerraBlade, 0f, 0f, 155, default, 0.8f);
                greenDust.velocity *= 0.3f;
                greenDust.position = Projectile.Center + rotateSecondDust * Projectile.width / 2f;
                if (Main.rand.NextBool())
                {
                    greenDust.fadeIn = 1.4f;
                }
            }
            if (Main.rand.NextBool(8))
            {
                Vector2 rotateThirdDust = -Vector2.UnitX.RotatedByRandom(MathHelper.PiOver4).RotatedBy(Projectile.velocity.ToRotation());
                Dust randomDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 0, default, 1f);
                randomDust.velocity *= 0.3f;
                randomDust.noGravity = true;
                randomDust.position = Projectile.Center + rotateThirdDust * Projectile.width / 2f;
                if (Main.rand.NextBool())
                {
                    randomDust.fadeIn = 1.4f;
                }
            }
            if (Main.rand.NextBool(6))
            {
                Vector2 value13 = -Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 16f).RotatedBy(Projectile.velocity.ToRotation());
                Dust crystalDust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, crystalDustType, 0f, 0f, 100, default, 1f);
                crystalDust2.velocity *= 0.3f;
                crystalDust2.position = Projectile.Center + value13 * Projectile.width / 2f;
                crystalDust2.fadeIn = 1.2f;
                crystalDust2.scale = 1.5f;
                crystalDust2.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.2f / 255f, (255 - Projectile.alpha) * 0.2f / 255f, (255 - Projectile.alpha) * 0.2f / 255f);
            Dust paleDust = Dust.NewDustDirect(Projectile.position, Projectile.width - 28, Projectile.height - 28, DustID.BoneTorch, 0f, 0f, 100, default, 0.8f);
            paleDust.velocity *= 0.1f;
            paleDust.velocity += Projectile.velocity * 0.5f;
            paleDust.noGravity = true;
            if (Main.rand.NextBool(12))
            {
                Dust shinyDust = Dust.NewDustDirect(Projectile.position, Projectile.width - 32, Projectile.height - 32, DustID.Teleporter, 0f, 0f, 100, default, 1f);
                shinyDust.velocity *= 0.25f;
                shinyDust.velocity += Projectile.velocity * 0.5f;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Vaporfied>(), 600);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Vaporfied>(), 600);

        public override void OnKill(int timeLeft)
        {
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4f;
            for (int i = 0; i < 4; i++)
            {
                float offset = MathHelper.ToRadians(MathHelper.Lerp(-22.5f, 22.5f, i / 3f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(offset), ModContent.ProjectileType<CursorProjSplit>(), Projectile.damage / 3, Projectile.knockBack * 0.33f, Projectile.owner);
            }

            SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
        }
    }
}
