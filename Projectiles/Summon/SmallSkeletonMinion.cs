using System;
using System.IO;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class SmallSkeletonMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public int Variant;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 34;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = AIType = -1;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Variant);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Variant = reader.ReadInt32();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();

            if (Projectile.localAI[0] == 0f)
            {
                Variant = Main.rand.Next(3);
                Projectile.netUpdate = true;
                Projectile.localAI[0] += 1f;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 6f == 5f)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame < 1)
                Projectile.frame = 1;
            if (Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = 1;

            bool isCorrectProjectile = Projectile.type == ModContent.ProjectileType<SmallSkeletonMinion>();
            player.AddBuff(ModContent.BuffType<SmallSkeletonBuff>(), 3600);
            if (isCorrectProjectile)
            {
                if (player.dead)
                    modPlayer.necrosteocytesDudes = false;
                if (modPlayer.necrosteocytesDudes)
                    Projectile.timeLeft = 2;
            }
            NPC potentialTarget = Projectile.Center.MinionHoming(900f, player);
            if (Projectile.velocity.Y == 0 && (HoleBelow() || (Projectile.Distance(player.Center) > 320f && Projectile.position.X == Projectile.oldPosition.X) || (potentialTarget is not null && Projectile.Top.Y > potentialTarget.Bottom.Y)))
                Projectile.velocity.Y = -10f;
            else if (Projectile.velocity.Y != 0f)
                Projectile.frame = 2;
            if (Projectile.velocity.Y < 16f)
                Projectile.velocity.Y += 0.3f;

            if (potentialTarget is null)
            {
                if (Math.Abs(player.Center.X - Projectile.Center.X - 30f * Projectile.minionPos * player.direction) > 80f)
                {
                    Projectile.velocity.X += Main.rand.NextFloat(0.1f, 0.13f) * (player.Center.X - Projectile.Center.X + 40f * Projectile.minionPos > 0f).ToDirectionInt();
                    Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -13f, 13f);
                }
                else
                {
                    Projectile.velocity.X *= 0.925f;
                    if (MathF.Abs(Projectile.velocity.X) < 0.05f)
                        Projectile.velocity.X = 0f;
                }

                if (Projectile.velocity.X == 0f)
                    Projectile.frame = 0;
                if (Projectile.Distance(player.Center) > 1600f)
                {
                    Projectile.Center = player.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                Projectile.velocity.X += Main.rand.NextFloat(0.1f, 0.13f) * (potentialTarget.Center.X > Projectile.Center.X).ToDirectionInt();
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -13f, 13f);
                if (Math.Sign(Projectile.velocity.X) != (potentialTarget.Center.X > Projectile.Center.X).ToDirectionInt())
                    Projectile.velocity.X *= 0.95f;

                if (Projectile.Distance(player.Center) > 1400f)
                {
                    Projectile.Center = player.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
                Projectile.MinionAntiClump(0.075f);
            }
            if (Projectile.velocity.X > 0.25f)
                Projectile.spriteDirection = 1;
            else if (Projectile.velocity.X < -0.25f)
                Projectile.spriteDirection = -1;
        }
        public bool HoleBelow()
        {
            int tileWidth = 4;
            int tileX = (int)(Projectile.Center.X / 16f) - tileWidth;
            if (Projectile.velocity.X > 0)
            {
                tileX += tileWidth;
            }
            int tileY = (int)((Projectile.position.Y + Projectile.height) / 16f);
            for (int y = tileY; y < tileY + 2; y++)
            {
                for (int x = tileX; x < tileX + tileWidth; x++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool MinionContactDamage() => true;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Rectangle frame = new Rectangle(Variant * Projectile.width, Projectile.frame * Projectile.height, Projectile.width, Projectile.height);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, Projectile.Size / 2f, 1f, spriteEffects, 0);
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = Projectile.Bottom.Y < Main.player[Projectile.owner].Top.Y - 120f;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}
