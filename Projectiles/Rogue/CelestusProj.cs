using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CelestusProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        private bool initialized = false;
        private float speed = 25f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.localNPCHitCooldown = 30;
            Projectile.extraUpdates = 3;
            Projectile.penetrate = -1;

            Projectile.width = Projectile.height = 132;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!initialized)
            {
                speed = Projectile.velocity.Length();
                initialized = true;
            }

            Lighting.AddLight(Projectile.Center, Main.DiscoR * 0.5f / 255f, Main.DiscoG * 0.5f / 255f, Main.DiscoB * 0.5f / 255f);
            Projectile.rotation += 1f;

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 8;
                SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
            }

            switch (Projectile.ai[0])
            {
                case 0f:
                    Projectile.ai[1] += 1f;
                    if (Projectile.ai[1] >= 40f)
                    {
                        Projectile.ai[0] = 1f;
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    break;
                case 1f:
                    float returnSpeed = 25f;
                    float acceleration = 5f;
                    Vector2 playerVec = player.Center - Projectile.Center;
                    if (playerVec.Length() > 4000f)
                    {
                        Projectile.Kill();
                    }
                    playerVec.Normalize();
                    playerVec *= returnSpeed;
                    if (Projectile.velocity.X < playerVec.X)
                    {
                        Projectile.velocity.X += acceleration;
                        if (Projectile.velocity.X < 0f && playerVec.X > 0f)
                        {
                            Projectile.velocity.X += acceleration;
                        }
                    }
                    else if (Projectile.velocity.X > playerVec.X)
                    {
                        Projectile.velocity.X -= acceleration;
                        if (Projectile.velocity.X > 0f && playerVec.X < 0f)
                        {
                            Projectile.velocity.X -= acceleration;
                        }
                    }
                    if (Projectile.velocity.Y < playerVec.Y)
                    {
                        Projectile.velocity.Y += acceleration;
                        if (Projectile.velocity.Y < 0f && playerVec.Y > 0f)
                        {
                            Projectile.velocity.Y += acceleration;
                        }
                    }
                    else if (Projectile.velocity.Y > playerVec.Y)
                    {
                        Projectile.velocity.Y -= acceleration;
                        if (Projectile.velocity.Y > 0f && playerVec.Y < 0f)
                        {
                            Projectile.velocity.Y -= acceleration;
                        }
                    }
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Rectangle projHitbox = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                        Rectangle playerHitbox = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
                        if (projHitbox.Intersects(playerHitbox))
                        {
                            if (Projectile.Calamity().stealthStrike)
                            {
                                Projectile.velocity *= -1f;
                                Projectile.timeLeft = 600;
                                Projectile.penetrate = 1;
                                Projectile.localNPCHitCooldown = -1;
                                Projectile.ai[0] = 2f;
                                Projectile.netUpdate = true;
                            }
                            else
                                Projectile.Kill();
                        }
                    }
                    break;
                case 2f:
                    CalamityUtils.HomeInOnNPC(Projectile, true, 250f, speed, 20f);
                    break;
                default:
                    break;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            OnHitEffects();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            OnHitEffects();
        }

        private void OnHitEffects()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 velocity = ((MathHelper.TwoPi * i / 8f) - (MathHelper.ToRadians(67.5f) - Projectile.velocity.ToRotation())).ToRotationVector2() * 2.5f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CelestusMiniScythe>(), (int)(Projectile.damage * 0.7), Projectile.knockBack, Projectile.owner);
                }
            }
            SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Rectangle frame = new Rectangle(0, 0, 132, 132);
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/CelestusProjGlow").Value,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                Projectile.rotation,
                Projectile.Size / 2,
                1f,
                SpriteEffects.None,
                0);
        }
    }
}
