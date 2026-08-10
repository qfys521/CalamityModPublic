using System;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class MidnightSunUFO : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public const float DistanceToCheck = 2600f;
        public ref float Timer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 58;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.SkyBlue.ToVector3());
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.velocity.Y = Main.rand.NextFloat(8f, 11f) * Main.rand.NextBool().ToDirectionInt();
                Projectile.velocity.Y = Main.rand.NextFloat(3f, 5f) * Main.rand.NextBool().ToDirectionInt();

                // This AI variable doubles as the random frame on which this UFO chooses to shoot its machine gun.
                Projectile.localAI[0] = Main.rand.Next(1, (int)MidnightSunBeacon.MachineGunRate);
            }

            bool isProperProjectile = Projectile.type == ModContent.ProjectileType<MidnightSunUFO>();
            player.AddBuff(ModContent.BuffType<MidnightSunBuff>(), 3600);
            if (isProperProjectile)
            {
                if (player.dead)
                {
                    modPlayer.midnightUFO = false;
                }
                if (modPlayer.midnightUFO)
                {
                    Projectile.timeLeft = 2;
                }
            }

            NPC potentialTarget = Projectile.Center.MinionHoming(DistanceToCheck, player);

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }

            if (potentialTarget != null)
            {
                Timer++;
                if (Timer % 330f < 180f)
                {
                    Projectile.rotation = Projectile.rotation.AngleTowards(0f, 0.2f);
                    float angle = MathHelper.ToRadians(2f * Timer % 180f);
                    Vector2 destination = potentialTarget.Center - new Vector2((float)Math.Cos(angle) * potentialTarget.width * 0.65f, 250f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(destination) * 24f, 0.03f);

                    if (Timer % MidnightSunBeacon.MachineGunRate == Projectile.localAI[0] && potentialTarget.Top.Y > Projectile.Bottom.Y)
                    {
                        // Vector2 laserVelocity = Projectile.SafeDirectionTo(potentialTarget.Center, Vector2.UnitY).RotatedByRandom(0.05f) * 25f;
                        Vector2 laserVelocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(Projectile.Center, potentialTarget, 25f, MidnightSunShot.MaxUpdate).RotatedByRandom(0.04f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Bottom, laserVelocity, ModContent.ProjectileType<MidnightSunShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    Projectile.MinionAntiClump(0.35f);
                    Projectile.ai[1] = 0f;
                }
                else
                {
                    // Move very, very quickly above the target.
                    Vector2 hoverDestination = potentialTarget.Top - Vector2.UnitY * 40f + (Projectile.minionPos + Timer / 7f).ToRotationVector2() * 40f;
                    Projectile.Center = Vector2.Lerp(Projectile.Center, hoverDestination, 0.1f).MoveTowards(hoverDestination, 20f);
                    Projectile.velocity = Projectile.velocity.MoveTowards(Vector2.Zero, 4f);
                    Projectile.ai[1] = Math.Abs(hoverDestination.Y - potentialTarget.Bottom.Y) + MathHelper.Lerp(30f, 50f, Projectile.identity % 7f / 7f);

                    if (Timer % 330f == 210f)
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
                            Vector2 laserVelocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, laserVelocity, ModContent.ProjectileType<MidnightSunBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, Projectile.whoAmI);
                        }
                    }
                }
            }
            else
            {
                Projectile.velocity = (Projectile.velocity * 15f + Projectile.SafeDirectionTo(player.Center - new Vector2(player.direction * -80f, 160f)) * 19f) / 16f;

                Vector2 distanceVector = player.Center - Projectile.Center;
                if (distanceVector.Length() > DistanceToCheck * 1.5f)
                {
                    Projectile.Center = player.Center;
                    Projectile.netUpdate = true;
                }

                Projectile.MinionAntiClump(0.35f);
                Projectile.rotation = Projectile.velocity.X * 0.03f;
            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Color drawColor = Projectile.GetAlpha(lightColor);

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    Color trailColor = Color.Lerp(drawColor, Color.Transparent, i / (float)Projectile.oldPos.Length);
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                    Main.EntitySpriteDraw(tex, trailPos, frame, trailColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, drawColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
