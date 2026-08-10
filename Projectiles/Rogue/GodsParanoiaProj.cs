using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class GodsParanoiaProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/GodsParanoia";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.35f, 0f, 0.25f);
            if (Main.rand.NextBool())
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, 1, 1, Main.rand.NextBool(3) ? 56 : 242, 0f, 0f, 0, default, 0.5f);
                flame.alpha = Projectile.alpha;
                flame.velocity = Vector2.Zero;
                flame.noGravity = true;
            }

            Projectile.StickyProjAI(50);

            if (Projectile.ai[0] == 1f)
                Projectile.MaxUpdates = 1; // Prevents the homing function extra updates from carrying over
            else
            {
                Projectile.rotation += 0.2f * Projectile.direction;
                CalamityUtils.HomeInOnNPC(Projectile, false, 400f, Projectile.Calamity().stealthStrike ? 16f : 8f, 20f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 120);
            OnHitDarts();
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 120);
            OnHitDarts();
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => Projectile.ModifyHitNPCSticky(10);
        private void OnHitDarts()
        {
            for (int i = 0; i < 2; i++)
            {
                float startOffsetX = Main.rand.NextFloat(125f, 250f) * Main.rand.NextBool().ToDirectionInt();
                float startOffsetY = Main.rand.NextFloat(125f, 250f) * Main.rand.NextBool().ToDirectionInt();
                Vector2 startPos = Projectile.Center + new Vector2(startOffsetX, startOffsetY);

                Vector2 kunaiSp = Vector2.Normalize(Projectile.Center - startPos) * 25f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), startPos, kunaiSp, ModContent.ProjectileType<GodsParanoiaDart>(), Projectile.damage / 2, Projectile.knockBack / 3f, Projectile.owner);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            return null;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
