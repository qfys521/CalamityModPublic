using System;
using System.IO;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class StarmageddonHeld : ModProjectile
    {
        private bool starHasBeenFired = false;

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Starmageddon>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            ProjectileID.Sets.NeedsUUID[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 166;
            Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(starHasBeenFired);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            starHasBeenFired = reader.ReadBoolean();
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.8f, 0.1f, 1f);

            Player player = Main.player[Projectile.owner];

            int projectileType = ModContent.ProjectileType<StarmageddonBinaryStarCenter>();
            bool noStars = player.ownedProjectileCounts[projectileType] == 0;

            if (noStars && starHasBeenFired)
            {
                Projectile.frame = 0;
                starHasBeenFired = false;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = 7;

            bool timeToFire = noStars && Projectile.frame == 7;
            if (player.CantUseHoldout())
                Projectile.Kill();

            Vector2 playerPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            if (timeToFire && Main.myPlayer == Projectile.owner)
            {
                if (!player.CantUseHoldout())
                {
                    SoundEngine.PlaySound(SoundID.Item92, Projectile.Center);

                    float shootSpeed = 12f;
                    int damage = player.GetWeaponDamage(player.HeldItem);
                    float knockBack = player.HeldItem.knockBack;

                    Projectile.velocity = Main.screenPosition - playerPosition;
                    Projectile.velocity.X += Main.mouseX;
                    Projectile.velocity.Y += Main.mouseY;

                    if (player.gravDir == -1f)
                        Projectile.velocity.Y = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - playerPosition.Y;

                    Projectile.velocity.Normalize();
                    Vector2 dustSpawnPosition = Projectile.Center + (Vector2.UnitY * -12f) + (Projectile.velocity * 40f);
                    Vector2 starSpawnPosition = Projectile.Center + (Vector2.UnitY * -12f) + (Projectile.velocity * 80f);

                    // Create dust sprays above and below the barrel.
                    int dustPerSpray = 50;
                    for (int h = 0; h < 2; h++)
                    {
                        bool top = h == 0;
                        for (int i = 0; i < dustPerSpray; i++)
                        {
                            bool useAltDust = i % 2 == 0;
                            int dustID = top ? (useAltDust ? 31 : 6) : (useAltDust ? 160 : 229);
                            float dustSpeed = i * 0.2f;
                            float angle = top ? -0.12f : 0.12f;
                            Vector2 dustVel = new Vector2(dustSpeed, 0f).RotatedBy((Projectile.velocity * shootSpeed).ToRotation());
                            dustVel = dustVel.RotatedBy(angle);

                            // Pick a size for the particles.
                            float scale = 1.8f - (i * 0.01f);

                            // Actually spawn the particles.
                            int idx = Dust.NewDust(dustSpawnPosition, 1, 1, dustID, dustVel.X, dustVel.Y, 0, default, scale);
                            Main.dust[idx].noGravity = true;
                            Main.dust[idx].position = dustSpawnPosition;
                        }
                    }

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), starSpawnPosition, Projectile.velocity * shootSpeed, projectileType, damage, knockBack, Projectile.owner, Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI));
                    starHasBeenFired = true;
                    Projectile.netUpdate = true;
                }
            }

            if (starHasBeenFired)
            {
                Projectile.velocity = Main.screenPosition - playerPosition;
                Projectile.velocity.X += Main.mouseX;
                Projectile.velocity.Y += Main.mouseY;

                if (player.gravDir == -1f)
                    Projectile.velocity.Y = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - playerPosition.Y;

                Projectile.velocity.Normalize();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector2 displayOffset = new Vector2(27f, -10f * Projectile.direction).RotatedBy(Projectile.rotation);
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true) + displayOffset;
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.Pi;

            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int height = texture.Height / Main.projFrames[Type];
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Size / 2;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/StarmageddonHeldGlow").Value, Projectile.Center - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, drawStart, texture.Width, height)), Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
        }

        public override bool? CanDamage() => false;
    }
}
