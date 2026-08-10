using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Tiles.Abyss;

namespace CalamityMod.Projectiles.Rogue
{
    public class ApoctolithProj : ModProjectile, ILocalizedModType
    {
        public static Color LowBlueColor => Color.Blue;
        public static Color HighBlueColor => Color.DodgerBlue;

        public int ShardDamage => (int)(Projectile.damage * 0.15f);
        public int ExplosionDamage => (int)(Projectile.damage * 0.5f);
        public int ExplosionRadius => 150;

        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Apoctolith";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.2f, 0.5f));

            Projectile.ai[0] = Projectile.Calamity().stealthStrike ? 1 : 0;
            if (Main.rand.NextBool(3)) GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(12), 0).RotatedBy(Vector2.Zero.AngleTo(Projectile.velocity) + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20))), false, 10, 1f, Main.rand.NextBool(3) ? LowBlueColor : Color.Black, true));

            Projectile.ai[1]++;
            //Constant rotation and gravity
            Projectile.rotation += 0.4f * Projectile.direction;
            Projectile.velocity.X *= 0.98f;
            Projectile.velocity.Y = Projectile.velocity.Y + MathHelper.Clamp(Projectile.ai[1] / 40, 0, 0.6f);

            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }
            //Dust trail
            if (Main.rand.NextBool(13))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default, 0.9f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 240);

            if (hit.Crit)
                target.Calamity().miscDefenseLoss = Math.Min(target.defense, 15);

            if (Projectile.Calamity().stealthStrike)
                target.AddBuff(ModContent.BuffType<Eutrophication>(), 120);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 240);
            if (Projectile.Calamity().stealthStrike)
            {
                target.AddBuff(ModContent.BuffType<Eutrophication>(), 120);
            }
        }

        public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, 2, Color.Lerp(HighBlueColor, Color.Transparent, 0.8f), texture: ModContent.Request<Texture2D>(Texture).Value);
            return base.PreDrawExtras(player);
        }

        public override void OnKill(int timeLeft)
        {
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, LowBlueColor, "CalamityMod/Particles/LargeBloom", Vector2.One, 0f, 1f, 0f, 25));
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", Vector2.One, 0f, 0.5f, 0f, 15));

            for (int i = 0; i < 5; i++) GeneralParticleHandler.SpawnParticle(new BloodParticle2(Projectile.Center, new Vector2(Main.rand.NextFloat(6, 12), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)), 12, Main.rand.NextFloat(0.2f, 0.8f), HighBlueColor));

            GlowSparkParticle gs = new(Projectile.Center, Vector2.Zero, false, 20, Projectile.Calamity().stealthStrike ? 0.06f : 0.03f, HighBlueColor, Vector2.One, true);
            gs.Rotation = Main.rand.NextFloat(-10, 10);
            GeneralParticleHandler.SpawnParticle(gs);

            // Sounds
            SoundEngine.PlaySound(AbyssGravel.MineSound, Projectile.position);
            SoundEngine.PlaySound(GiantClam.SlamSound, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode.WithPitchOffset(0.5f), Projectile.position);
            if (Projectile.Calamity().stealthStrike) SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/MineralMortarExplode"), Projectile.position);

            // This only triggers if stealth is full
            if (Projectile.Calamity().stealthStrike && Main.myPlayer == Projectile.owner)
            {
                // Explosion
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ApoctolithExplosion>(), ExplosionDamage, 0, Projectile.owner);

                // Split shards
                for (int i = 0; i < 5; i++)
                {
                    // Calculate the velocity of the projectile
                    float shardspeedX = -Projectile.velocity.X * Main.rand.NextFloat(.5f, .7f) + Main.rand.NextFloat(-3f, 3f);
                    float shardspeedY = -Projectile.velocity.Y * Main.rand.Next(50, 70) * 0.01f + Main.rand.Next(-8, 9) * 0.2f;

                    // Prevents the projectile speed from being too low
                    if (shardspeedX < 2f && shardspeedX > -2f)
                    {
                        shardspeedX += -Projectile.velocity.X;
                    }
                    if (shardspeedY > 2f && shardspeedY < 2f)
                    {
                        shardspeedY += -Projectile.velocity.Y;
                    }
                    shardspeedX *= 2f;
                    shardspeedY *= 2f;

                    // Spawn the projectile
                    int shard = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + shardspeedX, Projectile.Center.Y + shardspeedY, shardspeedX, shardspeedY, ModContent.ProjectileType<ApoctolithShard>(), ShardDamage, Projectile.knockBack / 2f, Projectile.owner);
                    Main.projectile[shard].frame = Main.rand.Next(3);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(32f, 33f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/ApoctolithGlow").Value;
            Vector2 origin = new Vector2(32f, 33f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}
