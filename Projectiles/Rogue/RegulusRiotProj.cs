using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class RegulusRiotProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/RegulusRiot";

        public ref float Timer => ref Projectile.ai[0];
        private bool canHome = false;
        private int homingDelay = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.timeLeft = 240;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 15;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
                Projectile.alpha -= 20;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            Dust blueDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralBlue>(), 0f, 0f, 100);
            blueDust.noGravity = true;
            blueDust.velocity = Vector2.Zero;
            Dust orangeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100);
            orangeDust.noGravity = true;
            orangeDust.velocity = Vector2.Zero;

            Timer++;
            Projectile.rotation -= MathHelper.Pi / 30f;
            if (homingDelay > 0)
                homingDelay--;
            if (Timer > 30f && homingDelay == 0)
                canHome = true;

            if (Timer == 30f && !canHome)
            {
                Projectile.velocity *= 0.5f;
                canHome = true;
            }

            if (canHome)
            {
                Vector2 targetCenter = Projectile.Center;
                float homingRange = 500f;
                int targetIndex = -1;

                foreach (NPC n in Main.ActiveNPCs)
                {
                    float extraDistance = (n.width / 2) + (n.height / 2);
                    if (!n.CanBeChasedBy(Projectile) || !Projectile.WithinRange(n.Center, homingRange + extraDistance))
                        continue;

                    float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                    if (currentNPCDist < homingRange)
                    {
                        homingRange = currentNPCDist;
                        targetIndex = n.whoAmI;
                    }
                }
                if (targetIndex != -1)
                {
                    targetCenter = Main.npc[targetIndex].Center;
                    Projectile.velocity = (Projectile.velocity * 15f + Vector2.Normalize(targetCenter - Projectile.Center) * 14f) / 16f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
            if (Timer > 30f)
            {
                canHome = false;
                homingDelay = 20;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 2);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                Dust killBlue = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralBlue>(), 0f, 0f, 100, default, 1.5f);
                killBlue.noGravity = true;
                killBlue.velocity = Vector2.Zero;

                Dust killOrange = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.5f);
                killOrange.noGravity = true;
                killOrange.velocity = Vector2.Zero;
            }
            if (Projectile.Calamity().stealthStrike)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 velocity = ((MathHelper.TwoPi * i / 5f) - (MathHelper.Pi / 3f - Projectile.velocity.ToRotation())).ToRotationVector2() * 2f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<RegulusEnergy>(), (int)(Projectile.damage * 0.45f), Projectile.knockBack, Projectile.owner);
                    }
                }
            }
        }
    }
}
