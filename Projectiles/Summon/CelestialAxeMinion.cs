using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class CelestialAxeMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 110;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.75f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            bool isMinion = Projectile.type == ModContent.ProjectileType<CelestialAxeMinion>();
            player.AddBuff(ModContent.BuffType<LegionofCelestiaBuff>(), 3600);
            if (isMinion)
            {
                if (player.dead)
                {
                    modPlayer.eAxe = false;
                }
                if (modPlayer.eAxe)
                {
                    Projectile.timeLeft = 2;
                }
            }
            Projectile.rotation += 0.075f;

            Projectile.ChargingMinionAI(1600f, 1800f, 2500f, 400f, 1, 30f, 24f, 12f, new Vector2(0f, -60f), 30f, 16f, true, true);

            // Destroy trees in GFB, because it's an axe, and axes should do that
            if (Main.zenithWorld)
            {
                Point pointToCheck = (Projectile.position + Projectile.Size * 0.5f).ToTileCoordinates();
                var prismTooth = new PrismTooth(); // Grrrr CS0120
                prismTooth.AbsolutelyFuckingAnnihilateTrees(pointToCheck.X, pointToCheck.Y);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool MinionContactDamage() => true;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 60);
        }
    }
}
