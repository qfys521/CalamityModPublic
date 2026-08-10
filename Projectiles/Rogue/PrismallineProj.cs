using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class PrismallineProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Prismalline";

        public bool hitEnemy = false;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = ProjAIStyleID.StickProjectile;
            Projectile.timeLeft = 60;
            AIType = ProjectileID.BoneJavelin;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
            Projectile.ai[1] += 1f;
            if (Projectile.ai[1] == 40f)
            {
                int numProj = 3;
                int numSpecProj = 0;
                int numStealthProj = 5;
                float rotation = MathHelper.ToRadians(50);
                if (Projectile.owner == Main.myPlayer)
                {
                    if (!Projectile.Calamity().stealthStrike)
                    {
                        for (int i = 0; i < numProj; i++)
                        {
                            Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f, 0.2f);
                            if (numSpecProj < 1 && !hitEnemy)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<Prismalline3>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
                                ++numSpecProj;
                            }
                            else
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<Prismalline2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
                            }
                        }
                    }
                    else //stealth strike
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                            int shard = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AquashardSplit>(), Projectile.damage / 2, 0f, Projectile.owner, 0f, 1f);
                            if (shard.WithinBounds(Main.maxProjectiles))
                            {
                                Main.projectile[shard].DamageType = RogueDamageClass.Instance;
                                Main.projectile[shard].usesLocalNPCImmunity = true;
                                Main.projectile[shard].localNPCHitCooldown = 10;
                            }
                        }
                        for (int i = 0; i < numStealthProj + 1; i++)
                        {
                            Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f, 0.2f);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<Prismalline3>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 1f, 0f);
                        }
                    }
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Rain, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            if (Projectile.Calamity().stealthStrike)
            {
                for (int s = 0; s < 5; s++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    int shard = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AquashardSplit>(), Projectile.damage / 2, 0f, Projectile.owner);
                    if (shard.WithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[shard].DamageType = RogueDamageClass.Instance;
                        Main.projectile[shard].penetrate = 1;
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitEnemy = true;
            if (Projectile.Calamity().stealthStrike)
                target.AddBuff(ModContent.BuffType<Eutrophication>(), 30);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            hitEnemy = true;
            if (Projectile.Calamity().stealthStrike)
                target.AddBuff(ModContent.BuffType<Eutrophication>(), 30);
        }
    }
}
