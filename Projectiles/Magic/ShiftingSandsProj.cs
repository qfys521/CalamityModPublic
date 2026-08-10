using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class ShiftingSandsProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            int maxVelocity = 32;
            Player player = Main.player[Projectile.owner];
            int height = Main.maxTilesY * 16;
            int heightRatio = 0;

            if (Projectile.ai[0] >= 0f)
                heightRatio = (int)(Projectile.ai[1] / (float)height);

            bool notBeingChanneled = Projectile.ai[0] == -1f || Projectile.ai[0] == -2f;
            if (Projectile.penetrate == 1 && Projectile.ai[0] >= 0f && heightRatio == 0)
            {
                Projectile.ai[1] += height;
                heightRatio = 1;
                Projectile.netUpdate = true;
            }
            if (Projectile.penetrate == 1 && Projectile.ai[0] == -1f)
            {
                Projectile.ai[0] = -2f;
                Projectile.netUpdate = true;
            }
            if (heightRatio > 0 || Projectile.ai[0] == -2f)
                Projectile.localAI[0] += 1f;

            if (Main.myPlayer == Projectile.owner)
            {
                if (Projectile.ai[0] >= 0f)
                {
                    if (player.channel && player.HeldItem.shoot == Projectile.type)
                    {
                        Vector2 channelPos = Main.MouseWorld;
                        player.LimitPointToPlayerReachableArea(ref channelPos);
                        if (Projectile.ai[0] != channelPos.X || Projectile.ai[1] != channelPos.Y)
                        {
                            Projectile.netUpdate = true;
                            Projectile.ai[0] = channelPos.X;
                            Projectile.ai[1] = channelPos.Y + (float)(height * heightRatio);
                        }
                    }
                    else
                    {
                        Projectile.netUpdate = true;
                        Projectile.ai[0] = -1f;
                        Projectile.ai[1] = -1f;
                        NPC homeTarget = ClosestNPCAtMagicMissileStyle(Projectile.Center, 800f);
                        if (homeTarget != null)
                        {
                            int targetIndex = homeTarget.whoAmI;
                            if (targetIndex != -1)
                                Projectile.ai[1] = targetIndex;
                        }
                        else if (Projectile.velocity.Length() < 2f)
                            Projectile.velocity = Projectile.DirectionFrom(player.Center) * maxVelocity;
                        else
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxVelocity;
                    }
                }

                if (notBeingChanneled && Projectile.ai[1] == -1f)
                {
                    NPC homeTarget = ClosestNPCAtMagicMissileStyle(Projectile.Center, 800f);
                    if (homeTarget != null)
                    {
                        int targetIndex = homeTarget.whoAmI;
                        if (targetIndex != -1)
                        {
                            Projectile.ai[1] = targetIndex;
                            Projectile.netUpdate = true;
                        }
                    }
                }
            }

            Vector2 targetVector = Vector2.Zero;
            float chaseLerp = 1f;
            if (Projectile.ai[0] > 0f && Projectile.ai[1] > 0f)
                targetVector = new Vector2(Projectile.ai[0], Projectile.ai[1] % height);

            if (notBeingChanneled && Projectile.ai[1] >= 0f)
            {
                Projectile.tileCollide = false;
                NPC target = Main.npc[(int)Projectile.ai[1]];
                if (target.CanBeChasedBy())
                {
                    targetVector = target.Center;
                    float invFineL = Utils.GetLerpValue(0f, 100f, Projectile.Distance(targetVector), true) * Utils.GetLerpValue(600f, 400f, Projectile.Distance(targetVector), true);
                    chaseLerp = MathHelper.Lerp(0f, 0.2f, Utils.GetLerpValue(200f, 20f, 1f - invFineL, true));
                }
                else
                {
                    Projectile.ai[1] = -1f;
                    Projectile.netUpdate = true;
                }
            }

            if (targetVector != Vector2.Zero)
            {
                if (Projectile.Distance(targetVector) >= 64f)
                {
                    Vector2 distanceVector = targetVector - Projectile.Center;
                    Vector2 moveVelocity = distanceVector.SafeNormalize(Vector2.Zero);
                    float velocityMult = Math.Min(maxVelocity, distanceVector.Length());
                    moveVelocity *= velocityMult;
                    if (Projectile.velocity.Length() < 4f)
                        Projectile.velocity += Projectile.velocity.RotatedBy(MathHelper.PiOver4).SafeNormalize(Vector2.Zero) * 4f;

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveVelocity, chaseLerp);
                }
                else
                {
                    Projectile.velocity *= 0.3f;
                    Projectile.velocity += (targetVector - Projectile.Center) * 0.3f;
                }

                if (Projectile.timeLeft < 60)
                    Projectile.timeLeft = 60;
            }

            if (notBeingChanneled && Projectile.ai[1] < 0f)
            {
                if (Projectile.velocity.Length() != maxVelocity)
                    Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.velocity.SafeNormalize(Vector2.UnitY) * maxVelocity, 4f);

                if (Projectile.timeLeft > 300)
                    Projectile.timeLeft = 300;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        private NPC ClosestNPCAtMagicMissileStyle(Vector2 origin, float maxDistanceToCheck = 800f)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].CanBeChasedBy() && Projectile.localNPCImmunity[i] == 0)
                {
                    float extraDistance = (Main.npc[i].width / 2) + (Main.npc[i].height / 2);
                    if (Vector2.Distance(origin, Main.npc[i].Center) < distance)
                    {
                        distance = Vector2.Distance(origin, Main.npc[i].Center);
                        closestTarget = Main.npc[i];
                    }
                }
            }
            return closestTarget;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) => Projectile.ai[0] < 0f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == -1f)
            {
                Projectile.ai[1] = -1f;
                Projectile.netUpdate = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 64;
            Projectile.Center = Projectile.position;
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            int dustAmt = 36;
            for (int i = 0; i < dustAmt; i++)
            {
                Vector2 dustPos = Vector2.Normalize(Projectile.velocity) * new Vector2((float)Projectile.width / 2f, (float)Projectile.height) * 0.75f;
                dustPos = dustPos.RotatedBy((double)((float)(i - (dustAmt / 2 - 1)) * MathHelper.TwoPi / (float)dustAmt), default) + Projectile.Center;
                Vector2 dustVel = dustPos - Projectile.Center;
                int sand = Dust.NewDust(dustPos + dustVel, 0, 0, DustID.UnusedBrown, dustVel.X * 1.5f, dustVel.Y * 1.5f, 100, default, 1.2f);
                Main.dust[sand].noGravity = true;
                Main.dust[sand].noLight = true;
                Main.dust[sand].velocity = dustVel;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
